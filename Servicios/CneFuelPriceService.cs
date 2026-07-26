using System.Globalization;
using System.Xml.Linq;
using Microsoft.Extensions.Caching.Memory;
using NSIE.Models;

namespace NSIE.Servicios;

public interface ICneFuelPriceService
{
    Task<CneFuelPriceResponse> GetAsync(
        double minLat,
        double minLon,
        double maxLat,
        double maxLon,
        CancellationToken cancellationToken);
}

public sealed class CneFuelPriceService : ICneFuelPriceService
{
    private const string CacheKey = "cne-fuel-price-snapshot-v1";
    private static readonly SemaphoreSlim RefreshLock = new(1, 1);
    private static readonly TimeSpan Freshness = TimeSpan.FromHours(8);

    private readonly HttpClient _httpClient;
    private readonly IMemoryCache _cache;
    private readonly ILogger<CneFuelPriceService> _logger;

    public CneFuelPriceService(
        HttpClient httpClient,
        IMemoryCache cache,
        ILogger<CneFuelPriceService> logger)
    {
        _httpClient = httpClient;
        _cache = cache;
        _logger = logger;
    }

    public async Task<CneFuelPriceResponse> GetAsync(
        double minLat,
        double minLon,
        double maxLat,
        double maxLon,
        CancellationToken cancellationToken)
    {
        var cached = _cache.Get<CneFuelPriceSnapshot>(CacheKey);
        var snapshot = cached;
        var fallback = false;

        if (snapshot is null || DateTimeOffset.UtcNow - snapshot.FetchedAt > Freshness)
        {
            await RefreshLock.WaitAsync(cancellationToken);
            try
            {
                cached = _cache.Get<CneFuelPriceSnapshot>(CacheKey);
                if (cached is not null &&
                    DateTimeOffset.UtcNow - cached.FetchedAt <= Freshness)
                {
                    snapshot = cached;
                }
                else
                {
                    try
                    {
                        snapshot = await DownloadSnapshotAsync(cancellationToken);
                        _cache.Set(CacheKey, snapshot);
                    }
                    catch (Exception ex) when (
                        ex is HttpRequestException or
                        TaskCanceledException or
                        System.Xml.XmlException or
                        InvalidOperationException)
                    {
                        if (cached is null)
                        {
                            throw;
                        }

                        fallback = true;
                        snapshot = cached;
                        _logger.LogWarning(
                            ex,
                            "La actualización de precios CNE falló; se conserva el último corte válido de {FetchedAt}.",
                            cached.FetchedAt);
                    }
                }
            }
            finally
            {
                RefreshLock.Release();
            }
        }

        if (snapshot is null)
        {
            throw new HttpRequestException("La CNE no devolvió precios de combustibles.");
        }

        var stations = snapshot.Stations
            .Where(station =>
                station.Latitud >= minLat &&
                station.Latitud <= maxLat &&
                station.Longitud >= minLon &&
                station.Longitud <= maxLon)
            .ToArray();

        return new CneFuelPriceResponse
        {
            FechaConsulta = snapshot.FetchedAt,
            EsRespaldo = fallback,
            Estaciones = stations
        };
    }

    private async Task<CneFuelPriceSnapshot> DownloadSnapshotAsync(
        CancellationToken cancellationToken)
    {
        var placesTask = DownloadXmlAsync("publicaciones/places", cancellationToken);
        var pricesTask = DownloadXmlAsync("publicaciones/prices", cancellationToken);
        await Task.WhenAll(placesTask, pricesTask);

        var placesDocument = await placesTask;
        var pricesDocument = await pricesTask;
        var prices = ParsePrices(pricesDocument);
        var stations = ParsePlaces(placesDocument)
            .Select(place =>
            {
                prices.TryGetValue(place.PlaceId, out var stationPrices);
                return new CneFuelStationPrice
                {
                    PlaceId = place.PlaceId,
                    NumeroPermiso = place.NumeroPermiso,
                    Nombre = place.Nombre,
                    Latitud = place.Latitud,
                    Longitud = place.Longitud,
                    Regular = stationPrices?.Regular,
                    Premium = stationPrices?.Premium,
                    Diesel = stationPrices?.Diesel
                };
            })
            .Where(station =>
                station.Latitud is >= 14 and <= 33.5 &&
                station.Longitud is >= -118 and <= -86 &&
                (station.Regular is not null ||
                 station.Premium is not null ||
                 station.Diesel is not null))
            .ToArray();

        if (stations.Length == 0)
        {
            throw new InvalidOperationException("El feed CNE no contiene estaciones utilizables.");
        }

        return new CneFuelPriceSnapshot(DateTimeOffset.UtcNow, stations);
    }

