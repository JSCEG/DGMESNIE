using Dapper;
using Microsoft.Data.SqlClient;
using NSIE.Models;
using System.Globalization;

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

    Task<PermisoEnergeticoDetalle?> ObtenerDetalleAsync(
        string tipo,
        string numeroPermiso,
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

    Task<PermisoEnergeticoDetalle?> ObtenerDetalleAsync(
        string tipo,
        string numeroPermiso,
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

    public async Task<PermisoEnergeticoDetalle?> ObtenerDetalleAsync(
        string tipo,
        string numeroPermiso,
        CancellationToken cancellationToken)
    {
        var definition = ObtenerDefinicionDetalle(tipo);
        var normalizedPermit = (numeroPermiso ?? string.Empty).Trim();
        if (definition is null || string.IsNullOrWhiteSpace(normalizedPermit))
        {
            return null;
        }

        var command = new CommandDefinition(
            definition.Query,
            new { NumeroPermiso = normalizedPermit },
            cancellationToken: cancellationToken);

        await using var connection = new SqlConnection(_connectionString);
        await connection.OpenAsync(cancellationToken);
        var row = await connection.QueryFirstOrDefaultAsync(command);
        if (row is not IDictionary<string, object> values)
        {
            return null;
        }

        var fields = definition.Fields
            .Select(field =>
            {
                values.TryGetValue(field.Key, out var rawValue);
                var formatted = FormatDetailValue(rawValue);
                return string.IsNullOrWhiteSpace(formatted)
                    ? null
                    : new PermisoEnergeticoDetalleCampo
                    {
                        Clave = field.Key,
                        Etiqueta = field.Label,
                        Valor = formatted
                    };
            })
            .Where(field => field is not null)
            .Cast<PermisoEnergeticoDetalleCampo>()
            .ToArray();

        return new PermisoEnergeticoDetalle
        {
            Tipo = definition.Tipo,
            Mercado = definition.Mercado,
            Fuente = definition.Fuente,
            NumeroPermiso = normalizedPermit,
            Nombre = GetDetailValue(values, "Nombre") ?? normalizedPermit,
            Campos = fields
        };
    }

    private static string? GetDetailValue(
        IDictionary<string, object> values,
        string key)
    {
        return values.TryGetValue(key, out var value)
            ? FormatDetailValue(value)
            : null;
    }

    private static string FormatDetailValue(object? value)
    {
        return value switch
        {
            null or DBNull => string.Empty,
            DateTime date => date.ToString("yyyy-MM-dd", CultureInfo.InvariantCulture),
            DateTimeOffset date => date.ToString("yyyy-MM-dd", CultureInfo.InvariantCulture),
            decimal number => number.ToString("0.####", CultureInfo.InvariantCulture),
            double number => number.ToString("0.####", CultureInfo.InvariantCulture),
            float number => number.ToString("0.####", CultureInfo.InvariantCulture),
            _ => (Convert.ToString(value, CultureInfo.InvariantCulture) ?? string.Empty).Trim()
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

    private static PermisoDetailQueryDefinition? ObtenerDefinicionDetalle(string tipo)
    {
        return tipo switch
        {
            "electricidad" => new PermisoDetailQueryDefinition(
                "electricidad",
                "Electricidad",
                "CRE · Inventario de permisos eléctricos DGMESNIE",
                @"
SELECT TOP (1)
    COALESCE(NULLIF(LTRIM(RTRIM(RazonSocial)), ''), NumeroPermiso) AS Nombre,
    NumeroDeExpediente,
    [Dirección] AS Direccion,
    RFC,
    FechaRecepcion,
    EstatusInstalacion,
    InicioVigencia,
    [Generación_estimada_anual] AS GeneracionEstimadaAnual,
    Inversion_estimada_mdls AS InversionEstimadaMdls,
    Energetico_primario AS EnergeticoPrimario,
    Actividad_economica AS ActividadEconomica,
    [EmpresaLíder] AS EmpresaLider,
    [PaísDeOrigen] AS PaisDeOrigen,
    Subasta,
    Planta,
    Combustible,
    [FuenteEnergía] AS FuenteEnergia,
    Comentarios,
    [Tipo_Empresa] AS TipoEmpresa,
    LatitudGeo AS Latitud,
    LongitudGeo AS Longitud
FROM dbo.vElectricidad_autorizado_mapa
WHERE NumeroPermiso = @NumeroPermiso
ORDER BY FechaOtorgamiento DESC;",
                new[]
                {
                    new PermisoDetailField("NumeroDeExpediente", "Expediente"),
                    new PermisoDetailField("Direccion", "Dirección"),
                    new PermisoDetailField("RFC", "RFC"),
                    new PermisoDetailField("FechaRecepcion", "Fecha de recepción"),
                    new PermisoDetailField("EstatusInstalacion", "Estatus de la instalación"),
                    new PermisoDetailField("InicioVigencia", "Inicio de vigencia"),
                    new PermisoDetailField("GeneracionEstimadaAnual", "Generación estimada anual"),
                    new PermisoDetailField("InversionEstimadaMdls", "Inversión estimada (MUSD)"),
                    new PermisoDetailField("EnergeticoPrimario", "Energético primario"),
                    new PermisoDetailField("ActividadEconomica", "Actividad económica"),
                    new PermisoDetailField("EmpresaLider", "Empresa líder"),
                    new PermisoDetailField("PaisDeOrigen", "País de origen"),
                    new PermisoDetailField("Subasta", "Subasta"),
                    new PermisoDetailField("Planta", "Planta"),
                    new PermisoDetailField("Combustible", "Combustible"),
                    new PermisoDetailField("FuenteEnergia", "Fuente de energía"),
                    new PermisoDetailField("Comentarios", "Comentarios"),
                    new PermisoDetailField("TipoEmpresa", "Tipo de empresa"),
                    new PermisoDetailField("Latitud", "Latitud"),
                    new PermisoDetailField("Longitud", "Longitud")
                }),

            "gas-natural" => new PermisoDetailQueryDefinition(
                "gas-natural",
                "Hidrocarburos · Gas natural",
                "CRE · Inventario de permisos de gas natural DGMESNIE",
                @"
SELECT TOP (1)
    COALESCE(NULLIF(LTRIM(RTRIM(RazonSocial)), ''), NumeroPermiso) AS Nombre,
    NumeroDeExpediente,
    RFC,
    EstatusInstalacion,
    CalleNumEs,
    ColoniaEs,
    CodigoPostal,
    InversionEstimada,
    PermisoSuministrador,
    PermisoDistribuidor,
    Suministrador,
    Distribuidor,
    [Interconexión] AS Interconexion,
    Compresores,
    NumeroDespachadores,
    [CapacidadDiseño] AS CapacidadDiseno,
    Cilindros,
    Comentarios,
    LatitudGeo AS Latitud,
    LongitudGeo AS Longitud
FROM dbo.vGasNatural_autorizado_mapa
WHERE NumeroPermiso = @NumeroPermiso
ORDER BY FechaOtorgamiento DESC;",
                new[]
                {
                    new PermisoDetailField("NumeroDeExpediente", "Expediente"),
                    new PermisoDetailField("RFC", "RFC"),
                    new PermisoDetailField("EstatusInstalacion", "Estatus de la instalación"),
                    new PermisoDetailField("CalleNumEs", "Calle y número"),
                    new PermisoDetailField("ColoniaEs", "Colonia"),
                    new PermisoDetailField("CodigoPostal", "Código postal"),
                    new PermisoDetailField("InversionEstimada", "Inversión estimada"),
                    new PermisoDetailField("PermisoSuministrador", "Permiso del suministrador"),
                    new PermisoDetailField("PermisoDistribuidor", "Permiso del distribuidor"),
                    new PermisoDetailField("Suministrador", "Suministrador"),
                    new PermisoDetailField("Distribuidor", "Distribuidor"),
                    new PermisoDetailField("Interconexion", "Interconexión"),
                    new PermisoDetailField("Compresores", "Compresores"),
                    new PermisoDetailField("NumeroDespachadores", "Número de despachadores"),
                    new PermisoDetailField("CapacidadDiseno", "Capacidad de diseño"),
                    new PermisoDetailField("Cilindros", "Cilindros"),
                    new PermisoDetailField("Comentarios", "Comentarios"),
                    new PermisoDetailField("Latitud", "Latitud"),
                    new PermisoDetailField("Longitud", "Longitud")
                }),

            "gas-lp" => new PermisoDetailQueryDefinition(
                "gas-lp",
                "Hidrocarburos · Gas LP",
                "CRE · Inventario de permisos de Gas LP DGMESNIE",
                @"
SELECT TOP (1)
    COALESCE(NULLIF(LTRIM(RTRIM(RazonSocial)), ''), NumeroPermiso) AS Nombre,
    Expediente,
    Calle,
    Colonia,
    CodigoPostal,
    RFC,
    FechaRecepcion,
    Marca,
    VigenciaAnos,
    NumeroSENER,
    SubTipo,
    SiglasTipo,
    Otorgamiento,
    FechaAcuse,
    EstatusSAT,
    Subestatus,
    SuspensionInicio,
    SuspensionFin,
    NumeroTanques,
    CapacidadLitros,
    NumeroUnidades,
    NumeroDeCentralesDeGuarda,
    DomicilioDeGuarda,
    SuministroRecepcion,
    PermisoSuministro,
    CompartenTanques,
    Modificacion,
    Asociacion,
    Gie,
    LatitudGeo AS Latitud,
    LongitudGeo AS Longitud
FROM dbo.vGasLP_autorizado_mapa
WHERE NumeroPermiso = @NumeroPermiso
ORDER BY FechaDeOtorgamiento DESC;",
                new[]
                {
                    new PermisoDetailField("Expediente", "Expediente"),
                    new PermisoDetailField("Calle", "Calle"),
                    new PermisoDetailField("Colonia", "Colonia"),
                    new PermisoDetailField("CodigoPostal", "Código postal"),
                    new PermisoDetailField("RFC", "RFC"),
                    new PermisoDetailField("FechaRecepcion", "Fecha de recepción"),
                    new PermisoDetailField("Marca", "Marca"),
                    new PermisoDetailField("VigenciaAnos", "Vigencia"),
                    new PermisoDetailField("NumeroSENER", "Número SENER"),
                    new PermisoDetailField("SubTipo", "Subtipo"),
                    new PermisoDetailField("SiglasTipo", "Siglas del tipo"),
                    new PermisoDetailField("Otorgamiento", "Otorgamiento"),
                    new PermisoDetailField("FechaAcuse", "Fecha de acuse"),
                    new PermisoDetailField("EstatusSAT", "Estatus SAT"),
                    new PermisoDetailField("Subestatus", "Subestatus"),
                    new PermisoDetailField("SuspensionInicio", "Inicio de suspensión"),
                    new PermisoDetailField("SuspensionFin", "Fin de suspensión"),
                    new PermisoDetailField("NumeroTanques", "Número de tanques"),
                    new PermisoDetailField("CapacidadLitros", "Capacidad en litros"),
                    new PermisoDetailField("NumeroUnidades", "Número de unidades"),
                    new PermisoDetailField("NumeroDeCentralesDeGuarda", "Centrales de guarda"),
                    new PermisoDetailField("DomicilioDeGuarda", "Domicilio de guarda"),
                    new PermisoDetailField("SuministroRecepcion", "Suministro o recepción"),
                    new PermisoDetailField("PermisoSuministro", "Permiso de suministro"),
                    new PermisoDetailField("CompartenTanques", "Comparte tanques"),
                    new PermisoDetailField("Modificacion", "Modificación"),
                    new PermisoDetailField("Asociacion", "Asociación"),
                    new PermisoDetailField("Gie", "GIE"),
                    new PermisoDetailField("Latitud", "Latitud"),
                    new PermisoDetailField("Longitud", "Longitud")
                }),

            "petroliferos" => new PermisoDetailQueryDefinition(
                "petroliferos",
                "Hidrocarburos · Petrolíferos",
                "CRE · Inventario de permisos de petrolíferos DGMESNIE",
                @"
SELECT TOP (1)
    COALESCE(NULLIF(LTRIM(RTRIM(RazonSocial)), ''), NumeroPermiso) AS Nombre,
    NumeroDeExpediente,
    CalleNumEs,
    ColoniaEs,
    CodigoPostal,
    RFC,
    FechaRecepcion,
    EstatusInstalacion,
    CausaSuspension,
    Marca,
    InicioVigencia,
    TerminoVigencia,
    InversionEstimada,
    Productos,
    Comentarios,
    TipoPersona,
    NumeroDeEstacionesDeServicio,
    TipoDeEstacion,
    FechaDeAcuse,
    FechaEntregaEstadosFinancieros,
    Propietario,
    CapacidadMaximaDeLaBomba,
    CapacidadOperativaReal,
    ServicioDeRegadera,
    ServicioDeRestaurante,
    ServicioDeSanitario,
    OtrosServicios,
    TiendaDeConveniencia,
    NumeroDeModulosDespachadores,
    EstadoDePermiso,
    EstatusDeLaInstalacion,
    ImagenCorporativa,
    LatitudGeo AS Latitud,
    LongitudGeo AS Longitud
FROM dbo.vExpendios_autorizado_mapa
WHERE NumeroPermiso = @NumeroPermiso
ORDER BY FechaOtorgamiento DESC;",
                new[]
                {
                    new PermisoDetailField("NumeroDeExpediente", "Expediente"),
                    new PermisoDetailField("CalleNumEs", "Calle y número"),
                    new PermisoDetailField("ColoniaEs", "Colonia"),
                    new PermisoDetailField("CodigoPostal", "Código postal"),
                    new PermisoDetailField("RFC", "RFC"),
                    new PermisoDetailField("FechaRecepcion", "Fecha de recepción"),
                    new PermisoDetailField("EstatusInstalacion", "Estatus de la instalación"),
                    new PermisoDetailField("CausaSuspension", "Causa de suspensión"),
                    new PermisoDetailField("Marca", "Marca"),
                    new PermisoDetailField("InicioVigencia", "Inicio de vigencia"),
                    new PermisoDetailField("TerminoVigencia", "Término de vigencia"),
                    new PermisoDetailField("InversionEstimada", "Inversión estimada"),
                    new PermisoDetailField("Productos", "Productos"),
                    new PermisoDetailField("Comentarios", "Comentarios"),
                    new PermisoDetailField("TipoPersona", "Tipo de persona"),
                    new PermisoDetailField("NumeroDeEstacionesDeServicio", "Número de estaciones de servicio"),
                    new PermisoDetailField("TipoDeEstacion", "Tipo de estación"),
                    new PermisoDetailField("FechaDeAcuse", "Fecha de acuse"),
                    new PermisoDetailField("FechaEntregaEstadosFinancieros", "Entrega de estados financieros"),
                    new PermisoDetailField("Propietario", "Propietario"),
                    new PermisoDetailField("CapacidadMaximaDeLaBomba", "Capacidad máxima de bomba"),
                    new PermisoDetailField("CapacidadOperativaReal", "Capacidad operativa real"),
                    new PermisoDetailField("ServicioDeRegadera", "Servicio de regadera"),
                    new PermisoDetailField("ServicioDeRestaurante", "Servicio de restaurante"),
                    new PermisoDetailField("ServicioDeSanitario", "Servicio sanitario"),
                    new PermisoDetailField("OtrosServicios", "Otros servicios"),
                    new PermisoDetailField("TiendaDeConveniencia", "Tienda de conveniencia"),
                    new PermisoDetailField("NumeroDeModulosDespachadores", "Módulos despachadores"),
                    new PermisoDetailField("EstadoDePermiso", "Estado del permiso"),
                    new PermisoDetailField("EstatusDeLaInstalacion", "Estatus de la instalación (catálogo)"),
                    new PermisoDetailField("ImagenCorporativa", "Imagen corporativa"),
                    new PermisoDetailField("Latitud", "Latitud"),
                    new PermisoDetailField("Longitud", "Longitud")
                }),

            _ => null
        };
    }

    private sealed record PermisoQueryDefinition(
        string Tipo,
        string Mercado,
        string Fuente,
        string Query,
        string CutoffQuery);

    private sealed record PermisoDetailQueryDefinition(
        string Tipo,
        string Mercado,
        string Fuente,
        string Query,
        IReadOnlyList<PermisoDetailField> Fields);

    private sealed record PermisoDetailField(string Key, string Label);
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

    public Task<PermisoEnergeticoDetalle?> ObtenerDetalleAsync(
        string tipo,
        string numeroPermiso,
        CancellationToken cancellationToken)
    {
        var normalizedType = (tipo ?? string.Empty).Trim().ToLowerInvariant();
        if (!SupportedTypes.Contains(normalizedType, StringComparer.Ordinal))
        {
            return Task.FromResult<PermisoEnergeticoDetalle?>(null);
        }

        return _repository.ObtenerDetalleAsync(
            normalizedType,
            numeroPermiso,
            cancellationToken);
    }
}
