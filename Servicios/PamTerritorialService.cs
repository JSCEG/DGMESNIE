using Dapper;
using Microsoft.Data.SqlClient;
using NSIE.Models;
using System.Text.Json;

namespace NSIE.Servicios;

public interface IPamTerritorialService
{
    Task<IReadOnlyList<PamTerritorialProyecto>> BuscarAsync(
        string? busqueda,
        int limite,
        CancellationToken cancellationToken);

    Task<PamTerritorialProyecto?> ObtenerAsync(
        long proyectoId,
        CancellationToken cancellationToken);

    Task<PamTerritorialGeoJson> ObtenerGeoJsonAsync(
        CancellationToken cancellationToken);
}

public sealed class PamTerritorialService : IPamTerritorialService
{
    private readonly string _connectionString;
    private readonly IPamRedAssociationService _redAssociationService;
    private readonly ILogger<PamTerritorialService> _logger;

    public PamTerritorialService(
        IConfiguration configuration,
        IPamRedAssociationService redAssociationService,
        ILogger<PamTerritorialService> logger)
    {
        _connectionString = configuration.GetConnectionString("DefaultConnection")
            ?? throw new InvalidOperationException("DefaultConnection no está configurada.");
        _redAssociationService = redAssociationService;
        _logger = logger;
    }

    public Task<IReadOnlyList<PamTerritorialProyecto>> BuscarAsync(
        string? busqueda,
        int limite,
        CancellationToken cancellationToken)
    {
        var normalizedSearch = string.IsNullOrWhiteSpace(busqueda)
            ? null
            : busqueda.Trim();
        var (searchLatitude, searchLongitude) = ParseCoordinates(normalizedSearch);

        return ConsultarAsync(
            normalizedSearch,
            Math.Clamp(limite, 1, 500),
            null,
            searchLatitude,
            searchLongitude,
            cancellationToken);
    }

    public async Task<PamTerritorialProyecto?> ObtenerAsync(
        long proyectoId,
        CancellationToken cancellationToken)
    {
        if (proyectoId <= 0)
        {
            return null;
        }

        var projects = await ConsultarAsync(
            null,
            1,
            proyectoId,
            null,
            null,
            cancellationToken);
        var project = projects.FirstOrDefault();
        if (project is null || project.TieneUbicacionValidada)
        {
            return project;
        }

        try
        {
            var association = (await _redAssociationService.ResolverAsync(
                new[] { project },
                cancellationToken)).FirstOrDefault();
            if (association is null)
            {
                return project;
            }

            var suggested = _redAssociationService.CrearUbicaciones(
                association,
                soloConfianzaAlta: false);
            return suggested.Count == 0
                ? project
                : WithLocations(
                    project,
                    project.Ubicaciones.Concat(suggested).ToList());
        }
        catch (Exception ex) when (
            ex is HttpRequestException or
            JsonException or
            TaskCanceledException)
        {
            _logger.LogWarning(
                ex,
                "No fue posible resolver las asociaciones de red del proyecto PAM {ProyectoId}; se conserva la cobertura GCR.",
                project.ProyectoId);
            return project;
        }
    }

