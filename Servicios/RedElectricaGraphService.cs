using System.Data;
using System.Diagnostics;
using System.Globalization;
using System.Security.Cryptography;
using System.Text;
using System.Text.Json;
using System.Text.RegularExpressions;
using Microsoft.Data.SqlClient;
using Microsoft.Extensions.Caching.Memory;
using Microsoft.Extensions.Options;
using NSIE.Models;

namespace NSIE.Servicios;

public sealed class RedElectricaGraphOptions
{
    public const string SectionName = "PamTerritorial:GrafoRedElectrica";

    public string SubstationsUrl { get; set; } =
        "https://cdn.sassoapps.com/dgmesnie/geojson/dgmesnie_subestaciones2.geojson";
    public string LinesUrl { get; set; } =
        "https://cdn.sassoapps.com/dgmesnie/geojson/dgmesnie_lt.geojson";
    public int CacheMinutes { get; set; } = 360;
    public double SearchRadiusKm { get; set; } = 5;
    public double EndpointToleranceKm { get; set; } = 3;
    public double VirtualMergeToleranceKm { get; set; } = 0.1;
    public int HighConfidenceThreshold { get; set; } = 85;
    public int ReviewThreshold { get; set; } = 70;
    public int AmbiguityMargin { get; set; } = 10;
    public int MaximumCandidatesPerEndpoint { get; set; } = 5;
    public bool PreferPersistedVersion { get; set; } = true;
}

public interface IRedElectricaGraphService
{
    Task<RedElectricaGraphSnapshot> GetAsync(
        bool forceRebuild = false,
        CancellationToken cancellationToken = default);

    Task<RedElectricaGraphSnapshot> RebuildAsync(
        bool persist,
        CancellationToken cancellationToken = default);

    Task<RedElectricaGraphSimulation> SimulateAsync(
        CancellationToken cancellationToken = default);

    Task<RedElectricaGraphNeighborResult?> GetNeighborsAsync(
        string nodeId,
        int depth,
        CancellationToken cancellationToken = default);

    Task<RedElectricaGraphRouteResult?> FindRouteAsync(
        string originNodeId,
        string destinationNodeId,
        CancellationToken cancellationToken = default);

    Task<RedElectricaGeoJson> GetNodesGeoJsonAsync(
        bool includeVirtual,
        string? networkLevels = null,
        CancellationToken cancellationToken = default);

    Task<RedElectricaGeoJson> GetEdgesGeoJsonAsync(
        string? connectionState,
        string? networkLevels = null,
        CancellationToken cancellationToken = default);

    Task<RedElectricaPamSubgraphResult?> GetPamSubgraphAsync(
        long projectId,
        int depth,
        CancellationToken cancellationToken = default);
}

public sealed partial class RedElectricaGraphService : IRedElectricaGraphService
{
    public const string RulesVersion = "RED-GRAFO-v1.3";
    private const string CacheKey = "red-electrica-graph-v4";
    private const string SimulationCacheKey =
        "red-electrica-graph-simulation-v3";
    private static readonly SemaphoreSlim BuildLock = new(1, 1);

    private readonly HttpClient _httpClient;
    private readonly IMemoryCache _cache;
    private readonly RedElectricaGraphOptions _options;
    private readonly IPamTerritorialService _pamTerritorialService;
    private readonly IPamRedAssociationService _pamAssociationService;
    private readonly ILogger<RedElectricaGraphService> _logger;
    private readonly string _connectionString;

    public RedElectricaGraphService(
        HttpClient httpClient,
        IMemoryCache cache,
        IOptions<RedElectricaGraphOptions> options,
        IConfiguration configuration,
        IPamTerritorialService pamTerritorialService,
        IPamRedAssociationService pamAssociationService,
        ILogger<RedElectricaGraphService> logger)
    {
        _httpClient = httpClient;
        _cache = cache;
        _options = options.Value;
        _connectionString =
            configuration.GetConnectionString("DefaultConnection") ??
            string.Empty;
        _pamTerritorialService = pamTerritorialService;
        _pamAssociationService = pamAssociationService;
        _logger = logger;
    }

    public async Task<RedElectricaGraphSnapshot> GetAsync(
        bool forceRebuild = false,
        CancellationToken cancellationToken = default)
    {
        if (!forceRebuild &&
            _cache.TryGetValue<RedElectricaGraphSnapshot>(CacheKey, out var cached) &&
            cached is not null)
        {
            return cached;
        }

        await BuildLock.WaitAsync(cancellationToken);
        try
        {
            if (!forceRebuild &&
                _cache.TryGetValue<RedElectricaGraphSnapshot>(CacheKey, out cached) &&
                cached is not null)
            {
                return cached;
            }

            RedElectricaGraphSnapshot? snapshot = null;
            if (!forceRebuild &&
                _options.PreferPersistedVersion &&
                !string.IsNullOrWhiteSpace(_connectionString))
            {
                try
                {
                    snapshot = await LoadPersistedAsync(cancellationToken);
                }
                catch (SqlException ex) when (
                    ex.Number is 208 or 4060 or 18456)
                {
                    _logger.LogDebug(
                        ex,
                        "No existe una versión persistida del grafo o la base no está disponible; se reconstruirá desde GeoJSON.");
                }
            }

            snapshot ??= await BuildAsync(cancellationToken);
            _cache.Set(
                CacheKey,
                snapshot,
                TimeSpan.FromMinutes(Math.Max(5, _options.CacheMinutes)));
            return snapshot;
        }
        finally
        {
            BuildLock.Release();
        }
    }

    public async Task<RedElectricaGraphSnapshot> RebuildAsync(
        bool persist,
        CancellationToken cancellationToken = default)
    {
        await BuildLock.WaitAsync(cancellationToken);
        try
        {
            var snapshot = await BuildAsync(cancellationToken);
            if (persist)
            {
                await PersistAsync(snapshot, cancellationToken);
            }
            _cache.Remove(SimulationCacheKey);
            _cache.Set(
                CacheKey,
                snapshot,
                TimeSpan.FromMinutes(Math.Max(5, _options.CacheMinutes)));
            return snapshot;
        }
        finally
        {
            BuildLock.Release();
        }
    }

    public async Task<RedElectricaGraphSimulation> SimulateAsync(
        CancellationToken cancellationToken = default)
    {
        if (_cache.TryGetValue<RedElectricaGraphSimulation>(
                SimulationCacheKey,
                out var cached) &&
            cached is not null)
        {
            return cached;
        }

        await BuildLock.WaitAsync(cancellationToken);
        try
        {
            if (_cache.TryGetValue<RedElectricaGraphSimulation>(
                    SimulationCacheKey,
                    out cached) &&
                cached is not null)
            {
                return cached;
            }

            var baseline = await LoadPersistedAsync(cancellationToken) ??
                throw new InvalidOperationException(
                    "No existe una versión activa persistida para comparar.");
            var candidate = await BuildAsync(cancellationToken);
            var simulation = CompareSnapshots(baseline, candidate);
            _cache.Set(
                SimulationCacheKey,
                simulation,
                TimeSpan.FromMinutes(10));
            return simulation;
        }
        finally
        {
            BuildLock.Release();
        }
    }

    public async Task<RedElectricaGraphNeighborResult?> GetNeighborsAsync(
        string nodeId,
        int depth,
        CancellationToken cancellationToken = default)
    {
        var snapshot = await GetAsync(false, cancellationToken);
        if (!snapshot.Nodes.TryGetValue(nodeId, out var origin))
        {
            return null;
        }

        depth = Math.Clamp(depth, 1, 5);
        var nodeIds = new HashSet<string>(StringComparer.Ordinal) { nodeId };
        var edgeIds = new HashSet<string>(StringComparer.Ordinal);
        var frontier = new HashSet<string>(StringComparer.Ordinal) { nodeId };

        for (var level = 0; level < depth && frontier.Count > 0; level++)
        {
            var next = new HashSet<string>(StringComparer.Ordinal);
            foreach (var current in frontier)
            {
                if (!snapshot.Adjacency.TryGetValue(current, out var adjacent))
                {
                    continue;
                }

                foreach (var connection in adjacent)
                {
                    edgeIds.Add(connection.EdgeId);
                    if (nodeIds.Add(connection.NeighborNodeId))
                    {
                        next.Add(connection.NeighborNodeId);
                    }
                }
            }
            frontier = next;
        }

        return new RedElectricaGraphNeighborResult
        {
            Origin = origin,
            Depth = depth,
            Nodes = nodeIds
                .Select(id => snapshot.Nodes[id])
                .OrderBy(node => node.Name, StringComparer.OrdinalIgnoreCase)
                .ToList(),
            Edges = edgeIds
                .Select(id => snapshot.Edges[id])
                .OrderBy(edge => edge.Name, StringComparer.OrdinalIgnoreCase)
                .ToList()
        };
    }

    public async Task<RedElectricaGraphRouteResult?> FindRouteAsync(
        string originNodeId,
        string destinationNodeId,
        CancellationToken cancellationToken = default)
    {
        var snapshot = await GetAsync(false, cancellationToken);
        if (!snapshot.Nodes.ContainsKey(originNodeId) ||
            !snapshot.Nodes.ContainsKey(destinationNodeId))
        {
            return null;
        }

        var distances = new Dictionary<string, double>(StringComparer.Ordinal)
        {
            [originNodeId] = 0
        };
        var previous = new Dictionary<string, (string NodeId, string EdgeId)>(
            StringComparer.Ordinal);
        var queue = new PriorityQueue<string, double>();
        queue.Enqueue(originNodeId, 0);

        while (queue.TryDequeue(out var current, out var currentDistance))
        {
            cancellationToken.ThrowIfCancellationRequested();
            if (currentDistance >
                distances.GetValueOrDefault(current, double.PositiveInfinity))
            {
                continue;
            }
            if (string.Equals(
                current,
                destinationNodeId,
                StringComparison.Ordinal))
            {
                break;
            }
            if (!snapshot.Adjacency.TryGetValue(current, out var adjacent))
            {
                continue;
            }

            foreach (var connection in adjacent)
            {
                var nextDistance = currentDistance + Math.Max(
                    0.001,
                    connection.WeightKm);
                if (nextDistance >= distances.GetValueOrDefault(
                    connection.NeighborNodeId,
                    double.PositiveInfinity))
                {
                    continue;
                }

                distances[connection.NeighborNodeId] = nextDistance;
                previous[connection.NeighborNodeId] =
                    (current, connection.EdgeId);
                queue.Enqueue(connection.NeighborNodeId, nextDistance);
            }
        }

        if (!distances.TryGetValue(destinationNodeId, out var total))
        {
            return new RedElectricaGraphRouteResult
            {
                OriginNodeId = originNodeId,
                DestinationNodeId = destinationNodeId,
                Found = false,
                Nodes = Array.Empty<RedElectricaGraphNode>(),
                Edges = Array.Empty<RedElectricaGraphEdge>()
            };
        }

        var routeNodeIds = new List<string> { destinationNodeId };
        var routeEdgeIds = new List<string>();
        var cursor = destinationNodeId;
        while (!string.Equals(cursor, originNodeId, StringComparison.Ordinal))
        {
            if (!previous.TryGetValue(cursor, out var step))
            {
                break;
            }
            routeEdgeIds.Add(step.EdgeId);
            cursor = step.NodeId;
            routeNodeIds.Add(cursor);
        }
        routeNodeIds.Reverse();
        routeEdgeIds.Reverse();

        return new RedElectricaGraphRouteResult
        {
            OriginNodeId = originNodeId,
            DestinationNodeId = destinationNodeId,
            Found = true,
            TotalDistanceKm = total,
            Nodes = routeNodeIds.Select(id => snapshot.Nodes[id]).ToList(),
            Edges = routeEdgeIds.Select(id => snapshot.Edges[id]).ToList()
        };
    }

