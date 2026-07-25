using Dapper;
using Microsoft.Data.SqlClient;
using NSIE.Models;

namespace NSIE.Servicios;

public interface IRepositorioPermisosEnergeticos
{
    Task<PermisosEnergeticosDatos?> ObtenerAsync(
        string tipo,
        double minLat,
        double minLon,
        double maxLat,
        double maxLon,
        CancellationToken cancellationToken);
}

public interface IServicioPermisosEnergeticos
{
    IReadOnlyCollection<string> TiposSoportados { get; }

    Task<PermisosEnergeticosGeoJson?> ObtenerGeoJsonAsync(
        string tipo,
        double minLat,
        double minLon,
        double maxLat,
        double maxLon,
        CancellationToken cancellationToken);
}

public sealed class RepositorioPermisosEnergeticos : IRepositorioPermisosEnergeticos
{
    private const string BboxWhere = @"
WHERE LatitudGeo BETWEEN @MinLat AND @MaxLat
  AND LongitudGeo BETWEEN @MinLon AND @MaxLon
  AND LatitudGeo BETWEEN 14 AND 33.5
  AND LongitudGeo BETWEEN -118 AND -86";

    private readonly string _connectionString;

    public RepositorioPermisosEnergeticos(IConfiguration configuration)
    {
        _connectionString = configuration.GetConnectionString("DefaultConnection")
            ?? throw new InvalidOperationException("DefaultConnection no está configurada.");
    }

    public async Task<PermisosEnergeticosDatos?> ObtenerAsync(
        string tipo,
        double minLat,
        double minLon,
        double maxLat,
        double maxLon,
        CancellationToken cancellationToken)
    {
        var definition = ObtenerDefinicion(tipo);
        if (definition is null)
        {
            return null;
        }

        var parameters = new
        {
            MinLat = minLat,
            MinLon = minLon,
            MaxLat = maxLat,
            MaxLon = maxLon
        };

        var command = new CommandDefinition(
            definition.Query + Environment.NewLine + definition.CutoffQuery,
            parameters,
            cancellationToken: cancellationToken);

        await using var connection = new SqlConnection(_connectionString);
        await connection.OpenAsync(cancellationToken);
        using var multi = await connection.QueryMultipleAsync(command);
        var rows = (await multi.ReadAsync<PermisoEnergeticoRegistro>()).ToList();
        var cutoff = await multi.ReadFirstOrDefaultAsync<DateTime?>();

        return new PermisosEnergeticosDatos
        {
            Tipo = definition.Tipo,
            Mercado = definition.Mercado,
            Fuente = definition.Fuente,
            FechaCorte = cutoff,
            Registros = rows
        };
    }

