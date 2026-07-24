using System.Globalization;
using System.Text.Json;
using Microsoft.Extensions.Caching.Memory;
using Microsoft.Extensions.Options;
using NSIE.Models;

namespace NSIE.Servicios;

public sealed class InegiOptions
{
    public const string SectionName = "Inegi";
    public string Token { get; init; } = string.Empty;
}

public interface IInegiTerritorialService
{
    bool IsConfigured { get; }

    Task<InegiTerritorialResponse> GetAsync(
        string geographicCode,
        double? areaKm2,
        CancellationToken cancellationToken);
}

public sealed class InegiTerritorialService : IInegiTerritorialService
{
    private const string PopulationIndicatorId = "1002000002";
    private const string DenueSectorCodes =
        "11,21,22,23,31,32,33,43,46,48,49,51,52,53,54,55,56,61,62,71,72,81,93";

    private static readonly (string Key, string Label, string[] Codes)[] EconomicSectorGroups =
    {
        ("primary", "Agropecuario y extracción", new[] { "11", "21" }),
        ("industry", "Industria, energía y construcción", new[] { "22", "23", "31", "32", "33" }),
        ("commerce", "Comercio", new[] { "43", "46" }),
        ("transport", "Transporte e información", new[] { "48", "49", "51" }),
        ("business_services", "Servicios productivos", new[] { "52", "53", "54", "55", "56" }),
        ("social_services", "Servicios sociales y personales", new[] { "61", "62", "71", "72", "81", "93" })
    };

    private readonly HttpClient _httpClient;
    private readonly IMemoryCache _cache;
    private readonly InegiOptions _options;
    private readonly ILogger<InegiTerritorialService> _logger;

    public InegiTerritorialService(
        HttpClient httpClient,
        IMemoryCache cache,
        IOptions<InegiOptions> options,
        ILogger<InegiTerritorialService> logger)
    {
        _httpClient = httpClient;
        _cache = cache;
        _options = options.Value;
        _logger = logger;
    }

    public bool IsConfigured => !string.IsNullOrWhiteSpace(_options.Token);

