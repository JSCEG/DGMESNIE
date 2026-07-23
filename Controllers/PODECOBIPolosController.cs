using Microsoft.AspNetCore.Mvc;
using NSIE.Models;
using NSIE.Servicios;
using Newtonsoft.Json;
using Newtonsoft.Json.Linq;

namespace NSIE.Controllers
{
    /// <summary>
    /// Endpoints de lectura para PODECOBI (alimenta el dashboard territorial
    /// y reemplaza la dependencia del CDN para datos estructurados del informe).
    /// </summary>
    [Route("api/podecobi")]
    [ApiController]
    public class PODECOBIPolosController : ControllerBase
    {
        private readonly IRepositorioPODECOBIPolos _repo;

        public PODECOBIPolosController(IRepositorioPODECOBIPolos repo)
        {
            _repo = repo;
        }

        /// <summary>Listado ligero para tablas, autocompletar, dashboard.</summary>
        [HttpGet("polos")]
        public async Task<ActionResult<List<PODECOBIPolo>>> Listar(
            [FromQuery] string? busqueda = null,
            [FromQuery] bool soloActivos = true)
        {
            var data = await _repo.ListarAsync(busqueda, soloActivos);
            return Ok(data);
        }

        /// <summary>Ficha completa de un polo: cabecera + vocaciones + contactos + fuentes + geometría.</summary>
        [HttpGet("polos/{numero}")]
        public async Task<ActionResult<PODECOBIPoloDetalle>> ObtenerPorNumero(string numero)
        {
            if (string.IsNullOrWhiteSpace(numero))
                return BadRequest(new { error = "numero requerido" });

            var det = await _repo.ObtenerPorNumeroAsync(numero);
            if (det == null) return NotFound(new { error = "Polo no encontrado", numero });

            return Ok(det);
        }

        /// <summary>Lookup por coordenada (clic en mapa). Usa bbox de centroide + tolerance.</summary>
        [HttpGet("polos/coords")]
        public async Task<ActionResult<PODECOBIPoloDetalle>> ObtenerPorCoordenada(
            [FromQuery] double lon, [FromQuery] double lat)
        {
            if (lon < -180 || lon > 180 || lat < -90 || lat > 90)
                return BadRequest(new { error = "coordenadas fuera de rango" });

            var det = await _repo.ObtenerPorCoordenadaAsync(lon, lat);
            if (det == null) return NotFound(new { error = "Sin polo cerca de la coordenada", lon, lat });

            return Ok(det);
        }

        /// <summary>KPI nacional (14 polos, superficie agregada, cuenta por etapa, último corte).</summary>
        [HttpGet("resumen")]
        public async Task<ActionResult<PODECOBIPoloResumen>> Resumen()
        {
            var r = await _repo.ObtenerResumenAsync();
            if (r == null) return Ok(new PODECOBIPoloResumen());
            return Ok(r);
        }

        /// <summary>Estados con conteo de polos (alimenta filtros del dashboard).</summary>
        [HttpGet("estados")]
        public async Task<ActionResult<List<PODECOBIEstadoConteo>>> Estados()
        {
            return Ok(await _repo.ListarEstadosAsync());
        }

        /// <summary>
        /// FeatureCollection GeoJSON para capa del mapa. Cada feature incluye:
        /// properties.Numero, properties.NombreOficial, properties.Estado, properties.Municipio,
        /// properties.AreaOficialHa, properties.AreaGeojsonHa, properties.Etapa, properties.Url.
        /// La geometría es la(s) del CDN original (almacenada en PODECOBI_Geometria.GeometryJson).
        /// </summary>
        [HttpGet("geojson")]
        public async Task<IActionResult> GeoJson()
        {
            var rows = await _repo.ListarComoGeoJsonAsync();
            var features = new JArray();

            foreach (var row in rows)
            {
                if (string.IsNullOrWhiteSpace(row.GeometriasJson)) continue;

                JArray? geoms;
                try { geoms = JArray.Parse(row.GeometriasJson); }
                catch { continue; }

                foreach (var g in geoms)
                {
                    var geomJson = (string?)g["GeometryJson"];
                    if (string.IsNullOrWhiteSpace(geomJson)) continue;

                    JObject? geom;
                    try { geom = JObject.Parse(geomJson); }
                    catch { continue; }

                    var props = new JObject
                    {
                        ["Numero"]         = row.Numero,
                        ["NombreOficial"]  = row.NombreOficial,
                        ["Estado"]         = row.Estado,
                        ["Municipio"]      = row.Municipio,
                        ["AreaOficialHa"]  = row.AreaOficialHa,
                        ["AreaGeojsonHa"]  = row.AreaGeojsonHa,
                        ["Etapa"]          = row.Etapa,
                        ["Url"]            = $"/api/podecobi/polos/{row.Numero}",
                        ["Fuente"]         = "DGMESNIE · DOF/SIDOF · corte " + (row.AreaOficialHa.HasValue ? "vigente" : "—")
                    };

                    features.Add(new JObject
                    {
                        ["type"] = "Feature",
                        ["geometry"] = geom,
                        ["properties"] = props
                    });
                }
            }

            var fc = new JObject
            {
                ["type"] = "FeatureCollection",
                ["features"] = features,
                ["metadata"] = new JObject
                {
                    ["generado"] = DateTime.UtcNow.ToString("o"),
                    ["polos"]    = rows.Count,
                    ["features"] = features.Count,
                    ["fuente"]   = "dgmesnie.PODECOBI_Polo + PODECOBI_Geometria"
                }
            };

            return Content(fc.ToString(Newtonsoft.Json.Formatting.None), "application/geo+json");
        }
    }
}
