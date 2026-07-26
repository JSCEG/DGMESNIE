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
        private const double GeometryAssociationToleranceKm = 100;
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

                var parsedGeometries = new List<(JObject Geometry, double DistanceKm)>();
                foreach (var g in geoms)
                {
                    var geomJson = (string?)g["GeometryJson"];
                    if (string.IsNullOrWhiteSpace(geomJson)) continue;

                    JObject? geom;
                    try { geom = JObject.Parse(geomJson); }
                    catch { continue; }

                    var distanceKm = row.Lon.HasValue && row.Lat.HasValue
                        ? DistanceToGeometryBoundsKm(
                            (double)row.Lon.Value,
                            (double)row.Lat.Value,
                            geom)
                        : 0;
                    parsedGeometries.Add((geom, distanceKm));
                }

                // La tabla histórica puede contener, bajo un mismo número, una geometría
                // remota que no corresponde al centroide oficial del polo. Conservamos
                // todas las partes cercanas (un polo sí puede ser multiparte) y evitamos
                // que un fragmento huérfano expanda el análisis y el reporte a otra región.
                var selectedGeometries = row.Lon.HasValue && row.Lat.HasValue
                    ? parsedGeometries
                        .Where(item => item.DistanceKm <= GeometryAssociationToleranceKm)
                        .ToList()
                    : parsedGeometries;
                if (selectedGeometries.Count == 0 && parsedGeometries.Count > 0)
                {
                    selectedGeometries.Add(parsedGeometries.MinBy(item => item.DistanceKm));
                }

                foreach (var item in selectedGeometries)
                {
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
                        ["geometry"] = item.Geometry,
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

        private static double DistanceToGeometryBoundsKm(double lon, double lat, JObject geometry)
        {
            var minLon = double.PositiveInfinity;
            var minLat = double.PositiveInfinity;
            var maxLon = double.NegativeInfinity;
            var maxLat = double.NegativeInfinity;
            AccumulateBounds(geometry["coordinates"], ref minLon, ref minLat, ref maxLon, ref maxLat);

            if (!double.IsFinite(minLon) || !double.IsFinite(minLat))
                return double.PositiveInfinity;

            var nearestLon = Math.Clamp(lon, minLon, maxLon);
            var nearestLat = Math.Clamp(lat, minLat, maxLat);
            return HaversineKm(lat, lon, nearestLat, nearestLon);
        }

        private static void AccumulateBounds(
            JToken? token,
            ref double minLon,
            ref double minLat,
            ref double maxLon,
            ref double maxLat)
        {
            if (token is not JArray array) return;

            if (array.Count >= 2 &&
                array[0]?.Type is JTokenType.Float or JTokenType.Integer &&
                array[1]?.Type is JTokenType.Float or JTokenType.Integer)
            {
                var lon = (double)array[0]!;
                var lat = (double)array[1]!;
                minLon = Math.Min(minLon, lon);
                minLat = Math.Min(minLat, lat);
                maxLon = Math.Max(maxLon, lon);
                maxLat = Math.Max(maxLat, lat);
                return;
            }

            foreach (var child in array)
                AccumulateBounds(child, ref minLon, ref minLat, ref maxLon, ref maxLat);
        }

        private static double HaversineKm(double lat1, double lon1, double lat2, double lon2)
        {
            const double EarthRadiusKm = 6371;
            static double ToRadians(double value) => value * Math.PI / 180;

            var dLat = ToRadians(lat2 - lat1);
            var dLon = ToRadians(lon2 - lon1);
            var a = Math.Pow(Math.Sin(dLat / 2), 2) +
                    Math.Cos(ToRadians(lat1)) * Math.Cos(ToRadians(lat2)) *
                    Math.Pow(Math.Sin(dLon / 2), 2);
            return 2 * EarthRadiusKm * Math.Asin(Math.Sqrt(a));
        }
    }
}
