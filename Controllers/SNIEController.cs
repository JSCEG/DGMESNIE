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


    public class SNIEController : Controller
    {

        public IActionResult Escenarios()
        {
            return View();
        }
        public IActionResult FOTEASE()
        {
            return View();
        }


        public IActionResult FSUE()
        {
            return View();
        }


        public IActionResult Fondos_i_D()
        {
            return View();
        }
        public IActionResult EquiposE()
        {
            return View();
        }
        public IActionResult DesempeñoMIM()
        {
            return View();
        }
        public IActionResult AltaColaboradores()
        {
            return View();
        }
        public IActionResult Inventarios()
        {
            return View();
        }
        public IActionResult CargaInformacion()
        {
            return View();
        }

        public IActionResult ValidacionFirma()
        {
            return View();
        }

        public IActionResult Semaforo()
        {
            return View();
        }
        public IActionResult BuzonNotificaciones()
        {
            // Aquí implementarías la lógica para obtener las notificaciones
            // desde tu base de datos o servicio
            return View();
        }

        public IActionResult BalanceNacional()
        {
            // Aquí implementarías la lógica para obtener las notificaciones
            // desde tu base de datos o servicio
            return View();
        }
        public IActionResult SubsistemasExternos()
        {

            return View();
        }

        public IActionResult OrigenyDestinoE()
        {
            return View();
        }

        public IActionResult InformacionIntegralProyectos()
        {
            return View();
        }
        public IActionResult InformacionTecnicaEconomica()
        {
            return View();
        }

        public IActionResult Combustibles()
        {
            return View();
        }

        public IActionResult Equipos()
        {
            return View();
        }

        public IActionResult Instalaciones()
        {
            return View();
        }

        public IActionResult Ahorros()
        {
            return View();
        }

        public IActionResult CTG()
        {
            return View();
        }
        public IActionResult Otros()
        {
            return View();
        }
        public IActionResult Normalizacion()
        {
            return View();
        }
        public IActionResult Publicacion()
        {
            return View();
        }
        public IActionResult IMP()
        {
            return View();
        }
        public IActionResult CargaD()
        {
            return View();
        }
        public IActionResult RevisionS()
        {
            return View();
        }
        public IActionResult EnvioS()
        {
            return View();
        }
        public IActionResult EscenariosMLP()
        {
            return View();
        }

        public IActionResult CatalogoTG()
        {
            return View();
        }

        public IActionResult AvanceProyectos()
        {
            var datosDemo = new
            {
                TotalProyectos = 245,
                InversionTotal = 8500, // Millones USD
                AvancePromedio = 65,
                ProyectosActivos = 180
            };

            return View(datosDemo);
        }

        public IActionResult CertificadosLimpios()
        {
            var datosDemo = new
            {
                TotalCELs = 12500000,
                CertificadosEmitidos = 8750000,
                ValorPromedio = 25.80M,
                PorcentajeCumplimiento = 85.5
            };

            return View(datosDemo);
        }
    }


}