    public async Task<RedElectricaGeoJson> GetNodesGeoJsonAsync(
        bool includeVirtual,
        string? networkLevels = null,
        CancellationToken cancellationToken = default)
    {
        var snapshot = await GetAsync(false, cancellationToken);
        var requestedLevels = ParseNetworkLevels(networkLevels);
        var nodes = snapshot.Nodes.Values
            .Where(node => includeVirtual || !node.IsVirtual)
            .Where(node =>
                requestedLevels.Count == 0 ||
                requestedLevels.Contains(node.NetworkLevel))
            .ToList();
        return CreateGeoJson(
            snapshot,
            "nodos",
            nodes.Select(NodeFeature).ToList(),
            nodes.Count,
            0);
    }

    public async Task<RedElectricaGeoJson> GetEdgesGeoJsonAsync(
        string? connectionState,
        string? networkLevels = null,
        CancellationToken cancellationToken = default)
    {
        var snapshot = await GetAsync(false, cancellationToken);
        var normalizedState = NormalizeConnectionState(connectionState);
        var requestedLevels = ParseNetworkLevels(networkLevels);
        var edges = snapshot.Edges.Values
            .Where(edge =>
                string.IsNullOrWhiteSpace(normalizedState) ||
                string.Equals(
                    edge.ConnectionState,
                    normalizedState,
                    StringComparison.Ordinal))
            .Where(edge =>
                requestedLevels.Count == 0 ||
                requestedLevels.Contains(edge.NetworkLevel))
            .ToList();
        return CreateGeoJson(
            snapshot,
            "aristas",
            edges.Select(edge => EdgeFeature(edge, snapshot.Nodes)).ToList(),
            0,
            edges.Count);
    }

    public async Task<RedElectricaPamSubgraphResult?> GetPamSubgraphAsync(
        long projectId,
        int depth,
        CancellationToken cancellationToken = default)
    {
        var project = await _pamTerritorialService.ObtenerAsync(
            projectId,
            cancellationToken);
        if (project is null)
        {
            return null;
        }

        var association = (await _pamAssociationService.ResolverAsync(
            new[] { project },
            cancellationToken)).First();
        var highCandidates = association.Candidatos
            .Where(candidate =>
                string.Equals(
                    candidate.NivelConfianza,
                    "alta",
                    StringComparison.OrdinalIgnoreCase))
            .ToList();
        var seedKeys = highCandidates
            .Select(candidate => candidate.ClaveElementoRed)
            .Where(value => !string.IsNullOrWhiteSpace(value))
            .Distinct(StringComparer.Ordinal)
            .ToList();
        var snapshot = await GetAsync(false, cancellationToken);
        depth = Math.Clamp(depth, 0, 5);

        var nodeIds = snapshot.Nodes.Values
            .Where(node => seedKeys.Contains(
                node.CatalogElementKey,
                StringComparer.Ordinal))
            .Select(node => node.NodeId)
            .ToHashSet(StringComparer.Ordinal);
        var edgeIds = snapshot.Edges.Values
            .Where(edge => seedKeys.Contains(
                edge.CatalogElementKey,
                StringComparer.Ordinal))
            .Select(edge => edge.EdgeId)
            .ToHashSet(StringComparer.Ordinal);

        foreach (var edgeId in edgeIds.ToArray())
        {
            var edge = snapshot.Edges[edgeId];
            nodeIds.Add(edge.FromNodeId);
            nodeIds.Add(edge.ToNodeId);
        }

        var frontier = new HashSet<string>(nodeIds, StringComparer.Ordinal);
        for (var level = 0; level < depth && frontier.Count > 0; level++)
        {
            var next = new HashSet<string>(StringComparer.Ordinal);
            foreach (var nodeId in frontier)
            {
                if (!snapshot.Adjacency.TryGetValue(nodeId, out var adjacent))
                {
                    continue;
                }

                foreach (var connection in adjacent)
                {
                    edgeIds.Add(connection.EdgeId);
                    if (nodeIds.Add(connection.NeighborNodeId))
                    {
                        next.Add(connection.NeighborNodeId);
                    }
                }
            }
            frontier = next;
        }

        var features = edgeIds
            .Select(id => EdgeFeature(snapshot.Edges[id], snapshot.Nodes))
            .Concat(nodeIds.Select(id => NodeFeature(snapshot.Nodes[id])))
            .ToList();

        return new RedElectricaPamSubgraphResult
        {
            ProjectId = project.ProyectoId,
            ProjectKey = project.ClaveProyecto,
            ProjectName = project.NombreProyecto,
            Depth = depth,
            HighConfidenceAssociations = association.ConfianzaAlta,
            ReviewAssociations = association.RequierenRevision,
            SeedElementKeys = seedKeys,
            GeoJson = CreateGeoJson(
                snapshot,
                "subgrafo_pam",
                features,
                nodeIds.Count,
                edgeIds.Count)
        };
    }

    private static RedElectricaGraphSimulation CompareSnapshots(
        RedElectricaGraphSnapshot baseline,
        RedElectricaGraphSnapshot candidate)
    {
        var commonEdgeIds = baseline.Edges.Keys
            .Intersect(candidate.Edges.Keys, StringComparer.Ordinal)
            .ToList();
        var changes = new List<RedElectricaGraphSimulationChange>();
        var promotedToConnected = 0;
        var promotedToPartial = 0;
        var regressions = 0;

        foreach (var edgeId in commonEdgeIds)
        {
            var previous = baseline.Edges[edgeId];
            var current = candidate.Edges[edgeId];
            var previousRank = ConnectionStateRank(
                previous.ConnectionState);
            var currentRank = ConnectionStateRank(
                current.ConnectionState);
            if (currentRank == 2 && previousRank < 2)
            {
                promotedToConnected++;
            }
            else if (currentRank == 1 && previousRank == 0)
            {
                promotedToPartial++;
            }
            else if (currentRank < previousRank)
            {
                regressions++;
            }

            var changed =
                !string.Equals(
                    previous.ConnectionState,
                    current.ConnectionState,
                    StringComparison.Ordinal) ||
                !string.Equals(
                    previous.FromNodeId,
                    current.FromNodeId,
                    StringComparison.Ordinal) ||
                !string.Equals(
                    previous.ToNodeId,
                    current.ToNodeId,
                    StringComparison.Ordinal) ||
                previous.FromConfidence != current.FromConfidence ||
                previous.ToConfidence != current.ToConfidence;
            if (!changed)
            {
                continue;
            }

            changes.Add(new RedElectricaGraphSimulationChange
            {
                EdgeId = edgeId,
                NombreLinea = current.Name,
                EstadoAnterior = previous.ConnectionState,
                EstadoCandidato = current.ConnectionState,
                ExtremoOrigenNominal = current.NominalEndpointA,
                ExtremoDestinoNominal = current.NominalEndpointB,
                OrigenAnterior = ResolveNodeName(
                    baseline,
                    previous.FromNodeId),
                OrigenCandidato = ResolveNodeName(
                    candidate,
                    current.FromNodeId),
                DestinoAnterior = ResolveNodeName(
                    baseline,
                    previous.ToNodeId),
                DestinoCandidato = ResolveNodeName(
                    candidate,
                    current.ToNodeId),
                ConfianzaOrigenAnterior = previous.FromConfidence,
                ConfianzaOrigenCandidata = current.FromConfidence,
                ConfianzaDestinoAnterior = previous.ToConfidence,
                ConfianzaDestinoCandidata = current.ToConfidence,
                ResolucionOrigenCandidata = current.FromResolution,
                ResolucionDestinoCandidata = current.ToResolution
            });
        }

        var regressionEdgeIds = changes
            .Where(change =>
                ConnectionStateRank(change.EstadoCandidato) <
                ConnectionStateRank(change.EstadoAnterior))
            .Select(change => change.EdgeId)
            .ToHashSet(StringComparer.Ordinal);

        return new RedElectricaGraphSimulation
        {
            BasePersistida = baseline.Summary,
            Candidata = candidate.Summary,
            MismasFuentes =
                string.Equals(
                    baseline.Summary.HashSubestaciones,
                    candidate.Summary.HashSubestaciones,
                    StringComparison.Ordinal) &&
                string.Equals(
                    baseline.Summary.HashLineas,
                    candidate.Summary.HashLineas,
                    StringComparison.Ordinal),
            AristasComparables = commonEdgeIds.Count,
            AristasNuevas = candidate.Edges.Keys
                .Except(baseline.Edges.Keys, StringComparer.Ordinal)
                .Count(),
            AristasRetiradas = baseline.Edges.Keys
                .Except(candidate.Edges.Keys, StringComparer.Ordinal)
                .Count(),
            AristasConCambio = changes.Count,
            PromovidasAConectada = promotedToConnected,
            PromovidasAParcial = promotedToPartial,
            Regresiones = regressions,
            RevisionesReducidas = Math.Max(
                0,
                baseline.Summary.RevisionesPendientes -
                candidate.Summary.RevisionesPendientes),
            Cambios = changes
                .OrderByDescending(change =>
                    ConnectionStateRank(change.EstadoCandidato) -
                    ConnectionStateRank(change.EstadoAnterior))
                .ThenBy(
                    change => change.NombreLinea,
                    StringComparer.OrdinalIgnoreCase)
                .Take(250)
                .ToList(),
            RegresionesDetalle = changes
                .Where(change =>
                    ConnectionStateRank(change.EstadoCandidato) <
                    ConnectionStateRank(change.EstadoAnterior))
                .OrderBy(
                    change => change.NombreLinea,
                    StringComparer.OrdinalIgnoreCase)
                .ToList(),
            RevisionesRegresion = candidate.Reviews
                .Where(review =>
                    regressionEdgeIds.Contains(review.EdgeId))
                .OrderBy(
                    review => review.LineName,
                    StringComparer.OrdinalIgnoreCase)
                .ThenBy(review => review.EndpointSide)
                .ToList()
        };
    }

