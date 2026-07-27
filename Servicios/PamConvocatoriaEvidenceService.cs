using Microsoft.Extensions.Caching.Memory;
using Microsoft.Extensions.Options;
using Microsoft.VisualBasic.FileIO;
using NSIE.Models;
using System.Globalization;
using System.Security.Cryptography;
using System.Text;
using System.Text.Json;
using System.Text.Json.Serialization;
using System.Text.RegularExpressions;

namespace NSIE.Servicios;

public sealed class PamConvocatoriaEvidenceOptions
{
    public const string SectionName = "PamTerritorial:SegundaConvocatoria";

    public string BaseUrl { get; set; } =
        "https://docs.google.com/spreadsheets/d/e/2PACX-1vR9QhQpXdFeh9ZkNh2WqA28v61ySMz4pISGX0hbZl5QAEU8LTypjCprVndMnkWPok1JdgjujmTV9p9i/pub?output=csv";
    public string TraceUrl { get; set; } =
        "https://docs.google.com/spreadsheets/d/e/2PACX-1vR9QhQpXdFeh9ZkNh2WqA28v61ySMz4pISGX0hbZl5QAEU8LTypjCprVndMnkWPok1JdgjujmTV9p9i/pub?gid=681832892&single=true&output=csv";
    public string SubstationsUrl { get; set; } =
        "https://cdn.sassoapps.com/dgmesnie/geojson/dgmesnie_subestaciones2.geojson";
    public string LinesUrl { get; set; } =
        "https://cdn.sassoapps.com/dgmesnie/geojson/dgmesnie_lt.geojson";
    public string GerenciasUrl { get; set; } =
        "https://cdn.sassoapps.com/Mapas/gerencias_javs_2.geojson";
    public int CacheMinutes { get; set; } = 30;
    public int SnapshotMaxAgeMinutes { get; set; } = 1440;
    public string SnapshotPath { get; set; } =
        "App_Data/cache/pam_convocatoria_coverage_v14.json";
    public int HighConfidenceThreshold { get; set; } = 90;
    public int ReviewThreshold { get; set; } = 70;
    public int MaximumMatchesPerPam { get; set; } = 12;
    public double SubstationCoordinateToleranceKm { get; set; } = 3;
    public int UniqueSubstationNameBonus { get; set; } = 20;
    public double StrongNameSimilarity { get; set; } = 0.9;
    public int StrongNameMargin { get; set; } = 15;
    public double TopologyEndpointClusterKm { get; set; } = 3;
    public double SourceSubstationClusterKm { get; set; } = 3;
    public double SourceToCatalogSubstationKm { get; set; } = 15;
    public string TechnicalAnnexEvidencePath { get; set; } =
        "wwwroot/data/pam_convocatoria_evidencia_anexos.json";
}

public interface IPamConvocatoriaEvidenceService
{
    Task<IReadOnlyList<PamConvocatoriaEvidenceResult>> ResolverPamAsync(
        IReadOnlyCollection<PamTerritorialProyecto> proyectos,
        CancellationToken cancellationToken);

    IReadOnlyList<PamTerritorialUbicacion> CrearUbicaciones(
        PamConvocatoriaEvidenceResult resultado,
        bool soloConfianzaAlta);

    Task<PamConvocatoriaCoverageReport> ObtenerCoberturaAsync(
        CancellationToken cancellationToken,
        bool forceRefresh = false);

    Task<PamConvocatoriaGeoJson> ObtenerCoberturaGeoJsonAsync(
        CancellationToken cancellationToken);
}

public sealed class PamConvocatoriaEvidenceService : IPamConvocatoriaEvidenceService
{
    public const string RulesVersion = "PAM-CONV2-v1.14";

    private const string CacheKey = "pam-conv2-evidence-catalog-v14";
    private static readonly SemaphoreSlim CatalogLock = new(1, 1);
    private static readonly HashSet<string> EmptyValues = new(
        new[]
        {
            "SIN INFORMACION",
            "NO APLICA",
            "N A",
            "NA",
            "NULL",
            "-"
        },
        StringComparer.Ordinal);
    private static readonly HashSet<string> SubstationEquivalenceStopWords =
        new(
            new[]
            {
                "DE", "DEL", "LA", "EL", "LOS", "LAS", "Y", "EN"
            },
            StringComparer.Ordinal);
    private static readonly HashSet<string> SubstationBankQualifiers = new(
        new[]
        {
            "BCO", "BCOS", "BANCO", "BANCOS"
        },
        StringComparer.Ordinal);
    private static readonly HashSet<string> PrivateSubstationQualifiers = new(
        new[]
        {
            "SOLAR",
            "EOLICA",
            "FOTOVOLTAICA",
            "MANIOBRAS",
            "NUEVA",
            "CONSTRUCCION",
            "PRIVADA",
            "COLECTORA",
            "ELEVADORA"
        },
        StringComparer.Ordinal);

    private readonly HttpClient _httpClient;
    private readonly IMemoryCache _cache;
    private readonly PamConvocatoriaEvidenceOptions _options;
    private readonly ILogger<PamConvocatoriaEvidenceService> _logger;
    private readonly string _snapshotPath;

    public PamConvocatoriaEvidenceService(
        HttpClient httpClient,
        IMemoryCache cache,
        IOptions<PamConvocatoriaEvidenceOptions> options,
        IWebHostEnvironment environment,
        ILogger<PamConvocatoriaEvidenceService> logger)
    {
        _httpClient = httpClient;
        _cache = cache;
        _options = options.Value;
        _logger = logger;
        _snapshotPath = Path.IsPathRooted(_options.SnapshotPath)
            ? _options.SnapshotPath
            : Path.GetFullPath(
                Path.Combine(
                    environment.ContentRootPath,
                    _options.SnapshotPath));
    }

    public async Task<IReadOnlyList<PamConvocatoriaEvidenceResult>> ResolverPamAsync(
        IReadOnlyCollection<PamTerritorialProyecto> proyectos,
        CancellationToken cancellationToken)
    {
        if (proyectos.Count == 0)
        {
            return Array.Empty<PamConvocatoriaEvidenceResult>();
        }

        var catalog = await ObtenerCatalogoAsync(cancellationToken);
        var results = proyectos
            .Select(project => ResolverPam(project, catalog))
            .ToList();

        _logger.LogInformation(
            "Cruce {Version}: {Projects} PAM, {High} con evidencia alta y {Review} con evidencia para revisión.",
            RulesVersion,
            results.Count,
            results.Count(result => result.CoincidenciasAltas > 0),
            results.Count(result => result.RequierenRevision > 0));

        return results;
    }

    public IReadOnlyList<PamTerritorialUbicacion> CrearUbicaciones(
        PamConvocatoriaEvidenceResult resultado,
        bool soloConfianzaAlta)
    {
        var matches = resultado.Coincidencias
            .Where(match =>
                match.GeometriaSugerida.HasValue &&
                (!soloConfianzaAlta ||
                 string.Equals(match.NivelConfianza, "alta", StringComparison.Ordinal)))
            .GroupBy(
                match => string.IsNullOrWhiteSpace(match.ClaveElementoRed)
                    ? $"{match.PreFolio}|{match.TipoElementoRed}"
                    : match.ClaveElementoRed,
                StringComparer.Ordinal)
            .Select(group => group
                .OrderByDescending(match => match.Puntaje)
                .First())
            .Take(Math.Max(1, _options.MaximumMatchesPerPam))
            .ToList();

        return matches
            .Select((match, index) =>
            {
                var geometry = match.GeometriaSugerida!.Value;
                var geometryType = GetGeometryType(geometry);
                DateTime? cutoff = null;
                if (DateTimeOffset.TryParse(
                        match.FechaDecision,
                        CultureInfo.InvariantCulture,
                        DateTimeStyles.AssumeLocal,
                        out var parsedCutoff))
                {
                    cutoff = parsedCutoff.Date;
                }

                return new PamTerritorialUbicacion
                {
                    UbicacionId = -checked(
                        9_000_000_000L +
                        resultado.ProyectoId * 100L +
                        index +
                        1),
                    ProyectoId = resultado.ProyectoId,
                    Etiqueta = FirstNonEmpty(
                        match.ElementoRedCatalogado,
                        match.SubestacionDeclarada,
                        match.ProyectoConvocatoria,
                        match.PreFolio),
                    TipoGeometria = geometryType,
                    Geometria = geometry,
                    Latitud = match.Latitud,
                    Longitud = match.Longitud,
                    Entidad = match.Entidad,
                    Municipio = match.Municipio,
                    PrecisionUbicacion = string.Equals(
                        match.TipoCoincidencia,
                        "identidad_proyecto",
                        StringComparison.Ordinal)
                            ? "geometria declarada en Segunda Convocatoria"
                            : "infraestructura compartida con Segunda Convocatoria",
                    MetodoUbicacion = "cruce_segunda_convocatoria_v1",
                    Fuente = match.Fuente,
                    FechaCorte = cutoff,
                    RadioSugeridoKm = DefaultRadius(geometryType),
                    Orden = index + 1,
                    EsPrincipal = index == 0,
                    Validada = false,
                    EsAsociacionSugerida = true,
                    PuntajeCoincidencia = match.Puntaje,
                    NivelConfianza = match.NivelConfianza,
                    TipoElementoRed = match.TipoElementoRed,
                    ClaveElementoRed = match.ClaveElementoRed,
                    Evidencias = match.Evidencias
                };
            })
            .ToList();
    }

    public async Task<PamConvocatoriaCoverageReport> ObtenerCoberturaAsync(
        CancellationToken cancellationToken,
        bool forceRefresh = false)
    {
        if (!forceRefresh &&
            _cache.TryGetValue<EvidenceCatalog>(CacheKey, out var cached) &&
            cached is not null)
        {
            return cached.Coverage;
        }

        if (!forceRefresh)
        {
            var snapshot = await TryLoadCoverageSnapshotAsync(
                cancellationToken);
            if (snapshot is not null)
            {
                return snapshot;
            }
        }

        var catalog = await ObtenerCatalogoAsync(cancellationToken);
        return catalog.Coverage;
    }

    public async Task<PamConvocatoriaGeoJson> ObtenerCoberturaGeoJsonAsync(
        CancellationToken cancellationToken)
    {
        var report = await ObtenerCoberturaAsync(cancellationToken);
        var candidates = report.Candidatos
            .Where(candidate =>
                (candidate.GeometriaAnexoTecnico.HasValue ||
                 candidate.GeometriaSubestacionPrivada.HasValue ||
                 candidate.GeometriaTopologicaSugerida.HasValue ||
                 candidate.Geometria.HasValue) &&
                !string.Equals(candidate.Estado, "catalogada", StringComparison.Ordinal))
            .ToList();

        return new PamConvocatoriaGeoJson
        {
            Meta = new PamConvocatoriaGeoJsonMeta
            {
                UniverseProjects = report.ProyectosUniverso,
                Projects = report.ProyectosContinuan,
                Candidates = candidates.Count,
                MissingSubstations = report.SubestacionesFaltantes,
                ReviewSubstations = report.SubestacionesRevision,
                MissingLines = report.LineasSinGeometria,
                Source = report.Fuente
            },
            Features = candidates
                .Select(candidate => new PamConvocatoriaGeoJsonFeature
                {
                    Geometry = (
                        candidate.GeometriaAnexoTecnico ??
                        candidate.GeometriaSubestacionPrivada ??
                        candidate.GeometriaTopologicaSugerida ??
                        candidate.Geometria)!.Value,
                    Properties = new Dictionary<string, object?>
                    {
                        ["candidato_id"] = candidate.CandidatoId,
                        ["nombre"] = candidate.NombreDeclarado,
                        ["tipo_elemento"] = candidate.TipoElemento,
                        ["estado"] = candidate.Estado,
                        ["coincidencia_catalogo"] = candidate.CoincidenciaCatalogo,
                        ["puntaje"] = candidate.Puntaje,
                        ["tension_kv"] = candidate.TensionKv,
                        ["distancia_catalogo_km"] = candidate.DistanciaCatalogoKm,
                        ["geometrias_subestacion_privada"] =
                            candidate.GeometriasSubestacionPrivada,
                        ["distancias_declaradas"] =
                            candidate.DistanciasDeclaradas,
                        ["distancias_compatibles"] =
                            candidate.DistanciasCompatibles,
                        ["distancia_interconexion_min_km"] =
                            candidate.DistanciaInterconexionCalculadaMinimaKm,
                        ["distancia_interconexion_max_km"] =
                            candidate.DistanciaInterconexionCalculadaMaximaKm,
                        ["diferencia_distancia_max_km"] =
                            candidate.DiferenciaDistanciaMaximaKm,
                        ["resoluciones_catalogo_distintas"] =
                            candidate.ResolucionesCatalogoDistintas,
                        ["nombre_catalogo_unico"] =
                            candidate.NombreCatalogoUnico,
                        ["nombre_coincidencia_fuerte"] =
                            candidate.NombreCoincidenciaFuerte,
                        ["nombre_equivalente_catalogo"] =
                            candidate.NombreEquivalenteCatalogo,
                        ["similitud_nombre"] =
                            candidate.SimilitudNombre,
                        ["margen_puntaje"] =
                            candidate.MargenPuntaje,
                        ["gcr_catalogo"] =
                            candidate.GcrCatalogo,
                        ["coincidencia_topologica_firme"] =
                            candidate.CoincidenciaTopologicaFirme,
                        ["coincidencia_tension_topologica_firme"] =
                            candidate.CoincidenciaTensionTopologicaFirme,
                        ["niveles_tension_red_kv"] =
                            candidate.NivelesTensionRedKv,
                        ["homonimo_territorial_descartado"] =
                            candidate.HomonimoTerritorialDescartado,
                        ["grupo_territorial_separado"] =
                            candidate.GrupoTerritorialSeparado,
                        ["clave_agrupacion_territorial"] =
                            candidate.ClaveAgrupacionTerritorial,
                        ["coincidencia_fuente_georreferenciada_firme"] =
                            candidate.CoincidenciaFuenteGeorreferenciadaFirme,
                        ["coincidencia_interconexion_publica_firme"] =
                            candidate.CoincidenciaInterconexionPublicaFirme,
                        ["nombre_interconexion_publica"] =
                            candidate.NombreInterconexionPublica,
                        ["coincidencia_interconexion_catalogo"] =
                            candidate.CoincidenciaInterconexionCatalogo,
                        ["clave_interconexion_catalogo"] =
                            candidate.ClaveInterconexionCatalogo,
                        ["distancia_interconexion_publica_km"] =
                            candidate.DistanciaInterconexionPublicaKm,
                        ["filas_fuente_georreferenciada"] =
                            candidate.FilasFuenteGeorreferenciada,
                        ["separacion_maxima_fuente_km"] =
                            candidate.SeparacionMaximaFuenteKm,
                        ["soportes_fuente"] =
                            string.Join(" · ", candidate.SoportesFuente),
                        ["coincidencia_corroboracion_multifuente_firme"] =
                            candidate.CoincidenciaCorroboracionMultifuenteFirme,
                        ["proyectos_independientes_corroborados"] =
                            candidate.ProyectosIndependientesCorroborados,
                        ["soportes_corroboracion"] =
                            string.Join(" · ", candidate.SoportesCorroboracion),
                        ["coincidencia_anexo_tecnico_firme"] =
                            candidate.CoincidenciaAnexoTecnicoFirme,
                        ["modo_anexo_tecnico"] =
                            candidate.ModoAnexoTecnico,
                        ["fuente_anexo_tecnico"] =
                            candidate.FuenteAnexoTecnico,
                        ["sha256_anexo_tecnico"] =
                            candidate.Sha256AnexoTecnico,
                        ["soportes_anexo_tecnico"] =
                            string.Join(
                                " · ",
                                candidate.SoportesAnexoTecnico),
                        ["coincidencia_automatica_firme"] =
                            candidate.CoincidenciaAutomaticaFirme,
                        ["motivo_automatizacion"] =
                            candidate.MotivoAutomatizacion,
                        ["lineas_soporte"] =
                            string.Join(" · ", candidate.LineasSoporte),
                        ["estado_conexion_grafo"] =
                            candidate.EstadoConexionGrafo,
                        ["coincidencia_grafo_firme"] =
                            candidate.CoincidenciaGrafoFirme,
                        ["extremos_resueltos_grafo"] =
                            candidate.ExtremosResueltosGrafo,
                        ["extremo_a_declarado"] =
                            candidate.ExtremoADeclarado,
                        ["extremo_b_declarado"] =
                            candidate.ExtremoBDeclarado,
                        ["extremo_a_catalogo"] =
                            candidate.ExtremoACatalogo,
                        ["extremo_b_catalogo"] =
                            candidate.ExtremoBCatalogo,
                        ["distancia_extremo_a_km"] =
                            candidate.DistanciaExtremoAKm,
                        ["distancia_extremo_b_km"] =
                            candidate.DistanciaExtremoBKm,
                        ["soportes_grafo"] =
                            string.Join(" · ", candidate.SoportesGrafo),
                        ["modo_coincidencia_linea"] =
                            candidate.ModoCoincidenciaLinea,
                        ["coincidencia_par_extremos_firme"] =
                            candidate.CoincidenciaParExtremosFirme,
                        ["coincidencia_codigo_circuito_firme"] =
                            candidate.CoincidenciaCodigoCircuitoFirme,
                        ["codigos_circuito_declarados"] =
                            string.Join(
                                " · ",
                                candidate.CodigosCircuitoDeclarados),
                        ["codigos_circuito_catalogo"] =
                            string.Join(
                                " · ",
                                candidate.CodigosCircuitoCatalogo),
                        ["origen_geometria"] =
                            candidate.GeometriaAnexoTecnico.HasValue
                                ? "anexo_tecnico_verificado"
                                : candidate.GeometriaSubestacionPrivada.HasValue
                                ? "subestacion_privada_proyecto"
                                : candidate.GeometriaTopologicaSugerida.HasValue
                                    ? "extremo_real_linea"
                                : "catalogo_referencia",
                        ["gcr"] = candidate.Gcr,
                        ["entidad"] = candidate.Entidad,
                        ["municipio"] = candidate.Municipio,
                        ["proyectos_relacionados"] = candidate.ProyectosRelacionados,
                        ["proyectos_vigentes"] = candidate.ProyectosVigentes,
                        ["incluye_vigentes"] = candidate.ProyectosVigentes > 0,
                        ["decisiones"] = string.Join(" · ", candidate.Decisiones),
                        ["folios"] = string.Join(" · ", candidate.Folios),
                        ["evidencias"] = string.Join(" · ", candidate.Evidencias),
                        ["fuente"] = candidate.Fuente,
                        ["validada"] = false,
                        ["advertencia"] =
                            candidate.CoincidenciaInterconexionPublicaFirme
                                ? "La geometría principal representa la subestación privada del proyecto. El nodo público se resolvió por separado desde el campo de interconexión; la relación declarada no equivale a una línea física inferida."
                                : "La geometría de Segunda Convocatoria representa la subestación privada del proyecto; no constituye por sí misma un nodo CFE/CENACE."
                    }
                })
                .ToList()
        };
    }

