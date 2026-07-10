using Microsoft.AspNetCore.Mvc;
using NSIE.Models;
using Newtonsoft.Json;

namespace NSIE.Controllers
{
    [ServiceFilter(typeof(ValidacionInputFiltro))]
    [AutorizacionFiltro]
    public class SideoController : Controller
    {
        [HttpGet]
        public IActionResult Index()
        {
            ViewData["HeaderViewModel"] = new HeaderViewModel
            {
                Title = "Sistema de Diseño SIDEO · Especificaciones",
                IconPath = "organigrama.png",
                Description = "Especímen interactivo y documentación visual de los tokens de color, tipografía, botones, listas, cheurones y separadores de la línea editorial.",
                Section = "Diseño",
                ModuleInfo = JsonConvert.SerializeObject(new
                {
                    title = "Sistema de Diseño SIDEO · Guía Visual",
                    description = "Concentrado de elementos gráficos y guías tipográficas homologadas de la Secretaría de Energía.",
                    functionality = "Visualización de paletas de color, jerarquía de fuentes, tipos de botones y componentes interactivos.",
                    stage = "Línea Editorial",
                    highlights = new[]
                    {
                        "Uso de Newsreader, Libre Franklin e IBM Plex Mono.",
                        "Componentes UI homologados con radio y sombras suaves.",
                        "Estructura responsiva de láminas 16:9."
                    }
                })
            };
            return View();
        }

        [HttpGet]
        public IActionResult Presentacion()
        {
            ViewData["HeaderViewModel"] = new HeaderViewModel
            {
                Title = "Sistema de Diseño SIDEO · Presentaciones",
                IconPath = "organigrama.png",
                Description = "Plantillas de láminas en formato 16:9 homologadas según el Plan de Expansión del Sistema Eléctrico Nacional 2024–2030.",
                Section = "Diseño",
                ModuleInfo = JsonConvert.SerializeObject(new
                {
                    title = "Plantilla de Diapositivas 16:9",
                    description = "Estructura de presentaciones oficiales con logos institucionales, kickers, títulos y áreas de contenido.",
                    functionality = "Navegación secuencial de láminas de muestra (Portada, Sección, Datos).",
                    stage = "Línea Editorial",
                    highlights = new[]
                    {
                        "Diseño a sangre con filetes dorados ornamentales.",
                        "KPIs integrados con reglas de acento de tecnologías.",
                        "Tablas de datos y gráficos alineados visualmente."
                    }
                })
            };
            return View();
        }
    }
}