    public async Task<PamTerritorialGeoJson> ObtenerGeoJsonAsync(
        CancellationToken cancellationToken)
    {
        var baseProjects = await ConsultarAsync(
            null,
            500,
            null,
            null,
            null,
            cancellationToken);

        var projects = baseProjects.ToList();
        var associationLocations = new Dictionary<long, IReadOnlyList<PamTerritorialUbicacion>>();
        var projectsWithoutValidatedLocation = projects
            .Where(project => !project.TieneUbicacionValidada)
            .ToList();

        if (projectsWithoutValidatedLocation.Count > 0)
        {
            try
            {
                var associations = await _redAssociationService.ResolverAsync(
                    projectsWithoutValidatedLocation,
                    cancellationToken);
                associationLocations = associations.ToDictionary(
                    result => result.ProyectoId,
                    result => _redAssociationService.CrearUbicaciones(
                        result,
                        soloConfianzaAlta: true));
            }
            catch (Exception ex) when (
                ex is HttpRequestException or
                JsonException or
                TaskCanceledException)
            {
                _logger.LogWarning(
                    ex,
                    "No fue posible enriquecer el GeoJSON PAM con el catálogo de red; se publican sólo ubicaciones validadas.");
            }
        }

        var resolvedProjects = projects
            .Select(project =>
                project.TieneUbicacionValidada ||
                !associationLocations.TryGetValue(project.ProyectoId, out var suggested) ||
                suggested.Count == 0
                    ? project
                    : WithLocations(
                        project,
                        project.Ubicaciones.Concat(suggested).ToList()))
            .ToList();

        var features = resolvedProjects
            .SelectMany(project => project.Ubicaciones
                .Where(location =>
                    location.Geometria.HasValue &&
                    (location.Validada ||
                     (location.EsAsociacionSugerida &&
                      string.Equals(
                          location.NivelConfianza,
                          "alta",
                          StringComparison.Ordinal))))
                .Select(location => new PamTerritorialFeature
                {
                    Geometry = location.Geometria!.Value,
                    Properties = new PamTerritorialFeatureProperties
                    {
                        ProyectoId = project.ProyectoId,
                        UbicacionId = location.UbicacionId,
                        ClaveProyecto = project.ClaveProyecto,
                        NombreProyecto = project.NombreProyecto,
                        Gcr = project.Gcr,
                        TipoProyecto = project.TipoProyecto,
                        EtapaProyecto = project.EtapaProyecto,
                        EstatusLicitacion = project.EstatusLicitacion,
                        ZonaAtendida = project.ZonaAtendida,
                        ElementosEquiposAsociados = project.ElementosEquiposAsociados,
                        EtiquetaUbicacion = location.Etiqueta,
                        TipoGeometria = location.TipoGeometria,
                        PrecisionUbicacion = location.PrecisionUbicacion,
                        MetodoUbicacion = location.MetodoUbicacion,
                        Direccion = location.Direccion,
                        Entidad = location.Entidad,
                        Municipio = location.Municipio,
                        Fuente = location.Fuente,
                        FechaCorte = location.FechaCorte ?? project.FechaCorte,
                        RadioSugeridoKm = location.RadioSugeridoKm,
                        Latitud = location.Latitud,
                        Longitud = location.Longitud,
                        FichaUrl = project.FichaUrl,
                        Validada = location.Validada,
                        EsAsociacionSugerida = location.EsAsociacionSugerida,
                        PuntajeCoincidencia = location.PuntajeCoincidencia,
                        NivelConfianza = location.NivelConfianza,
                        TipoElementoRed = location.TipoElementoRed,
                        ClaveElementoRed = location.ClaveElementoRed,
                        Evidencias = location.Evidencias
                    }
                }))
            .ToList();

        var validatedProjects = projects.Count(project => project.TieneUbicacionValidada);
        var associatedProjectIds = features
            .Where(feature => feature.Properties.EsAsociacionSugerida)
            .Select(feature => feature.Properties.ProyectoId)
            .Distinct()
            .ToHashSet();

        return new PamTerritorialGeoJson
        {
            Meta = new PamTerritorialGeoJsonMeta
            {
                CatalogoProyectos = projects.Count,
                Proyectos = features
                    .Select(feature => feature.Properties.ProyectoId)
                    .Distinct()
                    .Count(),
                Ubicaciones = features.Count,
                ProyectosValidados = validatedProjects,
                ProyectosAsociadosRed = associatedProjectIds.Count,
                AsociacionesSugeridas = features.Count(feature =>
                    feature.Properties.EsAsociacionSugerida)
            },
            Features = features
        };
    }

    private static PamTerritorialProyecto WithLocations(
        PamTerritorialProyecto project,
        IReadOnlyList<PamTerritorialUbicacion> locations)
    {
        return new PamTerritorialProyecto
        {
            ProyectoId = project.ProyectoId,
            ClaveProyecto = project.ClaveProyecto,
            ClavesAlternas = project.ClavesAlternas,
            NombreProyecto = project.NombreProyecto,
            Gcr = project.Gcr,
            TipoProyecto = project.TipoProyecto,
            EtapaProyecto = project.EtapaProyecto,
            EstadoVigenciaCartera = project.EstadoVigenciaCartera,
            EstatusLicitacion = project.EstatusLicitacion,
            ZonaAtendida = project.ZonaAtendida,
            ElementosEquiposAsociados = project.ElementosEquiposAsociados,
            FuenteDocumento = project.FuenteDocumento,
            FechaCorte = project.FechaCorte,
            Ubicaciones = locations
        };
    }