    private PamConvocatoriaEvidenceResult ResolverPam(
        PamTerritorialProyecto project,
        EvidenceCatalog catalog)
    {
        var spec = PamSpec.Create(project);
        var repeatedSubstations = catalog.Rows
            .Where(row => !string.IsNullOrWhiteSpace(row.NormalizedSubstation))
            .GroupBy(row => row.NormalizedSubstation, StringComparer.Ordinal)
            .ToDictionary(
                group => group.Key,
                group => group.Count(),
                StringComparer.Ordinal);

        var matches = new List<PamConvocatoriaProjectMatch>();
        foreach (var row in catalog.Rows)
        {
            var score = 0;
            var evidence = new List<string>();
            var identityEvidence = false;
            var infrastructureEvidence = false;

            var rowFolios = new[]
                {
                    NormalizeFolio(row.PreFolio),
                    NormalizeFolio(row.ProjectFolio)
                }
                .Where(value => !string.IsNullOrWhiteSpace(value))
                .ToHashSet(StringComparer.Ordinal);
            if (spec.ProjectKeys.Overlaps(rowFolios))
            {
                score += 80;
                identityEvidence = true;
                evidence.Add("folio del PAM y de Segunda Convocatoria coincidente (+80)");
            }

            var nameSimilarity = TokenSimilarity(
                project.NombreProyecto,
                row.ProjectName);
            if (nameSimilarity >= 0.92)
            {
                score += 60;
                identityEvidence = true;
                evidence.Add("nombre de proyecto prácticamente idéntico (+60)");
            }
            else if (nameSimilarity >= 0.72)
            {
                score += 40;
                identityEvidence = true;
                evidence.Add("nombre de proyecto compatible (+40)");
            }
            else if (nameSimilarity >= 0.55)
            {
                score += 20;
                evidence.Add("nombre de proyecto parcialmente compatible (+20)");
            }

            var sharedSubstation =
                !string.IsNullOrWhiteSpace(row.NormalizedSubstation) &&
                ContainsWhole(spec.PaddedText, row.NormalizedSubstation);
            if (sharedSubstation)
            {
                score += 55;
                infrastructureEvidence = true;
                evidence.Add(
                    $"subestación declarada “{row.DeclaredSubstation}” presente en el PAM (+55)");
            }

            var sharedLine = row.LineResolutions
                .Where(line =>
                    !string.IsNullOrWhiteSpace(line.NormalizedDeclaredName) &&
                    ContainsWhole(spec.PaddedText, line.NormalizedDeclaredName))
                .OrderByDescending(line => line.Score)
                .FirstOrDefault();
            if (sharedLine is not null)
            {
                score += 55;
                infrastructureEvidence = true;
                evidence.Add(
                    $"línea declarada “{sharedLine.DeclaredName}” presente en el PAM (+55)");
            }

            var pamGcr = spec.GcrKey;
            var rowGcr = ResolveGcrKey(row.Gcr);
            if (!string.IsNullOrWhiteSpace(pamGcr) &&
                !string.IsNullOrWhiteSpace(rowGcr))
            {
                if (string.Equals(pamGcr, rowGcr, StringComparison.Ordinal))
                {
                    score += 15;
                    evidence.Add($"GCR coincidente: {row.Gcr} (+15)");
                }
                else
                {
                    score -= 25;
                    evidence.Add($"GCR incompatible: PAM {project.Gcr} / convocatoria {row.Gcr} (-25)");
                }
            }

            if (row.VoltageKv.HasValue && spec.VoltagesKv.Count > 0)
            {
                if (spec.VoltagesKv.Any(value =>
                        Math.Abs(value - row.VoltageKv.Value) < 0.6))
                {
                    score += 10;
                    evidence.Add($"tensión coincidente: {row.VoltageKv:0.##} kV (+10)");
                }
                else
                {
                    score -= 10;
                    evidence.Add($"tensión incompatible: {row.VoltageKv:0.##} kV (-10)");
                }
            }

            if (!string.IsNullOrWhiteSpace(row.Municipality) &&
                ContainsWhole(spec.PaddedText, NormalizeText(row.Municipality)))
            {
                score += 5;
                evidence.Add($"municipio coincidente: {row.Municipality} (+5)");
            }

            if (identityEvidence && row.ProjectGeometry.HasValue)
            {
                score += 10;
                evidence.Add("geometría declarada del proyecto disponible (+10)");
            }
            if (sharedSubstation &&
                string.Equals(row.SubstationResolution.State, "catalogada", StringComparison.Ordinal))
            {
                score += 10;
                evidence.Add("subestación confirmada en el catálogo geoespacial (+10)");
                if (row.SubstationResolution.DistanceCompatible == true)
                {
                    score += 10;
                    evidence.Add(
                        $"distancia calculada compatible con la declarada ({row.DeclaredDistanceKm:0.##} km) (+10)");
                }
            }
            if (sharedSubstation &&
                repeatedSubstations.GetValueOrDefault(row.NormalizedSubstation) >= 2)
            {
                score += 5;
                evidence.Add("subestación respaldada por varios proyectos de la convocatoria (+5)");
            }
            if (sharedLine?.Element is not null)
            {
                score += 10;
                evidence.Add("línea confirmada en el catálogo geoespacial (+10)");
            }

            if (score < _options.ReviewThreshold ||
                (!identityEvidence && !infrastructureEvidence))
            {
                continue;
            }

            var type = identityEvidence
                ? "identidad_proyecto"
                : "infraestructura_compartida";
            var geometry = default(JsonElement?);
            var latitude = default(double?);
            var longitude = default(double?);
            var elementType = string.Empty;
            var elementKey = string.Empty;
            var catalogName = string.Empty;
            var networkState = string.Empty;

            if (identityEvidence && row.ProjectGeometry.HasValue)
            {
                geometry = row.ProjectGeometry;
                latitude = row.ProjectPoint?.Latitude;
                longitude = row.ProjectPoint?.Longitude;
                elementType = "proyecto_convocatoria";
                elementKey = StableKey(
                    "C2-PROY",
                    NormalizeText(row.PreFolio),
                    row.ProjectGeometry.Value.GetRawText());
                networkState = "geometria_convocatoria";
            }
            else if (sharedSubstation)
            {
                var resolution = row.SubstationResolution;
                geometry = resolution.Element?.Geometry;
                latitude = resolution.Element?.Point?.Latitude;
                longitude = resolution.Element?.Point?.Longitude;
                elementType = "subestacion";
                elementKey = resolution.Element?.Key ??
                    StableKey(
                        "C2-SE",
                        row.NormalizedSubstation,
                        geometry?.GetRawText() ?? string.Empty);
                catalogName = resolution.Element?.Name ?? string.Empty;
                networkState = resolution.State;
            }
            else if (sharedLine is not null)
            {
                geometry = sharedLine.Element?.Geometry;
                latitude = sharedLine.Element?.Point?.Latitude;
                longitude = sharedLine.Element?.Point?.Longitude;
                elementType = "linea_transmision";
                elementKey = sharedLine.Element?.Key ??
                    StableKey(
                        "C2-LT",
                        sharedLine.NormalizedDeclaredName,
                        string.Empty);
                catalogName = sharedLine.Element?.Name ?? string.Empty;
                networkState = sharedLine.State;
            }

            matches.Add(new PamConvocatoriaProjectMatch
            {
                PreFolio = row.PreFolio,
                FolioProyecto = row.ProjectFolio,
                ProyectoConvocatoria = row.ProjectName,
                TipoCoincidencia = type,
                Puntaje = Math.Clamp(score, 0, 100),
                NivelConfianza = score >= _options.HighConfidenceThreshold
                    ? "alta"
                    : "revision",
                Gcr = row.Gcr,
                Entidad = row.State,
                Municipio = row.Municipality,
                SubestacionDeclarada = row.DeclaredSubstation,
                PuntoInterconexion = row.InterconnectionPoint,
                TensionKv = row.VoltageKv,
                DistanciaDeclaradaKm = row.DeclaredDistanceKm,
                EstadoElementoRed = networkState,
                ElementoRedCatalogado = catalogName,
                TipoElementoRed = elementType,
                ClaveElementoRed = elementKey,
                GeometriaSugerida = geometry,
                Latitud = latitude,
                Longitud = longitude,
                KmlProyecto = row.ProjectKml,
                KmlSubestacion = row.SubstationKml,
                FechaDecision = row.DecisionDate,
                Evidencias = evidence.Distinct(StringComparer.Ordinal).ToList(),
                Fuente = _options.BaseUrl
            });
        }

        var selected = matches
            .OrderByDescending(match => match.Puntaje)
            .ThenBy(match => match.PreFolio, StringComparer.OrdinalIgnoreCase)
            .Take(Math.Max(1, _options.MaximumMatchesPerPam))
            .ToList();
        var high = selected.Count(match =>
            string.Equals(match.NivelConfianza, "alta", StringComparison.Ordinal));
        var review = selected.Count - high;

        return new PamConvocatoriaEvidenceResult
        {
            ProyectoId = project.ProyectoId,
            ClaveProyecto = project.ClaveProyecto,
            Estado = high > 0
                ? "confianza_alta"
                : review > 0
                    ? "requiere_revision"
                    : "sin_coincidencias",
            CoincidenciasAltas = high,
            RequierenRevision = review,
            Coincidencias = selected
        };
    }

    private async Task<EvidenceCatalog> ObtenerCatalogoAsync(
        CancellationToken cancellationToken)
    {
        if (_cache.TryGetValue<EvidenceCatalog>(CacheKey, out var cached) &&
            cached is not null)
        {
            return cached;
        }

        await CatalogLock.WaitAsync(cancellationToken);
        try
        {
            if (_cache.TryGetValue<EvidenceCatalog>(CacheKey, out cached) &&
                cached is not null)
            {
                return cached;
            }

            var baseTask = _httpClient.GetStringAsync(
                _options.BaseUrl,
                cancellationToken);
            var traceTask = _httpClient.GetStringAsync(
                _options.TraceUrl,
                cancellationToken);
            var substationsTask = _httpClient.GetStringAsync(
                _options.SubstationsUrl,
                cancellationToken);
            var linesTask = _httpClient.GetStringAsync(
                _options.LinesUrl,
                cancellationToken);
            var gcrTask = _httpClient.GetStringAsync(
                _options.GerenciasUrl,
                cancellationToken);
            await Task.WhenAll(
                baseTask,
                traceTask,
                substationsTask,
                linesTask,
                gcrTask);

            var gcrAreas = ParseGcrAreas(await gcrTask);
            var substations = ParseSubstations(
                await substationsTask,
                gcrAreas);
            var lines = ParseLines(
                await linesTask,
                gcrAreas);
            var lineEndpoints = BuildLineEndpoints(
                lines,
                substations,
                _options.TopologyEndpointClusterKm);
            var lineTopology = BuildLineTopologyCatalog(
                lines,
                substations,
                _options.TopologyEndpointClusterKm);
            var allRows = ParseSecondCallRows(
                await baseTask,
                await traceTask,
                substations,
                lines,
                lineEndpoints,
                lineTopology);
            var technicalAnnexEvidence =
                LoadTechnicalAnnexEvidence();
            var activeRows = allRows
                .Where(row => row.IsActive)
                .ToList();
            var coverage = BuildCoverage(
                allRows,
                technicalAnnexEvidence);
            await SaveCoverageSnapshotAsync(
                coverage,
                cancellationToken);
            var catalog = new EvidenceCatalog(
                activeRows,
                allRows,
                substations,
                lines,
                coverage);

            _cache.Set(
                CacheKey,
                catalog,
                TimeSpan.FromMinutes(Math.Max(5, _options.CacheMinutes)));
            _logger.LogInformation(
                "Segunda Convocatoria cargada: {Universe} proyectos base, {Active} con decisión Continúa, {Substations} subestaciones declaradas y {LineRefs} referencias de línea.",
                coverage.ProyectosUniverso,
                coverage.ProyectosContinuan,
                coverage.ReferenciasSubestacion,
                coverage.ReferenciasLinea);
            return catalog;
        }
        finally
        {
            CatalogLock.Release();
        }
    }

    private async Task<PamConvocatoriaCoverageReport?>
        TryLoadCoverageSnapshotAsync(
            CancellationToken cancellationToken)
    {
        if (_options.SnapshotMaxAgeMinutes <= 0 ||
            !File.Exists(_snapshotPath))
        {
            return null;
        }

        try
        {
            await using var stream = new FileStream(
                _snapshotPath,
                FileMode.Open,
                FileAccess.Read,
                FileShare.ReadWrite,
                bufferSize: 64 * 1024,
                useAsync: true);
            var snapshot =
                await JsonSerializer.DeserializeAsync<CoverageSnapshot>(
                    stream,
                    new JsonSerializerOptions
                    {
                        PropertyNameCaseInsensitive = true
                    },
                    cancellationToken);
            if (snapshot?.Coverage is null ||
                !string.Equals(
                    snapshot.VersionReglas,
                    RulesVersion,
                    StringComparison.Ordinal) ||
                DateTime.UtcNow - snapshot.GuardadoUtc >
                    TimeSpan.FromMinutes(
                        Math.Max(5, _options.SnapshotMaxAgeMinutes)))
            {
                return null;
            }

            _logger.LogInformation(
                "Diagnóstico {Version} de Segunda Convocatoria recuperado de la fotografía persistida de {SavedUtc}.",
                snapshot.VersionReglas,
                snapshot.GuardadoUtc);
            return snapshot.Coverage;
        }
        catch (Exception ex) when (
            ex is IOException or
            UnauthorizedAccessException or
            JsonException)
        {
            _logger.LogWarning(
                ex,
                "No fue posible leer la fotografía persistida de Segunda Convocatoria; se reconstruirá desde las fuentes.");
            return null;
        }
    }

    private async Task SaveCoverageSnapshotAsync(
        PamConvocatoriaCoverageReport coverage,
        CancellationToken cancellationToken)
    {
        var directory = Path.GetDirectoryName(_snapshotPath);
        if (string.IsNullOrWhiteSpace(directory))
        {
            return;
        }

        var temporaryPath =
            _snapshotPath + "." + Guid.NewGuid().ToString("N") + ".tmp";
        try
        {
            Directory.CreateDirectory(directory);
            await using (var stream = new FileStream(
                temporaryPath,
                FileMode.CreateNew,
                FileAccess.Write,
                FileShare.None,
                bufferSize: 64 * 1024,
                useAsync: true))
            {
                await JsonSerializer.SerializeAsync(
                    stream,
                    new CoverageSnapshot
                    {
                        VersionReglas = RulesVersion,
                        GuardadoUtc = DateTime.UtcNow,
                        Coverage = coverage
                    },
                    cancellationToken: cancellationToken);
                await stream.FlushAsync(cancellationToken);
            }

            File.Move(
                temporaryPath,
                _snapshotPath,
                overwrite: true);
        }
        catch (Exception ex) when (
            ex is IOException or
            UnauthorizedAccessException)
        {
            _logger.LogWarning(
                ex,
                "No fue posible persistir la fotografía de Segunda Convocatoria.");
        }
        finally
        {
            if (File.Exists(temporaryPath))
            {
                try
                {
                    File.Delete(temporaryPath);
                }
                catch (IOException)
                {
                    // El archivo temporal se podrá limpiar en el siguiente mantenimiento.
                }
            }
        }
    }

    private List<SecondCallRow> ParseSecondCallRows(
        string baseCsv,
        string traceCsv,
        IReadOnlyList<NetworkElement> substations,
        IReadOnlyList<NetworkElement> lines,
        IReadOnlyList<LineEndpointReference> lineEndpoints,
        IReadOnlyDictionary<string, LineTopologyEvidence> lineTopology)
    {
        var latestDecisions = ParseLatestDecisions(traceCsv);
        if (latestDecisions.Count == 0)
        {
            throw new InvalidDataException(
                "La trazabilidad de Segunda Convocatoria no contiene decisiones.");
        }

        var csvRows = ParseCsv(baseCsv);
        var headerIndex = csvRows.FindIndex(row =>
            row.Any(value =>
                string.Equals(
                    NormalizeHeader(value),
                    "PRE FOLIO",
                    StringComparison.Ordinal)) &&
            row.Any(value =>
                string.Equals(
                    NormalizeHeader(value),
                    "SUBESTACION ELECTRICA DE INTERCONEXION",
                    StringComparison.Ordinal)));
        if (headerIndex < 0)
        {
            throw new InvalidDataException(
                "No se encontró la cabecera esperada en la hoja base de Segunda Convocatoria.");
        }

        var header = csvRows[headerIndex];
        var indexes = SecondCallIndexes.Create(header);
        var substationNameCounts = substations
            .Where(element => !string.IsNullOrWhiteSpace(element.EquivalenceKey))
            .GroupBy(
                element => element.EquivalenceKey,
                StringComparer.Ordinal)
            .ToDictionary(
                group => group.Key,
                group => group.Count(),
                StringComparer.Ordinal);
        var substationScopeNameCounts = substations
            .SelectMany(element =>
                element.GcrKeys.DefaultIfEmpty(string.Empty)
                    .Select(gcr => new
                    {
                        Key =
                            $"{gcr}|{element.EquivalenceKey}"
                    }))
            .GroupBy(item => item.Key, StringComparer.Ordinal)
            .ToDictionary(
                group => group.Key,
                group => group.Count(),
                StringComparer.Ordinal);
        var output = new List<SecondCallRow>();

        foreach (var values in csvRows.Skip(headerIndex + 1))
        {
            var preFolio = Clean(Read(values, indexes.PreFolio));
            var decisionKey = NormalizeFolio(preFolio);
            if (string.IsNullOrWhiteSpace(decisionKey))
            {
                continue;
            }
            latestDecisions.TryGetValue(decisionKey, out var decision);

            var projectPoints = ParsePoints(
                values,
                indexes.ProjectVertexStart,
                indexes.ProjectVertexEnd);
            var substationPoints = ParsePoints(
                values,
                indexes.SubstationVertexStart,
                indexes.SubstationVertexEnd);
            var projectGeometry = CreateGeometry(projectPoints);
            var substationGeometry = CreateGeometry(substationPoints);
            var projectPoint = RepresentativePoint(projectPoints);
            var substationPoint = RepresentativePoint(substationPoints);
            var substationGeometryMatchesProject =
                SamePointSet(projectPoints, substationPoints);

            var interconnectionPoint = Clean(
                Read(values, indexes.InterconnectionPoint));
            var interconnectionSubstation =
                ExtractSubstationFromPoint(interconnectionPoint);
            var declaredSubstation = SelectDeclaredSubstation(
                Clean(Read(values, indexes.Substation)),
                Clean(Read(values, indexes.NamedSubstation)),
                interconnectionSubstation);
            var description = Clean(Read(values, indexes.Description));
            var voltage = ParseNumber(Read(values, indexes.VoltageKv), 1, 500) ??
                ParseVoltage(interconnectionPoint) ??
                ParseVoltage(description);
            var normalizedSubstation = NormalizeSubstationName(declaredSubstation);

            var row = new SecondCallRow
            {
                PreFolio = preFolio,
                ProjectFolio = Clean(Read(values, indexes.ProjectFolio)),
                ProjectName = FirstNonEmpty(
                    Clean(Read(values, indexes.Project)),
                    Clean(Read(values, indexes.Name)),
                    preFolio),
                Description = description,
                Gcr = FirstNonEmpty(
                    Clean(Read(values, indexes.Gcr)),
                    decision?.Gcr ?? string.Empty),
                State = Clean(Read(values, indexes.State)),
                Municipality = Clean(Read(values, indexes.Municipality)),
                DeclaredSubstation = declaredSubstation,
                NormalizedSubstation = normalizedSubstation,
                InterconnectionPoint = interconnectionPoint,
                InterconnectionSubstation = interconnectionSubstation,
                NormalizedInterconnectionSubstation =
                    NormalizeSubstationName(interconnectionSubstation),
                VoltageKv = voltage,
                DeclaredDistanceKm = ParseNumber(
                    Read(values, indexes.DistanceKm),
                    0,
                    1000),
                ProjectGeometry = projectGeometry,
                ProjectPoint = projectPoint,
                SubstationGeometry = substationGeometry,
                SubstationPoint = substationPoint,
                SubstationGeometryMatchesProject =
                    substationGeometryMatchesProject,
                ProjectKml = Clean(Read(values, indexes.ProjectKml)),
                SubstationKml = Clean(Read(values, indexes.SubstationKml)),
                Decision = decision?.Decision ?? string.Empty,
                DecisionDate = decision?.Timestamp ?? string.Empty,
                IsActive = string.Equals(
                    NormalizeText(decision?.Decision ?? string.Empty),
                    "CONTINUA",
                    StringComparison.Ordinal)
            };
            row.SubstationResolution = ResolveSubstation(
                row,
                substations,
                substationNameCounts,
                substationScopeNameCounts,
                lineEndpoints);
            row.InterconnectionSubstationResolution =
                ResolveInterconnectionSubstation(
                    row,
                    substations,
                    substationNameCounts,
                    substationScopeNameCounts,
                    lineEndpoints);
            row.LineResolutions = ResolveLines(
                row,
                lines,
                lineTopology);
            output.Add(row);
        }

        return output;
    }

    private Dictionary<string, TraceDecision> ParseLatestDecisions(string csv)
    {
        var rows = ParseCsv(csv);
        var headerIndex = rows.FindIndex(row =>
            row.Any(value =>
                string.Equals(
                    NormalizeHeader(value),
                    "FOLIO",
                    StringComparison.Ordinal)) &&
            row.Any(value =>
                string.Equals(
                    NormalizeHeader(value),
                    "DECISION",
                    StringComparison.Ordinal)));
        if (headerIndex < 0)
        {
            throw new InvalidDataException(
                "No se encontró la cabecera de trazabilidad de Segunda Convocatoria.");
        }

        var header = rows[headerIndex];
        var folioIndex = FindIndex(header, "FOLIO");
        var decisionIndex = FindIndex(header, "DECISION");
        var timestampIndex = FindIndex(header, "TIMESTAMP");
        var gcrIndex = FindIndex(header, "GCR");
        var latest = new Dictionary<string, TraceDecision>(StringComparer.Ordinal);

        for (var index = headerIndex + 1; index < rows.Count; index++)
        {
            var row = rows[index];
            var folio = NormalizeFolio(Read(row, folioIndex));
            if (string.IsNullOrWhiteSpace(folio))
            {
                continue;
            }

            var timestamp = Clean(Read(row, timestampIndex));
            var sortValue = DateTimeOffset.TryParse(
                    timestamp,
                    CultureInfo.InvariantCulture,
                    DateTimeStyles.AssumeLocal,
                    out var parsed)
                ? parsed.UtcTicks
                : index;
            var candidate = new TraceDecision(
                Clean(Read(row, decisionIndex)),
                timestamp,
                Clean(Read(row, gcrIndex)),
                sortValue);
            if (!latest.TryGetValue(folio, out var current) ||
                candidate.SortValue >= current.SortValue)
            {
                latest[folio] = candidate;
            }
        }

        return latest;
    }

    private NetworkResolution ResolveInterconnectionSubstation(
        SecondCallRow row,
        IReadOnlyList<NetworkElement> substations,
        IReadOnlyDictionary<string, int> substationNameCounts,
        IReadOnlyDictionary<string, int> substationScopeNameCounts,
        IReadOnlyList<LineEndpointReference> lineEndpoints)
    {
        if (!HasDistinctPrivateAndPublicSubstation(row))
        {
            return NetworkResolution.Empty("sin_referencia_independiente");
        }

        var publicRow = new SecondCallRow
        {
            PreFolio = row.PreFolio,
            ProjectFolio = row.ProjectFolio,
            ProjectName = row.ProjectName,
            Description = row.Description,
            Gcr = row.Gcr,
            State = row.State,
            Municipality = row.Municipality,
            DeclaredSubstation = row.InterconnectionSubstation,
            NormalizedSubstation =
                row.NormalizedInterconnectionSubstation,
            InterconnectionPoint = row.InterconnectionPoint,
            VoltageKv = row.VoltageKv,
            DeclaredDistanceKm = row.DeclaredDistanceKm,
            ProjectGeometry = row.ProjectGeometry,
            ProjectPoint = row.ProjectPoint,
            SubstationGeometry = row.SubstationGeometry,
            SubstationPoint = row.SubstationPoint,
            SubstationGeometryMatchesProject =
                row.SubstationGeometryMatchesProject,
            ProjectKml = row.ProjectKml,
            SubstationKml = row.SubstationKml,
            Decision = row.Decision,
            DecisionDate = row.DecisionDate,
            IsActive = row.IsActive
        };

        return ResolveSubstation(
            publicRow,
            substations,
            substationNameCounts,
            substationScopeNameCounts,
            lineEndpoints);
    }

