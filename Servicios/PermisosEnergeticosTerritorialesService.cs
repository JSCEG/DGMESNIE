using Dapper;
using Microsoft.Data.SqlClient;
using NSIE.Models;
using System.Globalization;
using System.Text;

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
        PermisoEnergeticoContextoAcceso acceso,
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
        PermisoEnergeticoContextoAcceso acceso,
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
        PermisoEnergeticoContextoAcceso acceso,
        CancellationToken cancellationToken)
    {
        var definition = ObtenerDefinicionDetalle(tipo);
        var normalizedPermit = (numeroPermiso ?? string.Empty).Trim();
        if (definition is null || string.IsNullOrWhiteSpace(normalizedPermit))
        {
            return null;
        }

        var detailQuery = $"""
SELECT TOP (1) *
FROM dbo.[{definition.ViewName}]
WHERE [NumeroPermiso] = @NumeroPermiso
ORDER BY [{definition.OrderColumn}] DESC;

SELECT MAX([{definition.CutoffColumn}])
FROM dbo.[{definition.ViewName}];
""";
        var command = new CommandDefinition(
            detailQuery,
            new { NumeroPermiso = normalizedPermit },
            cancellationToken: cancellationToken);

        await using var connection = new SqlConnection(_connectionString);
        await connection.OpenAsync(cancellationToken);
        using var multi = await connection.QueryMultipleAsync(command);
        var row = await multi.ReadFirstOrDefaultAsync();
        var cutoff = await multi.ReadFirstOrDefaultAsync<DateTime?>();
        if (row is not IDictionary<string, object> values)
        {
            return null;
        }

        HashSet<string>? allowedFields = null;
        if (!acceso.EsInstitucional)
        {
            var configured = await ObtenerCamposVisiblesAsync(
                connection,
                definition.ViewName,
                acceso.RolId,
                acceso.MercadoId,
                cancellationToken);
            allowedFields = configured
                .Select(NormalizeFieldName)
                .ToHashSet(StringComparer.Ordinal);
        }

        var fields = values
            .Where(pair =>
                !IsTechnicalField(pair.Key) &&
                (acceso.EsInstitucional ||
                 IsIdentityField(pair.Key) ||
                 allowedFields!.Contains(NormalizeFieldName(pair.Key))))
            .Select(pair =>
            {
                var formatted = FormatDetailValue(pair.Value);
                return string.IsNullOrWhiteSpace(formatted)
                    ? null
                    : new PermisoEnergeticoDetalleCampo
                    {
                        Clave = pair.Key,
                        Etiqueta = GetFieldLabel(pair.Key),
                        Categoria = GetFieldCategory(pair.Key),
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
            Nombre = GetFirstDetailValue(
                values,
                "RazonSocial",
                "RazónSocial",
                "Nombre",
                "Titular",
                "Permisionario") ?? normalizedPermit,
            FechaCorte = cutoff,
            FechaConsulta = DateTime.Now,
            NivelAcceso = acceso.NivelAcceso,
            EsDetalleInstitucional = acceso.EsInstitucional,
            Entidad = GetFirstDetailValue(values, "EntidadFederativa", "Entidad", "EfId", "Estado") ?? string.Empty,
            Municipio = GetFirstDetailValue(values, "Municipio", "MunicipioEs", "MunicipioId", "MpoId") ?? string.Empty,
            Estatus = GetFirstDetailValue(values, "Estatus", "EstatusPermiso", "EstadoDePermiso", "EstatusInstalacion") ?? string.Empty,
            Latitud = GetFirstDouble(values, "LatitudGeo", "Latitud"),
            Longitud = GetFirstDouble(values, "LongitudGeo", "Longitud"),
            Campos = fields
        };
    }

    private static async Task<IReadOnlyList<string>> ObtenerCamposVisiblesAsync(
        SqlConnection connection,
        string viewName,
        int roleId,
        int marketId,
        CancellationToken cancellationToken)
    {
        const string query = """
SELECT Nombre_Campo
FROM dbo.CamposMapas
WHERE REPLACE([Tabla], ' ', '') = @ViewName
  AND Visible = 1
  AND Rol_ID = @RoleId
  AND Mercado_ID = @MarketId
ORDER BY Nombre_Campo;
""";
        var command = new CommandDefinition(
            query,
            new { ViewName = viewName, RoleId = roleId, MarketId = marketId },
            cancellationToken: cancellationToken);
        var configured = (await connection.QueryAsync<string>(command)).ToList();

        // El perfil público es únicamente un fallback. No se combina con una
        // configuración externa existente porque podría reactivar un campo que
        // ese rol o mercado ocultó expresamente.
        if (configured.Count == 0 && (roleId != 0 || marketId != 0))
        {
            var fallback = new CommandDefinition(
                query,
                new { ViewName = viewName, RoleId = 0, MarketId = 0 },
                cancellationToken: cancellationToken);
            configured = (await connection.QueryAsync<string>(fallback)).ToList();
        }

        return configured
            .Distinct(StringComparer.OrdinalIgnoreCase)
            .ToArray();
    }

    private static string? GetFirstDetailValue(
        IDictionary<string, object> values,
        params string[] keys)
    {
        foreach (var key in keys)
        {
            var pair = values.FirstOrDefault(item =>
                string.Equals(item.Key, key, StringComparison.OrdinalIgnoreCase));
            var formatted = FormatDetailValue(pair.Value);
            if (!string.IsNullOrWhiteSpace(formatted))
            {
                return formatted;
            }
        }

        return null;
    }

    private static double? GetFirstDouble(
        IDictionary<string, object> values,
        params string[] keys)
    {
        var raw = GetFirstDetailValue(values, keys);
        return double.TryParse(
            raw,
            NumberStyles.Any,
            CultureInfo.InvariantCulture,
            out var parsed)
            ? parsed
            : null;
    }

    private static string FormatDetailValue(object? value)
    {
        return value switch
        {
            null or DBNull => string.Empty,
            DateTime date when date.Year <= 1900 => string.Empty,
            DateTime date => date.ToString("yyyy-MM-dd", CultureInfo.InvariantCulture),
            DateTimeOffset date when date.Year <= 1900 => string.Empty,
            DateTimeOffset date => date.ToString("yyyy-MM-dd", CultureInfo.InvariantCulture),
            decimal number => number.ToString("0.####", CultureInfo.InvariantCulture),
            double number => number.ToString("0.####", CultureInfo.InvariantCulture),
            float number => number.ToString("0.####", CultureInfo.InvariantCulture),
            byte[] => string.Empty,
            _ => (Convert.ToString(value, CultureInfo.InvariantCulture) ?? string.Empty).Trim()
        };
    }

    private static bool IsIdentityField(string key)
    {
        var normalized = NormalizeFieldName(key);
        return normalized is
            "numeropermiso" or
            "razonsocial" or
            "nombre" or
            "titular" or
            "permisionario";
    }

    private static bool IsTechnicalField(string key)
    {
        var normalized = NormalizeFieldName(key);
        return normalized is
            "shape" or
            "geom" or
            "geometry" or
            "geografia" or
            "geography" or
            "ubicaciongeo" or
            "objectid" or
            "wkt" or
            "geojson";
    }

    private static string NormalizeFieldName(string? value)
    {
        var source = (value ?? string.Empty).Normalize(NormalizationForm.FormD);
        var result = new StringBuilder(source.Length);
        foreach (var character in source)
        {
            if (CharUnicodeInfo.GetUnicodeCategory(character) == UnicodeCategory.NonSpacingMark ||
                !char.IsLetterOrDigit(character))
            {
                continue;
            }

            result.Append(char.ToLowerInvariant(character));
        }

        return result.ToString();
    }

    private static string GetFieldLabel(string key)
    {
        var normalized = NormalizeFieldName(key);
        if (KnownFieldLabels.TryGetValue(normalized, out var label))
        {
            return label;
        }

        var result = new StringBuilder(key.Length + 8);
        for (var index = 0; index < key.Length; index++)
        {
            var character = key[index] == '_' ? ' ' : key[index];
            if (index > 0 &&
                char.IsUpper(character) &&
                char.IsLetterOrDigit(key[index - 1]) &&
                char.IsLower(key[index - 1]))
            {
                result.Append(' ');
            }

            result.Append(character);
        }

        var humanized = string.Join(
            " ",
            result.ToString().Split(
                ' ',
                StringSplitOptions.RemoveEmptyEntries | StringSplitOptions.TrimEntries));
        return humanized.Length == 0
            ? key
            : char.ToUpperInvariant(humanized[0]) + humanized[1..];
    }

    private static string GetFieldCategory(string key)
    {
        var normalized = NormalizeFieldName(key);
        if (normalized.Contains("latitud", StringComparison.Ordinal) ||
            normalized.Contains("longitud", StringComparison.Ordinal) ||
            normalized.Contains("calle", StringComparison.Ordinal) ||
            normalized.Contains("colonia", StringComparison.Ordinal) ||
            normalized.Contains("municipio", StringComparison.Ordinal) ||
            normalized.Contains("entidad", StringComparison.Ordinal) ||
            normalized.Contains("estado", StringComparison.Ordinal) ||
            normalized.Contains("direccion", StringComparison.Ordinal) ||
            normalized.Contains("domicilio", StringComparison.Ordinal) ||
            normalized.Contains("postal", StringComparison.Ordinal))
        {
            return "Ubicación";
        }

        if (normalized.Contains("fecha", StringComparison.Ordinal) ||
            normalized.Contains("vigencia", StringComparison.Ordinal) ||
            normalized.Contains("estatus", StringComparison.Ordinal) ||
            normalized.Contains("suspension", StringComparison.Ordinal) ||
            normalized.Contains("otorgamiento", StringComparison.Ordinal) ||
            normalized.Contains("acuse", StringComparison.Ordinal))
        {
            return "Situación regulatoria";
        }

        if (normalized.Contains("capacidad", StringComparison.Ordinal) ||
            normalized.Contains("tecnologia", StringComparison.Ordinal) ||
            normalized.Contains("combustible", StringComparison.Ordinal) ||
            normalized.Contains("energetico", StringComparison.Ordinal) ||
            normalized.Contains("producto", StringComparison.Ordinal) ||
            normalized.Contains("tanque", StringComparison.Ordinal) ||
            normalized.Contains("unidad", StringComparison.Ordinal) ||
            normalized.Contains("despach", StringComparison.Ordinal) ||
            normalized.Contains("servicio", StringComparison.Ordinal) ||
            normalized.Contains("generacion", StringComparison.Ordinal) ||
            normalized.Contains("interconexion", StringComparison.Ordinal))
        {
            return "Perfil técnico y operativo";
        }

        return "Identificación";
    }

    private static readonly IReadOnlyDictionary<string, string> KnownFieldLabels =
        new Dictionary<string, string>(StringComparer.Ordinal)
        {
            ["numeropermiso"] = "Número de permiso",
            ["numerodeexpediente"] = "Expediente",
            ["razonsocial"] = "Razón social",
            ["efid"] = "Entidad federativa",
            ["mpoid"] = "Municipio",
            ["rfc"] = "RFC",
            ["fechaotorgamiento"] = "Fecha de otorgamiento",
            ["fechadeotorgamiento"] = "Fecha de otorgamiento",
            ["fecharecepcion"] = "Fecha de recepción",
            ["iniciovigencia"] = "Inicio de vigencia",
            ["terminovigencia"] = "Término de vigencia",
            ["estatusinstalacion"] = "Estatus de la instalación",
            ["estadodepermiso"] = "Estado del permiso",
            ["latitudgeo"] = "Latitud",
            ["longitudgeo"] = "Longitud",
            ["codigopostal"] = "Código postal",
            ["energeticoprimario"] = "Energético primario",
            ["generacionestimadaanual"] = "Generación estimada anual",
            ["inversionestimadamdls"] = "Inversión estimada (MUSD)",
            ["inversionestimada"] = "Inversión estimada",
            ["capacidaddiseno"] = "Capacidad de diseño",
            ["capacidadlitros"] = "Capacidad (litros)",
            ["numerotanques"] = "Número de tanques",
            ["numerounidades"] = "Número de unidades",
            ["numerodemodulosdespachadores"] = "Módulos despachadores",
            ["tiendadeconveniencia"] = "Tienda de conveniencia"
        };

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
        IReadOnlyList<PermisoDetailField> Fields)
    {
        public string ViewName => Tipo switch
        {
            "electricidad" => "vElectricidad_autorizado_mapa",
            "gas-natural" => "vGasNatural_autorizado_mapa",
            "gas-lp" => "vGasLP_autorizado_mapa",
            "petroliferos" => "vExpendios_autorizado_mapa",
            _ => throw new InvalidOperationException($"Tipo de permiso no soportado: {Tipo}.")
        };

        public string OrderColumn => Tipo == "gas-lp"
            ? "FechaDeOtorgamiento"
            : "FechaOtorgamiento";

        public string CutoffColumn => OrderColumn;
    }

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
        PermisoEnergeticoContextoAcceso acceso,
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
            acceso,
            cancellationToken);
    }
}
