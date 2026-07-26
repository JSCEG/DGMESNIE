using Microsoft.Extensions.Caching.Memory;
using Microsoft.Extensions.Options;
using NSIE.Models;
using System.Globalization;
using System.Security.Cryptography;
using System.Text;
using System.Text.Json;
using System.Text.RegularExpressions;

namespace NSIE.Servicios;

public sealed class PamRedAssociationOptions
{
    public const string SectionName = "PamTerritorial:AsociacionRed";

    public string SubestacionesUrl { get; set; } =
        "https://cdn.sassoapps.com/dgmesnie/geojson/dgmesnie_subestaciones2.geojson";
    public string LineasUrl { get; set; } =
        "https://cdn.sassoapps.com/dgmesnie/geojson/dgmesnie_lt.geojson";
    public string GerenciasUrl { get; set; } =
        "https://cdn.sassoapps.com/Mapas/gerencias_javs_2.geojson";
    public int CacheMinutos { get; set; } = 360;
    public int UmbralConfianzaAlta { get; set; } = 90;
    public int UmbralRevision { get; set; } = 70;
    public int MaximoCandidatosPorProyecto { get; set; } = 40;
    public double ToleranciaExtremoKm { get; set; } = 3;
}

public interface IPamRedAssociationService
{
    Task<IReadOnlyList<PamRedAssociationResult>> ResolverAsync(
        IReadOnlyCollection<PamTerritorialProyecto> proyectos,
        CancellationToken cancellationToken);

    IReadOnlyList<PamTerritorialUbicacion> CrearUbicaciones(
        PamRedAssociationResult resultado,
        bool soloConfianzaAlta);
}

public sealed partial class PamRedAssociationService : IPamRedAssociationService
{
    public const string RulesVersion = "PAM-RED-v1.0";

    private const string CatalogCacheKey = "pam-red-network-catalog-v1";
    private static readonly SemaphoreSlim CatalogLock = new(1, 1);

    private readonly HttpClient _httpClient;
    private readonly IMemoryCache _cache;
    private readonly PamRedAssociationOptions _options;
    private readonly ILogger<PamRedAssociationService> _logger;

    public PamRedAssociationService(
        HttpClient httpClient,
        IMemoryCache cache,
        IOptions<PamRedAssociationOptions> options,
        ILogger<PamRedAssociationService> logger)
    {
        _httpClient = httpClient;
        _cache = cache;
        _options = options.Value;
        _logger = logger;
    }

    public async Task<IReadOnlyList<PamRedAssociationResult>> ResolverAsync(
        IReadOnlyCollection<PamTerritorialProyecto> proyectos,
        CancellationToken cancellationToken)
    {
        if (proyectos.Count == 0)
        {
            return Array.Empty<PamRedAssociationResult>();
        }

        var catalog = await ObtenerCatalogoAsync(cancellationToken);
        var results = proyectos
            .Select(project => ResolverProyecto(project, catalog))
            .ToList();

        _logger.LogInformation(
            "Asociación PAM-red {Version}: {Projects} proyectos, {High} con candidatos de confianza alta y {Review} para revisión.",
            RulesVersion,
            results.Count,
            results.Count(result => result.ConfianzaAlta > 0),
            results.Count(result => result.RequierenRevision > 0));

        return results;
    }

    public IReadOnlyList<PamTerritorialUbicacion> CrearUbicaciones(
        PamRedAssociationResult resultado,
        bool soloConfianzaAlta)
    {
        var candidates = resultado.Candidatos
            .Where(candidate =>
                !soloConfianzaAlta ||
                string.Equals(
                    candidate.NivelConfianza,
                    "alta",
                    StringComparison.Ordinal))
            .Take(Math.Max(1, _options.MaximoCandidatosPorProyecto))
            .ToList();

        return candidates
            .Select((candidate, index) => new PamTerritorialUbicacion
            {
                UbicacionId = -checked(resultado.ProyectoId * 100_000L + index + 1),
                ProyectoId = resultado.ProyectoId,
                Etiqueta = candidate.NombreElementoRed,
                TipoGeometria = GetGeometryType(candidate.Geometria),
                Geometria = candidate.Geometria,
                Latitud = candidate.Latitud,
                Longitud = candidate.Longitud,
                PrecisionUbicacion = "asociada",
                MetodoUbicacion = "cruce_catalogo_red_v1",
                Fuente = candidate.Fuente,
                RadioSugeridoKm = string.Equals(
                    candidate.TipoElementoRed,
                    "linea_transmision",
                    StringComparison.Ordinal)
                        ? 10
                        : 20,
                Orden = index + 1,
                EsPrincipal = index == 0,
                Validada = false,
                EsAsociacionSugerida = true,
                PuntajeCoincidencia = candidate.Puntaje,
                NivelConfianza = candidate.NivelConfianza,
                TipoElementoRed = candidate.TipoElementoRed,
                ClaveElementoRed = candidate.ClaveElementoRed,
                Evidencias = candidate.Evidencias
            })
            .ToList();
    }

