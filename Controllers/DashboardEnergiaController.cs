using Microsoft.Data.SqlClient;
using Microsoft.AspNetCore.Mvc;
using Newtonsoft.Json;
using NSIE.Models;
using NSIE.Servicios;

namespace NSIE.Controllers;

[ApiController]
[Route("DashboardProyectos")]
public sealed class DashboardEnergiaController : ControllerBase
{
    private readonly IServicioPermisosEnergeticos _service;
    private readonly IPoliticaAccesoPermisosEnergeticos _accessPolicy;
    private readonly ILogger<DashboardEnergiaController> _logger;

    public DashboardEnergiaController(
        IServicioPermisosEnergeticos service,
        IPoliticaAccesoPermisosEnergeticos accessPolicy,
        ILogger<DashboardEnergiaController> logger)
    {
        _service = service;
        _accessPolicy = accessPolicy;
        _logger = logger;
    }

    [HttpGet("PermisosEnergeticos")]
    [ResponseCache(
        Duration = 300,
        Location = ResponseCacheLocation.Client)]
    [ProducesResponseType(typeof(PermisosEnergeticosGeoJson), StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status400BadRequest)]
    public async Task<ActionResult<PermisosEnergeticosGeoJson>> PermisosEnergeticos(
        [FromQuery] string tipo,
        [FromQuery] double minLat,
        [FromQuery] double minLon,
        [FromQuery] double maxLat,
        [FromQuery] double maxLon,
        CancellationToken cancellationToken,
        [FromQuery] string? estatus = null)
    {
        // Por omisión sólo vigentes (como siempre vio el dashboard); el mapa SEM pide "todos".
        var soloVigentes = !string.Equals(estatus, "todos", StringComparison.OrdinalIgnoreCase);
        var normalizedType = (tipo ?? string.Empty).Trim().ToLowerInvariant();
        if (!_service.TiposSoportados.Contains(normalizedType, StringComparer.Ordinal))
        {
            return Problem(
                statusCode: StatusCodes.Status400BadRequest,
                title: "Tipo de permiso no válido",
                detail: $"Tipos permitidos: {string.Join(", ", _service.TiposSoportados)}.");
        }

        if (!BoundingBoxIsValid(minLat, minLon, maxLat, maxLon))
        {
            return Problem(
                statusCode: StatusCodes.Status400BadRequest,
                title: "Cobertura geográfica no válida",
                detail: "La caja geográfica debe estar ordenada y dentro del entorno territorial de México.");
        }

        try
        {
            var result = await _service.ObtenerGeoJsonAsync(
                normalizedType,
                minLat,
                minLon,
                maxLat,
                maxLon,
                soloVigentes,
                cancellationToken);

            return result is null
                ? Problem(
                    statusCode: StatusCodes.Status400BadRequest,
                    title: "Tipo de permiso no válido")
                : Ok(result);
        }
        catch (Exception ex) when (cancellationToken.IsCancellationRequested &&
                                   ex is OperationCanceledException or InvalidOperationException)
        {
            // El cliente cerró la petición a media lectura; Dapper lo reporta como
            // "reader is closed" en lugar de una cancelación.
            return new EmptyResult();
        }
        catch (SqlException ex)
        {
            _logger.LogError(
                ex,
                "Falló la consulta territorial de permisos energéticos para {Tipo}.",
                normalizedType);
            return Problem(
                statusCode: StatusCodes.Status503ServiceUnavailable,
                title: "Fuente de permisos no disponible",
                detail: "No fue posible consultar el inventario institucional en este momento.");
        }
    }

    [HttpGet("PermisosEnergeticos/Detalle")]
    [ResponseCache(NoStore = true, Location = ResponseCacheLocation.None)]
    [ProducesResponseType(typeof(PermisoEnergeticoDetalle), StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status400BadRequest)]
    [ProducesResponseType(StatusCodes.Status404NotFound)]
    public async Task<ActionResult<PermisoEnergeticoDetalle>> DetallePermisoEnergetico(
        [FromQuery] string tipo,
        [FromQuery] string numeroPermiso,
        CancellationToken cancellationToken)
    {
        var normalizedType = (tipo ?? string.Empty).Trim().ToLowerInvariant();
        if (!_service.TiposSoportados.Contains(normalizedType, StringComparer.Ordinal))
        {
            return Problem(
                statusCode: StatusCodes.Status400BadRequest,
                title: "Tipo de permiso no válido",
                detail: $"Tipos permitidos: {string.Join(", ", _service.TiposSoportados)}.");
        }

        var normalizedPermit = (numeroPermiso ?? string.Empty).Trim();
        if (string.IsNullOrWhiteSpace(normalizedPermit) ||
            normalizedPermit.Length > 120 ||
            normalizedPermit.Any(char.IsControl))
        {
            return Problem(
                statusCode: StatusCodes.Status400BadRequest,
                title: "Número de permiso no válido",
                detail: "Se requiere un número de permiso válido de hasta 120 caracteres.");
        }

        try
        {
            var acceso = _accessPolicy.Resolver(ObtenerPerfilUsuario());
            var result = await _service.ObtenerDetalleAsync(
                normalizedType,
                normalizedPermit,
                acceso,
                cancellationToken);

            return result is null
                ? Problem(
                    statusCode: StatusCodes.Status404NotFound,
                    title: "Permiso no encontrado",
                    detail: "El permiso solicitado no existe en el inventario territorial.")
                : Ok(result);
        }
        catch (OperationCanceledException) when (cancellationToken.IsCancellationRequested)
        {
            return new EmptyResult();
        }
        catch (SqlException ex)
        {
            _logger.LogError(
                ex,
                "Falló la consulta de detalle del permiso energético {NumeroPermiso} ({Tipo}).",
                normalizedPermit,
                normalizedType);
            return Problem(
                statusCode: StatusCodes.Status503ServiceUnavailable,
                title: "Fuente de permisos no disponible",
                detail: "No fue posible consultar la ficha del permiso en este momento.");
        }
    }

