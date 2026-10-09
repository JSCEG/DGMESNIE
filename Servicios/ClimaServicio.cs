using Microsoft.Extensions.Caching.Memory;
using System.Globalization;
using System.Text.Json;
using System.Text.Json.Nodes;

namespace NSIE.Servicios;

/// <summary>
/// Clima para el mapa de infraestructura: huracanes activos del National Hurricane
/// Center (NOAA) y viento en superficie de Open-Meteo. Ambas fuentes son gratuitas y
/// sin llave; se consultan desde el servidor (NHC no permite CORS en su JSON) y se
/// guardan en caché para no saturarlas.
/// </summary>
public interface IClimaServicio
{
    Task<JsonObject> ObtenerHuracanesAsync(CancellationToken cancellationToken);

    Task<JsonArray> ObtenerVientoAsync(double minLat, double minLon, double maxLat, double maxLon, CancellationToken cancellationToken);
}

public sealed class ClimaServicio : IClimaServicio
{
    private const string UrlTormentas = "https://www.nhc.noaa.gov/CurrentStorms.json";
    private const string UrlServicioNhc = "https://mapservices.weather.noaa.gov/tropical/rest/services/tropical/NHC_tropical_weather/MapServer";
    private const string UrlOpenMeteo = "https://api.open-meteo.com/v1/forecast";

    // Capas del servicio NHC por tormenta ("AT4 Forecast Cone", "EP5 Past Track", …).
    private static readonly (string Sufijo, string Capa)[] CapasTormenta =
    {
        ("Forecast Cone", "cono"),
        ("Forecast Track", "pronostico"),
        ("Forecast Points", "punto"),
        ("Past Track", "historico")
    };

    private readonly HttpClient _http;
    private readonly IMemoryCache _cache;
    private readonly ILogger<ClimaServicio> _logger;

    public ClimaServicio(HttpClient http, IMemoryCache cache, ILogger<ClimaServicio> logger)
    {
        _http = http;
        _cache = cache;
        _logger = logger;
    }

    public async Task<JsonObject> ObtenerHuracanesAsync(CancellationToken cancellationToken)
    {
        if (_cache.TryGetValue("clima:huracanes", out JsonObject? cached) && cached is not null)
        {
            return cached;
        }

        var tormentasJson = await GetJsonAsync(UrlTormentas, cancellationToken);
        var activas = tormentasJson?["activeStorms"] as JsonArray ?? new JsonArray();
        var capasPorNombre = await ObtenerCapasNhcAsync(cancellationToken);

        var tormentas = new JsonArray();
        var features = new JsonArray();
        foreach (var nodo in activas.OfType<JsonObject>())
        {
            var bin = nodo["binNumber"]?.GetValue<string>() ?? string.Empty;
            var id = nodo["id"]?.GetValue<string>() ?? bin;
            tormentas.Add(new JsonObject
            {
                ["id"] = id,
                ["nombre"] = nodo["name"]?.GetValue<string>(),
                ["clasificacion"] = nodo["classification"]?.GetValue<string>(),
                ["intensidadKt"] = ParseInt(nodo["intensity"]),
                ["presionMb"] = ParseInt(nodo["pressure"]),
                ["latitud"] = nodo["latitudeNumeric"]?.GetValue<double>(),
                ["longitud"] = nodo["longitudeNumeric"]?.GetValue<double>(),
                ["rumbo"] = ParseInt(nodo["movementDir"]),
                ["velocidadMph"] = ParseInt(nodo["movementSpeed"]),
                ["actualizado"] = nodo["lastUpdate"]?.GetValue<string>(),
                ["aviso"] = (nodo["publicAdvisory"] as JsonObject)?["url"]?.GetValue<string>()
            });

            var consultas = CapasTormenta
                .Select(c => (c.Capa, Id: capasPorNombre.TryGetValue($"{bin} {c.Sufijo}", out var layerId) ? layerId : (int?)null))
                .Where(c => c.Id is not null)
                .Select(async c => (c.Capa, Datos: await GetJsonAsync(
                    $"{UrlServicioNhc}/{c.Id}/query?where=1%3D1&outFields=*&f=geojson", cancellationToken)))
                .ToList();

            foreach (var (capa, datos) in await Task.WhenAll(consultas))
            {
                foreach (var feature in (datos?["features"] as JsonArray ?? new JsonArray()).OfType<JsonObject>())
                {
                    var copia = (JsonObject)feature.DeepClone();
                    var props = copia["properties"] as JsonObject ?? new JsonObject();
                    props["capa"] = capa;
                    props["tormentaId"] = id;
                    copia["properties"] = props;
                    features.Add(copia);
                }
            }
        }

        var resultado = new JsonObject
        {
            ["fuente"] = "NOAA · National Hurricane Center",
            ["consultado"] = DateTime.UtcNow.ToString("o", CultureInfo.InvariantCulture),
            ["tormentas"] = tormentas,
            ["type"] = "FeatureCollection",
            ["features"] = features
        };
        _cache.Set("clima:huracanes", resultado, TimeSpan.FromMinutes(10));
        return resultado;
    }

