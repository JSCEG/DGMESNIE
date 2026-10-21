using System.Globalization;
using Dapper;
using Microsoft.Data.SqlClient;

namespace NSIE.Servicios;

// Identificadores y columnas SQL proceden exclusivamente de este catálogo cerrado.
public sealed record TablaPermisosCne(string Nombre, string Columnas)
{
    public static TablaPermisosCne? Resolver(string? clave) => clave switch
    {
        "transporte" => new("Transporte_autorizado", "t.Producto AS producto, t.Medio_transporte AS medioTransporte, t.Modalidad AS modalidad, t.Capacidad AS capacidad, t.Unidad_capacidad AS unidadCapacidad, t.Numero_unidades AS numeroUnidades, t.Trayecto AS trayecto, t.Longitud_km AS longitudKm, t.Cobertura AS cobertura"),
        "comercializacion" => new("Comercializacion_autorizado", "t.Producto AS producto, t.Modalidad AS modalidad"),
        "infraestructura" => new("Infraestructura_Hidrocarburos_autorizado", "t.Producto AS producto, t.Capacidad AS capacidad, t.Unidad_capacidad AS unidadCapacidad, t.Zona_geografica AS zonaGeografica, t.Longitud_sistema_km AS longitudSistemaKm, t.Cobertura AS cobertura"),
        "suministro" => new("Suministro_Electrico_autorizado", "t.Modalidad AS modalidad, t.Cobertura AS cobertura"),
        "terceros" => new("Terceros_Acreditados_CNE", "t.Ambito AS ambito"),
        _ => null
    };
}

public sealed class RepositorioTablasPermisosCne
{
    private readonly string _connectionString;

    public RepositorioTablasPermisosCne(IConfiguration configuration)
    {
        _connectionString = configuration.GetConnectionString("DefaultConnection")
            ?? throw new InvalidOperationException("DefaultConnection no está configurada.");
    }

    public Task<IReadOnlyList<Dictionary<string, object?>>> ListaAsync(
        TablaPermisosCne tabla, string? estatus, string? ef, string? tipo, CancellationToken cancellationToken) =>
        ConsultarAsync($"""
            SELECT t.Id AS id, t.NumeroPermiso AS numeroPermiso, t.NumeroPermisoNorm AS numeroPermisoNorm,
                   t.Mercado AS mercado, t.Codigo_actividad AS codigoActividad, t.Tipo_permiso AS tipoPermiso,
                   t.[Razón_social] AS razonSocial, t.RFC AS rfc, t.Numero_de_Expediente AS expediente,
                   t.FechaOtorgamiento AS fechaOtorgamiento, t.Inicio_vigencia AS inicioVigencia,
                   t.Termino_vigencia AS terminoVigencia, t.Estatus AS estatus, t.Estatus_CNE AS estatusCne,
                   t.Estatus_instalacion AS estatusInstalacion, t.EF_ID AS efId, ef.EF_Nombre AS entidad,
                   t.MPO_ID AS mpoId, m.Municipio_Nombre AS municipio, t.Calle_num_ES AS calle,
                   t.Colonia_ES AS colonia, t.Codigo_Postal AS codigoPostal, t.Latitud_GEO AS latitud,
                   t.Longitud_GEO AS longitud, t.Inversion_estimada AS inversionEstimada,
                   t.Comentarios AS comentarios, t.FechaActualizacion AS fechaActualizacion,
                   l.LigaPermiso AS ligaPermiso, l.LigaResolucionOtorgamiento AS ligaResolucion,
                   {tabla.Columnas}
            FROM dbo.[{tabla.Nombre}] t
            LEFT JOIN dbo.Entidades_Federativas ef ON ef.EF_ID = t.EF_ID
            LEFT JOIN dbo.Municipios m ON m.EF_ID = t.EF_ID AND m.MunicipioID = t.MPO_ID
            LEFT JOIN dbo.Ligas_Permisos_CNE l ON l.NumeroPermisoNorm = t.NumeroPermisoNorm
            WHERE (@Estatus IS NULL OR t.Estatus = @Estatus)
              AND (@EF_ID IS NULL OR t.EF_ID = @EF_ID)
              AND (@Tipo IS NULL OR t.Tipo_permiso = @Tipo)
            ORDER BY t.FechaOtorgamiento DESC, t.Id;
            """, new { Estatus = estatus, EF_ID = ef, Tipo = tipo }, cancellationToken);