    public async Task<InegiTerritorialResponse> GetAsync(
        string geographicCode,
        double? areaKm2,
        CancellationToken cancellationToken)
    {
        if (!IsConfigured)
        {
            throw new InvalidOperationException("La integración INEGI no tiene token configurado.");
        }

        var normalizedCode = geographicCode.Trim();
        var normalizedArea = areaKm2 is > 0
            ? Math.Round(areaKm2.Value, 2)
            : (double?)null;
        var cacheKey = $"inegi-territorial:{normalizedCode}:{normalizedArea?.ToString(CultureInfo.InvariantCulture) ?? "na"}";

        if (_cache.TryGetValue(cacheKey, out InegiTerritorialResponse? cached) && cached is not null)
        {
            return cached;
        }

        var populationTask = TryGetPopulationAsync(normalizedCode, cancellationToken);
        var establishmentsTask = TryGetEstablishmentsAsync(normalizedCode, cancellationToken);
        var sectorCountsTask = TryGetSectorCountsAsync(normalizedCode, cancellationToken);
        await Task.WhenAll(populationTask, establishmentsTask, sectorCountsTask);

        var population = await populationTask;
        var establishments = await establishmentsTask;
        var sectorCounts = await sectorCountsTask;
        if (population is null && establishments is null && sectorCounts is null)
        {
            throw new HttpRequestException("INEGI no devolvió indicadores para el ámbito solicitado.");
        }

        var indicators = new List<InegiTerritorialIndicator>();
        var breakdowns = new List<InegiTerritorialBreakdown>();
        if (population is not null)
        {
            var previousDetail = population.PreviousValue is > 0
                ? $"Dato previo: {population.PreviousValue.Value.ToString("N0", CultureInfo.GetCultureInfo("es-MX"))} personas ({population.PreviousPeriod})."
                : string.Empty;
            indicators.Add(new InegiTerritorialIndicator
            {
                Key = "population_total",
                Label = "Población total",
                Value = population.Value,
                Unit = "personas",
                Period = population.Period,
                Source = "INEGI · Banco de Indicadores (BISE)",
                Detail = previousDetail,
                Decimals = 0
            });

            if (population.PreviousValue is > 0)
            {
                indicators.Add(new InegiTerritorialIndicator
                {
                    Key = "population_change",
                    Label = "Variación de población",
                    Value = ((population.Value - population.PreviousValue.Value) / population.PreviousValue.Value) * 100d,
                    Unit = "%",
                    Period = $"{population.PreviousPeriod}-{population.Period}",
                    Source = "Cálculo DGMESNIE con datos INEGI",
                    Detail = "Cambio respecto del dato censal previo disponible.",
                    Decimals = 1
                });
            }

            if (normalizedArea is > 0)
            {
                indicators.Add(new InegiTerritorialIndicator
                {
                    Key = "population_density",
                    Label = "Densidad de población",
                    Value = population.Value / normalizedArea.Value,
                    Unit = "hab/km²",
                    Period = population.Period,
                    Source = "Cálculo DGMESNIE con datos INEGI",
                    Detail = $"Superficie geográfica aproximada: {normalizedArea.Value.ToString("N1", CultureInfo.GetCultureInfo("es-MX"))} km².",
                    Decimals = 1
                });
            }

            if (population.PreviousValue is > 0)
            {
                breakdowns.Add(new InegiTerritorialBreakdown
                {
                    Key = "population_trend",
                    Title = "Evolución de la población",
                    Subtitle = "Comparación entre los dos levantamientos censales disponibles.",
                    Unit = "personas",
                    Period = $"{population.PreviousPeriod}-{population.Period}",
                    Source = "INEGI · Banco de Indicadores (BISE)",
                    Decimals = 0,
                    Items = new[]
                    {
                        new InegiTerritorialBreakdownItem
                        {
                            Key = "previous",
                            Label = population.PreviousPeriod,
                            Value = population.PreviousValue.Value
                        },
                        new InegiTerritorialBreakdownItem
                        {
                            Key = "current",
                            Label = population.Period,
                            Value = population.Value
                        }
                    }
                });
            }
        }

        if (establishments is not null)
        {
            indicators.Add(new InegiTerritorialIndicator
            {
                Key = "economic_units",
                Label = "Unidades económicas",
                Value = establishments.Value,
                Unit = "establecimientos",
                Period = "DENUE vigente",
                Source = "INEGI · DENUE",
                Detail = "Conteo de establecimientos de todas las actividades y estratos.",
                Decimals = 0
            });

            if (population is not null && population.Value > 0)
            {
                indicators.Add(new InegiTerritorialIndicator
                {
                    Key = "economic_units_per_1000",
                    Label = "Unidades económicas por 1,000 habitantes",
                    Value = establishments.Value * 1000d / population.Value,
                    Unit = "por 1,000 hab.",
                    Period = "DENUE vigente",
                    Source = "Cálculo DGMESNIE con datos INEGI",
                    Detail = "Indicador de intensidad económica territorial.",
                    Decimals = 1
                });
            }

            if (normalizedArea is > 0)
            {
                indicators.Add(new InegiTerritorialIndicator
                {
                    Key = "economic_units_density",
                    Label = "Densidad de unidades económicas",
                    Value = establishments.Value / normalizedArea.Value,
                    Unit = "establecimientos/km²",
                    Period = "DENUE vigente",
                    Source = "Cálculo DGMESNIE con datos INEGI",
                    Detail = $"Superficie geográfica aproximada: {normalizedArea.Value.ToString("N1", CultureInfo.GetCultureInfo("es-MX"))} km².",
                    Decimals = 1
                });
            }
        }

        if (sectorCounts is not null)
        {
            var sectorItems = EconomicSectorGroups
                .Select(group => new InegiTerritorialBreakdownItem
                {
                    Key = group.Key,
                    Label = group.Label,
                    Value = group.Codes.Sum(code => sectorCounts.GetValueOrDefault(code))
                })
                .Where(item => item.Value > 0)
                .ToArray();

            if (sectorItems.Length > 0)
            {
                breakdowns.Add(new InegiTerritorialBreakdown
                {
                    Key = "economic_structure",
                    Title = "Estructura económica por sector",
                    Subtitle = "Distribución de establecimientos agrupados por sector SCIAN.",
                    Unit = "establecimientos",
                    Period = "DENUE vigente",
                    Source = "INEGI · DENUE",
                    Decimals = 0,
                    Items = sectorItems
                });
            }
        }

        var response = new InegiTerritorialResponse
        {
            GeographicCode = normalizedCode,
            Scope = normalizedCode.Length == 2 ? "Entidad federativa" : "Municipio",
            QueriedAt = DateTimeOffset.UtcNow,
            Indicators = indicators,
            Breakdowns = breakdowns
        };

        _cache.Set(cacheKey, response, new MemoryCacheEntryOptions
        {
            AbsoluteExpirationRelativeToNow = TimeSpan.FromHours(12),
            Size = null
        });
        return response;
    }