    private NetworkResolution ResolveSubstation(
        SecondCallRow row,
        IReadOnlyList<NetworkElement> substations,
        IReadOnlyDictionary<string, int> substationNameCounts,
        IReadOnlyDictionary<string, int> substationScopeNameCounts,
        IReadOnlyList<LineEndpointReference> lineEndpoints)
    {
        if (string.IsNullOrWhiteSpace(row.NormalizedSubstation))
        {
            return NetworkResolution.Empty("sin_referencia");
        }

        var declaredAlias =
            NormalizeSubstationAlias(row.NormalizedSubstation);
        var declaredEquivalenceKey =
            NormalizeSubstationEquivalenceKey(declaredAlias);
        var rowGcr = ResolveGcrKey(row.Gcr);
        var rejectedTerritorialHomonyms = new List<string>();
        var candidates = new List<(
            NetworkElement Element,
            int Score,
            bool NameExact,
            bool NameEquivalent,
            bool NameStrong,
            double NameSimilarity,
            bool NameUnique,
            bool NameScopeUnique,
            bool? GcrCompatible,
            bool? VoltageCompatible,
            IReadOnlyList<string> VoltageSupportingLines,
            IReadOnlyList<double> NetworkVoltageLevelsKv,
            double? CalculatedDistance,
            bool? DistanceCompatible,
            double? DistanceDifference)>();
        foreach (var element in substations)
        {
            var score = 0;
            var catalogAlias =
                NormalizeSubstationAlias(element.NormalizedName);
            var catalogEquivalenceKey = element.EquivalenceKey;
            var nameSimilarity = CalculateNameSimilarity(
                declaredAlias,
                catalogAlias);
            var nameExact = string.Equals(
                    catalogAlias,
                    declaredAlias,
                    StringComparison.Ordinal);
            var nameEquivalent =
                !nameExact &&
                !string.IsNullOrWhiteSpace(declaredEquivalenceKey) &&
                string.Equals(
                    declaredEquivalenceKey,
                    catalogEquivalenceKey,
                    StringComparison.Ordinal);
            var nameStrong =
                nameExact ||
                nameEquivalent ||
                nameSimilarity >= _options.StrongNameSimilarity;
            if (nameExact)
            {
                score += 55;
            }
            else if (nameEquivalent)
            {
                score += 50;
            }
            else if (ContainsWhole(
                         $" {catalogAlias} ",
                         declaredAlias) ||
                     ContainsWhole(
                         $" {declaredAlias} ",
                         catalogAlias))
            {
                score += 35;
            }
            else if (nameStrong)
            {
                score += 30;
            }
            else
            {
                continue;
            }

            var nameUnique =
                (nameExact || nameEquivalent) &&
                substationNameCounts.GetValueOrDefault(
                    catalogEquivalenceKey) == 1;
            if (nameUnique)
            {
                score += _options.UniqueSubstationNameBonus;
            }
            var nameScopeUnique =
                (nameExact || nameEquivalent) &&
                !string.IsNullOrWhiteSpace(rowGcr) &&
                element.GcrKeys.Contains(rowGcr) &&
                substationScopeNameCounts.GetValueOrDefault(
                    $"{rowGcr}|{catalogEquivalenceKey}") == 1;
            if (!nameUnique && nameScopeUnique)
            {
                score += 15;
            }

            bool? gcrCompatible = null;
            if (!string.IsNullOrWhiteSpace(rowGcr) &&
                element.GcrKeys.Count > 0)
            {
                gcrCompatible = element.GcrKeys.Contains(rowGcr);
                if (gcrCompatible.Value)
                {
                    score += 20;
                }
                else if (!nameUnique)
                {
                    score -= 30;
                }
            }

            bool? voltageCompatible = null;
            var voltageNetworkSupport =
                FindVoltageNetworkSupport(
                    row,
                    element,
                    lineEndpoints);
            var voltageSupportingLines =
                voltageNetworkSupport.MatchingLines;
            if (row.VoltageKv.HasValue && element.VoltageKv.HasValue)
            {
                voltageCompatible =
                    Math.Abs(
                        row.VoltageKv.Value -
                        element.VoltageKv.Value) < 0.6;
                if (!voltageCompatible.Value &&
                    voltageSupportingLines.Count > 0)
                {
                    voltageCompatible = true;
                }
                score += voltageCompatible.Value ? 15 : -15;
            }
            else if (row.VoltageKv.HasValue &&
                     voltageSupportingLines.Count > 0)
            {
                voltageCompatible = true;
                score += 15;
            }

            double? calculatedDistance = null;
            bool? distanceCompatible = null;
            double? distanceDifference = null;
            var privateSubstationPoint =
                row.SubstationPoint ?? row.ProjectPoint;
            if (privateSubstationPoint.HasValue &&
                element.Point.HasValue)
            {
                calculatedDistance = HaversineKm(
                    privateSubstationPoint.Value,
                    element.Point.Value);
                if (row.DeclaredDistanceKm.HasValue)
                {
                    var tolerance = Math.Max(
                        _options.SubstationCoordinateToleranceKm,
                        row.DeclaredDistanceKm.Value * 0.25);
                    distanceDifference = Math.Abs(
                        calculatedDistance.Value -
                        row.DeclaredDistanceKm.Value);
                    if (distanceDifference.Value <= tolerance)
                    {
                        distanceCompatible = true;
                        score += 25;
                    }
                    else if (distanceDifference.Value <= tolerance * 2)
                    {
                        distanceCompatible = null;
                        score += 5;
                    }
                    else
                    {
                        distanceCompatible = false;
                        score -= 20;
                    }
                }
            }

            if (gcrCompatible == false &&
                calculatedDistance >= 100)
            {
                rejectedTerritorialHomonyms.Add(
                    $"{element.Name} · {calculatedDistance:0.##} km · GCR {string.Join("/", element.GcrKeys.OrderBy(value => value, StringComparer.Ordinal))}");
                continue;
            }

            candidates.Add((
                element,
                score,
                nameExact,
                nameEquivalent,
                nameStrong,
                nameSimilarity,
                nameUnique,
                nameScopeUnique,
                gcrCompatible,
                voltageCompatible,
                voltageSupportingLines,
                voltageNetworkSupport.LevelsKv,
                calculatedDistance,
                distanceCompatible,
                distanceDifference));
        }

        var ordered = candidates
            .OrderByDescending(candidate => candidate.Score)
            .ThenBy(candidate =>
                candidate.DistanceDifference ?? double.MaxValue)
            .ThenBy(candidate => candidate.Element.Key, StringComparer.Ordinal)
            .ToList();
        if (ordered.Count == 0)
        {
            var topology = ResolveTopologyEndpoint(row, lineEndpoints);
            if (topology.TopologyFirm)
            {
                return topology;
            }
            if (rejectedTerritorialHomonyms.Count > 0)
            {
                return new NetworkResolution(
                    "faltante",
                    null,
                    0,
                    null,
                    rejectedTerritorialHomonyms
                        .Select(value =>
                            $"homónimo de catálogo descartado por GCR incompatible y distancia extrema: {value}")
                        .ToList(),
                    row.DeclaredSubstation,
                    row.NormalizedSubstation)
                {
                    TerritorialHomonymRejected = true,
                    ScoreMargin = 100
                };
            }
            return topology;
        }

        var best = ordered[0];
        var topologyResolution =
            ResolveTopologyEndpoint(row, lineEndpoints);
        var topologyConfirmsCatalog =
            topologyResolution.TopologyFirm &&
            (best.NameExact || best.NameEquivalent) &&
            best.Element.Point.HasValue &&
            topologyResolution.TopologyPoints.Any(point =>
                HaversineKm(
                    best.Element.Point.Value,
                    point) <=
                _options.TopologyEndpointClusterKm);
        var preferTopology =
            topologyResolution.TopologyFirm &&
            !topologyConfirmsCatalog &&
            (!best.NameExact &&
             !best.NameEquivalent ||
             best.GcrCompatible != true ||
             best.VoltageCompatible != true ||
             (!row.SubstationGeometryMatchesProject &&
              best.CalculatedDistance >
                  _options.SourceToCatalogSubstationKm));
        if (preferTopology)
        {
            return topologyResolution;
        }
        if (HasPrivateSubstationQualifier(row.DeclaredSubstation) &&
            !best.NameExact &&
            !best.NameEquivalent)
        {
            return new NetworkResolution(
                "faltante",
                null,
                0,
                null,
                new[]
                {
                    $"coincidencia parcial {best.Element.Name} descartada para no homologar una instalación privada, propuesta o de maniobras con un nodo distinto"
                },
                row.DeclaredSubstation,
                row.NormalizedSubstation)
            {
                ScoreMargin = 100
            };
        }
        var scoreMargin = ordered.Count > 1
            ? best.Score - ordered[1].Score
            : 100;
        var ambiguous =
            scoreMargin < _options.StrongNameMargin;
        var effectiveScore = topologyConfirmsCatalog
            ? Math.Max(best.Score, 90)
            : best.Score;
        var effectiveGcrCompatible = topologyConfirmsCatalog
            ? true
            : best.GcrCompatible;
        var effectiveVoltageCompatible = topologyConfirmsCatalog
            ? true
            : best.VoltageCompatible;
        var effectiveSupportingLines = best.VoltageSupportingLines
            .Concat(topologyConfirmsCatalog
                ? topologyResolution.SupportingLines
                : Array.Empty<string>())
            .Distinct(StringComparer.OrdinalIgnoreCase)
            .OrderBy(value => value, StringComparer.OrdinalIgnoreCase)
            .ToList();
        var effectiveNetworkVoltageLevels =
            best.NetworkVoltageLevelsKv
                .Concat(topologyConfirmsCatalog
                    ? topologyResolution.NetworkVoltageLevelsKv
                    : Array.Empty<double>())
                .Distinct()
                .OrderByDescending(value => value)
                .ToList();
        var state = effectiveScore >= 85 && !ambiguous
            ? "catalogada"
            : effectiveScore >= 40
                ? "revision"
                : "faltante";
        var evidence = new List<string>
        {
            $"nombre comparado con catálogo ({effectiveScore} puntos)"
        };
        if (best.NameExact)
        {
            evidence.Add(best.NameUnique
                ? "nombre exacto tras normalización documental y único en el catálogo"
                : "nombre exacto tras normalización documental con homónimos o registros repetidos en el catálogo");
        }
        else if (best.NameEquivalent)
        {
            evidence.Add(
                "nombre equivalente tras retirar artículos y calificadores controlados de nomenclatura; se conservaron numerales y calificadores funcionales");
        }
        else
        {
            evidence.Add(
                $"similitud nominal: {best.NameSimilarity:P0}");
        }
        if (best.NameScopeUnique)
        {
            evidence.Add(
                $"nombre único dentro de la GCR {row.Gcr}");
        }
        if (effectiveGcrCompatible.HasValue)
        {
            evidence.Add(effectiveGcrCompatible.Value
                ? $"GCR compatible: {row.Gcr}"
                : $"GCR incompatible: {row.Gcr}");
        }
        if (effectiveVoltageCompatible.HasValue)
        {
            evidence.Add(effectiveVoltageCompatible.Value
                ? effectiveSupportingLines.Count > 0 &&
                  best.Element.VoltageKv.HasValue &&
                  Math.Abs(
                      best.Element.VoltageKv.Value -
                      (row.VoltageKv ?? best.Element.VoltageKv.Value)) >= 0.6
                    ? $"tensión {row.VoltageKv:0.##} kV respaldada por línea(s) que terminan nominal y espacialmente en la subestación: {string.Join(" · ", effectiveSupportingLines)}"
                    : $"tensión compatible: {row.VoltageKv:0.##} kV"
                : $"tensión incompatible: convocatoria {row.VoltageKv:0.##} kV / catálogo {best.Element.VoltageKv:0.##} kV");
        }
        if (topologyConfirmsCatalog)
        {
            evidence.Add(
                "el extremo nominal de línea coincide espacialmente con el punto exacto del catálogo y aporta GCR y tensión compatibles");
        }
        if (best.CalculatedDistance.HasValue &&
            row.DeclaredDistanceKm.HasValue)
        {
            evidence.Add(
                $"distancia calculada desde la subestación privada del proyecto al nodo de catálogo: {best.CalculatedDistance:0.##} km");
            evidence.Add(
                $"distancia declarada hacia la interconexión: {row.DeclaredDistanceKm:0.##} km");
            evidence.Add(best.DistanceCompatible switch
            {
                true => $"distancia compatible; diferencia {best.DistanceDifference:0.##} km",
                false => $"distancia incompatible; diferencia {best.DistanceDifference:0.##} km",
                _ => $"distancia aproximada; diferencia {best.DistanceDifference:0.##} km"
            });
        }
        if (ambiguous)
        {
            evidence.Add(
                $"existen candidatos con puntaje similar; margen {scoreMargin}");
        }
        if (string.Equals(state, "faltante", StringComparison.Ordinal))
        {
            var topology = ResolveTopologyEndpoint(row, lineEndpoints);
            if (topology.TopologyFirm)
            {
                return topology;
            }
        }

        return new NetworkResolution(
            state,
            best.Element,
            effectiveScore,
            best.CalculatedDistance,
            evidence,
            row.DeclaredSubstation,
            row.NormalizedSubstation)
        {
            NameExact = best.NameExact,
            NameEquivalent = best.NameEquivalent,
            NameStrong = best.NameStrong,
            NameSimilarity = best.NameSimilarity,
            NameUnique = best.NameUnique,
            NameScopeUnique = best.NameScopeUnique,
            GcrCompatible = effectiveGcrCompatible,
            VoltageCompatible = effectiveVoltageCompatible,
            DistanceCompatible = best.DistanceCompatible,
            DistanceDifferenceKm = best.DistanceDifference,
            ScoreMargin = scoreMargin,
            VoltageTopologyFirm =
                effectiveSupportingLines.Count > 0,
            SupportingLines = effectiveSupportingLines,
            NetworkVoltageLevelsKv =
                effectiveNetworkVoltageLevels
        };
    }

    private VoltageNetworkSupport FindVoltageNetworkSupport(
        SecondCallRow row,
        NetworkElement substation,
        IReadOnlyList<LineEndpointReference> lineEndpoints)
    {
        if (!row.VoltageKv.HasValue)
        {
            return VoltageNetworkSupport.Empty;
        }
        if (!row.SubstationGeometryMatchesProject &&
            row.SubstationPoint.HasValue &&
            substation.Point.HasValue &&
            HaversineKm(
                row.SubstationPoint.Value,
                substation.Point.Value) >
            _options.SourceToCatalogSubstationKm)
        {
            return VoltageNetworkSupport.Empty;
        }

        var catalogAlias =
            NormalizeSubstationAlias(substation.NormalizedName);
        var rowGcr = ResolveGcrKey(row.Gcr);
        var endpointReferences = lineEndpoints
            .Where(reference =>
                string.Equals(
                    reference.NormalizedName,
                    catalogAlias,
                    StringComparison.Ordinal) &&
                reference.VoltageKv.HasValue &&
                (string.IsNullOrWhiteSpace(rowGcr) ||
                 reference.GcrKeys.Contains(rowGcr)) &&
                (!substation.Point.HasValue ||
                 HaversineKm(
                     substation.Point.Value,
                     reference.Point) <=
                 _options.TopologyEndpointClusterKm))
            .ToList();
        var voltageLevels = endpointReferences
            .Select(reference => reference.VoltageKv!.Value)
            .Append(substation.VoltageKv ?? double.NaN)
            .Where(value => !double.IsNaN(value))
            .Distinct()
            .OrderByDescending(value => value)
            .ToList();
        var matchingLines = endpointReferences
            .Where(reference =>
                Math.Abs(
                    reference.VoltageKv!.Value -
                    row.VoltageKv.Value) < 0.6)
            .Select(reference => reference.LineName)
            .Where(value => !string.IsNullOrWhiteSpace(value))
            .Distinct(StringComparer.OrdinalIgnoreCase)
            .OrderBy(value => value, StringComparer.OrdinalIgnoreCase)
            .ToList();
        return new VoltageNetworkSupport(
            voltageLevels,
            matchingLines);
    }

    private NetworkResolution ResolveTopologyEndpoint(
        SecondCallRow row,
        IReadOnlyList<LineEndpointReference> lineEndpoints)
    {
        var declaredAlias =
            NormalizeTopologyEndpointAlias(row.NormalizedSubstation);
        var rowGcr = ResolveGcrKey(row.Gcr);
        var exactReferences = lineEndpoints
            .Where(reference => string.Equals(
                NormalizeTopologyEndpointAlias(
                    reference.NormalizedName),
                declaredAlias,
                StringComparison.Ordinal))
            .ToList();
        if (exactReferences.Count == 0)
        {
            return NetworkResolution.Empty("faltante");
        }

        var scopedReferences = !string.IsNullOrWhiteSpace(rowGcr)
            ? exactReferences
                .Where(reference => reference.GcrKeys.Contains(rowGcr))
                .ToList()
            : exactReferences;
        var references = scopedReferences.Count > 0
            ? scopedReferences
            : exactReferences;
        var gcrCompatible = !string.IsNullOrWhiteSpace(rowGcr) &&
            scopedReferences.Count > 0;
        bool? voltageCompatible = null;
        var referencesWithVoltage = references
            .Where(reference => reference.VoltageKv.HasValue)
            .ToList();
        if (row.VoltageKv.HasValue && referencesWithVoltage.Count > 0)
        {
            voltageCompatible = referencesWithVoltage.Any(reference =>
                Math.Abs(
                    reference.VoltageKv!.Value -
                    row.VoltageKv.Value) < 0.6);
        }

        var points = references
            .Select(reference => reference.Point)
            .Distinct()
            .ToList();
        var maximumSeparation = 0d;
        for (var first = 0; first < points.Count; first++)
        {
            for (var second = first + 1; second < points.Count; second++)
            {
                maximumSeparation = Math.Max(
                    maximumSeparation,
                    HaversineKm(points[first], points[second]));
            }
        }
        var clustered =
            points.Count > 0 &&
            maximumSeparation <= _options.TopologyEndpointClusterKm;
        var supportingLines = references
            .Select(reference => reference.LineName)
            .Where(value => !string.IsNullOrWhiteSpace(value))
            .Distinct(StringComparer.OrdinalIgnoreCase)
            .OrderBy(value => value, StringComparer.OrdinalIgnoreCase)
            .ToList();
        var topologyFirm =
            gcrCompatible &&
            voltageCompatible == true &&
            clustered &&
            supportingLines.Count > 0;
        var score =
            55 +
            (gcrCompatible ? 20 : 0) +
            (voltageCompatible == true ? 15 : 0) +
            (clustered ? 10 : 0);
        var evidence = new List<string>
        {
            $"nombre exacto en {supportingLines.Count} línea(s) del catálogo",
            $"extremos topológicos encontrados: {points.Count}",
            $"separación máxima entre extremos homónimos: {maximumSeparation:0.###} km"
        };
        evidence.Add(gcrCompatible
            ? $"extremo compatible con GCR {row.Gcr}"
            : "el extremo no pudo desambiguarse por GCR");
        evidence.Add(voltageCompatible switch
        {
            true => $"tensión de línea compatible: {row.VoltageKv:0.##} kV",
            false => "la tensión de las líneas de soporte es incompatible",
            _ => "sin tensión suficiente para confirmar el extremo"
        });

        return new NetworkResolution(
            "faltante",
            null,
            Math.Clamp(score, 0, 100),
            null,
            evidence,
            row.DeclaredSubstation,
            row.NormalizedSubstation)
        {
            NameExact = true,
            NameStrong = true,
            NameSimilarity = 1,
            NameScopeUnique = gcrCompatible,
            GcrCompatible = gcrCompatible,
            VoltageCompatible = voltageCompatible,
            ScoreMargin = 100,
            TopologyFirm = topologyFirm,
            TopologyPoints = points,
            SupportingLines = supportingLines,
            NetworkVoltageLevelsKv = referencesWithVoltage
                .Select(reference => reference.VoltageKv!.Value)
                .Distinct()
                .OrderByDescending(value => value)
                .ToList()
        };
    }

