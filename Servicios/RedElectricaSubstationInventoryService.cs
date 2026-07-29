using System.Data;
using System.Globalization;
using System.Security.Cryptography;
using System.Text;
using System.Text.Json;
using Microsoft.Data.SqlClient;
using NSIE.Models;

namespace NSIE.Servicios;

public interface IRedElectricaSubstationInventoryService
{
    Task<RedElectricaSubstationInventorySummary> SynchronizeAsync(
        CancellationToken cancellationToken = default);

    Task<RedElectricaSubstationInventorySummary> GetSummaryAsync(
        CancellationToken cancellationToken = default);

    Task<IReadOnlyList<RedElectricaSubstationInventoryRecord>> GetRecordsAsync(
        string? source = null,
        string? reconciliationState = null,
        int limit = 500,
        CancellationToken cancellationToken = default);

    Task<RedElectricaGeoJson> GetGeoJsonAsync(
        string? networkLevels = null,
        string? source = null,
        CancellationToken cancellationToken = default);

    Task<RedElectricaSubstationPromotionSummary>
        PromoteHighConfidenceAsync(
            long executionId,
            CancellationToken cancellationToken = default);
}

public sealed class RedElectricaSubstationInventoryService :
    IRedElectricaSubstationInventoryService
{
    private const double SpatialMatchToleranceKm = 0.25;
    private static readonly SemaphoreSlim SyncLock = new(1, 1);

    private readonly IRedElectricaGraphService _graphService;
    private readonly IAtlasSenReferenceService _atlasService;
    private readonly ILogger<RedElectricaSubstationInventoryService> _logger;
    private readonly string _connectionString;

    public RedElectricaSubstationInventoryService(
        IRedElectricaGraphService graphService,
        IAtlasSenReferenceService atlasService,
        IConfiguration configuration,
        ILogger<RedElectricaSubstationInventoryService> logger)
    {
        _graphService = graphService;
        _atlasService = atlasService;
        _connectionString =
            configuration.GetConnectionString("DefaultConnection") ??
            string.Empty;
        _logger = logger;
    }

    public async Task<RedElectricaSubstationInventorySummary>
        SynchronizeAsync(CancellationToken cancellationToken = default)
    {
        EnsureConnectionConfigured();
        await SyncLock.WaitAsync(cancellationToken);
        try
        {
            var graph = await _graphService.GetAsync(
                false,
                cancellationToken);
            var graphVersionId = graph.Summary.VersionIdBaseDatos ??
                throw new InvalidOperationException(
                    "El grafo activo debe estar persistido antes de sincronizar el inventario.");
            var graphNodes = graph.Nodes.Values
                .Where(node =>
                    !node.IsVirtual &&
                    string.Equals(
                        node.Type,
                        "subestacion",
                        StringComparison.OrdinalIgnoreCase))
                .ToList();
            var virtualNodes = graph.Nodes.Values
                .Where(node => node.IsVirtual)
                .ToList();
            var references = await _atlasService.GetSubstationsAsync(
                limit: 5000,
                cancellationToken: cancellationToken);
            var generatedUtc = DateTime.UtcNow;
            var rows = BuildRows(
                graphVersionId,
                graphNodes,
                virtualNodes,
                references,
                generatedUtc);

            await PersistAsync(
                graphVersionId,
                rows,
                generatedUtc,
                cancellationToken);
            var summary = await GetSummaryAsync(cancellationToken);
            _logger.LogInformation(
                "Inventario de subestaciones sincronizado: {SourceRecords} registros fuente, {UniverseCandidates} candidatos de universo, {GeoreferencedUniverse} georreferenciados y {PendingGeoreferencing} pendientes de coordenadas.",
                summary.SourceRecords,
                summary.UniverseCandidates,
                summary.GeoreferencedUniverse,
                summary.PendingGeoreferencing);
            return summary;
        }
        finally
        {
            SyncLock.Release();
        }
    }

    public async Task<RedElectricaSubstationInventorySummary> GetSummaryAsync(
        CancellationToken cancellationToken = default)
    {
        EnsureConnectionConfigured();
        await using var connection = new SqlConnection(_connectionString);
        await connection.OpenAsync(cancellationToken);
        await using var command = new SqlCommand(
            """
            SELECT
                MAX(UltimaObservacionUtc),
                MAX(VersionIdCanonica),
                COUNT_BIG(*),
                COUNT(DISTINCT UniversoClave),
                COUNT(DISTINCT CASE
                    WHEN Latitud IS NOT NULL AND Longitud IS NOT NULL
                    THEN UniversoClave END),
                COUNT(DISTINCT UniversoClave) -
                    COUNT(DISTINCT CASE
                        WHEN Latitud IS NOT NULL AND Longitud IS NOT NULL
                        THEN UniversoClave END),
                SUM(CASE WHEN FuenteClave = N'dgmesnie_geojson'
                    THEN 1 ELSE 0 END),
                SUM(CASE WHEN FuenteClave = N'atlas_sen'
                    THEN 1 ELSE 0 END),
                SUM(CASE WHEN FuenteClave = N'openstreetmap'
                    THEN 1 ELSE 0 END),
                SUM(CASE WHEN NivelRed = N'transmision'
                    THEN 1 ELSE 0 END),
                SUM(CASE WHEN NivelRed = N'subtransmision'
                    THEN 1 ELSE 0 END),
                SUM(CASE WHEN NivelRed = N'distribucion'
                    THEN 1 ELSE 0 END),
                SUM(CASE WHEN NodoCanonicoClave IS NOT NULL
                    THEN 1 ELSE 0 END),
                SUM(CASE
                    WHEN FuenteClave <> N'dgmesnie_geojson'
                     AND NodoCanonicoClave IS NULL
                    THEN 1 ELSE 0 END)
            FROM dgmesnie.RedElectricaSubestacionInventario
            WHERE Activa = 1;
            """,
            connection);
        await using var reader = await command.ExecuteReaderAsync(
            cancellationToken);
        if (!await reader.ReadAsync(cancellationToken) ||
            reader.IsDBNull(0))
        {
            return new RedElectricaSubstationInventorySummary();
        }

        return new RedElectricaSubstationInventorySummary
        {
            GeneratedUtc = reader.GetDateTime(0),
            GraphVersionId = reader.IsDBNull(1)
                ? null
                : reader.GetInt64(1),
            SourceRecords = Convert.ToInt32(reader.GetInt64(2)),
            UniverseCandidates = reader.GetInt32(3),
            GeoreferencedUniverse = reader.GetInt32(4),
            PendingGeoreferencing = reader.GetInt32(5),
            DgmesnieRecords = reader.GetInt32(6),
            AtlasSenRecords = reader.GetInt32(7),
            OpenStreetMapRecords = reader.GetInt32(8),
            TransmissionRecords = reader.GetInt32(9),
            SubtransmissionRecords = reader.GetInt32(10),
            DistributionRecords = reader.GetInt32(11),
            ReconciledRecords = reader.GetInt32(12),
            PendingReconciliation = reader.GetInt32(13)
        };
    }

    public async Task<IReadOnlyList<
        RedElectricaSubstationInventoryRecord>> GetRecordsAsync(
        string? source = null,
        string? reconciliationState = null,
        int limit = 500,
        CancellationToken cancellationToken = default)
    {
        EnsureConnectionConfigured();
        await using var connection = new SqlConnection(_connectionString);
        await connection.OpenAsync(cancellationToken);
        await using var command = new SqlCommand(
            """
            SELECT TOP (@Limit)
                RegistroClave,
                UniversoClave,
                FuenteClave,
                ReferenciaClave,
                VersionIdCanonica,
                NodoCanonicoClave,
                Nombre,
                NombreNormalizado,
                TensionKv,
                NivelRed,
                Latitud,
                Longitud,
                Region,
                Zona,
                DivisionTarifaria,
                Operador,
                Fuente,
                FuenteCoordenadas,
                Licencia,
                EstadoConciliacion,
                EstadoValidacion,
                MetadatosJson,
                PrimeraObservacionUtc,
                UltimaObservacionUtc
            FROM dgmesnie.RedElectricaSubestacionInventario
            WHERE Activa = 1
              AND (@Fuente = N'' OR FuenteClave = @Fuente)
              AND (@Estado = N'' OR EstadoConciliacion = @Estado)
            ORDER BY
                CASE FuenteClave
                    WHEN N'dgmesnie_geojson' THEN 0
                    WHEN N'atlas_sen' THEN 1
                    ELSE 2
                END,
                Nombre,
                RegistroClave;
            """,
            connection);
        command.Parameters.AddWithValue("@Limit", limit);
        command.Parameters.AddWithValue(
            "@Fuente",
            (source ?? string.Empty).Trim().ToLowerInvariant());
        command.Parameters.AddWithValue(
            "@Estado",
            (reconciliationState ?? string.Empty)
                .Trim()
                .ToLowerInvariant());

        var output = new List<RedElectricaSubstationInventoryRecord>();
        await using var reader = await command.ExecuteReaderAsync(
            cancellationToken);
        while (await reader.ReadAsync(cancellationToken))
        {
            using var metadata = JsonDocument.Parse(reader.GetString(21));
            output.Add(new RedElectricaSubstationInventoryRecord
            {
                RecordKey = reader.GetString(0),
                UniverseKey = reader.GetString(1),
                SourceKey = reader.GetString(2),
                ReferenceKey = reader.GetString(3),
                GraphVersionId = reader.IsDBNull(4)
                    ? null
                    : reader.GetInt64(4),
                CanonicalNodeKey = ReadString(reader, 5),
                Name = reader.GetString(6),
                NormalizedName = reader.GetString(7),
                VoltageKv = ReadDouble(reader, 8),
                NetworkLevel = reader.GetString(9),
                Latitude = ReadDouble(reader, 10),
                Longitude = ReadDouble(reader, 11),
                Region = ReadString(reader, 12),
                Zone = ReadString(reader, 13),
                TariffDivision = ReadString(reader, 14),
                Operator = ReadString(reader, 15),
                Source = reader.GetString(16),
                CoordinateSourceKey = ReadString(reader, 17),
                License = ReadString(reader, 18),
                ReconciliationState = reader.GetString(19),
                ValidationState = reader.GetString(20),
                Metadata = metadata.RootElement.Clone(),
                FirstSeenUtc = reader.GetDateTime(22),
                LastSeenUtc = reader.GetDateTime(23)
            });
        }
        return output;
    }

    public async Task<RedElectricaGeoJson> GetGeoJsonAsync(
        string? networkLevels = null,
        string? source = null,
        CancellationToken cancellationToken = default)
    {
        var records = await LoadActiveRecordsAsync(
            cancellationToken);
        var lineAssociations = await LoadActiveLineAssociationsAsync(
            cancellationToken);
        var associationsByRecord = lineAssociations
            .GroupBy(
                association => association.RecordKey,
                StringComparer.Ordinal)
            .ToDictionary(
                group => group.Key,
                group => group.ToList(),
                StringComparer.Ordinal);
        var levels = ParseFilter(networkLevels);
        var normalizedSource = (source ?? string.Empty)
            .Trim()
            .ToLowerInvariant();
        var groups = records
            .GroupBy(
                record => record.UniverseKey,
                StringComparer.Ordinal)
            .Where(group => group.Any(record =>
                record.Latitude.HasValue &&
                record.Longitude.HasValue))
            .Where(group =>
                levels.Count == 0 ||
                levels.Contains(ResolveNetworkLevel(group)))
            .Where(group =>
                string.IsNullOrWhiteSpace(normalizedSource) ||
                group.Any(record => record.SourceKey.Equals(
                    normalizedSource,
                    StringComparison.OrdinalIgnoreCase)))
            .ToList();
        var features = new List<RedElectricaGeoJsonFeature>(
            groups.Count);
        foreach (var group in groups)
        {
            var representative = group
                .Where(record =>
                    record.Latitude.HasValue &&
                    record.Longitude.HasValue)
                .OrderBy(SourcePriority)
                .ThenBy(record => record.RecordKey, StringComparer.Ordinal)
                .First();
            var sources = group
                .Select(record => record.SourceKey)
                .Distinct(StringComparer.OrdinalIgnoreCase)
                .OrderBy(value => value, StringComparer.Ordinal)
                .ToArray();
            var voltageKv = group
                .Where(record => record.VoltageKv.HasValue)
                .Select(record => record.VoltageKv!.Value)
                .DefaultIfEmpty(representative.VoltageKv ?? 0)
                .Max();
            var networkLevel = ResolveNetworkLevel(group);
            var associations = group
                .SelectMany(record =>
                    associationsByRecord.TryGetValue(
                        record.RecordKey,
                        out var recordAssociations)
                        ? recordAssociations
                        : [])
                .DistinctBy(
                    association => association.CatalogElementKey,
                    StringComparer.Ordinal)
                .OrderBy(association => association.DistanceKm)
                .ThenBy(
                    association => association.LineName,
                    StringComparer.OrdinalIgnoreCase)
                .ToList();
            var geometry = JsonSerializer.SerializeToElement(new
            {
                type = "Point",
                coordinates = new[]
                {
                    representative.Longitude!.Value,
                    representative.Latitude!.Value
                }
            });
            features.Add(new RedElectricaGeoJsonFeature
            {
                Geometry = geometry,
                Properties = new Dictionary<string, object?>
                {
                    ["element_type"] = "node",
                    ["inventory_kind"] = "substation_inventory",
                    ["record_key"] = representative.RecordKey,
                    ["universe_key"] = group.Key,
                    ["node_id"] = representative.CanonicalNodeKey,
                    ["name"] = representative.Name,
                    ["voltage_kv"] = voltageKv > 0
                        ? voltageKv
                        : null,
                    ["network_level"] = networkLevel,
                    ["source"] = string.Join(" + ", sources),
                    ["sources"] = sources,
                    ["source_count"] = sources.Length,
                    ["coordinate_source"] =
                        representative.CoordinateSourceKey,
                    ["reconciliation_state"] =
                        representative.ReconciliationState,
                    ["validation_state"] =
                        representative.ValidationState,
                    ["connection_count"] = associations.Count,
                    ["connection_state"] = associations.Count > 0
                        ? "asociacion_automatica_fuente_abierta"
                        : "sin_asociacion_topologica",
                    ["connection_validation_state"] =
                        associations.FirstOrDefault()?.ValidationState ??
                        string.Empty,
                    ["connected_line_ids"] = associations
                        .Select(association => association.EdgeKey)
                        .ToArray(),
                    ["connected_line_catalog_keys"] = associations
                        .Select(
                            association =>
                                association.CatalogElementKey)
                        .ToArray(),
                    ["connected_line_names"] = associations
                        .Select(association => association.LineName)
                        .ToArray(),
                    ["connection_max_distance_km"] =
                        associations.Count > 0
                            ? associations.Max(
                                association => association.DistanceKm)
                            : null
                }
            });
        }

        return new RedElectricaGeoJson
        {
            Meta = new RedElectricaGeoJsonMeta
            {
                Version = $"inventario:{DateTime.UtcNow:yyyyMMddHHmmss}",
                Layer = "subestaciones_inventario",
                GeneratedUtc = DateTime.UtcNow,
                Nodes = features.Count,
                Source =
                    "Inventario conciliado DGMESNIE GeoJSON + Atlas SEN + OpenStreetMap"
            },
            Features = features
        };
    }

    public async Task<RedElectricaSubstationPromotionSummary>
        PromoteHighConfidenceAsync(
            long executionId,
            CancellationToken cancellationToken = default)
    {
        EnsureConnectionConfigured();
        if (executionId <= 0)
        {
            throw new ArgumentOutOfRangeException(
                nameof(executionId),
                "La ejecución debe ser mayor que cero.");
        }

        var promotedUtc = DateTime.UtcNow;
        await using var connection = new SqlConnection(_connectionString);
        await connection.OpenAsync(cancellationToken);
        await using var transaction = (SqlTransaction)
            await connection.BeginTransactionAsync(cancellationToken);
        int eligible;
        try
        {
            await using (var promote = new SqlCommand(
                """
                DECLARE @Elegibles TABLE
                (
                    RegistroClave NVARCHAR(80) NOT NULL,
                    ReferenciaOsm NVARCHAR(80) NOT NULL,
                    Latitud DECIMAL(10,7) NOT NULL,
                    Longitud DECIMAL(11,7) NOT NULL,
                    Puntaje DECIMAL(5,2) NOT NULL
                );

                INSERT INTO @Elegibles
                (
                    RegistroClave,
                    ReferenciaOsm,
                    Latitud,
                    Longitud,
                    Puntaje
                )
                SELECT
                    candidato.RegistroClave,
                    candidato.ReferenciaOsm,
                    candidato.Latitud,
                    candidato.Longitud,
                    candidato.Puntaje
                FROM
                    dgmesnie.RedElectricaSubestacionGeorefCandidato
                        AS candidato
                WHERE candidato.EjecucionId = @EjecucionId
                  AND candidato.EstadoPropuesta =
                        N'propuesta_alta_confianza'
                  AND candidato.EsPrincipal = 1
                  AND candidato.Puntaje >= 90
                  AND JSON_VALUE(
                        candidato.EvidenciasJson,
                        '$.nameRule') =
                        N'nombre_normalizado_exacto'
                  AND JSON_VALUE(
                        candidato.EvidenciasJson,
                        '$.voltageMatch') = N'true'
                  AND JSON_VALUE(
                        candidato.EvidenciasJson,
                        '$.regionMatch') = N'true';

                MERGE
                    dgmesnie.RedElectricaSubestacionGeorefPromocion
                        AS destino
                USING @Elegibles AS fuente
                  ON fuente.RegistroClave = destino.RegistroClave
                WHEN MATCHED THEN UPDATE SET
                    EjecucionId = @EjecucionId,
                    ReferenciaOsm = fuente.ReferenciaOsm,
                    Latitud = fuente.Latitud,
                    Longitud = fuente.Longitud,
                    Puntaje = fuente.Puntaje,
                    FuenteCoordenadas = N'openstreetmap',
                    LicenciaCoordenadas = N'ODbL 1.0',
                    EstadoValidacion =
                        N'validada_automatica_fuente_abierta',
                    ReglaPromocion =
                        N'DGMESNIE-SUBSTATION-GEOREF-PROMOTION-v1',
                    EsOficial = 0,
                    Activa = 1,
                    PromovidaUtc = @PromovidaUtc,
                    UltimaAplicacionUtc = @PromovidaUtc
                WHEN NOT MATCHED BY TARGET THEN
                    INSERT
                    (
                        RegistroClave,
                        EjecucionId,
                        ReferenciaOsm,
                        Latitud,
                        Longitud,
                        Puntaje,
                        FuenteCoordenadas,
                        LicenciaCoordenadas,
                        EstadoValidacion,
                        ReglaPromocion,
                        EsOficial,
                        Activa,
                        PromovidaUtc,
                        UltimaAplicacionUtc
                    )
                    VALUES
                    (
                        fuente.RegistroClave,
                        @EjecucionId,
                        fuente.ReferenciaOsm,
                        fuente.Latitud,
                        fuente.Longitud,
                        fuente.Puntaje,
                        N'openstreetmap',
                        N'ODbL 1.0',
                        N'validada_automatica_fuente_abierta',
                        N'DGMESNIE-SUBSTATION-GEOREF-PROMOTION-v1',
                        0,
                        1,
                        @PromovidaUtc,
                        @PromovidaUtc
                    );

                SELECT COUNT(*) FROM @Elegibles;
                """,
                connection,
                transaction))
            {
                promote.Parameters.AddWithValue(
                    "@EjecucionId",
                    executionId);
                promote.Parameters.AddWithValue(
                    "@PromovidaUtc",
                    promotedUtc);
                eligible = Convert.ToInt32(
                    await promote.ExecuteScalarAsync(cancellationToken));
            }

            if (eligible == 0)
            {
                throw new InvalidOperationException(
                    "La ejecución no contiene candidatas de alta confianza que cumplan nombre, tensión y gerencia.");
            }

            var applied = await ApplyActivePromotionsAsync(
                connection,
                transaction,
                promotedUtc,
                cancellationToken);
            if (applied < eligible)
            {
                throw new InvalidOperationException(
                    "No fue posible aplicar todas las promociones al inventario activo.");
            }

            await using (var markExecution = new SqlCommand(
                """
                UPDATE
                    dgmesnie.RedElectricaSubestacionGeorefEjecucion
                SET InventarioModificado = 1
                WHERE EjecucionId = @EjecucionId;
                """,
                connection,
                transaction))
            {
                markExecution.Parameters.AddWithValue(
                    "@EjecucionId",
                    executionId);
                if (await markExecution.ExecuteNonQueryAsync(
                        cancellationToken) != 1)
                {
                    throw new InvalidOperationException(
                        "La ejecución de georreferenciación no existe.");
                }
            }

            await transaction.CommitAsync(cancellationToken);
        }
        catch
        {
            await transaction.RollbackAsync(cancellationToken);
            throw;
        }

        var inventory = await GetSummaryAsync(cancellationToken);
        _logger.LogInformation(
            "Promovidas {Count} coordenadas OSM de alta confianza desde la ejecución {ExecutionId}; se mantienen como fuente abierta no oficial.",
            eligible,
            executionId);
        return new RedElectricaSubstationPromotionSummary
        {
            ExecutionId = executionId,
            PromotedUtc = promotedUtc,
            HighConfidenceCandidates = eligible,
            PromotionsApplied = eligible,
            OfficialCoordinates = false,
            Inventory = inventory
        };
    }

    private static IReadOnlyList<InventorySeed> BuildRows(
        long graphVersionId,
        IReadOnlyList<RedElectricaGraphNode> graphNodes,
        IReadOnlyList<RedElectricaGraphNode> virtualNodes,
        IReadOnlyList<AtlasSenSubstationReference> references,
        DateTime generatedUtc)
    {
        var rows = new List<InventorySeed>(
            graphNodes.Count + references.Count);
        var byName = graphNodes
            .GroupBy(
                node =>
                    RedElectricaEndpointNameMatcher.NormalizeAlias(
                        node.Name),
                StringComparer.Ordinal)
            .ToDictionary(
                group => group.Key,
                group => group.ToList(),
                StringComparer.Ordinal);
        var virtualByName = virtualNodes
            .GroupBy(
                node =>
                    RedElectricaEndpointNameMatcher.NormalizeAlias(
                        node.Name),
                StringComparer.Ordinal)
            .ToDictionary(
                group => group.Key,
                group => group.ToList(),
                StringComparer.Ordinal);

        foreach (var node in graphNodes)
        {
            rows.Add(new InventorySeed
            {
                RecordKey =
                    $"{RedElectricaSubstationSources.DgmesnieGeoJson}:{node.NodeId}",
                UniverseKey = node.NodeId,
                SourceKey =
                    RedElectricaSubstationSources.DgmesnieGeoJson,
                ReferenceKey = node.NodeId,
                GraphVersionId = graphVersionId,
                CanonicalNodeKey = node.NodeId,
                Name = node.Name,
                NormalizedName = node.NormalizedName,
                VoltageKv = node.VoltageKv,
                NetworkLevel = node.NetworkLevel,
                Latitude = node.Latitude,
                Longitude = node.Longitude,
                Source = node.Source,
                CoordinateSourceKey =
                    RedElectricaSubstationSources.DgmesnieGeoJson,
                ReconciliationState =
                    RedElectricaSubstationReconciliationStates
                        .CanonicalDgmesnie,
                ValidationState = node.ValidationState,
                MetadataJson = JsonSerializer.Serialize(new
                {
                    node.NodeId,
                    node.Phase,
                    node.Degree,
                    node.ComponentId
                }),
                GeneratedUtc = generatedUtc
            });
        }

        foreach (var reference in references)
        {
            var sourceKey = reference.SourceDataset ==
                AtlasSenDatasetKeys.Atlas
                ? RedElectricaSubstationSources.AtlasSen
                : RedElectricaSubstationSources.OpenStreetMap;
            var recordKey = StableRecordKey(sourceKey, reference);
            var candidates = byName.TryGetValue(
                reference.NormalizedName,
                out var named)
                ? named
                : [];
            var voltageMatches = candidates
                .Where(node =>
                    VoltageCompatible(
                        reference.VoltageKv,
                        node.VoltageKv))
                .ToList();
            RedElectricaGraphNode? canonical = null;
            RedElectricaGraphNode? coordinateNode = null;
            string reconciliationState;
            if (voltageMatches.Count == 1)
            {
                canonical = voltageMatches[0];
                coordinateNode = canonical;
                reconciliationState =
                    RedElectricaSubstationReconciliationStates
                        .NameAndVoltageMatch;
            }
            else if (voltageMatches.Count > 1)
            {
                reconciliationState =
                    RedElectricaSubstationReconciliationStates
                        .AmbiguousMatch;
            }
            else if (candidates.Count > 0)
            {
                if (candidates.Count == 1)
                {
                    canonical = candidates[0];
                    coordinateNode = canonical;
                    reconciliationState =
                        RedElectricaSubstationReconciliationStates
                            .NameVoltageConflict;
                }
                else
                {
                    reconciliationState =
                        RedElectricaSubstationReconciliationStates
                            .AmbiguousMatch;
                }
            }
            else
            {
                var spatialCandidates = FindSpatialCandidates(
                    reference,
                    graphNodes);
                if (spatialCandidates.Count == 1)
                {
                    canonical = spatialCandidates[0];
                    coordinateNode = canonical;
                    reconciliationState =
                        RedElectricaSubstationReconciliationStates
                            .SuggestedSpatialMatch;
                }
                else if (spatialCandidates.Count > 1)
                {
                    reconciliationState =
                        RedElectricaSubstationReconciliationStates
                            .AmbiguousMatch;
                }
                else
                {
                    var virtualCandidates =
                        virtualByName.TryGetValue(
                            reference.NormalizedName,
                            out var virtualNamed)
                            ? virtualNamed
                            : [];
                    if (!reference.Latitude.HasValue &&
                        virtualCandidates.Count == 1)
                    {
                        coordinateNode = virtualCandidates[0];
                        reconciliationState =
                            RedElectricaSubstationReconciliationStates
                                .SuggestedSpatialMatch;
                    }
                    else
                    {
                        reconciliationState =
                            reference.Latitude.HasValue &&
                            reference.Longitude.HasValue
                                ? RedElectricaSubstationReconciliationStates
                                    .PendingWithCoordinates
                                : RedElectricaSubstationReconciliationStates
                                    .PendingGeoreferencing;
                    }
                }
            }

            var latitude = reference.Latitude ??
                coordinateNode?.Latitude;
            var longitude = reference.Longitude ??
                coordinateNode?.Longitude;
            var universeKey = canonical?.NodeId ??
                (coordinateNode?.IsVirtual == true
                    ? $"endpoint:{coordinateNode.NodeId}"
                    : $"ref:{recordKey}");
            var coordinateSourceKey =
                reference.Latitude.HasValue &&
                reference.Longitude.HasValue
                    ? sourceKey
                    : canonical is not null
                        ? RedElectricaSubstationSources.DgmesnieGeoJson
                        : coordinateNode?.IsVirtual == true
                            ? RedElectricaSubstationSources
                                .DgmesnieLineEndpoint
                            : string.Empty;
            rows.Add(new InventorySeed
            {
                RecordKey = recordKey,
                UniverseKey = universeKey,
                SourceKey = sourceKey,
                ReferenceKey = reference.ReferenceId,
                GraphVersionId = canonical is null
                    ? null
                    : graphVersionId,
                CanonicalNodeKey = canonical?.NodeId ?? string.Empty,
                Name = reference.Name,
                NormalizedName = reference.NormalizedName,
                VoltageKv = reference.VoltageKv,
                NetworkLevel = reference.NetworkLevel,
                Latitude = latitude,
                Longitude = longitude,
                Region = reference.Region,
                Zone = reference.Zone,
                TariffDivision = reference.TariffDivision,
                Operator = reference.Operator,
                Source = reference.Source,
                CoordinateSourceKey = coordinateSourceKey,
                License = reference.License,
                ReconciliationState = reconciliationState,
                ValidationState = reference.ValidationState,
                MetadataJson = JsonSerializer.Serialize(new
                {
                    reference.TransformerMva,
                    reference.Saturation,
                    reference.VoltageRaw,
                    coordinateReferenceKey = coordinateNode?.NodeId
                }),
                GeneratedUtc = generatedUtc
            });
        }
        return rows;
    }

    private async Task PersistAsync(
        long graphVersionId,
        IReadOnlyList<InventorySeed> rows,
        DateTime generatedUtc,
        CancellationToken cancellationToken)
    {
        await using var connection = new SqlConnection(_connectionString);
        await connection.OpenAsync(cancellationToken);
        await using var transaction = (SqlTransaction)
            await connection.BeginTransactionAsync(cancellationToken);
        try
        {
            await using (var createStage = new SqlCommand(
                """
                CREATE TABLE #SubestacionInventarioStage
                (
                    RegistroClave NVARCHAR(80) NOT NULL,
                    UniversoClave NVARCHAR(80) NOT NULL,
                    FuenteClave NVARCHAR(40) NOT NULL,
                    ReferenciaClave NVARCHAR(160) NOT NULL,
                    VersionIdCanonica BIGINT NULL,
                    NodoCanonicoClave NVARCHAR(64) NULL,
                    Nombre NVARCHAR(500) NOT NULL,
                    NombreNormalizado NVARCHAR(500) NOT NULL,
                    TensionKv DECIMAL(9,3) NULL,
                    NivelRed NVARCHAR(30) NOT NULL,
                    Latitud DECIMAL(10,7) NULL,
                    Longitud DECIMAL(11,7) NULL,
                    Region NVARCHAR(150) NULL,
                    Zona NVARCHAR(200) NULL,
                    DivisionTarifaria NVARCHAR(150) NULL,
                    Operador NVARCHAR(200) NULL,
                    Fuente NVARCHAR(1000) NOT NULL,
                    FuenteCoordenadas NVARCHAR(40) NULL,
                    Licencia NVARCHAR(100) NULL,
                    EstadoConciliacion NVARCHAR(60) NOT NULL,
                    EstadoValidacion NVARCHAR(80) NOT NULL,
                    MetadatosJson NVARCHAR(MAX) NOT NULL,
                    UltimaObservacionUtc DATETIME2(3) NOT NULL
                );
                """,
                connection,
                transaction))
            {
                await createStage.ExecuteNonQueryAsync(cancellationToken);
            }

            using (var bulk = new SqlBulkCopy(
                connection,
                SqlBulkCopyOptions.CheckConstraints,
                transaction))
            {
                bulk.DestinationTableName =
                    "#SubestacionInventarioStage";
                bulk.BatchSize = 1000;
                bulk.BulkCopyTimeout = 120;
                await bulk.WriteToServerAsync(
                    CreateStageTable(rows),
                    cancellationToken);
            }

            await using (var merge = new SqlCommand(
                """
                MERGE dgmesnie.RedElectricaSubestacionInventario AS destino
                USING #SubestacionInventarioStage AS fuente
                  ON fuente.RegistroClave = destino.RegistroClave
                WHEN MATCHED THEN UPDATE SET
                    UniversoClave = fuente.UniversoClave,
                    FuenteClave = fuente.FuenteClave,
                    ReferenciaClave = fuente.ReferenciaClave,
                    VersionIdCanonica = fuente.VersionIdCanonica,
                    NodoCanonicoClave = fuente.NodoCanonicoClave,
                    Nombre = fuente.Nombre,
                    NombreNormalizado = fuente.NombreNormalizado,
                    TensionKv = fuente.TensionKv,
                    NivelRed = fuente.NivelRed,
                    Latitud = fuente.Latitud,
                    Longitud = fuente.Longitud,
                    Region = fuente.Region,
                    Zona = fuente.Zona,
                    DivisionTarifaria = fuente.DivisionTarifaria,
                    Operador = fuente.Operador,
                    Fuente = fuente.Fuente,
                    FuenteCoordenadas = fuente.FuenteCoordenadas,
                    Licencia = fuente.Licencia,
                    EstadoConciliacion = fuente.EstadoConciliacion,
                    EstadoValidacion = fuente.EstadoValidacion,
                    MetadatosJson = fuente.MetadatosJson,
                    Activa = 1,
                    UltimaObservacionUtc = fuente.UltimaObservacionUtc
                WHEN NOT MATCHED BY TARGET THEN
                    INSERT
                    (
                        RegistroClave,
                        UniversoClave,
                        FuenteClave,
                        ReferenciaClave,
                        VersionIdCanonica,
                        NodoCanonicoClave,
                        Nombre,
                        NombreNormalizado,
                        TensionKv,
                        NivelRed,
                        Latitud,
                        Longitud,
                        Region,
                        Zona,
                        DivisionTarifaria,
                        Operador,
                        Fuente,
                        FuenteCoordenadas,
                        Licencia,
                        EstadoConciliacion,
                        EstadoValidacion,
                        MetadatosJson,
                        Activa,
                        PrimeraObservacionUtc,
                        UltimaObservacionUtc
                    )
                    VALUES
                    (
                        fuente.RegistroClave,
                        fuente.UniversoClave,
                        fuente.FuenteClave,
                        fuente.ReferenciaClave,
                        fuente.VersionIdCanonica,
                        fuente.NodoCanonicoClave,
                        fuente.Nombre,
                        fuente.NombreNormalizado,
                        fuente.TensionKv,
                        fuente.NivelRed,
                        fuente.Latitud,
                        fuente.Longitud,
                        fuente.Region,
                        fuente.Zona,
                        fuente.DivisionTarifaria,
                        fuente.Operador,
                        fuente.Fuente,
                        fuente.FuenteCoordenadas,
                        fuente.Licencia,
                        fuente.EstadoConciliacion,
                        fuente.EstadoValidacion,
                        fuente.MetadatosJson,
                        1,
                        fuente.UltimaObservacionUtc,
                        fuente.UltimaObservacionUtc
                    )
                WHEN NOT MATCHED BY SOURCE AND destino.Activa = 1 THEN
                    UPDATE SET
                        Activa = 0,
                        UltimaObservacionUtc = @GeneradoUtc;
                """,
                connection,
                transaction))
            {
                merge.Parameters.AddWithValue(
                    "@GeneradoUtc",
                    generatedUtc);
                await merge.ExecuteNonQueryAsync(cancellationToken);
            }

            await ApplyActivePromotionsAsync(
                connection,
                transaction,
                generatedUtc,
                cancellationToken);

            await using (var execution = new SqlCommand(
                """
                INSERT INTO
                    dgmesnie.RedElectricaSubestacionInventarioEjecucion
                (
                    VersionIdCanonica,
                    GeneradoUtc,
                    RegistrosFuente,
                    RegistrosDgmesnie,
                    RegistrosAtlasSen,
                    RegistrosOpenStreetMap
                )
                VALUES
                (
                    @VersionId,
                    @GeneradoUtc,
                    @Registros,
                    @Dgmesnie,
                    @Atlas,
                    @Osm
                );
                """,
                connection,
                transaction))
            {
                execution.Parameters.AddWithValue(
                    "@VersionId",
                    graphVersionId);
                execution.Parameters.AddWithValue(
                    "@GeneradoUtc",
                    generatedUtc);
                execution.Parameters.AddWithValue("@Registros", rows.Count);
                execution.Parameters.AddWithValue(
                    "@Dgmesnie",
                    rows.Count(row =>
                        row.SourceKey ==
                        RedElectricaSubstationSources.DgmesnieGeoJson));
                execution.Parameters.AddWithValue(
                    "@Atlas",
                    rows.Count(row =>
                        row.SourceKey ==
                        RedElectricaSubstationSources.AtlasSen));
                execution.Parameters.AddWithValue(
                    "@Osm",
                    rows.Count(row =>
                        row.SourceKey ==
                        RedElectricaSubstationSources.OpenStreetMap));
                await execution.ExecuteNonQueryAsync(cancellationToken);
            }
            await transaction.CommitAsync(cancellationToken);
        }
        catch
        {
            await transaction.RollbackAsync(cancellationToken);
            throw;
        }
    }

    private static async Task<int> ApplyActivePromotionsAsync(
        SqlConnection connection,
        SqlTransaction transaction,
        DateTime appliedUtc,
        CancellationToken cancellationToken)
    {
        int applied;
        await using (var apply = new SqlCommand(
            """
            UPDATE inventario
            SET
                Latitud = promocion.Latitud,
                Longitud = promocion.Longitud,
                FuenteCoordenadas = promocion.FuenteCoordenadas,
                EstadoConciliacion =
                    N'georreferenciacion_automatica',
                EstadoValidacion = promocion.EstadoValidacion,
                MetadatosJson = JSON_MODIFY(
                    JSON_MODIFY(
                        JSON_MODIFY(
                            inventario.MetadatosJson,
                            '$.georeferenceExecutionId',
                            promocion.EjecucionId),
                        '$.georeferenceReference',
                        promocion.ReferenciaOsm),
                    '$.georeferenceScore',
                    promocion.Puntaje)
            FROM
                dgmesnie.RedElectricaSubestacionInventario
                    AS inventario
            INNER JOIN
                dgmesnie.RedElectricaSubestacionGeorefPromocion
                    AS promocion
              ON promocion.RegistroClave =
                    inventario.RegistroClave
             AND promocion.Activa = 1
            WHERE inventario.Activa = 1;
            """,
            connection,
            transaction))
        {
            applied = await apply.ExecuteNonQueryAsync(cancellationToken);
        }

        await using (var stamp = new SqlCommand(
            """
            UPDATE
                dgmesnie.RedElectricaSubestacionGeorefPromocion
            SET UltimaAplicacionUtc = @AplicadaUtc
            WHERE Activa = 1;
            """,
            connection,
            transaction))
        {
            stamp.Parameters.AddWithValue("@AplicadaUtc", appliedUtc);
            await stamp.ExecuteNonQueryAsync(cancellationToken);
        }
        return applied;
    }

    private async Task<IReadOnlyList<
        RedElectricaSubstationInventoryRecord>>
        LoadActiveRecordsAsync(CancellationToken cancellationToken)
    {
        EnsureConnectionConfigured();
        await using var connection = new SqlConnection(_connectionString);
        await connection.OpenAsync(cancellationToken);
        await using var command = new SqlCommand(
            """
            SELECT
                RegistroClave,
                UniversoClave,
                FuenteClave,
                ReferenciaClave,
                VersionIdCanonica,
                NodoCanonicoClave,
                Nombre,
                NombreNormalizado,
                TensionKv,
                NivelRed,
                Latitud,
                Longitud,
                Region,
                Zona,
                DivisionTarifaria,
                Operador,
                Fuente,
                FuenteCoordenadas,
                Licencia,
                EstadoConciliacion,
                EstadoValidacion
            FROM dgmesnie.RedElectricaSubestacionInventario
            WHERE Activa = 1;
            """,
            connection);
        var output = new List<RedElectricaSubstationInventoryRecord>();
        await using var reader = await command.ExecuteReaderAsync(
            cancellationToken);
        while (await reader.ReadAsync(cancellationToken))
        {
            output.Add(new RedElectricaSubstationInventoryRecord
            {
                RecordKey = reader.GetString(0),
                UniverseKey = reader.GetString(1),
                SourceKey = reader.GetString(2),
                ReferenceKey = reader.GetString(3),
                GraphVersionId = reader.IsDBNull(4)
                    ? null
                    : reader.GetInt64(4),
                CanonicalNodeKey = ReadString(reader, 5),
                Name = reader.GetString(6),
                NormalizedName = reader.GetString(7),
                VoltageKv = ReadDouble(reader, 8),
                NetworkLevel = reader.GetString(9),
                Latitude = ReadDouble(reader, 10),
                Longitude = ReadDouble(reader, 11),
                Region = ReadString(reader, 12),
                Zone = ReadString(reader, 13),
                TariffDivision = ReadString(reader, 14),
                Operator = ReadString(reader, 15),
                Source = reader.GetString(16),
                CoordinateSourceKey = ReadString(reader, 17),
                License = ReadString(reader, 18),
                ReconciliationState = reader.GetString(19),
                ValidationState = reader.GetString(20)
            });
        }
        return output;
    }

    private async Task<IReadOnlyList<LineAssociation>>
        LoadActiveLineAssociationsAsync(
            CancellationToken cancellationToken)
    {
        EnsureConnectionConfigured();
        await using var connection = new SqlConnection(_connectionString);
        await connection.OpenAsync(cancellationToken);
        await using var command = new SqlCommand(
            """
            SELECT
                RegistroClave,
                ElementoCatalogoClave,
                AristaClave,
                NombreLinea,
                TensionKv,
                DistanciaKm,
                EstadoValidacion
            FROM
                dgmesnie.RedElectricaSubestacionLineaPromocion
            WHERE Activa = 1;
            """,
            connection);
        var output = new List<LineAssociation>();
        await using var reader = await command.ExecuteReaderAsync(
            cancellationToken);
        while (await reader.ReadAsync(cancellationToken))
        {
            output.Add(new LineAssociation
            {
                RecordKey = reader.GetString(0),
                CatalogElementKey = reader.GetString(1),
                EdgeKey = reader.GetString(2),
                LineName = reader.GetString(3),
                VoltageKv = Convert.ToDouble(
                    reader.GetDecimal(4),
                    CultureInfo.InvariantCulture),
                DistanceKm = Convert.ToDouble(
                    reader.GetDecimal(5),
                    CultureInfo.InvariantCulture),
                ValidationState = reader.GetString(6)
            });
        }
        return output;
    }

    private static DataTable CreateStageTable(
        IReadOnlyList<InventorySeed> rows)
    {
        var table = new DataTable();
        AddColumn<string>(table, "RegistroClave");
        AddColumn<string>(table, "UniversoClave");
        AddColumn<string>(table, "FuenteClave");
        AddColumn<string>(table, "ReferenciaClave");
        AddColumn<long>(table, "VersionIdCanonica", true);
        AddColumn<string>(table, "NodoCanonicoClave", true);
        AddColumn<string>(table, "Nombre");
        AddColumn<string>(table, "NombreNormalizado");
        AddColumn<decimal>(table, "TensionKv", true);
        AddColumn<string>(table, "NivelRed");
        AddColumn<decimal>(table, "Latitud", true);
        AddColumn<decimal>(table, "Longitud", true);
        AddColumn<string>(table, "Region", true);
        AddColumn<string>(table, "Zona", true);
        AddColumn<string>(table, "DivisionTarifaria", true);
        AddColumn<string>(table, "Operador", true);
        AddColumn<string>(table, "Fuente");
        AddColumn<string>(table, "FuenteCoordenadas", true);
        AddColumn<string>(table, "Licencia", true);
        AddColumn<string>(table, "EstadoConciliacion");
        AddColumn<string>(table, "EstadoValidacion");
        AddColumn<string>(table, "MetadatosJson");
        AddColumn<DateTime>(table, "UltimaObservacionUtc");

        foreach (var row in rows)
        {
            table.Rows.Add(
                row.RecordKey,
                row.UniverseKey,
                row.SourceKey,
                row.ReferenceKey,
                DbValue(row.GraphVersionId),
                DbValue(row.CanonicalNodeKey),
                row.Name,
                row.NormalizedName,
                DbValue(row.VoltageKv),
                row.NetworkLevel,
                DbValue(row.Latitude),
                DbValue(row.Longitude),
                DbValue(row.Region),
                DbValue(row.Zone),
                DbValue(row.TariffDivision),
                DbValue(row.Operator),
                row.Source,
                DbValue(row.CoordinateSourceKey),
                DbValue(row.License),
                row.ReconciliationState,
                row.ValidationState,
                row.MetadataJson,
                row.GeneratedUtc);
        }
        return table;
    }

    private static List<RedElectricaGraphNode> FindSpatialCandidates(
        AtlasSenSubstationReference reference,
        IReadOnlyList<RedElectricaGraphNode> graphNodes)
    {
        if (!reference.Latitude.HasValue ||
            !reference.Longitude.HasValue)
        {
            return [];
        }
        return graphNodes
            .Where(node =>
                VoltageCompatible(reference.VoltageKv, node.VoltageKv) &&
                HaversineKm(
                    reference.Latitude.Value,
                    reference.Longitude.Value,
                    node.Latitude,
                    node.Longitude) <= SpatialMatchToleranceKm)
            .ToList();
    }

    private static bool VoltageCompatible(double? left, double? right) =>
        left.HasValue &&
        right.HasValue &&
        Math.Abs(left.Value - right.Value) <= 1;

    private static string StableRecordKey(
        string sourceKey,
        AtlasSenSubstationReference reference)
    {
        var material = string.Join(
            "\u001f",
            sourceKey,
            reference.ReferenceId,
            reference.Name,
            reference.VoltageKv?.ToString(
                "0.###",
                CultureInfo.InvariantCulture) ?? string.Empty,
            reference.Region,
            reference.Zone,
            reference.TariffDivision,
            reference.Latitude?.ToString(
                "0.#######",
                CultureInfo.InvariantCulture) ?? string.Empty,
            reference.Longitude?.ToString(
                "0.#######",
                CultureInfo.InvariantCulture) ?? string.Empty,
            reference.Operator);
        var hash = Convert.ToHexString(
            SHA256.HashData(Encoding.UTF8.GetBytes(material)))
            .ToLowerInvariant();
        return $"{sourceKey}:{hash[..40]}";
    }

    private static HashSet<string> ParseFilter(string? value) =>
        new(
            (value ?? string.Empty)
                .Split(
                    ',',
                    StringSplitOptions.TrimEntries |
                    StringSplitOptions.RemoveEmptyEntries)
                .Select(item => item.ToLowerInvariant()),
            StringComparer.OrdinalIgnoreCase);

    private static int SourcePriority(
        RedElectricaSubstationInventoryRecord record) =>
        record.SourceKey switch
        {
            RedElectricaSubstationSources.DgmesnieGeoJson => 0,
            RedElectricaSubstationSources.OpenStreetMap => 1,
            _ => 2
        };

    private static string ResolveNetworkLevel(
        IEnumerable<RedElectricaSubstationInventoryRecord> records)
    {
        var levels = records
            .Select(record => record.NetworkLevel)
            .ToHashSet(StringComparer.OrdinalIgnoreCase);
        if (levels.Contains(RedElectricaNetworkLevels.Transmission))
        {
            return RedElectricaNetworkLevels.Transmission;
        }
        if (levels.Contains(RedElectricaNetworkLevels.Subtransmission))
        {
            return RedElectricaNetworkLevels.Subtransmission;
        }
        return levels.Contains(RedElectricaNetworkLevels.Distribution)
            ? RedElectricaNetworkLevels.Distribution
            : RedElectricaNetworkLevels.Undetermined;
    }

    private static double HaversineKm(
        double latitudeA,
        double longitudeA,
        double latitudeB,
        double longitudeB)
    {
        const double radiusKm = 6371.0088;
        var deltaLatitude = DegreesToRadians(latitudeB - latitudeA);
        var deltaLongitude = DegreesToRadians(longitudeB - longitudeA);
        var a = Math.Pow(Math.Sin(deltaLatitude / 2), 2) +
            Math.Cos(DegreesToRadians(latitudeA)) *
            Math.Cos(DegreesToRadians(latitudeB)) *
            Math.Pow(Math.Sin(deltaLongitude / 2), 2);
        return 2 * radiusKm * Math.Asin(Math.Min(1, Math.Sqrt(a)));
    }

    private static double DegreesToRadians(double degrees) =>
        degrees * Math.PI / 180;

    private static double? ReadDouble(SqlDataReader reader, int ordinal) =>
        reader.IsDBNull(ordinal)
            ? null
            : Convert.ToDouble(
                reader.GetValue(ordinal),
                CultureInfo.InvariantCulture);

    private static string ReadString(SqlDataReader reader, int ordinal) =>
        reader.IsDBNull(ordinal)
            ? string.Empty
            : reader.GetString(ordinal);

    private static object DbValue(string? value) =>
        string.IsNullOrWhiteSpace(value)
            ? DBNull.Value
            : value;

    private static object DbValue(double? value) =>
        value.HasValue
            ? Convert.ToDecimal(value.Value)
            : DBNull.Value;

    private static object DbValue(long? value) =>
        value.HasValue
            ? value.Value
            : DBNull.Value;

    private static void AddColumn<T>(
        DataTable table,
        string name,
        bool nullable = false)
    {
        var column = table.Columns.Add(name, typeof(T));
        column.AllowDBNull = nullable;
    }

    private void EnsureConnectionConfigured()
    {
        if (string.IsNullOrWhiteSpace(_connectionString))
        {
            throw new InvalidOperationException(
                "No existe ConnectionStrings:DefaultConnection para el inventario de subestaciones.");
        }
    }

    private sealed class InventorySeed
    {
        public string RecordKey { get; init; } = string.Empty;
        public string UniverseKey { get; init; } = string.Empty;
        public string SourceKey { get; init; } = string.Empty;
        public string ReferenceKey { get; init; } = string.Empty;
        public long? GraphVersionId { get; init; }
        public string CanonicalNodeKey { get; init; } = string.Empty;
        public string Name { get; init; } = string.Empty;
        public string NormalizedName { get; init; } = string.Empty;
        public double? VoltageKv { get; init; }
        public string NetworkLevel { get; init; } = "indeterminado";
        public double? Latitude { get; init; }
        public double? Longitude { get; init; }
        public string Region { get; init; } = string.Empty;
        public string Zone { get; init; } = string.Empty;
        public string TariffDivision { get; init; } = string.Empty;
        public string Operator { get; init; } = string.Empty;
        public string Source { get; init; } = string.Empty;
        public string CoordinateSourceKey { get; init; } = string.Empty;
        public string License { get; init; } = string.Empty;
        public string ReconciliationState { get; init; } = string.Empty;
        public string ValidationState { get; init; } = string.Empty;
        public string MetadataJson { get; init; } = "{}";
        public DateTime GeneratedUtc { get; init; }
    }

    private sealed class LineAssociation
    {
        public string RecordKey { get; init; } = string.Empty;
        public string CatalogElementKey { get; init; } = string.Empty;
        public string EdgeKey { get; init; } = string.Empty;
        public string LineName { get; init; } = string.Empty;
        public double VoltageKv { get; init; }
        public double DistanceKm { get; init; }
        public string ValidationState { get; init; } = string.Empty;
    }
}
