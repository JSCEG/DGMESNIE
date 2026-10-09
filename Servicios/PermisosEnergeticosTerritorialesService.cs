using Dapper;
using Microsoft.Data.SqlClient;
using Microsoft.Extensions.Caching.Memory;
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
        bool soloVigentes,
        CancellationToken cancellationToken);

    Task<IReadOnlyList<PermisoSinUbicacion>> ObtenerSinUbicacionAsync(
        string tipo,
        bool soloVigentes,
        CancellationToken cancellationToken);

    Task<ActualizacionPermisosCne> ObtenerActualizacionAsync(CancellationToken cancellationToken);

    Task<PermisoEnergeticoDetalle?> ObtenerDetalleAsync(
        string tipo,
        string numeroPermiso,
        PermisoEnergeticoContextoAcceso acceso,
        CancellationToken cancellationToken);

    Task<IReadOnlyList<PermisoEnergeticoBusquedaItem>> ObtenerIndiceBusquedaAsync(
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
        bool soloVigentes,
        CancellationToken cancellationToken);

    Task<PermisoEnergeticoDetalle?> ObtenerDetalleAsync(
        string tipo,
        string numeroPermiso,
        PermisoEnergeticoContextoAcceso acceso,
        CancellationToken cancellationToken);

    Task<IReadOnlyList<PermisoSinUbicacion>?> ObtenerSinUbicacionAsync(
        string tipo,
        bool soloVigentes,
        CancellationToken cancellationToken);

    Task<ActualizacionPermisosCne> ObtenerActualizacionAsync(CancellationToken cancellationToken);

    Task<PermisosEnergeticosBusqueda?> BuscarAsync(
        string termino,
        int limite,
        CancellationToken cancellationToken);
}

public sealed class RepositorioPermisosEnergeticos : IRepositorioPermisosEnergeticos
{
    // Fuente única: las tablas base actualizadas contra el registro público de la CNE
    // (9-oct-2026). Las vistas v*_autorizado_mapa dejaban fuera no vigentes y permisos
    // sin coordenadas, y vElectricidad_autorizado trae nombres ficticios: no se usan.
    private static readonly IReadOnlyDictionary<string, FuentePermisos> Fuentes = new Dictionary<string, FuentePermisos>(StringComparer.Ordinal)
    {
        ["electricidad"] = new("electricidad", "Electricidad", "Electricidad_autorizado", "Razón_social", "Longitud_GEO",
            "COALESCE(LTRIM(RTRIM(t.[Tecnología])), '')", "COALESCE(LTRIM(RTRIM(t.[Clasifica_Menú])), '')",
            "TRY_CONVERT(float, t.Capacidad_autorizada_MW)", "MW", "''", "TRY_CONVERT(date, t.Inicio_operaciones)"),
        ["gas-natural"] = new("gas-natural", "Hidrocarburos · Gas natural", "GasNatural_autorizado", "Razón_Social", "longitud_GEO",
            "''", "COALESCE(LTRIM(RTRIM(t.Tipo_permiso)), '')",
            "CAST(NULL AS float)", "", "COALESCE(CONVERT(nvarchar(200), t.[Capacidad_diseño]), '')", "CAST(NULL AS date)"),
        ["gas-lp"] = new("gas-lp", "Hidrocarburos · Gas LP", "GasLP_autorizado", "Razón_social", "Longitud_GEO",
            "''", "COALESCE(LTRIM(RTRIM(t.Tipo_permiso)), '')",
            "COALESCE(TRY_CONVERT(float, t.Capacidad_litros), TRY_CONVERT(float, t.[Capacidad_Instalación]))", "litros",
            "COALESCE(CONVERT(nvarchar(100), t.[Capacidad_Instalación]), '')", "TRY_CONVERT(date, t.Inicio_operaciones)"),
        ["petroliferos"] = new("petroliferos", "Hidrocarburos · Petrolíferos", "Expendios_autorizado", "Razón_social", "Longitud_GEO",
            "''", "COALESCE(LTRIM(RTRIM(t.Tipo_permiso)), '')",
            "TRY_CONVERT(float, t.Capacidad_autorizada_barriles)", "barriles", "''", "TRY_CONVERT(date, t.Inicio_operaciones)")
    };

    private const string FuenteCne = "CNE · Registro público de permisos";

    // Coordenada utilizable: ni NULL, ni 0, ni fuera de México.
    private const string CoordenadaValida =
        "COALESCE(Latitud, 0) BETWEEN 14 AND 33 AND COALESCE(Longitud, 0) BETWEEN -118.5 AND -86.5";

