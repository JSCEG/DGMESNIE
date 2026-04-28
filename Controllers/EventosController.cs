using Microsoft.AspNetCore.Mvc;
using Newtonsoft.Json.Linq;
using System.Net.Http;
using System.Net.Http.Headers;
using System.Threading.Tasks;

namespace NSIE.Controllers
{
    [ServiceFilter(typeof(ValidacionInputFiltro))]
    [AutorizacionFiltro]
    public class Eventos : Controller
    {
        public IActionResult Menu_Eventos()
        {
            return View();
        }

        public IActionResult E_Huracan_Eleven()
        {
            return View();
        }

        public IActionResult E_Huracan_Beryl()
        {
            return View();
        }

        public IActionResult E_Huracan_John()
        {
            return View();
        }

        public IActionResult E_Huracan_Rafael()
        {
            return View();
        }


        // Huracán Rafael - 2024
        public IActionResult Huracan_Rafael_2024()
        {
            return View();
        }

    }
}
