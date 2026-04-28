using Microsoft.AspNetCore.Mvc;
using NSIE.Servicios;
using System.Threading.Tasks;
using System.Data;

namespace NSIE.Controllers
{
    [ServiceFilter(typeof(ValidacionInputFiltro))]
    [AutorizacionFiltro]


    public class EstacionesdeCarga : Controller
    {
        public IActionResult Electrolineras()
        {
            return View();
        }
    }

}
