using Microsoft.AspNetCore.Mvc.Filters;
using Microsoft.AspNetCore.Mvc;
using Newtonsoft.Json;
using NSIE.Models;
using Microsoft.Extensions.Configuration;
using System;

public class AutorizacionFiltro : ActionFilterAttribute
{
    public override void OnActionExecuting(ActionExecutingContext context)
    {
        // Log para confirmar que el filtro se ejecuta
        Console.WriteLine("Filtro AutorizacionFiltro ejecutado");

        var serviceProvider = context.HttpContext.RequestServices;
        var configuration = serviceProvider.GetRequiredService<IConfiguration>();

        var session = context.HttpContext.Session;
        var usuarioLogeado = !string.IsNullOrEmpty(session.GetString("PerfilUsuario"));

        if (!usuarioLogeado)
        {
            // Si el usuario no está logueado, redirigir a la página de sesión expirada
            context.Result = new RedirectToActionResult("SesionExpirada", "Acceso", null);
        }
        else
        {
            // Obtener información del usuario desde la sesión
            var perfilUsuarioJson = session.GetString("PerfilUsuario");
            var perfilUsuario = JsonConvert.DeserializeObject<PerfilUsuario>(perfilUsuarioJson);

            // Opcional: verificar roles o permisos adicionales según sea necesario
            Console.WriteLine($"Usuario logueado: {perfilUsuario.Nombre}");

            // Continuar con la ejecución normal de la acción
        }
    }
}