    private static int ConnectionStateRank(string state) =>
        state switch
        {
            "conectada" => 2,
            "parcial" => 1,
            _ => 0
        };

    private static string ResolveNodeName(
        RedElectricaGraphSnapshot snapshot,
        string nodeId) =>
        snapshot.Nodes.TryGetValue(nodeId, out var node)
            ? node.Name
            : nodeId;

    private async Task PersistAsync(
        RedElectricaGraphSnapshot snapshot,
        CancellationToken cancellationToken)
    {
        if (string.IsNullOrWhiteSpace(_connectionString))
        {
            throw new InvalidOperationException(
                "No existe ConnectionStrings:DefaultConnection para persistir el grafo.");
        }

        await using var connection = new SqlConnection(_connectionString);
        await connection.OpenAsync(cancellationToken);
        await using var transaction =
            (SqlTransaction)await connection.BeginTransactionAsync(
                cancellationToken);

        try
        {
            long? existingVersionId;
            await using (var existingCommand = new SqlCommand(
                """
                SELECT VersionId
                FROM dgmesnie.RedElectricaVersion WITH (UPDLOCK, HOLDLOCK)
                WHERE VersionClave = @VersionClave;
                """,
                connection,
                transaction))
            {
                existingCommand.Parameters.AddWithValue(
                    "@VersionClave",
                    snapshot.Summary.Version);
                var existing = await existingCommand.ExecuteScalarAsync(
                    cancellationToken);
                existingVersionId = existing is null or DBNull
                    ? null
                    : Convert.ToInt64(existing, CultureInfo.InvariantCulture);
            }

            long versionId;
            if (existingVersionId.HasValue)
            {
                versionId = existingVersionId.Value;
                await using var activateCommand = new SqlCommand(
                    """
                    UPDATE dgmesnie.RedElectricaVersion
                    SET Activa = CASE WHEN VersionId = @VersionId THEN 1 ELSE 0 END;
                    """,
                    connection,
                    transaction);
                activateCommand.Parameters.AddWithValue("@VersionId", versionId);
                await activateCommand.ExecuteNonQueryAsync(cancellationToken);
            }
            else
            {
                await using (var deactivateCommand = new SqlCommand(
                    """
                    UPDATE dgmesnie.RedElectricaVersion
                    SET Activa = 0
                    WHERE Activa = 1;
                    """,
                    connection,
                    transaction))
                {
                    await deactivateCommand.ExecuteNonQueryAsync(cancellationToken);
                }

                await using (var insertVersionCommand = new SqlCommand(
                    """
                    INSERT INTO dgmesnie.RedElectricaVersion
                    (
                        VersionClave,
                        VersionReglas,
                        FuenteSubestaciones,
                        FuenteLineas,
                        HashSubestaciones,
                        HashLineas,
                        GeneradoUtc,
                        DuracionConstruccionMs,
                        ResumenJson,
                        Estado,
                        Activa
                    )
                    OUTPUT INSERTED.VersionId
                    VALUES
                    (
                        @VersionClave,
                        @VersionReglas,
                        @FuenteSubestaciones,
                        @FuenteLineas,
                        @HashSubestaciones,
                        @HashLineas,
                        @GeneradoUtc,
                        @DuracionConstruccionMs,
                        @ResumenJson,
                        N'publicada',
                        1
                    );
                    """,
                    connection,
                    transaction))
                {
                    insertVersionCommand.Parameters.AddWithValue(
                        "@VersionClave",
                        snapshot.Summary.Version);
                    insertVersionCommand.Parameters.AddWithValue(
                        "@VersionReglas",
                        snapshot.Summary.VersionReglas);
                    insertVersionCommand.Parameters.AddWithValue(
                        "@FuenteSubestaciones",
                        snapshot.Summary.FuenteSubestaciones);
                    insertVersionCommand.Parameters.AddWithValue(
                        "@FuenteLineas",
                        snapshot.Summary.FuenteLineas);
                    insertVersionCommand.Parameters.AddWithValue(
                        "@HashSubestaciones",
                        snapshot.Summary.HashSubestaciones);
                    insertVersionCommand.Parameters.AddWithValue(
                        "@HashLineas",
                        snapshot.Summary.HashLineas);
                    insertVersionCommand.Parameters.AddWithValue(
                        "@GeneradoUtc",
                        snapshot.Summary.GeneradoUtc);
                    insertVersionCommand.Parameters.AddWithValue(
                        "@DuracionConstruccionMs",
                        snapshot.Summary.DuracionConstruccionMs);
                    insertVersionCommand.Parameters.AddWithValue(
                        "@ResumenJson",
                        JsonSerializer.Serialize(snapshot.Summary));
                    versionId = Convert.ToInt64(
                        await insertVersionCommand.ExecuteScalarAsync(
                            cancellationToken),
                        CultureInfo.InvariantCulture);
                }

                await BulkCopyAsync(
                    connection,
                    transaction,
                    "dgmesnie.RedElectricaNodo",
                    CreateNodesTable(versionId, snapshot.Nodes.Values),
                    cancellationToken);
                await BulkCopyAsync(
                    connection,
                    transaction,
                    "dgmesnie.RedElectricaArista",
                    CreateEdgesTable(versionId, snapshot.Edges.Values),
                    cancellationToken);
                await BulkCopyAsync(
                    connection,
                    transaction,
                    "dgmesnie.RedElectricaRevision",
                    CreateReviewsTable(versionId, snapshot.Reviews),
                    cancellationToken);
            }

            await transaction.CommitAsync(cancellationToken);
            snapshot.Summary.Persistida = true;
            snapshot.Summary.VersionIdBaseDatos = versionId;
            _logger.LogInformation(
                "Versión {Version} del grafo persistida como VersionId {VersionId}.",
                snapshot.Summary.Version,
                versionId);
        }
        catch
        {
            await transaction.RollbackAsync(cancellationToken);
            throw;
        }
    }

