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
    public int CacheMinutes { get; set; } = 30;
    public int HighConfidenceThreshold { get; set; } = 90;
    public int ReviewThreshold { get; set; } = 70;
    public int MaximumMatchesPerPam { get; set; } = 12;
    public double SubstationCoordinateToleranceKm { get; set; } = 3;
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
    public const string RulesVersion = "PAM-CONV2-v1.0";

    private const string CacheKey = "pam-conv2-evidence-catalog-v1";
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
                candidate.Geometria.HasValue &&
                !string.Equals(candidate.Estado, "catalogada", StringComparison.Ordinal))
            .ToList();

        return new PamConvocatoriaGeoJson
        {
            Meta = new PamConvocatoriaGeoJsonMeta
            {
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
                    Geometry = candidate.Geometria!.Value,
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
                        ["gcr"] = candidate.Gcr,
                        ["entidad"] = candidate.Entidad,
                        ["municipio"] = candidate.Municipio,
                        ["proyectos_relacionados"] = candidate.ProyectosRelacionados,
                        ["folios"] = string.Join(" · ", candidate.Folios),
                        ["evidencias"] = string.Join(" · ", candidate.Evidencias),
                        ["fuente"] = candidate.Fuente,
                        ["validada"] = false,
                        ["advertencia"] =
                            "Evidencia automática pendiente de validación; no forma parte del grafo oficial."
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
                if (row.SubstationResolution.DistanceKm.HasValue &&
                    row.DeclaredDistanceKm.HasValue)
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
            await Task.WhenAll(baseTask, traceTask, substationsTask, linesTask);

            var substations = ParseSubstations(await substationsTask);
            var lines = ParseLines(await linesTask);
            var rows = ParseSecondCallRows(
                await baseTask,
                await traceTask,
                substations,
                lines);
            var coverage = BuildCoverage(rows);
            var catalog = new EvidenceCatalog(rows, substations, lines, coverage);

            _cache.Set(
                CacheKey,
                catalog,
                TimeSpan.FromMinutes(Math.Max(5, _options.CacheMinutes)));
            _logger.LogInformation(
                "Segunda Convocatoria cargada: {Projects} proyectos con decisión Continúa, {Substations} subestaciones declaradas y {LineRefs} referencias de línea.",
                rows.Count,
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
        IReadOnlyList<NetworkElement> lines)
    {
        var activeDecisions = ParseActiveDecisions(traceCsv);
        if (activeDecisions.Count == 0)
        {
            throw new InvalidDataException(
                "La trazabilidad de Segunda Convocatoria no contiene decisiones vigentes Continúa.");
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
        var output = new List<SecondCallRow>();

        foreach (var values in csvRows.Skip(headerIndex + 1))
        {
            var preFolio = Clean(Read(values, indexes.PreFolio));
            var decisionKey = NormalizeFolio(preFolio);
            if (string.IsNullOrWhiteSpace(decisionKey) ||
                !activeDecisions.TryGetValue(decisionKey, out var decision))
            {
                continue;
            }

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
                    decision.Gcr),
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
                DecisionDate = decision.Timestamp
            };
            row.SubstationResolution = ResolveSubstation(row, substations);
            row.LineResolutions = ResolveLines(row, lines);
            output.Add(row);
        }

        return output;
    }

    private Dictionary<string, TraceDecision> ParseActiveDecisions(string csv)
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

        return latest
            .Where(pair =>
                string.Equals(
                    NormalizeText(pair.Value.Decision),
                    "CONTINUA",
                    StringComparison.Ordinal))
            .ToDictionary(pair => pair.Key, pair => pair.Value, StringComparer.Ordinal);
    }

    private NetworkResolution ResolveSubstation(
        SecondCallRow row,
        IReadOnlyList<NetworkElement> substations)
    {
        if (string.IsNullOrWhiteSpace(row.NormalizedSubstation))
        {
            return NetworkResolution.Empty("sin_referencia");
        }

        var candidates = new List<(NetworkElement Element, int Score, double? Distance)>();
        foreach (var element in substations)
        {
            var score = 0;
            if (string.Equals(
                    element.NormalizedName,
                    row.NormalizedSubstation,
                    StringComparison.Ordinal))
            {
                score += 55;
            }
            else if (ContainsWhole(
                         $" {element.NormalizedName} ",
                         row.NormalizedSubstation) ||
                     ContainsWhole(
                         $" {row.NormalizedSubstation} ",
                         element.NormalizedName))
            {
                score += 35;
            }
            else
            {
                continue;
            }

            if (row.VoltageKv.HasValue && element.VoltageKv.HasValue)
            {
                score += Math.Abs(row.VoltageKv.Value - element.VoltageKv.Value) < 0.6
                    ? 15
                    : -15;
            }

            double? distance = null;
            var origin = row.SubstationPoint ?? row.ProjectPoint;
            if (origin.HasValue &&
                element.Point.HasValue &&
                row.DeclaredDistanceKm.HasValue)
            {
                distance = HaversineKm(origin.Value, element.Point.Value);
                var tolerance = Math.Max(
                    _options.SubstationCoordinateToleranceKm,
                    row.DeclaredDistanceKm.Value * 0.25);
                var difference = Math.Abs(distance.Value - row.DeclaredDistanceKm.Value);
                if (difference <= tolerance)
                {
                    score += 25;
                }
                else if (difference <= tolerance * 2)
                {
                    score += 10;
                }
                else
                {
                    score -= 20;
                }
            }

            candidates.Add((element, score, distance));
        }

        var ordered = candidates
            .OrderByDescending(candidate => candidate.Score)
            .ThenBy(candidate => candidate.Distance ?? double.MaxValue)
            .ToList();
        if (ordered.Count == 0)
        {
            return NetworkResolution.Empty("faltante");
        }

        var best = ordered[0];
        var ambiguous = ordered.Count > 1 &&
            best.Score - ordered[1].Score < 10;
        var state = best.Score >= 85 && !ambiguous
            ? "catalogada"
            : best.Score >= 40
                ? "revision"
                : "faltante";
        var evidence = new List<string>
        {
            $"nombre comparado con catálogo ({best.Score} puntos)"
        };
        if (best.Distance.HasValue)
        {
            evidence.Add(
                $"distancia calculada desde la subestación del proyecto al catálogo: {best.Distance:0.##} km");
            if (row.DeclaredDistanceKm.HasValue)
            {
                evidence.Add(
                    $"distancia declarada hacia la interconexión: {row.DeclaredDistanceKm:0.##} km");
            }
        }
        if (ambiguous)
        {
            evidence.Add("existen candidatos con puntaje similar");
        }

        return new NetworkResolution(
            state,
            best.Element,
            best.Score,
            best.Distance,
            evidence,
            row.DeclaredSubstation,
            row.NormalizedSubstation);
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
            var representative = group
                .OrderByDescending(row => row.SubstationResolution.Score)
                .ThenByDescending(row => row.SubstationGeometry.HasValue)
                .First();
            var resolution = representative.SubstationResolution;
            var mayPublishCatalogGeometry =
                !string.Equals(resolution.State, "faltante", StringComparison.Ordinal);
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
                Estado = resolution.State,
                NombreDeclarado = representative.DeclaredSubstation,
                CoincidenciaCatalogo = resolution.Element?.Name ?? string.Empty,
                ClaveCatalogo = resolution.Element?.Key ?? string.Empty,
                Puntaje = Math.Clamp(resolution.Score, 0, 100),
                TensionKv = representative.VoltageKv,
                DistanciaCatalogoKm = resolution.DistanceKm,
                Gcr = FirstNonEmpty(group.Select(row => row.Gcr).ToArray()),
                Entidad = FirstNonEmpty(group.Select(row => row.State).ToArray()),
                Municipio = FirstNonEmpty(group.Select(row => row.Municipality).ToArray()),
                ProyectosRelacionados = group.Count(),
                Folios = group
                    .Select(row => row.PreFolio)
                    .Where(value => !string.IsNullOrWhiteSpace(value))
                    .Distinct(StringComparer.OrdinalIgnoreCase)
                    .Take(20)
                    .ToList(),
                Evidencias = resolution.Evidence,
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

        return new PamConvocatoriaCoverageReport
        {
            ProyectosContinuan = rows.Count,
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

    private List<NetworkElement> ParseSubstations(string json)
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
                    Geometry = geometryClone,
                    Point = point
                };
            })
            .Where(element => element is not null)
            .Cast<NetworkElement>()
            .ToList();
    }

    private List<NetworkElement> ParseLines(string json)
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
                    Geometry = geometryClone,
                    Point = representative
                };
            })
            .Where(element => element is not null)
            .Cast<NetworkElement>()
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
            ["CE"] = "central",
            ["VM"] = "central",
            ["CENTRAL"] = "central"
        };
        return aliases.GetValueOrDefault(normalized, string.Empty);
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

    private sealed record EvidenceCatalog(
        IReadOnlyList<SecondCallRow> Rows,
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
        public string DecisionDate { get; init; } = string.Empty;
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