    // Vigente: 'Vigente', 'Operando', 'En construcción', 'Por iniciar operaciones/obras'.
    private const string EsVigenteSql = @"CAST(CASE WHEN LTRIM(t.Estatus) COLLATE Latin1_General_CI_AI LIKE 'vigente%'
    OR LTRIM(t.Estatus) COLLATE Latin1_General_CI_AI LIKE 'operando%'
    OR LTRIM(t.Estatus) COLLATE Latin1_General_CI_AI LIKE 'en construccion%'
    OR LTRIM(t.Estatus) COLLATE Latin1_General_CI_AI LIKE 'por iniciar%' THEN 1 ELSE 0 END AS bit)";

    private static string SqlBase(FuentePermisos f) => $@"
SELECT
    COALESCE(NULLIF(LTRIM(RTRIM(t.[{f.ColumnaRazon}])), ''), LTRIM(RTRIM(t.NumeroPermiso)), '') AS Nombre,
    COALESCE(LTRIM(RTRIM(t.NumeroPermiso)), '') AS NumeroPermiso,
    COALESCE(LTRIM(RTRIM(t.EF_ID)), '') AS Entidad,
    COALESCE(LTRIM(RTRIM(t.MPO_ID)), '') AS Municipio,
    COALESCE(LTRIM(RTRIM(ef.EF_Nombre)), '') AS EntidadNombre,
    COALESCE(LTRIM(RTRIM(m.Municipio_Nombre)), '') AS MunicipioNombre,
    COALESCE(LTRIM(RTRIM(t.Estatus)), '') AS Estatus,
    {EsVigenteSql} AS EsVigente,
    COALESCE(LTRIM(RTRIM(t.Tipo_permiso)), '') AS TipoPermiso,
    {f.Tecnologia} AS Tecnologia,
    {f.Clasificacion} AS Clasificacion,
    {f.Capacidad} AS Capacidad,
    '{f.Unidad}' AS UnidadCapacidad,
    {f.CapacidadTexto} AS CapacidadTexto,
    TRY_CONVERT(date, t.FechaOtorgamiento) AS FechaOtorgamiento,
    {f.Operacion} AS FechaOperacion,
    TRY_CONVERT(float, t.Latitud_GEO) AS Latitud,
    TRY_CONVERT(float, t.[{f.ColumnaLongitud}]) AS Longitud,
    COALESCE(CONVERT(nvarchar(max), t.Comentarios), '') AS Comentarios