    private async Task<RedElectricaGraphSnapshot?> LoadPersistedAsync(
        CancellationToken cancellationToken)
    {
        await using var connection = new SqlConnection(_connectionString);
        await connection.OpenAsync(cancellationToken);

        long versionId;
        RedElectricaGraphSummary summary;
        await using (var versionCommand = new SqlCommand(
            """
            SELECT TOP (1)
                VersionId,
                ResumenJson
            FROM dgmesnie.RedElectricaVersion
            WHERE Activa = 1
              AND Estado = N'publicada'
            ORDER BY VersionId DESC;
            """,
            connection))
        await using (var reader = await versionCommand.ExecuteReaderAsync(
            cancellationToken))
        {
            if (!await reader.ReadAsync(cancellationToken))
            {
                return null;
            }

            versionId = reader.GetInt64(0);
            summary = JsonSerializer.Deserialize<RedElectricaGraphSummary>(
                reader.GetString(1),
                new JsonSerializerOptions
                {
                    PropertyNameCaseInsensitive = true
                }) ?? throw new JsonException(
                    "El resumen persistido del grafo no es válido.");
        }

        var nodes = new Dictionary<string, RedElectricaGraphNode>(
            StringComparer.Ordinal);
        await using (var nodeCommand = new SqlCommand(
            """
            SELECT
                NodoClave,
                ClaveElementoCatalogo,
                TipoNodo,
                Nombre,
                NombreNormalizado,
                Latitud,
                Longitud,
                TensionKv,
                Fase,
                Fuente,
                EsVirtual,
                MotivoVirtual,
                Grado,
                ComponenteClave
            FROM dgmesnie.RedElectricaNodo
            WHERE VersionId = @VersionId;
            """,
            connection))
        {
            nodeCommand.Parameters.AddWithValue("@VersionId", versionId);
            await using var reader = await nodeCommand.ExecuteReaderAsync(
                cancellationToken);
            while (await reader.ReadAsync(cancellationToken))
            {
                var node = new RedElectricaGraphNode
                {
                    NodeId = reader.GetString(0),
                    CatalogElementKey = ReadString(reader, 1),
                    Type = reader.GetString(2),
                    Name = reader.GetString(3),
                    NormalizedName = reader.GetString(4),
                    Latitude = Convert.ToDouble(
                        reader.GetValue(5),
                        CultureInfo.InvariantCulture),
                    Longitude = Convert.ToDouble(
                        reader.GetValue(6),
                        CultureInfo.InvariantCulture),
                    VoltageKv = ReadDouble(reader, 7),
                    NetworkLevel = RedElectricaNetworkClassifier.Classify(
                        ReadDouble(reader, 7)),
                    Phase = ReadString(reader, 8),
                    Source = reader.GetString(9),
                    IsVirtual = reader.GetBoolean(10),
                    VirtualReason = ReadString(reader, 11),
                    ValidationState = reader.GetBoolean(10)
                        ? "pendiente_revision"
                        : "catalogado",
                    Degree = reader.GetInt32(12),
                    ComponentId = ReadString(reader, 13)
                };
                nodes[node.NodeId] = node;
            }
        }

        var edges = new Dictionary<string, RedElectricaGraphEdge>(
            StringComparer.Ordinal);
        await using (var edgeCommand = new SqlCommand(
            """
            SELECT
                AristaClave,
                ClaveElementoCatalogo,
                Nombre,
                NombreNormalizado,
                NodoOrigenClave,
                NodoDestinoClave,
                ExtremoNominalA,
                ExtremoNominalB,
                ConfianzaOrigen,
                ConfianzaDestino,
                ResolucionOrigen,
                ResolucionDestino,
                EstadoConexion,
                TensionKv,
                Circuitos,
                LongitudCatalogoKm,
                LongitudGeometriaKm,
                IndiceSegmento,
                GeometriaJson,
                Fuente
            FROM dgmesnie.RedElectricaArista
            WHERE VersionId = @VersionId;
            """,
            connection))
        {
            edgeCommand.Parameters.AddWithValue("@VersionId", versionId);
            await using var reader = await edgeCommand.ExecuteReaderAsync(
                cancellationToken);
            while (await reader.ReadAsync(cancellationToken))
            {
                var geometry = JsonDocument.Parse(reader.GetString(18))
                    .RootElement.Clone();
                var edge = new RedElectricaGraphEdge
                {
                    EdgeId = reader.GetString(0),
                    CatalogElementKey = reader.GetString(1),
                    Name = reader.GetString(2),
                    NormalizedName = reader.GetString(3),
                    FromNodeId = reader.GetString(4),
                    ToNodeId = reader.GetString(5),
                    NominalEndpointA = ReadString(reader, 6),
                    NominalEndpointB = ReadString(reader, 7),
                    FromConfidence = reader.GetInt32(8),
                    ToConfidence = reader.GetInt32(9),
                    FromResolution = reader.GetString(10),
                    ToResolution = reader.GetString(11),
                    ConnectionState = reader.GetString(12),
                    VoltageKv = ReadDouble(reader, 13),
                    NetworkLevel = RedElectricaNetworkClassifier.Classify(
                        ReadDouble(reader, 13)),
                    Circuits = ReadInt(reader, 14),
                    CatalogLengthKm = ReadDouble(reader, 15),
                    GeometryLengthKm = Convert.ToDouble(
                        reader.GetValue(16),
                        CultureInfo.InvariantCulture),
                    SegmentIndex = reader.GetInt32(17),
                    Geometry = geometry,
                    Source = reader.GetString(19)
                };
                edges[edge.EdgeId] = edge;
            }
        }

        var reviews = new List<RedElectricaGraphReview>();
        await using (var reviewCommand = new SqlCommand(
            """
            SELECT
                RevisionClave,
                AristaClave,
                NombreLinea,
                LadoExtremo,
                ExtremoNominal,
                Latitud,
                Longitud,
                Motivo,
                CandidatosJson
            FROM dgmesnie.RedElectricaRevision
            WHERE VersionId = @VersionId
              AND Resuelta = 0
            ORDER BY RevisionId;
            """,
            connection))
        {
            reviewCommand.Parameters.AddWithValue("@VersionId", versionId);
            await using var reader = await reviewCommand.ExecuteReaderAsync(
                cancellationToken);
            while (await reader.ReadAsync(cancellationToken))
            {
                reviews.Add(new RedElectricaGraphReview
                {
                    ReviewId = reader.GetString(0),
                    EdgeId = reader.GetString(1),
                    LineName = reader.GetString(2),
                    EndpointSide = reader.GetString(3),
                    NominalEndpoint = ReadString(reader, 4),
                    Latitude = Convert.ToDouble(
                        reader.GetValue(5),
                        CultureInfo.InvariantCulture),
                    Longitude = Convert.ToDouble(
                        reader.GetValue(6),
                        CultureInfo.InvariantCulture),
                    Reason = reader.GetString(7),
                    Candidates =
                        JsonSerializer.Deserialize<List<RedElectricaGraphCandidate>>(
                            reader.GetString(8),
                            new JsonSerializerOptions
                            {
                                PropertyNameCaseInsensitive = true
                            }) ?? new List<RedElectricaGraphCandidate>()
                });
            }
        }

        var adjacency = BuildAdjacency(nodes, edges);
        AssignComponents(nodes, adjacency);
        summary.SubestacionesTransmision = nodes.Values.Count(node =>
            !node.IsVirtual &&
            node.NetworkLevel == RedElectricaNetworkLevels.Transmission);
        summary.SubestacionesSubtransmision = nodes.Values.Count(node =>
            !node.IsVirtual &&
            node.NetworkLevel == RedElectricaNetworkLevels.Subtransmission);
        summary.SubestacionesDistribucion = nodes.Values.Count(node =>
            !node.IsVirtual &&
            node.NetworkLevel == RedElectricaNetworkLevels.Distribution);
        summary.LineasTransmision = edges.Values.Count(edge =>
            edge.NetworkLevel == RedElectricaNetworkLevels.Transmission);
        summary.LineasSubtransmision = edges.Values.Count(edge =>
            edge.NetworkLevel == RedElectricaNetworkLevels.Subtransmission);
        summary.LineasDistribucion = edges.Values.Count(edge =>
            edge.NetworkLevel == RedElectricaNetworkLevels.Distribution);
        summary.Persistida = true;
        summary.VersionIdBaseDatos = versionId;
        _logger.LogInformation(
            "Grafo eléctrico {Version} cargado desde VersionId {VersionId}.",
            summary.Version,
            versionId);
        return new RedElectricaGraphSnapshot
        {
            Summary = summary,
            Nodes = nodes,
            Edges = edges,
            Adjacency = adjacency,
            Reviews = reviews
        };
    }

    private static async Task BulkCopyAsync(
        SqlConnection connection,
        SqlTransaction transaction,
        string destination,
        DataTable table,
        CancellationToken cancellationToken)
    {
        using var bulk = new SqlBulkCopy(
            connection,
            SqlBulkCopyOptions.TableLock |
            SqlBulkCopyOptions.CheckConstraints,
            transaction)
        {
            DestinationTableName = destination,
            BatchSize = 1000,
            BulkCopyTimeout = 120
        };
        foreach (DataColumn column in table.Columns)
        {
            bulk.ColumnMappings.Add(column.ColumnName, column.ColumnName);
        }
        await bulk.WriteToServerAsync(table, cancellationToken);
    }

    private static DataTable CreateNodesTable(
        long versionId,
        IEnumerable<RedElectricaGraphNode> nodes)
    {
        var table = new DataTable();
        AddColumn<long>(table, "VersionId");
        AddColumn<string>(table, "NodoClave");
        AddColumn<string>(table, "ClaveElementoCatalogo");
        AddColumn<string>(table, "TipoNodo");
        AddColumn<string>(table, "Nombre");
        AddColumn<string>(table, "NombreNormalizado");
        AddColumn<decimal>(table, "Latitud");
        AddColumn<decimal>(table, "Longitud");
        AddColumn<decimal>(table, "TensionKv", true);
        AddColumn<string>(table, "Fase");
        AddColumn<string>(table, "Fuente");
        AddColumn<bool>(table, "EsVirtual");
        AddColumn<string>(table, "MotivoVirtual");
        AddColumn<int>(table, "Grado");
        AddColumn<string>(table, "ComponenteClave");

        foreach (var node in nodes)
        {
            table.Rows.Add(
                versionId,
                node.NodeId,
                DbValue(node.CatalogElementKey),
                node.Type,
                node.Name,
                node.NormalizedName,
                Convert.ToDecimal(node.Latitude),
                Convert.ToDecimal(node.Longitude),
                DbValue(node.VoltageKv),
                DbValue(node.Phase),
                node.Source,
                node.IsVirtual,
                DbValue(node.VirtualReason),
                node.Degree,
                DbValue(node.ComponentId));
        }
        return table;
    }

    private static DataTable CreateEdgesTable(
        long versionId,
        IEnumerable<RedElectricaGraphEdge> edges)
    {
        var table = new DataTable();
        AddColumn<long>(table, "VersionId");
        AddColumn<string>(table, "AristaClave");
        AddColumn<string>(table, "ClaveElementoCatalogo");
        AddColumn<string>(table, "Nombre");
        AddColumn<string>(table, "NombreNormalizado");
        AddColumn<string>(table, "NodoOrigenClave");
        AddColumn<string>(table, "NodoDestinoClave");
        AddColumn<string>(table, "ExtremoNominalA");
        AddColumn<string>(table, "ExtremoNominalB");
        AddColumn<int>(table, "ConfianzaOrigen");
        AddColumn<int>(table, "ConfianzaDestino");
        AddColumn<string>(table, "ResolucionOrigen");
        AddColumn<string>(table, "ResolucionDestino");
        AddColumn<string>(table, "EstadoConexion");
        AddColumn<decimal>(table, "TensionKv", true);
        AddColumn<int>(table, "Circuitos", true);
        AddColumn<decimal>(table, "LongitudCatalogoKm", true);
        AddColumn<decimal>(table, "LongitudGeometriaKm");
        AddColumn<int>(table, "IndiceSegmento");
        AddColumn<string>(table, "GeometriaJson");
        AddColumn<string>(table, "Fuente");

        foreach (var edge in edges)
        {
            table.Rows.Add(
                versionId,
                edge.EdgeId,
                edge.CatalogElementKey,
                edge.Name,
                edge.NormalizedName,
                edge.FromNodeId,
                edge.ToNodeId,
                DbValue(edge.NominalEndpointA),
                DbValue(edge.NominalEndpointB),
                edge.FromConfidence,
                edge.ToConfidence,
                edge.FromResolution,
                edge.ToResolution,
                edge.ConnectionState,
                DbValue(edge.VoltageKv),
                DbValue(edge.Circuits),
                DbValue(edge.CatalogLengthKm),
                Convert.ToDecimal(edge.GeometryLengthKm),
                edge.SegmentIndex,
                edge.Geometry.GetRawText(),
                edge.Source);
        }
        return table;
    }

    private static DataTable CreateReviewsTable(
        long versionId,
        IEnumerable<RedElectricaGraphReview> reviews)
    {
        var table = new DataTable();
        AddColumn<long>(table, "VersionId");
        AddColumn<string>(table, "RevisionClave");
        AddColumn<string>(table, "AristaClave");
        AddColumn<string>(table, "NombreLinea");
        AddColumn<string>(table, "LadoExtremo");
        AddColumn<string>(table, "ExtremoNominal");
        AddColumn<decimal>(table, "Latitud");
        AddColumn<decimal>(table, "Longitud");
        AddColumn<string>(table, "Motivo");
        AddColumn<string>(table, "CandidatosJson");
        AddColumn<bool>(table, "Resuelta");

        foreach (var review in reviews)
        {
            table.Rows.Add(
                versionId,
                review.ReviewId,
                review.EdgeId,
                review.LineName,
                review.EndpointSide,
                DbValue(review.NominalEndpoint),
                Convert.ToDecimal(review.Latitude),
                Convert.ToDecimal(review.Longitude),
                review.Reason,
                JsonSerializer.Serialize(review.Candidates),
                false);
        }
        return table;
    }

    private static void AddColumn<T>(
        DataTable table,
        string name,
        bool allowNull = false)
    {
        var column = table.Columns.Add(name, typeof(T));
        column.AllowDBNull = allowNull || typeof(T) == typeof(string);
    }