    [HttpGet("PermisosEnergeticos/SinUbicacion")]
    [ResponseCache(Duration = 300, Location = ResponseCacheLocation.Client)]
    public async Task<ActionResult<IReadOnlyList<PermisoSinUbicacion>>> PermisosSinUbicacion(
        [FromQuery] string tipo,
        [FromQuery] string? estatus,
        CancellationToken cancellationToken)
    {
        try
        {
            var result = await _service.ObtenerSinUbicacionAsync(
                tipo,
                !string.Equals(estatus, "todos", StringComparison.OrdinalIgnoreCase),
                cancellationToken);
            return result is null
                ? Problem(statusCode: StatusCodes.Status400BadRequest, title: "Tipo de permiso no válido")
                : Ok(result);
        }
        catch (Exception ex) when (cancellationToken.IsCancellationRequested &&
                                   ex is OperationCanceledException or InvalidOperationException)
        {
            return new EmptyResult();
        }
        catch (SqlException ex)
        {
            _logger.LogError(ex, "Falló la consulta de permisos sin ubicación para {Tipo}.", tipo);
            return Problem(statusCode: StatusCodes.Status503ServiceUnavailable, title: "Fuente de permisos no disponible");
        }
    }

    [HttpGet("PermisosEnergeticos/Actualizacion")]
    [ResponseCache(Duration = 300, Location = ResponseCacheLocation.Client)]
    public async Task<ActionResult<ActualizacionPermisosCne>> ActualizacionPermisos(CancellationToken cancellationToken)
    {
        return Ok(await _service.ObtenerActualizacionAsync(cancellationToken));
    }

    [HttpGet("PermisosEnergeticos/Buscar")]
    [ResponseCache(Duration = 120, Location = ResponseCacheLocation.Client)]
    [ProducesResponseType(typeof(PermisosEnergeticosBusqueda), StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status400BadRequest)]
    public async Task<ActionResult<PermisosEnergeticosBusqueda>> BuscarPermisosEnergeticos(
        [FromQuery] string q,
        [FromQuery] int limite = 200,
        CancellationToken cancellationToken = default)
    {
        var termino = (q ?? string.Empty).Trim();
        if (termino.Length < 2 || termino.Length > 120 || termino.Any(char.IsControl))
        {
            return Problem(
                statusCode: StatusCodes.Status400BadRequest,
                title: "Búsqueda no válida",
                detail: "Escribe entre 2 y 120 caracteres: número de permiso, razón social, entidad o municipio.");
        }

        try
        {
            var result = await _service.BuscarAsync(termino, limite, cancellationToken);
            return result is null
                ? Problem(statusCode: StatusCodes.Status400BadRequest, title: "Búsqueda no válida")
                : Ok(result);
        }
        catch (OperationCanceledException) when (cancellationToken.IsCancellationRequested)
        {
            return new EmptyResult();
        }
        catch (SqlException ex)
        {
            _logger.LogError(ex, "Falló la búsqueda de permisos energéticos para {Termino}.", termino);
            return Problem(
                statusCode: StatusCodes.Status503ServiceUnavailable,
                title: "Fuente de permisos no disponible",
                detail: "No fue posible consultar el inventario institucional en este momento.");
        }
    }

    private PerfilUsuario? ObtenerPerfilUsuario()
    {
        var json = HttpContext.Session.GetString("PerfilUsuario");
        if (string.IsNullOrWhiteSpace(json))
        {
            return null;
        }

        try
        {
            return JsonConvert.DeserializeObject<PerfilUsuario>(json);
        }
        catch (JsonException ex)
        {
            _logger.LogWarning(ex, "El perfil de sesión no pudo interpretarse para resolver el detalle de permisos.");
            return null;
        }
    }

    private static bool BoundingBoxIsValid(
        double minLat,
        double minLon,
        double maxLat,
        double maxLon)
    {
        return double.IsFinite(minLat) &&
               double.IsFinite(minLon) &&
               double.IsFinite(maxLat) &&
               double.IsFinite(maxLon) &&
               minLat < maxLat &&
               minLon < maxLon &&
               minLat >= 10 &&
               maxLat <= 36 &&
               minLon >= -121 &&
               maxLon <= -82;
    }
}