    public async Task<JsonArray> ObtenerVientoAsync(double minLat, double minLon, double maxLat, double maxLon, CancellationToken cancellationToken)
    {
        // Malla de 7 × 7 puntos sobre la vista, redondeada a 0.25° para que vistas
        // parecidas compartan caché y no se gaste la cuota de Open-Meteo.
        static double R(double v) => Math.Round(v * 4) / 4;
        minLat = R(minLat); maxLat = R(maxLat); minLon = R(minLon); maxLon = R(maxLon);
        var clave = FormattableString.Invariant($"clima:viento:{minLat}:{minLon}:{maxLat}:{maxLon}");
        if (_cache.TryGetValue(clave, out JsonArray? cached) && cached is not null)
        {
            return cached;
        }

        const int n = 7;
        var lats = new List<double>();
        var lons = new List<double>();
        for (var i = 0; i < n; i++)
        {
            for (var j = 0; j < n; j++)
            {
                lats.Add(Math.Round(minLat + (maxLat - minLat) * (i + .5) / n, 3));
                lons.Add(Math.Round(minLon + (maxLon - minLon) * (j + .5) / n, 3));
            }
        }

        var url = $"{UrlOpenMeteo}?latitude={string.Join(',', lats.Select(v => v.ToString(CultureInfo.InvariantCulture)))}" +
                  $"&longitude={string.Join(',', lons.Select(v => v.ToString(CultureInfo.InvariantCulture)))}" +
                  "&current=wind_speed_10m,wind_direction_10m,wind_gusts_10m&wind_speed_unit=kmh&timezone=auto";
        var datos = await GetJsonAsync(url, cancellationToken);
        var lista = datos as JsonArray ?? (datos is JsonObject unico ? new JsonArray(unico.DeepClone()) : new JsonArray());

        var puntos = new JsonArray();
        foreach (var punto in lista.OfType<JsonObject>())
        {
            var actual = punto["current"] as JsonObject;
            if (actual is null) continue;
            puntos.Add(new JsonObject
            {
                ["lat"] = punto["latitude"]?.GetValue<double>(),
                ["lon"] = punto["longitude"]?.GetValue<double>(),
                ["velocidad"] = actual["wind_speed_10m"]?.GetValue<double>(),
                ["direccion"] = actual["wind_direction_10m"]?.GetValue<double>(),
                ["rafagas"] = actual["wind_gusts_10m"]?.GetValue<double>(),
                ["hora"] = actual["time"]?.GetValue<string>()
            });
        }

        _cache.Set(clave, puntos, TimeSpan.FromMinutes(15));
        return puntos;
    }

    private async Task<Dictionary<string, int>> ObtenerCapasNhcAsync(CancellationToken cancellationToken)
    {
        if (_cache.TryGetValue("clima:capas-nhc", out Dictionary<string, int>? cached) && cached is not null)
        {
            return cached;
        }

        var servicio = await GetJsonAsync($"{UrlServicioNhc}?f=json", cancellationToken);
        var capas = (servicio?["layers"] as JsonArray ?? new JsonArray())
            .OfType<JsonObject>()
            .Where(l => l["name"] is not null && l["id"] is not null)
            .GroupBy(l => l["name"]!.GetValue<string>(), StringComparer.OrdinalIgnoreCase)
            .ToDictionary(g => g.Key, g => g.First()["id"]!.GetValue<int>(), StringComparer.OrdinalIgnoreCase);
        _cache.Set("clima:capas-nhc", capas, TimeSpan.FromHours(6));
        return capas;
    }

    private async Task<JsonNode?> GetJsonAsync(string url, CancellationToken cancellationToken)
    {
        try
        {
            using var response = await _http.GetAsync(url, cancellationToken);
            response.EnsureSuccessStatusCode();
            await using var stream = await response.Content.ReadAsStreamAsync(cancellationToken);
            return await JsonNode.ParseAsync(stream, cancellationToken: cancellationToken);
        }
        catch (Exception ex) when (ex is HttpRequestException or JsonException or TaskCanceledException && !cancellationToken.IsCancellationRequested)
        {
            _logger.LogWarning(ex, "No se pudo consultar {Url}", url);
            return null;
        }
    }

    private static int? ParseInt(JsonNode? node)
    {
        if (node is null) return null;
        if (node is JsonValue value)
        {
            if (value.TryGetValue<int>(out var i)) return i;
            if (value.TryGetValue<double>(out var d)) return (int)Math.Round(d);
            if (value.TryGetValue<string>(out var s) && int.TryParse(s, NumberStyles.Integer, CultureInfo.InvariantCulture, out var p)) return p;
        }
        return null;
    }
}
