using Microsoft.AspNetCore.Mvc;
using Microsoft.Data.SqlClient;
using NSIE.Models;
using NSIE.Servicios;
using System.Text.Json;

namespace NSIE.Controllers;

[ApiController]
[Route("DashboardProyectos/RedElectrica/InventarioSubestaciones")]
public sealed class RedElectricaSubstationInventoryController :
    ControllerBase
{
    private readonly IRedElectricaSubstationInventoryService _service;
    private readonly ILogger<RedElectricaSubstationInventoryController>
        _logger;

    public RedElectricaSubstationInventoryController(
        IRedElectricaSubstationInventoryService service,
        ILogger<RedElectricaSubstationInventoryController> logger)
    {
        _service = service;
        _logger = logger;
    }

    [HttpGet("Resumen")]
    [ResponseCache(Duration = 300, Location = ResponseCacheLocation.Client)]
    public async Task<ActionResult<
        RedElectricaSubstationInventorySummary>> Summary(
        CancellationToken cancellationToken = default)
    {
        try
        {
            return Ok(await _service.GetSummaryAsync(cancellationToken));
        }
        catch (Exception exception) when (IsDataException(exception))
        {
            return InventoryUnavailable(exception, "consultar el resumen");
        }
    }

    [HttpPost("Sincronizar")]
    [ValidateAntiForgeryToken]
    public async Task<ActionResult<
        RedElectricaSubstationInventorySummary>> Synchronize(
        CancellationToken cancellationToken = default)
    {
        try
        {
            return Ok(await _service.SynchronizeAsync(cancellationToken));
        }
        catch (Exception exception) when (IsDataException(exception))
        {
            return InventoryUnavailable(exception, "sincronizar el inventario");
        }
    }

    [HttpGet("Registros")]
    public async Task<ActionResult<IReadOnlyList<
        RedElectricaSubstationInventoryRecord>>> Records(
        [FromQuery] string? source = null,
        [FromQuery] string? state = null,
        [FromQuery] int limit = 500,
        CancellationToken cancellationToken = default)
    {
        if (source is { Length: > 40 } ||
            state is { Length: > 60 } ||
            limit is < 1 or > 5000)
        {
            return Problem(
                statusCode: StatusCodes.Status400BadRequest,
                title: "Parámetros de inventario no válidos",
                detail:
                    "Fuente admite 40 caracteres, estado 60 y límite entre 1 y 5,000.");
        }

        try
        {
            return Ok(await _service.GetRecordsAsync(
                source,
                state,
                limit,
                cancellationToken));
        }
        catch (Exception exception) when (IsDataException(exception))
        {
            return InventoryUnavailable(exception, "listar los registros");
        }
    }

    [HttpGet("GeoJson")]
    [Produces("application/geo+json", "application/json")]
    public async Task<ActionResult<RedElectricaGeoJson>> GeoJson(
        [FromQuery] string? networkLevel = null,
        [FromQuery] string? source = null,
        CancellationToken cancellationToken = default)
    {
        if (networkLevel is { Length: > 100 } ||
            source is { Length: > 40 })
        {
            return Problem(
                statusCode: StatusCodes.Status400BadRequest,
                title: "Parámetros GeoJSON no válidos");
        }

        try
        {
            return Ok(await _service.GetGeoJsonAsync(
                networkLevel,
                source,
                cancellationToken));
        }
        catch (Exception exception) when (IsDataException(exception))
        {
            return InventoryUnavailable(exception, "exportar el GeoJSON");
        }
    }

    [HttpPost("Georreferenciacion/Promover/{executionId:long}")]
    [ValidateAntiForgeryToken]
    [ProducesResponseType(
        typeof(RedElectricaSubstationPromotionSummary),
        StatusCodes.Status200OK)]
    public async Task<ActionResult<
        RedElectricaSubstationPromotionSummary>> PromoteGeoreferencing(
        long executionId,
        CancellationToken cancellationToken = default)
    {
        if (executionId <= 0)
        {
            return Problem(
                statusCode: StatusCodes.Status400BadRequest,
                title: "Ejecución de georreferenciación no válida");
        }

        try
        {
            return Ok(await _service.PromoteHighConfidenceAsync(
                executionId,
                cancellationToken));
        }
        catch (ArgumentOutOfRangeException exception)
        {
            return Problem(
                statusCode: StatusCodes.Status400BadRequest,
                title: "Ejecución de georreferenciación no válida",
                detail: exception.Message);
        }
        catch (Exception exception) when (IsDataException(exception))
        {
            return InventoryUnavailable(
                exception,
                "promover las coordenadas candidatas");
        }
    }

    private ObjectResult InventoryUnavailable(
        Exception exception,
        string operation)
    {
        _logger.LogWarning(
            exception,
            "No fue posible {Operation} de subestaciones.",
            operation);
        return Problem(
            statusCode: StatusCodes.Status503ServiceUnavailable,
            title: "Inventario de subestaciones no disponible",
            detail:
                "La base o alguna fuente de referencia no está disponible temporalmente.");
    }

    private static bool IsDataException(Exception exception) =>
        exception is SqlException or
            HttpRequestException or
            JsonException or
            InvalidOperationException;
}
