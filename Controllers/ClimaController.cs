using Microsoft.AspNetCore.Mvc;
using NSIE.Servicios;

namespace NSIE.Controllers;

/// <summary>
/// Capas de clima del mapa de infraestructura: huracanes activos (NOAA NHC) y viento
/// en superficie (Open-Meteo). Proxy con caché; ver <see cref="ClimaServicio"/>.
/// </summary>
[AutorizacionFiltro]
[Route("Clima")]
public sealed class ClimaController : Controller
{
    private readonly IClimaServicio _clima;

    public ClimaController(IClimaServicio clima)
    {
        _clima = clima;
    }

    [HttpGet("Huracanes")]
    [ResponseCache(Duration = 300, Location = ResponseCacheLocation.Client)]
    public async Task<IActionResult> Huracanes(CancellationToken cancellationToken)
    {
        var data = await _clima.ObtenerHuracanesAsync(cancellationToken);
        return Content(data.ToJsonString(), "application/json");
    }

    [HttpGet("Viento")]
    [ResponseCache(Duration = 300, Location = ResponseCacheLocation.Client)]
    public async Task<IActionResult> Viento(
        [FromQuery] double minLat,
        [FromQuery] double minLon,
        [FromQuery] double maxLat,
        [FromQuery] double maxLon,
        CancellationToken cancellationToken)
    {
        if (!double.IsFinite(minLat) || !double.IsFinite(minLon) || !double.IsFinite(maxLat) || !double.IsFinite(maxLon) ||
            minLat >= maxLat || minLon >= maxLon || minLat < -90 || maxLat > 90 || minLon < -180 || maxLon > 180)
        {
            return BadRequest(new { mensaje = "La caja geográfica no es válida." });
        }

        var data = await _clima.ObtenerVientoAsync(minLat, minLon, maxLat, maxLon, cancellationToken);
        return Content(data.ToJsonString(), "application/json");
    }
}
