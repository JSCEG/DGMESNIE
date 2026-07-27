using System.Text.Json;

namespace NSIE.Models;

public sealed class PamConvocatoriaEvidenceResult
{
    public long ProyectoId { get; init; }
    public string ClaveProyecto { get; init; } = string.Empty;
    public string VersionReglas { get; init; } = "PAM-CONV2-v1.15";
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
    public string VersionReglas { get; init; } = "PAM-CONV2-v1.15";
    public DateTime GeneradoUtc { get; init; } = DateTime.UtcNow;
    public int ProyectosUniverso { get; init; }
    public int ProyectosConTrazabilidad { get; init; }
    public int ProyectosContinuan { get; init; }
    public int ProyectosOtrosEstatus { get; init; }
    public int ProyectosSinDecision { get; init; }
    public int ReferenciasSubestacion { get; init; }
    public int SubestacionesCatalogadas { get; init; }
    public int SubestacionesRevision { get; init; }
    public int SubestacionesFaltantes { get; init; }
    public int ReferenciasLinea { get; init; }
    public int LineasCatalogadas { get; init; }
    public int LineasCorredorCatalogado { get; init; }
    public int LineasRevision { get; init; }
    public int LineasSinGeometria { get; init; }
    public int LineasConectadasGrafo { get; init; }
    public int LineasParcialesGrafo { get; init; }
    public int LineasAmbiguasGrafo { get; init; }
    public int LineasSinResolverGrafo { get; init; }
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
    public int GeometriasSubestacionPrivada { get; init; }
    public int DistanciasDeclaradas { get; init; }
    public int DistanciasCompatibles { get; init; }
    public double? DistanciaInterconexionCalculadaMinimaKm { get; init; }
    public double? DistanciaInterconexionCalculadaMaximaKm { get; init; }
    public double? DiferenciaDistanciaMaximaKm { get; init; }
    public int ResolucionesCatalogoDistintas { get; init; }
    public bool NombreCatalogoUnico { get; init; }
    public bool NombreCoincidenciaFuerte { get; init; }
    public bool NombreEquivalenteCatalogo { get; init; }
    public double SimilitudNombre { get; init; }
    public int MargenPuntaje { get; init; }
    public string GcrCatalogo { get; init; } = string.Empty;
    public bool CoincidenciaTopologicaFirme { get; init; }
    public bool CoincidenciaTensionTopologicaFirme { get; init; }
    public IReadOnlyList<double> NivelesTensionRedKv { get; init; } =
        Array.Empty<double>();
    public bool HomonimoTerritorialDescartado { get; init; }
    public bool GrupoTerritorialSeparado { get; init; }
    public string ClaveAgrupacionTerritorial { get; init; } = string.Empty;
    public bool CoincidenciaFuenteGeorreferenciadaFirme { get; init; }
    public bool CoincidenciaInterconexionPublicaFirme { get; init; }
    public string NombreInterconexionPublica { get; init; } = string.Empty;
    public string CoincidenciaInterconexionCatalogo { get; init; } = string.Empty;
    public string ClaveInterconexionCatalogo { get; init; } = string.Empty;
    public double? DistanciaInterconexionPublicaKm { get; init; }
    public int FilasFuenteGeorreferenciada { get; init; }
    public double? SeparacionMaximaFuenteKm { get; init; }
    public IReadOnlyList<string> SoportesFuente { get; init; } =
        Array.Empty<string>();
    public bool CoincidenciaCorroboracionMultifuenteFirme { get; init; }
    public int ProyectosIndependientesCorroborados { get; init; }
    public IReadOnlyList<string> SoportesCorroboracion { get; init; } =
        Array.Empty<string>();
    public bool CoincidenciaAnexoTecnicoFirme { get; init; }
    public string ModoAnexoTecnico { get; init; } = string.Empty;
    public string FuenteAnexoTecnico { get; init; } = string.Empty;
    public string Sha256AnexoTecnico { get; init; } = string.Empty;
    public IReadOnlyList<string> SoportesAnexoTecnico { get; init; } =
        Array.Empty<string>();
    public bool CoincidenciaAutomaticaFirme { get; init; }
    public string MotivoAutomatizacion { get; init; } = string.Empty;
    public IReadOnlyList<string> LineasSoporte { get; init; } =
        Array.Empty<string>();
    public string EstadoConexionGrafo { get; init; } = string.Empty;
    public bool CoincidenciaGrafoFirme { get; init; }
    public int ExtremosResueltosGrafo { get; init; }
    public string ExtremoADeclarado { get; init; } = string.Empty;
    public string ExtremoBDeclarado { get; init; } = string.Empty;
    public string ExtremoACatalogo { get; init; } = string.Empty;
    public string ExtremoBCatalogo { get; init; } = string.Empty;
    public double? DistanciaExtremoAKm { get; init; }
    public double? DistanciaExtremoBKm { get; init; }
    public IReadOnlyList<string> SoportesGrafo { get; init; } =
        Array.Empty<string>();
    public string ModoCoincidenciaLinea { get; init; } = string.Empty;
    public bool CoincidenciaParExtremosFirme { get; init; }
    public bool CoincidenciaCodigoCircuitoFirme { get; init; }
    public IReadOnlyList<string> CodigosCircuitoDeclarados { get; init; } =
        Array.Empty<string>();
    public IReadOnlyList<string> CodigosCircuitoCatalogo { get; init; } =
        Array.Empty<string>();
    public JsonElement? GeometriaSubestacionPrivada { get; init; }
    public JsonElement? GeometriaInterconexionPublica { get; init; }
    public JsonElement? GeometriaTopologicaSugerida { get; init; }
    public JsonElement? GeometriaAnexoTecnico { get; init; }
    public string Gcr { get; init; } = string.Empty;
    public string Entidad { get; init; } = string.Empty;
    public string Municipio { get; init; } = string.Empty;
    public int ProyectosRelacionados { get; init; }
    public int ProyectosVigentes { get; init; }
    public IReadOnlyList<string> Decisiones { get; init; } = Array.Empty<string>();
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
    public string Version { get; init; } = "PAM-CONV2-v1.15";
    public string Layer { get; init; } = "evidencia_segunda_convocatoria";
    public DateTime GeneratedUtc { get; init; } = DateTime.UtcNow;
    public int UniverseProjects { get; init; }
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
