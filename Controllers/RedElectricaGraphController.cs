using System.Text.Json;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Data.SqlClient;
using NSIE.Models;
using NSIE.Servicios;

namespace NSIE.Controllers;

[ApiController]
[Route("DashboardProyectos/RedElectrica/Grafo")]
public sealed class RedElectricaGraphController : ControllerBase
{
    private readonly IRedElectricaGraphService _service;
    private readonly ILogger<RedElectricaGraphController> _logger;

    public RedElectricaGraphController(
        IRedElectricaGraphService service,
        ILogger<RedElectricaGraphController> logger)
    {
        _service = service;
        _logger = logger;
    }

    [HttpGet("Resumen")]
    [ResponseCache(Duration = 300, Location = ResponseCacheLocation.Client)]
    [ProducesResponseType(
        typeof(RedElectricaGraphSummary),
        StatusCodes.Status200OK)]
    public async Task<ActionResult<RedElectricaGraphSummary>> Summary(
        CancellationToken cancellationToken)
    {
        try
        {
            return Ok((await _service.GetAsync(
                false,
                cancellationToken)).Summary);
        }
        catch (OperationCanceledException) when (
            cancellationToken.IsCancellationRequested)
        {
            return new EmptyResult();
        }
        catch (Exception ex) when (IsDataSourceException(ex))
        {
            return GraphUnavailable(ex, "construir el resumen");
        }
    }

    [HttpGet("Simulacion")]
    [ResponseCache(NoStore = true, Location = ResponseCacheLocation.None)]
    [ProducesResponseType(
        typeof(RedElectricaGraphSimulation),
        StatusCodes.Status200OK)]
    public async Task<ActionResult<RedElectricaGraphSimulation>> Simulation(
        CancellationToken cancellationToken)
    {
        try
        {
            return Ok(await _service.SimulateAsync(cancellationToken));
        }
        catch (OperationCanceledException) when (
            cancellationToken.IsCancellationRequested)
        {
            return new EmptyResult();
        }
        catch (Exception ex) when (
            IsDataSourceException(ex) ||
            ex is InvalidOperationException)
        {
            return GraphUnavailable(
                ex,
                "simular la nueva versión del grafo");
        }
    }

    [HttpGet("Json")]
    [ResponseCache(Duration = 300, Location = ResponseCacheLocation.Client)]
    [ProducesResponseType(
        typeof(RedElectricaGraphSnapshot),
        StatusCodes.Status200OK)]
    public async Task<ActionResult<RedElectricaGraphSnapshot>> GraphJson(
        CancellationToken cancellationToken)
    {
        try
        {
            return Ok(await _service.GetAsync(false, cancellationToken));
        }
        catch (OperationCanceledException) when (
            cancellationToken.IsCancellationRequested)
        {
            return new EmptyResult();
        }
        catch (Exception ex) when (IsDataSourceException(ex))
        {
            return GraphUnavailable(ex, "exportar el grafo JSON");
        }
    }

    [HttpPost("Reconstruir")]
    [ValidateAntiForgeryToken]
    [ProducesResponseType(
        typeof(RedElectricaGraphSummary),
        StatusCodes.Status200OK)]
    public async Task<ActionResult<RedElectricaGraphSummary>> Rebuild(
        [FromQuery] bool persist = true,
        CancellationToken cancellationToken = default)
    {
        try
        {
            return Ok((await _service.RebuildAsync(
                persist,
                cancellationToken)).Summary);
        }
        catch (OperationCanceledException) when (
            cancellationToken.IsCancellationRequested)
        {
            return new EmptyResult();
        }
        catch (Exception ex) when (
            IsDataSourceException(ex) ||
            ex is InvalidOperationException)
        {
            return GraphUnavailable(ex, "reconstruir y persistir el grafo");
        }
    }

