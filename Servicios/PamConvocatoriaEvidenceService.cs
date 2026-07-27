using Microsoft.Extensions.Caching.Memory;
using Microsoft.Extensions.Options;
using Microsoft.VisualBasic.FileIO;
using NSIE.Models;
using System.Globalization;
using System.Security.Cryptography;
using System.Text;
using System.Text.Json;
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
    public int HighConfidenceThreshold { get; set; } = 90;
    public int ReviewThreshold { get; set; } = 70;
    public int MaximumMatchesPerPam { get; set; } = 12;
    public double SubstationCoordinateToleranceKm { get; set; } = 3;
    public int UniqueSubstationNameBonus { get; set; } = 20;
    public double StrongNameSimilarity { get; set; } = 0.9;
    public int StrongNameMargin { get; set; } = 15;
    public double TopologyEndpointClusterKm { get; set; } = 3;
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
        CancellationToken cancellationToken);

    Task<PamConvocatoriaGeoJson> ObtenerCoberturaGeoJsonAsync(
        CancellationToken cancellationToken);
}

public sealed class PamConvocatoriaEvidenceService : IPamConvocatoriaEvidenceService
{
    public const string RulesVersion = "PAM-CONV2-v1.3";

    private const string CacheKey = "pam-conv2-evidence-catalog-v3";
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

    private readonly HttpClient _httpClient;
    private readonly IMemoryCache _cache;
    private readonly PamConvocatoriaEvidenceOptions _options;
    private readonly ILogger<PamConvocatoriaEvidenceService> _logger;

    public PamConvocatoriaEvidenceService(
        HttpClient httpClient,
        IMemoryCache cache,
        IOptions<PamConvocatoriaEvidenceOptions> options,
        ILogger<PamConvocatoriaEvidenceService> logger)
    {
        _httpClient = httpClient;
        _cache = cache;
        _options = options.Value;
        _logger = logger;
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
        CancellationToken cancellationToken)
    {
        var catalog = await ObtenerCatalogoAsync(cancellationToken);
        return catalog.Coverage;
    }

