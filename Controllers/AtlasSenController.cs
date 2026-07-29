using Microsoft.AspNetCore.Mvc;
using NSIE.Models;
using NSIE.Servicios;
using System.Text.Json;

namespace NSIE.Controllers;

[ApiController]
[Route("DashboardProyectos/AtlasSen")]
public sealed class AtlasSenController : ControllerBase
{
    private readonly IAtlasSenReferenceService _service;
    private readonly ILogger<AtlasSenController> _logger;

    public AtlasSenController(
        IAtlasSenReferenceService service,
        ILogger<AtlasSenController> logger)
    {
        _service = service;
        _logger = logger;
    }

    [HttpGet("Estado")]
    [ResponseCache(Duration = 300, Location = ResponseCacheLocation.Client)]
    [ProducesResponseType(typeof(AtlasSenStatus), StatusCodes.Status200OK)]
    public async Task<ActionResult<AtlasSenStatus>> Status(
        [FromQuery] bool refresh = false,
        CancellationToken cancellationToken = default)
    {
        try
        {
            return Ok(await _service.GetStatusAsync(
                refresh,
                cancellationToken));
        }
        catch (OperationCanceledException) when (
            cancellationToken.IsCancellationRequested)
        {
            return new EmptyResult();
        }
        catch (Exception exception) when (IsSourceException(exception))
        {
            return AtlasUnavailable(exception, "consultar el estado");
        }
    }

    [HttpPost("Actualizar")]
    [ValidateAntiForgeryToken]
    [ProducesResponseType(typeof(AtlasSenStatus), StatusCodes.Status200OK)]
    public async Task<ActionResult<AtlasSenStatus>> Refresh(
        CancellationToken cancellationToken = default)
    {
        try
        {
            return Ok(await _service.RefreshAsync(cancellationToken));
        }
        catch (OperationCanceledException) when (
            cancellationToken.IsCancellationRequested)
        {
            return new EmptyResult();
        }
        catch (Exception exception) when (IsSourceException(exception))
        {
            return AtlasUnavailable(exception, "actualizar la fotografía");
        }
    }

    [HttpPost("Datasets/{datasetKey}/Reconocer")]
    [ValidateAntiForgeryToken]
    [ProducesResponseType(typeof(AtlasSenStatus), StatusCodes.Status200OK)]
    public async Task<ActionResult<AtlasSenStatus>> Acknowledge(
        string datasetKey,
        CancellationToken cancellationToken = default)
    {
        try
        {
            return Ok(await _service.AcknowledgeDatasetAsync(
                datasetKey,
                cancellationToken));
        }
        catch (ArgumentOutOfRangeException exception)
        {
            return Problem(
                statusCode: StatusCodes.Status400BadRequest,
                title: "Dataset Atlas SEN no válido",
                detail: exception.Message);
        }
        catch (OperationCanceledException) when (
            cancellationToken.IsCancellationRequested)
        {
            return new EmptyResult();
        }
        catch (Exception exception) when (IsSourceException(exception))
        {
            return AtlasUnavailable(exception, "reconocer el cambio");
        }
    }

    [HttpGet("Subestaciones")]
    [ResponseCache(Duration = 300, Location = ResponseCacheLocation.Client)]
    [ProducesResponseType(
        typeof(IReadOnlyList<AtlasSenSubstationReference>),
        StatusCodes.Status200OK)]
    public async Task<
        ActionResult<IReadOnlyList<AtlasSenSubstationReference>>> Substations(
        [FromQuery] string? search = null,
        [FromQuery] string? level = null,
        [FromQuery] int limit = 100,
        CancellationToken cancellationToken = default)
    {
        if (search is { Length: > 160 } ||
            level is { Length: > 40 } ||
            limit is < 1 or > 5000)
        {
            return Problem(
                statusCode: StatusCodes.Status400BadRequest,
                title: "Parámetros Atlas SEN no válidos",
                detail:
                    "La búsqueda admite 160 caracteres y el límite debe estar entre 1 y 5,000.");
        }

        try
        {
            return Ok(await _service.GetSubstationsAsync(
                search,
                level,
                limit,
                cancellationToken));
        }
        catch (OperationCanceledException) when (
            cancellationToken.IsCancellationRequested)
        {
            return new EmptyResult();
        }
        catch (Exception exception) when (IsSourceException(exception))
        {
            return AtlasUnavailable(exception, "consultar las subestaciones");
        }
    }

