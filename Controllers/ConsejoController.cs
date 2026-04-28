using Microsoft.AspNetCore.Mvc;
using NSIE.Models;
using NSIE.Servicios;
using System.Security.Cryptography;
using System.Text;
using System.Threading.Tasks;
using Microsoft.Data.SqlClient;
using Newtonsoft.Json;
using Dapper;
using System.Data;


namespace NSIE.Controllers
{
    [ServiceFilter(typeof(ValidacionInputFiltro))]
    public class ConsejoController : Controller
    {
        // GET: /Consejo/ProgramaAnual
        public IActionResult ProgramaAnual()
        {
            return View();
        }

        // GET: /Consejo/AvanceMetas
        public IActionResult AvanceMetas()
        {
            return View();
        }

        // GET: /Consejo/OrdenDelDia
        public IActionResult OrdenDelDia()
        {
            return View();
        }

        // GET: /Consejo/Votaciones
        public IActionResult Votaciones()
        {
            return View();
        }

        // GET: /Consejo/FirmaActas
        public IActionResult FirmaActas()
        {
            return View();
        }

        // GET: /Consejo/Acuerdos
        public IActionResult AcuerdosCoordinacion()
        {
            return View();
        }

        // GET: /Consejo/BuzonAsuntos
        public IActionResult BuzonAsuntos()
        {
            return View();
        }

        // GET: /Consejo/Indicadores
        public IActionResult IndicadoresConsejo()
        {
            return View();
        }

        // GET: /Consejo/TrazabilidadAcuerdos
        public IActionResult TrazabilidadAcuerdos()
        {
            return View();
        }

        // GET: /Consejo/RepositorioSesiones  
        public IActionResult RepositorioSesiones()
        {
            return View();
        }

        // GET: /Consejo/ConsultasPublicas
        public IActionResult ConsultasPublicas()
        {
            return View();
        }

        // GET: /Consejo/Integrantes
        public IActionResult IntegrantesRoles()
        {
            return View();
        }

        // GET: /Consejo/AsesoriaTecnica
        public IActionResult AsesoriaTecnica()
        {
            return View();
        }
    }
}


