using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using NSIE.Servicios;

namespace NSIE.Controllers;

[ApiController]
[AllowAnonymous]
[Route("DashboardProyectos/PermisosEnergeticos/Tablas")]
[Produces("application/json")]
public sealed class TablasPermisosCneController : ControllerBase
{
    private readonly RepositorioTablasPermisosCne _repository;
    private readonly ILogger<TablasPermisosCneController> _logger;

    public TablasPermisosCneController(RepositorioTablasPermisosCne repository,
        ILogger<TablasPermisosCneController> logger)
    {
        _repository = repository;
        _logger = logger;
    }

    [HttpGet("Lista")]
    public Task<IActionResult> Lista([FromQuery] string? tabla, [FromQuery] string? estatus,
        [FromQuery] string? ef, [FromQuery] string? tipo, CancellationToken cancellationToken) =>
        EjecutarAsync(tabla, async fuente => new
        {
            tabla, filas = await _repository.ListaAsync(fuente, estatus, ef, tipo, cancellationToken)
        }, cancellationToken, validarFiltros: true, estatus: estatus, ef: ef, tipo: tipo);

    [HttpGet("Estados")]
    public Task<IActionResult> Estados([FromQuery] string? tabla, CancellationToken cancellationToken) =>
        EjecutarAsync(tabla, async fuente => new
        {
            tabla, estados = await _repository.EstadosAsync(fuente, cancellationToken)
        }, cancellationToken);

    [HttpGet("Puntos")]
    public Task<IActionResult> Puntos([FromQuery] string? tabla, CancellationToken cancellationToken) =>
        EjecutarAsync(tabla, async fuente => new
        {
            tabla, puntos = await _repository.PuntosAsync(fuente, cancellationToken)
        }, cancellationToken);

    [HttpGet("Actualizacion")]
    public Task<IActionResult> Actualizacion([FromQuery] string? tabla, CancellationToken cancellationToken) =>
        EjecutarAsync(tabla, async fuente =>
        {
            var fila = await _repository.ActualizacionAsync(fuente, cancellationToken);
            if (fila is null) return new Dictionary<string, object?> { ["tabla"] = tabla, ["disponible"] = false };
            fila["tabla"] = tabla;
            fila["disponible"] = true;
            return fila;
        }, cancellationToken);

    private async Task<IActionResult> EjecutarAsync(string? tabla, Func<TablaPermisosCne, Task<object>> consultar,
        CancellationToken cancellationToken, bool validarFiltros = false,
        string? estatus = null, string? ef = null, string? tipo = null)
    {
        var fuente = TablaPermisosCne.Resolver(tabla);
        if (fuente is null || Request.Query["tabla"].Count != 1 ||
            (validarFiltros && !FiltrosValidos(estatus, ef, tipo)))
            return BadRequest(new { mensaje = "Tabla o parámetros no válidos." });

        try
        {
            return Ok(await consultar(fuente));
        }
        catch (OperationCanceledException) when (cancellationToken.IsCancellationRequested)
        {
            return new EmptyResult();
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Falló la consulta de tablas de permisos CNE para {Tabla}.", tabla);
            return StatusCode(StatusCodes.Status500InternalServerError,
                new { mensaje = "No fue posible consultar los permisos." });
        }
    }

    private bool FiltrosValidos(string? estatus, string? ef, string? tipo)
    {
        if (new[] { "estatus", "ef", "tipo" }.Any(clave => Request.Query[clave].Count > 1)) return false;
        if (Request.Query.ContainsKey("estatus") && estatus is not ("Vigente" or "No vigente")) return false;
        if (Request.Query.ContainsKey("ef") &&
            (ef is null || ef.Length != 2 || ef.Any(c => c < '0' || c > '9'))) return false;
        if (Request.Query.ContainsKey("tipo") &&
            (string.IsNullOrWhiteSpace(tipo) || tipo.Length > 400 || tipo.Any(char.IsControl))) return false;
        return true;
    }
}