    private async Task<IReadOnlyList<PamTerritorialProyecto>> ConsultarAsync(
        string? busqueda,
        int limite,
        long? proyectoId,
        double? searchLatitude,
        double? searchLongitude,
        CancellationToken cancellationToken)
    {
        const string sql = """
SELECT TOP (@Limite)
    v.ProyectoId,
    COALESCE(v.ClaveProyecto, N'') AS ClaveProyecto,
    COALESCE(claves.ClavesAlternas, N'') AS ClavesAlternas,
    COALESCE(v.NombreProyecto, N'') AS NombreProyecto,
    COALESCE(v.GRT, N'') AS Gcr,
    COALESCE(v.TipoProyecto, N'') AS TipoProyecto,
    COALESCE(v.EtapaProyecto, N'') AS EtapaProyecto,
    COALESCE(v.EstadoVigenciaCartera, N'') AS EstadoVigenciaCartera,
    COALESCE(v.EstatusLicitacion, N'') AS EstatusLicitacion,
    COALESCE(v.ZonaAtendida, N'') AS ZonaAtendida,
    COALESCE(v.ElementosEquiposAsociados, N'') AS ElementosEquiposAsociados,
    COALESCE(v.FuenteDocumento, N'') AS FuenteDocumento,
    v.FechaCorte
FROM dgmesnie.vw_PAMProyectoVigente v
OUTER APPLY
(
    SELECT STRING_AGG(CONVERT(NVARCHAR(MAX), clave.ClaveProyecto), N' · ') AS ClavesAlternas
    FROM dgmesnie.PAMProyectoClaveVersion clave
    WHERE clave.ProyectoId = v.ProyectoId AND clave.EsVigente = 1
) claves
WHERE (@ProyectoId IS NULL OR v.ProyectoId = @ProyectoId)
  AND v.EstadoVigenciaCartera = N'Vigente'
  AND
  (
      @Busqueda IS NULL
      OR v.ClaveProyecto LIKE @BusquedaLike
      OR v.NombreProyecto LIKE @BusquedaLike
      OR v.GRT LIKE @BusquedaLike
      OR v.ZonaAtendida LIKE @BusquedaLike
      OR v.ElementosEquiposAsociados LIKE @BusquedaLike
      OR claves.ClavesAlternas LIKE @BusquedaLike
      OR EXISTS
      (
          SELECT 1
          FROM dgmesnie.PAMProyectoUbicacion ubicacion
          WHERE ubicacion.ProyectoId = v.ProyectoId
            AND ubicacion.Activa = 1
            AND
            (
                ubicacion.Direccion LIKE @BusquedaLike
                OR ubicacion.Entidad LIKE @BusquedaLike
                OR ubicacion.Municipio LIKE @BusquedaLike
                OR ubicacion.Localidad LIKE @BusquedaLike
                OR ubicacion.Etiqueta LIKE @BusquedaLike
                OR
                (
                    @BusquedaLatitud IS NOT NULL
                    AND @BusquedaLongitud IS NOT NULL
                    AND ubicacion.Latitud IS NOT NULL
                    AND ubicacion.Longitud IS NOT NULL
                    AND geography::Point(
                            COALESCE(TRY_CONVERT(float, ubicacion.Latitud), 0),
                            COALESCE(TRY_CONVERT(float, ubicacion.Longitud), 0),
                            4326)
                        .STDistance(
                            geography::Point(
                                COALESCE(@BusquedaLatitud, 0),
                                COALESCE(@BusquedaLongitud, 0),
                                4326)) <= 5000
                )
            )
      )
  )
ORDER BY
    CASE WHEN NULLIF(LTRIM(RTRIM(v.ClaveProyecto)), N'') IS NULL THEN 1 ELSE 0 END,
    v.ClaveProyecto,
    v.NombreProyecto;

IF OBJECT_ID(N'dgmesnie.PAMProyectoUbicacion', N'U') IS NOT NULL
BEGIN
    SELECT
        u.UbicacionId,
        u.ProyectoId,
        COALESCE(u.Etiqueta, N'') AS Etiqueta,
        u.TipoGeometria,
        u.GeometriaJson,
        TRY_CONVERT(float, u.Latitud) AS Latitud,
        TRY_CONVERT(float, u.Longitud) AS Longitud,
        COALESCE(u.Direccion, N'') AS Direccion,
        COALESCE(u.Entidad, N'') AS Entidad,
        COALESCE(u.Municipio, N'') AS Municipio,
        COALESCE(u.Localidad, N'') AS Localidad,
        COALESCE(u.PrecisionUbicacion, N'') AS PrecisionUbicacion,
        COALESCE(u.MetodoUbicacion, N'') AS MetodoUbicacion,
        COALESCE(u.Fuente, N'') AS Fuente,
        u.FechaCorte,
        TRY_CONVERT(float, u.RadioSugeridoKm) AS RadioSugeridoKm,
        u.Orden,
        u.EsPrincipal,
        u.Validada
    FROM dgmesnie.PAMProyectoUbicacion u
    WHERE u.Activa = 1
      AND (@ProyectoId IS NULL OR u.ProyectoId = @ProyectoId)
    ORDER BY u.ProyectoId, u.EsPrincipal DESC, u.Orden, u.UbicacionId;
END
ELSE
BEGIN
    SELECT
        CAST(NULL AS BIGINT) AS UbicacionId,
        CAST(NULL AS BIGINT) AS ProyectoId,
        CAST(NULL AS NVARCHAR(300)) AS Etiqueta,
        CAST(NULL AS NVARCHAR(30)) AS TipoGeometria,
        CAST(NULL AS NVARCHAR(MAX)) AS GeometriaJson,
        CAST(NULL AS FLOAT) AS Latitud,
        CAST(NULL AS FLOAT) AS Longitud,
        CAST(NULL AS NVARCHAR(1000)) AS Direccion,
        CAST(NULL AS NVARCHAR(200)) AS Entidad,
        CAST(NULL AS NVARCHAR(300)) AS Municipio,
        CAST(NULL AS NVARCHAR(300)) AS Localidad,
        CAST(NULL AS NVARCHAR(40)) AS PrecisionUbicacion,
        CAST(NULL AS NVARCHAR(80)) AS MetodoUbicacion,
        CAST(NULL AS NVARCHAR(500)) AS Fuente,
        CAST(NULL AS DATE) AS FechaCorte,
        CAST(NULL AS FLOAT) AS RadioSugeridoKm,
        CAST(NULL AS INT) AS Orden,
        CAST(NULL AS BIT) AS EsPrincipal,
        CAST(NULL AS BIT) AS Validada
    WHERE 1 = 0;
END;
""";

        var command = new CommandDefinition(
            sql,
            new
            {
                Busqueda = busqueda,
                BusquedaLike = busqueda is null ? null : $"%{busqueda}%",
                Limite = limite,
                ProyectoId = proyectoId,
                BusquedaLatitud = searchLatitude,
                BusquedaLongitud = searchLongitude
            },
            cancellationToken: cancellationToken);

        await using var connection = new SqlConnection(_connectionString);
        await connection.OpenAsync(cancellationToken);
        using var multi = await connection.QueryMultipleAsync(command);
        var projectRows = (await multi.ReadAsync<PamProjectRow>()).ToList();
        var locationRows = (await multi.ReadAsync<PamLocationRow>()).ToList();
        var locationsByProject = locationRows
            .GroupBy(row => row.ProyectoId)
            .ToDictionary(
                group => group.Key,
                group => (IReadOnlyList<PamTerritorialUbicacion>)group
                    .Select(MapLocation)
                    .Where(location => location is not null)
                    .Cast<PamTerritorialUbicacion>()
                    .ToList());

        return projectRows
            .Select(row => new PamTerritorialProyecto
            {
                ProyectoId = row.ProyectoId,
                ClaveProyecto = row.ClaveProyecto,
                ClavesAlternas = row.ClavesAlternas,
                NombreProyecto = row.NombreProyecto,
                Gcr = row.Gcr,
                TipoProyecto = row.TipoProyecto,
                EtapaProyecto = row.EtapaProyecto,
                EstadoVigenciaCartera = row.EstadoVigenciaCartera,
                EstatusLicitacion = row.EstatusLicitacion,
                ZonaAtendida = row.ZonaAtendida,
                ElementosEquiposAsociados = row.ElementosEquiposAsociados,
                FuenteDocumento = row.FuenteDocumento,
                FechaCorte = row.FechaCorte,
                Ubicaciones = locationsByProject.GetValueOrDefault(
                    row.ProyectoId,
                    Array.Empty<PamTerritorialUbicacion>())
            })
            .ToList();
    }

