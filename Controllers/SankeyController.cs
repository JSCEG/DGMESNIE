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
    public class SankeyController : Controller
    {
        private readonly IRepositorioSankey repositorioSankey;


        public SankeyController(IRepositorioSankey repositorioSankey)
        {

            this.repositorioSankey = repositorioSankey;
        }



        public IActionResult SankeySener()
        {
            return View();
        }
        public IActionResult SankeyPublico()
        {
            return View();
        }

        public async Task<IActionResult> Sankey()
        {
            try
            {
                var años = await repositorioSankey.ObtenerAños();

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

                var calificacion = await repositorioSankey.devuelveSankey(consultaSankey);

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

                var calificacion = await repositorioSankey.obtenerNodosxAño(nodosSankey);

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

                var calificacion = await repositorioSankey.devuelveNodosCajaSankey(nodosCajaSankey);

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

                var calificacion = await repositorioSankey.devuelveNodosSectores(nodosSectores);

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

                var calificacion = await repositorioSankey.devuelveNodosTransformaciones(nodosTransformaciones);

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

                var calificacion = await repositorioSankey.devuelveNodosTiposEnergia(nodosTiposEnergia);

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

                var calificacion = await repositorioSankey.devuelveNodosUsoFinal(nodosUsoFinal);

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

                var calificacion = await repositorioSankey.devuelveGrafica(nodosGrafica);

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

                var calificacion = await repositorioSankey.devuelveTablaFep(nodosTablaFep);

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

                var calificacion = await repositorioSankey.devuelveTablaSector(nodosTablaSector);

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

                var calificacion = await repositorioSankey.devuelveTablaTransformacion(nodosTablaTransformacion);

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

                var calificacion = await repositorioSankey.devuelveTablaTipos(nodosTablaTipos);

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

                var calificacion = await repositorioSankey.devuelveTablaUso(nodosTablaUso);

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

    }
}