    [HttpGet("Nodos")]
    [ResponseCache(Duration = 300, Location = ResponseCacheLocation.Client)]
    [ProducesResponseType(
        typeof(IReadOnlyList<RedElectricaGraphNode>),
        StatusCodes.Status200OK)]
    public async Task<ActionResult<IReadOnlyList<RedElectricaGraphNode>>> Nodes(
        [FromQuery] string? search = null,
        [FromQuery] bool includeVirtual = false,
        [FromQuery] int limit = 100,
        CancellationToken cancellationToken = default)
    {
        if (search is { Length: > 160 } || limit is < 1 or > 500)
        {
            return Problem(
                statusCode: StatusCodes.Status400BadRequest,
                title: "Parámetros de búsqueda no válidos",
                detail: "La búsqueda admite 160 caracteres y el límite debe estar entre 1 y 500.");
        }

        try
        {
            var normalized = (search ?? string.Empty).Trim();
            var snapshot = await _service.GetAsync(false, cancellationToken);
            var nodes = snapshot.Nodes.Values
                .Where(node => includeVirtual || !node.IsVirtual)
                .Where(node =>
                    string.IsNullOrWhiteSpace(normalized) ||
                    node.Name.Contains(
                        normalized,
                        StringComparison.OrdinalIgnoreCase) ||
                    node.NodeId.Contains(
                        normalized,
                        StringComparison.OrdinalIgnoreCase))
                .OrderBy(node => node.Name, StringComparer.OrdinalIgnoreCase)
                .Take(limit)
                .ToList();
            return Ok(nodes);
        }
        catch (OperationCanceledException) when (
            cancellationToken.IsCancellationRequested)
        {
            return new EmptyResult();
        }
        catch (Exception ex) when (IsDataSourceException(ex))
        {
            return GraphUnavailable(ex, "listar las subestaciones");
        }
    }

    [HttpGet("Nodos/{nodeId}")]
    [ResponseCache(Duration = 300, Location = ResponseCacheLocation.Client)]
    [ProducesResponseType(
        typeof(RedElectricaGraphNode),
        StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status404NotFound)]
    public async Task<ActionResult<RedElectricaGraphNode>> Node(
        string nodeId,
        CancellationToken cancellationToken)
    {
        try
        {
            var snapshot = await _service.GetAsync(false, cancellationToken);
            return snapshot.Nodes.TryGetValue(nodeId, out var node)
                ? Ok(node)
                : Problem(
                    statusCode: StatusCodes.Status404NotFound,
                    title: "Subestación o nodo no encontrado");
        }
        catch (OperationCanceledException) when (
            cancellationToken.IsCancellationRequested)
        {
            return new EmptyResult();
        }
        catch (Exception ex) when (IsDataSourceException(ex))
        {
            return GraphUnavailable(ex, "consultar el nodo");
        }
    }

    [HttpGet("Nodos/{nodeId}/Vecinos")]
    [ResponseCache(Duration = 300, Location = ResponseCacheLocation.Client)]
    [ProducesResponseType(
        typeof(RedElectricaGraphNeighborResult),
        StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status404NotFound)]
    public async Task<ActionResult<RedElectricaGraphNeighborResult>> Neighbors(
        string nodeId,
        [FromQuery] int depth = 1,
        CancellationToken cancellationToken = default)
    {
        if (depth is < 1 or > 5)
        {
            return Problem(
                statusCode: StatusCodes.Status400BadRequest,
                title: "Profundidad no válida",
                detail: "La profundidad permitida está entre 1 y 5 saltos.");
        }

        try
        {
            var result = await _service.GetNeighborsAsync(
                nodeId,
                depth,
                cancellationToken);
            return result is null
                ? Problem(
                    statusCode: StatusCodes.Status404NotFound,
                    title: "Subestación o nodo no encontrado")
                : Ok(result);
        }
        catch (OperationCanceledException) when (
            cancellationToken.IsCancellationRequested)
        {
            return new EmptyResult();
        }
        catch (Exception ex) when (IsDataSourceException(ex))
        {
            return GraphUnavailable(ex, "consultar las conexiones");
        }
    }