    private static object DbValue(string? value) =>
        string.IsNullOrWhiteSpace(value) ? DBNull.Value : value;

    private static object DbValue(double? value) =>
        value.HasValue ? Convert.ToDecimal(value.Value) : DBNull.Value;

    private static object DbValue(int? value) =>
        value.HasValue ? value.Value : DBNull.Value;

    private static string ReadString(SqlDataReader reader, int index) =>
        reader.IsDBNull(index) ? string.Empty : reader.GetString(index);

    private static double? ReadDouble(SqlDataReader reader, int index) =>
        reader.IsDBNull(index)
            ? null
            : Convert.ToDouble(
                reader.GetValue(index),
                CultureInfo.InvariantCulture);

    private static int? ReadInt(SqlDataReader reader, int index) =>
        reader.IsDBNull(index)
            ? null
            : Convert.ToInt32(
                reader.GetValue(index),
                CultureInfo.InvariantCulture);

    private async Task<RedElectricaGraphSnapshot> BuildAsync(
        CancellationToken cancellationToken)
    {
        var stopwatch = Stopwatch.StartNew();
        var substationsTask = _httpClient.GetStringAsync(
            _options.SubstationsUrl,
            cancellationToken);
        var linesTask = _httpClient.GetStringAsync(
            _options.LinesUrl,
            cancellationToken);
        await Task.WhenAll(substationsTask, linesTask);

        var substationsJson = await substationsTask;
        var linesJson = await linesTask;
        var substationHash = Sha256(substationsJson);
        var lineHash = Sha256(linesJson);
        var version = $"red:{Sha256($"{RulesVersion}|{substationHash}|{lineHash}")[..20]}";

        var nodes = ParseSubstations(substationsJson);
        var nameCounts = nodes.Values
            .Where(node => !string.IsNullOrWhiteSpace(node.NormalizedName))
            .GroupBy(node => node.NormalizedName, StringComparer.Ordinal)
            .ToDictionary(
                group => group.Key,
                group => group.Count(),
                StringComparer.Ordinal);
        var lines = ParseLines(linesJson);
        var reviews = new List<RedElectricaGraphReview>();
        var edges = new Dictionary<string, RedElectricaGraphEdge>(
            StringComparer.Ordinal);
        var virtualNodes = new List<RedElectricaGraphNode>();

        foreach (var line in lines)
        {
            cancellationToken.ThrowIfCancellationRequested();
            var direct = AnalyzeOrientation(
                line,
                line.Start,
                line.End,
                line.NominalEndpointA,
                line.NominalEndpointB,
                nodes.Values,
                nameCounts);
            var reverse = AnalyzeOrientation(
                line,
                line.Start,
                line.End,
                line.NominalEndpointB,
                line.NominalEndpointA,
                nodes.Values,
                nameCounts);
            var selected = reverse.TotalScore > direct.TotalScore
                ? reverse
                : direct;

            var fromNode = ResolveOrCreateVirtualNode(
                line,
                "A",
                line.Start,
                selected.From,
                nodes,
                virtualNodes);
            var toNode = ResolveOrCreateVirtualNode(
                line,
                "B",
                line.End,
                selected.To,
                nodes,
                virtualNodes);

            if (string.Equals(
                    fromNode.NodeId,
                    toNode.NodeId,
                    StringComparison.Ordinal) &&
                !fromNode.IsVirtual)
            {
                selected = selected with
                {
                    To = selected.To with
                    {
                        ResolvedNode = null,
                        Reason = "Los dos extremos resolverían a la misma subestación."
                    }
                };
                toNode = ResolveOrCreateVirtualNode(
                    line,
                    "B",
                    line.End,
                    selected.To,
                    nodes,
                    virtualNodes);
            }

            var edgeId = line.EdgeId;
            AddReviewIfNeeded(
                reviews,
                edgeId,
                line,
                "A",
                selected.From);
            AddReviewIfNeeded(
                reviews,
                edgeId,
                line,
                "B",
                selected.To);

            var physicalEnds =
                (fromNode.IsVirtual ? 0 : 1) +
                (toNode.IsVirtual ? 0 : 1);
            var state = physicalEnds switch
            {
                2 => "conectada",
                1 => "parcial",
                _ => "sin_resolver"
            };

            edges[edgeId] = new RedElectricaGraphEdge
            {
                EdgeId = edgeId,
                CatalogElementKey = line.CatalogElementKey,
                Name = line.Name,
                NormalizedName = line.NormalizedName,
                FromNodeId = fromNode.NodeId,
                ToNodeId = toNode.NodeId,
                NominalEndpointA = selected.From.NominalName,
                NominalEndpointB = selected.To.NominalName,
                FromConfidence = selected.From.TopScore,
                ToConfidence = selected.To.TopScore,
                FromResolution = selected.From.Resolution,
                ToResolution = selected.To.Resolution,
                ConnectionState = state,
                VoltageKv = line.VoltageKv,
                NetworkLevel = RedElectricaNetworkClassifier.Classify(
                    line.VoltageKv),
                Circuits = line.Circuits,
                CatalogLengthKm = line.CatalogLengthKm,
                GeometryLengthKm = line.GeometryLengthKm,
                SegmentIndex = line.SegmentIndex,
                Geometry = line.Geometry,
                Source = _options.LinesUrl
            };
        }

        var uniqueReviews = reviews
            .GroupBy(review => review.ReviewId, StringComparer.Ordinal)
            .Select(group => group.First())
            .ToList();
        var adjacency = BuildAdjacency(nodes, edges);
        AssignComponents(nodes, adjacency);
        stopwatch.Stop();

        var summary = new RedElectricaGraphSummary
        {
            Version = version,
            VersionReglas = RulesVersion,
            GeneradoUtc = DateTime.UtcNow,
            DuracionConstruccionMs = stopwatch.ElapsedMilliseconds,
            Subestaciones = nodes.Values.Count(node => !node.IsVirtual),
            NodosVirtuales = nodes.Values.Count(node => node.IsVirtual),
            NodosTotales = nodes.Count,
            LineasCatalogo = lines
                .Select(line => line.CatalogElementKey)
                .Distinct(StringComparer.Ordinal)
                .Count(),
            Aristas = edges.Count,
            AristasConectadas = edges.Values.Count(edge =>
                edge.ConnectionState == "conectada"),
            AristasParciales = edges.Values.Count(edge =>
                edge.ConnectionState == "parcial"),
            AristasSinResolver = edges.Values.Count(edge =>
                edge.ConnectionState == "sin_resolver"),
            Componentes = nodes.Values
                .Select(node => node.ComponentId)
                .Where(value => !string.IsNullOrWhiteSpace(value))
                .Distinct(StringComparer.Ordinal)
                .Count(),
            NodosAislados = nodes.Values.Count(node => node.Degree == 0),
            RevisionesPendientes = uniqueReviews.Count,
            SubestacionesTransmision = nodes.Values.Count(node =>
                !node.IsVirtual &&
                node.NetworkLevel ==
                RedElectricaNetworkLevels.Transmission),
            SubestacionesSubtransmision = nodes.Values.Count(node =>
                !node.IsVirtual &&
                node.NetworkLevel ==
                RedElectricaNetworkLevels.Subtransmission),
            SubestacionesDistribucion = nodes.Values.Count(node =>
                !node.IsVirtual &&
                node.NetworkLevel ==
                RedElectricaNetworkLevels.Distribution),
            LineasTransmision = edges.Values.Count(edge =>
                edge.NetworkLevel ==
                RedElectricaNetworkLevels.Transmission),
            LineasSubtransmision = edges.Values.Count(edge =>
                edge.NetworkLevel ==
                RedElectricaNetworkLevels.Subtransmission),
            LineasDistribucion = edges.Values.Count(edge =>
                edge.NetworkLevel ==
                RedElectricaNetworkLevels.Distribution),
            FuenteSubestaciones = _options.SubstationsUrl,
            FuenteLineas = _options.LinesUrl,
            HashSubestaciones = substationHash,
            HashLineas = lineHash
        };

        _logger.LogInformation(
            "Grafo eléctrico {Version}: {Nodes} nodos, {Edges} aristas, {Connected} conectadas y {Reviews} revisiones en {Elapsed} ms.",
            summary.Version,
            summary.NodosTotales,
            summary.Aristas,
            summary.AristasConectadas,
            summary.RevisionesPendientes,
            summary.DuracionConstruccionMs);

        return new RedElectricaGraphSnapshot
        {
            Summary = summary,
            Nodes = nodes,
            Edges = edges,
            Adjacency = adjacency,
            Reviews = uniqueReviews
        };
    }

    private Dictionary<string, RedElectricaGraphNode> ParseSubstations(string json)
    {
        using var document = JsonDocument.Parse(json);
        var nodes = new Dictionary<string, RedElectricaGraphNode>(
            StringComparer.Ordinal);

        foreach (var feature in GetFeatures(document.RootElement))
        {
            var properties = GetProperty(feature, "properties");
            var geometry = GetProperty(feature, "geometry");
            if (!string.Equals(
                    GetString(geometry, "type"),
                    "Point",
                    StringComparison.OrdinalIgnoreCase) ||
                !TryPoint(GetProperty(geometry, "coordinates"), out var point))
            {
                continue;
            }

            var name = GetString(properties, "name", "nombre", "subestacion");
            var normalizedName = NormalizeSubstationName(name);
            if (string.IsNullOrWhiteSpace(normalizedName))
            {
                continue;
            }

            var voltage = GetDouble(
                properties,
                "voltaje_kv",
                "voltaje_KV",
                "tension_kv");
            var nodeId = StableKey(
                "SE",
                normalizedName,
                voltage,
                geometry.GetRawText());
            nodes[nodeId] = new RedElectricaGraphNode
            {
                NodeId = nodeId,
                CatalogElementKey = nodeId,
                Type = "subestacion",
                Name = name,
                NormalizedName = normalizedName,
                Latitude = point.Latitude,
                Longitude = point.Longitude,
                VoltageKv = voltage,
                NetworkLevel = RedElectricaNetworkClassifier.Classify(
                    voltage),
                Phase = GetString(properties, "fase", "phase"),
                Source = _options.SubstationsUrl
            };
        }

        return nodes;
    }

