namespace NSIE.Models.ProyectosPrivados;

// Expediente documental versionado por folio y fuente; no modifica Considerar.
public sealed class CarteraConvocatoriaExpediente
{
    public string Folio { get; set; } = "";
    public string FileName { get; set; } = "";
    public string Sha256 { get; set; } = "";
    public DateTime? CutoffDate { get; set; }
    public decimal? ExchangeRate { get; set; }
    public string ExchangeRateSource { get; set; } = "";
    public List<ConvocatoriaRegistroFuente> Records { get; set; } = new();
    public List<string> Warnings { get; set; } = new();
    public string? CalculatorFile { get; set; }
    // Polígonos declarados en la solicitud (vértices "lat, lon" de BD_MIXTOS_II); respaldo cuando no hay KML.
    public List<double[]>? ProjectPolygon { get; set; }
    public List<double[]>? SubstationPolygon { get; set; }
    // Requerimiento de capacidad por sistema (MAPA REQUERIMIENTOS): metadato del corte, igual para todos los folios.
    public List<CarteraConvocatoriaRequerimientoSistema> SystemRequirements { get; set; } = new();
    public string? Value(string sheet, string field) => Records.FirstOrDefault(r => r.Sheet == sheet)?.Fields.FirstOrDefault(c => c.Name == field)?.Value;
}

// Marcas de clúster y grupos excluyentes que CFE entrega en un libro aparte (puede provenir de un
// corte distinto al de la cartera); se guardan por folio sin alterar la consideración vigente.
public sealed class CarteraConvocatoriaMarca
{
    public string Folio { get; set; } = "";
    public string? Cluster { get; set; }
    public string? Excluyente1 { get; set; }
    public string? Excluyente2 { get; set; }
    public bool? Considerar { get; set; }
    public string FileName { get; set; } = "";
    public string Sha256 { get; set; } = "";
    public DateTime? LoadedUtc { get; set; }
    public string? LoadedBy { get; set; }
}

public sealed class CarteraConvocatoriaMarcasDocument
{
    public string FileName { get; set; } = "";
    public string Sha256 { get; set; } = "";
    public int Rows { get; set; }
    public List<CarteraConvocatoriaMarca> Marks { get; set; } = new();
}

public sealed class CarteraConvocatoriaRequerimientoSistema
{
    public string System { get; set; } = "";
    public decimal? RequirementMw { get; set; }
    public decimal? WindMw { get; set; }
    public decimal? SolarMw { get; set; }
    public decimal? FeasibleMw { get; set; }
    public decimal? ShortfallMw { get; set; }
    public decimal? PreselectedMw { get; set; }
    public decimal? TotalMw { get; set; }
}

public sealed class ConvocatoriaRegistroFuente
{
    public string Sheet { get; set; } = "";
    public int Row { get; set; }
    public List<ConvocatoriaCampoFuente> Fields { get; set; } = new();
}

public sealed class ConvocatoriaCampoFuente
{
    public string Name { get; set; } = "";
    public string Column { get; set; } = "";
    public string? Value { get; set; }
    public string? Link { get; set; }
}