    private PamRedAssociationResult ResolverProyecto(
        PamTerritorialProyecto project,
        NetworkCatalog catalog)
    {
        var spec = ProjectSpec.Create(project);
        var states = new List<CandidateState>();

        foreach (var substation in catalog.Substations)
        {
            var candidate = ScoreSubstation(substation, spec, catalog);
            if (candidate is not null)
            {
                states.Add(candidate);
            }
        }

        foreach (var line in catalog.Lines)
        {
            var candidate = ScoreLine(line, spec, catalog);
            if (candidate is not null)
            {
                states.Add(candidate);
            }
        }

        ApplySpecificNamePenalty(states);
        ApplyTopologyBonus(states);

        var candidates = states
            .Where(candidate => candidate.Score >= _options.UmbralRevision)
            .OrderByDescending(candidate => candidate.Score)
            .ThenBy(candidate => candidate.Element.Type == NetworkElementType.Line ? 0 : 1)
            .ThenBy(candidate => candidate.Element.Name, StringComparer.OrdinalIgnoreCase)
            .Take(Math.Max(1, _options.MaximoCandidatosPorProyecto))
            .Select(ToDto)
            .ToList();

        var high = candidates.Count(candidate =>
            string.Equals(candidate.NivelConfianza, "alta", StringComparison.Ordinal));
        var review = candidates.Count - high;

        return new PamRedAssociationResult
        {
            ProyectoId = project.ProyectoId,
            ClaveProyecto = project.ClaveProyecto,
            GcrProyecto = spec.GcrKey,
            VersionReglas = RulesVersion,
            Estado = high > 0
                ? "confianza_alta"
                : review > 0
                    ? "requiere_revision"
                    : "sin_coincidencias",
            ConfianzaAlta = high,
            RequierenRevision = review,
            Candidatos = candidates
        };
    }

    private CandidateState? ScoreSubstation(
        NetworkElement element,
        ProjectSpec spec,
        NetworkCatalog catalog)
    {
        if (!ContainsWhole(spec.PaddedText, element.NormalizedName))
        {
            return null;
        }

        var score = 45;
        var evidence = new List<string>
        {
            "nombre de subestación presente en el PAM (+45)"
        };

        if (HasTaggedElement(spec.SourceLines, "SE", element.NormalizedName))
        {
            score += 15;
            evidence.Add("mención explícita como SE en elementos asociados (+15)");
        }

        if (ContainsWhole(spec.PaddedTitle, element.NormalizedName))
        {
            score += 10;
            evidence.Add("nombre presente en el título del proyecto (+10)");
        }

        ApplyGcrScore(element, spec, ref score, evidence);
        ApplyVoltageScore(element, spec, ref score, evidence);

        var sameNameCount = catalog.SubstationNameCounts.GetValueOrDefault(
            BuildNameScopeKey(element.NormalizedName, spec.GcrKey),
            catalog.SubstationNameCounts.GetValueOrDefault(
                BuildNameScopeKey(element.NormalizedName, string.Empty),
                1));
        if (sameNameCount == 1)
        {
            score += 10;
            evidence.Add("nombre único dentro del ámbito GCR (+10)");
        }
        else
        {
            score -= 15;
            evidence.Add($"homónimo: {sameNameCount} elementos candidatos (-15)");
        }

        if (element.NormalizedName.Split(' ', StringSplitOptions.RemoveEmptyEntries).Length == 1 &&
            element.NormalizedName.Length < 7)
        {
            score -= 10;
            evidence.Add("nombre corto con riesgo de falso positivo (-10)");
        }

        return new CandidateState(element, score, evidence);
    }

    private CandidateState? ScoreLine(
        NetworkElement element,
        ProjectSpec spec,
        NetworkCatalog catalog)
    {
        var fullName = ContainsWhole(spec.PaddedText, element.NormalizedName);
        var endpointA = !string.IsNullOrWhiteSpace(element.EndpointA) &&
            ContainsWhole(spec.PaddedText, element.EndpointA);
        var endpointB = !string.IsNullOrWhiteSpace(element.EndpointB) &&
            ContainsWhole(spec.PaddedText, element.EndpointB);

        if (!fullName && !(endpointA && endpointB))
        {
            return null;
        }

        var score = 0;
        var evidence = new List<string>();

        if (fullName)
        {
            score += 40;
            evidence.Add("nombre completo de línea presente en el PAM (+40)");
        }

        if (endpointA && endpointB)
        {
            score += 30;
            evidence.Add("coinciden ambos extremos de la línea (+30)");
        }
        else if (endpointA || endpointB)
        {
            score += 10;
            evidence.Add("coincide un extremo de la línea (+10)");
        }

        if (HasTaggedElement(spec.SourceLines, "LT", element.NormalizedName))
        {
            score += 10;
            evidence.Add("mención explícita como LT en elementos asociados (+10)");
        }

        if (ContainsWhole(spec.PaddedTitle, element.NormalizedName))
        {
            score += 5;
            evidence.Add("línea presente en el título del proyecto (+5)");
        }

        ApplyGcrScore(element, spec, ref score, evidence);
        ApplyVoltageScore(element, spec, ref score, evidence);

        if (element.Circuits.HasValue &&
            spec.Circuits.Contains(element.Circuits.Value))
        {
            score += 5;
            evidence.Add($"circuitos coincidentes: {element.Circuits.Value}C (+5)");
        }

        if (element.LengthKm.HasValue &&
            spec.LengthsKm.Any(length =>
                Math.Abs(length - element.LengthKm.Value) <=
                Math.Max(3, element.LengthKm.Value * 0.15)))
        {
            score += 5;
            evidence.Add($"longitud compatible: {element.LengthKm.Value:0.##} km (+5)");
        }

        var sameNameCount = catalog.LineNameCounts.GetValueOrDefault(
            BuildNameScopeKey(element.NormalizedName, spec.GcrKey),
            catalog.LineNameCounts.GetValueOrDefault(
                BuildNameScopeKey(element.NormalizedName, string.Empty),
                1));
        if (sameNameCount > 1)
        {
            score -= 10;
            evidence.Add($"nombre de línea repetido: {sameNameCount} candidatos (-10)");
        }

        return new CandidateState(element, score, evidence);
    }

