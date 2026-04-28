using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Rendering;
using Microsoft.Data.SqlClient;
using NSIE.Models;
using NSIE.Servicios;
using System.Security.Cryptography;
using System.Text;
using System.Data;
using System.Configuration;




namespace NSIE.Controllers
{
    [ServiceFilter(typeof(ValidacionInputFiltro))]
    [AutorizacionFiltro]
    public class MIMController : Controller
    {
        private readonly IRepositorioMIM repositorioMIM;


        public MIMController(IRepositorioMIM repositorioMIM)
        {

            this.repositorioMIM = repositorioMIM;
        }

        public IActionResult Reporte_Diario_PMLs()
        {
            return View();
        }

        public IActionResult Grafico_PMLS()
        {
            return View();
        }

        public IActionResult LineasTyS()
        {
            return View();
        }

        [HttpGet]
        public async Task<IActionResult> GetRD_PMLS()
        {
            try
            {
                var resultado = await repositorioMIM.EjecutarSP_GetRD_PMLS();
                return Json(resultado);
            }
            catch (Exception ex)
            {
                // Considera registrar el error
                Console.WriteLine(ex.ToString());
                return StatusCode(500, "Error interno del servidor");
            }
        }

        [HttpGet]
        public async Task<IActionResult> Grafico_PMLS(string claveProceso, string claveSistema, string nombreZonaCarga)
        {
            // Obtiene los datos filtrados por claveProceso, claveSistema y nombreZonaCarga
            var precios = await repositorioMIM.ObtenerPreciosPorFechas(claveProceso, claveSistema, nombreZonaCarga);

            if (precios == null || !precios.Any())
            {
                return NotFound("No se encontraron datos para la zona de carga especificada.");
            }

            return View(precios);
        }






        public async Task<IActionResult> ChequearConexion()
        {
            bool conexionExitosa = await repositorioMIM.ProbarConexion();
            return Json(conexionExitosa);
        }


        public IActionResult Reporte_Demanda()
        {
            return View();
        }


        //Demanda Diaria por GCR
        [HttpGet]
        public async Task<IActionResult> DemandaHistorica(DateTime inicio, DateTime fin)
        {
            try
            {
                var demandaHistorica = await repositorioMIM.ObtenerDemandaHistoricaAsync(inicio, fin, "MDA");

                if (demandaHistorica == null || !demandaHistorica.Any())
                {
                    return Json(new { success = false, message = "No se encontraron datos." });
                }

                // Transformar los datos para que cada área sea una serie
                var resultado = new
                {
                    fechas = demandaHistorica.Select(d => d.Fecha.ToString("yyyy-MM-dd")).ToList(),
                    series = new List<object>
            {
                new { name = "CEN", data = demandaHistorica.Select(d => d.CEN).ToList() },
                new { name = "ORI", data = demandaHistorica.Select(d => d.ORI).ToList() },
                new { name = "OCC", data = demandaHistorica.Select(d => d.OCC).ToList() },
                new { name = "NOR", data = demandaHistorica.Select(d => d.NOR).ToList() },
                new { name = "NTE", data = demandaHistorica.Select(d => d.NTE).ToList() },
                new { name = "NES", data = demandaHistorica.Select(d => d.NES).ToList() },
                new { name = "BCA", data = demandaHistorica.Select(d => d.BCA).ToList() },
                new { name = "BCS", data = demandaHistorica.Select(d => d.BCS).ToList() },
                new { name = "PEN", data = demandaHistorica.Select(d => d.PEN).ToList() }
            }
                };

                return Json(resultado);
            }
            catch (Exception ex)
            {
                Console.WriteLine(ex.ToString());
                return StatusCode(500, "Error interno del servidor");
            }
        }




    }
}