    [HttpGet("Subestaciones/Auditoria")]
    [ResponseCache(Duration = 300, Location = ResponseCacheLocation.Client)]
    [ProducesResponseType(
        typeof(AtlasSenSubstationAudit),
        StatusCodes.Status200OK)]
    public async Task<ActionResult<AtlasSenSubstationAudit>> AuditSubstation(
        [FromQuery] string name,
        [FromQuery] double? voltageKv = null,
        [FromQuery] string? region = null,
        CancellationToken cancellationToken = default)
    {
        if (string.IsNullOrWhiteSpace(name) ||
            name.Length > 160 ||
            voltageKv is < 0 or > 1000 ||
            region is { Length: > 100 })
        {
            return Problem(
                statusCode: StatusCodes.Status400BadRequest,
                title: "Referencia de subestación no válida");
        }

        try
        {
            return Ok(await _service.AuditSubstationAsync(
                name,
                voltageKv,
                region,
                cancellationToken));
        }
        catch (OperationCanceledException) when (
            cancellationToken.IsCancellationRequested)
        {
            return new EmptyResult();
        }
        catch (Exception exception) when (IsSourceException(exception))
        {
            return AtlasUnavailable(exception, "auditar la subestación");
        }
    }

    [HttpGet("Divisiones")]
    [ResponseCache(Duration = 600, Location = ResponseCacheLocation.Client)]
    [ProducesResponseType(
        typeof(AtlasSenTariffOverview),
        StatusCodes.Status200OK)]
    public async Task<ActionResult<AtlasSenTariffOverview>> TariffDivisions(
        CancellationToken cancellationToken = default)
    {
        try
        {
            return Ok(await _service.GetTariffOverviewAsync(
                cancellationToken));
        }
        catch (OperationCanceledException) when (
            cancellationToken.IsCancellationRequested)
        {
            return new EmptyResult();
        }
        catch (Exception exception) when (IsSourceException(exception))
        {
            return AtlasUnavailable(
                exception,
                "consultar las divisiones tarifarias");
        }
    }

    [HttpGet("Divisiones/GeoJson")]
    [ResponseCache(Duration = 600, Location = ResponseCacheLocation.Client)]
    [Produces("application/geo+json", "application/json")]
    [ProducesResponseType(
        typeof(AtlasSenGeoJson),
        StatusCodes.Status200OK)]
    public async Task<ActionResult<AtlasSenGeoJson>> TariffDivisionsGeoJson(
        CancellationToken cancellationToken = default)
    {
        try
        {
            return Ok(await _service.GetTariffDivisionsGeoJsonAsync(
                cancellationToken));
        }
        catch (OperationCanceledException) when (
            cancellationToken.IsCancellationRequested)
        {
            return new EmptyResult();
        }
        catch (Exception exception) when (IsSourceException(exception))
        {
            return AtlasUnavailable(
                exception,
                "exportar las divisiones tarifarias");
        }
    }

    [HttpGet("Mda")]
    [ResponseCache(Duration = 300, Location = ResponseCacheLocation.Client)]
    [ProducesResponseType(
        typeof(AtlasSenMdaResponse),
        StatusCodes.Status200OK)]
    public async Task<ActionResult<AtlasSenMdaResponse>> Mda(
        [FromQuery] string? zone = null,
        [FromQuery] bool hourly = false,
        CancellationToken cancellationToken = default)
    {
        if (zone is { Length: > 100 })
        {
            return Problem(
                statusCode: StatusCodes.Status400BadRequest,
                title: "Zona de carga no válida");
        }

        try
        {
            var snapshot = await _service.GetMdaAsync(cancellationToken);
            var selected = string.IsNullOrWhiteSpace(zone)
                ? snapshot.Zones
                : snapshot.Zones
                    .Where(pair => pair.Key.Contains(
                        zone.Trim(),
                        StringComparison.OrdinalIgnoreCase))
                    .ToDictionary(
                        pair => pair.Key,
                        pair => pair.Value,
                        StringComparer.OrdinalIgnoreCase);
            var zones = selected.ToDictionary(
                pair => pair.Key,
                pair => hourly
                    ? pair.Value
                    : new AtlasSenMdaZone
                    {
                        System = pair.Value.System,
                        AveragePnd = pair.Value.AveragePnd
                    },
                StringComparer.OrdinalIgnoreCase);
            return Ok(new AtlasSenMdaResponse
            {
                UpdatedAt = snapshot.UpdatedAt,
                OperatingDate = snapshot.OperatingDate,
                Source = snapshot.Source,
                Market = snapshot.Market,
                Count = snapshot.Count,
                IncludesHourlyDetail = hourly,
                Zones = zones
            });
        }
        catch (OperationCanceledException) when (
            cancellationToken.IsCancellationRequested)
        {
            return new EmptyResult();
        }
        catch (Exception exception) when (IsSourceException(exception))
        {
            return AtlasUnavailable(exception, "consultar el MDA");
        }
    }

    private ObjectResult AtlasUnavailable(
        Exception exception,
        string action)
    {
        _logger.LogWarning(
            exception,
            "No fue posible {Action} de Atlas SEN.",
            action);
        return Problem(
            statusCode: StatusCodes.Status503ServiceUnavailable,
            title: "Atlas SEN no disponible",
            detail:
                "No fue posible consultar la fuente ni una fotografía local válida.");
    }

    private static bool IsSourceException(Exception exception) =>
        exception is HttpRequestException or
            IOException or
            JsonException or
            InvalidOperationException;
}