    private void ApplyGcrScore(
        NetworkElement element,
        ProjectSpec spec,
        ref int score,
        ICollection<string> evidence)
    {
        if (string.IsNullOrWhiteSpace(spec.GcrKey))
        {
            return;
        }

        if (element.GcrKeys.Contains(spec.GcrKey))
        {
            score += 15;
            evidence.Add($"geometría dentro de GCR {spec.GcrKey} (+15)");
        }
        else
        {
            score -= 35;
            evidence.Add($"geometría fuera de GCR {spec.GcrKey} (-35)");
        }
    }

    private static void ApplyVoltageScore(
        NetworkElement element,
        ProjectSpec spec,
        ref int score,
        ICollection<string> evidence)
    {
        if (!element.VoltageKv.HasValue || spec.VoltagesKv.Count == 0)
        {
            return;
        }

        if (spec.VoltagesKv.Any(voltage =>
            Math.Abs(voltage - element.VoltageKv.Value) < 0.6))
        {
            score += 10;
            evidence.Add($"tensión coincidente: {element.VoltageKv.Value:0.##} kV (+10)");
        }
        else
        {
            score -= 15;
            evidence.Add($"tensión {element.VoltageKv.Value:0.##} kV no mencionada (-15)");
        }
    }

    private static void ApplySpecificNamePenalty(List<CandidateState> states)
    {
        var substations = states
            .Where(state => state.Element.Type == NetworkElementType.Substation)
            .ToList();

        foreach (var candidate in substations)
        {
            var moreSpecific = substations.FirstOrDefault(other =>
                !ReferenceEquals(candidate, other) &&
                other.Element.NormalizedName.Length > candidate.Element.NormalizedName.Length &&
                other.Element.NormalizedName.StartsWith(
                    candidate.Element.NormalizedName + " ",
                    StringComparison.Ordinal) &&
                other.Score >= candidate.Score);
            if (moreSpecific is null)
            {
                continue;
            }

            candidate.Score -= 20;
            candidate.Evidence.Add(
                $"nombre contenido en coincidencia más específica “{moreSpecific.Element.Name}” (-20)");
        }
    }

    private void ApplyTopologyBonus(List<CandidateState> states)
    {
        var lines = states
            .Where(state =>
                state.Element.Type == NetworkElementType.Line &&
                state.Score >= _options.UmbralRevision)
            .ToList();
        var substations = states
            .Where(state => state.Element.Type == NetworkElementType.Substation)
            .ToList();

        foreach (var line in lines)
        {
            foreach (var substation in substations)
            {
                var isNamedEndpoint =
                    string.Equals(
                        substation.Element.NormalizedName,
                        line.Element.EndpointA,
                        StringComparison.Ordinal) ||
                    string.Equals(
                        substation.Element.NormalizedName,
                        line.Element.EndpointB,
                        StringComparison.Ordinal);
                if (!isNamedEndpoint ||
                    !substation.Element.Latitude.HasValue ||
                    !substation.Element.Longitude.HasValue ||
                    line.Element.LineEndpoints.Count == 0)
                {
                    continue;
                }

                var distance = line.Element.LineEndpoints.Min(endpoint =>
                    HaversineKm(
                        substation.Element.Latitude.Value,
                        substation.Element.Longitude.Value,
                        endpoint.Latitude,
                        endpoint.Longitude));
                if (distance > _options.ToleranciaExtremoKm)
                {
                    continue;
                }

                substation.Score += 15;
                substation.Evidence.Add(
                    $"conectividad: extremo de LT a {distance:0.##} km (+15)");
            }
        }
    }

    private PamRedAssociationCandidate ToDto(CandidateState state)
    {
        var level = state.Score >= _options.UmbralConfianzaAlta
            ? "alta"
            : "revision";
        var element = state.Element;

        return new PamRedAssociationCandidate
        {
            TipoElementoRed = element.Type == NetworkElementType.Line
                ? "linea_transmision"
                : "subestacion",
            ClaveElementoRed = element.Key,
            NombreElementoRed = element.Name,
            TensionKv = element.VoltageKv,
            LongitudKm = element.LengthKm,
            Circuitos = element.Circuits,
            GcrCatalogo = string.Join(", ", element.GcrKeys.OrderBy(value => value)),
            Puntaje = Math.Clamp(state.Score, 0, 100),
            NivelConfianza = level,
            Evidencias = state.Evidence.Distinct(StringComparer.Ordinal).ToList(),
            Geometria = element.Geometry,
            Latitud = element.Latitude,
            Longitud = element.Longitude,
            Fuente = element.Source
        };
    }

