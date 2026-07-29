using System.Globalization;
using System.Net;
using System.Net.Http.Headers;
using System.Security.Cryptography;
using System.Text;
using System.Text.Json;
using System.Text.RegularExpressions;
using Microsoft.Extensions.Caching.Memory;
using Microsoft.Extensions.Options;
using NSIE.Models;

namespace NSIE.Servicios;

public sealed class AtlasSenOptions
{
    public const string SectionName = "AtlasSen";

    public bool Enabled { get; set; } = true;
    public string AtlasUrl { get; set; } =
        "https://raw.githubusercontent.com/batuenergy/atlas-sen/main/public/data/atlas.json";
    public string OsmSubstationsUrl { get; set; } =
        "https://raw.githubusercontent.com/batuenergy/atlas-sen/main/public/data/osm_substations.json";
    public string TariffUsersUrl { get; set; } =
        "https://raw.githubusercontent.com/batuenergy/atlas-sen/main/public/data/cfe_users_ts.json";
    public string TariffEnergyUrl { get; set; } =
        "https://raw.githubusercontent.com/batuenergy/atlas-sen/main/public/data/cfe_energy_ts.json";
    public string MdaUrl { get; set; } =
        "https://raw.githubusercontent.com/batuenergy/atlas-sen/data/public/data/pnd/today.json";
    public string SnapshotDirectory { get; set; } =
        "App_Data/cache/atlas-sen";
    public int CacheMinutes { get; set; } = 360;
    public int UpdateCheckHours { get; set; } = 24;
}

public interface IAtlasSenReferenceService
{
    Task<AtlasSenStatus> GetStatusAsync(
        bool forceRefresh = false,
        CancellationToken cancellationToken = default);

    Task<AtlasSenStatus> RefreshAsync(
        CancellationToken cancellationToken = default);

    Task<IReadOnlyList<AtlasSenSubstationReference>> GetSubstationsAsync(
        string? search = null,
        string? networkLevel = null,
        int limit = 2500,
        CancellationToken cancellationToken = default);

    Task<AtlasSenSubstationAudit> AuditSubstationAsync(
        string declaredName,
        double? declaredVoltageKv,
        string? declaredRegion,
        CancellationToken cancellationToken = default);

    Task<AtlasSenTariffOverview> GetTariffOverviewAsync(
        CancellationToken cancellationToken = default);

    Task<AtlasSenGeoJson> GetTariffDivisionsGeoJsonAsync(
        CancellationToken cancellationToken = default);

    Task<AtlasSenMdaSnapshot> GetMdaAsync(
        CancellationToken cancellationToken = default);

    Task<AtlasSenStatus> AcknowledgeDatasetAsync(
        string datasetKey,
        CancellationToken cancellationToken = default);
}