    private List<NetworkResolution> ResolveLines(
        SecondCallRow row,
        IReadOnlyList<NetworkElement> lines,
        IReadOnlyDictionary<string, LineTopologyEvidence> lineTopology)
    {
        var source = string.Join(
            Environment.NewLine,
            row.InterconnectionPoint,
            row.Description);
        var padded = $" {NormalizeText(source)} ";
        var results = new List<NetworkResolution>();

        NetworkResolution CreateCatalogResolution(
            NetworkElement element,
            int baseScore,
            string declaredName,
            string normalizedDeclaredName,
            string evidence,
            string matchMode = "nombre",
            string? stateOverride = null,
            bool endpointPairFirm = false,
            bool circuitCodeFirm = false,
            IReadOnlyList<string>? declaredCircuitCodes = null)
        {
            var topology = lineTopology.TryGetValue(
                element.Key,
                out var resolvedTopology)
                    ? resolvedTopology
                    : LineTopologyEvidence.Empty;
            var topologyBonus = topology.State switch
            {
                "conectada" => 15,
                "parcial" => 5,
                _ => 0
            };
            var score = Math.Clamp(baseScore + topologyBonus, 0, 100);
            return new NetworkResolution(
                stateOverride ??
                    (score >= 70 ? "catalogada" : "revision"),
                element,
                score,
                null,
                new[] { evidence }
                    .Concat(topology.Evidence)
                    .Distinct(StringComparer.Ordinal)
                    .ToList(),
                declaredName,
                normalizedDeclaredName)
            {
                LineTopologyState = topology.State,
                LineTopologyFirm = topology.IsFirm,
                ResolvedLineEndpoints = topology.ResolvedEndpoints,
                EndpointAName = topology.EndpointA.DeclaredName,
                EndpointBName = topology.EndpointB.DeclaredName,
                EndpointAResolvedName = topology.EndpointA.CatalogName,
                EndpointBResolvedName = topology.EndpointB.CatalogName,
                EndpointADistanceKm = topology.EndpointA.DistanceKm,
                EndpointBDistanceKm = topology.EndpointB.DistanceKm,
                LineTopologyEvidence = topology.Evidence,
                LineMatchMode = matchMode,
                LineEndpointPairFirm = endpointPairFirm,
                LineCircuitCodeFirm = circuitCodeFirm,
                DeclaredCircuitCodes =
                    declaredCircuitCodes ?? Array.Empty<string>(),
                CatalogCircuitCodes =
                    element.CircuitCodes
                        .OrderBy(value => value, StringComparer.Ordinal)
                        .ToList()
            };
        }

        foreach (var element in lines)
        {
            if (element.NormalizedName.Length < 5 ||
                !ContainsWhole(padded, element.NormalizedName))
            {
                continue;
            }

            var score = 60;
            if (row.VoltageKv.HasValue && element.VoltageKv.HasValue)
            {
                score += Math.Abs(row.VoltageKv.Value - element.VoltageKv.Value) < 0.6
                    ? 15
                    : -15;
            }
            results.Add(CreateCatalogResolution(
                element,
                score,
                element.Name,
                element.NormalizedName,
                "nombre de línea presente en los campos de interconexión",
                "nombre_catalogo"));
        }

        foreach (var declared in ExtractLineReferences(source))
        {
            var normalized = NormalizeLineName(declared);
            if (string.IsNullOrWhiteSpace(normalized) ||
                results.Any(result =>
                    string.Equals(
                        result.NormalizedDeclaredName,
                        normalized,
                        StringComparison.Ordinal)))
            {
                continue;
            }

            var candidates = lines
                .Where(line =>
                    string.Equals(
                        line.NormalizedName,
                        normalized,
                        StringComparison.Ordinal) ||
                    ContainsWhole($" {line.NormalizedName} ", normalized) ||
                    ContainsWhole($" {normalized} ", line.NormalizedName))
                .Select(line =>
                {
                    var score = string.Equals(
                        line.NormalizedName,
                        normalized,
                        StringComparison.Ordinal)
                            ? 60
                            : 40;
                    if (row.VoltageKv.HasValue && line.VoltageKv.HasValue)
                    {
                        score += Math.Abs(row.VoltageKv.Value - line.VoltageKv.Value) < 0.6
                            ? 15
                            : -15;
                    }
                    return (Element: line, Score: score);
                })
                .OrderByDescending(candidate => candidate.Score)
                .ToList();
            if (candidates.Count > 0)
            {
                var best = candidates[0];
                results.Add(CreateCatalogResolution(
                    best.Element,
                    best.Score,
                    declared,
                    normalized,
                    "referencia de línea comparada con el catálogo geoespacial",
                    "nombre_normalizado"));
                continue;
            }

            var declaredCodes = ExtractLineCircuitCodes(declared)
                .OrderBy(value => value, StringComparer.Ordinal)
                .ToList();
            var declaredSignature =
                NormalizeLineMatchSignature(declared);
            var rowGcr = ResolveGcrKey(row.Gcr);

            var signatureCandidates = lines
                .Where(line =>
                    !string.IsNullOrWhiteSpace(declaredSignature) &&
                    AreLineSignaturesEquivalent(
                        declaredSignature,
                        NormalizeLineMatchSignature(line.Name)))
                .Select(line => ScoreLineCandidate(
                    line,
                    row.VoltageKv,
                    rowGcr,
                    65,
                    lineTopology))
                .OrderByDescending(candidate => candidate.Score)
                .ToList();
            if (signatureCandidates.Count > 0)
            {
                var eligibleCandidates = signatureCandidates
                    .Where(candidate =>
                        candidate.VoltageCompatible != false &&
                        candidate.GcrCompatible != false)
                    .ToList();
                var distinctNames = eligibleCandidates
                    .Select(candidate =>
                        candidate.Element.NormalizedName)
                    .Distinct(StringComparer.Ordinal)
                    .ToList();
                var best = eligibleCandidates.FirstOrDefault() ??
                    signatureCandidates[0];
                var firm =
                    distinctNames.Count == 1 &&
                    best.VoltageCompatible != false &&
                    best.GcrCompatible != false;
                results.Add(CreateCatalogResolution(
                    best.Element,
                    best.Score,
                    declared,
                    normalized,
                    firm
                        ? "par de extremos equivalente, sin importar el sentido A–B/B–A, con tensión y territorio compatibles"
                        : "par de extremos equivalente con conflicto o ambigüedad pendiente de revisión",
                    "pares_extremos",
                    firm ? "catalogada" : "revision",
                    endpointPairFirm: firm,
                    declaredCircuitCodes: declaredCodes));
                continue;
            }

            var codeCandidates = declaredCodes.Count == 0
                ? new List<LineCodeResolutionCandidate>()
                : lines
                    .Where(line =>
                        line.CircuitCodes.Any(
                            declaredCodes.Contains))
                    .Select(line => new LineCodeResolutionCandidate(
                        ScoreLineCandidate(
                            line,
                            row.VoltageKv,
                            rowGcr,
                            55,
                            lineTopology),
                        CountLineSignatureOverlap(
                            declared,
                            line.Name)))
                    .OrderByDescending(candidate =>
                        candidate.TokenOverlap)
                    .ThenByDescending(candidate =>
                        candidate.Candidate.Score)
                    .ToList();
            if (codeCandidates.Count > 0)
            {
                var eligibleCandidates = codeCandidates
                    .Where(candidate =>
                        candidate.TokenOverlap > 0 &&
                        candidate.Candidate.VoltageCompatible == true &&
                        candidate.Candidate.GcrCompatible != false)
                    .ToList();
                var distinctNames = eligibleCandidates
                    .Select(candidate =>
                        candidate.Candidate.Element.NormalizedName)
                    .Distinct(StringComparer.Ordinal)
                    .ToList();
                var best = eligibleCandidates.FirstOrDefault();
                var firm =
                    best is not null &&
                    distinctNames.Count == 1 &&
                    best!.Candidate.VoltageCompatible == true &&
                    best.Candidate.GcrCompatible != false;
                if (firm)
                {
                    results.Add(CreateCatalogResolution(
                        best!.Candidate.Element,
                        best.Candidate.Score,
                        declared,
                        normalized,
                        $"código de circuito {string.Join("/", declaredCodes)} identifica de forma única el corredor base del GeoJSON y comparte {best.TokenOverlap} token(es) nominal(es); no demuestra por sí solo la geometría del nuevo entronque",
                        "codigo_circuito",
                        "corredor_catalogado",
                        circuitCodeFirm: true,
                        declaredCircuitCodes: declaredCodes));
                }
                else
                {
                    var candidateNames = codeCandidates
                        .Take(5)
                        .Select(candidate =>
                            candidate.Candidate.Element.Name)
                        .Distinct(
                            StringComparer.OrdinalIgnoreCase)
                        .ToList();
                    results.Add(new NetworkResolution(
                        "revision",
                        null,
                        0,
                        null,
                        new[]
                        {
                            $"código(s) {string.Join("/", declaredCodes)} encontrado(s), pero sin un corredor único compatible por nombre, tensión y GCR",
                            $"candidatos de referencia: {string.Join(" · ", candidateNames)}"
                        },
                        declared,
                        normalized)
                    {
                        LineTopologyState = "sin_geometria",
                        LineMatchMode = "codigo_ambiguo",
                        DeclaredCircuitCodes = declaredCodes,
                        LineTopologyEvidence = new[]
                        {
                            "El código de circuito no autoriza seleccionar una geometría arbitraria; la referencia permanece sin geometría hasta resolver la ambigüedad."
                        }
                    });
                }
                continue;
            }

            results.Add(new NetworkResolution(
                "tramo_sin_geometria",
                null,
                0,
                null,
                new[]
                {
                    "línea o tramo mencionado en Segunda Convocatoria sin geometría catalogada"
                },
                declared,
                normalized)
            {
                LineTopologyState = "sin_geometria",
                LineMatchMode = "sin_coincidencia",
                DeclaredCircuitCodes = declaredCodes,
                LineTopologyEvidence = new[]
                {
                    "Segunda Convocatoria menciona el tramo, pero el GeoJSON maestro no contiene una geometría homóloga ni un código de circuito único compatible."
                }
            });
        }

        return results
            .GroupBy(
                result => string.IsNullOrWhiteSpace(result.Element?.Key)
                    ? result.NormalizedDeclaredName
                    : result.Element.Key,
                StringComparer.Ordinal)
            .Select(group => group.OrderByDescending(value => value.Score).First())
            .ToList();
    }

    private static LineResolutionCandidate ScoreLineCandidate(
        NetworkElement line,
        double? declaredVoltageKv,
        string rowGcr,
        int baseScore,
        IReadOnlyDictionary<string, LineTopologyEvidence> lineTopology)
    {
        bool? voltageCompatible = null;
        if (declaredVoltageKv.HasValue &&
            line.VoltageKv.HasValue)
        {
            voltageCompatible =
                Math.Abs(
                    declaredVoltageKv.Value -
                    line.VoltageKv.Value) < 0.6;
        }

        bool? gcrCompatible = null;
        if (!string.IsNullOrWhiteSpace(rowGcr) &&
            line.GcrKeys.Count > 0)
        {
            gcrCompatible =
                line.GcrKeys.Contains(rowGcr);
        }

        var score = baseScore;
        score += voltageCompatible switch
        {
            true => 15,
            false => -20,
            _ => 0
        };
        score += gcrCompatible switch
        {
            true => 10,
            false => -20,
            _ => 0
        };
        if (lineTopology.TryGetValue(
                line.Key,
                out var topology))
        {
            score += topology.State switch
            {
                "conectada" => 15,
                "parcial" => 5,
                _ => 0
            };
        }

        return new LineResolutionCandidate(
            line,
            Math.Clamp(score, 0, 100),
            voltageCompatible,
            gcrCompatible);
    }

    private static IReadOnlyList<SubstationCandidateGroup>
        GroupSubstationRows(IReadOnlyList<SecondCallRow> rows)
    {
        var output = new List<SubstationCandidateGroup>();
        var baseGroups = rows
            .Where(row =>
                !string.IsNullOrWhiteSpace(
                    row.NormalizedSubstation))
            .GroupBy(
                row =>
                    $"{row.NormalizedSubstation}|" +
                    $"{row.VoltageKv:0.##}",
                StringComparer.Ordinal);
        foreach (var baseGroup in baseGroups)
        {
            var baseRows = baseGroup.ToList();
            var territoryGroups = baseRows
                .GroupBy(
                    row => BuildTerritoryKey(row),
                    StringComparer.Ordinal)
                .OrderByDescending(group =>
                    group.Count(row => row.IsActive))
                .ThenByDescending(group => group.Count())
                .ThenBy(group => group.Key, StringComparer.Ordinal)
                .ToList();
            if (territoryGroups.Count <= 1)
            {
                output.Add(new SubstationCandidateGroup(
                    baseRows,
                    string.Empty,
                    false,
                    territoryGroups.FirstOrDefault()?.Key ??
                    string.Empty));
                continue;
            }

            for (var index = 0;
                 index < territoryGroups.Count;
                 index++)
            {
                var territoryGroup = territoryGroups[index];
                output.Add(new SubstationCandidateGroup(
                    territoryGroup.ToList(),
                    index == 0
                        ? string.Empty
                        : $"TERRITORIO:{territoryGroup.Key}",
                    true,
                    territoryGroup.Key));
            }
        }
        return output;
    }

    private static string BuildTerritoryKey(SecondCallRow row)
    {
        var gcr = ResolveGcrKey(row.Gcr);
        var state = NormalizeText(row.State);
        return $"{FirstNonEmpty(gcr, "SIN_GCR")}|" +
            $"{FirstNonEmpty(state, "SIN_ENTIDAD")}";
    }

    private IReadOnlyList<TechnicalAnnexEvidence>
        LoadTechnicalAnnexEvidence()
    {
        var configuredPath = Clean(
            _options.TechnicalAnnexEvidencePath);
        if (string.IsNullOrWhiteSpace(configuredPath))
        {
            return Array.Empty<TechnicalAnnexEvidence>();
        }

        var candidatePaths = Path.IsPathRooted(configuredPath)
            ? new[] { configuredPath }
            : new[]
            {
                Path.GetFullPath(
                    configuredPath,
                    Directory.GetCurrentDirectory()),
                Path.GetFullPath(
                    configuredPath,
                    AppContext.BaseDirectory)
            };
        var evidencePath = candidatePaths
            .Distinct(StringComparer.OrdinalIgnoreCase)
            .FirstOrDefault(File.Exists);
        if (string.IsNullOrWhiteSpace(evidencePath))
        {
            _logger.LogWarning(
                "No se encontró el catálogo de evidencia técnica Conv2 en {Paths}.",
                string.Join(" · ", candidatePaths));
            return Array.Empty<TechnicalAnnexEvidence>();
        }

        try
        {
            var json = File.ReadAllText(evidencePath);
            var catalog =
                JsonSerializer.Deserialize<TechnicalAnnexEvidenceCatalog>(
                    json);
            var evidence = catalog?.Evidence
                .Where(item =>
                    !string.IsNullOrWhiteSpace(item.PreFolio) &&
                    !string.IsNullOrWhiteSpace(
                        item.DeclaredSubstation) &&
                    !string.IsNullOrWhiteSpace(item.Mode) &&
                    item.Point.HasValue)
                .ToList() ??
                new List<TechnicalAnnexEvidence>();
            _logger.LogInformation(
                "Evidencia técnica Conv2 cargada: {Version}, {Count} soporte(s).",
                catalog?.Version ?? "sin versión",
                evidence.Count);
            return evidence;
        }
        catch (Exception exception)
        {
            _logger.LogWarning(
                exception,
                "No fue posible leer la evidencia técnica Conv2 desde {Path}.",
                evidencePath);
            return Array.Empty<TechnicalAnnexEvidence>();
        }
    }

    private static TechnicalAnnexEvidence?
        ResolveTechnicalAnnexEvidence(
            IReadOnlyList<SecondCallRow> activeRows,
            IReadOnlyList<TechnicalAnnexEvidence> evidence)
    {
        if (activeRows.Count == 0 || evidence.Count == 0)
        {
            return null;
        }

        var byFolio = evidence
            .GroupBy(
                item => NormalizeFolio(item.PreFolio),
                StringComparer.Ordinal)
            .ToDictionary(
                group => group.Key,
                group => group.First(),
                StringComparer.Ordinal);
        var matches = new List<TechnicalAnnexEvidence>();
        foreach (var row in activeRows)
        {
            if (!byFolio.TryGetValue(
                    NormalizeFolio(row.PreFolio),
                    out var match))
            {
                return null;
            }
            if (!string.Equals(
                    NormalizeSubstationName(
                        row.DeclaredSubstation),
                    NormalizeSubstationName(
                        match.DeclaredSubstation),
                    StringComparison.Ordinal) ||
                !string.Equals(
                    ResolveGcrKey(row.Gcr),
                    ResolveGcrKey(match.Gcr),
                    StringComparison.Ordinal) ||
                !row.VoltageKv.HasValue ||
                Math.Abs(
                    row.VoltageKv.Value -
                    match.VoltageKv) >= 0.6)
            {
                return null;
            }
            matches.Add(match);
        }

        var representative = matches[0];
        var consistent = matches.All(item =>
            string.Equals(
                item.Mode,
                representative.Mode,
                StringComparison.OrdinalIgnoreCase) &&
            string.Equals(
                NormalizeSubstationName(
                    item.DeclaredSubstation),
                NormalizeSubstationName(
                    representative.DeclaredSubstation),
                StringComparison.Ordinal) &&
            string.Equals(
                NormalizeSubstationName(
                    item.CatalogMatch),
                NormalizeSubstationName(
                    representative.CatalogMatch),
                StringComparison.Ordinal) &&
            item.Point.HasValue &&
            representative.Point.HasValue &&
            HaversineKm(
                item.Point.Value,
                representative.Point.Value) <=
                Math.Max(
                    item.CatalogToleranceKm,
                    representative.CatalogToleranceKm));
        return consistent
            ? representative
            : null;
    }