    [HttpGet("Ruta")]
    [ResponseCache(Duration = 300, Location = ResponseCacheLocation.Client)]
    [ProducesResponseType(
        typeof(RedElectricaGraphRouteResult),
        StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status404NotFound)]
    public async Task<ActionResult<RedElectricaGraphRouteResult>> Route(
        [FromQuery] string origin,
        [FromQuery] string destination,
        CancellationToken cancellationToken)
    {
        if (string.IsNullOrWhiteSpace(origin) ||
            string.IsNullOrWhiteSpace(destination))
        {
            return Problem(
                statusCode: StatusCodes.Status400BadRequest,
                title: "Origen y destino son obligatorios");
        }

        try
        {
            var result = await _service.FindRouteAsync(
                origin,
                destination,
                cancellationToken);
            return result is null
                ? Problem(
                    statusCode: StatusCodes.Status404NotFound,
                    title: "Origen o destino no encontrado")
                : Ok(result);
        }
        catch (OperationCanceledException) when (
            cancellationToken.IsCancellationRequested)
        {
            return new EmptyResult();
        }
        catch (Exception ex) when (IsDataSourceException(ex))
        {
            return GraphUnavailable(ex, "calcular la ruta eléctrica");
        }
    }

    [HttpGet("Revisiones")]
    [ResponseCache(Duration = 300, Location = ResponseCacheLocation.Client)]
    [ProducesResponseType(
        typeof(IReadOnlyList<RedElectricaGraphReview>),
        StatusCodes.Status200OK)]
    public async Task<ActionResult<IReadOnlyList<RedElectricaGraphReview>>> Reviews(
        [FromQuery] int limit = 500,
        CancellationToken cancellationToken = default)
    {
        if (limit is < 1 or > 5000)
        {
            return Problem(
                statusCode: StatusCodes.Status400BadRequest,
                title: "Límite no válido",
                detail: "El límite debe estar entre 1 y 5,000.");
        }

        try
        {
            var snapshot = await _service.GetAsync(false, cancellationToken);
            return Ok(snapshot.Reviews.Take(limit).ToList());
        }
        catch (OperationCanceledException) when (
            cancellationToken.IsCancellationRequested)
        {
            return new EmptyResult();
        }
        catch (Exception ex) when (IsDataSourceException(ex))
        {
            return GraphUnavailable(ex, "listar las revisiones");
        }
    }

    [HttpGet("GeoJson/Nodos")]
    [ResponseCache(Duration = 300, Location = ResponseCacheLocation.Client)]
    [Produces("application/geo+json", "application/json")]
    [ProducesResponseType(
        typeof(RedElectricaGeoJson),
        StatusCodes.Status200OK)]
    public async Task<ActionResult<RedElectricaGeoJson>> NodesGeoJson(
        [FromQuery] bool includeVirtual = true,
        CancellationToken cancellationToken = default)
    {
        try
        {
            return Ok(await _service.GetNodesGeoJsonAsync(
                includeVirtual,
                cancellationToken));
        }
        catch (OperationCanceledException) when (
            cancellationToken.IsCancellationRequested)
        {
            return new EmptyResult();
        }
        catch (Exception ex) when (IsDataSourceException(ex))
        {
            return GraphUnavailable(ex, "exportar los nodos GeoJSON");
        }
    }

