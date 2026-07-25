namespace NSIE.Models;

public sealed class TarifaTerritorialDetalle
{
    public string Division { get; init; } = string.Empty;
    public string Esquema { get; init; } = string.Empty;
    public string Tarifa { get; init; } = string.Empty;
    public string Sector { get; init; } = string.Empty;
    public string Concepto { get; init; } = string.Empty;
    public DateTime Periodo { get; init; }
    public string Unidades { get; init; } = string.Empty;
    public string Segmento { get; init; } = string.Empty;
    public string IntervaloHorario { get; init; } = string.Empty;
    public string NivelTension { get; init; } = string.Empty;
    public double? VariacionMensual { get; init; }
    public double? Valor { get; init; }
    public double? TarifaMediaNacional { get; init; }
}

public sealed class TarifasTerritorialesResumen
{
    public DateTime? Periodo { get; init; }
    public IReadOnlyList<string> Divisiones { get; init; } = [];
    public IReadOnlyList<TarifaTerritorialDetalle> Registros { get; init; } = [];
    public string Fuente { get; init; } = "CRE / CFE · Cuadros tarifarios históricos DGMESNIE";
    public string Nota { get; init; } =
        "Último histórico disponible en la base institucional; no representa una tarifa vigente.";
}
