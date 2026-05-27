using Microsoft.AspNetCore.Mvc.Filters;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Http;
using System.Text.RegularExpressions;
using Microsoft.Extensions.Logging;
using System.Linq;
using System.Reflection;

public class ValidacionInputFiltro : ActionFilterAttribute
{
    private readonly ILogger<ValidacionInputFiltro> _logger;
    private static readonly Regex UnsafePattern = new(
        @"(--|/\*|\*/|\bUNION\b\s+\bSELECT\b|\bDROP\b\s+\bTABLE\b|\bINSERT\b\s+\bINTO\b|\bDELETE\b\s+\bFROM\b|\bUPDATE\b\s+\w+\s+\bSET\b|\bEXEC(?:UTE)?\b\s+\w+|\b(OR|AND)\b\s+1\s*=\s*1|<\s*script\b|<\s*/\s*script\s*>|javascript\s*:|on\w+\s*=)",
        RegexOptions.IgnoreCase | RegexOptions.Compiled);

    private static readonly HashSet<string> SafeHeaders = new(StringComparer.OrdinalIgnoreCase)
    {
        "Accept",
        "Accept-Encoding",
        "Accept-Language",
        "Cache-Control",
        "Connection",
        "Content-Length",
        "Content-Type",
        "Cookie",
        "Host",
        "Origin",
        "Pragma",
        "Referer",
        "User-Agent",
        "Upgrade-Insecure-Requests",
        "Sec-Fetch-Site",
        "Sec-Fetch-Mode",
        "Sec-Fetch-Dest",
        "Sec-Fetch-User",
        "Sec-CH-UA",
        "Sec-CH-UA-Mobile",
        "Sec-CH-UA-Platform",
        "Priority"
    };

    private static readonly HashSet<string> SafeAjaxRoutes = new(StringComparer.OrdinalIgnoreCase)
    {
        "Usuarios.GetNotifications",
        "Usuarios.MarkNotificationAsRead",
        "Usuarios.MonitoreoUsuario",
        "Bitacora.RegistrarActividad",
        "Acceso.Heartbeat",
        "Acceso.ActualizarInicioSesion",
        "Gestor.ApiCrearActividad",
        "Gestor.ApiActualizarActividad",
        "Gestor.ApiCrearTema",
        "Gestor.ApiActualizarTema",
        "ProyectosPrivados.ApiProyectos",
        "ProyectosPrivados.ApiProyecto",
        "ProyectosPrivados.ApiAcciones",
        "ProyectosPrivados.ApiActualizarEstatusAccion",
        "ProyectosPrivados.ApiCrearAccion",
        "ProyectosPrivados.Minuta"
    };

    public ValidacionInputFiltro(ILogger<ValidacionInputFiltro> logger)
    {
        _logger = logger;
    }

    public override void OnActionExecuting(ActionExecutingContext context)
    {
        var controller = context.RouteData.Values["controller"]?.ToString();
        var action = context.RouteData.Values["action"]?.ToString();
        var routeKey = $"{controller}.{action}";
        var isAjax = IsAjaxRequest(context.HttpContext.Request);

        if (string.Equals(controller, "Acceso", StringComparison.OrdinalIgnoreCase) &&
            string.Equals(action, "ActividadSospechosa", StringComparison.OrdinalIgnoreCase))
        {
            base.OnActionExecuting(context);
            return;
        }

        if (SafeAjaxRoutes.Contains(routeKey))
        {
            base.OnActionExecuting(context);
            return;
        }

        foreach (var argument in context.ActionArguments)
        {
            if (TryFindUnsafeValue(argument.Value, out var unsafeValue))
            {
                _logger.LogWarning("Entrada insegura detectada en argumento {Argument}: {UnsafeValue}", argument.Key, unsafeValue);
                HandleBlockedRequest(context, isAjax);
                return;
            }
        }

        if (context.HttpContext.Request.HasFormContentType)
        {
            foreach (var field in context.HttpContext.Request.Form)
            {
                if (ContainsUnsafeInput(field.Value.ToString()))
                {
                    _logger.LogWarning("Entrada insegura detectada en formulario {Field}: {UnsafeValue}", field.Key, field.Value.ToString());
                    HandleBlockedRequest(context, isAjax);
                    return;
                }
            }
        }

        foreach (var header in context.HttpContext.Request.Headers)
        {
            if (!IsSafeHeader(header.Key) && ContainsUnsafeInput(header.Value.ToString()))
            {
                _logger.LogWarning("Entrada insegura detectada en cabecera {Header}: {UnsafeValue}", header.Key, header.Value.ToString());
                HandleBlockedRequest(context, isAjax);
                return;
            }
        }

        base.OnActionExecuting(context);
    }