    public Task<IReadOnlyList<Dictionary<string, object?>>> EstadosAsync(
        TablaPermisosCne tabla, CancellationToken cancellationToken) =>
        ConsultarAsync($"""
            SELECT COALESCE(ef.EF_ID, N'--') AS efId,
                   COALESCE(ef.EF_Nombre, N'Sin entidad') AS entidad, COUNT(*) AS permisos
            FROM dbo.[{tabla.Nombre}] t
            LEFT JOIN dbo.Entidades_Federativas ef ON ef.EF_ID = t.EF_ID
            WHERE t.Estatus = N'Vigente'
            GROUP BY ef.EF_ID, ef.EF_Nombre;
            """, null, cancellationToken);

    public Task<IReadOnlyList<Dictionary<string, object?>>> PuntosAsync(
        TablaPermisosCne tabla, CancellationToken cancellationToken) =>
        ConsultarAsync($"""
            SELECT NumeroPermiso AS numeroPermiso, [Razón_social] AS razonSocial,
                   Tipo_permiso AS tipoPermiso, Latitud_GEO AS latitud, Longitud_GEO AS longitud
            FROM dbo.[{tabla.Nombre}]
            WHERE Latitud_GEO IS NOT NULL AND Longitud_GEO IS NOT NULL AND Estatus = N'Vigente';
            """, null, cancellationToken);

    public async Task<Dictionary<string, object?>?> ActualizacionAsync(
        TablaPermisosCne tabla, CancellationToken cancellationToken)
    {
        var filas = await ConsultarAsync("""
            SELECT TOP 1 FechaActualizacion AS fechaActualizacion, FechaCorteFuente AS fechaCorteFuente, Filas AS filas
            FROM dbo.Control_Actualizacion_CNE WHERE Tabla = @Tabla
            ORDER BY FechaActualizacion DESC;
            """, new { Tabla = tabla.Nombre }, cancellationToken);
        return filas.FirstOrDefault();
    }

    private async Task<IReadOnlyList<Dictionary<string, object?>>> ConsultarAsync(
        string sql, object? parametros, CancellationToken cancellationToken)
    {
        await using var connection = new SqlConnection(_connectionString);
        var filas = await connection.QueryAsync(new CommandDefinition(
            sql, parametros, commandTimeout: 60, cancellationToken: cancellationToken));
        return filas.Select(fila => ((IDictionary<string, object>)fila).ToDictionary(
            campo => campo.Key, campo => Formatear(campo.Key, campo.Value))).ToList();
    }

    internal static object? Formatear(string campo, object? valor)
    {
        if (valor is null or DBNull) return null;
        if (campo is "efId" or "mpoId") return ((string)valor).Trim();
        if (campo == "fechaActualizacion")
        {
            // datetime2 de la fuente ya contiene UTC; no reinterpretar como hora local.
            return valor is DateTimeOffset offset
                ? offset.UtcDateTime.ToString("O", CultureInfo.InvariantCulture)
                : DateTime.SpecifyKind((DateTime)valor, DateTimeKind.Utc).ToString("O", CultureInfo.InvariantCulture);
        }
        if (campo is "fechaOtorgamiento" or "inicioVigencia" or "terminoVigencia" or "fechaCorteFuente")
            return ((DateTime)valor).ToString("yyyy-MM-dd", CultureInfo.InvariantCulture);
        return valor;
    }
}
