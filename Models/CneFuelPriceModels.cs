namespace NSIE.Models;

public sealed class CneFuelStationPrice
{
    public int PlaceId { get; init; }
    public string NumeroPermiso { get; init; } = string.Empty;
    public string Nombre { get; init; } = string.Empty;
    public double Latitud { get; init; }
    public double Longitud { get; init; }
    public decimal? Regular { get; init; }
    public decimal? Premium { get; init; }
    public decimal? Diesel { get; init; }
}

public sealed class CneFuelPriceResponse
{
    public DateTimeOffset FechaConsulta { get; init; }
    public bool EsRespaldo { get; init; }
    public string Fuente { get; init; } =
        "CNE · Precios reportados por permisionarios de estaciones de servicio";
    public IReadOnlyList<CneFuelStationPrice> Estaciones { get; init; } =
        Array.Empty<CneFuelStationPrice>();
}