public sealed partial class AtlasSenReferenceService :
    IAtlasSenReferenceService
{
    private const string StatusCacheKey = "atlas-sen-status-v1";
    private const string SubstationsCacheKey = "atlas-sen-substations-v1";
    private const string TariffGeoJsonCacheKey = "atlas-sen-tariff-geojson-v1";
    private const string TariffOverviewCacheKey = "atlas-sen-tariff-overview-v1";
    private const string MdaCacheKey = "atlas-sen-mda-v1";
    private static readonly SemaphoreSlim RefreshLock = new(1, 1);
    private static readonly JsonSerializerOptions JsonOptions = new()
    {
        PropertyNameCaseInsensitive = true,
        WriteIndented = true
    };

    private readonly IHttpClientFactory _httpClientFactory;
    private readonly IMemoryCache _cache;
    private readonly AtlasSenOptions _options;
    private readonly ILogger<AtlasSenReferenceService> _logger;
    private readonly string _snapshotDirectory;
    private readonly string _manifestPath;

    public AtlasSenReferenceService(
        IHttpClientFactory httpClientFactory,
        IMemoryCache cache,
        IOptions<AtlasSenOptions> options,
        IWebHostEnvironment environment,
        ILogger<AtlasSenReferenceService> logger)
    {
        _httpClientFactory = httpClientFactory;
        _cache = cache;
        _options = options.Value;
        _logger = logger;
        _snapshotDirectory = Path.IsPathRooted(_options.SnapshotDirectory)
            ? Path.GetFullPath(_options.SnapshotDirectory)
            : Path.GetFullPath(
                Path.Combine(
                    environment.ContentRootPath,
                    _options.SnapshotDirectory));
        _manifestPath = Path.Combine(
            _snapshotDirectory,
            "manifest.json");
    }

    public async Task<AtlasSenStatus> GetStatusAsync(
        bool forceRefresh = false,
        CancellationToken cancellationToken = default)
    {
        if (!_options.Enabled)
        {
            throw new InvalidOperationException(
                "La integración Atlas SEN está deshabilitada.");
        }

        if (!forceRefresh &&
            _cache.TryGetValue<AtlasSenStatus>(
                StatusCacheKey,
                out var cached) &&
            cached is not null)
        {
            return cached;
        }

        return await RefreshAsync(cancellationToken);
    }

    public async Task<AtlasSenStatus> RefreshAsync(
        CancellationToken cancellationToken = default)
    {
        if (!_options.Enabled)
        {
            throw new InvalidOperationException(
                "La integración Atlas SEN está deshabilitada.");
        }

        await RefreshLock.WaitAsync(cancellationToken);
        try
        {
            Directory.CreateDirectory(_snapshotDirectory);
            var previous = await ReadManifestAsync(cancellationToken);
            var definitions = DatasetDefinitions();
            var fetches = definitions.Select(definition =>
                FetchDatasetAsync(
                    definition,
                    previous?.Datasets.FirstOrDefault(item =>
                        string.Equals(
                            item.Key,
                            definition.Key,
                            StringComparison.Ordinal)),
                    cancellationToken));
            var datasets = await Task.WhenAll(fetches);
            var checkedUtc = DateTime.UtcNow;
            var states = datasets
                .Select(dataset => BuildDatasetState(
                    dataset,
                    previous,
                    checkedUtc))
                .OrderBy(state => state.Key, StringComparer.Ordinal)
                .ToList();
            var manifest = new AtlasSenManifest
            {
                CheckedUtc = checkedUtc,
                Datasets = states
            };
            await WriteJsonAtomicAsync(
                _manifestPath,
                manifest,
                cancellationToken);

            var status = BuildStatus(datasets, states, checkedUtc);
            CacheDocuments(datasets);
            ClearDerivedCaches();
            _cache.Set(
                StatusCacheKey,
                status,
                TimeSpan.FromMinutes(Math.Max(5, _options.CacheMinutes)));

            _logger.LogInformation(
                "Atlas SEN revisado: {Transmission} subestaciones de transmisión, {Distribution} de distribución/subtransmisión, {Lines} trazos de línea, {Divisions} divisiones y {MdaZones} zonas MDA; {Pending} dataset(s) estructurales pendientes.",
                status.TransmissionSubstations,
                status.DistributionSubstations,
                status.TransmissionLineFeatures,
                status.TariffDivisions,
                status.MdaZones,
                states.Count(state => state.PendingReview));
            return status;
        }
        finally
        {
            RefreshLock.Release();
        }
    }

    public async Task<IReadOnlyList<AtlasSenSubstationReference>>
        GetSubstationsAsync(
            string? search = null,
            string? networkLevel = null,
            int limit = 2500,
            CancellationToken cancellationToken = default)
    {
        if (!_cache.TryGetValue<IReadOnlyList<AtlasSenSubstationReference>>(
                SubstationsCacheKey,
                out var references) ||
            references is null)
        {
            var atlas = await GetDocumentAsync(
                AtlasSenDatasetKeys.Atlas,
                cancellationToken);
            var osm = await GetDocumentAsync(
                AtlasSenDatasetKeys.OsmSubstations,
                cancellationToken);
            references = ParseSubstations(atlas, osm);
            _cache.Set(
                SubstationsCacheKey,
                references,
                TimeSpan.FromMinutes(Math.Max(5, _options.CacheMinutes)));
        }

        var normalizedSearch =
            RedElectricaEndpointNameMatcher.NormalizeAlias(search ?? string.Empty);
        var normalizedLevel = NormalizeNetworkLevel(networkLevel);
        return references
            .Where(reference =>
                string.IsNullOrWhiteSpace(normalizedLevel) ||
                string.Equals(
                    reference.NetworkLevel,
                    normalizedLevel,
                    StringComparison.Ordinal))
            .Where(reference =>
                string.IsNullOrWhiteSpace(normalizedSearch) ||
                reference.NormalizedName.Contains(
                    normalizedSearch,
                    StringComparison.Ordinal) ||
                normalizedSearch.Contains(
                    reference.NormalizedName,
                    StringComparison.Ordinal))
            .OrderByDescending(reference =>
                !string.IsNullOrWhiteSpace(normalizedSearch) &&
                string.Equals(
                    reference.NormalizedName,
                    normalizedSearch,
                    StringComparison.Ordinal))
            .ThenByDescending(reference =>
                reference.NetworkLevel ==
                RedElectricaNetworkLevels.Transmission)
            .ThenByDescending(reference => reference.VoltageKv ?? 0)
            .ThenBy(reference => reference.Name, StringComparer.OrdinalIgnoreCase)
            .Take(Math.Clamp(limit, 1, 5000))
            .ToList();
    }

    public async Task<AtlasSenSubstationAudit> AuditSubstationAsync(
        string declaredName,
        double? declaredVoltageKv,
        string? declaredRegion,
        CancellationToken cancellationToken = default)
    {
        var normalized =
            RedElectricaEndpointNameMatcher.NormalizeAlias(declaredName);
        if (string.IsNullOrWhiteSpace(normalized))
        {
            return new AtlasSenSubstationAudit
            {
                DeclaredName = declaredName,
                DeclaredVoltageKv = declaredVoltageKv
            };
        }

        var references = await GetSubstationsAsync(
            declaredName,
            null,
            50,
            cancellationToken);
        var exact = references
            .Where(reference => string.Equals(
                reference.NormalizedName,
                normalized,
                StringComparison.Ordinal))
            .ToList();
        var matches = references
            .Where(reference =>
                string.Equals(
                    reference.NormalizedName,
                    normalized,
                    StringComparison.Ordinal) ||
                reference.NormalizedName.StartsWith(
                    $"{normalized} ",
                    StringComparison.Ordinal) ||
                normalized.StartsWith(
                    $"{reference.NormalizedName} ",
                    StringComparison.Ordinal))
            .ToList();
        var voltageMatches = declaredVoltageKv.HasValue
            ? matches.Where(reference =>
                    reference.VoltageKv.HasValue &&
                    Math.Abs(
                        reference.VoltageKv.Value -
                        declaredVoltageKv.Value) <= 1)
                .ToList()
            : new List<AtlasSenSubstationReference>();
        var region = NormalizePlainText(declaredRegion ?? string.Empty);
        var regionMatches = string.IsNullOrWhiteSpace(region)
            ? matches
            : matches.Where(reference =>
                    NormalizePlainText(reference.Region)
                        .Contains(region, StringComparison.Ordinal) ||
                    region.Contains(
                        NormalizePlainText(reference.Region),
                        StringComparison.Ordinal))
                .ToList();
        var findings = new List<string>();
        if (matches.Count > 0)
        {
            findings.Add(
                $"{matches.Count} referencia(s) nominal(es) Atlas SEN; no se promueven automáticamente al catálogo oficial.");
        }
        if (voltageMatches.Count > 0)
        {
            findings.Add(
                $"La tensión declarada coincide con {voltageMatches.Count} referencia(s).");
        }
        if (regionMatches.Count > 0 &&
            !string.IsNullOrWhiteSpace(region))
        {
            findings.Add(
                $"La región declarada es compatible con {regionMatches.Count} referencia(s).");
        }
        var hasTransmission = matches.Any(reference =>
            reference.NetworkLevel ==
            RedElectricaNetworkLevels.Transmission);
        var hasDistribution = matches.Any(reference =>
            reference.NetworkLevel is
                RedElectricaNetworkLevels.Distribution or
                RedElectricaNetworkLevels.Subtransmission);
        if (hasTransmission && hasDistribution)
        {
            findings.Add(
                "Existen instalaciones homónimas en niveles de red distintos; deben mantenerse como nodos separados.");
        }

        bool? voltageCompatible = !declaredVoltageKv.HasValue
            ? null
            : matches.Count == 0
                ? null
                : voltageMatches.Count > 0;
        var state = matches.Count == 0
            ? "sin_coincidencia"
            : exact.Count == 0
                ? "coincidencia_nominal_parcial"
                : hasTransmission && hasDistribution
                    ? "homonimos_nivel_red"
                    : voltageCompatible == false
                        ? "tension_incompatible"
                        : "coincidencia_exacta_secundaria";
        return new AtlasSenSubstationAudit
        {
            State = state,
            DeclaredName = declaredName,
            DeclaredVoltageKv = declaredVoltageKv,
            ExactName = exact.Count > 0,
            VoltageCompatible = voltageCompatible,
            HasTransmissionReference = hasTransmission,
            HasDistributionReference = hasDistribution,
            Findings = findings,
            Matches = matches
                .OrderByDescending(reference =>
                    reference.NetworkLevel ==
                    RedElectricaNetworkLevels.Transmission)
                .ThenByDescending(reference => reference.VoltageKv ?? 0)
                .Take(10)
                .ToList()
        };
    }

    public async Task<AtlasSenTariffOverview> GetTariffOverviewAsync(
        CancellationToken cancellationToken = default)
    {
        if (_cache.TryGetValue<AtlasSenTariffOverview>(
                TariffOverviewCacheKey,
                out var cached) &&
            cached is not null)
        {
            return cached;
        }

        var atlas = await GetDocumentAsync(
            AtlasSenDatasetKeys.Atlas,
            cancellationToken);
        var users = await GetDocumentAsync(
            AtlasSenDatasetKeys.TariffUsers,
            cancellationToken);
        var energy = await GetDocumentAsync(
            AtlasSenDatasetKeys.TariffEnergy,
            cancellationToken);
        using var atlasDocument = JsonDocument.Parse(atlas);
        using var usersDocument = JsonDocument.Parse(users);
        using var energyDocument = JsonDocument.Parse(energy);
        var divisions = GetProperty(
                atlasDocument.RootElement,
                "TZ")
            .EnumerateObject()
            .Select(property => property.Name)
            .OrderBy(value => value, StringComparer.OrdinalIgnoreCase)
            .ToList();
        var overview = new AtlasSenTariffOverview
        {
            Divisions = divisions,
            UsersYears = ReadStringArray(
                GetProperty(usersDocument.RootElement, "years")),
            EnergyYears = ReadStringArray(
                GetProperty(energyDocument.RootElement, "years"))
        };
        _cache.Set(
            TariffOverviewCacheKey,
            overview,
            TimeSpan.FromMinutes(Math.Max(5, _options.CacheMinutes)));
        return overview;
    }

    public async Task<AtlasSenGeoJson> GetTariffDivisionsGeoJsonAsync(
        CancellationToken cancellationToken = default)
    {
        if (_cache.TryGetValue<AtlasSenGeoJson>(
                TariffGeoJsonCacheKey,
                out var cached) &&
            cached is not null)
        {
            return cached;
        }

        var atlas = await GetDocumentAsync(
            AtlasSenDatasetKeys.Atlas,
            cancellationToken);
        using var document = JsonDocument.Parse(atlas);
        var divisions = GetProperty(document.RootElement, "TZ");
        var features = new List<AtlasSenGeoJsonFeature>();
        if (divisions.ValueKind == JsonValueKind.Object)
        {
            foreach (var division in divisions.EnumerateObject())
            {
                var coordinates = ReverseLatLngCoordinates(division.Value);
                features.Add(new AtlasSenGeoJsonFeature
                {
                    Geometry = JsonSerializer.SerializeToElement(new
                    {
                        type = "MultiPolygon",
                        coordinates
                    }),
                    Properties = new Dictionary<string, object?>
                    {
                        ["division"] = division.Name,
                        ["network_context"] = "division_tarifaria",
                        ["source"] =
                            "Atlas SEN · DOF municipio-división + INEGI",
                        ["license"] = "CC-BY 4.0",
                        ["validation_state"] =
                            "referencia_secundaria_pendiente_comparacion"
                    }
                });
            }
        }

        var result = new AtlasSenGeoJson
        {
            Meta = new AtlasSenGeoJsonMeta
            {
                Layer = "divisiones_tarifarias_atlas_sen",
                GeneratedUtc = DateTime.UtcNow,
                Features = features.Count,
                Source = "Atlas SEN · DOF municipio-división + INEGI",
                License = "CC-BY 4.0",
                ValidationState =
                    "referencia_secundaria_pendiente_comparacion"
            },
            Features = features
        };
        _cache.Set(
            TariffGeoJsonCacheKey,
            result,
            TimeSpan.FromMinutes(Math.Max(5, _options.CacheMinutes)));
        return result;
    }

    public async Task<AtlasSenMdaSnapshot> GetMdaAsync(
        CancellationToken cancellationToken = default)
    {
        if (_cache.TryGetValue<AtlasSenMdaSnapshot>(
                MdaCacheKey,
                out var cached) &&
            cached is not null)
        {
            return cached;
        }

        var raw = await GetDocumentAsync(
            AtlasSenDatasetKeys.Mda,
            cancellationToken);
        var snapshot = JsonSerializer.Deserialize<AtlasSenMdaSnapshot>(
                raw,
                JsonOptions) ??
            throw new JsonException(
                "La fotografía MDA de Atlas SEN no es válida.");
        _cache.Set(
            MdaCacheKey,
            snapshot,
            TimeSpan.FromMinutes(30));
        return snapshot;
    }

    public async Task<AtlasSenStatus> AcknowledgeDatasetAsync(
        string datasetKey,
        CancellationToken cancellationToken = default)
    {
        var normalizedKey = (datasetKey ?? string.Empty).Trim().ToLowerInvariant();
        if (!DatasetDefinitions().Any(definition =>
                string.Equals(
                    definition.Key,
                    normalizedKey,
                    StringComparison.Ordinal)))
        {
            throw new ArgumentOutOfRangeException(
                nameof(datasetKey),
                "El dataset Atlas SEN no existe.");
        }

        await RefreshLock.WaitAsync(cancellationToken);
        try
        {
            var manifest = await ReadManifestAsync(cancellationToken) ??
                throw new InvalidOperationException(
                    "No existe una fotografía Atlas SEN que reconocer.");
            manifest = new AtlasSenManifest
            {
                CheckedUtc = manifest.CheckedUtc,
                Datasets = manifest.Datasets
                    .Select(state => string.Equals(
                            state.Key,
                            normalizedKey,
                            StringComparison.Ordinal)
                        ? CopyState(
                            state,
                            pendingReview: false,
                            changeState: "revisado")
                        : state)
                    .ToList()
            };
            await WriteJsonAtomicAsync(
                _manifestPath,
                manifest,
                cancellationToken);
            _cache.Remove(StatusCacheKey);
        }
        finally
        {
            RefreshLock.Release();
        }

        return await GetStatusAsync(false, cancellationToken);
    }

    private async Task<string> GetDocumentAsync(
        string key,
        CancellationToken cancellationToken)
    {
        if (_cache.TryGetValue<string>(
                DocumentCacheKey(key),
                out var cached) &&
            cached is not null)
        {
            return cached;
        }

        await GetStatusAsync(false, cancellationToken);
        if (_cache.TryGetValue<string>(
                DocumentCacheKey(key),
                out cached) &&
            cached is not null)
        {
            return cached;
        }

        var path = SnapshotPath(key);
        return File.Exists(path)
            ? await File.ReadAllTextAsync(path, cancellationToken)
            : throw new FileNotFoundException(
                $"No existe una fotografía local para {key}.",
                path);
    }

    private async Task<FetchedDataset> FetchDatasetAsync(
        AtlasSenDatasetDefinition definition,
        AtlasSenDatasetState? previous,
        CancellationToken cancellationToken)
    {
        var path = SnapshotPath(definition.Key);
        try
        {
            var client = _httpClientFactory.CreateClient("AtlasSen");
            using var request = new HttpRequestMessage(
                HttpMethod.Get,
                ValidateUri(definition.Url));
            if (!string.IsNullOrWhiteSpace(previous?.ETag) &&
                EntityTagHeaderValue.TryParse(
                    previous.ETag,
                    out var entityTag))
            {
                request.Headers.IfNoneMatch.Add(entityTag);
            }
            using var response = await client.SendAsync(
                request,
                HttpCompletionOption.ResponseHeadersRead,
                cancellationToken);
            if (response.StatusCode == HttpStatusCode.NotModified &&
                File.Exists(path))
            {
                var cached = await File.ReadAllTextAsync(
                    path,
                    cancellationToken);
                return ParseFetchedDataset(
                    definition,
                    cached,
                    previous?.ETag ?? string.Empty,
                    false);
            }

            response.EnsureSuccessStatusCode();
            var content = await response.Content.ReadAsStringAsync(
                cancellationToken);
            using (JsonDocument.Parse(content))
            {
                // Valida JSON antes de reemplazar la última fotografía buena.
            }
            await WriteTextAtomicAsync(path, content, cancellationToken);
            return ParseFetchedDataset(
                definition,
                content,
                response.Headers.ETag?.ToString() ?? string.Empty,
                false);
        }
        catch (Exception exception) when (
            (exception is HttpRequestException or
                TaskCanceledException or
                JsonException or
                IOException) &&
            !cancellationToken.IsCancellationRequested &&
            File.Exists(path))
        {
            _logger.LogWarning(
                exception,
                "Atlas SEN no respondió para {Dataset}; se conserva la última fotografía local.",
                definition.Key);
            var cached = await File.ReadAllTextAsync(
                path,
                cancellationToken);
            return ParseFetchedDataset(
                definition,
                cached,
                previous?.ETag ?? string.Empty,
                true);
        }
    }

    private static FetchedDataset ParseFetchedDataset(
        AtlasSenDatasetDefinition definition,
        string content,
        string etag,
        bool fromFallback)
    {
        using var document = JsonDocument.Parse(content);
        var (records, sourceUpdatedUtc) = definition.Key switch
        {
            AtlasSenDatasetKeys.Atlas => (
                GetArrayLength(document.RootElement, "H") +
                GetArrayLength(document.RootElement, "OL"),
                ReadUpdatedUtc(document.RootElement)),
            AtlasSenDatasetKeys.OsmSubstations => (
                GetArrayLength(document.RootElement, "subs"),
                ReadUpdatedUtc(document.RootElement)),
            AtlasSenDatasetKeys.TariffUsers or
                AtlasSenDatasetKeys.TariffEnergy => (
                GetObjectLength(document.RootElement, "byDivision"),
                ReadUpdatedUtc(document.RootElement)),
            AtlasSenDatasetKeys.Mda => (
                GetInt(document.RootElement, "count"),
                ReadUpdatedUtc(document.RootElement)),
            _ => (0, null)
        };
        return new FetchedDataset(
            definition,
            content,
            Sha256(content),
            etag,
            records,
            sourceUpdatedUtc,
            fromFallback);
    }

    private static AtlasSenDatasetState BuildDatasetState(
        FetchedDataset dataset,
        AtlasSenManifest? previous,
        DateTime checkedUtc)
    {
        var previousState = previous?.Datasets.FirstOrDefault(state =>
            string.Equals(
                state.Key,
                dataset.Definition.Key,
                StringComparison.Ordinal));
        var hasPrevious = previousState is not null;
        var changed = hasPrevious &&
            !string.Equals(
                previousState!.Sha256,
                dataset.Sha256,
                StringComparison.Ordinal);
        var autoAccepted = dataset.Definition.AutoAccepted;
        return new AtlasSenDatasetState
        {
            Key = dataset.Definition.Key,
            Url = dataset.Definition.Url,
            Branch = dataset.Definition.Branch,
            Sha256 = dataset.Sha256,
            ETag = dataset.ETag,
            CheckedUtc = checkedUtc,
            LastChangedUtc = changed || !hasPrevious
                ? checkedUtc
                : previousState!.LastChangedUtc,
            SourceUpdatedUtc = dataset.SourceUpdatedUtc,
            Records = dataset.Records,
            ChangeState = changed
                ? "modificado"
                : hasPrevious
                    ? "sin_cambio"
                    : "linea_base",
            PendingReview = !autoAccepted &&
                (changed || previousState?.PendingReview == true),
            AutoAccepted = autoAccepted,
            FromLocalFallback = dataset.FromFallback,
            Source = dataset.Definition.Source,
            License = dataset.Definition.License
        };
    }

    private static AtlasSenStatus BuildStatus(
        IReadOnlyList<FetchedDataset> datasets,
        IReadOnlyList<AtlasSenDatasetState> states,
        DateTime checkedUtc)
    {
        var atlas = datasets.Single(dataset =>
            dataset.Definition.Key == AtlasSenDatasetKeys.Atlas);
        var osm = datasets.Single(dataset =>
            dataset.Definition.Key == AtlasSenDatasetKeys.OsmSubstations);
        var mda = datasets.Single(dataset =>
            dataset.Definition.Key == AtlasSenDatasetKeys.Mda);
        using var atlasDocument = JsonDocument.Parse(atlas.Content);
        using var osmDocument = JsonDocument.Parse(osm.Content);
        using var mdaDocument = JsonDocument.Parse(mda.Content);
        return new AtlasSenStatus
        {
            CheckedUtc = checkedUtc,
            UsedLocalFallback = states.Any(state => state.FromLocalFallback),
            HasPendingStructuralChanges = states.Any(state =>
                state.PendingReview),
            TransmissionSubstations = GetArrayLength(
                atlasDocument.RootElement,
                "H"),
            DistributionSubstations = GetArrayLength(
                osmDocument.RootElement,
                "subs"),
            TransmissionLineFeatures = GetArrayLength(
                atlasDocument.RootElement,
                "OL"),
            TariffDivisions = GetObjectLength(
                atlasDocument.RootElement,
                "TZ"),
            MdaZones = GetInt(mdaDocument.RootElement, "count"),
            MdaUpdatedUtc = ReadUpdatedUtc(mdaDocument.RootElement),
            MdaOperatingDate = ReadDateOnly(
                mdaDocument.RootElement,
                "operatingDate"),
            Datasets = states
        };
    }

    private static IReadOnlyList<AtlasSenSubstationReference> ParseSubstations(
        string atlasJson,
        string osmJson)
    {
        var output = new List<AtlasSenSubstationReference>();
        using (var document = JsonDocument.Parse(atlasJson))
        {
            var root = document.RootElement;
            var regions = GetProperty(root, "REG");
            var hubs = GetProperty(root, "H");
            if (hubs.ValueKind == JsonValueKind.Array)
            {
                foreach (var hub in hubs.EnumerateArray())
                {
                    if (hub.ValueKind != JsonValueKind.Array)
                    {
                        continue;
                    }
                    var name = GetArrayString(hub, 0);
                    if (string.IsNullOrWhiteSpace(name))
                    {
                        continue;
                    }
                    var voltage = GetArrayDouble(hub, 3);
                    var regionIndex = GetArrayInt(hub, 7);
                    var region = regionIndex.HasValue &&
                                 regions.ValueKind == JsonValueKind.Array &&
                                 regionIndex.Value >= 0 &&
                                 regionIndex.Value < regions.GetArrayLength()
                        ? GetArrayString(regions, regionIndex.Value)
                        : string.Empty;
                    output.Add(new AtlasSenSubstationReference
                    {
                        ReferenceId = StableReferenceId(
                            "ATLAS-H",
                            name,
                            voltage),
                        Name = name,
                        NormalizedName =
                            RedElectricaEndpointNameMatcher.NormalizeAlias(name),
                        NetworkLevel =
                            RedElectricaNetworkLevels.Transmission,
                        VoltageKv = voltage,
                        VoltageRaw = voltage.HasValue
                            ? $"{voltage:0.###} kV"
                            : string.Empty,
                        TransformerMva = GetArrayDouble(hub, 4),
                        Saturation = GetArrayDouble(hub, 6),
                        Region = region,
                        Zone = GetArrayString(hub, 8),
                        TariffDivision = GetArrayString(hub, 9),
                        SourceDataset = AtlasSenDatasetKeys.Atlas,
                        Source =
                            "Atlas SEN · extracción de diagramas unifilares CENACE",
                        License = "CC-BY 4.0"
                    });
                }
            }
        }

        using (var document = JsonDocument.Parse(osmJson))
        {
            var substations = GetProperty(document.RootElement, "subs");
            if (substations.ValueKind == JsonValueKind.Array)
            {
                foreach (var item in substations.EnumerateArray())
                {
                    if (item.ValueKind != JsonValueKind.Array)
                    {
                        continue;
                    }
                    var name = GetArrayString(item, 3);
                    if (string.IsNullOrWhiteSpace(name))
                    {
                        name = "Subestación OSM sin nombre";
                    }
                    var band = GetArrayInt(item, 2) ?? 0;
                    var voltageRaw = GetArrayString(item, 5);
                    var voltage = ParseHighestVoltageKv(voltageRaw);
                    var networkLevel = band == 0
                        ? RedElectricaNetworkLevels.Distribution
                        : RedElectricaNetworkLevels.Subtransmission;
                    output.Add(new AtlasSenSubstationReference
                    {
                        ReferenceId = StableReferenceId(
                            "ATLAS-OSM",
                            name,
                            voltage,
                            GetArrayDouble(item, 0),
                            GetArrayDouble(item, 1)),
                        Name = name,
                        NormalizedName =
                            RedElectricaEndpointNameMatcher.NormalizeAlias(name),
                        NetworkLevel = networkLevel,
                        VoltageKv = voltage,
                        VoltageRaw = voltageRaw,
                        Operator = GetArrayString(item, 4),
                        Latitude = GetArrayDouble(item, 0),
                        Longitude = GetArrayDouble(item, 1),
                        SourceDataset =
                            AtlasSenDatasetKeys.OsmSubstations,
                        Source =
                            "OpenStreetMap contributors · power=substation",
                        License = "ODbL 1.0"
                    });
                }
            }
        }

        return output;
    }

    private void CacheDocuments(IEnumerable<FetchedDataset> datasets)
    {
        foreach (var dataset in datasets)
        {
            _cache.Set(
                DocumentCacheKey(dataset.Definition.Key),
                dataset.Content,
                TimeSpan.FromMinutes(Math.Max(5, _options.CacheMinutes)));
        }
    }

    private void ClearDerivedCaches()
    {
        _cache.Remove(SubstationsCacheKey);
        _cache.Remove(TariffGeoJsonCacheKey);
        _cache.Remove(TariffOverviewCacheKey);
        _cache.Remove(MdaCacheKey);
    }

    private IReadOnlyList<AtlasSenDatasetDefinition> DatasetDefinitions() =>
        new[]
        {
            new AtlasSenDatasetDefinition(
                AtlasSenDatasetKeys.Atlas,
                _options.AtlasUrl,
                "main",
                "Atlas SEN · CENACE/DOF/INEGI/OSM compilado",
                "Licencia según dataset; OSM bajo ODbL",
                false),
            new AtlasSenDatasetDefinition(
                AtlasSenDatasetKeys.OsmSubstations,
                _options.OsmSubstationsUrl,
                "main",
                "OpenStreetMap contributors",
                "ODbL 1.0",
                false),
            new AtlasSenDatasetDefinition(
                AtlasSenDatasetKeys.TariffUsers,
                _options.TariffUsersUrl,
                "main",
                "CNE · memorias de cálculo del Suministro Básico",
                "CC-BY 4.0",
                false),
            new AtlasSenDatasetDefinition(
                AtlasSenDatasetKeys.TariffEnergy,
                _options.TariffEnergyUrl,
                "main",
                "CNE · memorias de cálculo del Suministro Básico",
                "CC-BY 4.0",
                false),
            new AtlasSenDatasetDefinition(
                AtlasSenDatasetKeys.Mda,
                _options.MdaUrl,
                "data",
                "CENACE · SWPEND MDA, PND por zona de carga",
                "Datos públicos CENACE",
                true)
        };

    private async Task<AtlasSenManifest?> ReadManifestAsync(
        CancellationToken cancellationToken)
    {
        if (!File.Exists(_manifestPath))
        {
            return null;
        }

        try
        {
            var json = await File.ReadAllTextAsync(
                _manifestPath,
                cancellationToken);
            return JsonSerializer.Deserialize<AtlasSenManifest>(
                json,
                JsonOptions);
        }
        catch (Exception exception) when (
            exception is IOException or JsonException)
        {
            _logger.LogWarning(
                exception,
                "El manifiesto Atlas SEN no pudo leerse; se reconstruirá.");
            return null;
        }
    }

    private string SnapshotPath(string key) =>
        Path.Combine(_snapshotDirectory, $"{key}.json");

    private static string DocumentCacheKey(string key) =>
        $"atlas-sen-document-v1:{key}";

    private static Uri ValidateUri(string value)
    {
        if (!Uri.TryCreate(value, UriKind.Absolute, out var uri) ||
            uri.Scheme != Uri.UriSchemeHttps)
        {
            throw new InvalidOperationException(
                "Las fuentes Atlas SEN deben usar una URL HTTPS absoluta.");
        }
        return uri;
    }

    private static async Task WriteTextAtomicAsync(
        string path,
        string content,
        CancellationToken cancellationToken)
    {
        Directory.CreateDirectory(
            Path.GetDirectoryName(path) ??
            throw new InvalidOperationException(
                "La ruta de fotografía Atlas SEN no es válida."));
        var temporary = $"{path}.{Guid.NewGuid():N}.tmp";
        try
        {
            await File.WriteAllTextAsync(
                temporary,
                content,
                new UTF8Encoding(false),
                cancellationToken);
            File.Move(temporary, path, true);
        }
        finally
        {
            if (File.Exists(temporary))
            {
                File.Delete(temporary);
            }
        }
    }

    private static Task WriteJsonAtomicAsync<T>(
        string path,
        T value,
        CancellationToken cancellationToken) =>
        WriteTextAtomicAsync(
            path,
            JsonSerializer.Serialize(value, JsonOptions),
            cancellationToken);

    private static string Sha256(string value) =>
        Convert.ToHexString(
                SHA256.HashData(Encoding.UTF8.GetBytes(value)))
            .ToLowerInvariant();

    private static string StableReferenceId(
        string prefix,
        params object?[] parts) =>
        $"{prefix}:{Sha256(string.Join("|", parts.Select(part =>
            Convert.ToString(
                part,
                CultureInfo.InvariantCulture) ?? string.Empty)))[..20]}";

    private static string NormalizeNetworkLevel(string? value)
    {
        var normalized = NormalizePlainText(value ?? string.Empty)
            .ToLowerInvariant();
        return normalized switch
        {
            "transmision" => RedElectricaNetworkLevels.Transmission,
            "subtransmision" => RedElectricaNetworkLevels.Subtransmission,
            "distribucion" => RedElectricaNetworkLevels.Distribution,
            _ => string.Empty
        };
    }

    private static string NormalizePlainText(string value)
    {
        if (string.IsNullOrWhiteSpace(value))
        {
            return string.Empty;
        }
        var decomposed = value.Normalize(NormalizationForm.FormD);
        var builder = new StringBuilder(decomposed.Length);
        foreach (var character in decomposed)
        {
            if (CharUnicodeInfo.GetUnicodeCategory(character) !=
                UnicodeCategory.NonSpacingMark)
            {
                builder.Append(character);
            }
        }
        return WhitespaceRegex().Replace(
                NonAlphanumericRegex().Replace(
                    builder.ToString().ToUpperInvariant(),
                    " "),
                " ")
            .Trim();
    }

    private static AtlasSenDatasetState CopyState(
        AtlasSenDatasetState source,
        bool pendingReview,
        string changeState) =>
        new()
        {
            Key = source.Key,
            Url = source.Url,
            Branch = source.Branch,
            Sha256 = source.Sha256,
            ETag = source.ETag,
            CheckedUtc = source.CheckedUtc,
            LastChangedUtc = source.LastChangedUtc,
            SourceUpdatedUtc = source.SourceUpdatedUtc,
            Records = source.Records,
            ChangeState = changeState,
            PendingReview = pendingReview,
            AutoAccepted = source.AutoAccepted,
            FromLocalFallback = source.FromLocalFallback,
            Source = source.Source,
            License = source.License
        };

    private static JsonElement GetProperty(
        JsonElement element,
        string name)
    {
        if (element.ValueKind == JsonValueKind.Object &&
            element.TryGetProperty(name, out var value))
        {
            return value;
        }
        return default;
    }

    private static int GetArrayLength(
        JsonElement element,
        string name)
    {
        var value = GetProperty(element, name);
        return value.ValueKind == JsonValueKind.Array
            ? value.GetArrayLength()
            : 0;
    }

    private static int GetObjectLength(
        JsonElement element,
        string name)
    {
        var value = GetProperty(element, name);
        return value.ValueKind == JsonValueKind.Object
            ? value.EnumerateObject().Count()
            : 0;
    }

    private static int GetInt(JsonElement element, string name)
    {
        var value = GetProperty(element, name);
        return value.ValueKind == JsonValueKind.Number &&
               value.TryGetInt32(out var number)
            ? number
            : 0;
    }

    private static DateTime? ReadUpdatedUtc(JsonElement element)
    {
        var value = GetProperty(element, "updatedAt");
        return value.ValueKind == JsonValueKind.String &&
               DateTime.TryParse(
                   value.GetString(),
                   CultureInfo.InvariantCulture,
                   DateTimeStyles.AssumeUniversal |
                   DateTimeStyles.AdjustToUniversal,
                   out var timestamp)
            ? timestamp
            : null;
    }

    private static DateOnly? ReadDateOnly(
        JsonElement element,
        string name)
    {
        var value = GetProperty(element, name);
        return value.ValueKind == JsonValueKind.String &&
               DateOnly.TryParse(
                   value.GetString(),
                   CultureInfo.InvariantCulture,
                   DateTimeStyles.None,
                   out var date)
            ? date
            : null;
    }

    private static IReadOnlyList<string> ReadStringArray(
        JsonElement element) =>
        element.ValueKind == JsonValueKind.Array
            ? element.EnumerateArray()
                .Select(item => item.ValueKind == JsonValueKind.String
                    ? item.GetString() ?? string.Empty
                    : item.GetRawText())
                .Where(value => !string.IsNullOrWhiteSpace(value))
                .ToList()
            : Array.Empty<string>();

    private static string GetArrayString(JsonElement array, int index)
    {
        if (array.ValueKind != JsonValueKind.Array ||
            index < 0 ||
            index >= array.GetArrayLength())
        {
            return string.Empty;
        }
        var value = array[index];
        return value.ValueKind switch
        {
            JsonValueKind.String => value.GetString() ?? string.Empty,
            JsonValueKind.Number => value.GetRawText(),
            _ => string.Empty
        };
    }

    private static double? GetArrayDouble(JsonElement array, int index)
    {
        if (array.ValueKind != JsonValueKind.Array ||
            index < 0 ||
            index >= array.GetArrayLength())
        {
            return null;
        }
        var value = array[index];
        if (value.ValueKind == JsonValueKind.Number &&
            value.TryGetDouble(out var number))
        {
            return number;
        }
        return value.ValueKind == JsonValueKind.String &&
               double.TryParse(
                   value.GetString(),
                   NumberStyles.Float,
                   CultureInfo.InvariantCulture,
                   out number)
            ? number
            : null;
    }

    private static int? GetArrayInt(JsonElement array, int index)
    {
        var value = GetArrayDouble(array, index);
        return value.HasValue
            ? Convert.ToInt32(
                value.Value,
                CultureInfo.InvariantCulture)
            : null;
    }

    private static double? ParseHighestVoltageKv(string value)
    {
        var voltages = NumberRegex()
            .Matches(value ?? string.Empty)
            .Select(match => double.TryParse(
                    match.Value,
                    NumberStyles.Float,
                    CultureInfo.InvariantCulture,
                    out var parsed)
                ? parsed
                : 0)
            .Where(parsed => parsed > 0)
            .Select(parsed => parsed > 1000 ? parsed / 1000 : parsed)
            .ToList();
        return voltages.Count > 0 ? voltages.Max() : null;
    }

    private static object ReverseLatLngCoordinates(JsonElement element)
    {
        if (element.ValueKind != JsonValueKind.Array)
        {
            return Array.Empty<object>();
        }

        var items = element.EnumerateArray().ToList();
        if (items.Count >= 2 &&
            items[0].ValueKind == JsonValueKind.Number &&
            items[1].ValueKind == JsonValueKind.Number)
        {
            return new[]
            {
                items[1].GetDouble(),
                items[0].GetDouble()
            };
        }

        return items
            .Select(ReverseLatLngCoordinates)
            .ToList();
    }

    [GeneratedRegex(@"[^A-Z0-9]+", RegexOptions.CultureInvariant)]
    private static partial Regex NonAlphanumericRegex();

    [GeneratedRegex(@"\s+", RegexOptions.CultureInvariant)]
    private static partial Regex WhitespaceRegex();

    [GeneratedRegex(@"\d+(?:\.\d+)?", RegexOptions.CultureInvariant)]
    private static partial Regex NumberRegex();

    private sealed class AtlasSenManifest
    {
        public DateTime CheckedUtc { get; init; }
        public IReadOnlyList<AtlasSenDatasetState> Datasets { get; init; } =
            Array.Empty<AtlasSenDatasetState>();
    }

    private sealed record AtlasSenDatasetDefinition(
        string Key,
        string Url,
        string Branch,
        string Source,
        string License,
        bool AutoAccepted);

    private sealed record FetchedDataset(
        AtlasSenDatasetDefinition Definition,
        string Content,
        string Sha256,
        string ETag,
        int Records,
        DateTime? SourceUpdatedUtc,
        bool FromFallback);
}

