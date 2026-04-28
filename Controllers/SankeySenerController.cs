using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Rendering;
using Microsoft.Data.SqlClient;
using NSIE.Models;
using NSIE.Servicios;
using System.Security.Cryptography;
using System.Text;
using System.Data;




namespace NSIE.Controllers
{
    [ServiceFilter(typeof(ValidacionInputFiltro))]
    [AutorizacionFiltro]
    public class SankeySenerController : Controller
    {
        private readonly IRepositorioSankeySener repositorioSankeySener;


        public SankeySenerController(IRepositorioSankeySener repositorioSankeySener)
        {

            this.repositorioSankeySener = repositorioSankeySener;
        }



        public async Task<IActionResult> SankeySener()
        {
            try
            {
                //Obtener los años disponibles desde el repositorio
                var años = await repositorioSankeySener.ObtenerAños();

                // Pasar los años a la vista
                ViewBag.Years = años;
            }
            catch (Exception)
            {
                // Manejar excepciones
                // Puedes decidir qué hacer si hay un error, como mostrar un mensaje de error
            }
            return View();
        }

        public async Task<IActionResult> Sankey()
        {
            try
            {
                var años = await repositorioSankeySener.ObtenerAños();

                // Pasar los años a la vista
                ViewBag.Years = años;
            }
            catch (Exception)
            {
                // Manejar excepciones
                // Puedes decidir qué hacer si hay un error, como mostrar un mensaje de error
            }

            return View();
        }

        [HttpPost]
        public async Task<IActionResult> consulta_Sankey([FromBody] ConsultaSankey consultaSankey)
        {
            try
            {

                var calificacion = await repositorioSankeySener.devuelveSankey(consultaSankey);

                if (calificacion == null)
                {
                    return NotFound(); // Manejar el caso en que no se encuentre el año
                }

                return Json(calificacion);
            }
            catch (Exception ex)
            {
                // Excepción si es necesario
                return StatusCode(500, new { message = ex.Message });
            }
        }



        [HttpPost]
        public async Task<IActionResult> obtieneNodos([FromBody] NodosSankey nodosSankey)
        {
            try
            {

                var calificacion = await repositorioSankeySener.obtenerNodosxAño(nodosSankey);

                if (calificacion == null)
                {
                    return NotFound(); // Manejar el caso en que no se encuentre el año
                }

                return Json(calificacion);
            }
            catch (Exception ex)
            {
                // Excepción si es necesario
                return StatusCode(500, new { message = ex.Message });
            }
        }

        //Nodos Caja
        public async Task<IActionResult> nodoscaja(NodosCajaSankey nodosCajaSankey)
        {
            try
            {

                var calificacion = await repositorioSankeySener.devuelveNodosCajaSankey(nodosCajaSankey);

                if (calificacion == null)
                {
                    return NotFound(); // Manejar el caso en que no se encuentre el año
                }

                return Json(calificacion);
            }
            catch (Exception ex)
            {
                // Excepción si es necesario
                return StatusCode(500, new { message = ex.Message });
            }
        }

        [HttpPost]
        public async Task<IActionResult> nodossectores([FromBody] NodosSectores nodosSectores)
        {
            try
            {

                var calificacion = await repositorioSankeySener.devuelveNodosSectores(nodosSectores);

                if (calificacion == null)
                {
                    return NotFound(); // Manejar el caso en que no se encuentre el año
                }

                return Json(calificacion);
            }
            catch (Exception ex)
            {
                // Excepción si es necesario
                return StatusCode(500, new { message = ex.Message });
            }
        }

        [HttpPost]
        public async Task<IActionResult> nodostransformaciones([FromBody] NodosTransformaciones nodosTransformaciones)
        {
            try
            {

                var calificacion = await repositorioSankeySener.devuelveNodosTransformaciones(nodosTransformaciones);

                if (calificacion == null)
                {
                    return NotFound(); // Manejar el caso en que no se encuentre el año
                }

                return Json(calificacion);
            }
            catch (Exception ex)
            {
                // Excepción si es necesario
                return StatusCode(500, new { message = ex.Message });
            }
        }

        [HttpPost]
        public async Task<IActionResult> nodostiposenergia([FromBody] NodosTiposEnergia nodosTiposEnergia)
        {
            try
            {

                var calificacion = await repositorioSankeySener.devuelveNodosTiposEnergia(nodosTiposEnergia);

                if (calificacion == null)
                {
                    return NotFound(); // Manejar el caso en que no se encuentre el año
                }

                return Json(calificacion);
            }
            catch (Exception ex)
            {
                // Excepción si es necesario
                return StatusCode(500, new { message = ex.Message });
            }
        }

        [HttpPost]
        public async Task<IActionResult> nodosusofinal([FromBody] NodosUsoFinal nodosUsoFinal)
        {
            try
            {

                var calificacion = await repositorioSankeySener.devuelveNodosUsoFinal(nodosUsoFinal);

                if (calificacion == null)
                {
                    return NotFound(); // Manejar el caso en que no se encuentre el año
                }

                return Json(calificacion);
            }
            catch (Exception ex)
            {
                // Excepción si es necesario
                return StatusCode(500, new { message = ex.Message });
            }
        }