    [HttpGet("GeoJson")]
    [ResponseCache(Duration = 300, Location = ResponseCacheLocation.Client)]
    [Produces("application/geo+json", "application/json")]
    [ProducesResponseType(
        typeof(RedElectricaGeoJson),
        StatusCodes.Status200OK)]
    public async Task<ActionResult<RedElectricaGeoJson>> GraphGeoJson(
        [FromQuery] bool includeVirtual = true,
        CancellationToken cancellationToken = default)
    {
        try
        {
            var nodes = await _service.GetNodesGeoJsonAsync(
                includeVirtual,
                cancellationToken);
            var edges = await _service.GetEdgesGeoJsonAsync(
                null,
                cancellationToken);
            return Ok(new RedElectricaGeoJson
            {
                Meta = new RedElectricaGeoJsonMeta
                {
                    Version = nodes.Meta.Version,
                    Layer = "grafo",
                    GeneratedUtc = nodes.Meta.GeneratedUtc,
                    Nodes = nodes.Meta.Nodes,
                    Edges = edges.Meta.Edges,
                    Reviews = nodes.Meta.Reviews,
                    Source = nodes.Meta.Source
                },
                Features = edges.Features.Concat(nodes.Features).ToList()
            });
        }
        catch (OperationCanceledException) when (
            cancellationToken.IsCancellationRequested)
        {
            return new EmptyResult();
        }
        catch (Exception ex) when (IsDataSourceException(ex))
        {
            return GraphUnavailable(ex, "exportar el grafo GeoJSON");
        }
    }

    [HttpGet("GeoJson/Aristas")]
    [ResponseCache(Duration = 300, Location = ResponseCacheLocation.Client)]
    [Produces("application/geo+json", "application/json")]
    [ProducesResponseType(
        typeof(RedElectricaGeoJson),
        StatusCodes.Status200OK)]
    public async Task<ActionResult<RedElectricaGeoJson>> EdgesGeoJson(
        [FromQuery] string? state = null,
        CancellationToken cancellationToken = default)
    {
        try
        {
            return Ok(await _service.GetEdgesGeoJsonAsync(
                state,
                cancellationToken));
        }
        catch (OperationCanceledException) when (
            cancellationToken.IsCancellationRequested)
        {
            return new EmptyResult();
        }
        catch (Exception ex) when (IsDataSourceException(ex))
        {
            return GraphUnavailable(ex, "exportar las aristas GeoJSON");
        }
    }

    [HttpGet("Pam/{projectId:long}")]
    [ResponseCache(Duration = 300, Location = ResponseCacheLocation.Client)]
    [Produces("application/geo+json", "application/json")]
    [ProducesResponseType(
        typeof(RedElectricaPamSubgraphResult),
        StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status404NotFound)]
    public async Task<ActionResult<RedElectricaPamSubgraphResult>> PamSubgraph(
        long projectId,
        [FromQuery] int depth = 1,
        CancellationToken cancellationToken = default)
    {
        if (depth is < 0 or > 5)
        {
            return Problem(
                statusCode: StatusCodes.Status400BadRequest,
                title: "Profundidad no válida",
                detail: "La profundidad permitida está entre 0 y 5 saltos.");
        }

        try
        {
            var result = await _service.GetPamSubgraphAsync(
                projectId,
                depth,
                cancellationToken);
            return result is null
                ? Problem(
                    statusCode: StatusCodes.Status404NotFound,
                    title: "Proyecto PAM no encontrado")
                : Ok(result);
        }
        catch (OperationCanceledException) when (
            cancellationToken.IsCancellationRequested)
        {
            return new EmptyResult();
        }
        catch (Exception ex) when (IsDataSourceException(ex))
        {
            return GraphUnavailable(ex, "construir el subgrafo PAM");
        }
    }

    private ObjectResult GraphUnavailable(Exception exception, string operation)
    {
        _logger.LogError(
            exception,
            "No fue posible {Operation} del grafo eléctrico.",
            operation);
        return Problem(
            statusCode: StatusCodes.Status503ServiceUnavailable,
            title: "Grafo eléctrico no disponible",
            detail: $"No fue posible {operation} en este momento.");
    }

    private static bool IsDataSourceException(Exception exception) =>
        exception is HttpRequestException or JsonException or SqlException;
}