FROM dbo.[{f.Tabla}] t
LEFT JOIN dbo.Entidades_Federativas ef ON ef.EF_ID = t.EF_ID
LEFT JOIN dbo.Municipios m ON m.EF_ID = t.EF_ID AND m.MunicipioID = t.MPO_ID";

    // Los cuatro inventarios para el buscador; sin coordenada utilizable, Latitud/Longitud van NULL.
    private static readonly string BusquedaCte = "WITH Q AS (" + string.Join(Environment.NewLine + "UNION ALL" + Environment.NewLine,
        Fuentes.Values.Select(f => $@"SELECT '{f.Tipo}' AS Tipo, NumeroPermiso, Nombre, EntidadNombre AS Entidad, MunicipioNombre AS Municipio,
    TipoPermiso, Estatus,
    CASE WHEN {CoordenadaValida} THEN Latitud END AS Latitud,
    CASE WHEN {CoordenadaValida} THEN Longitud END AS Longitud,
    EsVigente AS Vigente
FROM ({SqlBase(f)}) b")) + ")";

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
        bool soloVigentes,
        CancellationToken cancellationToken)
    {
        if (!Fuentes.TryGetValue(tipo, out var fuente))
        {
            return null;
        }

        var baseSql = SqlBase(fuente);
        var sql = $@"
WITH B AS ({baseSql})
SELECT TOP (60000) *
FROM B
WHERE {CoordenadaValida}
  AND Latitud BETWEEN @MinLat AND @MaxLat
  AND Longitud BETWEEN @MinLon AND @MaxLon
  AND (@SoloVigentes = 0 OR EsVigente = 1)
ORDER BY FechaOtorgamiento DESC, NumeroPermiso;

SELECT MAX(TRY_CONVERT(date, FechaOtorgamiento)) FROM dbo.[{fuente.Tabla}];

WITH B AS ({baseSql})
SELECT COUNT(*) FROM B WHERE NOT ({CoordenadaValida}) AND (@SoloVigentes = 0 OR EsVigente = 1);";

        var command = new CommandDefinition(
            sql,
            new { MinLat = minLat, MinLon = minLon, MaxLat = maxLat, MaxLon = maxLon, SoloVigentes = soloVigentes },
            cancellationToken: cancellationToken);

        await using var connection = new SqlConnection(_connectionString);
        await connection.OpenAsync(cancellationToken);
        using var multi = await connection.QueryMultipleAsync(command);
        var rows = (await multi.ReadAsync<PermisoEnergeticoRegistro>()).ToList();
        var cutoff = await multi.ReadFirstOrDefaultAsync<DateTime?>();
        var sinUbicacion = await multi.ReadFirstOrDefaultAsync<int>();

        return new PermisosEnergeticosDatos
        {
            Tipo = fuente.Tipo,
            Mercado = fuente.Mercado,
            Fuente = FuenteCne,
            FechaCorte = cutoff,
            SinUbicacion = sinUbicacion,
            Registros = rows
        };
    }

    public async Task<IReadOnlyList<PermisoSinUbicacion>> ObtenerSinUbicacionAsync(
        string tipo,
        bool soloVigentes,
        CancellationToken cancellationToken)
    {
        if (!Fuentes.TryGetValue(tipo, out var fuente))
        {
            return Array.Empty<PermisoSinUbicacion>();
        }

        var sql = $@"
WITH B AS ({SqlBase(fuente)})
SELECT NumeroPermiso, Nombre, EntidadNombre AS Entidad, MunicipioNombre AS Municipio, Estatus, EsVigente
FROM B
WHERE NOT ({CoordenadaValida}) AND (@SoloVigentes = 0 OR EsVigente = 1)
ORDER BY EntidadNombre, MunicipioNombre, Nombre;";

        await using var connection = new SqlConnection(_connectionString);
        await connection.OpenAsync(cancellationToken);
        var rows = await connection.QueryAsync<PermisoSinUbicacion>(
            new CommandDefinition(sql, new { SoloVigentes = soloVigentes }, cancellationToken: cancellationToken));
        return rows.ToList();
    }

    public async Task<ActualizacionPermisosCne> ObtenerActualizacionAsync(CancellationToken cancellationToken)
    {
        // La tabla de control la crea el proceso de carga; si aún no existe no debe tronar.
        const string sql = @"
IF OBJECT_ID(N'dbo.Control_Actualizacion_CNE', N'U') IS NULL
    SELECT CAST(NULL AS nvarchar(256)) AS Tabla, CAST(NULL AS datetime2) AS Fecha WHERE 1 = 0;
ELSE
    EXEC(N'SELECT Tabla, MAX(TRY_CONVERT(datetime2, FechaActualizacion)) AS Fecha FROM dbo.Control_Actualizacion_CNE GROUP BY Tabla');
SELECT CAST(CASE WHEN OBJECT_ID(N'dbo.Control_Actualizacion_CNE', N'U') IS NULL THEN 0 ELSE 1 END AS bit);";

        await using var connection = new SqlConnection(_connectionString);
        await connection.OpenAsync(cancellationToken);
        using var multi = await connection.QueryMultipleAsync(new CommandDefinition(sql, cancellationToken: cancellationToken));
        var filas = (await multi.ReadAsync<(string Tabla, DateTime? Fecha)>()).ToList();
        var existe = await multi.ReadFirstOrDefaultAsync<bool>();

        var porTabla = new Dictionary<string, DateTime?>(StringComparer.OrdinalIgnoreCase);
        foreach (var (tabla, fecha) in filas.Where(f => !string.IsNullOrWhiteSpace(f.Tabla)))
        {
            var clave = tabla.Trim();
            var corta = clave.Contains('.') ? clave[(clave.LastIndexOf('.') + 1)..].Trim('[', ']') : clave.Trim('[', ']');
            porTabla[corta] = fecha;
        }

        return new ActualizacionPermisosCne
        {
            Disponible = existe && porTabla.Values.Any(v => v is not null),
            General = porTabla.Values.Where(v => v is not null).DefaultIfEmpty(null).Max(),
            PorTabla = porTabla
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
        if (row is null && Fuentes.TryGetValue(definition.Tipo, out var fuenteBase))
        {
            // La vista excluye no vigentes y permisos sin coordenadas: se lee la tabla base.
            row = await connection.QueryFirstOrDefaultAsync(new CommandDefinition(
                $"SELECT TOP (1) * FROM dbo.[{fuenteBase.Tabla}] WHERE LTRIM(RTRIM(NumeroPermiso)) = @NumeroPermiso",
                new { NumeroPermiso = normalizedPermit },
                cancellationToken: cancellationToken));
        }

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
                "Razón_social",
                "Razón_Social",
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

    public async Task<IReadOnlyList<PermisoEnergeticoBusquedaItem>> ObtenerIndiceBusquedaAsync(
        CancellationToken cancellationToken)
    {
        var command = new CommandDefinition(
            BusquedaCte + @"
SELECT Tipo, NumeroPermiso, Nombre, Entidad, Municipio, TipoPermiso, Estatus, Latitud, Longitud, Vigente
FROM Q;",
            cancellationToken: cancellationToken);

        await using var connection = new SqlConnection(_connectionString);
        await connection.OpenAsync(cancellationToken);
        var rows = await connection.QueryAsync<PermisoEnergeticoBusquedaItem>(command);
        return rows.ToList();
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

    private sealed record FuentePermisos(
        string Tipo,
        string Mercado,
        string Tabla,
        string ColumnaRazon,
        string ColumnaLongitud,
        string Tecnologia,
        string Clasificacion,
        string Capacidad,
        string Unidad,
        string CapacidadTexto,
        string Operacion);

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

    private const string CacheIndiceBusqueda = "permisos-energeticos:indice-busqueda";
    private static readonly TimeSpan VigenciaIndice = TimeSpan.FromMinutes(30);

    // Un solo armado del índice a la vez: sin esto, al vencer la caché cada tecla del
    // buscador lanzaba su propia consulta pesada y se encimaban en la base.
    private static readonly SemaphoreSlim CandadoIndice = new(1, 1);

    private readonly IRepositorioPermisosEnergeticos _repository;
    private readonly IMemoryCache _cache;

    public ServicioPermisosEnergeticos(IRepositorioPermisosEnergeticos repository, IMemoryCache cache)
    {
        _repository = repository;
        _cache = cache;
    }

    public IReadOnlyCollection<string> TiposSoportados => SupportedTypes;

    public async Task<PermisosEnergeticosGeoJson?> ObtenerGeoJsonAsync(
        string tipo,
        double minLat,
        double minLon,
        double maxLat,
        double maxLon,
        bool soloVigentes,
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
            soloVigentes,
            cancellationToken);

        if (data is null)
        {
            return null;
        }

        var features = data.Registros
            .Where(row =>
                double.IsFinite(row.Latitud) &&
                double.IsFinite(row.Longitud) &&
                row.Latitud is >= 14 and <= 33 &&
                row.Longitud is >= -118.5 and <= -86.5)
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
                    Fuente = data.Fuente,
                    Vigente = row.EsVigente,
                    Precision = ClasificarPrecision(row.Comentarios),
                    EntidadNombre = row.EntidadNombre,
                    MunicipioNombre = row.MunicipioNombre
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
                Conteo = features.Count,
                SinUbicacion = data.SinUbicacion
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

    public async Task<IReadOnlyList<PermisoSinUbicacion>?> ObtenerSinUbicacionAsync(
        string tipo,
        bool soloVigentes,
        CancellationToken cancellationToken)
    {
        var normalizedType = (tipo ?? string.Empty).Trim().ToLowerInvariant();
        if (!SupportedTypes.Contains(normalizedType, StringComparer.Ordinal))
        {
            return null;
        }

        return await _repository.ObtenerSinUbicacionAsync(normalizedType, soloVigentes, cancellationToken);
    }

    public async Task<ActualizacionPermisosCne> ObtenerActualizacionAsync(CancellationToken cancellationToken)
    {
        if (_cache.TryGetValue("permisos-energeticos:actualizacion", out ActualizacionPermisosCne? cached) && cached is not null)
        {
            return cached;
        }

        ActualizacionPermisosCne resultado;
        try
        {
            resultado = await _repository.ObtenerActualizacionAsync(cancellationToken);
        }
        catch (SqlException)
        {
            resultado = new ActualizacionPermisosCne { Disponible = false };
        }

        _cache.Set("permisos-energeticos:actualizacion", resultado, TimeSpan.FromMinutes(10));
        return resultado;
    }

    // Precisión de la coordenada según la nota que deja la carga CNE en Comentarios.
    public static string ClasificarPrecision(string? comentarios)
    {
        var c = Normalizar(comentarios);
        if (c.Length == 0)
        {
            return "exacta";
        }

        if ((c.Contains("geocodificad", StringComparison.Ordinal) && c.Contains("centroide de municipio", StringComparison.Ordinal)) ||
            c.Contains("confianza baja", StringComparison.Ordinal))
        {
            return "municipio";
        }

        return c.Contains("precision: calle", StringComparison.Ordinal) || c.Contains("precision calle", StringComparison.Ordinal)
            ? "calle"
            : "exacta";
    }

    public async Task<PermisosEnergeticosBusqueda?> BuscarAsync(
        string termino,
        int limite,
        CancellationToken cancellationToken)
    {
        var normalized = (termino ?? string.Empty).Trim();
        if (normalized.Length < 2)
        {
            return null;
        }

        // Cada palabra debe aparecer en permiso, razón social, entidad o municipio,
        // sin distinguir acentos ni mayúsculas ("pemex tabasco", "monterrey gas").
        var palabras = Normalizar(normalized)
            .Split(' ', StringSplitOptions.RemoveEmptyEntries)
            .Take(6)
            .ToArray();
        if (palabras.Length == 0)
        {
            return null;
        }

        var indice = await ObtenerIndiceAsync(cancellationToken);
        var permisoBuscado = Normalizar(normalized);
        var coincidencias = indice
            .Where(e => palabras.All(p => e.Texto.Contains(p, StringComparison.Ordinal)))
            .ToList();

        var resultados = coincidencias
            .OrderBy(e => e.Permiso == permisoBuscado ? 0
                : e.Permiso.StartsWith(permisoBuscado, StringComparison.Ordinal) ? 1
                : e.NombreNormalizado.StartsWith(permisoBuscado, StringComparison.Ordinal) ? 2
                : 3)
            .ThenBy(e => e.Item.Nombre, StringComparer.CurrentCultureIgnoreCase)
            .ThenBy(e => e.Item.NumeroPermiso, StringComparer.Ordinal)
            .Take(Math.Clamp(limite, 1, 500))
            .Select(e => e.Item)
            .ToList();

        return new PermisosEnergeticosBusqueda
        {
            Termino = normalized,
            Total = coincidencias.Count,
            PorTipo = coincidencias
                .GroupBy(e => e.Item.Tipo)
                .Select(g => new PermisoEnergeticoBusquedaConteo { Tipo = g.Key, Total = g.Count() })
                .ToList(),
            Resultados = resultados
        };
    }

    private async Task<IReadOnlyList<EntradaIndice>> ObtenerIndiceAsync(CancellationToken cancellationToken)
    {
        if (_cache.TryGetValue(CacheIndiceBusqueda, out IReadOnlyList<EntradaIndice>? indice) && indice is not null)
        {
            return indice;
        }

        await CandadoIndice.WaitAsync(cancellationToken);
        try
        {
            if (_cache.TryGetValue(CacheIndiceBusqueda, out indice) && indice is not null)
            {
                return indice;
            }

            // El armado no se cancela con la petición que lo inició: lo aprovechan las demás.
            var filas = await _repository.ObtenerIndiceBusquedaAsync(CancellationToken.None);
            indice = filas
                .Select(CoordenadaValida)
                .Select(f => new EntradaIndice(
                    f,
                    Normalizar(string.Join(' ', f.NumeroPermiso, f.Nombre, f.Entidad, f.Municipio)),
                    Normalizar(f.NumeroPermiso),
                    Normalizar(f.Nombre)))
                .ToList();
            _cache.Set(CacheIndiceBusqueda, indice, VigenciaIndice);
            return indice;
        }
        finally
        {
            CandadoIndice.Release();
        }
    }

    // Mismo recorte que las capas: una coordenada fuera de México (p. ej. una longitud
    // capturada sin punto decimal) haría volar el mapa al vacío.
    private static PermisoEnergeticoBusquedaItem CoordenadaValida(PermisoEnergeticoBusquedaItem f) =>
        f.Latitud is >= 14 and <= 33 && f.Longitud is >= -118.5 and <= -86.5
            ? f
            : new PermisoEnergeticoBusquedaItem
            {
                Tipo = f.Tipo,
                NumeroPermiso = f.NumeroPermiso,
                Nombre = f.Nombre,
                Entidad = f.Entidad,
                Municipio = f.Municipio,
                TipoPermiso = f.TipoPermiso,
                Estatus = f.Estatus,
                Vigente = f.Vigente
            };

    private static string Normalizar(string? value)
    {
        if (string.IsNullOrWhiteSpace(value))
        {
            return string.Empty;
        }

        var decomposed = value.Trim().ToLowerInvariant().Normalize(NormalizationForm.FormD);
        var builder = new StringBuilder(decomposed.Length);
        foreach (var c in decomposed)
        {
            if (CharUnicodeInfo.GetUnicodeCategory(c) != UnicodeCategory.NonSpacingMark)
            {
                builder.Append(char.IsWhiteSpace(c) ? ' ' : c);
            }
        }

        return builder.ToString();
    }

    private sealed record EntradaIndice(
        PermisoEnergeticoBusquedaItem Item,
        string Texto,
        string Permiso,
        string NombreNormalizado);
}