public sealed class AtlasSenUpdateMonitor : BackgroundService
{
    private readonly IServiceScopeFactory _scopeFactory;
    private readonly AtlasSenOptions _options;
    private readonly ILogger<AtlasSenUpdateMonitor> _logger;

    public AtlasSenUpdateMonitor(
        IServiceScopeFactory scopeFactory,
        IOptions<AtlasSenOptions> options,
        ILogger<AtlasSenUpdateMonitor> logger)
    {
        _scopeFactory = scopeFactory;
        _options = options.Value;
        _logger = logger;
    }

    protected override async Task ExecuteAsync(
        CancellationToken stoppingToken)
    {
        if (!_options.Enabled)
        {
            return;
        }

        await CheckAsync(stoppingToken);
        using var timer = new PeriodicTimer(
            TimeSpan.FromHours(Math.Max(1, _options.UpdateCheckHours)));
        while (await timer.WaitForNextTickAsync(stoppingToken))
        {
            await CheckAsync(stoppingToken);
        }
    }

    private async Task CheckAsync(CancellationToken cancellationToken)
    {
        try
        {
            using var scope = _scopeFactory.CreateScope();
            var service =
                scope.ServiceProvider.GetRequiredService<
                    IAtlasSenReferenceService>();
            await service.RefreshAsync(cancellationToken);
            var inventory =
                scope.ServiceProvider.GetRequiredService<
                    IRedElectricaSubstationInventoryService>();
            await inventory.SynchronizeAsync(cancellationToken);
        }
        catch (OperationCanceledException) when (
            cancellationToken.IsCancellationRequested)
        {
        }
        catch (Exception exception)
        {
            _logger.LogWarning(
                exception,
                "La revisión periódica de Atlas SEN no pudo completarse; la aplicación conserva la última fotografía válida.");
        }
    }
}
