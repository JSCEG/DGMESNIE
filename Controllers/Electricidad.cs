using Microsoft.AspNetCore.Mvc;

namespace NSIE.Controllers
{
    [ServiceFilter(typeof(ValidacionInputFiltro))]
    [AutorizacionFiltro]
    public class Electricidad : Controller
    {
        public IActionResult E_TipoTec()
        {
            return View();
        }

        public IActionResult E_TipoGen()
        {
            return View();
        }
        public IActionResult E_RTTR()
        {
            return View();
        }
        public IActionResult E_RTTF()
        {
            return View();
        }

        public IActionResult E_SolarI()
        {
            return View();
        }

        public IActionResult E_Solar()
        {
            return View();
        }

        public IActionResult E_Viento()
        {
            return View();
        }
        public IActionResult E_VI()
        {
            return View();
        }
        public IActionResult E_Agua()
        {
            return View();
        }
        public IActionResult E_AguaI()
        {
            return View();
        }
    }
}
