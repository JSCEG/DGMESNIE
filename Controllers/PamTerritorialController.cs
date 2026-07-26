using Microsoft.AspNetCore.Mvc;
using Microsoft.Data.SqlClient;
using NSIE.Models;
using NSIE.Servicios;

namespace NSIE.Controllers;

[ApiController]
[Route("DashboardProyectos/PamTerritorial")]
public sealed class PamTerritorialController : ControllerBase
{
    private readonly IPamTerritorialService _service;
    private readonly IPamRedAssociationService _redAssociationService;
    private readonly ILogger<PamTerritorialController> _logger;

    public PamTerritorialController(
        IPamTerritorialService service,
        IPamRedAssociationService redAssociationService,
        ILogger<PamTerritorialController> logger)
    {
        _service = service;
        _redAssociationService = redAssociationService;
        _logger = logger;
    }

    [HttpGet]
    [ResponseCache(Duration = 300, Location = ResponseCacheLocation.Client)]
    [ProducesResponseType(
        typeof(IReadOnlyList<PamTerritorialProyecto>),
        StatusCodes.Status200OK)]
    public async Task<ActionResult<IReadOnlyList<PamTerritorialProyecto>>> Buscar(
        [FromQuery] string? busqueda = null,
        [FromQuery] int limite = 500,
        CancellationToken cancellationToken = default)
    {
        if (busqueda is { Length: > 160 } || limite is < 1 or > 500)
        {
            return Problem(
                statusCode: StatusCodes.Status400BadRequest,
                title: "Parámetros de búsqueda PAM no válidos",
                detail: "La búsqueda admite hasta 160 caracteres y el límite debe estar entre 1 y 500.");
        }

        try
        {
            return Ok(await _service.BuscarAsync(
                busqueda,
                limite,
                cancellationToken));
        }
        catch (OperationCanceledException) when (cancellationToken.IsCancellationRequested)
        {
            return new EmptyResult();
        }
        catch (SqlException ex)
        {
            _logger.LogError(ex, "Falló la consulta del índice territorial PAM.");
            return Problem(
                statusCode: StatusCodes.Status503ServiceUnavailable,
                title: "Fuente PAM no disponible",
                detail: "No fue posible consultar el catálogo territorial PAM en este momento.");
        }
    }

    [HttpGet("{proyectoId:long}")]
    [ResponseCache(Duration = 300, Location = ResponseCacheLocation.Client)]
    [ProducesResponseType(typeof(PamTerritorialProyecto), StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status404NotFound)]
    public async Task<ActionResult<PamTerritorialProyecto>> Obtener(
        long proyectoId,
        CancellationToken cancellationToken)
    {
        try
        {
            var project = await _service.ObtenerAsync(proyectoId, cancellationToken);
            return project is null
                ? Problem(
                    statusCode: StatusCodes.Status404NotFound,
                    title: "Proyecto PAM no encontrado")
                : Ok(project);
        }
        catch (OperationCanceledException) when (cancellationToken.IsCancellationRequested)
        {
            return new EmptyResult();
        }
        catch (SqlException ex)
        {
            _logger.LogError(
                ex,
                "Falló la consulta territorial del proyecto PAM {ProyectoId}.",
                proyectoId);
            return Problem(
                statusCode: StatusCodes.Status503ServiceUnavailable,
                title: "Fuente PAM no disponible",
                detail: "No fue posible consultar el proyecto PAM en este momento.");
        }
    }

    [HttpGet("{proyectoId:long}/AsociacionesRed")]
    [ResponseCache(Duration = 300, Location = ResponseCacheLocation.Client)]
    [ProducesResponseType(typeof(PamRedAssociationResult), StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status404NotFound)]
    public async Task<ActionResult<PamRedAssociationResult>> AsociacionesRed(
        long proyectoId,
        CancellationToken cancellationToken)
    {
        try
        {
            var project = await _service.ObtenerAsync(proyectoId, cancellationToken);
            if (project is null)
            {
                return Problem(
                    statusCode: StatusCodes.Status404NotFound,
                    title: "Proyecto PAM no encontrado");
            }

            var result = (await _redAssociationService.ResolverAsync(
                new[] { project },
                cancellationToken)).First();
            return Ok(result);
        }
        catch (OperationCanceledException) when (cancellationToken.IsCancellationRequested)
        {
            return new EmptyResult();
        }
        catch (Exception ex) when (
            ex is SqlException or
            HttpRequestException or
            System.Text.Json.JsonException)
        {
            _logger.LogError(
                ex,
                "Falló el análisis de asociaciones de red del proyecto PAM {ProyectoId}.",
                proyectoId);
            return Problem(
                statusCode: StatusCodes.Status503ServiceUnavailable,
                title: "Catálogo eléctrico no disponible",
                detail: "No fue posible contrastar el proyecto PAM con las subestaciones y líneas en este momento.");
        }
    }

    [HttpGet("GeoJson")]
    [ResponseCache(Duration = 300, Location = ResponseCacheLocation.Client)]
    [Produces("application/geo+json", "application/json")]
    [ProducesResponseType(typeof(PamTerritorialGeoJson), StatusCodes.Status200OK)]
    public async Task<ActionResult<PamTerritorialGeoJson>> GeoJson(
        CancellationToken cancellationToken)
    {
        try
        {
            return Ok(await _service.ObtenerGeoJsonAsync(cancellationToken));
        }
        catch (OperationCanceledException) when (cancellationToken.IsCancellationRequested)
        {
            return new EmptyResult();
        }
        catch (SqlException ex)
        {
            _logger.LogError(ex, "Falló la consulta GeoJSON de proyectos PAM.");
            return Problem(
                statusCode: StatusCodes.Status503ServiceUnavailable,
                title: "Fuente PAM no disponible",
                detail: "No fue posible consultar las ubicaciones PAM en este momento.");
        }
    }
}