    private IReadOnlyList<LineSeed> ParseLines(string json)
    {
        using var document = JsonDocument.Parse(json);
        var output = new List<LineSeed>();

        foreach (var feature in GetFeatures(document.RootElement))
        {
            var properties = GetProperty(feature, "properties");
            var geometry = GetProperty(feature, "geometry");
            var name = GetString(properties, "nombre_lt", "name", "nombre");
            var normalizedName = NormalizeLineName(name);
            if (string.IsNullOrWhiteSpace(normalizedName))
            {
                continue;
            }

            var voltage = GetDouble(properties, "voltaje_KV", "voltaje_kv");
            var characteristics = GetString(
                properties,
                "caracteris",
                "caracteristicas");
            var catalogKey = StableKey(
                "LT",
                normalizedName,
                voltage,
                geometry.GetRawText());
            var (endpointA, endpointB) = ParseLineEndpoints(name);
            var segments = ExtractLineSegments(geometry);
            for (var index = 0; index < segments.Count; index++)
            {
                var segment = segments[index];
                var points = ExtractLinePoints(segment);
                if (points.Count < 2)
                {
                    continue;
                }

                var geometryLength = 0d;
                for (var pointIndex = 1; pointIndex < points.Count; pointIndex++)
                {
                    geometryLength += HaversineKm(
                        points[pointIndex - 1],
                        points[pointIndex]);
                }
                var edgeId = StableKey(
                    "AR",
                    normalizedName,
                    voltage,
                    $"{index}|{segment.GetRawText()}");
                output.Add(new LineSeed(
                    edgeId,
                    catalogKey,
                    name,
                    normalizedName,
                    endpointA,
                    endpointB,
                    voltage,
                    ParseCircuits(characteristics),
                    ParseLengthKm(characteristics),
                    geometryLength,
                    index + 1,
                    points[0],
                    points[^1],
                    segment));
            }
        }

        return output;
    }

    private OrientationResolution AnalyzeOrientation(
        LineSeed line,
        GeoPoint start,
        GeoPoint end,
        string nominalStart,
        string nominalEnd,
        IEnumerable<RedElectricaGraphNode> nodes,
        IReadOnlyDictionary<string, int> nameCounts)
    {
        var from = AnalyzeEndpoint(
            line,
            start,
            nominalStart,
            nodes,
            nameCounts);
        var to = AnalyzeEndpoint(
            line,
            end,
            nominalEnd,
            nodes,
            nameCounts);
        return new OrientationResolution(from, to);
    }

    private EndpointResolution AnalyzeEndpoint(
        LineSeed line,
        GeoPoint endpoint,
        string nominalName,
        IEnumerable<RedElectricaGraphNode> nodes,
        IReadOnlyDictionary<string, int> nameCounts)
    {
        var candidates = new List<RedElectricaGraphCandidate>();
        foreach (var node in nodes)
        {
            if (node.IsVirtual ||
                Math.Abs(node.Latitude - endpoint.Latitude) >
                    _options.SearchRadiusKm / 110.574)
            {
                continue;
            }

            var longitudeDelta = _options.SearchRadiusKm /
                Math.Max(
                    20,
                    111.320 * Math.Cos(
                        endpoint.Latitude * Math.PI / 180));
            if (Math.Abs(node.Longitude - endpoint.Longitude) >
                longitudeDelta)
            {
                continue;
            }

            var distance = HaversineKm(
                endpoint,
                new GeoPoint(node.Latitude, node.Longitude));
            var nameMatch =
                RedElectricaEndpointNameMatcher.Evaluate(
                    nominalName,
                    node.NormalizedName,
                    distance);
            if (!nameMatch.IsMatch &&
                distance > _options.SearchRadiusKm)
            {
                continue;
            }

            var score = 0;
            var evidence = new List<string>();
            if (nameMatch.IsMatch)
            {
                var nameScore =
                    nameMatch.Kind ==
                        RedElectricaEndpointNameMatchKind.LiteralExact
                        ? 55
                        : 45;
                score += nameScore;
                evidence.Add(
                    $"{nameMatch.Evidence} (+{nameScore})");
                var uniqueCatalogName =
                    nameCounts.GetValueOrDefault(
                        node.NormalizedName) == 1;
                if (uniqueCatalogName)
                {
                    score += 10;
                    evidence.Add("nombre único en el catálogo (+10)");
                }
                if (nameMatch.Kind !=
                        RedElectricaEndpointNameMatchKind.LiteralExact &&
                    distance <= 0.25 &&
                    uniqueCatalogName)
                {
                    score += 10;
                    evidence.Add(
                        "equivalencia controlada única a 250 m (+10)");
                }
            }

            if (distance <= 0.25)
            {
                score += 25;
                evidence.Add($"extremo a {distance:0.###} km (+25)");
            }
            else if (distance <= 1)
            {
                score += 20;
                evidence.Add($"extremo a {distance:0.###} km (+20)");
            }
            else if (distance <= _options.EndpointToleranceKm)
            {
                score += 15;
                evidence.Add($"extremo a {distance:0.###} km (+15)");
            }
            else if (distance <= _options.SearchRadiusKm)
            {
                score += 8;
                evidence.Add($"extremo cercano a {distance:0.###} km (+8)");
            }

            if (line.VoltageKv.HasValue && node.VoltageKv.HasValue)
            {
                if (Math.Abs(line.VoltageKv.Value - node.VoltageKv.Value) <= 0.5)
                {
                    score += 10;
                    evidence.Add("tensión coincidente (+10)");
                }
                else
                {
                    // Una subestación puede transformar varios niveles de tensión y
                    // el catálogo puntual expone sólo uno. El nombre exacto y el
                    // extremo geométrico prevalecen; la diferencia sólo penaliza.
                    score -= 5;
                    evidence.Add("tensión catalogada distinta (-5)");
                }
            }

            if (score > 0)
            {
                candidates.Add(new RedElectricaGraphCandidate
                {
                    NodeId = node.NodeId,
                    Name = node.Name,
                    NameMatchKind = nameMatch.Kind.ToString(),
                    Score = Math.Clamp(score, 0, 100),
                    DistanceKm = distance,
                    VoltageKv = node.VoltageKv,
                    Evidence = evidence
                });
            }
        }

        // Una coincidencia nominal exacta en el extremo conserva precedencia
        // sobre variantes ortográficas o nombres extendidos cercanos. Así se
        // evita que estaciones numeradas contiguas (I/II, POT/DIST) empaten
        // artificialmente y degraden una conexión ya firme.
        var literalExactKind = nameof(
            RedElectricaEndpointNameMatchKind.LiteralExact);
        var canonicalExactKind = nameof(
            RedElectricaEndpointNameMatchKind.Exact);
        if (candidates.Any(candidate =>
                string.Equals(
                    candidate.NameMatchKind,
                    literalExactKind,
                    StringComparison.Ordinal) &&
                candidate.Score >=
                    _options.HighConfidenceThreshold))
        {
            candidates = candidates
                .Where(candidate =>
                    string.IsNullOrWhiteSpace(
                        candidate.NameMatchKind) ||
                    string.Equals(
                        candidate.NameMatchKind,
                        literalExactKind,
                        StringComparison.Ordinal))
                .ToList();
        }
        else if (candidates.Any(candidate =>
                     string.Equals(
                         candidate.NameMatchKind,
                         canonicalExactKind,
                         StringComparison.Ordinal) &&
                     candidate.Score >=
                        _options.HighConfidenceThreshold))
        {
            candidates = candidates
                .Where(candidate =>
                    string.IsNullOrWhiteSpace(
                        candidate.NameMatchKind) ||
                    string.Equals(
                        candidate.NameMatchKind,
                        canonicalExactKind,
                        StringComparison.Ordinal))
                .ToList();
        }

        // Un extremo de LT que cae prácticamente sobre una única subestación
        // aporta evidencia topológica aunque el nombre tenga abreviaturas o
        // errores ortográficos. Esto no considera cruces intermedios: sólo los
        // dos extremos reales de cada LineString.
        var nearest = candidates
            .OrderBy(candidate => candidate.DistanceKm)
            .Take(2)
            .ToList();
        if (nearest.Count > 0 &&
            nearest[0].DistanceKm <= 0.15 &&
            (nearest.Count == 1 || nearest[1].DistanceKm >= 0.5) &&
            !string.Equals(
                nearest[0].NameMatchKind,
                literalExactKind,
                StringComparison.Ordinal))
        {
            var candidateIndex = candidates.FindIndex(candidate =>
                string.Equals(
                    candidate.NodeId,
                    nearest[0].NodeId,
                    StringComparison.Ordinal));
            if (candidateIndex >= 0)
            {
                var topologyOnlyScore = 75;
                if (line.VoltageKv.HasValue &&
                    nearest[0].VoltageKv.HasValue)
                {
                    topologyOnlyScore +=
                        Math.Abs(
                            line.VoltageKv.Value -
                            nearest[0].VoltageKv.Value) <= 0.5
                            ? 10
                            : -5;
                }
                var evidence = nearest[0].Evidence
                    .Concat(new[]
                    {
                        "piso topológico: única subestación a 150 m del extremo geométrico"
                    })
                    .ToList();
                candidates[candidateIndex] = new RedElectricaGraphCandidate
                {
                    NodeId = nearest[0].NodeId,
                    Name = nearest[0].Name,
                    NameMatchKind = nearest[0].NameMatchKind,
                    Score = Math.Max(
                        nearest[0].Score,
                        Math.Clamp(topologyOnlyScore, 0, 100)),
                    DistanceKm = nearest[0].DistanceKm,
                    VoltageKv = nearest[0].VoltageKv,
                    Evidence = evidence
                };
            }
        }

        // Una subestación puede transformar de la RNT a un nivel menor de
        // distribución. Por ello, una diferencia de tensión no invalida un
        // extremo cuyo nombre es exacto, está a 150 m o menos y no compite
        // con otro nodo cercano. La tensión sigue desambiguando duplicados
        // próximos (por ejemplo, dos nodos homónimos de 115/400 kV).
        if (nearest.Count > 0 &&
            nearest[0].DistanceKm <= 0.15 &&
            (nearest.Count == 1 || nearest[1].DistanceKm >= 0.5) &&
            (string.Equals(
                 nearest[0].NameMatchKind,
                 literalExactKind,
                 StringComparison.Ordinal) ||
             string.Equals(
                 nearest[0].NameMatchKind,
                 canonicalExactKind,
                 StringComparison.Ordinal)) &&
            nearest[0].Score < _options.HighConfidenceThreshold)
        {
            var candidateIndex = candidates.FindIndex(candidate =>
                string.Equals(
                    candidate.NodeId,
                    nearest[0].NodeId,
                    StringComparison.Ordinal));
            if (candidateIndex >= 0)
            {
                var evidence = nearest[0].Evidence
                    .Concat(new[]
                    {
                        "piso transformador: nombre exacto y único a 150 m; la diferencia de tensión puede corresponder a transformación RNT-distribución"
                    })
                    .ToList();
                candidates[candidateIndex] =
                    new RedElectricaGraphCandidate
                    {
                        NodeId = nearest[0].NodeId,
                        Name = nearest[0].Name,
                        NameMatchKind = nearest[0].NameMatchKind,
                        Score = _options.HighConfidenceThreshold,
                        DistanceKm = nearest[0].DistanceKm,
                        VoltageKv = nearest[0].VoltageKv,
                        Evidence = evidence
                    };
            }
        }

        var ordered = candidates
            .OrderByDescending(candidate => candidate.Score)
            .ThenBy(candidate => candidate.DistanceKm)
            .Take(Math.Max(1, _options.MaximumCandidatesPerEndpoint))
            .ToList();
        if (ordered.Count == 0)
        {
            return new EndpointResolution(
                nominalName,
                null,
                0,
                "sin_resolver",
                "No se encontraron candidatos por nombre o proximidad.",
                ordered);
        }

        var top = ordered[0];
        var margin = ordered.Count == 1
            ? 100
            : top.Score - ordered[1].Score;
        if (top.Score >= _options.HighConfidenceThreshold &&
            margin >= _options.AmbiguityMargin)
        {
            return new EndpointResolution(
                nominalName,
                top.NodeId,
                top.Score,
                "alta",
                string.Empty,
                ordered);
        }

        var reason = top.Score < _options.ReviewThreshold
            ? $"Mejor coincidencia insuficiente ({top.Score}/100)."
            : margin < _options.AmbiguityMargin
                ? $"Candidatos demasiado cercanos en puntaje (margen {margin})."
                : $"Coincidencia por revisar ({top.Score}/100).";
        return new EndpointResolution(
            nominalName,
            null,
            top.Score,
            top.Score >= _options.ReviewThreshold ? "revision" : "sin_resolver",
            reason,
            ordered);
    }

