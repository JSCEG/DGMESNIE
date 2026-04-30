using Microsoft.AspNetCore.Mvc.Filters;
using Microsoft.AspNetCore.Mvc;
using System.Text.RegularExpressions;
using Microsoft.Extensions.Logging;
using System.Linq;
using System.Reflection;

public class ValidacionInputFiltro : ActionFilterAttribute
{
    private readonly ILogger<ValidacionInputFiltro> _logger;
    private static readonly Regex UnsafePattern = new(
        @"(--|;|'|""|\b(OR|AND)\b\s*\d+|=\s*\d+|UNION\s+SELECT|DROP\s+TABLE|INSERT\s+INTO|DELETE\s+FROM|UPDATE\s+\w+|<.*?>|1\s*=\s*1|script\s*:|javascript\s*:)",
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

    public ValidacionInputFiltro(ILogger<ValidacionInputFiltro> logger)
    {
        _logger = logger;
    }

    public override void OnActionExecuting(ActionExecutingContext context)
    {
        var controller = context.RouteData.Values["controller"]?.ToString();
        var action = context.RouteData.Values["action"]?.ToString();

        if (string.Equals(controller, "Acceso", StringComparison.OrdinalIgnoreCase) &&
            string.Equals(action, "ActividadSospechosa", StringComparison.OrdinalIgnoreCase))
        {
            base.OnActionExecuting(context);
            return;
        }

        foreach (var argument in context.ActionArguments)
        {
            if (TryFindUnsafeValue(argument.Value, out var unsafeValue))
            {
                _logger.LogWarning("Entrada insegura detectada en argumento {Argument}: {UnsafeValue}", argument.Key, unsafeValue);
                RedirectToSuspiciousActivity(context);
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
                    RedirectToSuspiciousActivity(context);
                    return;
                }
            }
        }

        foreach (var header in context.HttpContext.Request.Headers)
        {
            if (!SafeHeaders.Contains(header.Key) && ContainsUnsafeInput(header.Value.ToString()))
            {
                _logger.LogWarning("Entrada insegura detectada en cabecera {Header}: {UnsafeValue}", header.Key, header.Value.ToString());
                RedirectToSuspiciousActivity(context);
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

    private static void RedirectToSuspiciousActivity(ActionExecutingContext context)
    {
        context.Result = new RedirectToActionResult("ActividadSospechosa", "Acceso", null);
    }
}
