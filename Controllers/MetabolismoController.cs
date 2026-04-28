using Microsoft.AspNetCore.Mvc;
using NSIE.Models;
using NSIE.Servicios;

namespace NSIE.Controllers
{
    [ServiceFilter(typeof(ValidacionInputFiltro))]
    [AutorizacionFiltro]
    public class Metabolismo : Controller
    {
        public IActionResult TasadeConsumoMetabolico()
        {
            return View();
        }
    }
}