    private async Task<XDocument> DownloadXmlAsync(
        string relativePath,
        CancellationToken cancellationToken)
    {
        using var response = await _httpClient.GetAsync(
            relativePath,
            HttpCompletionOption.ResponseHeadersRead,
            cancellationToken);
        response.EnsureSuccessStatusCode();
        await using var stream = await response.Content.ReadAsStreamAsync(cancellationToken);
        return await XDocument.LoadAsync(stream, LoadOptions.None, cancellationToken);
    }

    private static IReadOnlyList<CnePlace> ParsePlaces(XDocument document)
    {
        return document
            .Descendants("place")
            .Select(element =>
            {
                var idText = element.Attribute("place_id")?.Value;
                var longitudeText = element.Element("location")?.Element("x")?.Value;
                var latitudeText = element.Element("location")?.Element("y")?.Value;
                return int.TryParse(idText, NumberStyles.Integer, CultureInfo.InvariantCulture, out var placeId) &&
                       double.TryParse(longitudeText, NumberStyles.Float, CultureInfo.InvariantCulture, out var longitude) &&
                       double.TryParse(latitudeText, NumberStyles.Float, CultureInfo.InvariantCulture, out var latitude)
                    ? new CnePlace(
                        placeId,
                        (element.Element("cre_id")?.Value ?? string.Empty).Trim(),
                        (element.Element("name")?.Value ?? string.Empty).Trim(),
                        latitude,
                        longitude)
                    : null;
            })
            .Where(place => place is not null)
            .Cast<CnePlace>()
            .GroupBy(place => place.PlaceId)
            .Select(group => group.First())
            .ToArray();
    }

    private static IReadOnlyDictionary<int, CneStationPrices> ParsePrices(XDocument document)
    {
        var result = new Dictionary<int, CneStationPrices>();
        foreach (var element in document.Descendants("place"))
        {
            if (!int.TryParse(
                    element.Attribute("place_id")?.Value,
                    NumberStyles.Integer,
                    CultureInfo.InvariantCulture,
                    out var placeId))
            {
                continue;
            }

            if (!result.TryGetValue(placeId, out var stationPrices))
            {
                stationPrices = new CneStationPrices();
                result[placeId] = stationPrices;
            }

            foreach (var priceElement in element.Elements("gas_price"))
            {
                if (!decimal.TryParse(
                        priceElement.Value,
                        NumberStyles.Float,
                        CultureInfo.InvariantCulture,
                        out var price) ||
                    price <= 0)
                {
                    continue;
                }

                switch ((priceElement.Attribute("type")?.Value ?? string.Empty).Trim().ToLowerInvariant())
                {
                    case "regular":
                        stationPrices.Regular = price;
                        break;
                    case "premium":
                        stationPrices.Premium = price;
                        break;
                    case "diesel":
                        stationPrices.Diesel = price;
                        break;
                }
            }
        }

        return result;
    }

    private sealed record CneFuelPriceSnapshot(
        DateTimeOffset FetchedAt,
        IReadOnlyList<CneFuelStationPrice> Stations);

    private sealed record CnePlace(
        int PlaceId,
        string NumeroPermiso,
        string Nombre,
        double Latitud,
        double Longitud);

    private sealed class CneStationPrices
    {
        public decimal? Regular { get; set; }
        public decimal? Premium { get; set; }
        public decimal? Diesel { get; set; }
    }
}
