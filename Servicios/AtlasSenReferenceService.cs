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
    private const string DefaultSnapshotDirectory =
        "App_Data/cache/atlas-sen";

    public bool Enabled { get; set; } = true;
    public string AtlasUrl { get; set; } =
        "https://raw.githubusercontent.com/batuenergy/atlas-sen/main/public/data/atlas.json";
    public string OsmSubstationsUrl { get; set; } =
        "https://raw.githubusercontent.com/batuenergy/atlas-sen/main/public/data/osm_substations.json";
    public string TariffUsersUrl { get; set; } =
        "https://raw.githubusercontent.com/batuenergy/atlas-sen/main/public/data/cfe_users_ts.json";
    public string TariffEnergyUrl { get; set; } =
        "https://raw.githubusercontent.com/batuenergy/atlas-sen/main/public/data/cfe_energy_ts.json";
    public string DemandUrl { get; set; } =
        "https://raw.githubusercontent.com/batuenergy/atlas-sen/data/public/data/demand/today.json";
    public string MdaUrl { get; set; } =
        "https://raw.githubusercontent.com/batuenergy/atlas-sen/data/public/data/pnd/today.json";
    public string WeatherUrl { get; set; } =
        "https://api.open-meteo.com/v1/forecast";
    public string PrivateGenerationUrl { get; set; } =
        "https://raw.githubusercontent.com/batuenergy/atlas-sen/main/public/data/private_generation.json";
    public string DistributedGenerationByStateUrl { get; set; } =
        "https://raw.githubusercontent.com/batuenergy/atlas-sen/main/public/data/dg_by_state.json";
    public string DistributedGenerationBySizeUrl { get; set; } =
        "https://raw.githubusercontent.com/batuenergy/atlas-sen/main/public/data/dg_by_size.json";
    public string SnapshotDirectory { get; set; } =
        DefaultSnapshotDirectory;
    public int CacheMinutes { get; set; } = 360;
    public int LiveDataCacheMinutes { get; set; } = 5;
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

    Task<AtlasSenTariffSeriesResponse> GetTariffSeriesAsync(
        CancellationToken cancellationToken = default);

    Task<AtlasSenGeoJson> GetTariffDivisionsGeoJsonAsync(
        CancellationToken cancellationToken = default);

    Task<AtlasSenDemandSnapshot> GetDemandAsync(
        CancellationToken cancellationToken = default);

    Task<AtlasSenGeoJson> GetDemandRegionsGeoJsonAsync(
        CancellationToken cancellationToken = default);

    Task<AtlasSenWeatherResponse?> GetWeatherAsync(
        string region,
        CancellationToken cancellationToken = default);

    Task<AtlasSenMdaSnapshot> GetMdaAsync(
        CancellationToken cancellationToken = default);

    Task<AtlasSenGeoJson> GetPrivateGenerationGeoJsonAsync(
        CancellationToken cancellationToken = default);

    Task<AtlasSenDistributedGenerationResponse> GetDistributedGenerationAsync(
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
    private const string TariffSeriesCacheKey = "atlas-sen-tariff-series-v1";
    private const string DemandCacheKey = "atlas-sen-demand-v1";
    private const string DemandGeoJsonCacheKey = "atlas-sen-demand-geojson-v1";
    private const string WeatherCacheKeyPrefix = "atlas-sen-weather-v1";
    private const string MdaCacheKey = "atlas-sen-mda-v1";
    private const string PrivateGenerationGeoJsonCacheKey =
        "atlas-sen-private-generation-geojson-v1";
    private const string DistributedGenerationCacheKey =
        "atlas-sen-distributed-generation-v1";
    private static readonly SemaphoreSlim RefreshLock = new(1, 1);
    private static readonly JsonSerializerOptions JsonOptions = new()
    {
        PropertyNameCaseInsensitive = true,
        WriteIndented = true
    };
    private static readonly IReadOnlyDictionary<string, (double Latitude, double Longitude)>
        DemandRegionWeatherPoints =
            new Dictionary<string, (double Latitude, double Longitude)>(
                StringComparer.OrdinalIgnoreCase)
            {
                ["Baja California"] = (30.15, -115.15),
                ["Baja California Sur"] = (25.70, -111.75),
                ["Central"] = (19.43, -99.13),
                ["Noreste"] = (25.67, -100.31),
                ["Noroeste"] = (28.90, -110.95),
                ["Norte"] = (28.63, -106.08),
                ["Occidental"] = (20.67, -103.35),
                ["Oriental"] = (19.17, -96.13),
                ["Peninsular"] = (20.97, -89.62)
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
        _snapshotDirectory = ResolveSnapshotDirectory(
            _options.SnapshotDirectory,
            environment.ContentRootPath);
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
                "Atlas SEN revisado: {Transmission} subestaciones de transmisión, {Distribution} de distribución/subtransmisión, {Lines} trazos de línea, {Divisions} divisiones, {DemandRegions} regiones de demanda y {MdaZones} zonas MDA; {Pending} dataset(s) estructurales pendientes.",
                status.TransmissionSubstations,
                status.DistributionSubstations,
                status.TransmissionLineFeatures,
                status.TariffDivisions,
                status.DemandRegions,
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
        var users = await GetDocumentAsync(
            AtlasSenDatasetKeys.TariffUsers,
            cancellationToken);
        var energy = await GetDocumentAsync(
            AtlasSenDatasetKeys.TariffEnergy,
            cancellationToken);
        using var document = JsonDocument.Parse(atlas);
        using var usersDocument = JsonDocument.Parse(users);
        using var energyDocument = JsonDocument.Parse(energy);
        var divisions = GetProperty(document.RootElement, "TZ");
        var referenceYear = ResolveTariffReferenceYear(
            usersDocument.RootElement,
            energyDocument.RootElement);
        var usersByDivision = GetProperty(
            usersDocument.RootElement,
            "byDivision");
        var energyByDivision = GetProperty(
            energyDocument.RootElement,
            "byDivision");
        var features = new List<AtlasSenGeoJsonFeature>();
        if (divisions.ValueKind == JsonValueKind.Object)
        {
            foreach (var division in divisions.EnumerateObject())
            {
                var coordinates = ReverseLatLngCoordinates(division.Value);
                var usersTotal = SumTariffDivisionYear(
                    usersByDivision,
                    division.Name,
                    referenceYear);
                var energyMwh = SumTariffDivisionYear(
                    energyByDivision,
                    division.Name,
                    referenceYear);
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
                        ["reference_year"] = referenceYear,
                        ["users_total"] = usersTotal,
                        ["energy_mwh"] = energyMwh,
                        ["energy_gwh"] = energyMwh / 1000d,
                        ["source"] =
                            "Atlas SEN · DOF/INEGI + CNE memorias de cálculo",
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
                Source =
                    "Atlas SEN · DOF/INEGI + CNE memorias de cálculo",
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

    public async Task<AtlasSenTariffSeriesResponse> GetTariffSeriesAsync(
        CancellationToken cancellationToken = default)
    {
        if (_cache.TryGetValue<AtlasSenTariffSeriesResponse>(
                TariffSeriesCacheKey,
                out var cached) &&
            cached is not null)
        {
            return cached;
        }

        var users = await GetDocumentAsync(
            AtlasSenDatasetKeys.TariffUsers,
            cancellationToken);
        var energy = await GetDocumentAsync(
            AtlasSenDatasetKeys.TariffEnergy,
            cancellationToken);
        using var usersDocument = JsonDocument.Parse(users);
        using var energyDocument = JsonDocument.Parse(energy);
        var usersRoot = usersDocument.RootElement;
        var energyRoot = energyDocument.RootElement;
        var usersByDivision = GetProperty(usersRoot, "byDivision");
        var energyByDivision = GetProperty(energyRoot, "byDivision");
        var referenceYear = ResolveTariffReferenceYear(
            usersRoot,
            energyRoot);
        var usersYears = ReadStringArray(GetProperty(usersRoot, "years"))
            .ToHashSet(StringComparer.Ordinal);
        var years = ReadStringArray(GetProperty(energyRoot, "years"))
            .Where(usersYears.Contains)
            .OrderBy(year => year, StringComparer.Ordinal)
            .ToList();
        var yearStatus = GetProperty(
            GetProperty(energyRoot, "meta"),
            "year_status");
        var divisionNames = new HashSet<string>(
            StringComparer.OrdinalIgnoreCase);
        if (usersByDivision.ValueKind == JsonValueKind.Object)
        {
            foreach (var division in usersByDivision.EnumerateObject())
            {
                divisionNames.Add(division.Name);
            }
        }
        if (energyByDivision.ValueKind == JsonValueKind.Object)
        {
            foreach (var division in energyByDivision.EnumerateObject())
            {
                divisionNames.Add(division.Name);
            }
        }

        var divisions = new Dictionary<string, AtlasSenTariffDivisionSeries>(
            StringComparer.OrdinalIgnoreCase);
        foreach (var division in divisionNames.OrderBy(
                     value => value,
                     StringComparer.OrdinalIgnoreCase))
        {
            var points = new List<AtlasSenTariffSeriesPoint>();
            foreach (var year in years)
            {
                var usersTotal = SumTariffDivisionYear(
                    usersByDivision,
                    division,
                    year);
                var energyMwh = SumTariffDivisionYear(
                    energyByDivision,
                    division,
                    year);
                var status = yearStatus.ValueKind == JsonValueKind.Object &&
                             yearStatus.TryGetProperty(
                                 year,
                                 out var statusValue)
                    ? statusValue.GetString() ?? string.Empty
                    : string.Empty;
                var complete = string.Equals(
                    status,
                    "ok_12_months",
                    StringComparison.OrdinalIgnoreCase) ||
                    string.Equals(
                        year,
                        referenceYear,
                        StringComparison.Ordinal);
                points.Add(new AtlasSenTariffSeriesPoint
                {
                    Year = year,
                    Users = usersTotal,
                    EnergyMwh = energyMwh,
                    EnergyGwh = energyMwh / 1000d,
                    IntensityKwhPerUser =
                        usersTotal is > 0 && energyMwh.HasValue
                            ? energyMwh.Value * 1000d / usersTotal.Value
                            : null,
                    YearStatus = status,
                    IsComplete = complete
                });
            }
            divisions[division] = new AtlasSenTariffDivisionSeries
            {
                Division = division,
                Series = points
            };
        }

        var result = new AtlasSenTariffSeriesResponse
        {
            GeneratedUtc = DateTime.UtcNow,
            ReferenceYear = referenceYear,
            Years = years,
            Divisions = divisions
        };
        _cache.Set(
            TariffSeriesCacheKey,
            result,
            TimeSpan.FromMinutes(Math.Max(5, _options.CacheMinutes)));
        return result;
    }

    public async Task<AtlasSenDemandSnapshot> GetDemandAsync(
        CancellationToken cancellationToken = default)
    {
        if (_cache.TryGetValue<AtlasSenDemandSnapshot>(
                DemandCacheKey,
                out var cached) &&
            cached is not null)
        {
            return cached;
        }

        var raw = await GetDocumentAsync(
            AtlasSenDatasetKeys.Demand,
            cancellationToken);
        var snapshot = JsonSerializer.Deserialize<AtlasSenDemandSnapshot>(
                raw,
                JsonOptions) ??
            throw new JsonException(
                "La fotografía de demanda de Atlas SEN no es válida.");
        _cache.Set(
            DemandCacheKey,
            snapshot,
            LiveDataCacheDuration());
        return snapshot;
    }

    public async Task<AtlasSenGeoJson> GetDemandRegionsGeoJsonAsync(
        CancellationToken cancellationToken = default)
    {
        if (_cache.TryGetValue<AtlasSenGeoJson>(
                DemandGeoJsonCacheKey,
                out var cached) &&
            cached is not null)
        {
            return cached;
        }

        var atlas = await GetDocumentAsync(
            AtlasSenDatasetKeys.Atlas,
            cancellationToken);
        var demand = await GetDemandAsync(cancellationToken);
        using var document = JsonDocument.Parse(atlas);
        var regionPolygons = GetProperty(document.RootElement, "RP");
        var features = new List<AtlasSenGeoJsonFeature>();
        foreach (var region in demand.Regions)
        {
            if (string.Equals(
                    region.Key,
                    "Sistema Interconectado Nacional",
                    StringComparison.OrdinalIgnoreCase) ||
                regionPolygons.ValueKind != JsonValueKind.Object ||
                !regionPolygons.TryGetProperty(
                    region.Key,
                    out var geometrySource))
            {
                continue;
            }

            var latest = region.Value.Latest ??
                region.Value.Hourly.LastOrDefault(hour =>
                    hour.DemandMw.HasValue);
            if (latest is null)
            {
                continue;
            }

            features.Add(new AtlasSenGeoJsonFeature
            {
                Geometry = JsonSerializer.SerializeToElement(new
                {
                    type = "MultiPolygon",
                    coordinates =
                        ReverseLatLngCoordinates(geometrySource)
                }),
                Properties = new Dictionary<string, object?>
                {
                    ["region"] = region.Key,
                    ["gerencia"] = region.Value.ManagementId,
                    ["operating_date"] =
                        demand.OperatingDate.ToString("yyyy-MM-dd"),
                    ["updated_at"] = demand.UpdatedAt,
                    ["hour"] = latest.Hour,
                    ["demand_mw"] = latest.DemandMw,
                    ["generation_mw"] = latest.GenerationMw,
                    ["forecast_mw"] = latest.ForecastMw,
                    ["balance_mw"] =
                        latest.GenerationMw - latest.DemandMw,
                    ["forecast_deviation_mw"] =
                        latest.DemandMw - latest.ForecastMw,
                    ["source"] =
                        "CENACE · GraficaDemanda, compilado por Atlas SEN",
                    ["validation_state"] =
                        "actualizacion_automatica_fuente_publica"
                }
            });
        }

        var result = new AtlasSenGeoJson
        {
            Meta = new AtlasSenGeoJsonMeta
            {
                Layer = "demanda_regional_atlas_sen",
                GeneratedUtc = DateTime.UtcNow,
                Features = features.Count,
                Source =
                    "CENACE · GraficaDemanda, compilado por Atlas SEN",
                License = "Datos públicos CENACE",
                ValidationState =
                    "actualizacion_automatica_fuente_publica"
            },
            Features = features
        };
        _cache.Set(
            DemandGeoJsonCacheKey,
            result,
            LiveDataCacheDuration());
        return result;
    }

    public async Task<AtlasSenWeatherResponse?> GetWeatherAsync(
        string region,
        CancellationToken cancellationToken = default)
    {
        var resolvedRegion = DemandRegionWeatherPoints.Keys.FirstOrDefault(
            candidate => string.Equals(
                candidate,
                region?.Trim(),
                StringComparison.OrdinalIgnoreCase));
        if (resolvedRegion is null)
        {
            return null;
        }

        var demand = await GetDemandAsync(cancellationToken);
        var cacheKey =
            $"{WeatherCacheKeyPrefix}:{resolvedRegion}:{demand.OperatingDate:yyyy-MM-dd}";
        if (_cache.TryGetValue<AtlasSenWeatherResponse>(
                cacheKey,
                out var cached) &&
            cached is not null)
        {
            return cached;
        }

        var point = DemandRegionWeatherPoints[resolvedRegion];
        var date = demand.OperatingDate.ToString("yyyy-MM-dd");
        var query = string.Join(
            "&",
            $"latitude={point.Latitude.ToString(System.Globalization.CultureInfo.InvariantCulture)}",
            $"longitude={point.Longitude.ToString(System.Globalization.CultureInfo.InvariantCulture)}",
            "hourly=temperature_2m",
            "timezone=America%2FMexico_City",
            $"start_date={date}",
            $"end_date={date}");
        var separator = _options.WeatherUrl.Contains(
            '?',
            StringComparison.Ordinal)
            ? "&"
            : "?";
        var requestUri = $"{_options.WeatherUrl}{separator}{query}";
        var client = _httpClientFactory.CreateClient("AtlasSen");
        var raw = await client.GetStringAsync(
            requestUri,
            cancellationToken);
        using var document = JsonDocument.Parse(raw);
        var hourlySource = GetProperty(
            document.RootElement,
            "hourly");
        var times = GetProperty(hourlySource, "time");
        var temperatures = GetProperty(
            hourlySource,
            "temperature_2m");
        if (times.ValueKind != JsonValueKind.Array ||
            temperatures.ValueKind != JsonValueKind.Array)
        {
            throw new JsonException(
                "La respuesta meteorológica no contiene la serie horaria esperada.");
        }

        var timeValues = times.EnumerateArray().ToList();
        var temperatureValues = temperatures.EnumerateArray().ToList();
        var hours = new List<AtlasSenWeatherHour>();
        var length = Math.Min(
            timeValues.Count,
            temperatureValues.Count);
        for (var index = 0; index < length; index++)
        {
            var time = timeValues[index].GetString() ?? string.Empty;
            if (time.Length < 13 ||
                !int.TryParse(
                    time.AsSpan(11, 2),
                    out var hour))
            {
                continue;
            }

            var temperature = temperatureValues[index].ValueKind ==
                              JsonValueKind.Number &&
                              temperatureValues[index].TryGetDouble(
                                  out var value)
                ? value
                : (double?)null;
            hours.Add(new AtlasSenWeatherHour
            {
                Hour = hour,
                Time = time,
                TemperatureC = temperature
            });
        }

        var result = new AtlasSenWeatherResponse
        {
            Region = resolvedRegion,
            OperatingDate = demand.OperatingDate,
            Latitude = point.Latitude,
            Longitude = point.Longitude,
            Hourly = hours
        };
        _cache.Set(
            cacheKey,
            result,
            TimeSpan.FromMinutes(30));
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
            LiveDataCacheDuration());
        return snapshot;
    }

    public async Task<AtlasSenGeoJson> GetPrivateGenerationGeoJsonAsync(
        CancellationToken cancellationToken = default)
    {
        if (_cache.TryGetValue<AtlasSenGeoJson>(
                PrivateGenerationGeoJsonCacheKey,
                out var cached) &&
            cached is not null)
        {
            return cached;
        }

        var raw = await GetDocumentAsync(
            AtlasSenDatasetKeys.PrivateGeneration,
            cancellationToken);
        using var document = JsonDocument.Parse(raw);
        var root = document.RootElement;
        var projects = GetProperty(root, "projects");
        var source = ReadObjectString(root, "src");
        var note = ReadObjectString(root, "note");
        var features = new List<AtlasSenGeoJsonFeature>();
        if (projects.ValueKind == JsonValueKind.Array)
        {
            foreach (var project in projects.EnumerateArray())
            {
                var latitude = ReadObjectDouble(project, "lat");
                var longitude = ReadObjectDouble(project, "lng");
                if (!latitude.HasValue ||
                    !longitude.HasValue ||
                    latitude is < 14 or > 33.5 ||
                    longitude is < -118 or > -86)
                {
                    continue;
                }

                var precision = ReadObjectString(project, "prec");
                var firmness = ReadObjectString(project, "firm");
                features.Add(new AtlasSenGeoJsonFeature
                {
                    Geometry = JsonSerializer.SerializeToElement(new
                    {
                        type = "Point",
                        coordinates = new[]
                        {
                            longitude.Value,
                            latitude.Value
                        }
                    }),
                    Properties = new Dictionary<string, object?>
                    {
                        ["name"] = ReadObjectString(project, "n"),
                        ["developer"] = ReadObjectString(project, "dev"),
                        ["technology"] =
                            ReadObjectString(project, "t") switch
                            {
                                "pv" => "Fotovoltaica",
                                "wind" => "Eólica",
                                var value => value
                            },
                        ["capacity_mw"] =
                            ReadObjectDouble(project, "mw"),
                        ["storage_mw"] =
                            ReadObjectDouble(project, "st"),
                        ["state"] = ReadObjectString(project, "edo"),
                        ["municipality"] =
                            ReadObjectString(project, "mun"),
                        ["coordinate_precision"] = precision,
                        ["coordinate_reference"] = precision switch
                        {
                            "exact" => "MIA localizada",
                            "near" => "Referencia próxima",
                            "muni" => "Centroide municipal",
                            _ => "Sin clasificación"
                        },
                        ["project_status"] = firmness,
                        ["environmental_reference"] =
                            ReadObjectString(project, "mia"),
                        ["planned_cod"] =
                            ReadObjectInt(project, "cod"),
                        ["planning_context"] =
                            "1ª Convocatoria de Atención Prioritaria · planeación vinculante",
                        ["source"] =
                            string.IsNullOrWhiteSpace(source)
                                ? "CNE/SENER · compilado por Atlas SEN"
                                : $"{source} · compilado por Atlas SEN",
                        ["source_note"] = note,
                        ["validation_state"] =
                            precision == "exact"
                                ? "referencia_secundaria_geolocalizacion_documental"
                                : "referencia_secundaria_ubicacion_aproximada"
                    }
                });
            }
        }

        var result = new AtlasSenGeoJson
        {
            Meta = new AtlasSenGeoJsonMeta
            {
                Layer = "generacion_privada_planeada_atlas_sen",
                GeneratedUtc = DateTime.UtcNow,
                Features = features.Count,
                Source =
                    "CNE/SENER · 1ª Convocatoria de Atención Prioritaria, compilado por Atlas SEN",
                License = "Datos públicos; revisar fuente por proyecto",
                ValidationState =
                    "referencia_secundaria_no_sustituye_registro_de_permisos"
            },
            Features = features
        };
        _cache.Set(
            PrivateGenerationGeoJsonCacheKey,
            result,
            TimeSpan.FromMinutes(Math.Max(5, _options.CacheMinutes)));
        return result;
    }

    public async Task<AtlasSenDistributedGenerationResponse>
        GetDistributedGenerationAsync(
            CancellationToken cancellationToken = default)
    {
        if (_cache.TryGetValue<AtlasSenDistributedGenerationResponse>(
                DistributedGenerationCacheKey,
                out var cached) &&
            cached is not null)
        {
            return cached;
        }

        var stateTask = GetDocumentAsync(
            AtlasSenDatasetKeys.DistributedGenerationByState,
            cancellationToken);
        var sizeTask = GetDocumentAsync(
            AtlasSenDatasetKeys.DistributedGenerationBySize,
            cancellationToken);
        await Task.WhenAll(stateTask, sizeTask);

        using var stateDocument = JsonDocument.Parse(await stateTask);
        using var sizeDocument = JsonDocument.Parse(await sizeTask);
        var stateRoot = stateDocument.RootElement;
        var sizeRoot = sizeDocument.RootElement;
        var periods = ReadStringArray(stateRoot, "periods");
        var years = ReadStringArray(sizeRoot, "years");
        var referencePeriod = periods.LastOrDefault() ?? string.Empty;
        var referenceYear = years.LastOrDefault() ?? string.Empty;
        var macroRegions = GetProperty(stateRoot, "macroRegion");
        var statesSource = GetProperty(stateRoot, "byState");
        var states = new Dictionary<string, AtlasSenDistributedGenerationState>(
            StringComparer.OrdinalIgnoreCase);

        if (statesSource.ValueKind == JsonValueKind.Object)
        {
            foreach (var stateProperty in statesSource.EnumerateObject())
            {
                var series = new Dictionary<
                    string,
                    AtlasSenDistributedGenerationPoint>(
                    StringComparer.OrdinalIgnoreCase);
                foreach (var period in periods)
                {
                    var point = GetProperty(stateProperty.Value, period);
                    if (point.ValueKind != JsonValueKind.Object)
                    {
                        continue;
                    }
                    series[period] = new AtlasSenDistributedGenerationPoint
                    {
                        Mw = ReadObjectDouble(point, "mw"),
                        Contracts = ReadObjectInt(point, "contratos")
                    };
                }
                series.TryGetValue(referencePeriod, out var latest);
                states[stateProperty.Name] =
                    new AtlasSenDistributedGenerationState
                    {
                        Name = stateProperty.Name,
                        MacroRegion = ReadObjectString(
                            macroRegions,
                            stateProperty.Name),
                        Latest = latest,
                        Series = series
                    };
            }
        }

        var result = new AtlasSenDistributedGenerationResponse
        {
            GeneratedUtc = DateTime.UtcNow,
            ReferencePeriod = referencePeriod,
            ReferenceYear = referenceYear,
            Periods = periods,
            Years = years,
            States = states,
            CapacityTotalMw = ReadDoubleSeries(
                GetProperty(sizeRoot, "capacityMWp_total"),
                years),
            ContractsTotal = ReadIntSeries(
                GetProperty(sizeRoot, "contracts_total"),
                years),
            CapacityBySize = ReadNestedDoubleSeries(
                GetProperty(sizeRoot, "capacityMWp"),
                years),
            ContractsBySize = ReadNestedIntSeries(
                GetProperty(sizeRoot, "contracts"),
                years)
        };
        _cache.Set(
            DistributedGenerationCacheKey,
            result,
            TimeSpan.FromMinutes(Math.Max(5, _options.CacheMinutes)));
        return result;
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

        await GetStatusAsync(
            IsLiveDataset(key),
            cancellationToken);
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
            ValidateDatasetContent(definition, content);
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
        ValidateDatasetContent(definition, content);
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
            AtlasSenDatasetKeys.Demand => (
                GetObjectLength(document.RootElement, "regions"),
                ReadUpdatedUtc(document.RootElement)),
            AtlasSenDatasetKeys.Mda => (
                GetInt(document.RootElement, "count"),
                ReadUpdatedUtc(document.RootElement)),
            AtlasSenDatasetKeys.PrivateGeneration => (
                GetArrayLength(document.RootElement, "projects"),
                ReadUpdatedUtc(document.RootElement)),
            AtlasSenDatasetKeys.DistributedGenerationByState => (
                GetObjectLength(document.RootElement, "byState"),
                ReadUpdatedUtc(document.RootElement)),
            AtlasSenDatasetKeys.DistributedGenerationBySize => (
                GetObjectLength(document.RootElement, "capacityMWp"),
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
        var demand = datasets.Single(dataset =>
            dataset.Definition.Key == AtlasSenDatasetKeys.Demand);
        var mda = datasets.Single(dataset =>
            dataset.Definition.Key == AtlasSenDatasetKeys.Mda);
        var privateGeneration = datasets.Single(dataset =>
            dataset.Definition.Key ==
            AtlasSenDatasetKeys.PrivateGeneration);
        var distributedGeneration = datasets.Single(dataset =>
            dataset.Definition.Key ==
            AtlasSenDatasetKeys.DistributedGenerationByState);
        using var atlasDocument = JsonDocument.Parse(atlas.Content);
        using var osmDocument = JsonDocument.Parse(osm.Content);
        using var demandDocument = JsonDocument.Parse(demand.Content);
        using var mdaDocument = JsonDocument.Parse(mda.Content);
        using var privateGenerationDocument =
            JsonDocument.Parse(privateGeneration.Content);
        using var distributedGenerationDocument =
            JsonDocument.Parse(distributedGeneration.Content);
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
            DemandRegions = GetObjectLength(
                demandDocument.RootElement,
                "regions"),
            DemandUpdatedUtc = ReadUpdatedUtc(
                demandDocument.RootElement),
            DemandOperatingDate = ReadDateOnly(
                demandDocument.RootElement,
                "operatingDate"),
            MdaZones = GetInt(mdaDocument.RootElement, "count"),
            MdaUpdatedUtc = ReadUpdatedUtc(mdaDocument.RootElement),
            MdaOperatingDate = ReadDateOnly(
                mdaDocument.RootElement,
                "operatingDate"),
            PrivateGenerationProjects = GetArrayLength(
                privateGenerationDocument.RootElement,
                "projects"),
            DistributedGenerationStates = GetObjectLength(
                distributedGenerationDocument.RootElement,
                "byState"),
            DistributedGenerationReferencePeriod = ReadStringArray(
                    distributedGenerationDocument.RootElement,
                    "periods")
                .LastOrDefault() ?? string.Empty,
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
                DocumentCacheDuration(dataset.Definition.Key));
        }
    }

    private void ClearDerivedCaches()
    {
        _cache.Remove(SubstationsCacheKey);
        _cache.Remove(TariffGeoJsonCacheKey);
        _cache.Remove(TariffOverviewCacheKey);
        _cache.Remove(TariffSeriesCacheKey);
        _cache.Remove(DemandCacheKey);
        _cache.Remove(DemandGeoJsonCacheKey);
        _cache.Remove(MdaCacheKey);
        _cache.Remove(PrivateGenerationGeoJsonCacheKey);
        _cache.Remove(DistributedGenerationCacheKey);
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
                AtlasSenDatasetKeys.Demand,
                _options.DemandUrl,
                "data",
                "CENACE · GraficaDemanda, compilado por Atlas SEN",
                "Datos públicos CENACE",
                true),
            new AtlasSenDatasetDefinition(
                AtlasSenDatasetKeys.Mda,
                _options.MdaUrl,
                "data",
                "CENACE · SWPEND MDA, PND por zona de carga",
                "Datos públicos CENACE",
                true),
            new AtlasSenDatasetDefinition(
                AtlasSenDatasetKeys.PrivateGeneration,
                _options.PrivateGenerationUrl,
                "main",
                "CNE/SENER · 1ª Convocatoria de Atención Prioritaria, compilado por Atlas SEN",
                "Datos públicos; revisar fuente por proyecto",
                false),
            new AtlasSenDatasetDefinition(
                AtlasSenDatasetKeys.DistributedGenerationByState,
                _options.DistributedGenerationByStateUrl,
                "main",
                "CNE · Generación Distribuida y Limpia, compilado por Atlas SEN",
                "Datos públicos CNE",
                false),
            new AtlasSenDatasetDefinition(
                AtlasSenDatasetKeys.DistributedGenerationBySize,
                _options.DistributedGenerationBySizeUrl,
                "main",
                "CNE · Generación Distribuida y Limpia, compilado por Atlas SEN",
                "Datos públicos CNE",
                false)
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

    private TimeSpan LiveDataCacheDuration() =>
        TimeSpan.FromMinutes(
            Math.Max(1, _options.LiveDataCacheMinutes));

    private TimeSpan DocumentCacheDuration(string key) =>
        IsLiveDataset(key)
            ? LiveDataCacheDuration()
            : TimeSpan.FromMinutes(Math.Max(5, _options.CacheMinutes));

    private static bool IsLiveDataset(string key) =>
        string.Equals(
            key,
            AtlasSenDatasetKeys.Demand,
            StringComparison.Ordinal) ||
        string.Equals(
            key,
            AtlasSenDatasetKeys.Mda,
            StringComparison.Ordinal);

    private static string ResolveSnapshotDirectory(
        string configuredPath,
        string contentRootPath)
    {
        var value = string.IsNullOrWhiteSpace(configuredPath)
            ? "App_Data/cache/atlas-sen"
            : configuredPath.Trim();
        if (Path.IsPathRooted(value))
        {
            return Path.GetFullPath(value);
        }

        var appServiceInstance = Environment.GetEnvironmentVariable(
            "WEBSITE_INSTANCE_ID");
        var appServiceHome = Environment.GetEnvironmentVariable("HOME");
        if (!string.IsNullOrWhiteSpace(appServiceInstance) &&
            !string.IsNullOrWhiteSpace(appServiceHome))
        {
            return Path.GetFullPath(
                Path.Combine(
                    appServiceHome,
                    "data",
                    "atlas-sen"));
        }

        return Path.GetFullPath(
            Path.Combine(contentRootPath, value));
    }

    private static void ValidateDatasetContent(
        AtlasSenDatasetDefinition definition,
        string content)
    {
        using var document = JsonDocument.Parse(content);
        var root = document.RootElement;
        var valid = definition.Key switch
        {
            AtlasSenDatasetKeys.Atlas =>
                HasNonEmptyArray(root, "H") &&
                HasNonEmptyArray(root, "OL") &&
                HasNonEmptyObject(root, "TZ") &&
                HasNonEmptyObject(root, "RP"),
            AtlasSenDatasetKeys.OsmSubstations =>
                HasNonEmptyArray(root, "subs"),
            AtlasSenDatasetKeys.TariffUsers or
                AtlasSenDatasetKeys.TariffEnergy =>
                HasNonEmptyObject(root, "byDivision"),
            AtlasSenDatasetKeys.Demand =>
                HasNonEmptyObject(root, "regions") &&
                ReadDateOnly(root, "operatingDate").HasValue,
            AtlasSenDatasetKeys.Mda =>
                HasNonEmptyObject(root, "zonas") &&
                GetInt(root, "count") > 0 &&
                ReadDateOnly(root, "operatingDate").HasValue,
            AtlasSenDatasetKeys.PrivateGeneration =>
                HasNonEmptyArray(root, "projects"),
            AtlasSenDatasetKeys.DistributedGenerationByState =>
                HasNonEmptyArray(root, "periods") &&
                HasNonEmptyObject(root, "byState"),
            AtlasSenDatasetKeys.DistributedGenerationBySize =>
                HasNonEmptyArray(root, "years") &&
                HasNonEmptyObject(root, "capacityMWp") &&
                HasNonEmptyObject(root, "contracts"),
            _ => false
        };
        if (!valid)
        {
            throw new JsonException(
                $"El dataset Atlas SEN '{definition.Key}' no contiene la estructura mínima esperada.");
        }
    }

    private static bool HasNonEmptyArray(
        JsonElement root,
        string propertyName)
    {
        var value = GetProperty(root, propertyName);
        return value.ValueKind == JsonValueKind.Array &&
            value.GetArrayLength() > 0;
    }

    private static bool HasNonEmptyObject(
        JsonElement root,
        string propertyName)
    {
        var value = GetProperty(root, propertyName);
        return value.ValueKind == JsonValueKind.Object &&
            value.EnumerateObject().Any();
    }

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

    private static IReadOnlyList<string> ReadStringArray(
        JsonElement root,
        string propertyName)
    {
        var source = GetProperty(root, propertyName);
        return source.ValueKind == JsonValueKind.Array
            ? source.EnumerateArray()
                .Where(item => item.ValueKind == JsonValueKind.String)
                .Select(item => item.GetString() ?? string.Empty)
                .Where(item => !string.IsNullOrWhiteSpace(item))
                .ToArray()
            : Array.Empty<string>();
    }

    private static IReadOnlyDictionary<string, double?> ReadDoubleSeries(
        JsonElement source,
        IReadOnlyList<string> keys) =>
        keys.ToDictionary(
            key => key,
            key => ReadObjectDouble(source, key),
            StringComparer.OrdinalIgnoreCase);

    private static IReadOnlyDictionary<string, int?> ReadIntSeries(
        JsonElement source,
        IReadOnlyList<string> keys) =>
        keys.ToDictionary(
            key => key,
            key => ReadObjectInt(source, key),
            StringComparer.OrdinalIgnoreCase);

    private static IReadOnlyDictionary<
        string,
        IReadOnlyDictionary<string, double?>>
        ReadNestedDoubleSeries(
            JsonElement source,
            IReadOnlyList<string> keys)
    {
        var result = new Dictionary<
            string,
            IReadOnlyDictionary<string, double?>>(
            StringComparer.OrdinalIgnoreCase);
        if (source.ValueKind != JsonValueKind.Object)
        {
            return result;
        }
        foreach (var property in source.EnumerateObject())
        {
            result[property.Name] = ReadDoubleSeries(property.Value, keys);
        }
        return result;
    }

    private static IReadOnlyDictionary<
        string,
        IReadOnlyDictionary<string, int?>>
        ReadNestedIntSeries(
            JsonElement source,
            IReadOnlyList<string> keys)
    {
        var result = new Dictionary<
            string,
            IReadOnlyDictionary<string, int?>>(
            StringComparer.OrdinalIgnoreCase);
        if (source.ValueKind != JsonValueKind.Object)
        {
            return result;
        }
        foreach (var property in source.EnumerateObject())
        {
            result[property.Name] = ReadIntSeries(property.Value, keys);
        }
        return result;
    }

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

    private static string ReadObjectString(
        JsonElement element,
        string name)
    {
        var value = GetProperty(element, name);
        return value.ValueKind switch
        {
            JsonValueKind.String => value.GetString() ?? string.Empty,
            JsonValueKind.Number => value.GetRawText(),
            _ => string.Empty
        };
    }

    private static double? ReadObjectDouble(
        JsonElement element,
        string name)
    {
        var value = GetProperty(element, name);
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

    private static int? ReadObjectInt(
        JsonElement element,
        string name)
    {
        var value = ReadObjectDouble(element, name);
        return value.HasValue
            ? Convert.ToInt32(
                value.Value,
                CultureInfo.InvariantCulture)
            : null;
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

    private static string ResolveTariffReferenceYear(
        JsonElement usersRoot,
        JsonElement energyRoot)
    {
        var userYears = ReadStringArray(GetProperty(usersRoot, "years"))
            .ToHashSet(StringComparer.Ordinal);
        var energyYears = ReadStringArray(GetProperty(energyRoot, "years"));
        var status = GetProperty(
            GetProperty(energyRoot, "meta"),
            "year_status");
        var completeYears = energyYears
            .Where(userYears.Contains)
            .Where(year =>
                status.ValueKind != JsonValueKind.Object ||
                !status.TryGetProperty(year, out var state) ||
                string.Equals(
                    state.GetString(),
                    "ok_12_months",
                    StringComparison.OrdinalIgnoreCase))
            .OrderByDescending(year => year, StringComparer.Ordinal)
            .ToList();
        if (completeYears.Count > 0)
        {
            return completeYears[0];
        }

        return energyYears
            .Where(userYears.Contains)
            .OrderByDescending(year => year, StringComparer.Ordinal)
            .FirstOrDefault() ?? string.Empty;
    }

    private static double? SumTariffDivisionYear(
        JsonElement byDivision,
        string division,
        string year)
    {
        if (string.IsNullOrWhiteSpace(year) ||
            byDivision.ValueKind != JsonValueKind.Object ||
            !byDivision.TryGetProperty(division, out var tariffs) ||
            tariffs.ValueKind != JsonValueKind.Object)
        {
            return null;
        }

        var total = 0d;
        var hasValue = false;
        foreach (var tariff in tariffs.EnumerateObject())
        {
            if (tariff.Value.ValueKind != JsonValueKind.Object ||
                !tariff.Value.TryGetProperty(year, out var value))
            {
                continue;
            }

            if (value.ValueKind == JsonValueKind.Number &&
                value.TryGetDouble(out var number))
            {
                total += number;
                hasValue = true;
            }
        }
        return hasValue ? total : null;
    }

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
