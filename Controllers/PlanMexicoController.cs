using Microsoft.AspNetCore.Mvc;
using NSIE.Servicios;
using System.Threading.Tasks;
using System.Data;

namespace NSIE.Controllers
{
    [ServiceFilter(typeof(ValidacionInputFiltro))]
    [AutorizacionFiltro]


    public class PlanMexicoController : Controller
    {
        public IActionResult Plan_Carretero()
        {
            return View();
        }
        public IActionResult Plan_Centrales_EPyPB()
        {
            return View();
        }
        public IActionResult Plan_GLP()
        {
            return View();
        }
        public IActionResult Plan_Generacion()
        {
            return View();
        }
        public IActionResult Plan_Transmision()
        {
            return View();
        }
        public IActionResult Plan_Petroliferos()
        {
            return View();
        }
        public IActionResult Plan_Polos()
        {
            return View();
        }
    }

}
