using System.Text.Json;

namespace NSIE.Models;

public sealed class PamTerritorialProyecto
{
    public long ProyectoId { get; init; }
    public string ClaveProyecto { get; init; } = string.Empty;
    public string ClavesAlternas { get; init; } = string.Empty;
    public string NombreProyecto { get; init; } = string.Empty;
    public string Gcr { get; init; } = string.Empty;
    public string TipoProyecto { get; init; } = string.Empty;
    public string EtapaProyecto { get; init; } = string.Empty;
    public string EstadoVigenciaCartera { get; init; } = string.Empty;
    public string EstatusLicitacion { get; init; } = string.Empty;
    public string ZonaAtendida { get; init; } = string.Empty;
    public string ElementosEquiposAsociados { get; init; } = string.Empty;
    public string FuenteDocumento { get; init; } = string.Empty;
    public DateTime? FechaCorte { get; init; }
    public IReadOnlyList<PamTerritorialUbicacion> Ubicaciones { get; init; } =
        Array.Empty<PamTerritorialUbicacion>();

    public bool TieneUbicacionValidada => Ubicaciones.Any(location => location.Validada);
    public string FichaUrl => string.IsNullOrWhiteSpace(ClaveProyecto)
        ? string.Empty
        : $"/InformePormenorizado/ProyectosIdentificados/Ficha/{Uri.EscapeDataString(ClaveProyecto)}";
}

public sealed class PamTerritorialUbicacion
{
    public long UbicacionId { get; init; }
    public long ProyectoId { get; init; }
    public string Etiqueta { get; init; } = string.Empty;
    public string TipoGeometria { get; init; } = string.Empty;
    public JsonElement? Geometria { get; init; }
    public double? Latitud { get; init; }
    public double? Longitud { get; init; }
    public string Direccion { get; init; } = string.Empty;
    public string Entidad { get; init; } = string.Empty;
    public string Municipio { get; init; } = string.Empty;
    public string Localidad { get; init; } = string.Empty;
    public string PrecisionUbicacion { get; init; } = string.Empty;
    public string MetodoUbicacion { get; init; } = string.Empty;
    public string Fuente { get; init; } = string.Empty;
    public DateTime? FechaCorte { get; init; }
    public double RadioSugeridoKm { get; init; }
    public int Orden { get; init; }
    public bool EsPrincipal { get; init; }
    public bool Validada { get; init; }
    public bool EsAsociacionSugerida { get; init; }
    public int? PuntajeCoincidencia { get; init; }
    public string NivelConfianza { get; init; } = string.Empty;
    public string TipoElementoRed { get; init; } = string.Empty;
    public string ClaveElementoRed { get; init; } = string.Empty;
    public IReadOnlyList<string> Evidencias { get; init; } = Array.Empty<string>();
}

public sealed class PamTerritorialGeoJson
{
    public string Type { get; init; } = "FeatureCollection";
    public required PamTerritorialGeoJsonMeta Meta { get; init; }
    public required IReadOnlyList<PamTerritorialFeature> Features { get; init; }
}

public sealed class PamTerritorialGeoJsonMeta
{
    public string Fuente { get; init; } =
        "DGMESNIE · PAM/PAMRNT · ubicaciones validadas y asociaciones de red de confianza alta";
    public DateTime GeneradoUtc { get; init; } = DateTime.UtcNow;
    public int CatalogoProyectos { get; init; }
    public int Proyectos { get; init; }
    public int Ubicaciones { get; init; }
    public int ProyectosValidados { get; init; }
    public int ProyectosAsociadosRed { get; init; }
    public int ProyectosAsociadosConvocatoria { get; init; }
    public int AsociacionesSugeridas { get; init; }
    public int AsociacionesConvocatoria { get; init; }
}

public sealed class PamTerritorialFeature
{
    public string Type { get; init; } = "Feature";
    public required JsonElement Geometry { get; init; }
    public required PamTerritorialFeatureProperties Properties { get; init; }
}

public sealed class PamTerritorialFeatureProperties
{
    public long ProyectoId { get; init; }
    public long UbicacionId { get; init; }
    public string ClaveProyecto { get; init; } = string.Empty;
    public string NombreProyecto { get; init; } = string.Empty;
    public string Gcr { get; init; } = string.Empty;
    public string TipoProyecto { get; init; } = string.Empty;
    public string EtapaProyecto { get; init; } = string.Empty;
    public string EstatusLicitacion { get; init; } = string.Empty;
    public string ZonaAtendida { get; init; } = string.Empty;
    public string ElementosEquiposAsociados { get; init; } = string.Empty;
    public string EtiquetaUbicacion { get; init; } = string.Empty;
    public string TipoGeometria { get; init; } = string.Empty;
    public string PrecisionUbicacion { get; init; } = string.Empty;
    public string MetodoUbicacion { get; init; } = string.Empty;
    public string Direccion { get; init; } = string.Empty;
    public string Entidad { get; init; } = string.Empty;
    public string Municipio { get; init; } = string.Empty;
    public string Fuente { get; init; } = string.Empty;
    public DateTime? FechaCorte { get; init; }
    public double RadioSugeridoKm { get; init; }
    public double? Latitud { get; init; }
    public double? Longitud { get; init; }
    public string FichaUrl { get; init; } = string.Empty;
    public bool Validada { get; init; }
    public bool EsAsociacionSugerida { get; init; }
    public int? PuntajeCoincidencia { get; init; }
    public string NivelConfianza { get; init; } = string.Empty;
    public string TipoElementoRed { get; init; } = string.Empty;
    public string ClaveElementoRed { get; init; } = string.Empty;
    public IReadOnlyList<string> Evidencias { get; init; } = Array.Empty<string>();
}

public sealed class PamRedAssociationResult
{
    public long ProyectoId { get; init; }
    public string ClaveProyecto { get; init; } = string.Empty;
    public string GcrProyecto { get; init; } = string.Empty;
    public string VersionReglas { get; init; } = "PAM-RED-v1.0";
    public string Estado { get; init; } = "sin_coincidencias";
    public int ConfianzaAlta { get; init; }
    public int RequierenRevision { get; init; }
    public IReadOnlyList<PamRedAssociationCandidate> Candidatos { get; init; } =
        Array.Empty<PamRedAssociationCandidate>();
}

public sealed class PamRedAssociationCandidate
{
    public string TipoElementoRed { get; init; } = string.Empty;
    public string ClaveElementoRed { get; init; } = string.Empty;
    public string NombreElementoRed { get; init; } = string.Empty;
    public double? TensionKv { get; init; }
    public double? LongitudKm { get; init; }
    public int? Circuitos { get; init; }
    public string GcrCatalogo { get; init; } = string.Empty;
    public int Puntaje { get; init; }
    public string NivelConfianza { get; init; } = string.Empty;
    public IReadOnlyList<string> Evidencias { get; init; } = Array.Empty<string>();
    public required JsonElement Geometria { get; init; }
    public double? Latitud { get; init; }
    public double? Longitud { get; init; }
    public string Fuente { get; init; } = string.Empty;
}
