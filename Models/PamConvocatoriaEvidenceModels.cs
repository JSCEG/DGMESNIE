using System.Text.Json;

namespace NSIE.Models;

public sealed class PamConvocatoriaEvidenceResult
{
    public long ProyectoId { get; init; }
    public string ClaveProyecto { get; init; } = string.Empty;
    public string VersionReglas { get; init; } = "PAM-CONV2-v1.0";
    public string Estado { get; init; } = "sin_coincidencias";
    public int CoincidenciasAltas { get; init; }
    public int RequierenRevision { get; init; }
    public IReadOnlyList<PamConvocatoriaProjectMatch> Coincidencias { get; init; } =
        Array.Empty<PamConvocatoriaProjectMatch>();
}

public sealed class PamConvocatoriaProjectMatch
{
    public string PreFolio { get; init; } = string.Empty;
    public string FolioProyecto { get; init; } = string.Empty;
    public string ProyectoConvocatoria { get; init; } = string.Empty;
    public string TipoCoincidencia { get; init; } = string.Empty;
    public int Puntaje { get; init; }
    public string NivelConfianza { get; init; } = string.Empty;
    public string Gcr { get; init; } = string.Empty;
    public string Entidad { get; init; } = string.Empty;
    public string Municipio { get; init; } = string.Empty;
    public string SubestacionDeclarada { get; init; } = string.Empty;
    public string PuntoInterconexion { get; init; } = string.Empty;
    public double? TensionKv { get; init; }
    public double? DistanciaDeclaradaKm { get; init; }
    public string EstadoElementoRed { get; init; } = string.Empty;
    public string ElementoRedCatalogado { get; init; } = string.Empty;
    public string TipoElementoRed { get; init; } = string.Empty;
    public string ClaveElementoRed { get; init; } = string.Empty;
    public JsonElement? GeometriaSugerida { get; init; }
    public double? Latitud { get; init; }
    public double? Longitud { get; init; }
    public string KmlProyecto { get; init; } = string.Empty;
    public string KmlSubestacion { get; init; } = string.Empty;
    public string FechaDecision { get; init; } = string.Empty;
    public IReadOnlyList<string> Evidencias { get; init; } = Array.Empty<string>();
    public string Fuente { get; init; } = string.Empty;
}

public sealed class PamConvocatoriaCoverageReport
{
    public string VersionReglas { get; init; } = "PAM-CONV2-v1.0";
    public DateTime GeneradoUtc { get; init; } = DateTime.UtcNow;
    public int ProyectosContinuan { get; init; }
    public int ReferenciasSubestacion { get; init; }
    public int SubestacionesCatalogadas { get; init; }
    public int SubestacionesRevision { get; init; }
    public int SubestacionesFaltantes { get; init; }
    public int ReferenciasLinea { get; init; }
    public int LineasCatalogadas { get; init; }
    public int LineasRevision { get; init; }
    public int LineasSinGeometria { get; init; }
    public string Fuente { get; init; } = string.Empty;
    public IReadOnlyList<PamConvocatoriaInfrastructureCandidate> Candidatos { get; init; } =
        Array.Empty<PamConvocatoriaInfrastructureCandidate>();
}

public sealed class PamConvocatoriaInfrastructureCandidate
{
    public string CandidatoId { get; init; } = string.Empty;
    public string TipoElemento { get; init; } = string.Empty;
    public string Estado { get; init; } = string.Empty;
    public string NombreDeclarado { get; init; } = string.Empty;
    public string CoincidenciaCatalogo { get; init; } = string.Empty;
    public string ClaveCatalogo { get; init; } = string.Empty;
    public int Puntaje { get; init; }
    public double? TensionKv { get; init; }
    public double? DistanciaCatalogoKm { get; init; }
    public string Gcr { get; init; } = string.Empty;
    public string Entidad { get; init; } = string.Empty;
    public string Municipio { get; init; } = string.Empty;
    public int ProyectosRelacionados { get; init; }
    public IReadOnlyList<string> Folios { get; init; } = Array.Empty<string>();
    public IReadOnlyList<string> Evidencias { get; init; } = Array.Empty<string>();
    public JsonElement? Geometria { get; init; }
    public double? Latitud { get; init; }
    public double? Longitud { get; init; }
    public string Fuente { get; init; } = string.Empty;
}

public sealed class PamConvocatoriaGeoJson
{
    public string Type { get; init; } = "FeatureCollection";
    public required PamConvocatoriaGeoJsonMeta Meta { get; init; }
    public required IReadOnlyList<PamConvocatoriaGeoJsonFeature> Features { get; init; }
}

public sealed class PamConvocatoriaGeoJsonMeta
{
    public string Version { get; init; } = "PAM-CONV2-v1.0";
    public string Layer { get; init; } = "evidencia_segunda_convocatoria";
    public DateTime GeneratedUtc { get; init; } = DateTime.UtcNow;
    public int Projects { get; init; }
    public int Candidates { get; init; }
    public int MissingSubstations { get; init; }
    public int ReviewSubstations { get; init; }
    public int MissingLines { get; init; }
    public string Source { get; init; } = string.Empty;
}

public sealed class PamConvocatoriaGeoJsonFeature
{
    public string Type { get; init; } = "Feature";
    public required JsonElement Geometry { get; init; }
    public required IReadOnlyDictionary<string, object?> Properties { get; init; }
}
