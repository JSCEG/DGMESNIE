using Microsoft.AspNetCore.Mvc;
using NSIE.Models;
using NSIE.Servicios;
using Newtonsoft.Json;

namespace NSIE.Controllers
{
    // Todo el controlador es exclusivo del administrador del sistema (lo aplica
    // AutorizacionFiltro), salvo RegistrarActividad y GetUltimasAcciones, que
    // usan todos los usuarios para su propia bitácora.
    [ServiceFilter(typeof(ValidacionInputFiltro))]
    [AutorizacionFiltro]
    public class BitacoraController : Controller
    {
        // Sin movimiento en este lapso, una sesión abierta se muestra como inactiva.
        private static readonly TimeSpan VentanaEnLinea = TimeSpan.FromMinutes(15);
        private static readonly TimeZoneInfo ZonaCdmx = TimeZoneInfo.FindSystemTimeZoneById("America/Mexico_City");

        private readonly IRepositorioBitacora repositorioBitacora;

        public BitacoraController(IRepositorioBitacora repositorioBitacora)
        {
            this.repositorioBitacora = repositorioBitacora;
        }

        // Registra la actividad del usuario en sesión. El usuario se toma de la
        // sesión, no del cuerpo: antes cualquiera podía registrar a nombre de otro.
        [HttpPost]
        public async Task<IActionResult> RegistrarActividad([FromBody] UserActivityModel model)
        {
            if (model == null)
            {
                return BadRequest("El modelo no puede ser nulo.");
            }

            var perfil = PerfilEnSesion();
            if (perfil == null)
            {
                return Unauthorized();
            }

            await repositorioBitacora.RegistrarActividadAsync(
                perfil.IdUsuario,
                perfil.Nombre,
                model.ActionName,
                model.ControllerName,
                model.PageName,
                model.Tipo,
                model.Elemento,
                model.IdElemento,
                model.Valor,
                model.AdditionalData
            );

            return Ok(new { success = true });
        }

        // ----------  Últimas acciones del usuario (TOP n) ----------
        [HttpGet]
        public async Task<IActionResult> GetUltimasAcciones(int top = 10)
        {
            var perfil = PerfilEnSesion();
            if (perfil == null)
                return Unauthorized();

            var idUsuario = int.TryParse(perfil.IdUsuario, out var tmp) ? tmp : 0;
            if (idUsuario == 0) return BadRequest("IdUsuario inválido");

            var acciones = await repositorioBitacora.ObtenerUltimasAccionesAsync(idUsuario, top);

            var result = acciones.Select(a => new
            {
                fecha = a.Timestamp.ToString("dd/MM/yyyy HH:mm"),
                pagina = a.PageName,
                controller = a.ControllerName,
                action = a.ActionName
            });

            return Json(result);
        }

        // ============================
        // Seguridad y Auditoría
        // ============================

        public IActionResult Dashboard()
        {
            return View();
        }

        [HttpGet]
        public async Task<IActionResult> Resumen()
        {
            var resumen = await repositorioBitacora.ObtenerResumenAuditoriaAsync(AhoraCdmx().Date);
            return Json(resumen);
        }

        [HttpGet]
        public async Task<IActionResult> SesionesAbiertas()
        {
            var ahoraUtc = DateTime.UtcNow;
            var sesiones = await repositorioBitacora.ObtenerSesionesAbiertasAsync(AhoraCdmx().AddHours(-12));

            return Json(sesiones.Select(s =>
            {
                var ultimoMovimiento = UltimoMovimientoUtc(s.UltimaActividad, s.UltimaAccionLocal);
                return new
                {
                    idUsuario = s.IdUsuario,
                    nombre = s.Nombre,
                    cargo = s.Cargo,
                    inicio = Cdmx(s.FechaInicio),
                    ultimoMovimiento = Cdmx(ultimoMovimiento),
                    expira = Cdmx(s.FechaExpiracion),
                    estado = EstadoSesion(s.Activa, s.FechaExpiracion, ultimoMovimiento, ahoraUtc),
                    ip = s.Ip,
                    navegador = DescribirNavegador(s.UserAgent)
                };
            }));
        }

        [HttpGet]
        public async Task<IActionResult> Usuarios()
        {
            var usuarios = await repositorioBitacora.ObtenerUsuariosAuditoriaAsync();

            return Json(usuarios.Select(u => new
            {
                idUsuario = u.IdUsuario,
                nombre = u.Nombre,
                correo = u.Correo,
                cargo = u.Cargo,
                unidad = u.UnidadAdscripcion,
                ultimoAcceso = u.UltimoAcceso.HasValue ? Cdmx(u.UltimoAcceso.Value) : null,
                accesos = u.Accesos,
                sesionesAbiertas = u.SesionesAbiertas,
                ultimaAccion = u.UltimaAccionLocal.HasValue ? Iso(u.UltimaAccionLocal.Value) : null
            }));
        }