    private static PermisoQueryDefinition? ObtenerDefinicion(string tipo)
    {
        return tipo switch
        {
            "electricidad" => new PermisoQueryDefinition(
                "electricidad",
                "Electricidad",
                "CRE · Inventario de permisos eléctricos DGMESNIE",
                @"
SELECT TOP (25000)
    COALESCE(NULLIF(LTRIM(RTRIM(RazonSocial)), ''), NULLIF(LTRIM(RTRIM(NumeroPermiso)), ''), 'Permiso eléctrico') AS Nombre,
    COALESCE(NumeroPermiso, '') AS NumeroPermiso,
    COALESCE(EfId, '') AS Entidad,
    COALESCE(MpoId, '') AS Municipio,
    COALESCE(Estatus, '') AS Estatus,
    COALESCE(TipoPermiso, '') AS TipoPermiso,
    COALESCE([Tecnología], '') AS Tecnologia,
    COALESCE([Clasifica_Menú], '') AS Clasificacion,
    TRY_CONVERT(float, CapacidadAutorizadaMW) AS Capacidad,
    'MW' AS UnidadCapacidad,
    '' AS CapacidadTexto,
    FechaOtorgamiento,
    InicioOperaciones AS FechaOperacion,
    LatitudGeo AS Latitud,
    LongitudGeo AS Longitud
FROM dbo.vElectricidad_autorizado_mapa
" + BboxWhere + @"
ORDER BY FechaOtorgamiento DESC, NumeroPermiso;",
                "SELECT MAX(FechaOtorgamiento) AS FechaCorte FROM dbo.vElectricidad_autorizado_mapa;"),

            "gas-natural" => new PermisoQueryDefinition(
                "gas-natural",
                "Hidrocarburos · Gas natural",
                "CRE · Inventario de permisos de gas natural DGMESNIE",
                @"
SELECT TOP (25000)
    COALESCE(NULLIF(LTRIM(RTRIM(RazonSocial)), ''), NULLIF(LTRIM(RTRIM(NumeroPermiso)), ''), 'Permiso de gas natural') AS Nombre,
    COALESCE(NumeroPermiso, '') AS NumeroPermiso,
    COALESCE(EfId, '') AS Entidad,
    COALESCE(MpoId, '') AS Municipio,
    COALESCE(Estatus, '') AS Estatus,
    COALESCE(TipoPermiso, '') AS TipoPermiso,
    '' AS Tecnologia,
    COALESCE(TipoPermiso, '') AS Clasificacion,
    NULL AS Capacidad,
    '' AS UnidadCapacidad,
    COALESCE(CapacidadDiseño, '') AS CapacidadTexto,
    FechaOtorgamiento,
    NULL AS FechaOperacion,
    LatitudGeo AS Latitud,
    LongitudGeo AS Longitud
FROM dbo.vGasNatural_autorizado_mapa
" + BboxWhere + @"
ORDER BY FechaOtorgamiento DESC, NumeroPermiso;",
                "SELECT MAX(FechaOtorgamiento) AS FechaCorte FROM dbo.vGasNatural_autorizado_mapa;"),

            "gas-lp" => new PermisoQueryDefinition(
                "gas-lp",
                "Hidrocarburos · Gas LP",
                "CRE · Inventario de permisos de Gas LP DGMESNIE",
                @"
SELECT TOP (25000)
    COALESCE(NULLIF(LTRIM(RTRIM(RazonSocial)), ''), NULLIF(LTRIM(RTRIM(NumeroPermiso)), ''), 'Permiso de Gas LP') AS Nombre,
    COALESCE(NumeroPermiso, '') AS NumeroPermiso,
    COALESCE(EfId, '') AS Entidad,
    COALESCE(MpoId, '') AS Municipio,
    COALESCE(Estatus, '') AS Estatus,
    COALESCE(TipoPermiso, '') AS TipoPermiso,
    '' AS Tecnologia,
    COALESCE(NULLIF(SubTipo, ''), TipoPermiso, '') AS Clasificacion,
    TRY_CONVERT(float, CapacidadInstalacion) AS Capacidad,
    '' AS UnidadCapacidad,
    COALESCE(CONVERT(nvarchar(100), CapacidadInstalacion), '') AS CapacidadTexto,
    FechaDeOtorgamiento AS FechaOtorgamiento,
    inicioOperaciones AS FechaOperacion,
    LatitudGeo AS Latitud,
    LongitudGeo AS Longitud
FROM dbo.vGasLP_autorizado_mapa
" + BboxWhere + @"
ORDER BY FechaDeOtorgamiento DESC, NumeroPermiso;",
                "SELECT MAX(FechaDeOtorgamiento) AS FechaCorte FROM dbo.vGasLP_autorizado_mapa;"),

            "petroliferos" => new PermisoQueryDefinition(
                "petroliferos",
                "Hidrocarburos · Petrolíferos",
                "CRE · Inventario de permisos de petrolíferos DGMESNIE",
                @"
SELECT TOP (25000)
    COALESCE(NULLIF(LTRIM(RTRIM(RazonSocial)), ''), NULLIF(LTRIM(RTRIM(NumeroPermiso)), ''), 'Permiso de petrolíferos') AS Nombre,
    COALESCE(NumeroPermiso, '') AS NumeroPermiso,
    COALESCE(EfId, '') AS Entidad,
    COALESCE(MpoId, '') AS Municipio,
    COALESCE(Estatus, '') AS Estatus,
    COALESCE(TipoPermiso, '') AS TipoPermiso,
    '' AS Tecnologia,
    COALESCE(NULLIF(TipoDeEstacion, ''), TipoPermiso, '') AS Clasificacion,
    TRY_CONVERT(float, CapacidadAutorizadaBarriles) AS Capacidad,
    'barriles' AS UnidadCapacidad,
    COALESCE(CONVERT(nvarchar(100), CapacidadAutorizadaBarriles), '') AS CapacidadTexto,
    FechaOtorgamiento,
    InicioOperaciones AS FechaOperacion,
    LatitudGeo AS Latitud,
    LongitudGeo AS Longitud
FROM dbo.vExpendios_autorizado_mapa
" + BboxWhere + @"
ORDER BY FechaOtorgamiento DESC, NumeroPermiso;",
                "SELECT MAX(FechaOtorgamiento) AS FechaCorte FROM dbo.vExpendios_autorizado_mapa;"),

            _ => null
        };
    }