    private static (double? Latitude, double? Longitude) ParseCoordinates(
        string? search)
    {
        if (string.IsNullOrWhiteSpace(search))
        {
            return (null, null);
        }

        var parts = search
            .Replace(';', ',')
            .Split(',', StringSplitOptions.TrimEntries | StringSplitOptions.RemoveEmptyEntries);
        if (parts.Length != 2 ||
            !double.TryParse(
                parts[0],
                System.Globalization.NumberStyles.Float,
                System.Globalization.CultureInfo.InvariantCulture,
                out var latitude) ||
            !double.TryParse(
                parts[1],
                System.Globalization.NumberStyles.Float,
                System.Globalization.CultureInfo.InvariantCulture,
                out var longitude) ||
            latitude is < 14 or > 33.5 ||
            longitude is < -118 or > -86)
        {
            return (null, null);
        }

        return (latitude, longitude);
    }

    private static PamTerritorialUbicacion? MapLocation(PamLocationRow row)
    {
        if (row.UbicacionId <= 0 ||
            row.ProyectoId <= 0 ||
            string.IsNullOrWhiteSpace(row.GeometriaJson))
        {
            return null;
        }

        JsonElement geometry;
        try
        {
            using var document = JsonDocument.Parse(row.GeometriaJson);
            geometry = document.RootElement.Clone();
        }
        catch (JsonException)
        {
            return null;
        }

        var (lat, lon) = ResolveRepresentativePoint(
            geometry,
            row.Latitud,
            row.Longitud);
        var suggestedRadius = row.RadioSugeridoKm is > 0
            ? row.RadioSugeridoKm.Value
            : DefaultRadius(row.TipoGeometria);

        return new PamTerritorialUbicacion
        {
            UbicacionId = row.UbicacionId,
            ProyectoId = row.ProyectoId,
            Etiqueta = row.Etiqueta,
            TipoGeometria = row.TipoGeometria,
            Geometria = geometry,
            Latitud = lat,
            Longitud = lon,
            Direccion = row.Direccion,
            Entidad = row.Entidad,
            Municipio = row.Municipio,
            Localidad = row.Localidad,
            PrecisionUbicacion = row.PrecisionUbicacion,
            MetodoUbicacion = row.MetodoUbicacion,
            Fuente = row.Fuente,
            FechaCorte = row.FechaCorte,
            RadioSugeridoKm = suggestedRadius,
            Orden = row.Orden,
            EsPrincipal = row.EsPrincipal,
            Validada = row.Validada
        };
    }

