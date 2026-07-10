using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Http;
using Newtonsoft.Json;
using NSIE.Models;

namespace NSIE.Controllers
{
    public class ManualesController : Controller
    {
        public IActionResult Index()
        {
            var perfilJson = HttpContext.Session.GetString("PerfilUsuario");
            if (!string.IsNullOrEmpty(perfilJson))
            {
                var perfilUsuario = JsonConvert.DeserializeObject<PerfilUsuario>(perfilJson);
                ViewData["NombreUsuario"] = perfilUsuario.Nombre;
                ViewData["RolUsuario"] = perfilUsuario.Rol;
            }
            else
            {
                ViewData["NombreUsuario"] = "Invitado";
                ViewData["RolUsuario"] = "Público";
            }

            return View();
        }
    }
}