    private async Task<PopulationSnapshot?> TryGetPopulationAsync(
        string geographicCode,
        CancellationToken cancellationToken)
    {
        try
        {
            var token = Uri.EscapeDataString(_options.Token);
            // El Banco de Indicadores codifica una entidad como EEDD (p. ej. 0700);
            // DENUE usa sólo EE. Los municipios conservan la clave geoestadística EEMMM.
            var indicatorGeography = geographicCode.Length == 2
                ? geographicCode + "00"
                : geographicCode;
            var path =
                $"app/api/indicadores/desarrolladores/jsonxml/INDICATOR/{PopulationIndicatorId}/es/{indicatorGeography}/false/BISE/2.0/{token}?type=json";
            using var response = await _httpClient.GetAsync(path, cancellationToken);
            response.EnsureSuccessStatusCode();
            await using var stream = await response.Content.ReadAsStreamAsync(cancellationToken);
            using var document = await JsonDocument.ParseAsync(stream, cancellationToken: cancellationToken);
            return ParsePopulation(document.RootElement);
        }
        catch (Exception ex) when (ex is HttpRequestException or JsonException or TaskCanceledException)
        {
            _logger.LogWarning(ex, "No fue posible consultar la población INEGI para {GeographicCode}.", geographicCode);
            return null;
        }
    }

    private async Task<double?> TryGetEstablishmentsAsync(
        string geographicCode,
        CancellationToken cancellationToken)
    {
        try
        {
            var token = Uri.EscapeDataString(_options.Token);
            var path =
                $"app/api/denue/v1/consulta/Cuantificar/0/{geographicCode}/0/{token}";
            using var response = await _httpClient.GetAsync(path, cancellationToken);
            response.EnsureSuccessStatusCode();
            await using var stream = await response.Content.ReadAsStreamAsync(cancellationToken);
            using var document = await JsonDocument.ParseAsync(stream, cancellationToken: cancellationToken);
            return ParseEstablishmentCount(document.RootElement);
        }
        catch (Exception ex) when (ex is HttpRequestException or JsonException or TaskCanceledException)
        {
            _logger.LogWarning(ex, "No fue posible consultar DENUE para {GeographicCode}.", geographicCode);
            return null;
        }
    }

    private async Task<IReadOnlyDictionary<string, double>?> TryGetSectorCountsAsync(
        string geographicCode,
        CancellationToken cancellationToken)
    {
        try
        {
            var token = Uri.EscapeDataString(_options.Token);
            var path =
                $"app/api/denue/v1/consulta/Cuantificar/{DenueSectorCodes}/{geographicCode}/0/{token}";
            using var response = await _httpClient.GetAsync(path, cancellationToken);
            response.EnsureSuccessStatusCode();
            await using var stream = await response.Content.ReadAsStreamAsync(cancellationToken);
            using var document = await JsonDocument.ParseAsync(stream, cancellationToken: cancellationToken);
            return ParseSectorCounts(document.RootElement);
        }
        catch (Exception ex) when (ex is HttpRequestException or JsonException or TaskCanceledException)
        {
            _logger.LogWarning(ex, "No fue posible consultar la estructura sectorial DENUE para {GeographicCode}.", geographicCode);
            return null;
        }
    }