    private static (double? Latitude, double? Longitude) ResolveRepresentativePoint(
        JsonElement geometry,
        double? latitude,
        double? longitude)
    {
        if (latitude is >= 14 and <= 33.5 &&
            longitude is >= -118 and <= -86)
        {
            return (latitude, longitude);
        }

        if (!geometry.TryGetProperty("coordinates", out var coordinates))
        {
            return (null, null);
        }

        var bounds = new GeometryBounds();
        AccumulateCoordinates(coordinates, bounds);
        return bounds.HasValue
            ? ((bounds.MinLat + bounds.MaxLat) / 2, (bounds.MinLon + bounds.MaxLon) / 2)
            : (null, null);
    }

    private static void AccumulateCoordinates(
        JsonElement element,
        GeometryBounds bounds)
    {
        if (element.ValueKind != JsonValueKind.Array)
        {
            return;
        }

        if (element.GetArrayLength() >= 2 &&
            element[0].ValueKind == JsonValueKind.Number &&
            element[1].ValueKind == JsonValueKind.Number &&
            element[0].TryGetDouble(out var lon) &&
            element[1].TryGetDouble(out var lat))
        {
            if (lat is >= 14 and <= 33.5 && lon is >= -118 and <= -86)
            {
                bounds.Add(lat, lon);
            }
            return;
        }

        foreach (var child in element.EnumerateArray())
        {
            AccumulateCoordinates(child, bounds);
        }
    }

    private static double DefaultRadius(string geometryType)
    {
        return geometryType switch
        {
            "LineString" or "MultiLineString" => 10,
            "Polygon" or "MultiPolygon" => 0,
            "MultiPoint" => 10,
            _ => 20
        };
    }

    private sealed class GeometryBounds
    {
        public double MinLat { get; private set; } = double.PositiveInfinity;
        public double MinLon { get; private set; } = double.PositiveInfinity;
        public double MaxLat { get; private set; } = double.NegativeInfinity;
        public double MaxLon { get; private set; } = double.NegativeInfinity;
        public bool HasValue => double.IsFinite(MinLat) && double.IsFinite(MinLon);

        public void Add(double latitude, double longitude)
        {
            MinLat = Math.Min(MinLat, latitude);
            MinLon = Math.Min(MinLon, longitude);
            MaxLat = Math.Max(MaxLat, latitude);
            MaxLon = Math.Max(MaxLon, longitude);
        }
    }

    private sealed class PamProjectRow
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
    }

    private sealed class PamLocationRow
    {
        public long UbicacionId { get; init; }
        public long ProyectoId { get; init; }
        public string Etiqueta { get; init; } = string.Empty;
        public string TipoGeometria { get; init; } = string.Empty;
        public string GeometriaJson { get; init; } = string.Empty;
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
        public double? RadioSugeridoKm { get; init; }
        public int Orden { get; init; }
        public bool EsPrincipal { get; init; }
        public bool Validada { get; init; }
    }
}
