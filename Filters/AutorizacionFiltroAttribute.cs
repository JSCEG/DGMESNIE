using Microsoft.AspNetCore.Mvc.Filters;
using Microsoft.AspNetCore.Mvc;
using Newtonsoft.Json;
using NSIE.Models;
using Microsoft.Extensions.Configuration;
using Microsoft.AspNetCore.Authorization;
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
        "ActualizarInicioSesion",
        "DevBypass"
    };

    public override void OnActionExecuting(ActionExecutingContext context)
    {
        if (context.HttpContext.GetEndpoint()?.Metadata.GetMetadata<IAllowAnonymous>() != null)
        {
            base.OnActionExecuting(context);
            return;
        }

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
             || requestPath.StartsWith("/Acceso/LoginFacebook", StringComparison.OrdinalIgnoreCase)
             || requestPath.StartsWith("/Acceso/DevBypass", StringComparison.OrdinalIgnoreCase)))
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

            if (perfilUsuario != null && int.TryParse(perfilUsuario.IdUsuario, out int idUsuario))
            {
                var restrictedControllers = new HashSet<string>(StringComparer.OrdinalIgnoreCase)
                {
                    "InformePormenorizado",
                    "ProyectosPrivados",
                    "PermisosPV",
                    "PODECOBIS",
                    "PlanMexico"
                };

                if (restrictedControllers.Contains(controller))
                {
                    if (idUsuario != 1 && idUsuario != 86)
                    {
                        var seccionesUsuarioJson = session.GetString("SeccionesUsuario");
                        if (string.IsNullOrEmpty(seccionesUsuarioJson))
                        {
                            Console.WriteLine($"Acceso denegado: menú de sesión vacío para {perfilUsuario.Nombre} ({idUsuario}) al intentar entrar a {controller}/{action}");
                            context.Result = new RedirectToActionResult("Index", "Home", null);
                            return;
                        }

                        var seccionesUsuario = JsonConvert.DeserializeObject<List<SeccionSNIER>>(seccionesUsuarioJson) ?? new List<SeccionSNIER>();
                        bool tieneAcceso = false;

                        foreach (var seccion in seccionesUsuario)
                        {
                            if (seccion.Modulos == null) continue;
                            foreach (var mod in seccion.Modulos)
                            {
                                if (string.Equals(mod.Controller, controller, StringComparison.OrdinalIgnoreCase))
                                {
                                    if (string.Equals(controller, "PlanMexico", StringComparison.OrdinalIgnoreCase))
                                    {
                                        if (string.Equals(action, "Plan_Polos", StringComparison.OrdinalIgnoreCase))
                                        {
                                            if (string.Equals(mod.Action, "Plan_Polos", StringComparison.OrdinalIgnoreCase) ||
                                                (mod.Vistas != null && mod.Vistas.Any(v => string.Equals(v.VistaAction ?? v.Action, "Plan_Polos", StringComparison.OrdinalIgnoreCase))))
                                            {
                                                tieneAcceso = true;
                                                break;
                                            }
                                        }
                                        else
                                        {
                                            tieneAcceso = true;
                                            break;
                                        }
                                    }
                                    else
                                    {
                                        tieneAcceso = true;
                                        break;
                                    }
                                }
                            }
                            if (tieneAcceso) break;
                        }

                        if (!tieneAcceso)
                        {
                            Console.WriteLine($"Acceso denegado dinámicamente: {perfilUsuario.Nombre} ({idUsuario}) no tiene el módulo {controller} (o acción {action}) en su menú.");
                            
                            bool tieneGestor = seccionesUsuario
                                .SelectMany(s => s.Modulos)
                                .Any(m => string.Equals(m.Controller, "Gestor", StringComparison.OrdinalIgnoreCase));
                            
                            string redirectController = tieneGestor ? "Gestor" : "Home";
                            context.Result = new RedirectToActionResult("Index", redirectController, null);
                            return;
                        }
                    }
                }
            }
        }
    }
}
