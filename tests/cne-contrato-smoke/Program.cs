using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.Logging.Abstractions;
using NSIE.Controllers;
using NSIE.Servicios;
using System.Text.Json;

static void Check(bool condition, string message)
{
    if (!condition) throw new Exception(message);
}

var tables = new Dictionary<string, string>
{
    ["transporte"] = "Transporte_autorizado",
    ["comercializacion"] = "Comercializacion_autorizado",
    ["infraestructura"] = "Infraestructura_Hidrocarburos_autorizado",
    ["suministro"] = "Suministro_Electrico_autorizado",
    ["terceros"] = "Terceros_Acreditados_CNE"
};
foreach (var (key, name) in tables) Check(TablaPermisosCne.Resolver(key)?.Nombre == name, key);
foreach (var key in new string?[] { null, "otra", "", "dbo.Transporte_autorizado", "transporte; SELECT 1" })
    Check(TablaPermisosCne.Resolver(key) is null, "Catálogo debe ser cerrado");

Check(RepositorioTablasPermisosCne.Formatear("capacidad", null) is null, "NULL numérico");
Check(RepositorioTablasPermisosCne.Formatear("comentarios", DBNull.Value) is null, "DBNull");
Check((string?)RepositorioTablasPermisosCne.Formatear("efId", "09  ") == "09", "nchar EF");
Check((string?)RepositorioTablasPermisosCne.Formatear("mpoId", "015 ") == "015", "nchar municipio");
var utc = new DateTime(2026, 10, 21, 16, 5, 0);
Check((string?)RepositorioTablasPermisosCne.Formatear("fechaActualizacion", utc) == "2026-10-21T16:05:00.0000000Z", "datetime2 UTC");
Check((string?)RepositorioTablasPermisosCne.Formatear("fechaCorteFuente", utc) == "2026-10-21", "date");
var offset = new DateTimeOffset(2026, 10, 21, 10, 5, 0, TimeSpan.FromHours(-6));
Check((string?)RepositorioTablasPermisosCne.Formatear("fechaActualizacion", offset) == "2026-10-21T16:05:00.0000000Z", "offset UTC");

// Cadena deliberadamente inválida: el fallo SQL debe producir sólo el mensaje público.
var config = new ConfigurationBuilder().AddInMemoryCollection(new Dictionary<string, string?>
{
    ["ConnectionStrings:DefaultConnection"] = "INVALID_CONNECTION_SECRET"
}).Build();
var controller = new TablasPermisosCneController(new RepositorioTablasPermisosCne(config),
    NullLogger<TablasPermisosCneController>.Instance);
controller.ControllerContext = new ControllerContext { HttpContext = new DefaultHttpContext() };
foreach (var endpoint in new Func<string?, CancellationToken, Task<IActionResult>>[]
         { controller.Estados, controller.Puntos, controller.Actualizacion })
{
    controller.Request.QueryString = new QueryString("?tabla=otra");
    Check((await endpoint("otra", default) as ObjectResult)?.StatusCode == 400, "Tabla inválida");
    controller.Request.QueryString = new QueryString("?tabla=transporte");
    var result = await endpoint("transporte", default) as ObjectResult;
    Check(result?.StatusCode == 500, "Fallo de conexión debe ser 500");
    Check(JsonSerializer.Serialize(result!.Value) == "{\"mensaje\":\"No fue posible consultar los permisos.\"}", "Error sin detalles internos");
}
controller.Request.QueryString = new QueryString("?tabla=transporte&estatus=todos");
Check((await controller.Lista("transporte", "todos", null, null, default) as ObjectResult)?.StatusCode == 400, "Estatus inválido");
controller.Request.QueryString = new QueryString("?tabla=transporte&ef=9");
Check((await controller.Lista("transporte", null, "9", null, default) as ObjectResult)?.StatusCode == 400, "EF inválida");
controller.Request.QueryString = new QueryString("?tabla=transporte");
Check((await controller.Lista("transporte", null, null, null, default) as ObjectResult)?.StatusCode == 500, "Lista 500");
Console.WriteLine("OK catálogo, nulos, códigos INEGI, fechas UTC, validación 400 y errores 500 sin detalles internos.");