        [HttpGet]
        public async Task<IActionResult> DetalleUsuario(int id, string periodo = "mes")
        {
            if (id <= 0)
            {
                return BadRequest(new { message = "Usuario inválido." });
            }

            var ahoraLocal = AhoraCdmx();
            var desdeLocal = (periodo ?? string.Empty).ToLowerInvariant() switch
            {
                "dia" => ahoraLocal.Date,
                "semana" => ahoraLocal.Date.AddDays(-6),
                "mes" => ahoraLocal.Date.AddDays(-29),
                "anio" => ahoraLocal.Date.AddYears(-1),
                "todo" => new DateTime(2000, 1, 1),
                _ => ahoraLocal.Date.AddDays(-29)
            };
            var desdeUtc = TimeZoneInfo.ConvertTimeToUtc(DateTime.SpecifyKind(desdeLocal, DateTimeKind.Unspecified), ZonaCdmx);

            var accesosTask = repositorioBitacora.ObtenerAccesosUsuarioAsync(id, desdeUtc);
            var sesionesTask = repositorioBitacora.ObtenerSesionesUsuarioAsync(id, desdeUtc);
            var accionesTask = repositorioBitacora.ObtenerAccionesUsuarioAsync(id, desdeLocal);
            await Task.WhenAll(accesosTask, sesionesTask, accionesTask);

            var accesos = accesosTask.Result;
            var sesiones = sesionesTask.Result;
            var acciones = accionesTask.Result;

            // Cada sesión dura hasta que empieza la siguiente o hasta que expira;
            // las acciones de la bitácora que caen en ese tramo le pertenecen.
            var accionesUtc = acciones
                .Select(a => TimeZoneInfo.ConvertTimeToUtc(DateTime.SpecifyKind(a.TimestampLocal, DateTimeKind.Unspecified), ZonaCdmx))
                .OrderBy(t => t)
                .ToList();
            var ordenadas = sesiones.OrderBy(s => s.FechaInicio).ToList();
            var ahoraUtc = DateTime.UtcNow;

            var sesionesData = ordenadas.Select((s, i) =>
            {
                var fin = i + 1 < ordenadas.Count
                    ? Min(ordenadas[i + 1].FechaInicio, s.FechaExpiracion)
                    : s.FechaExpiracion;
                var desde = PrimeraPosicion(accionesUtc, s.FechaInicio);
                var hasta = PrimeraPosicion(accionesUtc, fin);
                var accionesEnTramo = hasta - desde;
                // En una sesión cerrada, UltimaActividad es la hora en que la cerró el
                // siguiente inicio de sesión (sp_RegistrarSesion), no actividad real.
                var ultimoMovimiento = s.Activa && s.UltimaActividad > s.FechaInicio ? s.UltimaActividad : s.FechaInicio;
                if (accionesEnTramo > 0 && accionesUtc[hasta - 1] > ultimoMovimiento) ultimoMovimiento = accionesUtc[hasta - 1];

                return new
                {
                    inicio = Cdmx(s.FechaInicio),
                    ultimoMovimiento = Cdmx(ultimoMovimiento),
                    duracionMin = (int)Math.Max(0, (ultimoMovimiento - s.FechaInicio).TotalMinutes),
                    acciones = accionesEnTramo,
                    estado = EstadoSesion(s.Activa, s.FechaExpiracion, ultimoMovimiento, ahoraUtc),
                    ip = s.Ip,
                    navegador = DescribirNavegador(s.UserAgent)
                };
            }).Reverse().ToList();

            return Json(new
            {
                periodo,
                desde = Iso(desdeLocal),
                limites = new { accesos = 2000, sesiones = 1000, acciones = 3000 },
                totales = new
                {
                    accesos = accesos.Count,
                    sesiones = sesiones.Count,
                    acciones = acciones.Count,
                    ips = accesos.Select(a => a.Ip).Where(ip => !string.IsNullOrWhiteSpace(ip)).Distinct().Count()
                },
                accesos = accesos.Select(a => new
                {
                    fecha = Cdmx(a.FechaAcceso),
                    tipo = a.TipoAcceso,
                    ip = a.Ip,
                    exitoso = a.Exitoso
                }),
                sesiones = sesionesData,
                acciones = acciones.Select(a => new
                {
                    fecha = Iso(a.TimestampLocal),
                    pagina = a.Pagina,
                    accion = a.Accion,
                    tipo = a.Tipo,
                    elemento = a.Elemento,
                    valor = a.Valor
                })
            });
        }