    private PamConvocatoriaCoverageReport BuildCoverage(
        IReadOnlyList<SecondCallRow> rows,
        IReadOnlyList<TechnicalAnnexEvidence>
            technicalAnnexEvidence)
    {
        var candidates = new List<PamConvocatoriaInfrastructureCandidate>();
        var substationGroups = GroupSubstationRows(rows);
        foreach (var group in substationGroups)
        {
            var groupRows = group.Rows.ToList();
            var activeRows = groupRows
                .Where(row => row.IsActive)
                .ToList();
            var evaluationRows = activeRows.Count > 0
                ? activeRows
                : groupRows;
            var representative = evaluationRows
                .OrderByDescending(row => row.SubstationResolution.Score)
                .ThenByDescending(row => row.SubstationGeometry.HasValue)
                .First();
            var resolution = representative.SubstationResolution;
            var resolutions = evaluationRows
                .Select(row => row.SubstationResolution)
                .ToList();
            var territorialHomonymRejected = resolutions.Any(item =>
                item.TerritorialHomonymRejected);
            var directCatalogKeys = resolutions
                .Select(item => item.Element?.Key ?? string.Empty)
                .Where(value => !string.IsNullOrWhiteSpace(value))
                .Distinct(StringComparer.Ordinal)
                .ToList();
            var allCatalogued = resolutions.All(item =>
                string.Equals(
                    item.State,
                    "catalogada",
                    StringComparison.Ordinal));
            var oneCatalogResolution =
                directCatalogKeys.Count == 1 &&
                resolutions.All(item => item.Element is not null);
            var allExactNames = resolutions.All(item => item.NameExact);
            var allEquivalentNames = resolutions.All(item =>
                item.NameExact || item.NameEquivalent);
            var allStrongNames = resolutions.All(item => item.NameStrong);
            var nameUnique = resolutions.All(item => item.NameUnique);
            var nameScopeUnique = resolutions.All(item =>
                item.NameUnique || item.NameScopeUnique);
            var voltageConflict = resolutions.Any(item =>
                item.VoltageCompatible == false);
            var distanceConflict = resolutions.Any(item =>
                item.DistanceCompatible == false);
            var gcrConflict = resolutions.Any(item =>
                item.GcrCompatible == false);
            var effectiveGcrConflict =
                gcrConflict && !nameUnique;
            var gcrSupport = resolutions.Any(item =>
                item.GcrCompatible == true);
            var voltageSupport = resolutions.Any(item =>
                item.VoltageCompatible == true);
            var minimumNameSimilarity = resolutions.Min(item =>
                item.NameSimilarity);
            var minimumScoreMargin = resolutions.Min(item =>
                item.ScoreMargin);
            var independentEvidence = resolutions.Any(item =>
                item.VoltageCompatible == true ||
                item.DistanceCompatible == true);
            var declaredDistanceRows = groupRows.Count(row =>
                row.DeclaredDistanceKm.HasValue);
            var compatibleDistanceRows = resolutions.Count(item =>
                item.DistanceCompatible == true);
            var allDeclaredDistancesCompatible =
                declaredDistanceRows > 0 &&
                compatibleDistanceRows == declaredDistanceRows;
            var exactCatalogFirm =
                oneCatalogResolution &&
                allCatalogued &&
                allExactNames &&
                !voltageConflict &&
                !distanceConflict &&
                !effectiveGcrConflict &&
                independentEvidence &&
                (nameUnique ||
                 allDeclaredDistancesCompatible ||
                 (nameScopeUnique && gcrSupport));
            var strongCatalogFirm =
                oneCatalogResolution &&
                allCatalogued &&
                allStrongNames &&
                (allEquivalentNames ||
                 minimumNameSimilarity >= _options.StrongNameSimilarity) &&
                minimumScoreMargin >= _options.StrongNameMargin &&
                nameScopeUnique &&
                gcrSupport &&
                (voltageSupport || allDeclaredDistancesCompatible) &&
                !voltageConflict &&
                !distanceConflict &&
                !effectiveGcrConflict;
            var topologyFirm =
                directCatalogKeys.Count == 0 &&
                resolutions.All(item => item.TopologyFirm);
            var voltageTopologyFirm = resolutions.Any(item =>
                item.VoltageTopologyFirm);
            var networkVoltageLevels = resolutions
                .SelectMany(item => item.NetworkVoltageLevelsKv)
                .Distinct()
                .OrderByDescending(value => value)
                .ToList();
            var sourceGeoreferencedRows = activeRows
                .Where(row =>
                    row.SubstationGeometry.HasValue &&
                    row.SubstationPoint.HasValue &&
                    row.VoltageKv.HasValue &&
                    !string.IsNullOrWhiteSpace(row.Gcr) &&
                    !string.IsNullOrWhiteSpace(row.NormalizedSubstation))
                .ToList();
            var sourceGeoreferencedPoints = sourceGeoreferencedRows
                .Select(row => row.SubstationPoint!.Value)
                .Distinct()
                .ToList();
            var sourceMaximumSeparation =
                MaximumSeparationKm(sourceGeoreferencedPoints);
            var sourceGeoreferencedBaseFirm =
                activeRows.Count > 0 &&
                sourceGeoreferencedRows.Count == activeRows.Count &&
                sourceGeoreferencedPoints.Count > 0 &&
                sourceMaximumSeparation <=
                    _options.SourceSubstationClusterKm;
            var privatePublicRows = activeRows
                .Where(HasDistinctPrivateAndPublicSubstation)
                .ToList();
            var publicInterconnectionResolutions = privatePublicRows
                .Select(row =>
                    row.InterconnectionSubstationResolution)
                .ToList();
            var publicInterconnectionKeys =
                publicInterconnectionResolutions
                    .Select(item => item.Element?.Key ?? string.Empty)
                    .Where(value => !string.IsNullOrWhiteSpace(value))
                    .Distinct(StringComparer.Ordinal)
                    .ToList();
            var distinctPrivatePublicFirm =
                sourceGeoreferencedBaseFirm &&
                privatePublicRows.Count == activeRows.Count &&
                publicInterconnectionKeys.Count == 1 &&
                publicInterconnectionResolutions.Count > 0 &&
                publicInterconnectionResolutions.All(
                    IsFirmPublicInterconnection);
            var publicInterconnectionResolution =
                publicInterconnectionResolutions
                    .OrderByDescending(item => item.Score)
                    .FirstOrDefault();
            var publicInterconnectionRow = privatePublicRows
                .OrderByDescending(row =>
                    row.InterconnectionSubstationResolution.Score)
                .FirstOrDefault();
            var technicalAnnex =
                ResolveTechnicalAnnexEvidence(
                    activeRows,
                    technicalAnnexEvidence);
            var technicalAnnexPoint = technicalAnnex?.Point;
            var technicalCatalogDistance =
                technicalAnnexPoint.HasValue &&
                resolution.Element?.Point is GeoPoint catalogPoint
                    ? HaversineKm(
                        technicalAnnexPoint.Value,
                        catalogPoint)
                    : (double?)null;
            var technicalCatalogFirm =
                technicalAnnex is not null &&
                string.Equals(
                    technicalAnnex.Mode,
                    "catalogo_multitension",
                    StringComparison.OrdinalIgnoreCase) &&
                resolution.Element is not null &&
                !string.IsNullOrWhiteSpace(
                    technicalAnnex.CatalogMatch) &&
                string.Equals(
                    NormalizeSubstationName(
                        resolution.Element.Name),
                    NormalizeSubstationName(
                        technicalAnnex.CatalogMatch),
                    StringComparison.Ordinal) &&
                technicalCatalogDistance.HasValue &&
                technicalCatalogDistance.Value <=
                    technicalAnnex.CatalogToleranceKm;
            var technicalMissingFirm =
                technicalAnnex is not null &&
                string.Equals(
                    technicalAnnex.Mode,
                    "referencia_faltante_distinta",
                    StringComparison.OrdinalIgnoreCase) &&
                (!technicalCatalogDistance.HasValue ||
                 technicalCatalogDistance.Value >
                    technicalAnnex.CatalogToleranceKm);
            var technicalAnnexFirm =
                technicalCatalogFirm ||
                technicalMissingFirm;
            if (distinctPrivatePublicFirm ||
                technicalMissingFirm)
            {
                exactCatalogFirm = false;
                strongCatalogFirm = false;
                topologyFirm = false;
            }
            var catalogKeys =
                distinctPrivatePublicFirm ||
                technicalMissingFirm
                ? new List<string>()
                : directCatalogKeys;
            var sourceGeoreferencedFirm =
                catalogKeys.Count == 0 &&
                !technicalAnnexFirm &&
                sourceGeoreferencedBaseFirm;
            var sourceSupports = sourceGeoreferencedRows
                .Select(row =>
                    $"{NormalizeFolio(row.PreFolio)} · " +
                    $"{row.Gcr} · {row.VoltageKv:0.##} kV")
                .Distinct(StringComparer.OrdinalIgnoreCase)
                .OrderBy(value => value, StringComparer.OrdinalIgnoreCase)
                .ToList();
            var independentActiveProjects = activeRows
                .Select(row => FirstNonEmpty(
                    row.ProjectFolio,
                    row.ProjectName,
                    row.PreFolio))
                .Select(NormalizeText)
                .Where(value => !string.IsNullOrWhiteSpace(value))
                .Distinct(StringComparer.Ordinal)
                .ToList();
            var activeGcrKeys = activeRows
                .Select(row => ResolveGcrKey(row.Gcr))
                .Where(value => !string.IsNullOrWhiteSpace(value))
                .Distinct(StringComparer.Ordinal)
                .ToList();
            var activeVoltageLevels = activeRows
                .Where(row => row.VoltageKv.HasValue)
                .Select(row => Math.Round(row.VoltageKv!.Value, 2))
                .Distinct()
                .ToList();
            var corroborationSupports = activeRows
                .Select(row =>
                    $"{NormalizeFolio(row.PreFolio)} · " +
                    $"{row.ProjectName} · {row.Gcr} · " +
                    $"{row.VoltageKv:0.##} kV")
                .Distinct(StringComparer.OrdinalIgnoreCase)
                .OrderBy(value => value, StringComparer.OrdinalIgnoreCase)
                .ToList();
            var multiSourceCorroborationFirm =
                catalogKeys.Count == 0 &&
                !topologyFirm &&
                !sourceGeoreferencedBaseFirm &&
                !distinctPrivatePublicFirm &&
                !technicalAnnexFirm &&
                activeRows.Count >= 3 &&
                activeRows
                    .Select(row => NormalizeFolio(row.PreFolio))
                    .Where(value => !string.IsNullOrWhiteSpace(value))
                    .Distinct(StringComparer.Ordinal)
                    .Count() >= 3 &&
                independentActiveProjects.Count >= 3 &&
                activeGcrKeys.Count == 1 &&
                activeVoltageLevels.Count == 1;
            var automaticFirm =
                exactCatalogFirm ||
                strongCatalogFirm ||
                topologyFirm ||
                sourceGeoreferencedFirm ||
                distinctPrivatePublicFirm ||
                multiSourceCorroborationFirm ||
                technicalAnnexFirm;
            var topologyPoints = resolutions
                .SelectMany(item => item.TopologyPoints)
                .Distinct()
                .ToList();
            var supportingLines = resolutions
                .SelectMany(item => item.SupportingLines)
                .Distinct(StringComparer.OrdinalIgnoreCase)
                .OrderBy(value => value, StringComparer.OrdinalIgnoreCase)
                .ToList();
            var effectiveDistanceResolutions = distinctPrivatePublicFirm
                ? publicInterconnectionResolutions
                : resolutions;
            var calculatedDistances = effectiveDistanceResolutions
                .Where(item => item.DistanceKm.HasValue)
                .Select(item => item.DistanceKm!.Value)
                .ToList();
            var distanceDifferences = effectiveDistanceResolutions
                .Where(item => item.DistanceDifferenceKm.HasValue)
                .Select(item => item.DistanceDifferenceKm!.Value)
                .ToList();
            var privateSubstationPoints = evaluationRows
                .Where(row => row.SubstationPoint.HasValue)
                .Select(row => row.SubstationPoint!.Value)
                .Distinct()
                .ToList();
            var candidateEvidence = resolutions
                .SelectMany(item => item.Evidence)
                .Distinct(StringComparer.Ordinal)
                .ToList();
            if (distinctPrivatePublicFirm)
            {
                candidateEvidence.AddRange(
                    publicInterconnectionResolutions
                        .SelectMany(item => item.Evidence)
                        .Select(value => $"interconexión pública: {value}")
                        .Distinct(StringComparer.Ordinal));
            }
            candidateEvidence.Add(
                $"consistencia vigente: {catalogKeys.Count} resolución(es) de catálogo distinta(s), {evaluationRows.Count} fila(s) evaluada(s) de {groupRows.Count} histórica(s)");
            if (group.IsTerritoriallySplit)
            {
                candidateEvidence.Add(
                    $"grupo nominal separado por territorio: {group.TerritoryKey}");
            }
            if (declaredDistanceRows > 0)
            {
                candidateEvidence.Add(
                    $"distancias declaradas compatibles: {compatibleDistanceRows} de {declaredDistanceRows}");
            }
            if (sourceGeoreferencedRows.Count > 0)
            {
                candidateEvidence.Add(
                    $"fuente vigente con geometría explícita de subestación, tensión y GCR: {sourceGeoreferencedRows.Count} de {activeRows.Count} fila(s)");
                candidateEvidence.Add(
                    $"separación máxima entre puntos de subestación de la fuente: {sourceMaximumSeparation:0.###} km");
            }
            var repeatedProjectGeometryRows = evaluationRows.Count(row =>
                row.SubstationGeometryMatchesProject);
            if (repeatedProjectGeometryRows > 0)
            {
                candidateEvidence.Add(
                    $"la geometría capturada para subestación replica la geometría del proyecto en {repeatedProjectGeometryRows} fila(s); no se usa como evidencia independiente para descartar un nodo exacto del catálogo");
            }
            if (voltageTopologyFirm)
            {
                candidateEvidence.Add(
                    "la tensión de la convocatoria está respaldada por líneas que terminan nominal y espacialmente en la misma subestación de catálogo");
                candidateEvidence.Add(
                    $"niveles de tensión observados en el nodo y sus líneas conectadas: {string.Join(", ", networkVoltageLevels.Select(value => $"{value:0.##} kV"))}");
            }
            if (distinctPrivatePublicFirm &&
                publicInterconnectionResolution?.Element is not null)
            {
                candidateEvidence.Add(
                    $"la fuente distingue la subestación privada/propuesta {representative.DeclaredSubstation} del punto público de interconexión {publicInterconnectionResolution.Element.Name}");
                candidateEvidence.Add(
                    "la coincidencia nominal directa de la instalación privada se excluyó como asociación oficial para no confundir instalaciones distintas");
                candidateEvidence.Add(
                    "la relación con el nodo público proviene del campo de interconexión de la fuente; no se infiere por proximidad ni por cruce visual");
            }
            if (multiSourceCorroborationFirm)
            {
                candidateEvidence.Add(
                    $"referencia corroborada por {independentActiveProjects.Count} proyectos vigentes independientes, {activeGcrKeys[0]} y {activeVoltageLevels[0]:0.##} kV");
                candidateEvidence.Add(
                    $"la corroboración confirma la referencia documental ausente del catálogo; las {sourceGeoreferencedRows.Count} geometría(s) capturadas no alcanzaron consistencia espacial y no se usan para ubicar ni conectar el elemento");
            }
            if (technicalAnnexFirm &&
                technicalAnnex is not null)
            {
                candidateEvidence.AddRange(
                    technicalAnnex.Evidence
                        .Select(value =>
                            $"anexo técnico: {value}"));
                candidateEvidence.Add(
                    $"anexo técnico verificable: {technicalAnnex.Document}; SHA-256 {technicalAnnex.DocumentSha256}");
                candidateEvidence.Add(
                    $"fuente pública del anexo: {technicalAnnex.SourceUrl}");
                if (technicalCatalogDistance.HasValue)
                {
                    candidateEvidence.Add(
                        $"distancia entre el punto técnico del anexo y el candidato de catálogo: {technicalCatalogDistance:0.###} km");
                }
                candidateEvidence.Add(
                    technicalCatalogFirm
                        ? "el anexo confirma el mismo nodo de catálogo y documenta un nivel de tensión adicional"
                        : "el anexo confirma una subestación de maniobras distinta del homónimo de catálogo; no se homologa ni se infiere conectividad por proximidad");
            }

            var automationReason = automaticFirm
                ? technicalCatalogFirm
                    ? "Anexo técnico trazable: el unifilar identifica el nodo público, su tensión y una geometría que coincide con el punto de catálogo dentro de la tolerancia."
                  : technicalMissingFirm
                    ? "Anexo técnico trazable: documenta una subestación de maniobras distinta del homónimo de catálogo mediante función, tensión y coordenadas explícitas."
                  : multiSourceCorroborationFirm
                    ? "Referencia ausente del catálogo corroborada por al menos tres proyectos vigentes independientes con nombre, GCR y tensión consistentes; confirma la referencia documental, no su ubicación ni conectividad."
                : territorialHomonymRejected &&
                  sourceGeoreferencedFirm
                    ? "Homónimo de catálogo descartado por GCR incompatible y distancia extrema; la referencia se conserva como subestación de fuente georreferenciada faltante, sin promoverla al catálogo oficial."
                    : distinctPrivatePublicFirm
                    ? "Subestación privada o propuesta georreferenciada y nodo público de interconexión resueltos por separado; la relación es declarada por la fuente y no convierte la instalación privada en nodo oficial."
                    : topologyFirm
                    ? "Referencia faltante confirmable por extremo nominal de línea, GCR, tensión y agrupación geográfica compatibles."
                    : sourceGeoreferencedFirm
                        ? "Referencia faltante confirmable como subestación privada o propuesta georreferenciada en la fuente vigente; no confirma conexión eléctrica ni la convierte en nodo oficial."
                    : allEquivalentNames && !allExactNames
                        ? "Nombre equivalente tras normalización controlada de nomenclatura, candidato único, GCR y tensión compatibles."
                    : strongCatalogFirm && !allExactNames
                        ? "Nombre fuertemente similar, candidato único en GCR, tensión compatible y margen suficiente."
                        : nameUnique
                            ? "Nombre exacto único, misma clave de catálogo, evidencia independiente y sin conflictos."
                            : nameScopeUnique && gcrSupport
                                ? "Nombre exacto desambiguado por GCR y evidencia eléctrica compatible."
                                : "Nombre exacto repetido, pero todas las distancias declaradas son compatibles con la misma clave y no hay conflictos."
                : catalogKeys.Count == 0
                    ? supportingLines.Count > 0
                        ? "Existe como extremo nominal de línea, pero falta evidencia suficiente para confirmarlo automáticamente."
                        : "Sin coincidencia en el catálogo."
                    : catalogKeys.Count > 1
                        ? "El grupo resuelve a más de una clave de catálogo."
                        : voltageConflict ||
                          distanceConflict ||
                          effectiveGcrConflict
                            ? "Existe conflicto de tensión, distancia declarada o GCR."
                            : !allStrongNames
                                ? "La coincidencia nominal no alcanza el umbral fuerte."
                                : minimumScoreMargin < _options.StrongNameMargin
                                    ? "El margen entre candidatos es insuficiente."
                                : !independentEvidence
                                    ? "Falta una segunda evidencia independiente."
                                    : !nameScopeUnique && !allDeclaredDistancesCompatible
                                        ? "El nombre tiene homónimos sin desambiguación territorial o por distancia."
                                        : "La evidencia agregada requiere revisión.";
            if (automaticFirm)
            {
                candidateEvidence.Add(
                    "coincidencia automática firme por criterio compuesto v11");
            }

            var candidateState = automaticFirm && catalogKeys.Count > 0
                ? "catalogada"
                : catalogKeys.Count == 0
                    ? "faltante"
                    : "revision";
            var mayPublishCatalogGeometry = catalogKeys.Count > 0;
            var catalogElement = mayPublishCatalogGeometry
                ? resolution.Element
                : null;
            var geometry = mayPublishCatalogGeometry
                ? catalogElement?.Geometry
                : null;
            var point = mayPublishCatalogGeometry
                ? catalogElement?.Point
                : technicalAnnexFirm
                    ? technicalAnnexPoint
                    : null;
            var technicalAnnexGeometry =
                technicalAnnexFirm &&
                technicalAnnexPoint.HasValue
                    ? CreatePointCollectionGeometry(
                        new[] { technicalAnnexPoint.Value })
                    : null;
            var candidateId = string.IsNullOrWhiteSpace(
                group.CandidateIdDiscriminator)
                    ? StableKey(
                        "C2-SE",
                        representative.NormalizedSubstation,
                        representative.VoltageKv?.ToString(
                            "0.##",
                            CultureInfo.InvariantCulture) ?? string.Empty)
                    : StableKey(
                        "C2-SE",
                        representative.NormalizedSubstation,
                        representative.VoltageKv?.ToString(
                            "0.##",
                            CultureInfo.InvariantCulture) ?? string.Empty,
                        group.CandidateIdDiscriminator);
            candidates.Add(new PamConvocatoriaInfrastructureCandidate
            {
                CandidatoId = candidateId,
                TipoElemento = "subestacion",
                Estado = candidateState,
                NombreDeclarado = representative.DeclaredSubstation,
                CoincidenciaCatalogo = catalogElement?.Name ?? string.Empty,
                ClaveCatalogo = catalogElement?.Key ?? string.Empty,
                Puntaje = sourceGeoreferencedFirm ||
                          multiSourceCorroborationFirm ||
                          technicalAnnexFirm
                    ? _options.HighConfidenceThreshold
                    : Math.Clamp(resolution.Score, 0, 100),
                TensionKv = representative.VoltageKv,
                DistanciaCatalogoKm = catalogElement is null
                    ? null
                    : technicalCatalogFirm
                        ? technicalCatalogDistance
                        : resolution.DistanceKm,
                GeometriasSubestacionPrivada =
                    privateSubstationPoints.Count,
                DistanciasDeclaradas = declaredDistanceRows,
                DistanciasCompatibles = compatibleDistanceRows,
                DistanciaInterconexionCalculadaMinimaKm =
                    calculatedDistances.Count > 0
                        ? calculatedDistances.Min()
                        : null,
                DistanciaInterconexionCalculadaMaximaKm =
                    calculatedDistances.Count > 0
                        ? calculatedDistances.Max()
                        : null,
                DiferenciaDistanciaMaximaKm =
                    distanceDifferences.Count > 0
                        ? distanceDifferences.Max()
                        : null,
                ResolucionesCatalogoDistintas = catalogKeys.Count,
                NombreCatalogoUnico =
                    !distinctPrivatePublicFirm && nameUnique,
                NombreCoincidenciaFuerte =
                    !distinctPrivatePublicFirm && allStrongNames,
                NombreEquivalenteCatalogo =
                    !distinctPrivatePublicFirm &&
                    allEquivalentNames &&
                    !allExactNames,
                SimilitudNombre = distinctPrivatePublicFirm
                    ? 0
                    : minimumNameSimilarity,
                MargenPuntaje = distinctPrivatePublicFirm
                    ? publicInterconnectionResolution?.ScoreMargin ?? 0
                    : minimumScoreMargin,
                GcrCatalogo = catalogElement is null
                    ? string.Empty
                    : string.Join(
                        ", ",
                        catalogElement.GcrKeys.OrderBy(
                            value => value,
                            StringComparer.Ordinal)),
                CoincidenciaTopologicaFirme = topologyFirm,
                CoincidenciaTensionTopologicaFirme =
                    voltageTopologyFirm,
                NivelesTensionRedKv = networkVoltageLevels,
                HomonimoTerritorialDescartado =
                    territorialHomonymRejected,
                GrupoTerritorialSeparado = group.IsTerritoriallySplit,
                ClaveAgrupacionTerritorial =
                    group.TerritoryKey,
                CoincidenciaFuenteGeorreferenciadaFirme =
                    sourceGeoreferencedFirm,
                CoincidenciaInterconexionPublicaFirme =
                    distinctPrivatePublicFirm,
                NombreInterconexionPublica =
                    publicInterconnectionRow?.InterconnectionSubstation ??
                    string.Empty,
                CoincidenciaInterconexionCatalogo =
                    distinctPrivatePublicFirm
                        ? publicInterconnectionResolution?.Element?.Name ??
                          string.Empty
                        : string.Empty,
                ClaveInterconexionCatalogo =
                    distinctPrivatePublicFirm
                        ? publicInterconnectionResolution?.Element?.Key ??
                          string.Empty
                        : string.Empty,
                DistanciaInterconexionPublicaKm =
                    distinctPrivatePublicFirm
                        ? publicInterconnectionResolution?.DistanceKm
                        : null,
                FilasFuenteGeorreferenciada =
                    sourceGeoreferencedRows.Count,
                SeparacionMaximaFuenteKm =
                    sourceGeoreferencedRows.Count > 0
                        ? sourceMaximumSeparation
                        : null,
                SoportesFuente = sourceSupports,
                CoincidenciaCorroboracionMultifuenteFirme =
                    multiSourceCorroborationFirm,
                ProyectosIndependientesCorroborados =
                    independentActiveProjects.Count,
                SoportesCorroboracion = corroborationSupports,
                CoincidenciaAnexoTecnicoFirme =
                    technicalAnnexFirm,
                ModoAnexoTecnico =
                    technicalAnnex?.Mode ?? string.Empty,
                FuenteAnexoTecnico =
                    technicalAnnex?.SourceUrl ?? string.Empty,
                Sha256AnexoTecnico =
                    technicalAnnex?.DocumentSha256 ?? string.Empty,
                SoportesAnexoTecnico =
                    technicalAnnex?.Evidence ??
                    Array.Empty<string>(),
                CoincidenciaAutomaticaFirme = automaticFirm,
                MotivoAutomatizacion = automationReason,
                LineasSoporte = supportingLines,
                GeometriaSubestacionPrivada =
                    CreateGeometry(privateSubstationPoints),
                GeometriaInterconexionPublica =
                    distinctPrivatePublicFirm
                        ? publicInterconnectionResolution?.Element?.Geometry
                        : null,
                GeometriaTopologicaSugerida =
                    CreatePointCollectionGeometry(topologyPoints),
                GeometriaAnexoTecnico =
                    technicalAnnexGeometry,
                Gcr = FirstNonEmpty(evaluationRows.Select(row => row.Gcr).ToArray()),
                Entidad = FirstNonEmpty(evaluationRows.Select(row => row.State).ToArray()),
                Municipio = FirstNonEmpty(evaluationRows.Select(row => row.Municipality).ToArray()),
                ProyectosRelacionados = groupRows
                    .Select(row => NormalizeFolio(row.PreFolio))
                    .Where(value => !string.IsNullOrWhiteSpace(value))
                    .Distinct(StringComparer.Ordinal)
                    .Count(),
                ProyectosVigentes = groupRows
                    .Where(row => row.IsActive)
                    .Select(row => NormalizeFolio(row.PreFolio))
                    .Where(value => !string.IsNullOrWhiteSpace(value))
                    .Distinct(StringComparer.Ordinal)
                    .Count(),
                Decisiones = groupRows
                    .Select(row => FirstNonEmpty(row.Decision, "Sin decisión"))
                    .Distinct(StringComparer.OrdinalIgnoreCase)
                    .OrderBy(value => value, StringComparer.OrdinalIgnoreCase)
                    .ToList(),
                Folios = groupRows
                    .Select(row => row.PreFolio)
                    .Where(value => !string.IsNullOrWhiteSpace(value))
                    .Distinct(StringComparer.OrdinalIgnoreCase)
                    .Take(20)
                    .ToList(),
                Evidencias = candidateEvidence,
                Geometria = geometry ??
                    (technicalMissingFirm
                        ? technicalAnnexGeometry
                        : null),
                Latitud = point?.Latitude,
                Longitud = point?.Longitude,
                Fuente = _options.BaseUrl
            });
        }

        var lineGroups = rows
            .SelectMany(row => row.LineResolutions.Select(line => (Row: row, Line: line)))
            .Where(item => !string.IsNullOrWhiteSpace(item.Line.NormalizedDeclaredName))
            .GroupBy(
                item => $"{item.Line.NormalizedDeclaredName}|{item.Row.VoltageKv:0.##}",
                StringComparer.Ordinal);
        foreach (var group in lineGroups)
        {
            var representative = group
                .OrderByDescending(item => item.Line.Score)
                .First();
            var resolution = representative.Line;
            candidates.Add(new PamConvocatoriaInfrastructureCandidate
            {
                CandidatoId = StableKey(
                    "C2-LT",
                    resolution.NormalizedDeclaredName,
                    representative.Row.VoltageKv?.ToString(
                        "0.##",
                        CultureInfo.InvariantCulture) ?? string.Empty),
                TipoElemento = "linea_transmision",
                Estado = resolution.State,
                NombreDeclarado = resolution.DeclaredName,
                CoincidenciaCatalogo = resolution.Element?.Name ?? string.Empty,
                ClaveCatalogo = resolution.Element?.Key ?? string.Empty,
                Puntaje = Math.Clamp(resolution.Score, 0, 100),
                TensionKv = representative.Row.VoltageKv,
                EstadoConexionGrafo = resolution.LineTopologyState,
                CoincidenciaGrafoFirme =
                    resolution.LineTopologyFirm,
                ExtremosResueltosGrafo =
                    resolution.ResolvedLineEndpoints,
                ExtremoADeclarado = resolution.EndpointAName,
                ExtremoBDeclarado = resolution.EndpointBName,
                ExtremoACatalogo =
                    resolution.EndpointAResolvedName,
                ExtremoBCatalogo =
                    resolution.EndpointBResolvedName,
                DistanciaExtremoAKm =
                    resolution.EndpointADistanceKm,
                DistanciaExtremoBKm =
                    resolution.EndpointBDistanceKm,
                SoportesGrafo =
                    resolution.LineTopologyEvidence,
                ModoCoincidenciaLinea =
                    resolution.LineMatchMode,
                CoincidenciaParExtremosFirme =
                    resolution.LineEndpointPairFirm,
                CoincidenciaCodigoCircuitoFirme =
                    resolution.LineCircuitCodeFirm,
                CodigosCircuitoDeclarados =
                    resolution.DeclaredCircuitCodes,
                CodigosCircuitoCatalogo =
                    resolution.CatalogCircuitCodes,
                Gcr = FirstNonEmpty(group.Select(item => item.Row.Gcr).ToArray()),
                Entidad = FirstNonEmpty(group.Select(item => item.Row.State).ToArray()),
                Municipio = FirstNonEmpty(group.Select(item => item.Row.Municipality).ToArray()),
                ProyectosRelacionados = group
                    .Select(item => item.Row.PreFolio)
                    .Distinct(StringComparer.Ordinal)
                    .Count(),
                ProyectosVigentes = group
                    .Where(item => item.Row.IsActive)
                    .Select(item => NormalizeFolio(item.Row.PreFolio))
                    .Where(value => !string.IsNullOrWhiteSpace(value))
                    .Distinct(StringComparer.Ordinal)
                    .Count(),
                Decisiones = group
                    .Select(item => FirstNonEmpty(
                        item.Row.Decision,
                        "Sin decisión"))
                    .Distinct(StringComparer.OrdinalIgnoreCase)
                    .OrderBy(value => value, StringComparer.OrdinalIgnoreCase)
                    .ToList(),
                Folios = group
                    .Select(item => item.Row.PreFolio)
                    .Where(value => !string.IsNullOrWhiteSpace(value))
                    .Distinct(StringComparer.OrdinalIgnoreCase)
                    .Take(20)
                    .ToList(),
                Evidencias = resolution.Evidence,
                Geometria = resolution.Element?.Geometry,
                Latitud = resolution.Element?.Point?.Latitude,
                Longitud = resolution.Element?.Point?.Longitude,
                Fuente = _options.BaseUrl
            });
        }

        var projectKeys = rows
            .Select(row => NormalizeFolio(row.PreFolio))
            .Where(value => !string.IsNullOrWhiteSpace(value))
            .Distinct(StringComparer.Ordinal)
            .ToHashSet(StringComparer.Ordinal);
        var tracedProjectKeys = rows
            .Where(row => !string.IsNullOrWhiteSpace(row.Decision))
            .Select(row => NormalizeFolio(row.PreFolio))
            .Where(value => !string.IsNullOrWhiteSpace(value))
            .Distinct(StringComparer.Ordinal)
            .ToHashSet(StringComparer.Ordinal);
        var activeProjectKeys = rows
            .Where(row => row.IsActive)
            .Select(row => NormalizeFolio(row.PreFolio))
            .Where(value => !string.IsNullOrWhiteSpace(value))
            .Distinct(StringComparer.Ordinal)
            .ToHashSet(StringComparer.Ordinal);

        return new PamConvocatoriaCoverageReport
        {
            ProyectosUniverso = projectKeys.Count,
            ProyectosConTrazabilidad = tracedProjectKeys.Count,
            ProyectosContinuan = activeProjectKeys.Count,
            ProyectosOtrosEstatus = tracedProjectKeys.Count - activeProjectKeys.Count,
            ProyectosSinDecision = projectKeys.Count - tracedProjectKeys.Count,
            ReferenciasSubestacion = candidates.Count(candidate =>
                string.Equals(candidate.TipoElemento, "subestacion", StringComparison.Ordinal)),
            SubestacionesCatalogadas = candidates.Count(candidate =>
                string.Equals(candidate.TipoElemento, "subestacion", StringComparison.Ordinal) &&
                string.Equals(candidate.Estado, "catalogada", StringComparison.Ordinal)),
            SubestacionesRevision = candidates.Count(candidate =>
                string.Equals(candidate.TipoElemento, "subestacion", StringComparison.Ordinal) &&
                string.Equals(candidate.Estado, "revision", StringComparison.Ordinal)),
            SubestacionesFaltantes = candidates.Count(candidate =>
                string.Equals(candidate.TipoElemento, "subestacion", StringComparison.Ordinal) &&
                string.Equals(candidate.Estado, "faltante", StringComparison.Ordinal)),
            ReferenciasLinea = candidates.Count(candidate =>
                string.Equals(candidate.TipoElemento, "linea_transmision", StringComparison.Ordinal)),
            LineasCatalogadas = candidates.Count(candidate =>
                string.Equals(candidate.TipoElemento, "linea_transmision", StringComparison.Ordinal) &&
                string.Equals(candidate.Estado, "catalogada", StringComparison.Ordinal)),
            LineasCorredorCatalogado = candidates.Count(candidate =>
                string.Equals(candidate.TipoElemento, "linea_transmision", StringComparison.Ordinal) &&
                string.Equals(candidate.Estado, "corredor_catalogado", StringComparison.Ordinal)),
            LineasRevision = candidates.Count(candidate =>
                string.Equals(candidate.TipoElemento, "linea_transmision", StringComparison.Ordinal) &&
                string.Equals(candidate.Estado, "revision", StringComparison.Ordinal)),
            LineasSinGeometria = candidates.Count(candidate =>
                string.Equals(candidate.TipoElemento, "linea_transmision", StringComparison.Ordinal) &&
                !candidate.Geometria.HasValue),
            LineasConectadasGrafo = candidates.Count(candidate =>
                string.Equals(candidate.TipoElemento, "linea_transmision", StringComparison.Ordinal) &&
                string.Equals(candidate.EstadoConexionGrafo, "conectada", StringComparison.Ordinal)),
            LineasParcialesGrafo = candidates.Count(candidate =>
                string.Equals(candidate.TipoElemento, "linea_transmision", StringComparison.Ordinal) &&
                string.Equals(candidate.EstadoConexionGrafo, "parcial", StringComparison.Ordinal)),
            LineasAmbiguasGrafo = candidates.Count(candidate =>
                string.Equals(candidate.TipoElemento, "linea_transmision", StringComparison.Ordinal) &&
                string.Equals(candidate.EstadoConexionGrafo, "ambigua", StringComparison.Ordinal)),
            LineasSinResolverGrafo = candidates.Count(candidate =>
                string.Equals(candidate.TipoElemento, "linea_transmision", StringComparison.Ordinal) &&
                string.Equals(candidate.EstadoConexionGrafo, "sin_resolver", StringComparison.Ordinal)),
            Fuente = _options.BaseUrl,
            Candidatos = candidates
                .OrderBy(candidate => candidate.TipoElemento, StringComparer.Ordinal)
                .ThenBy(candidate => candidate.Estado, StringComparer.Ordinal)
                .ThenBy(candidate => candidate.NombreDeclarado, StringComparer.OrdinalIgnoreCase)
                .ToList()
        };
    }

    private List<GcrArea> ParseGcrAreas(string json)
    {
        using var document = JsonDocument.Parse(json);
        return GetFeatures(document.RootElement)
            .Select(feature =>
            {
                var properties = GetProperty(feature, "properties");
                var geometry = GetProperty(feature, "geometry");
                var key = ResolveGcrKey(
                    GetString(properties, "Field1", "nombre", "name"));
                var polygons = ParsePolygonShapes(geometry);
                return string.IsNullOrWhiteSpace(key) || polygons.Count == 0
                    ? null
                    : new GcrArea(
                        key,
                        polygons,
                        polygons.Min(polygon => polygon.MinLongitude),
                        polygons.Min(polygon => polygon.MinLatitude),
                        polygons.Max(polygon => polygon.MaxLongitude),
                        polygons.Max(polygon => polygon.MaxLatitude));
            })
            .Where(area => area is not null)
            .Cast<GcrArea>()
            .ToList();
    }

    private List<NetworkElement> ParseSubstations(
        string json,
        IReadOnlyList<GcrArea> gcrAreas)
    {
        using var document = JsonDocument.Parse(json);
        return GetFeatures(document.RootElement)
            .Select(feature =>
            {
                var properties = GetProperty(feature, "properties");
                var geometry = GetProperty(feature, "geometry");
                if (!TryPoint(GetProperty(geometry, "coordinates"), out var point))
                {
                    return null;
                }

                var name = GetString(properties, "name", "nombre", "subestacion");
                var normalized = NormalizeSubstationName(name);
                if (string.IsNullOrWhiteSpace(normalized))
                {
                    return null;
                }

                var geometryClone = geometry.Clone();
                var voltage = GetDouble(
                    properties,
                    "voltaje_kv",
                    "voltaje_KV",
                    "tension_kv");
                return new NetworkElement
                {
                    Type = "subestacion",
                    Key = StableKey(
                        "SE",
                        normalized,
                        geometryClone.GetRawText()),
                    Name = name,
                    NormalizedName = normalized,
                    EquivalenceKey =
                        NormalizeSubstationEquivalenceKey(normalized),
                    VoltageKv = voltage,
                    GcrKeys = gcrAreas
                        .Where(area => AreaContains(area, point))
                        .Select(area => area.Key)
                        .ToHashSet(StringComparer.Ordinal),
                    Geometry = geometryClone,
                    Point = point
                };
            })
            .Where(element => element is not null)
            .Cast<NetworkElement>()
            .ToList();
    }

    private List<NetworkElement> ParseLines(
        string json,
        IReadOnlyList<GcrArea> gcrAreas)
    {
        using var document = JsonDocument.Parse(json);
        return GetFeatures(document.RootElement)
            .Select(feature =>
            {
                var properties = GetProperty(feature, "properties");
                var geometry = GetProperty(feature, "geometry");
                var name = GetString(properties, "nombre_lt", "name", "nombre");
                var normalized = NormalizeLineName(name);
                if (string.IsNullOrWhiteSpace(normalized) ||
                    geometry.ValueKind != JsonValueKind.Object)
                {
                    return null;
                }

                var geometryClone = geometry.Clone();
                var points = ExtractCoordinates(geometry).ToList();
                var (endpointA, endpointB) =
                    ParseLineEndpointNames(name);
                var characteristics = GetString(
                    properties,
                    "caracteris",
                    "caracteristicas");
                var representative = points.Count == 0
                    ? default(GeoPoint?)
                    : new GeoPoint(
                        points.Average(point => point.Latitude),
                        points.Average(point => point.Longitude));
                return new NetworkElement
                {
                    Type = "linea_transmision",
                    Key = StableKey(
                        "LT",
                        normalized,
                        geometryClone.GetRawText()),
                    Name = name,
                    NormalizedName = normalized,
                    VoltageKv = GetDouble(
                        properties,
                        "voltaje_KV",
                        "voltaje_kv",
                        "tension_kv"),
                    GcrKeys = gcrAreas
                        .Where(area => SamplePoints(points, 5).Any(point =>
                            AreaContains(area, point)))
                        .Select(area => area.Key)
                        .ToHashSet(StringComparer.Ordinal),
                    EndpointAName = endpointA,
                    EndpointBName = endpointB,
                    EndpointAPoint = points.Count > 0
                        ? points[0]
                        : null,
                    EndpointBPoint = points.Count > 0
                        ? points[^1]
                        : null,
                    Characteristics = characteristics,
                    CircuitCodes = ExtractLineCircuitCodes(
                        $"{name} {characteristics}"),
                    Geometry = geometryClone,
                    Point = representative
                };
            })
            .Where(element => element is not null)
            .Cast<NetworkElement>()
            .ToList();
    }

    private static IReadOnlyList<LineEndpointReference> BuildLineEndpoints(
        IReadOnlyList<NetworkElement> lines,
        IReadOnlyList<NetworkElement> substations,
        double catalogEndpointToleranceKm)
    {
        var output = new List<LineEndpointReference>(lines.Count * 2);
        foreach (var line in lines)
        {
            var (endpointAPoint, endpointBPoint) =
                OrientLineEndpoints(
                    line,
                    substations,
                    catalogEndpointToleranceKm);
            Add(line.EndpointAName, endpointAPoint);
            Add(line.EndpointBName, endpointBPoint);

            void Add(string name, GeoPoint? point)
            {
                if (string.IsNullOrWhiteSpace(name) || !point.HasValue)
                {
                    return;
                }
                output.Add(new LineEndpointReference(
                    name,
                    NormalizeSubstationAlias(name),
                    line.Key,
                    line.Name,
                    line.VoltageKv,
                    point.Value,
                    line.GcrKeys));
            }
        }
        return output;
    }

    private static IReadOnlyDictionary<string, LineTopologyEvidence>
        BuildLineTopologyCatalog(
            IReadOnlyList<NetworkElement> lines,
            IReadOnlyList<NetworkElement> substations,
            double endpointToleranceKm)
    {
        return lines
            .GroupBy(line => line.Key, StringComparer.Ordinal)
            .ToDictionary(
            group => group.Key,
            group =>
            {
                var line = group.First();
                var (endpointAPoint, endpointBPoint) =
                    OrientLineEndpoints(
                        line,
                        substations,
                        endpointToleranceKm);
                var endpointA = ResolveLineTopologyEndpoint(
                    "A",
                    line.EndpointAName,
                    endpointAPoint,
                    substations,
                    endpointToleranceKm);
                var endpointB = ResolveLineTopologyEndpoint(
                    "B",
                    line.EndpointBName,
                    endpointBPoint,
                    substations,
                    endpointToleranceKm);
                var resolved = (endpointA.IsResolved ? 1 : 0) +
                    (endpointB.IsResolved ? 1 : 0);
                var state = endpointA.IsAmbiguous || endpointB.IsAmbiguous
                    ? "ambigua"
                    : resolved switch
                    {
                        2 => "conectada",
                        1 => "parcial",
                        _ => "sin_resolver"
                    };
                var evidence = endpointA.Evidence
                    .Concat(endpointB.Evidence)
                    .Append(
                        $"GeoJSON de líneas: conectividad {state}; {resolved} de 2 extremos resueltos contra subestaciones catalogadas.")
                    .Distinct(StringComparer.Ordinal)
                    .ToList();
                return new LineTopologyEvidence(
                    state,
                    resolved == 2 &&
                    !endpointA.IsAmbiguous &&
                    !endpointB.IsAmbiguous,
                    resolved,
                    endpointA,
                    endpointB,
                    evidence);
            },
            StringComparer.Ordinal);
    }

    private static LineEndpointTopology ResolveLineTopologyEndpoint(
        string side,
        string declaredName,
        GeoPoint? endpointPoint,
        IReadOnlyList<NetworkElement> substations,
        double endpointToleranceKm)
    {
        if (string.IsNullOrWhiteSpace(declaredName) ||
            !endpointPoint.HasValue)
        {
            return LineEndpointTopology.Unresolved(
                declaredName,
                $"Extremo {side}: la línea GeoJSON no aporta nombre y coordenada suficientes.");
        }

        var candidates = substations
            .Where(substation => substation.Point.HasValue)
            .Select(substation => new
            {
                Element = substation,
                DistanceKm = HaversineKm(
                    endpointPoint.Value,
                    substation.Point!.Value)
            })
            .Where(candidate =>
                candidate.DistanceKm <= endpointToleranceKm)
            .Select(candidate => new
            {
                candidate.Element,
                candidate.DistanceKm,
                NameMatch = RedElectricaEndpointNameMatcher.Evaluate(
                    declaredName,
                    candidate.Element.NormalizedName,
                    candidate.DistanceKm)
            })
            .Where(candidate => candidate.NameMatch.IsMatch)
            .GroupBy(candidate =>
                $"{candidate.Element.EquivalenceKey}|" +
                $"{candidate.Element.Point!.Value.Latitude:0.0000}|" +
                $"{candidate.Element.Point!.Value.Longitude:0.0000}",
                StringComparer.Ordinal)
            .Select(group => group
                .OrderByDescending(candidate =>
                    candidate.NameMatch.Priority)
                .ThenBy(candidate => candidate.DistanceKm)
                .ThenBy(
                    candidate => candidate.Element.Name,
                    StringComparer.OrdinalIgnoreCase)
                .First())
            .OrderByDescending(candidate =>
                candidate.NameMatch.Priority)
            .ThenBy(candidate => candidate.DistanceKm)
            .ToList();

        if (candidates.Count == 0)
        {
            return LineEndpointTopology.Unresolved(
                declaredName,
                $"Extremo {side} «{declaredName}»: sin subestación homónima dentro de {endpointToleranceKm:0.##} km del extremo real del GeoJSON.");
        }

        var best = candidates[0];
        if (candidates.Count > 1 &&
            candidates[1].NameMatch.Priority ==
                best.NameMatch.Priority &&
            candidates[1].DistanceKm - best.DistanceKm < 0.25)
        {
            return new LineEndpointTopology(
                declaredName,
                string.Empty,
                null,
                false,
                true,
                new[]
                {
                    $"Extremo {side} «{declaredName}»: ambiguo entre {candidates.Count} subestaciones homónimas cercanas al extremo real."
                });
        }

        return new LineEndpointTopology(
            declaredName,
            best.Element.Name,
            best.DistanceKm,
            true,
            false,
            new[]
            {
                $"Extremo {side} «{declaredName}» → «{best.Element.Name}» a {best.DistanceKm:0.###} km del extremo real del GeoJSON; {best.NameMatch.Evidence}."
            });
    }

    private static (GeoPoint? EndpointA, GeoPoint? EndpointB)
        OrientLineEndpoints(
            NetworkElement line,
            IReadOnlyList<NetworkElement> substations,
            double catalogEndpointToleranceKm)
    {
        if (!line.EndpointAPoint.HasValue ||
            !line.EndpointBPoint.HasValue)
        {
            return (line.EndpointAPoint, line.EndpointBPoint);
        }

        var endpointACatalogPoints = FindEndpointCatalogPoints(
            line.EndpointAName,
            substations);
        var endpointBCatalogPoints = FindEndpointCatalogPoints(
            line.EndpointBName,
            substations);
        if (endpointACatalogPoints.Count == 0 &&
            endpointBCatalogPoints.Count == 0)
        {
            return (line.EndpointAPoint, line.EndpointBPoint);
        }

        var rawA = line.EndpointAPoint.Value;
        var rawB = line.EndpointBPoint.Value;
        var directDistances = new List<double>(2);
        var reverseDistances = new List<double>(2);
        if (endpointACatalogPoints.Count > 0)
        {
            directDistances.Add(endpointACatalogPoints.Min(point =>
                HaversineKm(point, rawA)));
            reverseDistances.Add(endpointACatalogPoints.Min(point =>
                HaversineKm(point, rawB)));
        }
        if (endpointBCatalogPoints.Count > 0)
        {
            directDistances.Add(endpointBCatalogPoints.Min(point =>
                HaversineKm(point, rawB)));
            reverseDistances.Add(endpointBCatalogPoints.Min(point =>
                HaversineKm(point, rawA)));
        }

        var directCost = directDistances.Sum();
        var reverseCost = reverseDistances.Sum();
        var nearestKnownEndpoint = directDistances
            .Concat(reverseDistances)
            .DefaultIfEmpty(double.MaxValue)
            .Min();
        if (nearestKnownEndpoint > catalogEndpointToleranceKm ||
            reverseCost + 0.1 >= directCost)
        {
            return (rawA, rawB);
        }

        return (rawB, rawA);
    }

    private static IReadOnlyList<GeoPoint> FindEndpointCatalogPoints(
        string endpointName,
        IReadOnlyList<NetworkElement> substations)
    {
        var alias = NormalizeSubstationAlias(endpointName);
        var equivalenceKey =
            NormalizeSubstationEquivalenceKey(alias);
        if (string.IsNullOrWhiteSpace(alias))
        {
            return Array.Empty<GeoPoint>();
        }

        return substations
            .Where(substation =>
                substation.Point.HasValue &&
                (string.Equals(
                     NormalizeSubstationAlias(
                         substation.NormalizedName),
                     alias,
                     StringComparison.Ordinal) ||
                 (!string.IsNullOrWhiteSpace(equivalenceKey) &&
                  string.Equals(
                      substation.EquivalenceKey,
                      equivalenceKey,
                      StringComparison.Ordinal))))
            .Select(substation => substation.Point!.Value)
            .Distinct()
            .ToList();
    }

    private static List<string[]> ParseCsv(string text)
    {
        var rows = new List<string[]>();
        using var reader = new StringReader(text);
        using var parser = new TextFieldParser(reader)
        {
            HasFieldsEnclosedInQuotes = true,
            TrimWhiteSpace = false
        };
        parser.SetDelimiters(",");
        while (!parser.EndOfData)
        {
            rows.Add(parser.ReadFields() ?? Array.Empty<string>());
        }
        return rows;
    }

    private static IReadOnlyList<GeoPoint> ParsePoints(
        string[] row,
        int start,
        int end)
    {
        if (start < 0 || end <= start)
        {
            return Array.Empty<GeoPoint>();
        }

        var output = new List<GeoPoint>();
        for (var index = start; index < Math.Min(end, row.Length); index++)
        {
            if (!TryParsePoint(row[index], out var point) ||
                output.Any(existing =>
                    Math.Abs(existing.Latitude - point.Latitude) < 0.0000001 &&
                    Math.Abs(existing.Longitude - point.Longitude) < 0.0000001))
            {
                continue;
            }
            output.Add(point);
        }
        return output;
    }

    private static bool TryParsePoint(string value, out GeoPoint point)
    {
        point = default;
        var numbers = Regex.Matches(
                value ?? string.Empty,
                @"-?\d{1,3}(?:\.\d+)?",
                RegexOptions.CultureInvariant)
            .Select(match => double.TryParse(
                match.Value,
                NumberStyles.Float,
                CultureInfo.InvariantCulture,
                out var parsed)
                    ? (double?)parsed
                    : null)
            .Where(parsed => parsed.HasValue)
            .Select(parsed => parsed!.Value)
            .ToList();
        if (numbers.Count < 2)
        {
            return false;
        }

        var first = numbers[0];
        var second = numbers[1];
        var latitude = first;
        var longitude = second;
        if (first is < -86 or > 33.5 &&
            second is >= 14 and <= 33.5)
        {
            latitude = second;
            longitude = first;
        }
        if (latitude is < 14 or > 33.5 || longitude is < -118 or > -86)
        {
            return false;
        }

        point = new GeoPoint(latitude, longitude);
        return true;
    }

    private static JsonElement? CreateGeometry(IReadOnlyList<GeoPoint> points)
    {
        if (points.Count == 0)
        {
            return null;
        }
        if (points.Count == 1)
        {
            return JsonSerializer.SerializeToElement(new
            {
                type = "Point",
                coordinates = new[]
                {
                    points[0].Longitude,
                    points[0].Latitude
                }
            });
        }
        if (points.Count == 2)
        {
            return JsonSerializer.SerializeToElement(new
            {
                type = "MultiPoint",
                coordinates = points
                    .Select(point => new[]
                    {
                        point.Longitude,
                        point.Latitude
                    })
                    .ToArray()
            });
        }

        var ring = points
            .Select(point => new[]
            {
                point.Longitude,
                point.Latitude
            })
            .ToList();
        if (!ring[0].SequenceEqual(ring[^1]))
        {
            ring.Add(ring[0]);
        }
        return JsonSerializer.SerializeToElement(new
        {
            type = "Polygon",
            coordinates = new[] { ring.ToArray() }
        });
    }

    private static JsonElement? CreatePointCollectionGeometry(
        IReadOnlyList<GeoPoint> points)
    {
        if (points.Count == 0)
        {
            return null;
        }
        if (points.Count == 1)
        {
            return JsonSerializer.SerializeToElement(new
            {
                type = "Point",
                coordinates = new[]
                {
                    points[0].Longitude,
                    points[0].Latitude
                }
            });
        }

        return JsonSerializer.SerializeToElement(new
        {
            type = "MultiPoint",
            coordinates = points
                .Select(point => new[]
                {
                    point.Longitude,
                    point.Latitude
                })
                .ToArray()
        });
    }

    private static GeoPoint? RepresentativePoint(IReadOnlyList<GeoPoint> points) =>
        points.Count == 0
            ? null
            : new GeoPoint(
                points.Average(point => point.Latitude),
                points.Average(point => point.Longitude));

    private static IEnumerable<string> ExtractLineReferences(string text)
    {
        var results = new HashSet<string>(StringComparer.Ordinal);
        foreach (Match match in Regex.Matches(
                     text ?? string.Empty,
                     @"(?:\bL\.?\s*T\.?\b|\bL[IÍ]NEA(?:\s+DE\s+TRANSMISI[ÓO]N)?\b)\s*(?:DE\s+)?(?<name>[^\r\n.;]{3,120})",
                     RegexOptions.IgnoreCase | RegexOptions.CultureInvariant))
        {
            var value = match.Groups["name"].Value
                .Trim(' ', ':', '-', ',', '.');
            var references = Regex.Split(
                    value,
                    @"\s+Y\s+(?=(?:L\.?\s*T\.?|LINEA(?:\s+DE\s+TRANSMISION)?)\b)",
                    RegexOptions.IgnoreCase |
                    RegexOptions.CultureInvariant)
                .Select(part => Regex.Replace(
                    part,
                    @"^\s*(?:L\.?\s*T\.?|LINEA(?:\s+DE\s+TRANSMISION)?)\s*",
                    string.Empty,
                    RegexOptions.IgnoreCase |
                    RegexOptions.CultureInvariant))
                .Select(part =>
                    part.Trim(' ', ':', '-', ',', '.'));
            foreach (var reference in references)
            {
                if (reference.Length is >= 3 and <= 120 &&
                    IsPlausibleLineReference(reference))
                {
                    results.Add(reference);
                }
            }
        }
        return results;
    }

    private static bool IsPlausibleLineReference(string value)
    {
        var normalized = NormalizeText(value);
        if (normalized.Length < 5 ||
            normalized.Contains("APROXIMAD", StringComparison.Ordinal) ||
            normalized.Contains("APORXIAMD", StringComparison.Ordinal) ||
            normalized.Contains("QUE CONECTA", StringComparison.Ordinal) ||
            normalized.Contains("A LA SUBESTACION", StringComparison.Ordinal) ||
            Regex.IsMatch(
                normalized,
                @"\b(?:KCMIL|ACSR|CABLE|CONDUCTOR|TERNA|MVA|MW)\b",
                RegexOptions.CultureInvariant) ||
            Regex.IsMatch(
                normalized,
                @"^\d+(?:\.\d+)?\s*KM$",
                RegexOptions.CultureInvariant))
        {
            return false;
        }

        return value.Contains('-') ||
            value.Contains('–') ||
            Regex.IsMatch(
                normalized,
                @"\b[A-Z]{1,5}\d{2,}[A-Z0-9]*\b",
                RegexOptions.CultureInvariant);
    }

    private static string ExtractSubstationFromPoint(string value)
    {
        var match = Regex.Match(
            value ?? string.Empty,
            @"(?:\bS\.?\s*E\.?\b|\bSUBESTACI[ÓO]N\b)\s*(?<name>[^,;]{2,100})",
            RegexOptions.IgnoreCase | RegexOptions.CultureInvariant);
        return match.Success
            ? match.Groups["name"].Value.Trim()
            : string.Empty;
    }

    private static bool HasDistinctPrivateAndPublicSubstation(
        SecondCallRow row)
    {
        if (!HasPrivateSubstationQualifier(row.DeclaredSubstation) ||
            string.IsNullOrWhiteSpace(
                row.NormalizedInterconnectionSubstation))
        {
            return false;
        }

        var privateKey = NormalizeSubstationEquivalenceKey(
            NormalizeSubstationAlias(row.NormalizedSubstation));
        var publicKey = NormalizeSubstationEquivalenceKey(
            NormalizeSubstationAlias(
                row.NormalizedInterconnectionSubstation));
        return !string.IsNullOrWhiteSpace(privateKey) &&
            !string.IsNullOrWhiteSpace(publicKey) &&
            !string.Equals(privateKey, publicKey, StringComparison.Ordinal);
    }

    private static bool HasPrivateSubstationQualifier(string value)
    {
        return TokenSet(NormalizeSubstationName(value))
            .Any(PrivateSubstationQualifiers.Contains);
    }

    private bool IsFirmPublicInterconnection(
        NetworkResolution resolution)
    {
        return resolution.Element is not null &&
            string.Equals(
                resolution.State,
                "catalogada",
                StringComparison.Ordinal) &&
            (resolution.NameExact || resolution.NameEquivalent) &&
            (resolution.NameUnique || resolution.NameScopeUnique) &&
            resolution.GcrCompatible == true &&
            resolution.VoltageCompatible == true &&
            resolution.DistanceCompatible != false &&
            resolution.ScoreMargin >= _options.StrongNameMargin;
    }

    private static string SelectDeclaredSubstation(params string[] values)
    {
        return values
            .Select(Clean)
            .FirstOrDefault(IsPlausibleSubstationReference) ??
            string.Empty;
    }

    private static bool IsPlausibleSubstationReference(string value)
    {
        var normalized = NormalizeText(value);
        if (string.IsNullOrWhiteSpace(normalized) ||
            normalized.Length < 3 ||
            Regex.IsMatch(
                normalized,
                @"^(?:\d+(?:\s+\d+)?\s*)?(?:MVA|MW|KV)$",
                RegexOptions.CultureInvariant) ||
            (normalized.Contains("APERTURA", StringComparison.Ordinal) &&
             Regex.IsMatch(
                 normalized,
                 @"\bLT\b",
                 RegexOptions.CultureInvariant)) ||
            normalized.Contains("LINEA DE TRANSMISION", StringComparison.Ordinal) ||
            Regex.IsMatch(
                normalized,
                @"^LT\s+",
                RegexOptions.CultureInvariant))
        {
            return false;
        }

        return normalized.Any(char.IsLetter);
    }

    private static double TokenSimilarity(string left, string right)
    {
        var leftTokens = TokenSet(left);
        var rightTokens = TokenSet(right);
        if (leftTokens.Count == 0 || rightTokens.Count == 0)
        {
            return 0;
        }

        var intersection = leftTokens.Intersect(rightTokens, StringComparer.Ordinal).Count();
        var union = leftTokens.Union(rightTokens, StringComparer.Ordinal).Count();
        return union == 0 ? 0 : (double)intersection / union;
    }

    private static HashSet<string> TokenSet(string value)
    {
        var stop = new HashSet<string>(
            new[]
            {
                "DE", "DEL", "LA", "EL", "LOS", "LAS", "Y", "EN",
                "PROYECTO", "AMPLIACION", "MODERNIZACION"
            },
            StringComparer.Ordinal);
        return NormalizeText(value)
            .Split(' ', StringSplitOptions.RemoveEmptyEntries)
            .Where(token => token.Length > 2 && !stop.Contains(token))
            .ToHashSet(StringComparer.Ordinal);
    }

    private static string NormalizeSubstationName(string value)
    {
        var normalized = NormalizeText(value);
        normalized = Regex.Replace(
            normalized,
            @"^(?:CFE\s+)?(?:S\s*E|SUBESTACION(?:\s+ELECTRICA)?)\s+",
            string.Empty,
            RegexOptions.CultureInvariant);
        normalized = Regex.Replace(
            normalized,
            @"(?:\s+AT)?\s+\d+(?:\.\d+)?\s*KV\b",
            string.Empty,
            RegexOptions.CultureInvariant);
        return normalized.Trim();
    }

    private static string NormalizeSubstationAlias(string value)
    {
        var normalized = NormalizeText(value);
        normalized = Regex.Replace(
            normalized,
            @"^(?:SET|CFE)\s+",
            string.Empty,
            RegexOptions.CultureInvariant);
        normalized = Regex.Replace(
            normalized,
            @"\s+DE\s+LA\s+CFE$",
            string.Empty,
            RegexOptions.CultureInvariant);
        normalized = Regex.Replace(
            normalized,
            @"\s+\d{2}[A-Z]{3}\s+\d{2,3}$",
            string.Empty,
            RegexOptions.CultureInvariant);
        return normalized.Trim();
    }

    private static string NormalizeTopologyEndpointAlias(string value)
    {
        var normalized = NormalizeSubstationAlias(value);
        normalized = Regex.Replace(
            normalized,
            @"\bPOT\b",
            "POTENCIA",
            RegexOptions.CultureInvariant);
        return normalized.Trim();
    }

    private static string NormalizeSubstationEquivalenceKey(string value)
    {
        var tokens = NormalizeText(value)
            .Split(' ', StringSplitOptions.RemoveEmptyEntries)
            .Where(token =>
                !SubstationEquivalenceStopWords.Contains(token))
            .ToList();
        var hasBankQualifier = tokens.Any(
            SubstationBankQualifiers.Contains);
        tokens = tokens
            .Where(token =>
                !SubstationBankQualifiers.Contains(token) &&
                !string.Equals(token, "PRESA", StringComparison.Ordinal) &&
                (!hasBankQualifier ||
                 !Regex.IsMatch(
                     token,
                     @"^(?:\d+|[IVXLCDM]+)$",
                     RegexOptions.CultureInvariant)))
            .OrderBy(token => token, StringComparer.Ordinal)
            .ToList();
        return string.Join("|", tokens);
    }

    private static double CalculateNameSimilarity(
        string left,
        string right)
    {
        if (string.Equals(left, right, StringComparison.Ordinal))
        {
            return 1;
        }

        var tokenSimilarity = TokenSimilarity(left, right);
        var leftTokens = TokenSet(left);
        var rightTokens = TokenSet(right);
        var shouldCompareCharacters =
            tokenSimilarity >= 0.75 ||
            (leftTokens.Count == 1 &&
             rightTokens.Count == 1 &&
             left.Length >= 7 &&
             right.Length >= 7 &&
             left[0] == right[0] &&
             Math.Abs(left.Length - right.Length) <= 2);
        if (!shouldCompareCharacters)
        {
            return tokenSimilarity;
        }

        var maximumLength = Math.Max(left.Length, right.Length);
        return maximumLength == 0
            ? 1
            : Math.Max(
                tokenSimilarity,
                1d - (double)LevenshteinDistance(left, right) /
                    maximumLength);
    }

    private static int LevenshteinDistance(string left, string right)
    {
        var previous = new int[right.Length + 1];
        var current = new int[right.Length + 1];
        for (var column = 0; column <= right.Length; column++)
        {
            previous[column] = column;
        }

        for (var row = 1; row <= left.Length; row++)
        {
            current[0] = row;
            for (var column = 1; column <= right.Length; column++)
            {
                var substitutionCost =
                    left[row - 1] == right[column - 1] ? 0 : 1;
                current[column] = Math.Min(
                    Math.Min(
                        current[column - 1] + 1,
                        previous[column] + 1),
                    previous[column - 1] + substitutionCost);
            }
            (previous, current) = (current, previous);
        }
        return previous[right.Length];
    }

    private static (string EndpointA, string EndpointB)
        ParseLineEndpointNames(string name)
    {
        var withoutPrefix = Regex.Replace(
            name ?? string.Empty,
            @"^\s*(?:L\s*\.\s*T\s*\.?|LT|LINEA(?:\s+DE\s+TRANSMISION)?)\s*",
            string.Empty,
            RegexOptions.IgnoreCase | RegexOptions.CultureInvariant);
        var parts = Regex.Split(
                withoutPrefix,
                @"\s*[-–—]\s*",
                RegexOptions.CultureInvariant)
            .Select(NormalizeSubstationName)
            .Select(NormalizeSubstationAlias)
            .Where(value => !string.IsNullOrWhiteSpace(value))
            .ToArray();
        return parts.Length >= 2
            ? (parts[0], parts[^1])
            : (string.Empty, string.Empty);
    }

    private static string NormalizeLineName(string value)
    {
        var normalized = NormalizeText(value);
        normalized = Regex.Replace(
            normalized,
            @"^(?:L\s*T|LINEA(?:\s+DE\s+TRANSMISION)?)\s+",
            string.Empty,
            RegexOptions.CultureInvariant);
        return normalized.Trim();
    }

    private static string NormalizeLineMatchSignature(
        string value)
    {
        var withoutParenthetical = Regex.Replace(
            value ?? string.Empty,
            @"\([^)]*\)",
            " ",
            RegexOptions.CultureInvariant);
        var normalized = NormalizeText(
            withoutParenthetical);
        normalized = Regex.Replace(
            normalized,
            @"\b\d{2,3}(?:\.\d+)?\s*K\s*V\b",
            " ",
            RegexOptions.CultureInvariant);
        normalized = Regex.Replace(
            normalized,
            @"\bPB\s+0\b",
            "PB0",
            RegexOptions.CultureInvariant);

        var narrativeMarkers = new[]
        {
            " EQUIPADO ",
            " A UN NIVEL ",
            " QUE CORRE ",
            " A LO LARGO ",
            " MEDIANTE UN PUNTO ",
            " UBICADO A UNA DISTANCIA "
        };
        foreach (var marker in narrativeMarkers)
        {
            var index = normalized.IndexOf(
                marker,
                StringComparison.Ordinal);
            if (index >= 0)
            {
                normalized = normalized[..index];
            }
        }

        var stopWords = new HashSet<string>(
            new[]
            {
                "L", "T", "LT", "LINEA", "TRANSMISION",
                "DE", "DEL", "LA", "EL", "LOS", "LAS",
                "Y", "EN", "ENTRE", "SE", "SUBESTACION",
                "CFE", "CIRCUITO", "CIRCUITOS",
                "POT", "POTENCIA", "MANIOBRAS"
            },
            StringComparer.Ordinal);
        var tokens = normalized
            .Split(
                ' ',
                StringSplitOptions.RemoveEmptyEntries)
            .Where(token =>
                !stopWords.Contains(token) &&
                !Regex.IsMatch(
                    token,
                    @"^\d+$",
                    RegexOptions.CultureInvariant) &&
                !IsLineCircuitCode(token))
            .Distinct(StringComparer.Ordinal)
            .OrderBy(token => token, StringComparer.Ordinal)
            .ToList();
        return tokens.Count < 2
            ? string.Empty
            : string.Join("|", tokens);
    }

    private static bool AreLineSignaturesEquivalent(
        string leftSignature,
        string rightSignature)
    {
        if (string.Equals(
                leftSignature,
                rightSignature,
                StringComparison.Ordinal))
        {
            return true;
        }

        var left = leftSignature
            .Split(
                '|',
                StringSplitOptions.RemoveEmptyEntries)
            .ToHashSet(StringComparer.Ordinal);
        var right = rightSignature
            .Split(
                '|',
                StringSplitOptions.RemoveEmptyEntries)
            .ToHashSet(StringComparer.Ordinal);
        if (left.Count < 3 || right.Count < 3)
        {
            return false;
        }

        var intersection = left
            .Intersect(right, StringComparer.Ordinal)
            .Count();
        return intersection == Math.Min(left.Count, right.Count) &&
            Math.Abs(left.Count - right.Count) <= 1;
    }

    private static int CountLineSignatureOverlap(
        string left,
        string right)
    {
        var leftSignature =
            NormalizeLineMatchSignature(left);
        var rightSignature =
            NormalizeLineMatchSignature(right);
        if (string.IsNullOrWhiteSpace(leftSignature) ||
            string.IsNullOrWhiteSpace(rightSignature))
        {
            return 0;
        }

        var rightTokens = rightSignature
            .Split(
                '|',
                StringSplitOptions.RemoveEmptyEntries)
            .ToHashSet(StringComparer.Ordinal);
        return leftSignature
            .Split(
                '|',
                StringSplitOptions.RemoveEmptyEntries)
            .Count(rightTokens.Contains);
    }

    private static IReadOnlySet<string> ExtractLineCircuitCodes(
        string value)
    {
        return Regex.Matches(
                NormalizeText(value),
                @"\b(?:A\d[0-9A-Z]{2,4}|[79][0-9A-Z]{4})\b",
                RegexOptions.CultureInvariant)
            .Select(match => match.Value)
            .Where(IsLineCircuitCode)
            .ToHashSet(StringComparer.Ordinal);
    }

    private static bool IsLineCircuitCode(string value)
    {
        return Regex.IsMatch(
            value ?? string.Empty,
            @"^(?:A\d[0-9A-Z]{2,4}|[79][0-9A-Z]{4})$",
            RegexOptions.CultureInvariant);
    }

    private static string NormalizeText(string? value)
    {
        if (string.IsNullOrWhiteSpace(value))
        {
            return string.Empty;
        }

        var builder = new StringBuilder(value.Length);
        foreach (var character in value.Normalize(NormalizationForm.FormD))
        {
            if (CharUnicodeInfo.GetUnicodeCategory(character) ==
                UnicodeCategory.NonSpacingMark)
            {
                continue;
            }
            builder.Append(char.IsLetterOrDigit(character)
                ? char.ToUpperInvariant(character)
                : ' ');
        }
        return Regex.Replace(
                builder.ToString(),
                @"\s+",
                " ",
                RegexOptions.CultureInvariant)
            .Trim();
    }

    private static string NormalizeHeader(string value) => NormalizeText(value);

    private static string NormalizeKey(string? value) =>
        Regex.Replace(
            NormalizeText(value),
            @"[^A-Z0-9]",
            string.Empty,
            RegexOptions.CultureInvariant);

    private static string NormalizeFolio(string? value) =>
        Regex.Replace(
            NormalizeKey(value),
            @"^PRE",
            string.Empty,
            RegexOptions.CultureInvariant);

    private static string Clean(string? value)
    {
        var trimmed = (value ?? string.Empty).Trim();
        return EmptyValues.Contains(NormalizeText(trimmed))
            ? string.Empty
            : trimmed;
    }

    private static string FirstNonEmpty(params string[] values) =>
        values.FirstOrDefault(value => !string.IsNullOrWhiteSpace(value)) ??
        string.Empty;

    private static bool ContainsWhole(string paddedText, string normalizedName) =>
        !string.IsNullOrWhiteSpace(normalizedName) &&
        paddedText.Contains(
            $" {normalizedName} ",
            StringComparison.Ordinal);

    private static double? ParseVoltage(string value) =>
        ParseNumber(
            Regex.Match(
                    value ?? string.Empty,
                    @"(\d+(?:[.,]\d+)?)\s*(?:KV|KILOVOLT)",
                    RegexOptions.IgnoreCase | RegexOptions.CultureInvariant)
                .Groups[1]
                .Value,
            1,
            500);

    private static double? ParseNumber(
        string? value,
        double minimum,
        double maximum)
    {
        var match = Regex.Match(
            value ?? string.Empty,
            @"-?\d+(?:[.,]\d+)?",
            RegexOptions.CultureInvariant);
        if (!match.Success ||
            !double.TryParse(
                match.Value.Replace(',', '.'),
                NumberStyles.Float,
                CultureInfo.InvariantCulture,
                out var parsed) ||
            parsed < minimum ||
            parsed > maximum)
        {
            return null;
        }
        return parsed;
    }

    private static string ResolveGcrKey(string value)
    {
        var normalized = NormalizeText(value).Replace(" ", string.Empty);
        if (string.IsNullOrWhiteSpace(normalized) ||
            normalized.Contains("VARIAS", StringComparison.Ordinal))
        {
            return string.Empty;
        }

        var aliases = new Dictionary<string, string>(StringComparer.Ordinal)
        {
            ["BC"] = "bcalifornia",
            ["BAJACALIFORNIA"] = "bcalifornia",
            ["BS"] = "bcsur",
            ["BAJACALIFORNIASUR"] = "bcsur",
            ["NO"] = "noroeste",
            ["NOROESTE"] = "noroeste",
            ["NE"] = "noreste",
            ["NORESTE"] = "noreste",
            ["NT"] = "norte",
            ["NORTE"] = "norte",
            ["PE"] = "peninsular",
            ["PENINSULAR"] = "peninsular",
            ["OR"] = "oriental",
            ["SE"] = "oriental",
            ["ORIENTAL"] = "oriental",
            ["OC"] = "occidental",
            ["OCCIDENTAL"] = "occidental",
            ["OCCIDENTE"] = "occidental",
            ["CE"] = "central",
            ["VM"] = "central",
            ["CENTRAL"] = "central"
        };
        if (aliases.TryGetValue(normalized, out var alias))
        {
            return alias;
        }
        if (normalized.Contains(
                "BAJACALIFORNIASUR",
                StringComparison.Ordinal))
        {
            return "bcsur";
        }
        if (normalized.Contains(
                "BAJACALIFORNIA",
                StringComparison.Ordinal))
        {
            return "bcalifornia";
        }
        var names = new (string Match, string Key)[]
        {
            ("NOROESTE", "noroeste"),
            ("NORESTE", "noreste"),
            ("PENINSULAR", "peninsular"),
            ("ORIENTAL", "oriental"),
            ("OCCIDENTAL", "occidental"),
            ("OCCIDENTE", "occidental"),
            ("CENTRAL", "central"),
            ("NORTE", "norte")
        };
        return names
            .FirstOrDefault(item =>
                normalized.Contains(
                    item.Match,
                    StringComparison.Ordinal))
            .Key ?? string.Empty;
    }

    private static double HaversineKm(GeoPoint left, GeoPoint right)
    {
        const double earthRadiusKm = 6371.0088;
        var lat1 = DegreesToRadians(left.Latitude);
        var lat2 = DegreesToRadians(right.Latitude);
        var deltaLat = lat2 - lat1;
        var deltaLon = DegreesToRadians(right.Longitude - left.Longitude);
        var a = Math.Pow(Math.Sin(deltaLat / 2), 2) +
            Math.Cos(lat1) *
            Math.Cos(lat2) *
            Math.Pow(Math.Sin(deltaLon / 2), 2);
        return earthRadiusKm * 2 * Math.Atan2(Math.Sqrt(a), Math.Sqrt(1 - a));
    }

    private static double MaximumSeparationKm(
        IReadOnlyList<GeoPoint> points)
    {
        var maximum = 0d;
        for (var first = 0; first < points.Count; first++)
        {
            for (var second = first + 1; second < points.Count; second++)
            {
                maximum = Math.Max(
                    maximum,
                    HaversineKm(points[first], points[second]));
            }
        }
        return maximum;
    }

    private static bool SamePointSet(
        IReadOnlyList<GeoPoint> first,
        IReadOnlyList<GeoPoint> second)
    {
        if (first.Count == 0 || first.Count != second.Count)
        {
            return false;
        }

        return first.All(point =>
            second.Any(candidate =>
                Math.Abs(point.Latitude - candidate.Latitude) <
                    0.0000001 &&
                Math.Abs(point.Longitude - candidate.Longitude) <
                    0.0000001));
    }

    private static double DegreesToRadians(double value) =>
        value * Math.PI / 180;

    private static string StableKey(string prefix, params string[] parts)
    {
        var bytes = SHA256.HashData(
            Encoding.UTF8.GetBytes(
                string.Join("|", parts.Prepend(prefix))));
        return $"{prefix}-{Convert.ToHexString(bytes)[..16]}";
    }

    private static string GetGeometryType(JsonElement geometry) =>
        GetString(geometry, "type");

    private static double DefaultRadius(string geometryType) =>
        geometryType switch
        {
            "Polygon" or "MultiPolygon" => 0,
            "LineString" or "MultiLineString" => 10,
            "MultiPoint" => 10,
            _ => 20
        };

    private static string Read(string[] row, int index) =>
        index >= 0 && index < row.Length
            ? row[index]
            : string.Empty;

    private static int FindIndex(
        IReadOnlyList<string> header,
        string name,
        int start = 0)
    {
        var normalized = NormalizeHeader(name);
        for (var index = Math.Max(0, start); index < header.Count; index++)
        {
            if (string.Equals(
                    NormalizeHeader(header[index]),
                    normalized,
                    StringComparison.Ordinal))
            {
                return index;
            }
        }
        return -1;
    }

    private static IReadOnlyList<JsonElement> GetFeatures(JsonElement root)
    {
        var features = GetProperty(root, "features");
        return features.ValueKind == JsonValueKind.Array
            ? features.EnumerateArray().Select(value => value.Clone()).ToList()
            : Array.Empty<JsonElement>();
    }

    private static JsonElement GetProperty(JsonElement element, string name)
    {
        if (element.ValueKind != JsonValueKind.Object)
        {
            return default;
        }
        if (element.TryGetProperty(name, out var value))
        {
            return value;
        }
        foreach (var property in element.EnumerateObject())
        {
            if (string.Equals(
                    property.Name,
                    name,
                    StringComparison.OrdinalIgnoreCase))
            {
                return property.Value;
            }
        }
        return default;
    }

    private static string GetString(JsonElement element, params string[] names)
    {
        foreach (var name in names)
        {
            var value = GetProperty(element, name);
            if (value.ValueKind == JsonValueKind.String)
            {
                return value.GetString() ?? string.Empty;
            }
            if (value.ValueKind is JsonValueKind.Number or JsonValueKind.True or JsonValueKind.False)
            {
                return value.ToString();
            }
        }
        return string.Empty;
    }

    private static double? GetDouble(JsonElement element, params string[] names)
    {
        foreach (var name in names)
        {
            var value = GetProperty(element, name);
            if (value.ValueKind == JsonValueKind.Number &&
                value.TryGetDouble(out var number))
            {
                return number;
            }
            if (value.ValueKind == JsonValueKind.String &&
                double.TryParse(
                    value.GetString()?.Replace(',', '.'),
                    NumberStyles.Float,
                    CultureInfo.InvariantCulture,
                    out number))
            {
                return number;
            }
        }
        return null;
    }

    private static bool TryPoint(JsonElement coordinates, out GeoPoint point)
    {
        point = default;
        if (coordinates.ValueKind != JsonValueKind.Array ||
            coordinates.GetArrayLength() < 2 ||
            !coordinates[0].TryGetDouble(out var longitude) ||
            !coordinates[1].TryGetDouble(out var latitude))
        {
            return false;
        }
        point = new GeoPoint(latitude, longitude);
        return true;
    }

    private static IEnumerable<GeoPoint> ExtractCoordinates(JsonElement geometry)
    {
        var coordinates = GetProperty(geometry, "coordinates");
        return ExtractCoordinateArray(coordinates);
    }

    private static IEnumerable<GeoPoint> ExtractCoordinateArray(JsonElement value)
    {
        if (value.ValueKind != JsonValueKind.Array)
        {
            yield break;
        }
        if (value.GetArrayLength() >= 2 &&
            value[0].ValueKind == JsonValueKind.Number &&
            value[1].ValueKind == JsonValueKind.Number &&
            value[0].TryGetDouble(out var longitude) &&
            value[1].TryGetDouble(out var latitude))
        {
            yield return new GeoPoint(latitude, longitude);
            yield break;
        }
        foreach (var child in value.EnumerateArray())
        {
            foreach (var point in ExtractCoordinateArray(child))
            {
                yield return point;
            }
        }
    }

    private static IReadOnlyList<GeoPoint> SamplePoints(
        IReadOnlyList<GeoPoint> points,
        int maximum)
    {
        if (points.Count <= maximum)
        {
            return points;
        }
        var output = new List<GeoPoint>(maximum)
        {
            points[0]
        };
        var step = (double)(points.Count - 1) / (maximum - 1);
        for (var index = 1; index < maximum - 1; index++)
        {
            output.Add(points[(int)Math.Round(index * step)]);
        }
        output.Add(points[^1]);
        return output;
    }

    private static IReadOnlyList<PolygonShape> ParsePolygonShapes(
        JsonElement geometry)
    {
        var type = GetString(geometry, "type");
        var coordinates = GetProperty(geometry, "coordinates");
        var output = new List<PolygonShape>();
        if (string.Equals(type, "Polygon", StringComparison.OrdinalIgnoreCase))
        {
            var polygon = ParsePolygonShape(coordinates);
            if (polygon is not null)
            {
                output.Add(polygon);
            }
        }
        else if (string.Equals(
                     type,
                     "MultiPolygon",
                     StringComparison.OrdinalIgnoreCase) &&
                 coordinates.ValueKind == JsonValueKind.Array)
        {
            foreach (var polygonCoordinates in coordinates.EnumerateArray())
            {
                var polygon = ParsePolygonShape(polygonCoordinates);
                if (polygon is not null)
                {
                    output.Add(polygon);
                }
            }
        }
        return output;
    }

    private static PolygonShape? ParsePolygonShape(JsonElement rings)
    {
        if (rings.ValueKind != JsonValueKind.Array ||
            rings.GetArrayLength() == 0)
        {
            return null;
        }
        var parsedRings = rings
            .EnumerateArray()
            .Select(ParseRing)
            .Where(ring => ring.Count >= 3)
            .ToArray();
        if (parsedRings.Length == 0)
        {
            return null;
        }
        var exterior = parsedRings[0];
        return new PolygonShape(
            exterior,
            parsedRings.Skip(1).Cast<IReadOnlyList<GeoPoint>>().ToArray(),
            exterior.Min(point => point.Longitude),
            exterior.Min(point => point.Latitude),
            exterior.Max(point => point.Longitude),
            exterior.Max(point => point.Latitude));
    }

    private static IReadOnlyList<GeoPoint> ParseRing(JsonElement ring)
    {
        if (ring.ValueKind != JsonValueKind.Array)
        {
            return Array.Empty<GeoPoint>();
        }
        var output = new List<GeoPoint>(ring.GetArrayLength());
        foreach (var coordinate in ring.EnumerateArray())
        {
            if (TryPoint(coordinate, out var point))
            {
                output.Add(point);
            }
        }
        return output;
    }

    private static bool AreaContains(GcrArea area, GeoPoint point)
    {
        if (point.Longitude < area.MinLongitude ||
            point.Longitude > area.MaxLongitude ||
            point.Latitude < area.MinLatitude ||
            point.Latitude > area.MaxLatitude)
        {
            return false;
        }
        foreach (var polygon in area.Polygons)
        {
            if (point.Longitude < polygon.MinLongitude ||
                point.Longitude > polygon.MaxLongitude ||
                point.Latitude < polygon.MinLatitude ||
                point.Latitude > polygon.MaxLatitude ||
                !RingContains(polygon.Exterior, point))
            {
                continue;
            }
            if (!polygon.Holes.Any(hole => RingContains(hole, point)))
            {
                return true;
            }
        }
        return false;
    }

    private static bool RingContains(
        IReadOnlyList<GeoPoint> ring,
        GeoPoint point)
    {
        if (ring.Count < 3)
        {
            return false;
        }
        var inside = false;
        var previous = ring[^1];
        foreach (var current in ring)
        {
            var intersects =
                (current.Latitude > point.Latitude) !=
                    (previous.Latitude > point.Latitude) &&
                point.Longitude <
                    (previous.Longitude - current.Longitude) *
                    (point.Latitude - current.Latitude) /
                    ((previous.Latitude - current.Latitude) +
                     double.Epsilon) +
                    current.Longitude;
            if (intersects)
            {
                inside = !inside;
            }
            previous = current;
        }
        return inside;
    }

    private sealed record EvidenceCatalog(
        IReadOnlyList<SecondCallRow> Rows,
        IReadOnlyList<SecondCallRow> AllRows,
        IReadOnlyList<NetworkElement> Substations,
        IReadOnlyList<NetworkElement> Lines,
        PamConvocatoriaCoverageReport Coverage);

    private sealed record SubstationCandidateGroup(
        IReadOnlyList<SecondCallRow> Rows,
        string CandidateIdDiscriminator,
        bool IsTerritoriallySplit,
        string TerritoryKey);

    private sealed class SecondCallRow
    {
        public string PreFolio { get; init; } = string.Empty;
        public string ProjectFolio { get; init; } = string.Empty;
        public string ProjectName { get; init; } = string.Empty;
        public string Description { get; init; } = string.Empty;
        public string Gcr { get; init; } = string.Empty;
        public string State { get; init; } = string.Empty;
        public string Municipality { get; init; } = string.Empty;
        public string DeclaredSubstation { get; init; } = string.Empty;
        public string NormalizedSubstation { get; init; } = string.Empty;
        public string InterconnectionPoint { get; init; } = string.Empty;
        public string InterconnectionSubstation { get; init; } = string.Empty;
        public string NormalizedInterconnectionSubstation { get; init; } =
            string.Empty;
        public double? VoltageKv { get; init; }
        public double? DeclaredDistanceKm { get; init; }
        public JsonElement? ProjectGeometry { get; init; }
        public GeoPoint? ProjectPoint { get; init; }
        public JsonElement? SubstationGeometry { get; init; }
        public GeoPoint? SubstationPoint { get; init; }
        public bool SubstationGeometryMatchesProject { get; init; }
        public string ProjectKml { get; init; } = string.Empty;
        public string SubstationKml { get; init; } = string.Empty;
        public string Decision { get; init; } = string.Empty;
        public string DecisionDate { get; init; } = string.Empty;
        public bool IsActive { get; init; }
        public NetworkResolution SubstationResolution { get; set; } =
            NetworkResolution.Empty("sin_referencia");
        public NetworkResolution InterconnectionSubstationResolution
        {
            get;
            set;
        } = NetworkResolution.Empty("sin_referencia_independiente");
        public IReadOnlyList<NetworkResolution> LineResolutions { get; set; } =
            Array.Empty<NetworkResolution>();
    }

    private sealed class NetworkElement
    {
        public string Type { get; init; } = string.Empty;
        public string Key { get; init; } = string.Empty;
        public string Name { get; init; } = string.Empty;
        public string NormalizedName { get; init; } = string.Empty;
        public string EquivalenceKey { get; init; } = string.Empty;
        public double? VoltageKv { get; init; }
        public HashSet<string> GcrKeys { get; init; } =
            new(StringComparer.Ordinal);
        public string EndpointAName { get; init; } = string.Empty;
        public string EndpointBName { get; init; } = string.Empty;
        public GeoPoint? EndpointAPoint { get; init; }
        public GeoPoint? EndpointBPoint { get; init; }
        public string Characteristics { get; init; } = string.Empty;
        public IReadOnlySet<string> CircuitCodes { get; init; } =
            new HashSet<string>(StringComparer.Ordinal);
        public required JsonElement Geometry { get; init; }
        public GeoPoint? Point { get; init; }
    }

    private sealed record NetworkResolution(
        string State,
        NetworkElement? Element,
        int Score,
        double? DistanceKm,
        IReadOnlyList<string> Evidence,
        string DeclaredName,
        string NormalizedDeclaredName)
    {
        public bool NameExact { get; init; }
        public bool NameEquivalent { get; init; }
        public bool NameStrong { get; init; }
        public double NameSimilarity { get; init; }
        public bool NameUnique { get; init; }
        public bool NameScopeUnique { get; init; }
        public bool? GcrCompatible { get; init; }
        public bool? VoltageCompatible { get; init; }
        public bool? DistanceCompatible { get; init; }
        public double? DistanceDifferenceKm { get; init; }
        public int ScoreMargin { get; init; }
        public bool TopologyFirm { get; init; }
        public bool VoltageTopologyFirm { get; init; }
        public bool TerritorialHomonymRejected { get; init; }
        public IReadOnlyList<double> NetworkVoltageLevelsKv { get; init; } =
            Array.Empty<double>();
        public IReadOnlyList<GeoPoint> TopologyPoints { get; init; } =
            Array.Empty<GeoPoint>();
        public IReadOnlyList<string> SupportingLines { get; init; } =
            Array.Empty<string>();
        public string LineTopologyState { get; init; } = string.Empty;
        public bool LineTopologyFirm { get; init; }
        public int ResolvedLineEndpoints { get; init; }
        public string EndpointAName { get; init; } = string.Empty;
        public string EndpointBName { get; init; } = string.Empty;
        public string EndpointAResolvedName { get; init; } = string.Empty;
        public string EndpointBResolvedName { get; init; } = string.Empty;
        public double? EndpointADistanceKm { get; init; }
        public double? EndpointBDistanceKm { get; init; }
        public IReadOnlyList<string> LineTopologyEvidence { get; init; } =
            Array.Empty<string>();
        public string LineMatchMode { get; init; } = string.Empty;
        public bool LineEndpointPairFirm { get; init; }
        public bool LineCircuitCodeFirm { get; init; }
        public IReadOnlyList<string> DeclaredCircuitCodes { get; init; } =
            Array.Empty<string>();
        public IReadOnlyList<string> CatalogCircuitCodes { get; init; } =
            Array.Empty<string>();

        public static NetworkResolution Empty(string state) =>
            new(
                state,
                null,
                0,
                null,
                Array.Empty<string>(),
                string.Empty,
                string.Empty);
    }

    private sealed class PamSpec
    {
        public required string PaddedText { get; init; }
        public required HashSet<string> ProjectKeys { get; init; }
        public required string GcrKey { get; init; }
        public required HashSet<double> VoltagesKv { get; init; }

        public static PamSpec Create(PamTerritorialProyecto project)
        {
            var source = string.Join(
                Environment.NewLine,
                project.ClaveProyecto,
                project.ClavesAlternas,
                project.NombreProyecto,
                project.ZonaAtendida,
                project.ElementosEquiposAsociados);
            var keys = string.Join(
                    Environment.NewLine,
                    project.ClaveProyecto,
                    project.ClavesAlternas)
                .Split(
                    new[] { "\r\n", "\n", "\r", "·", "/", ",", ";" },
                    StringSplitOptions.RemoveEmptyEntries | StringSplitOptions.TrimEntries)
                .Select(NormalizeFolio)
                .Where(value => !string.IsNullOrWhiteSpace(value))
                .ToHashSet(StringComparer.Ordinal);
            return new PamSpec
            {
                PaddedText = $" {NormalizeText(source)} ",
                ProjectKeys = keys,
                GcrKey = ResolveGcrKey(project.Gcr),
                VoltagesKv = Regex.Matches(
                        source,
                        @"(\d+(?:[.,]\d+)?)\s*(?:KV|KILOVOLT)",
                        RegexOptions.IgnoreCase | RegexOptions.CultureInvariant)
                    .Select(match => ParseNumber(match.Groups[1].Value, 1, 500))
                    .Where(value => value.HasValue)
                    .Select(value => value!.Value)
                    .ToHashSet()
            };
        }
    }

    private sealed class TechnicalAnnexEvidenceCatalog
    {
        [JsonPropertyName("version")]
        public string Version { get; init; } = string.Empty;

        [JsonPropertyName("evidencias")]
        public IReadOnlyList<TechnicalAnnexEvidence> Evidence { get; init; } =
            Array.Empty<TechnicalAnnexEvidence>();
    }

    private sealed class TechnicalAnnexEvidence
    {
        [JsonPropertyName("pre_folio")]
        public string PreFolio { get; init; } = string.Empty;

        [JsonPropertyName("subestacion_declarada")]
        public string DeclaredSubstation { get; init; } = string.Empty;

        [JsonPropertyName("modo")]
        public string Mode { get; init; } = string.Empty;

        [JsonPropertyName("gcr")]
        public string Gcr { get; init; } = string.Empty;

        [JsonPropertyName("tension_kv")]
        public double VoltageKv { get; init; }

        [JsonPropertyName("coincidencia_catalogo")]
        public string CatalogMatch { get; init; } = string.Empty;

        [JsonPropertyName("longitud")]
        public double Longitude { get; init; }

        [JsonPropertyName("latitud")]
        public double Latitude { get; init; }

        [JsonPropertyName("tolerancia_catalogo_km")]
        public double CatalogToleranceKm { get; init; } = 0.25;

        [JsonPropertyName("fuente_url")]
        public string SourceUrl { get; init; } = string.Empty;

        [JsonPropertyName("sha256_archivo_fuente")]
        public string SourceSha256 { get; init; } = string.Empty;

        [JsonPropertyName("documento")]
        public string Document { get; init; } = string.Empty;

        [JsonPropertyName("sha256_documento")]
        public string DocumentSha256 { get; init; } = string.Empty;

        [JsonPropertyName("evidencias")]
        public IReadOnlyList<string> Evidence { get; init; } =
            Array.Empty<string>();

        [JsonIgnore]
        public GeoPoint? Point =>
            Latitude is >= 14 and <= 33.5 &&
            Longitude is >= -118 and <= -86
                ? new GeoPoint(Latitude, Longitude)
                : null;
    }

    private sealed record TraceDecision(
        string Decision,
        string Timestamp,
        string Gcr,
        long SortValue);

    private readonly record struct GeoPoint(double Latitude, double Longitude);

    private sealed record LineEndpointReference(
        string Name,
        string NormalizedName,
        string LineKey,
        string LineName,
        double? VoltageKv,
        GeoPoint Point,
        IReadOnlySet<string> GcrKeys);

    private sealed record LineTopologyEvidence(
        string State,
        bool IsFirm,
        int ResolvedEndpoints,
        LineEndpointTopology EndpointA,
        LineEndpointTopology EndpointB,
        IReadOnlyList<string> Evidence)
    {
        public static LineTopologyEvidence Empty { get; } =
            new(
                "sin_geometria",
                false,
                0,
                LineEndpointTopology.Empty,
                LineEndpointTopology.Empty,
                Array.Empty<string>());
    }

    private sealed record LineResolutionCandidate(
        NetworkElement Element,
        int Score,
        bool? VoltageCompatible,
        bool? GcrCompatible);

    private sealed record LineCodeResolutionCandidate(
        LineResolutionCandidate Candidate,
        int TokenOverlap);

    private sealed record LineEndpointTopology(
        string DeclaredName,
        string CatalogName,
        double? DistanceKm,
        bool IsResolved,
        bool IsAmbiguous,
        IReadOnlyList<string> Evidence)
    {
        public static LineEndpointTopology Empty { get; } =
            new(
                string.Empty,
                string.Empty,
                null,
                false,
                false,
                Array.Empty<string>());

        public static LineEndpointTopology Unresolved(
            string declaredName,
            string evidence) =>
            new(
                declaredName,
                string.Empty,
                null,
                false,
                false,
                new[] { evidence });
    }

    private sealed record VoltageNetworkSupport(
        IReadOnlyList<double> LevelsKv,
        IReadOnlyList<string> MatchingLines)
    {
        public static VoltageNetworkSupport Empty { get; } =
            new(
                Array.Empty<double>(),
                Array.Empty<string>());
    }

    private sealed record GcrArea(
        string Key,
        IReadOnlyList<PolygonShape> Polygons,
        double MinLongitude,
        double MinLatitude,
        double MaxLongitude,
        double MaxLatitude);

    private sealed record PolygonShape(
        IReadOnlyList<GeoPoint> Exterior,
        IReadOnlyList<IReadOnlyList<GeoPoint>> Holes,
        double MinLongitude,
        double MinLatitude,
        double MaxLongitude,
        double MaxLatitude);

    private sealed class CoverageSnapshot
    {
        public string VersionReglas { get; init; } = string.Empty;
        public DateTime GuardadoUtc { get; init; }
        public PamConvocatoriaCoverageReport? Coverage { get; init; }
    }

    private sealed class SecondCallIndexes
    {
        public int PreFolio { get; init; }
        public int ProjectFolio { get; init; }
        public int Name { get; init; }
        public int Project { get; init; }
        public int Description { get; init; }
        public int Substation { get; init; }
        public int Gcr { get; init; }
        public int InterconnectionPoint { get; init; }
        public int VoltageKv { get; init; }
        public int NamedSubstation { get; init; }
        public int DistanceKm { get; init; }
        public int State { get; init; }
        public int Municipality { get; init; }
        public int ProjectVertexStart { get; init; }
        public int ProjectVertexEnd { get; init; }
        public int ProjectKml { get; init; }
        public int SubstationVertexStart { get; init; }
        public int SubstationVertexEnd { get; init; }
        public int SubstationKml { get; init; }

        public static SecondCallIndexes Create(IReadOnlyList<string> header)
        {
            var substation = FindIndex(
                header,
                "SUBESTACIÓN ELÉCTRICA DE INTERCONEXIÓN");
            var locationState = FindIndex(
                header,
                "ENTIDAD FEDERATIVA",
                Math.Max(0, FindIndex(header, "MONTO DE INVERSIÓN TOTAL DEL PROYECTO")));
            var municipality = FindIndex(
                header,
                "MUNICIPIO/ALCALDÍA",
                Math.Max(0, locationState));
            var projectVertexStart = FindIndex(
                header,
                "VÉRTICE A",
                Math.Max(0, municipality));
            var projectKml = FindIndex(
                header,
                "ARCHIVO KMZ",
                Math.Max(0, projectVertexStart));
            var substationVertexStart = FindIndex(
                header,
                "VÉRTICE A",
                Math.Max(0, projectKml + 1));
            var substationKml = FindIndex(
                header,
                "ARCHIVO KMZ",
                Math.Max(0, substationVertexStart));

            return new SecondCallIndexes
            {
                PreFolio = FindIndex(header, "PRE FOLIO"),
                ProjectFolio = FindIndex(header, "FOLIO PROYECTO"),
                Name = FindIndex(header, "NOMBRE"),
                Project = FindIndex(header, "PROYECTO"),
                Description = FindIndex(header, "DESCRIPCIÓN PROYECTO"),
                Substation = substation,
                Gcr = FindIndex(header, "GERENCIA REGIONAL"),
                InterconnectionPoint = FindIndex(header, "PUNTO INTERCONEXIÓN"),
                VoltageKv = FindIndex(header, "NIVEL DE TENSIÓN (KV):"),
                NamedSubstation = FindIndex(
                    header,
                    "¿CUÁL ES EL NOMBRE DE LA SUBESTACIÓN A LA CUAL SE CONECTARÍA?"),
                DistanceKm = FindIndex(
                    header,
                    "¿CUÁL ES LA DISTANCIA QUE HAY ENTRE EL PROYECTO Y LA SUBESTACIÓN?"),
                State = locationState >= 0
                    ? locationState
                    : FindIndex(header, "ENTIDAD FEDERATIVA", substation),
                Municipality = municipality,
                ProjectVertexStart = projectVertexStart,
                ProjectVertexEnd = projectKml,
                ProjectKml = projectKml,
                SubstationVertexStart = substationVertexStart,
                SubstationVertexEnd = substationKml,
                SubstationKml = substationKml
            };
        }
    }
}