        public async Task<IActionResult> nodosgrafica(NodosGrafica nodosGrafica)
        {
            try
            {

                var calificacion = await repositorioSankeySener.devuelveGrafica(nodosGrafica);

                if (calificacion == null)
                {
                    return NotFound(); // Manejar el caso en que no se encuentre el año
                }

                return Json(calificacion);
            }
            catch (Exception ex)
            {
                // Excepción si es necesario
                return StatusCode(500, new { message = ex.Message });
            }
        }

        public async Task<IActionResult> nodostablafep(NodosTablaFep nodosTablaFep)
        {
            try
            {

                var calificacion = await repositorioSankeySener.devuelveTablaFep(nodosTablaFep);

                if (calificacion == null)
                {
                    return NotFound(); // Manejar el caso en que no se encuentre el año
                }

                return Json(calificacion);
            }
            catch (Exception ex)
            {
                // Excepción si es necesario
                return StatusCode(500, new { message = ex.Message });
            }
        }

        public async Task<IActionResult> nodostablasector(NodosTablaSector nodosTablaSector)
        {
            try
            {

                var calificacion = await repositorioSankeySener.devuelveTablaSector(nodosTablaSector);

                if (calificacion == null)
                {
                    return NotFound(); // Manejar el caso en que no se encuentre el año
                }

                return Json(calificacion);
            }
            catch (Exception ex)
            {
                // Excepción si es necesario
                return StatusCode(500, new { message = ex.Message });
            }
        }

        public async Task<IActionResult> nodostablatransformacion(NodosTablaTransformacion nodosTablaTransformacion)
        {
            try
            {

                var calificacion = await repositorioSankeySener.devuelveTablaTransformacion(nodosTablaTransformacion);

                if (calificacion == null)
                {
                    return NotFound(); // Manejar el caso en que no se encuentre el año
                }

                return Json(calificacion);
            }
            catch (Exception ex)
            {
                // Excepción si es necesario
                return StatusCode(500, new { message = ex.Message });
            }
        }

        public async Task<IActionResult> nodostablatipos(NodosTablaTipos nodosTablaTipos)
        {
            try
            {

                var calificacion = await repositorioSankeySener.devuelveTablaTipos(nodosTablaTipos);

                if (calificacion == null)
                {
                    return NotFound(); // Manejar el caso en que no se encuentre el año
                }

                return Json(calificacion);
            }
            catch (Exception ex)
            {
                // Excepción si es necesario
                return StatusCode(500, new { message = ex.Message });
            }
        }

        public async Task<IActionResult> nodostablauso(NodosTablaUso nodosTablaUso)
        {
            try
            {

                var calificacion = await repositorioSankeySener.devuelveTablaUso(nodosTablaUso);

                if (calificacion == null)
                {
                    return NotFound(); // Manejar el caso en que no se encuentre el año
                }

                return Json(calificacion);
            }
            catch (Exception ex)
            {
                // Excepción si es necesario
                return StatusCode(500, new { message = ex.Message });
            }
        }

        //NUEVO SANKEY
        public async Task<IActionResult> BalanceNacionalEnergia()
        {
            return View();
        }

        public async Task<IActionResult> BNE()
        {
            return View();
        }

        public async Task<IActionResult> GraficosBNE()
        {
            return View();
        }

        public async Task<IActionResult> FlujoEnergia()
        {
            return View();
        }

        [HttpGet]
        public async Task<IActionResult> GetEnergyData()
        {
            var rows = await repositorioSankeySener.ObtenerEnergyDataAsync();

            var result = new Dictionary<int, dynamic>();

            foreach (var row in rows)
            {
                if (!result.ContainsKey(row.ParentId))
                {
                    result[row.ParentId] = new Dictionary<string, object>
                    {
                        ["Nodo Padre"] = row.ParentName,
                        ["Nodos Hijo"] = new Dictionary<int, Dictionary<string, object>>(),
                        ["descripcion"] = row.ParentDescription,
                        ["id_padre"] = row.ParentId,
                        ["color"] = row.ParentColor
                    };
                }

                var parent = (Dictionary<string, object>)result[row.ParentId];
                var hijos = (Dictionary<int, Dictionary<string, object>>)parent["Nodos Hijo"];

                if (!hijos.ContainsKey(row.ChildId))
                {
                    hijos[row.ChildId] = new Dictionary<string, object>
                    {
                        ["Nodo Hijo"] = row.ChildName,
                        ["tipo"] = row.Tipo,
                        ["descripcion"] = row.ChildDescription,
                        ["id_hijo"] = row.ChildId,
                        ["color"] = row.ChildColor
                    };
                }

                hijos[row.ChildId][row.Year.ToString()] = row.Value;
            }

            return Json(new
            {
                Datos = result.Values.Select(p => new
                {
                    NodoPadre = p["Nodo Padre"],
                    NodosHijo = ((Dictionary<int, Dictionary<string, object>>)p["Nodos Hijo"]).Values,
                    descripcion = p["descripcion"],
                    id_padre = p["id_padre"],
                    color = p["color"]
                })
            });
        }

        [HttpGet]
        public async Task<IActionResult> GetEnergeticColor()
        {
            var colors = await repositorioSankeySener.ObtenerColorDataAsync();

            return Json(colors);
        }

        [HttpPost]
        public async Task<IActionResult> InsertData([FromBody] EnergyRequest request)
        {
            Console.WriteLine("Datos recibidos en el controlador:");

            await repositorioSankeySener.UpsertEnergyDataAsync(request.Datos);

            return Ok(new { success = true });
        }

    }
}
