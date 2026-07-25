using Microsoft.Data.SqlClient;
using Microsoft.AspNetCore.Mvc;
using NSIE.Models;
using NSIE.Servicios;

namespace NSIE.Controllers;

[ApiController]
[Route("DashboardProyectos")]
public sealed class DashboardEnergiaController : ControllerBase
{
    private readonly IServicioPermisosEnergeticos _service;
    private readonly ILogger<DashboardEnergiaController> _logger;

    public DashboardEnergiaController(
        IServicioPermisosEnergeticos service,
        ILogger<DashboardEnergiaController> logger)
    {
        _service = service;
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
                cancellationToken);

            return result is null
                ? Problem(
                    statusCode: StatusCodes.Status400BadRequest,
                    title: "Tipo de permiso no válido")
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
                "Falló la consulta territorial de permisos energéticos para {Tipo}.",
                normalizedType);
            return Problem(
                statusCode: StatusCodes.Status503ServiceUnavailable,
                title: "Fuente de permisos no disponible",
                detail: "No fue posible consultar el inventario institucional en este momento.");
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
