using Microsoft.AspNetCore.Mvc.Filters;
using Microsoft.AspNetCore.Mvc;
using System.Text.RegularExpressions;
using Microsoft.Extensions.Logging;
using System.Linq;

public class ValidacionInputFiltro : ActionFilterAttribute
{
    private readonly ILogger<ValidacionInputFiltro> _logger;

    public ValidacionInputFiltro(ILogger<ValidacionInputFiltro> logger)
    {
        _logger = logger;
    }

//     public override void OnActionExecuting(ActionExecutingContext context)
//     {
//         _logger.LogInformation("Filtro ValidacionInputFiltro ejecutado");

//         // Evitar que el filtro se aplique en la vista "ActividadSospechosa"
//         if (context.RouteData.Values["controller"]?.ToString() == "Error" &&
//             context.RouteData.Values["action"]?.ToString() == "ActividadSospechosa")
//         {
//             _logger.LogInformation("Vista ActividadSospechosa excluida del filtro");
//             base.OnActionExecuting(context);
//             return;
//         }

//         // Validar inputs en los parámetros de acción
//         foreach (var param in context.ActionArguments)
//         {
//             if (param.Value is string input && ContainsUnsafeInput(input))
//             {
//                 _logger.LogWarning($"Entrada insegura detectada en parámetros: {input}");
//                 RedirigirActividadSospechosa(context);
//                 return;
//             }
//         }

//         // Validar cabeceras HTTP no estándar
//         foreach (var header in context.HttpContext.Request.Headers)
//         {
//             if (!EsCabeceraSegura(header.Key) && ContainsUnsafeInput(header.Value))
//             {
//                 _logger.LogWarning($"Entrada insegura detectada en cabeceras: {header.Key} = {header.Value}");
//                 RedirigirActividadSospechosa(context);
//                 return;
//             }
//         }

//         // Continuar con la ejecución normal de la acción si no hay problemas
//         base.OnActionExecuting(context);
//     }

//     private bool ContainsUnsafeInput(string input)
//     {
//         // Regex para detectar patrones sospechosos
//         var unsafePattern = new Regex(@"(--|;|'|""|\b(OR|AND)\b\s*\d+|=\s*\d+|UNION\s+SELECT|DROP\s+TABLE|INSERT\s+INTO|DELETE\s+FROM|UPDATE\s+\w+|<.*?>|1\s*=\s*1|script\s*:|javascript\s*:)", RegexOptions.IgnoreCase);
//         return unsafePattern.IsMatch(input);
//     }

//     private bool EsCabeceraSegura(string headerKey)
//     {
//         // Lista de cabeceras consideradas seguras
//         var cabecerasSeguras = new[]
//         {
//             "Accept",
//             "Accept-Encoding",
//             "Accept-Language",
//             "Cache-Control",
//             "Connection",
//             "Content-Length",
//             "Content-Type",
//             "Cookie",
//             "Host",
//             "Origin",
//             "Pragma",
//             "Referer",
//             "User-Agent",
//             "Upgrade-Insecure-Requests",
//             "Sec-Fetch-Site",
//             "Sec-Fetch-Mode",
//             "Sec-Fetch-Dest",
//             "Sec-Fetch-User",
//             "Sec-CH-UA",
//             "Sec-CH-UA-Mobile",
//             "Sec-CH-UA-Platform",
//             "Priority" // Cabecera agregada
//         };

//         // Comparación insensible a mayúsculas
//         return cabecerasSeguras.Contains(headerKey, StringComparer.OrdinalIgnoreCase);
//     }

//     private void RedirigirActividadSospechosa(ActionExecutingContext context)
//     {
//         _logger.LogWarning("Redirigiendo a la vista ActividadSospechosa por entrada sospechosa");
//         context.Result = new RedirectToActionResult("ActividadSospechosa", "Error", null);
//     }
 }