    private RedElectricaGraphNode ResolveOrCreateVirtualNode(
        LineSeed line,
        string side,
        GeoPoint endpoint,
        EndpointResolution resolution,
        IDictionary<string, RedElectricaGraphNode> nodes,
        IList<RedElectricaGraphNode> virtualNodes)
    {
        if (!string.IsNullOrWhiteSpace(resolution.ResolvedNode) &&
            nodes.TryGetValue(resolution.ResolvedNode, out var resolved))
        {
            return resolved;
        }

        var existing = virtualNodes.FirstOrDefault(node =>
            (!line.VoltageKv.HasValue ||
             !node.VoltageKv.HasValue ||
             Math.Abs(line.VoltageKv.Value - node.VoltageKv.Value) <= 0.5) &&
            HaversineKm(
                endpoint,
                new GeoPoint(node.Latitude, node.Longitude)) <=
            _options.VirtualMergeToleranceKm);
        if (existing is not null)
        {
            return existing;
        }

        var nominal = string.IsNullOrWhiteSpace(resolution.NominalName)
            ? $"Extremo {side} · {line.Name}"
            : resolution.NominalName;
        var nodeId = StableKey(
            "NV",
            NormalizeText(nominal),
            line.VoltageKv,
            $"{endpoint.Longitude:0.######}|{endpoint.Latitude:0.######}");
        var virtualNode = new RedElectricaGraphNode
        {
            NodeId = nodeId,
            Type = "nodo_virtual",
            Name = nominal,
            NormalizedName = NormalizeText(nominal),
            Latitude = endpoint.Latitude,
            Longitude = endpoint.Longitude,
            VoltageKv = line.VoltageKv,
            NetworkLevel = RedElectricaNetworkClassifier.Classify(
                line.VoltageKv),
            Source = _options.LinesUrl,
            IsVirtual = true,
            VirtualReason = resolution.Reason,
            ValidationState = "pendiente_revision"
        };
        nodes[nodeId] = virtualNode;
        virtualNodes.Add(virtualNode);
        return virtualNode;
    }

    private static void AddReviewIfNeeded(
        ICollection<RedElectricaGraphReview> reviews,
        string edgeId,
        LineSeed line,
        string side,
        EndpointResolution resolution)
    {
        if (resolution.ResolvedNode is not null)
        {
            return;
        }

        var point = side == "A" ? line.Start : line.End;
        reviews.Add(new RedElectricaGraphReview
        {
            ReviewId = StableTextKey(
                "REV",
                $"{edgeId}|{side}|{resolution.NominalName}"),
            EdgeId = edgeId,
            LineName = line.Name,
            EndpointSide = side,
            NominalEndpoint = resolution.NominalName,
            Latitude = point.Latitude,
            Longitude = point.Longitude,
            Reason = resolution.Reason,
            Candidates = resolution.Candidates
        });
    }

    private static IReadOnlyDictionary<string, IReadOnlyList<RedElectricaGraphAdjacency>>
        BuildAdjacency(
            IDictionary<string, RedElectricaGraphNode> nodes,
            IReadOnlyDictionary<string, RedElectricaGraphEdge> edges)
    {
        var mutable = nodes.Keys.ToDictionary(
            key => key,
            _ => new List<RedElectricaGraphAdjacency>(),
            StringComparer.Ordinal);
        foreach (var edge in edges.Values)
        {
            var weight = edge.GeometryLengthKm > 0
                ? edge.GeometryLengthKm
                : edge.CatalogLengthKm ?? 0.001;
            mutable[edge.FromNodeId].Add(new RedElectricaGraphAdjacency
            {
                EdgeId = edge.EdgeId,
                NeighborNodeId = edge.ToNodeId,
                WeightKm = weight
            });
            mutable[edge.ToNodeId].Add(new RedElectricaGraphAdjacency
            {
                EdgeId = edge.EdgeId,
                NeighborNodeId = edge.FromNodeId,
                WeightKm = weight
            });
        }

        foreach (var node in nodes.Values)
        {
            node.Degree = mutable[node.NodeId].Count;
        }

        return mutable.ToDictionary(
            pair => pair.Key,
            pair => (IReadOnlyList<RedElectricaGraphAdjacency>)pair.Value,
            StringComparer.Ordinal);
    }

    private static void AssignComponents(
        IDictionary<string, RedElectricaGraphNode> nodes,
        IReadOnlyDictionary<string, IReadOnlyList<RedElectricaGraphAdjacency>> adjacency)
    {
        var visited = new HashSet<string>(StringComparer.Ordinal);
        var component = 0;
        foreach (var nodeId in nodes.Keys.OrderBy(value => value, StringComparer.Ordinal))
        {
            if (!visited.Add(nodeId))
            {
                continue;
            }

            component++;
            var componentId = $"cmp:{component:0000}";
            var queue = new Queue<string>();
            queue.Enqueue(nodeId);
            while (queue.Count > 0)
            {
                var current = queue.Dequeue();
                nodes[current].ComponentId = componentId;
                if (!adjacency.TryGetValue(current, out var adjacent))
                {
                    continue;
                }

                foreach (var connection in adjacent)
                {
                    if (visited.Add(connection.NeighborNodeId))
                    {
                        queue.Enqueue(connection.NeighborNodeId);
                    }
                }
            }
        }
    }

    private static RedElectricaGeoJsonFeature NodeFeature(
        RedElectricaGraphNode node) =>
        new()
        {
            Geometry = JsonSerializer.SerializeToElement(new
            {
                type = "Point",
                coordinates = new[] { node.Longitude, node.Latitude }
            }),
            Properties = new Dictionary<string, object?>
            {
                ["element_type"] = "node",
                ["node_id"] = node.NodeId,
                ["catalog_element_key"] = node.CatalogElementKey,
                ["node_type"] = node.Type,
                ["name"] = node.Name,
                ["voltage_kv"] = node.VoltageKv,
                ["network_level"] = node.NetworkLevel,
                ["network_level_label"] =
                    RedElectricaNetworkClassifier.Label(node.NetworkLevel),
                ["phase"] = node.Phase,
                ["degree"] = node.Degree,
                ["component_id"] = node.ComponentId,
                ["is_virtual"] = node.IsVirtual,
                ["virtual_reason"] = node.VirtualReason,
                ["source"] = node.Source,
                ["source_kind"] = node.SourceKind,
                ["validation_state"] = node.ValidationState
            }
        };

    private static RedElectricaGeoJsonFeature EdgeFeature(
        RedElectricaGraphEdge edge,
        IReadOnlyDictionary<string, RedElectricaGraphNode>? nodes = null)
    {
        RedElectricaGraphNode? fromNode = null;
        RedElectricaGraphNode? toNode = null;
        nodes?.TryGetValue(edge.FromNodeId, out fromNode);
        nodes?.TryGetValue(edge.ToNodeId, out toNode);

        return new RedElectricaGeoJsonFeature
        {
            Geometry = edge.Geometry,
            Properties = new Dictionary<string, object?>
            {
                ["element_type"] = "edge",
                ["edge_id"] = edge.EdgeId,
                ["catalog_element_key"] = edge.CatalogElementKey,
                ["name"] = edge.Name,
                ["from_id"] = edge.FromNodeId,
                ["to_id"] = edge.ToNodeId,
                ["from_name"] = fromNode?.Name ?? edge.NominalEndpointA,
                ["to_name"] = toNode?.Name ?? edge.NominalEndpointB,
                ["from_latitude"] = fromNode?.Latitude,
                ["from_longitude"] = fromNode?.Longitude,
                ["to_latitude"] = toNode?.Latitude,
                ["to_longitude"] = toNode?.Longitude,
                ["from_confidence"] = edge.FromConfidence,
                ["to_confidence"] = edge.ToConfidence,
                ["from_resolution"] = edge.FromResolution,
                ["to_resolution"] = edge.ToResolution,
                ["connection_state"] = edge.ConnectionState,
                ["voltage_kv"] = edge.VoltageKv,
                ["network_level"] = edge.NetworkLevel,
                ["network_level_label"] =
                    RedElectricaNetworkClassifier.Label(edge.NetworkLevel),
                ["circuits"] = edge.Circuits,
                ["length_km"] = edge.GeometryLengthKm,
                ["catalog_length_km"] = edge.CatalogLengthKm,
                ["segment_index"] = edge.SegmentIndex,
                ["source"] = edge.Source,
                ["source_kind"] = edge.SourceKind,
                ["validation_state"] = edge.ValidationState
            }
        };
    }

