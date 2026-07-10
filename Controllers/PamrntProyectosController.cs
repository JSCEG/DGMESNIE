using Microsoft.AspNetCore.Mvc;
using Newtonsoft.Json;
using NSIE.Models;
using NSIE.Servicios;

namespace NSIE.Controllers
{
    [ServiceFilter(typeof(ValidacionInputFiltro))]
    [AutorizacionFiltro]
    [Route("InformePormenorizado/ProyectosIdentificados")]
    public class PamrntProyectosController : Controller
    {
        private readonly IPamrntProyectosIdentificadosService _service;

        public PamrntProyectosController(IPamrntProyectosIdentificadosService service)
        {
            _service = service;
        }

        [HttpGet("")]
        public async Task<IActionResult> Index()
        {
            var model = await _service.ObtenerProyectosAsync();
            model.Header = BuildHeader();
            return View(model);
        }

        [HttpGet("Ficha/{clavePem}")]
        public async Task<IActionResult> Ficha(string clavePem)
        {
            var model = await _service.ObtenerFichaAsync(clavePem);
            if (model == null)
            {
                return NotFound();
            }

            model.Header = BuildHeader();
            return View(model);
        }

        private static HeaderViewModel BuildHeader()
        {
            var moduleInfo = new
            {
                title = "Proyectos identificados PAMRNT 2026-2040",
                description = "Universo separado para identificar los proyectos del nuevo ejercicio de planeación y comprobar su presencia en el Informe Pormenorizado.",
                stage = "Identificación y conciliación",
                functionality = "Lee una fuente versionada del PAMRNT y cruza las claves PEM contra la base vigente sin modificarla.",
                highlights = new[]
                {
                    "Ocho proyectos ordenados por prioridad",
                    "Cruce no destructivo con la base SQL",
                    "Ficha ejecutiva inicial de I26-PE1"
                }
            };

            return new HeaderViewModel
            {
                Title = "Proyectos Identificados",
                IconPath = "proyecto.png",
                Description = "PAMRNT 2026-2040: identificación, conciliación y fichas ejecutivas.",
                Section = "Informe Pormenorizado",
                ModuleInfo = JsonConvert.SerializeObject(moduleInfo)
            };
        }
    }
}
