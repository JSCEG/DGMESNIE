using Microsoft.AspNetCore.Mvc;
using NSIE.Servicios;
using System.Threading.Tasks;
using System.Data;

namespace NSIE.Controllers
{
    [ServiceFilter(typeof(ValidacionInputFiltro))]
    [AutorizacionFiltro]


    public class ErrorController : Controller
    {
        public IActionResult AccesoDenegado()
        {
            return View();
        }
        public IActionResult ActividadSospechosa()
        {
            return View();
        }
    }

}
