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
        "ActualizarInicioSesion",
        "DevBypass"
    };

    public override void OnActionExecuting(ActionExecutingContext context)
    {
        // Log para confirmar que el filtro se ejecuta
        Console.WriteLine("Filtro AutorizacionFiltro ejecutado");

        var requestPath = context.HttpContext.Request.Path.ToString();
        var isCarteraApiRequest = requestPath.StartsWith(
            "/ProyectosPrivados/Api/CarteraConvocatoria",
            StringComparison.OrdinalIgnoreCase);
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
        var controllerAutorizacion = string.Equals(controller, "PamrntProyectos", StringComparison.OrdinalIgnoreCase)
            ? "InformePormenorizado"
            : controller;

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
            // Las peticiones AJAX no deben recibir HTML por una redirección silenciosa.
            if (isCarteraApiRequest)
            {
                context.Result = new JsonResult(new
                {
                    error = "Tu sesión expiró. Inicia sesión nuevamente; el comentario permanece en el formulario."
                })
                {
                    StatusCode = StatusCodes.Status401Unauthorized
                };
                return;
            }

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

                if (restrictedControllers.Contains(controllerAutorizacion))
                {
                    if (idUsuario != 1 && idUsuario != 86)
                    {
                        var seccionesUsuarioJson = session.GetString("SeccionesUsuario");
                        if (string.IsNullOrEmpty(seccionesUsuarioJson))
                        {
                            Console.WriteLine($"Acceso denegado: menú de sesión vacío para {perfilUsuario.Nombre} ({idUsuario}) al intentar entrar a {controller}/{action}");
                            // Antes esto mandaba al inicio sin decir nada: el usuario
                            // hacía clic y aparecía en otra pantalla, sin saber si
                            // falló, si se perdió o si no tiene el permiso.
                            context.Result = new RedirectToActionResult("SinPermiso", "Acceso",
                                new { modulo = controller });
                            return;
                        }

                        var seccionesUsuario = JsonConvert.DeserializeObject<List<SeccionSNIER>>(seccionesUsuarioJson) ?? new List<SeccionSNIER>();
                        bool tieneAcceso = false;

                        foreach (var seccion in seccionesUsuario)
                        {
                            if (seccion.Modulos == null) continue;
                            foreach (var mod in seccion.Modulos)
                            {
                                if (string.Equals(mod.Controller, controllerAutorizacion, StringComparison.OrdinalIgnoreCase))
                                {
                                    if (string.Equals(controllerAutorizacion, "PlanMexico", StringComparison.OrdinalIgnoreCase))
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
                            
                            context.Result = new RedirectToActionResult("SinPermiso", "Acceso",
                                new { modulo = controller });
                            return;
                        }
                    }
                }
            }
        }
    }
}