    private async Task<NetworkCatalog> ObtenerCatalogoAsync(
        CancellationToken cancellationToken)
    {
        if (_cache.TryGetValue<NetworkCatalog>(CatalogCacheKey, out var cached) &&
            cached is not null)
        {
            return cached;
        }

        await CatalogLock.WaitAsync(cancellationToken);
        try
        {
            if (_cache.TryGetValue<NetworkCatalog>(CatalogCacheKey, out cached) &&
                cached is not null)
            {
                return cached;
            }

            var gcrTask = _httpClient.GetStringAsync(
                _options.GerenciasUrl,
                cancellationToken);
            var substationsTask = _httpClient.GetStringAsync(
                _options.SubestacionesUrl,
                cancellationToken);
            var linesTask = _httpClient.GetStringAsync(
                _options.LineasUrl,
                cancellationToken);

            await Task.WhenAll(gcrTask, substationsTask, linesTask);

            var gcrAreas = ParseGcrAreas(await gcrTask);
            var substations = ParseSubstations(
                await substationsTask,
                gcrAreas);
            var lines = ParseLines(
                await linesTask,
                gcrAreas);

            var catalog = new NetworkCatalog(
                substations,
                lines,
                BuildNameCounts(substations),
                BuildNameCounts(lines));

            _cache.Set(
                CatalogCacheKey,
                catalog,
                TimeSpan.FromMinutes(Math.Max(5, _options.CacheMinutos)));

            _logger.LogInformation(
                "Catálogo de asociación PAM cargado: {Substations} subestaciones y {Lines} líneas.",
                substations.Count,
                lines.Count);
            return catalog;
        }
        finally
        {
            CatalogLock.Release();
        }
    }