        [HttpGet]
        public async Task<IActionResult> AccesosPorDia(int dias = 90)
        {
            dias = Math.Clamp(dias, 7, 365);
            var hoyLocal = AhoraCdmx().Date;
            var desdeLocal = hoyLocal.AddDays(-(dias - 1));
            var desdeUtc = TimeZoneInfo.ConvertTimeToUtc(DateTime.SpecifyKind(desdeLocal, DateTimeKind.Unspecified), ZonaCdmx);

            var accesos = await repositorioBitacora.ObtenerAccesosDesdeAsync(desdeUtc);
            var porDia = accesos
                .GroupBy(a => ACdmx(a.FechaAcceso).Date)
                .ToDictionary(g => g.Key, g => new { accesos = g.Count(), usuarios = g.Select(a => a.IdUsuario).Distinct().Count() });

            var serie = Enumerable.Range(0, dias)
                .Select(i => desdeLocal.AddDays(i))
                .Select(d => new
                {
                    fecha = d.ToString("yyyy-MM-dd"),
                    accesos = porDia.TryGetValue(d, out var v) ? v.accesos : 0,
                    usuarios = porDia.TryGetValue(d, out var u) ? u.usuarios : 0
                });

            return Json(serie);
        }

        // ============================
        // Utilidades
        // ============================

        private PerfilUsuario PerfilEnSesion()
        {
            var perfilJson = HttpContext.Session.GetString("PerfilUsuario");
            return string.IsNullOrWhiteSpace(perfilJson)
                ? null
                : JsonConvert.DeserializeObject<PerfilUsuario>(perfilJson);
        }

        private static DateTime AhoraCdmx() => TimeZoneInfo.ConvertTimeFromUtc(DateTime.UtcNow, ZonaCdmx);

        private static DateTime ACdmx(DateTime utc) =>
            TimeZoneInfo.ConvertTimeFromUtc(DateTime.SpecifyKind(utc, DateTimeKind.Utc), ZonaCdmx);

        private static string Cdmx(DateTime utc) => Iso(ACdmx(utc));

        private static string Iso(DateTime valor) => valor.ToString("yyyy-MM-ddTHH:mm:ss");

        private static DateTime Min(DateTime a, DateTime b) => a < b ? a : b;

        // Índice del primer elemento >= valor en una lista ordenada.
        private static int PrimeraPosicion(List<DateTime> ordenada, DateTime valor)
        {
            int bajo = 0, alto = ordenada.Count;
            while (bajo < alto)
            {
                var medio = (bajo + alto) / 2;
                if (ordenada[medio] < valor) bajo = medio + 1; else alto = medio;
            }
            return bajo;
        }

        private static DateTime UltimoMovimientoUtc(DateTime ultimaActividadUtc, DateTime? ultimaAccionLocal)
        {
            if (!ultimaAccionLocal.HasValue) return ultimaActividadUtc;
            var accionUtc = TimeZoneInfo.ConvertTimeToUtc(DateTime.SpecifyKind(ultimaAccionLocal.Value, DateTimeKind.Unspecified), ZonaCdmx);
            return accionUtc > ultimaActividadUtc ? accionUtc : ultimaActividadUtc;
        }

        // Activa en la tabla no basta: cerrar el navegador la deja en 1 hasta que vence.
        private static string EstadoSesion(bool activa, DateTime expiraUtc, DateTime ultimoMovimientoUtc, DateTime ahoraUtc)
        {
            if (!activa) return "cerrada";
            if (expiraUtc <= ahoraUtc) return "vencida";
            return ahoraUtc - ultimoMovimientoUtc <= VentanaEnLinea ? "enlinea" : "inactiva";
        }

        private static string DescribirNavegador(string userAgent)
        {
            if (string.IsNullOrWhiteSpace(userAgent)) return "Sin dato";

            var navegador = userAgent.Contains("Edg/") ? "Edge"
                : userAgent.Contains("OPR/") ? "Opera"
                : userAgent.Contains("Firefox/") ? "Firefox"
                : userAgent.Contains("Chrome/") ? "Chrome"
                : userAgent.Contains("Safari/") ? "Safari"
                : "Otro";

            var sistema = userAgent.Contains("Android") ? "Android"
                : userAgent.Contains("iPhone") || userAgent.Contains("iPad") ? "iOS"
                : userAgent.Contains("Windows") ? "Windows"
                : userAgent.Contains("Mac OS X") ? "macOS"
                : userAgent.Contains("Linux") ? "Linux"
                : "";

            return string.IsNullOrEmpty(sistema) ? navegador : $"{navegador} · {sistema}";
        }
    }
}