    private static PopulationSnapshot? ParsePopulation(JsonElement root)
    {
        if (!TryGetProperty(root, "Series", out var series) || series.ValueKind != JsonValueKind.Array)
        {
            return null;
        }

        var observations = new List<(string Period, double Value)>();
        foreach (var seriesItem in series.EnumerateArray())
        {
            if (!TryGetProperty(seriesItem, "OBSERVATIONS", out var items) ||
                items.ValueKind != JsonValueKind.Array)
            {
                continue;
            }

            foreach (var observation in items.EnumerateArray())
            {
                if (!TryGetProperty(observation, "TIME_PERIOD", out var periodElement) ||
                    !TryGetProperty(observation, "OBS_VALUE", out var valueElement) ||
                    !TryReadDouble(valueElement, out var value))
                {
                    continue;
                }

                var period = periodElement.ToString().Trim();
                if (!string.IsNullOrWhiteSpace(period))
                {
                    observations.Add((period, value));
                }
            }
        }

        var ordered = observations
            .OrderByDescending(item => PeriodSortKey(item.Period))
            .ThenByDescending(item => item.Period, StringComparer.OrdinalIgnoreCase)
            .ToList();
        if (ordered.Count == 0)
        {
            return null;
        }

        var latest = ordered[0];
        var previous = ordered.Skip(1).FirstOrDefault();
        return new PopulationSnapshot(
            latest.Value,
            latest.Period,
            previous == default ? null : previous.Value,
            previous == default ? string.Empty : previous.Period);
    }

    private static double? ParseEstablishmentCount(JsonElement root)
    {
        var objects = root.ValueKind == JsonValueKind.Array
            ? root.EnumerateArray().Where(item => item.ValueKind == JsonValueKind.Object).ToArray()
            : root.ValueKind == JsonValueKind.Object
                ? new[] { root }
                : Array.Empty<JsonElement>();
        if (objects.Length == 0)
        {
            return null;
        }

        double total = 0;
        var found = false;
        foreach (var item in objects)
        {
            var properties = item.EnumerateObject().ToArray();
            var preferred = properties.FirstOrDefault(property =>
                property.Name.Contains("total", StringComparison.OrdinalIgnoreCase) ||
                property.Name.Contains("establec", StringComparison.OrdinalIgnoreCase));
            if (!string.IsNullOrWhiteSpace(preferred.Name) && TryReadDouble(preferred.Value, out var value))
            {
                total += value;
                found = true;
                continue;
            }

            foreach (var property in properties.Reverse())
            {
                if (TryReadDouble(property.Value, out value))
                {
                    total += value;
                    found = true;
                    break;
                }
            }
        }

        return found ? total : null;
    }

    private static IReadOnlyDictionary<string, double>? ParseSectorCounts(JsonElement root)
    {
        var objects = root.ValueKind == JsonValueKind.Array
            ? root.EnumerateArray().Where(item => item.ValueKind == JsonValueKind.Object).ToArray()
            : root.ValueKind == JsonValueKind.Object
                ? new[] { root }
                : Array.Empty<JsonElement>();
        if (objects.Length == 0)
        {
            return null;
        }

        var counts = new Dictionary<string, double>(StringComparer.OrdinalIgnoreCase);
        foreach (var item in objects)
        {
            if (!TryGetProperty(item, "AE", out var activityElement) ||
                !TryGetProperty(item, "Total", out var totalElement) ||
                !TryReadDouble(totalElement, out var total))
            {
                continue;
            }

            var activity = activityElement.ToString().Trim();
            if (string.IsNullOrWhiteSpace(activity))
            {
                continue;
            }

            counts[activity] = counts.GetValueOrDefault(activity) + total;
        }

        return counts.Count > 0 ? counts : null;
    }

    private static bool TryGetProperty(JsonElement element, string name, out JsonElement value)
    {
        if (element.ValueKind == JsonValueKind.Object)
        {
            foreach (var property in element.EnumerateObject())
            {
                if (string.Equals(property.Name, name, StringComparison.OrdinalIgnoreCase))
                {
                    value = property.Value;
                    return true;
                }
            }
        }

        value = default;
        return false;
    }

    private static bool TryReadDouble(JsonElement element, out double value)
    {
        if (element.ValueKind == JsonValueKind.Number && element.TryGetDouble(out value))
        {
            return true;
        }

        if (element.ValueKind == JsonValueKind.String)
        {
            var text = (element.GetString() ?? string.Empty).Trim().Replace(",", string.Empty);
            return double.TryParse(
                text,
                NumberStyles.Float | NumberStyles.AllowLeadingSign,
                CultureInfo.InvariantCulture,
                out value);
        }

        value = 0;
        return false;
    }

    private static long PeriodSortKey(string period)
    {
        var digits = new string(period.Where(char.IsDigit).ToArray());
        return long.TryParse(digits, NumberStyles.None, CultureInfo.InvariantCulture, out var key)
            ? key
            : 0;
    }

    private sealed record PopulationSnapshot(
        double Value,
        string Period,
        double? PreviousValue,
        string PreviousPeriod);
}