    private static bool TryFindUnsafeValue(object? value, out string unsafeValue)
    {
        unsafeValue = string.Empty;

        if (value is null)
        {
            return false;
        }

        if (value is string text)
        {
            if (ContainsUnsafeInput(text))
            {
                unsafeValue = text;
                return true;
            }

            return false;
        }

        if (value is IEnumerable<object> collection)
        {
            foreach (var item in collection)
            {
                if (TryFindUnsafeValue(item, out unsafeValue))
                {
                    return true;
                }
            }

            return false;
        }

        var type = value.GetType();
        if (type.IsPrimitive || type.IsEnum || type == typeof(decimal) || type == typeof(DateTime) || type == typeof(Guid))
        {
            return false;
        }

        foreach (var property in type.GetProperties(BindingFlags.Instance | BindingFlags.Public))
        {
            if (!property.CanRead || property.GetIndexParameters().Length > 0)
            {
                continue;
            }

            var propertyValue = property.GetValue(value);
            if (TryFindUnsafeValue(propertyValue, out unsafeValue))
            {
                return true;
            }
        }

        return false;
    }

    private static bool ContainsUnsafeInput(string? input)
    {
        return !string.IsNullOrWhiteSpace(input) && UnsafePattern.IsMatch(input);
    }

    private static bool IsAjaxRequest(HttpRequest request)
    {
        if (request.Headers.TryGetValue("X-Requested-With", out var value) &&
            string.Equals(value.ToString(), "XMLHttpRequest", StringComparison.OrdinalIgnoreCase))
        {
            return true;
        }

        return request.Headers.Accept.Any(a => a.Contains("application/json", StringComparison.OrdinalIgnoreCase));
    }

    private static bool IsSafeHeader(string headerKey)
    {
        if (SafeHeaders.Contains(headerKey))
        {
            return true;
        }

        return headerKey.StartsWith("Sec-CH-UA", StringComparison.OrdinalIgnoreCase)
            || headerKey.StartsWith("X-ARR", StringComparison.OrdinalIgnoreCase)
            || headerKey.StartsWith("X-Forwarded", StringComparison.OrdinalIgnoreCase)
            || headerKey.StartsWith("X-Original", StringComparison.OrdinalIgnoreCase)
            || headerKey.StartsWith("X-Rewrite", StringComparison.OrdinalIgnoreCase)
            || headerKey.StartsWith("X-MS-", StringComparison.OrdinalIgnoreCase)
            || string.Equals(headerKey, "X-Requested-With", StringComparison.OrdinalIgnoreCase);
    }

    private static void HandleBlockedRequest(ActionExecutingContext context, bool isAjax)
    {
        if (isAjax)
        {
            context.Result = new JsonResult(new
            {
                success = false,
                blocked = true,
                message = "Solicitud bloqueada por validacion de seguridad."
            })
            {
                StatusCode = StatusCodes.Status400BadRequest
            };
            return;
        }

        RedirectToSuspiciousActivity(context);
    }

    private static void RedirectToSuspiciousActivity(ActionExecutingContext context)
    {
        context.Result = new RedirectToActionResult("ActividadSospechosa", "Acceso", null);
    }
}