    private List<GcrArea> ParseGcrAreas(string json)
    {
        using var document = JsonDocument.Parse(json);
        var features = GetFeatures(document.RootElement);
        return features
            .Select(feature =>
            {
                var properties = GetProperty(feature, "properties");
                var name = GetString(properties, "Field1", "nombre", "name");
                var geometry = GetProperty(feature, "geometry");
                var key = ResolveGcrKey(name);
                if (string.IsNullOrWhiteSpace(key) ||
                    geometry.ValueKind != JsonValueKind.Object)
                {
                    return null;
                }

                var polygons = ParsePolygonShapes(geometry);
                return polygons.Count == 0
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
        IReadOnlyList<GcrArea> areas)
    {
        using var document = JsonDocument.Parse(json);
        var output = new List<NetworkElement>();

        foreach (var feature in GetFeatures(document.RootElement))
        {
            var properties = GetProperty(feature, "properties");
            var geometry = GetProperty(feature, "geometry");
            var name = GetString(properties, "name", "nombre");
            var normalizedName = NormalizeElementName(name, NetworkElementType.Substation);
            var coordinates = GetProperty(geometry, "coordinates");
            if (string.IsNullOrWhiteSpace(normalizedName) ||
                coordinates.ValueKind != JsonValueKind.Array ||
                coordinates.GetArrayLength() < 2 ||
                !coordinates[0].TryGetDouble(out var longitude) ||
                !coordinates[1].TryGetDouble(out var latitude))
            {
                continue;
            }

            var voltage = GetDouble(properties, "voltaje_kv", "voltaje_KV");
            var phase = GetString(properties, "fase");
            var gcrKeys = areas
                .Where(area => AreaContains(area, longitude, latitude))
                .Select(area => area.Key)
                .ToHashSet(StringComparer.Ordinal);
            var geometryClone = geometry.Clone();

            output.Add(new NetworkElement
            {
                Type = NetworkElementType.Substation,
                Key = StableKey(
                    "SE",
                    normalizedName,
                    voltage,
                    geometryClone.GetRawText()),
                Name = name,
                NormalizedName = normalizedName,
                VoltageKv = voltage,
                Phase = phase,
                GcrKeys = gcrKeys,
                Geometry = geometryClone,
                Latitude = latitude,
                Longitude = longitude,
                Source = _options.SubestacionesUrl
            });
        }

        return output;
    }

    private List<NetworkElement> ParseLines(
        string json,
        IReadOnlyList<GcrArea> areas)
    {
        using var document = JsonDocument.Parse(json);
        var output = new List<NetworkElement>();

        foreach (var feature in GetFeatures(document.RootElement))
        {
            var properties = GetProperty(feature, "properties");
            var geometry = GetProperty(feature, "geometry");
            var name = GetString(properties, "nombre_lt", "name", "nombre");
            var normalizedName = NormalizeElementName(name, NetworkElementType.Line);
            if (string.IsNullOrWhiteSpace(normalizedName) ||
                geometry.ValueKind != JsonValueKind.Object)
            {
                continue;
            }

            var voltage = GetDouble(properties, "voltaje_KV", "voltaje_kv");
            var characteristics = GetString(properties, "caracteris", "caracteristicas");
            var circuits = ParseCircuits(characteristics);
            var lengthKm = ParseLengthKm(characteristics);
            var coordinatePairs = ExtractCoordinates(geometry).ToList();
            if (coordinatePairs.Count == 0)
            {
                continue;
            }

            // Cinco puntos representativos son suficientes para identificar la GCR y
            // evitan recorrer los polígonos complejos decenas de veces por cada LT.
            var sampled = SampleCoordinates(coordinatePairs, 5);
            var gcrKeys = areas
                .Where(area => sampled.Any(point =>
                    AreaContains(area, point.Longitude, point.Latitude)))
                .Select(area => area.Key)
                .ToHashSet(StringComparer.Ordinal);
            var (endpointA, endpointB) = ParseLineEndpoints(name);
            var lineEndpoints = ExtractLineEndpoints(geometry);
            var representative = RepresentativePoint(coordinatePairs);
            var geometryClone = geometry.Clone();

            output.Add(new NetworkElement
            {
                Type = NetworkElementType.Line,
                Key = StableKey(
                    "LT",
                    normalizedName,
                    voltage,
                    geometryClone.GetRawText()),
                Name = name,
                NormalizedName = normalizedName,
                VoltageKv = voltage,
                Circuits = circuits,
                LengthKm = lengthKm,
                EndpointA = endpointA,
                EndpointB = endpointB,
                GcrKeys = gcrKeys,
                Geometry = geometryClone,
                Latitude = representative.Latitude,
                Longitude = representative.Longitude,
                LineEndpoints = lineEndpoints,
                Source = _options.LineasUrl
            });
        }

        return output;
    }

    private static Dictionary<string, int> BuildNameCounts(
        IReadOnlyCollection<NetworkElement> elements)
    {
        var counts = new Dictionary<string, int>(StringComparer.Ordinal);
        foreach (var element in elements)
        {
            Increment(BuildNameScopeKey(element.NormalizedName, string.Empty));
            foreach (var gcr in element.GcrKeys)
            {
                Increment(BuildNameScopeKey(element.NormalizedName, gcr));
            }
        }

        return counts;

        void Increment(string key)
        {
            counts[key] = counts.GetValueOrDefault(key) + 1;
        }
    }

    private static string BuildNameScopeKey(string name, string gcr) =>
        $"{gcr}|{name}";

    private static IReadOnlyList<JsonElement> GetFeatures(JsonElement root)
    {
        var features = GetProperty(root, "features");
        return features.ValueKind == JsonValueKind.Array
            ? features.EnumerateArray().Select(item => item.Clone()).ToList()
            : Array.Empty<JsonElement>();
    }

    private static JsonElement GetProperty(
        JsonElement element,
        params string[] names)
    {
        if (element.ValueKind != JsonValueKind.Object)
        {
            return default;
        }

        foreach (var property in element.EnumerateObject())
        {
            if (names.Any(name =>
                string.Equals(
                    property.Name,
                    name,
                    StringComparison.OrdinalIgnoreCase)))
            {
                return property.Value;
            }
        }

        return default;
    }

    private static string GetString(JsonElement element, params string[] names)
    {
        var value = GetProperty(element, names);
        return value.ValueKind switch
        {
            JsonValueKind.String => value.GetString() ?? string.Empty,
            JsonValueKind.Number => value.GetRawText(),
            _ => string.Empty
        };
    }

    private static double? GetDouble(JsonElement element, params string[] names)
    {
        var value = GetProperty(element, names);
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

        return null;
    }

    private static IEnumerable<GeoPoint> ExtractCoordinates(JsonElement geometry)
    {
        var coordinates = GetProperty(geometry, "coordinates");
        return ExtractCoordinateArray(coordinates);
    }

    private static IEnumerable<GeoPoint> ExtractCoordinateArray(JsonElement element)
    {
        if (element.ValueKind != JsonValueKind.Array)
        {
            yield break;
        }

        if (element.GetArrayLength() >= 2 &&
            element[0].ValueKind == JsonValueKind.Number &&
            element[1].ValueKind == JsonValueKind.Number &&
            element[0].TryGetDouble(out var longitude) &&
            element[1].TryGetDouble(out var latitude))
        {
            yield return new GeoPoint(latitude, longitude);
            yield break;
        }

        foreach (var child in element.EnumerateArray())
        {
            foreach (var point in ExtractCoordinateArray(child))
            {
                yield return point;
            }
        }
    }

    private static IReadOnlyList<GeoPoint> SampleCoordinates(
        IReadOnlyList<GeoPoint> points,
        int maximum)
    {
        if (points.Count <= maximum)
        {
            return points;
        }

        var output = new List<GeoPoint>(maximum + 2)
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

    private static IReadOnlyList<GeoPoint> ExtractLineEndpoints(JsonElement geometry)
    {
        var coordinates = GetProperty(geometry, "coordinates");
        var output = new List<GeoPoint>();
        var type = GetString(geometry, "type");

        if (string.Equals(type, "LineString", StringComparison.OrdinalIgnoreCase))
        {
            AddSegmentEndpoints(coordinates, output);
        }
        else if (string.Equals(
            type,
            "MultiLineString",
            StringComparison.OrdinalIgnoreCase) &&
            coordinates.ValueKind == JsonValueKind.Array)
        {
            foreach (var segment in coordinates.EnumerateArray())
            {
                AddSegmentEndpoints(segment, output);
            }
        }

        return output;
    }

    private static void AddSegmentEndpoints(
        JsonElement segment,
        ICollection<GeoPoint> output)
    {
        if (segment.ValueKind != JsonValueKind.Array ||
            segment.GetArrayLength() < 2)
        {
            return;
        }

        AddCoordinate(segment[0], output);
        AddCoordinate(segment[segment.GetArrayLength() - 1], output);
    }

    private static void AddCoordinate(
        JsonElement coordinate,
        ICollection<GeoPoint> output)
    {
        if (coordinate.ValueKind == JsonValueKind.Array &&
            coordinate.GetArrayLength() >= 2 &&
            coordinate[0].TryGetDouble(out var longitude) &&
            coordinate[1].TryGetDouble(out var latitude))
        {
            output.Add(new GeoPoint(latitude, longitude));
        }
    }

    private static GeoPoint RepresentativePoint(IReadOnlyList<GeoPoint> points)
    {
        var minLat = points.Min(point => point.Latitude);
        var maxLat = points.Max(point => point.Latitude);
        var minLon = points.Min(point => point.Longitude);
        var maxLon = points.Max(point => point.Longitude);
        return new GeoPoint((minLat + maxLat) / 2, (minLon + maxLon) / 2);
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
        else if (string.Equals(type, "MultiPolygon", StringComparison.OrdinalIgnoreCase) &&
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
            .Where(ring => ring.Length >= 3)
            .ToArray();
        if (parsedRings.Length == 0)
        {
            return null;
        }

        var exterior = parsedRings[0];
        return new PolygonShape(
            exterior,
            parsedRings.Skip(1).ToArray(),
            exterior.Min(point => point.Longitude),
            exterior.Min(point => point.Latitude),
            exterior.Max(point => point.Longitude),
            exterior.Max(point => point.Latitude));
    }

    private static GeoPoint[] ParseRing(JsonElement ring)
    {
        if (ring.ValueKind != JsonValueKind.Array)
        {
            return Array.Empty<GeoPoint>();
        }

        var output = new List<GeoPoint>(ring.GetArrayLength());
        foreach (var coordinate in ring.EnumerateArray())
        {
            if (TryCoordinate(coordinate, out var longitude, out var latitude))
            {
                output.Add(new GeoPoint(latitude, longitude));
            }
        }

        return output.ToArray();
    }

    private static bool AreaContains(
        GcrArea area,
        double longitude,
        double latitude)
    {
        if (longitude < area.MinLongitude ||
            longitude > area.MaxLongitude ||
            latitude < area.MinLatitude ||
            latitude > area.MaxLatitude)
        {
            return false;
        }

        foreach (var polygon in area.Polygons)
        {
            if (longitude < polygon.MinLongitude ||
                longitude > polygon.MaxLongitude ||
                latitude < polygon.MinLatitude ||
                latitude > polygon.MaxLatitude ||
                !RingContains(polygon.Exterior, longitude, latitude))
            {
                continue;
            }

            if (!polygon.Holes.Any(hole =>
                RingContains(hole, longitude, latitude)))
            {
                return true;
            }
        }

        return false;
    }

    private static bool RingContains(
        IReadOnlyList<GeoPoint> ring,
        double longitude,
        double latitude)
    {
        if (ring.Count < 3)
        {
            return false;
        }

        var inside = false;
        var previous = ring[^1];
        var previousLon = previous.Longitude;
        var previousLat = previous.Latitude;

        foreach (var current in ring)
        {
            var currentLon = current.Longitude;
            var currentLat = current.Latitude;

            var intersects =
                (currentLat > latitude) != (previousLat > latitude) &&
                longitude <
                    (previousLon - currentLon) *
                    (latitude - currentLat) /
                    ((previousLat - currentLat) + double.Epsilon) +
                    currentLon;
            if (intersects)
            {
                inside = !inside;
            }

            previousLon = currentLon;
            previousLat = currentLat;
        }

        return inside;
    }

    private static bool TryCoordinate(
        JsonElement coordinate,
        out double longitude,
        out double latitude)
    {
        longitude = 0;
        latitude = 0;
        return coordinate.ValueKind == JsonValueKind.Array &&
               coordinate.GetArrayLength() >= 2 &&
               coordinate[0].TryGetDouble(out longitude) &&
               coordinate[1].TryGetDouble(out latitude);
    }

    private static (string EndpointA, string EndpointB) ParseLineEndpoints(
        string name)
    {
        var withoutPrefix = LinePrefixRegex().Replace(name ?? string.Empty, string.Empty);
        var parts = LineSeparatorRegex()
            .Split(withoutPrefix)
            .Select(part => NormalizeElementName(part, NetworkElementType.Substation))
            .Where(part => !string.IsNullOrWhiteSpace(part))
            .ToArray();

        return parts.Length >= 2
            ? (parts[0], parts[^1])
            : (string.Empty, string.Empty);
    }

    private static int? ParseCircuits(string text)
    {
        var match = CircuitsRegex().Match(text ?? string.Empty);
        return match.Success &&
               int.TryParse(match.Groups[1].Value, out var circuits)
            ? circuits
            : null;
    }

    private static double? ParseLengthKm(string text)
    {
        var match = LengthRegex().Match(text ?? string.Empty);
        return match.Success &&
               double.TryParse(
                   match.Groups[1].Value.Replace(',', '.'),
                   NumberStyles.Float,
                   CultureInfo.InvariantCulture,
                   out var length)
            ? length
            : null;
    }

    private static bool HasTaggedElement(
        IReadOnlyList<string> lines,
        string tag,
        string normalizedName)
    {
        foreach (var line in lines)
        {
            var normalized = NormalizeText(line);
            var stripped = tag switch
            {
                "SE" => SubstationPrefixNormalizedRegex().Replace(
                    normalized,
                    string.Empty),
                "LT" => LinePrefixNormalizedRegex().Replace(
                    normalized,
                    string.Empty),
                _ => normalized
            };
            if (!string.Equals(stripped, normalized, StringComparison.Ordinal) &&
                ContainsWhole($" {stripped} ", normalizedName))
            {
                return true;
            }
        }

        return false;
    }

    private static string NormalizeElementName(
        string value,
        NetworkElementType type)
    {
        var normalized = NormalizeText(value);
        return type == NetworkElementType.Substation
            ? SubstationPrefixNormalizedRegex().Replace(normalized, string.Empty).Trim()
            : LinePrefixNormalizedRegex().Replace(normalized, string.Empty).Trim();
    }

    private static string NormalizeText(string value)
    {
        if (string.IsNullOrWhiteSpace(value))
        {
            return string.Empty;
        }

        var decomposed = value.Normalize(NormalizationForm.FormD);
        var builder = new StringBuilder(decomposed.Length);
        foreach (var character in decomposed)
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

        return WhitespaceRegex().Replace(builder.ToString(), " ").Trim();
    }

    private static bool ContainsWhole(string paddedText, string normalizedName) =>
        !string.IsNullOrWhiteSpace(normalizedName) &&
        paddedText.Contains(
            $" {normalizedName} ",
            StringComparison.Ordinal);

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
            ["BS"] = "bcsur",
            ["NO"] = "noroeste",
            ["NE"] = "noreste",
            ["NT"] = "norte",
            ["PE"] = "peninsular",
            ["OR"] = "oriental",
            ["OC"] = "occidental",
            ["CE"] = "central",
            ["VM"] = "central",
            ["SE"] = "oriental"
        };
        if (aliases.TryGetValue(normalized.TrimEnd('…', '.'), out var alias))
        {
            return alias;
        }

        if (normalized.Contains("BAJACALIFORNIASUR", StringComparison.Ordinal))
        {
            return "bcsur";
        }
        if (normalized.Contains("MULEGE", StringComparison.Ordinal))
        {
            return "mulege";
        }
        if (normalized.Contains("BAJACALIFORNIA", StringComparison.Ordinal))
        {
            return "bcalifornia";
        }

        var names = new[]
        {
            "noroeste", "noreste", "norte", "peninsular",
            "oriental", "occidental", "central"
        };
        return names.FirstOrDefault(name =>
            normalized.Contains(
                name.ToUpperInvariant(),
                StringComparison.Ordinal)) ?? string.Empty;
    }

    private static string ResolveProjectCodeGcr(string projectKey)
    {
        var match = ProjectGcrCodeRegex().Match(projectKey ?? string.Empty);
        return match.Success ? ResolveGcrKey(match.Groups[1].Value) : string.Empty;
    }

    private static string StableKey(
        string prefix,
        string normalizedName,
        double? voltage,
        string geometry)
    {
        var raw = $"{prefix}|{normalizedName}|{voltage:0.###}|{geometry}";
        var hash = Convert.ToHexString(
            SHA256.HashData(Encoding.UTF8.GetBytes(raw)));
        return $"{prefix.ToLowerInvariant()}:{hash[..20].ToLowerInvariant()}";
    }

    private static double HaversineKm(
        double latitudeA,
        double longitudeA,
        double latitudeB,
        double longitudeB)
    {
        const double earthRadiusKm = 6371.0088;
        var lat1 = DegreesToRadians(latitudeA);
        var lat2 = DegreesToRadians(latitudeB);
        var deltaLat = DegreesToRadians(latitudeB - latitudeA);
        var deltaLon = DegreesToRadians(longitudeB - longitudeA);
        var a =
            Math.Sin(deltaLat / 2) * Math.Sin(deltaLat / 2) +
            Math.Cos(lat1) * Math.Cos(lat2) *
            Math.Sin(deltaLon / 2) * Math.Sin(deltaLon / 2);
        return earthRadiusKm * 2 * Math.Atan2(Math.Sqrt(a), Math.Sqrt(1 - a));
    }

    private static double DegreesToRadians(double degrees) =>
        degrees * Math.PI / 180;

    private static string GetGeometryType(JsonElement geometry) =>
        GetString(geometry, "type");

    private sealed class ProjectSpec
    {
        public required string PaddedText { get; init; }
        public required string PaddedTitle { get; init; }
        public required IReadOnlyList<string> SourceLines { get; init; }
        public required string GcrKey { get; init; }
        public required HashSet<double> VoltagesKv { get; init; }
        public required HashSet<int> Circuits { get; init; }
        public required IReadOnlyList<double> LengthsKm { get; init; }

        public static ProjectSpec Create(PamTerritorialProyecto project)
        {
            var source = string.Join(
                Environment.NewLine,
                new[]
                {
                    project.NombreProyecto,
                    project.ZonaAtendida,
                    project.ElementosEquiposAsociados
                });
            var normalized = NormalizeText(source);
            var title = NormalizeText(project.NombreProyecto);
            var gcr = ResolveGcrKey(project.Gcr);
            if (string.IsNullOrWhiteSpace(gcr))
            {
                gcr = ResolveProjectCodeGcr(project.ClaveProyecto);
            }

            return new ProjectSpec
            {
                PaddedText = $" {normalized} ",
                PaddedTitle = $" {title} ",
                SourceLines = (project.ElementosEquiposAsociados ?? string.Empty)
                    .Split(
                        new[] { "\r\n", "\n", "\r" },
                        StringSplitOptions.RemoveEmptyEntries | StringSplitOptions.TrimEntries),
                GcrKey = gcr,
                VoltagesKv = VoltageRegex()
                    .Matches(source)
                    .Select(match => ParseInvariant(match.Groups[1].Value))
                    .Where(value => value.HasValue)
                    .Select(value => value!.Value)
                    .ToHashSet(),
                Circuits = ProjectCircuitsRegex()
                    .Matches(source)
                    .Select(match =>
                        int.TryParse(match.Groups[1].Value, out var value)
                            ? (int?)value
                            : null)
                    .Where(value => value.HasValue)
                    .Select(value => value!.Value)
                    .ToHashSet(),
                LengthsKm = ProjectLengthRegex()
                    .Matches(source)
                    .Select(match => ParseInvariant(match.Groups[1].Value))
                    .Where(value => value.HasValue)
                    .Select(value => value!.Value)
                    .ToList()
            };
        }

        private static double? ParseInvariant(string value) =>
            double.TryParse(
                value.Replace(',', '.'),
                NumberStyles.Float,
                CultureInfo.InvariantCulture,
                out var parsed)
                ? parsed
                : null;
    }

    private sealed class CandidateState
    {
        public CandidateState(
            NetworkElement element,
            int score,
            List<string> evidence)
        {
            Element = element;
            Score = score;
            Evidence = evidence;
        }

        public NetworkElement Element { get; }
        public int Score { get; set; }
        public List<string> Evidence { get; }
    }

    private sealed class NetworkElement
    {
        public NetworkElementType Type { get; init; }
        public string Key { get; init; } = string.Empty;
        public string Name { get; init; } = string.Empty;
        public string NormalizedName { get; init; } = string.Empty;
        public double? VoltageKv { get; init; }
        public string Phase { get; init; } = string.Empty;
        public int? Circuits { get; init; }
        public double? LengthKm { get; init; }
        public string EndpointA { get; init; } = string.Empty;
        public string EndpointB { get; init; } = string.Empty;
        public HashSet<string> GcrKeys { get; init; } = new(StringComparer.Ordinal);
        public required JsonElement Geometry { get; init; }
        public double? Latitude { get; init; }
        public double? Longitude { get; init; }
        public IReadOnlyList<GeoPoint> LineEndpoints { get; init; } =
            Array.Empty<GeoPoint>();
        public string Source { get; init; } = string.Empty;
    }

    private sealed record NetworkCatalog(
        IReadOnlyList<NetworkElement> Substations,
        IReadOnlyList<NetworkElement> Lines,
        IReadOnlyDictionary<string, int> SubstationNameCounts,
        IReadOnlyDictionary<string, int> LineNameCounts);

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
    private readonly record struct GeoPoint(double Latitude, double Longitude);

    private enum NetworkElementType
    {
        Substation,
        Line
    }

    [GeneratedRegex(@"\s+")]
    private static partial Regex WhitespaceRegex();

    [GeneratedRegex(
        @"^(?:SUBESTACION(?:\s+ELECTRICA)?|S\s*E|SE)\s+",
        RegexOptions.IgnoreCase)]
    private static partial Regex SubstationPrefixNormalizedRegex();

    [GeneratedRegex(
        @"^(?:LINEA(?:\s+DE\s+TRANSMISION)?|L\s*T|LT)\s+",
        RegexOptions.IgnoreCase)]
    private static partial Regex LinePrefixNormalizedRegex();

    [GeneratedRegex(
        @"^\s*(?:L\s*\.\s*T\s*\.?|LT|LINEA(?:\s+DE\s+TRANSMISION)?)\s*",
        RegexOptions.IgnoreCase)]
    private static partial Regex LinePrefixRegex();

    [GeneratedRegex(@"\s*[-–—]\s*")]
    private static partial Regex LineSeparatorRegex();

    [GeneratedRegex(@"(?<!\d)(\d+)\s*C(?:\b|-)", RegexOptions.IgnoreCase)]
    private static partial Regex CircuitsRegex();

    [GeneratedRegex(@"(\d+(?:[.,]\d+)?)\s*KM", RegexOptions.IgnoreCase)]
    private static partial Regex LengthRegex();

    [GeneratedRegex(
        @"(?<!\d)(\d{2,3}(?:[.,]\d+)?)\s*K\s*V\b",
        RegexOptions.IgnoreCase)]
    private static partial Regex VoltageRegex();

    [GeneratedRegex(@"(?<!\d)(\d+)\s*C(?:\b|-)", RegexOptions.IgnoreCase)]
    private static partial Regex ProjectCircuitsRegex();

    [GeneratedRegex(
        @"(\d+(?:[.,]\d+)?)\s*KM(?:\s*-\s*C)?",
        RegexOptions.IgnoreCase)]
    private static partial Regex ProjectLengthRegex();

    [GeneratedRegex(
        @"(?:^|[-\s])(BC|BS|NO|NE|NT|PE|OR|OC|CE|VM|SE)\d",
        RegexOptions.IgnoreCase)]
    private static partial Regex ProjectGcrCodeRegex();
}