    private static RedElectricaGeoJson CreateGeoJson(
        RedElectricaGraphSnapshot snapshot,
        string layer,
        IReadOnlyList<RedElectricaGeoJsonFeature> features,
        int nodes,
        int edges) =>
        new()
        {
            Meta = new RedElectricaGeoJsonMeta
            {
                Version = snapshot.Summary.Version,
                Layer = layer,
                GeneratedUtc = snapshot.Summary.GeneradoUtc,
                Nodes = nodes,
                Edges = edges,
                Reviews = snapshot.Summary.RevisionesPendientes,
                Source = "DGMESNIE · grafo derivado de subestaciones y líneas de transmisión"
            },
            Features = features
        };

    private static string NormalizeConnectionState(string? value)
    {
        var normalized = NormalizeText(value ?? string.Empty)
            .ToLowerInvariant()
            .Replace(' ', '_');
        return normalized is "conectada" or "parcial" or "sin_resolver"
            ? normalized
            : string.Empty;
    }

    private static HashSet<string> ParseNetworkLevels(string? value)
    {
        var allowed = new HashSet<string>(
            new[]
            {
                RedElectricaNetworkLevels.Transmission,
                RedElectricaNetworkLevels.Subtransmission,
                RedElectricaNetworkLevels.Distribution,
                RedElectricaNetworkLevels.Undetermined
            },
            StringComparer.Ordinal);
        if (string.IsNullOrWhiteSpace(value))
        {
            return new HashSet<string>(StringComparer.Ordinal);
        }

        return value
            .Split(
                new[] { ',', ';', '|' },
                StringSplitOptions.RemoveEmptyEntries |
                StringSplitOptions.TrimEntries)
            .Select(item => NormalizeText(item).ToLowerInvariant().Replace(' ', '_'))
            .Where(allowed.Contains)
            .ToHashSet(StringComparer.Ordinal);
    }

    private static IReadOnlyList<JsonElement> GetFeatures(JsonElement root)
    {
        var features = GetProperty(root, "features");
        return features.ValueKind == JsonValueKind.Array
            ? features.EnumerateArray().Select(item => item.Clone()).ToList()
            : Array.Empty<JsonElement>();
    }

    private static JsonElement GetProperty(
        JsonElement element,
        params string[] names)
    {
        if (element.ValueKind != JsonValueKind.Object)
        {
            return default;
        }

        foreach (var property in element.EnumerateObject())
        {
            if (names.Any(name => string.Equals(
                property.Name,
                name,
                StringComparison.OrdinalIgnoreCase)))
            {
                return property.Value;
            }
        }

        return default;
    }

    private static string GetString(JsonElement element, params string[] names)
    {
        var value = GetProperty(element, names);
        return value.ValueKind switch
        {
            JsonValueKind.String => value.GetString() ?? string.Empty,
            JsonValueKind.Number => value.GetRawText(),
            _ => string.Empty
        };
    }

    private static double? GetDouble(JsonElement element, params string[] names)
    {
        var value = GetProperty(element, names);
        if (value.ValueKind == JsonValueKind.Number &&
            value.TryGetDouble(out var number))
        {
            return number;
        }
        if (value.ValueKind == JsonValueKind.String &&
            double.TryParse(
                value.GetString()?.Replace(',', '.'),
                NumberStyles.Float,
                CultureInfo.InvariantCulture,
                out number))
        {
            return number;
        }
        return null;
    }

    private static IReadOnlyList<JsonElement> ExtractLineSegments(
        JsonElement geometry)
    {
        var type = GetString(geometry, "type");
        var coordinates = GetProperty(geometry, "coordinates");
        if (string.Equals(type, "LineString", StringComparison.OrdinalIgnoreCase))
        {
            return new[] { geometry.Clone() };
        }
        if (!string.Equals(
                type,
                "MultiLineString",
                StringComparison.OrdinalIgnoreCase) ||
            coordinates.ValueKind != JsonValueKind.Array)
        {
            return Array.Empty<JsonElement>();
        }

        return coordinates
            .EnumerateArray()
            .Select(segment => JsonSerializer.SerializeToElement(new
            {
                type = "LineString",
                coordinates = segment.Clone()
            }))
            .ToList();
    }

    private static IReadOnlyList<GeoPoint> ExtractLinePoints(JsonElement geometry)
    {
        var coordinates = GetProperty(geometry, "coordinates");
        if (coordinates.ValueKind != JsonValueKind.Array)
        {
            return Array.Empty<GeoPoint>();
        }

        var output = new List<GeoPoint>();
        foreach (var coordinate in coordinates.EnumerateArray())
        {
            if (TryPoint(coordinate, out var point))
            {
                output.Add(point);
            }
        }
        return output;
    }

    private static bool TryPoint(JsonElement coordinate, out GeoPoint point)
    {
        point = default;
        if (coordinate.ValueKind != JsonValueKind.Array ||
            coordinate.GetArrayLength() < 2 ||
            !coordinate[0].TryGetDouble(out var longitude) ||
            !coordinate[1].TryGetDouble(out var latitude))
        {
            return false;
        }
        point = new GeoPoint(latitude, longitude);
        return true;
    }

    private static (string A, string B) ParseLineEndpoints(string name)
    {
        var withoutPrefix = LinePrefixRegex().Replace(name ?? string.Empty, string.Empty);
        var parts = LineSeparatorRegex()
            .Split(withoutPrefix)
            .Select(NormalizeSubstationName)
            .Where(value => !string.IsNullOrWhiteSpace(value))
            .ToArray();
        return parts.Length >= 2
            ? (parts[0], parts[^1])
            : (string.Empty, string.Empty);
    }

    private static string NormalizeSubstationName(string value) =>
        SubstationPrefixRegex().Replace(NormalizeText(value), string.Empty).Trim();

    private static string NormalizeLineName(string value) =>
        LinePrefixNormalizedRegex().Replace(NormalizeText(value), string.Empty).Trim();

    private static string NormalizeText(string value)
    {
        if (string.IsNullOrWhiteSpace(value))
        {
            return string.Empty;
        }

        var decomposed = value.Normalize(NormalizationForm.FormD);
        var output = new StringBuilder(decomposed.Length);
        foreach (var character in decomposed)
        {
            if (CharUnicodeInfo.GetUnicodeCategory(character) ==
                UnicodeCategory.NonSpacingMark)
            {
                continue;
            }
            output.Append(char.IsLetterOrDigit(character)
                ? char.ToUpperInvariant(character)
                : ' ');
        }
        return WhitespaceRegex().Replace(output.ToString(), " ").Trim();
    }

    private static int? ParseCircuits(string text)
    {
        var match = CircuitsRegex().Match(text ?? string.Empty);
        return match.Success &&
               int.TryParse(match.Groups[1].Value, out var value)
            ? value
            : null;
    }

    private static double? ParseLengthKm(string text)
    {
        var match = LengthRegex().Match(text ?? string.Empty);
        return match.Success &&
               double.TryParse(
                   match.Groups[1].Value.Replace(',', '.'),
                   NumberStyles.Float,
                   CultureInfo.InvariantCulture,
                   out var value)
            ? value
            : null;
    }

    private static string StableKey(
        string prefix,
        string normalizedName,
        double? voltage,
        string geometry) =>
        StableTextKey(
            prefix,
            $"{normalizedName}|{voltage:0.###}|{geometry}");

    private static string StableTextKey(string prefix, string value)
    {
        var hash = Sha256($"{prefix}|{value}");
        return $"{prefix.ToLowerInvariant()}:{hash[..20]}";
    }

    private static string Sha256(string value) =>
        Convert.ToHexString(
                SHA256.HashData(Encoding.UTF8.GetBytes(value)))
            .ToLowerInvariant();

    private static double HaversineKm(GeoPoint a, GeoPoint b)
    {
        const double radius = 6371.0088;
        var lat1 = a.Latitude * Math.PI / 180;
        var lat2 = b.Latitude * Math.PI / 180;
        var deltaLat = (b.Latitude - a.Latitude) * Math.PI / 180;
        var deltaLon = (b.Longitude - a.Longitude) * Math.PI / 180;
        var value =
            Math.Sin(deltaLat / 2) * Math.Sin(deltaLat / 2) +
            Math.Cos(lat1) * Math.Cos(lat2) *
            Math.Sin(deltaLon / 2) * Math.Sin(deltaLon / 2);
        return radius * 2 * Math.Atan2(Math.Sqrt(value), Math.Sqrt(1 - value));
    }

    private sealed record LineSeed(
        string EdgeId,
        string CatalogElementKey,
        string Name,
        string NormalizedName,
        string NominalEndpointA,
        string NominalEndpointB,
        double? VoltageKv,
        int? Circuits,
        double? CatalogLengthKm,
        double GeometryLengthKm,
        int SegmentIndex,
        GeoPoint Start,
        GeoPoint End,
        JsonElement Geometry);

    private sealed record EndpointResolution(
        string NominalName,
        string? ResolvedNode,
        int TopScore,
        string Resolution,
        string Reason,
        IReadOnlyList<RedElectricaGraphCandidate> Candidates);

    private sealed record OrientationResolution(
        EndpointResolution From,
        EndpointResolution To)
    {
        public int TotalScore => From.TopScore + To.TopScore;
    }

    private readonly record struct GeoPoint(double Latitude, double Longitude);

    [GeneratedRegex(@"\s+")]
    private static partial Regex WhitespaceRegex();

    [GeneratedRegex(
        @"^(?:SUBESTACION(?:\s+ELECTRICA)?|S\s*E|SE)\s+",
        RegexOptions.IgnoreCase)]
    private static partial Regex SubstationPrefixRegex();

    [GeneratedRegex(
        @"^(?:LINEA(?:\s+DE\s+TRANSMISION)?|L\s*T|LT)\s+",
        RegexOptions.IgnoreCase)]
    private static partial Regex LinePrefixNormalizedRegex();

    [GeneratedRegex(
        @"^\s*(?:L\s*\.\s*T\s*\.?|LT|LINEA(?:\s+DE\s+TRANSMISION)?)\s*",
        RegexOptions.IgnoreCase)]
    private static partial Regex LinePrefixRegex();

    [GeneratedRegex(@"\s*[-–—]\s*")]
    private static partial Regex LineSeparatorRegex();

    [GeneratedRegex(@"(?<!\d)(\d+)\s*C(?:\b|-)", RegexOptions.IgnoreCase)]
    private static partial Regex CircuitsRegex();

    [GeneratedRegex(@"(\d+(?:[.,]\d+)?)\s*KM", RegexOptions.IgnoreCase)]
    private static partial Regex LengthRegex();
}