    private sealed record PermisoQueryDefinition(
        string Tipo,
        string Mercado,
        string Fuente,
        string Query,
        string CutoffQuery);
}

public sealed class ServicioPermisosEnergeticos : IServicioPermisosEnergeticos
{
    private static readonly IReadOnlyCollection<string> SupportedTypes =
        new[] { "electricidad", "gas-natural", "gas-lp", "petroliferos" };

    private readonly IRepositorioPermisosEnergeticos _repository;

    public ServicioPermisosEnergeticos(IRepositorioPermisosEnergeticos repository)
    {
        _repository = repository;
    }

    public IReadOnlyCollection<string> TiposSoportados => SupportedTypes;

    public async Task<PermisosEnergeticosGeoJson?> ObtenerGeoJsonAsync(
        string tipo,
        double minLat,
        double minLon,
        double maxLat,
        double maxLon,
        CancellationToken cancellationToken)
    {
        var normalizedType = (tipo ?? string.Empty).Trim().ToLowerInvariant();
        if (!SupportedTypes.Contains(normalizedType, StringComparer.Ordinal))
        {
            return null;
        }

        var data = await _repository.ObtenerAsync(
            normalizedType,
            minLat,
            minLon,
            maxLat,
            maxLon,
            cancellationToken);

        if (data is null)
        {
            return null;
        }

        var features = data.Registros
            .Where(row =>
                double.IsFinite(row.Latitud) &&
                double.IsFinite(row.Longitud) &&
                row.Latitud is >= 14 and <= 33.5 &&
                row.Longitud is >= -118 and <= -86)
            .Select(row => new PermisoEnergeticoFeature
            {
                Geometry = new PermisoEnergeticoPoint
                {
                    Coordinates = new[] { row.Longitud, row.Latitud }
                },
                Properties = new PermisoEnergeticoProperties
                {
                    Nombre = row.Nombre,
                    NumeroPermiso = row.NumeroPermiso,
                    Mercado = data.Mercado,
                    Entidad = row.Entidad,
                    Municipio = row.Municipio,
                    Estatus = row.Estatus,
                    TipoPermiso = row.TipoPermiso,
                    Tecnologia = row.Tecnologia,
                    Clasificacion = row.Clasificacion,
                    Capacidad = row.Capacidad,
                    UnidadCapacidad = row.UnidadCapacidad,
                    CapacidadTexto = row.CapacidadTexto,
                    FechaOtorgamiento = row.FechaOtorgamiento,
                    FechaOperacion = row.FechaOperacion,
                    Fuente = data.Fuente
                }
            })
            .ToList();

        return new PermisosEnergeticosGeoJson
        {
            Meta = new PermisosEnergeticosMeta
            {
                Tipo = data.Tipo,
                Mercado = data.Mercado,
                Fuente = data.Fuente,
                FechaCorte = data.FechaCorte,
                Conteo = features.Count
            },
            Features = features
        };
    }
}
