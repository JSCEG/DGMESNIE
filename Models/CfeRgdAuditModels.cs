namespace NSIE.Models;

public static class CfeRgdAuditStates
{
    public const string Backed = "respaldada";
    public const string Review = "revision";
    public const string TerritoryMismatch = "territorio_incompatible";
    public const string NoMatch = "sin_coincidencia";
}

public sealed class CfeRgdSubstationAudit
{
    public string Estado { get; init; } = CfeRgdAuditStates.NoMatch;
    public bool NombreExacto { get; init; }
    public bool AliasControlado { get; init; }
    public bool TerritorioCompatible { get; init; }
    public bool TensionCompatible { get; init; }
    public bool RequiereValidacionHumana { get; init; } = true;
    public string Regla { get; init; } = "CFE-RGD-AUDIT-v1";
    public string Fuente { get; init; } = string.Empty;
    public string Sha256Fuente { get; init; } = string.Empty;
    public int AnioPublicacion { get; init; }
    public int AnioHorizonte { get; init; }
    public IReadOnlyList<string> Hallazgos { get; init; } =
        Array.Empty<string>();
    public IReadOnlyList<CfeRgdSubstationMatch> Coincidencias { get; init; } =
        Array.Empty<CfeRgdSubstationMatch>();
}

public sealed class CfeRgdSubstationMatch
{
    public string Division { get; init; } = string.Empty;
    public string Zona { get; init; } = string.Empty;
    public string Subestacion { get; init; } = string.Empty;
    public string Banco { get; init; } = string.Empty;
    public double TensionAtKv { get; init; }
    public double TensionMtKv { get; init; }
    public double CapacidadMva { get; init; }
    public int PaginaFuente { get; init; }
    public bool TensionCompatible { get; init; }
}
