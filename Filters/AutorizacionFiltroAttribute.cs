using Microsoft.AspNetCore.Mvc.Filters;
using Microsoft.AspNetCore.Mvc;
using Newtonsoft.Json;
using NSIE.Models;
using Microsoft.Extensions.Configuration;
using System;

public class AutorizacionFiltro : ActionFilterAttribute
{
    private static readonly HashSet<string> PublicAccesoActions = new(StringComparer.OrdinalIgnoreCase)
    {
        "Login",
        "LoginGoogle",
        "LoginFacebook",
        "GoogleResponse",
        "FacebookResponse",
        "ForgotPassword",
        "ResetPassword",
        "ResetPasswordUser",
        "SesionExpirada",
        "ActividadSospechosa",
        "Logout",
        "Heartbeat",
        "ActualizarInicioSesion"
    };

    public override void OnActionExecuting(ActionExecutingContext context)
    {
        // Log para confirmar que el filtro se ejecuta
        Console.WriteLine("Filtro AutorizacionFiltro ejecutado");

        var requestPath = context.HttpContext.Request.Path.ToString();
        if (!string.IsNullOrWhiteSpace(requestPath) &&
            (requestPath.StartsWith("/Acceso/Login", StringComparison.OrdinalIgnoreCase)
             || requestPath.StartsWith("/Acceso/ForgotPassword", StringComparison.OrdinalIgnoreCase)
             || requestPath.StartsWith("/Acceso/ResetPassword", StringComparison.OrdinalIgnoreCase)
             || requestPath.StartsWith("/Acceso/SesionExpirada", StringComparison.OrdinalIgnoreCase)
             || requestPath.StartsWith("/Acceso/ActividadSospechosa", StringComparison.OrdinalIgnoreCase)
             || requestPath.StartsWith("/Acceso/Heartbeat", StringComparison.OrdinalIgnoreCase)
             || requestPath.StartsWith("/Acceso/ActualizarInicioSesion", StringComparison.OrdinalIgnoreCase)
             || requestPath.StartsWith("/Acceso/GoogleResponse", StringComparison.OrdinalIgnoreCase)
             || requestPath.StartsWith("/Acceso/FacebookResponse", StringComparison.OrdinalIgnoreCase)
             || requestPath.StartsWith("/Acceso/LoginGoogle", StringComparison.OrdinalIgnoreCase)
             || requestPath.StartsWith("/Acceso/LoginFacebook", StringComparison.OrdinalIgnoreCase)))
        {
            base.OnActionExecuting(context);
            return;
        }

        var controller = context.RouteData.Values["controller"]?.ToString() ?? string.Empty;
        var action = context.RouteData.Values["action"]?.ToString() ?? string.Empty;

        if (string.Equals(controller, "Acceso", StringComparison.OrdinalIgnoreCase) &&
            PublicAccesoActions.Contains(action))
        {
            base.OnActionExecuting(context);
            return;
        }

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