    public async Task<PamConvocatoriaGeoJson> ObtenerCoberturaGeoJsonAsync(
        CancellationToken cancellationToken)
    {
        var report = await ObtenerCoberturaAsync(cancellationToken);
        var candidates = report.Candidatos
            .Where(candidate =>
                (candidate.GeometriaSubestacionPrivada.HasValue ||
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
                        ["similitud_nombre"] =
                            candidate.SimilitudNombre,
                        ["margen_puntaje"] =
                            candidate.MargenPuntaje,
                        ["gcr_catalogo"] =
                            candidate.GcrCatalogo,
                        ["coincidencia_topologica_firme"] =
                            candidate.CoincidenciaTopologicaFirme,
                        ["coincidencia_automatica_firme"] =
                            candidate.CoincidenciaAutomaticaFirme,
                        ["motivo_automatizacion"] =
                            candidate.MotivoAutomatizacion,
                        ["lineas_soporte"] =
                            string.Join(" · ", candidate.LineasSoporte),
                        ["origen_geometria"] =
                            candidate.GeometriaSubestacionPrivada.HasValue
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
                            "La geometría de Segunda Convocatoria representa la subestación privada del proyecto; no constituye por sí misma un nodo CFE/CENACE."
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
            var lineEndpoints = BuildLineEndpoints(lines);
            var allRows = ParseSecondCallRows(
                await baseTask,
                await traceTask,
                substations,
                lines,
                lineEndpoints);
            var activeRows = allRows
                .Where(row => row.IsActive)
                .ToList();
            var coverage = BuildCoverage(allRows);
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

    private List<SecondCallRow> ParseSecondCallRows(
        string baseCsv,
        string traceCsv,
        IReadOnlyList<NetworkElement> substations,
        IReadOnlyList<NetworkElement> lines,
        IReadOnlyList<LineEndpointReference> lineEndpoints)
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
            .Where(element => !string.IsNullOrWhiteSpace(element.NormalizedName))
            .GroupBy(
                element => NormalizeSubstationAlias(element.NormalizedName),
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
                            $"{gcr}|{NormalizeSubstationAlias(element.NormalizedName)}"
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

            var interconnectionPoint = Clean(
                Read(values, indexes.InterconnectionPoint));
            var declaredSubstation = SelectDeclaredSubstation(
                Clean(Read(values, indexes.Substation)),
                Clean(Read(values, indexes.NamedSubstation)),
                ExtractSubstationFromPoint(interconnectionPoint));
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
                VoltageKv = voltage,
                DeclaredDistanceKm = ParseNumber(
                    Read(values, indexes.DistanceKm),
                    0,
                    1000),
                ProjectGeometry = projectGeometry,
                ProjectPoint = projectPoint,
                SubstationGeometry = substationGeometry,
                SubstationPoint = substationPoint,
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
            row.LineResolutions = ResolveLines(row, lines);
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
        var rowGcr = ResolveGcrKey(row.Gcr);
        var candidates = new List<(
            NetworkElement Element,
            int Score,
            bool NameExact,
            bool NameStrong,
            double NameSimilarity,
            bool NameUnique,
            bool NameScopeUnique,
            bool? GcrCompatible,
            bool? VoltageCompatible,
            double? CalculatedDistance,
            bool? DistanceCompatible,
            double? DistanceDifference)>();
        foreach (var element in substations)
        {
            var score = 0;
            var catalogAlias =
                NormalizeSubstationAlias(element.NormalizedName);
            var nameSimilarity = CalculateNameSimilarity(
                declaredAlias,
                catalogAlias);
            var nameExact = string.Equals(
                    catalogAlias,
                    declaredAlias,
                    StringComparison.Ordinal);
            var nameStrong =
                nameExact ||
                nameSimilarity >= _options.StrongNameSimilarity;
            if (nameExact)
            {
                score += 55;
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
                nameExact &&
                substationNameCounts.GetValueOrDefault(
                    catalogAlias) == 1;
            if (nameUnique)
            {
                score += _options.UniqueSubstationNameBonus;
            }
            var nameScopeUnique =
                nameExact &&
                !string.IsNullOrWhiteSpace(rowGcr) &&
                element.GcrKeys.Contains(rowGcr) &&
                substationScopeNameCounts.GetValueOrDefault(
                    $"{rowGcr}|{catalogAlias}") == 1;
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
            if (row.VoltageKv.HasValue && element.VoltageKv.HasValue)
            {
                voltageCompatible =
                    Math.Abs(
                        row.VoltageKv.Value -
                        element.VoltageKv.Value) < 0.6;
                score += voltageCompatible.Value ? 15 : -15;
            }

            double? calculatedDistance = null;
            bool? distanceCompatible = null;
            double? distanceDifference = null;
            var privateSubstationPoint =
                row.SubstationPoint ?? row.ProjectPoint;
            if (privateSubstationPoint.HasValue &&
                element.Point.HasValue &&
                row.DeclaredDistanceKm.HasValue)
            {
                calculatedDistance = HaversineKm(
                    privateSubstationPoint.Value,
                    element.Point.Value);
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

            candidates.Add((
                element,
                score,
                nameExact,
                nameStrong,
                nameSimilarity,
                nameUnique,
                nameScopeUnique,
                gcrCompatible,
                voltageCompatible,
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
            return ResolveTopologyEndpoint(row, lineEndpoints);
        }

        var best = ordered[0];
        var scoreMargin = ordered.Count > 1
            ? best.Score - ordered[1].Score
            : 100;
        var ambiguous =
            scoreMargin < _options.StrongNameMargin;
        var state = best.Score >= 85 && !ambiguous
            ? "catalogada"
            : best.Score >= 40
                ? "revision"
                : "faltante";
        var evidence = new List<string>
        {
            $"nombre comparado con catálogo ({best.Score} puntos)"
        };
        if (best.NameExact)
        {
            evidence.Add(best.NameUnique
                ? "nombre exacto tras normalización documental y único en el catálogo"
                : "nombre exacto tras normalización documental con homónimos o registros repetidos en el catálogo");
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
        if (best.GcrCompatible.HasValue)
        {
            evidence.Add(best.GcrCompatible.Value
                ? $"GCR compatible: {row.Gcr}"
                : $"GCR incompatible: {row.Gcr}");
        }
        if (best.VoltageCompatible.HasValue)
        {
            evidence.Add(best.VoltageCompatible.Value
                ? $"tensión compatible: {row.VoltageKv:0.##} kV"
                : $"tensión incompatible: convocatoria {row.VoltageKv:0.##} kV / catálogo {best.Element.VoltageKv:0.##} kV");
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
            best.Score,
            best.CalculatedDistance,
            evidence,
            row.DeclaredSubstation,
            row.NormalizedSubstation)
        {
            NameExact = best.NameExact,
            NameStrong = best.NameStrong,
            NameSimilarity = best.NameSimilarity,
            NameUnique = best.NameUnique,
            NameScopeUnique = best.NameScopeUnique,
            GcrCompatible = best.GcrCompatible,
            VoltageCompatible = best.VoltageCompatible,
            DistanceCompatible = best.DistanceCompatible,
            DistanceDifferenceKm = best.DistanceDifference,
            ScoreMargin = scoreMargin
        };
    }

    private NetworkResolution ResolveTopologyEndpoint(
        SecondCallRow row,
        IReadOnlyList<LineEndpointReference> lineEndpoints)
    {
        var declaredAlias =
            NormalizeSubstationAlias(row.NormalizedSubstation);
        var rowGcr = ResolveGcrKey(row.Gcr);
        var exactReferences = lineEndpoints
            .Where(reference => string.Equals(
                reference.NormalizedName,
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
            SupportingLines = supportingLines
        };
    }

    private List<NetworkResolution> ResolveLines(
        SecondCallRow row,
        IReadOnlyList<NetworkElement> lines)
    {
        var source = string.Join(
            Environment.NewLine,
            row.InterconnectionPoint,
            row.Description);
        var padded = $" {NormalizeText(source)} ";
        var results = new List<NetworkResolution>();

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
            results.Add(new NetworkResolution(
                score >= 70 ? "catalogada" : "revision",
                element,
                score,
                null,
                new[] { "nombre de línea presente en los campos de interconexión" },
                element.Name,
                element.NormalizedName));
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
            if (candidates.Count == 0)
            {
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
                    normalized));
                continue;
            }

            var best = candidates[0];
            results.Add(new NetworkResolution(
                best.Score >= 70 ? "catalogada" : "revision",
                best.Element,
                best.Score,
                null,
                new[] { "referencia de línea comparada con el catálogo geoespacial" },
                declared,
                normalized));
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

    private PamConvocatoriaCoverageReport BuildCoverage(
        IReadOnlyList<SecondCallRow> rows)
    {
        var candidates = new List<PamConvocatoriaInfrastructureCandidate>();
        var substationGroups = rows
            .Where(row => !string.IsNullOrWhiteSpace(row.NormalizedSubstation))
            .GroupBy(
                row => $"{row.NormalizedSubstation}|{row.VoltageKv:0.##}",
                StringComparer.Ordinal);
        foreach (var group in substationGroups)
        {
            var groupRows = group.ToList();
            var representative = groupRows
                .OrderByDescending(row => row.SubstationResolution.Score)
                .ThenByDescending(row => row.SubstationGeometry.HasValue)
                .First();
            var resolution = representative.SubstationResolution;
            var resolutions = groupRows
                .Select(row => row.SubstationResolution)
                .ToList();
            var catalogKeys = resolutions
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
                catalogKeys.Count == 1 &&
                resolutions.All(item => item.Element is not null);
            var allExactNames = resolutions.All(item => item.NameExact);
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
                minimumNameSimilarity >= _options.StrongNameSimilarity &&
                minimumScoreMargin >= _options.StrongNameMargin &&
                nameScopeUnique &&
                gcrSupport &&
                (voltageSupport || allDeclaredDistancesCompatible) &&
                !voltageConflict &&
                !distanceConflict &&
                !effectiveGcrConflict;
            var topologyFirm =
                catalogKeys.Count == 0 &&
                resolutions.All(item => item.TopologyFirm);
            var automaticFirm =
                exactCatalogFirm ||
                strongCatalogFirm ||
                topologyFirm;
            var topologyPoints = resolutions
                .SelectMany(item => item.TopologyPoints)
                .Distinct()
                .ToList();
            var supportingLines = resolutions
                .SelectMany(item => item.SupportingLines)
                .Distinct(StringComparer.OrdinalIgnoreCase)
                .OrderBy(value => value, StringComparer.OrdinalIgnoreCase)
                .ToList();
            var calculatedDistances = resolutions
                .Where(item => item.DistanceKm.HasValue)
                .Select(item => item.DistanceKm!.Value)
                .ToList();
            var distanceDifferences = resolutions
                .Where(item => item.DistanceDifferenceKm.HasValue)
                .Select(item => item.DistanceDifferenceKm!.Value)
                .ToList();
            var privateSubstationPoints = groupRows
                .Where(row => row.SubstationPoint.HasValue)
                .Select(row => row.SubstationPoint!.Value)
                .Distinct()
                .ToList();
            var candidateEvidence = resolutions
                .SelectMany(item => item.Evidence)
                .Distinct(StringComparer.Ordinal)
                .ToList();
            candidateEvidence.Add(
                $"consistencia del grupo: {catalogKeys.Count} resolución(es) de catálogo distinta(s), {groupRows.Count} fila(s)");
            if (declaredDistanceRows > 0)
            {
                candidateEvidence.Add(
                    $"distancias declaradas compatibles: {compatibleDistanceRows} de {declaredDistanceRows}");
            }

            var automationReason = automaticFirm
                ? topologyFirm
                    ? "Referencia faltante confirmable por extremo nominal de línea, GCR, tensión y agrupación geográfica compatibles."
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
                    "coincidencia automática firme por criterio compuesto v2");
            }

            var candidateState = automaticFirm && !topologyFirm
                ? "catalogada"
                : catalogKeys.Count == 0
                    ? "faltante"
                    : "revision";
            var mayPublishCatalogGeometry = catalogKeys.Count > 0;
            var geometry = mayPublishCatalogGeometry
                ? resolution.Element?.Geometry
                : null;
            var point = mayPublishCatalogGeometry
                ? resolution.Element?.Point
                : null;
            candidates.Add(new PamConvocatoriaInfrastructureCandidate
            {
                CandidatoId = StableKey(
                    "C2-SE",
                    representative.NormalizedSubstation,
                    representative.VoltageKv?.ToString(
                        "0.##",
                        CultureInfo.InvariantCulture) ?? string.Empty),
                TipoElemento = "subestacion",
                Estado = candidateState,
                NombreDeclarado = representative.DeclaredSubstation,
                CoincidenciaCatalogo = resolution.Element?.Name ?? string.Empty,
                ClaveCatalogo = resolution.Element?.Key ?? string.Empty,
                Puntaje = Math.Clamp(resolution.Score, 0, 100),
                TensionKv = representative.VoltageKv,
                DistanciaCatalogoKm = resolution.DistanceKm,
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
                NombreCatalogoUnico = nameUnique,
                NombreCoincidenciaFuerte = allStrongNames,
                SimilitudNombre = minimumNameSimilarity,
                MargenPuntaje = minimumScoreMargin,
                GcrCatalogo = resolution.Element is null
                    ? string.Empty
                    : string.Join(
                        ", ",
                        resolution.Element.GcrKeys.OrderBy(
                            value => value,
                            StringComparer.Ordinal)),
                CoincidenciaTopologicaFirme = topologyFirm,
                CoincidenciaAutomaticaFirme = automaticFirm,
                MotivoAutomatizacion = automationReason,
                LineasSoporte = supportingLines,
                GeometriaSubestacionPrivada =
                    CreateGeometry(privateSubstationPoints),
                GeometriaTopologicaSugerida =
                    CreatePointCollectionGeometry(topologyPoints),
                Gcr = FirstNonEmpty(groupRows.Select(row => row.Gcr).ToArray()),
                Entidad = FirstNonEmpty(groupRows.Select(row => row.State).ToArray()),
                Municipio = FirstNonEmpty(groupRows.Select(row => row.Municipality).ToArray()),
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
                Geometria = geometry,
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
            LineasRevision = candidates.Count(candidate =>
                string.Equals(candidate.TipoElemento, "linea_transmision", StringComparison.Ordinal) &&
                string.Equals(candidate.Estado, "revision", StringComparison.Ordinal)),
            LineasSinGeometria = candidates.Count(candidate =>
                string.Equals(candidate.TipoElemento, "linea_transmision", StringComparison.Ordinal) &&
                string.Equals(candidate.Estado, "tramo_sin_geometria", StringComparison.Ordinal)),
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
                    Geometry = geometryClone,
                    Point = representative
                };
            })
            .Where(element => element is not null)
            .Cast<NetworkElement>()
            .ToList();
    }

    private static IReadOnlyList<LineEndpointReference> BuildLineEndpoints(
        IReadOnlyList<NetworkElement> lines)
    {
        var output = new List<LineEndpointReference>(lines.Count * 2);
        foreach (var line in lines)
        {
            Add(line.EndpointAName, line.EndpointAPoint);
            Add(line.EndpointBName, line.EndpointBPoint);

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
            if (value.Length is >= 3 and <= 120 &&
                IsPlausibleLineReference(value))
            {
                results.Add(value);
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
        public double? VoltageKv { get; init; }
        public double? DeclaredDistanceKm { get; init; }
        public JsonElement? ProjectGeometry { get; init; }
        public GeoPoint? ProjectPoint { get; init; }
        public JsonElement? SubstationGeometry { get; init; }
        public GeoPoint? SubstationPoint { get; init; }
        public string ProjectKml { get; init; } = string.Empty;
        public string SubstationKml { get; init; } = string.Empty;
        public string Decision { get; init; } = string.Empty;
        public string DecisionDate { get; init; } = string.Empty;
        public bool IsActive { get; init; }
        public NetworkResolution SubstationResolution { get; set; } =
            NetworkResolution.Empty("sin_referencia");
        public IReadOnlyList<NetworkResolution> LineResolutions { get; set; } =
            Array.Empty<NetworkResolution>();
    }

    private sealed class NetworkElement
    {
        public string Type { get; init; } = string.Empty;
        public string Key { get; init; } = string.Empty;
        public string Name { get; init; } = string.Empty;
        public string NormalizedName { get; init; } = string.Empty;
        public double? VoltageKv { get; init; }
        public HashSet<string> GcrKeys { get; init; } =
            new(StringComparer.Ordinal);
        public string EndpointAName { get; init; } = string.Empty;
        public string EndpointBName { get; init; } = string.Empty;
        public GeoPoint? EndpointAPoint { get; init; }
        public GeoPoint? EndpointBPoint { get; init; }
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
        public IReadOnlyList<GeoPoint> TopologyPoints { get; init; } =
            Array.Empty<GeoPoint>();
        public IReadOnlyList<string> SupportingLines { get; init; } =
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
