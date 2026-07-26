using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Http;
using Microsoft.Data.SqlClient;
using Newtonsoft.Json;
using NSIE.Models;
using NSIE.Servicios;

namespace NSIE.Controllers
{
    public class DashboardProyectosController : Controller
    {
        private readonly IPamrntProyectosIdentificadosService _pamService;
        private readonly IServicioEmailSMTP _emailService;
        private readonly IInegiTerritorialService _inegiService;
        private readonly ICneFuelPriceService _cneFuelPriceService;
        private readonly IRepositorioTarifas _tarifasRepository;
        private readonly ILogger<DashboardProyectosController> _logger;

        [ActivatorUtilitiesConstructor]
        public DashboardProyectosController(
            IPamrntProyectosIdentificadosService pamService,
            IServicioEmailSMTP emailService,
            IInegiTerritorialService inegiService,
            ICneFuelPriceService cneFuelPriceService,
            IRepositorioTarifas tarifasRepository,
            ILogger<DashboardProyectosController> logger)
        {
            _pamService = pamService;
            _emailService = emailService;
            _inegiService = inegiService;
            _cneFuelPriceService = cneFuelPriceService;
            _tarifasRepository = tarifasRepository;
            _logger = logger;
        }

        [HttpGet("DashboardProyectos/PreciosCombustibles")]
        public async Task<IActionResult> PreciosCombustibles(
            [FromQuery] double minLat,
            [FromQuery] double minLon,
            [FromQuery] double maxLat,
            [FromQuery] double maxLon,
            CancellationToken cancellationToken)
        {
            var validBounds =
                double.IsFinite(minLat) &&
                double.IsFinite(minLon) &&
                double.IsFinite(maxLat) &&
                double.IsFinite(maxLon) &&
                minLat >= 14 &&
                maxLat <= 33.5 &&
                minLon >= -118 &&
                maxLon <= -86 &&
                minLat < maxLat &&
                minLon < maxLon;
            if (!validBounds)
            {
                return BadRequest(new
                {
                    ok = false,
                    code = "INVALID_FUEL_PRICE_BOUNDS",
                    mensaje = "La cobertura solicitada para precios de combustibles no es válida."
                });
            }

            try
            {
                var data = await _cneFuelPriceService.GetAsync(
                    minLat,
                    minLon,
                    maxLat,
                    maxLon,
                    cancellationToken);
                return Json(new { ok = true, data });
            }
            catch (Exception ex) when (
                ex is HttpRequestException or
                TaskCanceledException or
                System.Xml.XmlException or
                InvalidOperationException)
            {
                _logger.LogWarning(
                    ex,
                    "No fue posible consultar precios CNE para la cobertura {MinLat},{MinLon},{MaxLat},{MaxLon}.",
                    minLat,
                    minLon,
                    maxLat,
                    maxLon);
                return StatusCode(StatusCodes.Status502BadGateway, new
                {
                    ok = false,
                    code = "CNE_FUEL_PRICES_UNAVAILABLE",
                    mensaje = "La CNE no devolvió precios de combustibles en este momento."
                });
            }
        }

        public async Task<IActionResult> Index()
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

            try
            {
                var destinatarios = await _pamService.ObtenerDestinatariosAsync();
                ViewData["ReportDestinatariosJson"] = JsonConvert.SerializeObject(destinatarios ?? new List<PamUsuarioDestinatario>());
            }
            catch (Exception ex)
            {
                _logger.LogWarning(ex, "No fue posible cargar destinatarios para el envío del reporte territorial.");
                ViewData["ReportDestinatariosJson"] = "[]";
            }

            return View();
        }

        [HttpGet("DashboardProyectos/TarifasTerritoriales")]
        public async Task<IActionResult> TarifasTerritoriales(
            [FromQuery] string? divisiones,
            CancellationToken cancellationToken)
        {
            var seleccion = (divisiones ?? string.Empty)
                .Split(',', StringSplitOptions.RemoveEmptyEntries | StringSplitOptions.TrimEntries)
                .Where(nombre => nombre.Length is > 0 and <= 80)
                .Distinct(StringComparer.OrdinalIgnoreCase)
                .ToArray();

            if (seleccion.Length == 0)
            {
                return BadRequest(new
                {
                    ok = false,
                    code = "INVALID_TARIFF_DIVISION",
                    mensaje = "Se requiere al menos una división tarifaria."
                });
            }

            if (seleccion.Length > 17)
            {
                return BadRequest(new
                {
                    ok = false,
                    code = "TOO_MANY_TARIFF_DIVISIONS",
                    mensaje = "El análisis admite como máximo las 17 divisiones tarifarias nacionales."
                });
            }

            try
            {
                var data = await _tarifasRepository.ObtenerResumenTerritorialAsync(
                    seleccion,
                    cancellationToken);
                return Json(new { ok = true, data });
            }
            catch (SqlException exception)
            {
                _logger.LogWarning(
                    exception,
                    "No fue posible consultar tarifas territoriales para {DivisionCount} divisiones.",
                    seleccion.Length);
                return StatusCode(StatusCodes.Status503ServiceUnavailable, new
                {
                    ok = false,
                    code = "TARIFFS_UNAVAILABLE",
                    mensaje = "No fue posible recuperar el histórico tarifario institucional en este momento."
                });
            }
        }

        [HttpGet("DashboardProyectos/IndicadoresInegi")]
        public async Task<IActionResult> IndicadoresInegi(
            [FromQuery] string entidad,
            [FromQuery] string? municipio,
            [FromQuery] double? areaKm2,
            CancellationToken cancellationToken)
        {
            var entidadNormalizada = (entidad ?? string.Empty).Trim();
            var municipioNormalizado = (municipio ?? string.Empty).Trim();
            if (entidadNormalizada.Length != 2 ||
                !int.TryParse(entidadNormalizada, out var entidadClave) ||
                entidadClave is < 1 or > 32)
            {
                return BadRequest(new { ok = false, code = "INVALID_STATE", mensaje = "La clave de entidad debe estar entre 01 y 32." });
            }

            if (!string.IsNullOrEmpty(municipioNormalizado) &&
                (municipioNormalizado.Length != 3 ||
                 !int.TryParse(municipioNormalizado, out var municipioClave) ||
                 municipioClave is < 1 or > 999))
            {
                return BadRequest(new { ok = false, code = "INVALID_MUNICIPALITY", mensaje = "La clave municipal debe estar entre 001 y 999." });
            }

            if (areaKm2 is <= 0 or > 10_000_000)
            {
                return BadRequest(new { ok = false, code = "INVALID_AREA", mensaje = "La superficie territorial no es válida." });
            }

            if (!_inegiService.IsConfigured)
            {
                return StatusCode(StatusCodes.Status503ServiceUnavailable, new
                {
                    ok = false,
                    code = "INEGI_TOKEN_MISSING",
                    mensaje = "Configure Inegi:Token mediante secretos de usuario o la variable Inegi__Token."
                });
            }

            var geographicCode = entidadNormalizada + municipioNormalizado;
            try
            {
                var result = await _inegiService.GetAsync(geographicCode, areaKm2, cancellationToken);
                return Json(new { ok = true, data = result });
            }
            catch (HttpRequestException ex)
            {
                _logger.LogWarning(ex, "La consulta territorial INEGI falló para {GeographicCode}.", geographicCode);
                return StatusCode(StatusCodes.Status502BadGateway, new
                {
                    ok = false,
                    code = "INEGI_UNAVAILABLE",
                    mensaje = "INEGI no devolvió datos para el ámbito solicitado."
                });
            }
        }

        [HttpPost("DashboardProyectos/EnviarReporte")]
        [ValidateAntiForgeryToken]
        [RequestSizeLimit(67108864)]
        [RequestFormLimits(MultipartBodyLengthLimit = 67108864)]
        public async Task<IActionResult> EnviarReporte([FromBody] PamEnviarFichaInput input)
        {
            if (input == null || string.IsNullOrWhiteSpace(input.ArchivoBase64) || input.UsuarioIds is not { Count: > 0 })
                return BadRequest(new { ok = false, mensaje = "Selecciona al menos un destinatario y genera primero el archivo." });

            var formato = string.Equals(input.Formato, "pdf16x9", StringComparison.OrdinalIgnoreCase) ? "pdf16x9" : "pdf";
            var descripcionFormato = formato == "pdf16x9" ? "PDF horizontal 16:9" : "PDF tamaño carta";
            var clave = string.IsNullOrWhiteSpace(input.ClavePem) ? "ReporteTerritorial" : input.ClavePem.Trim();

            byte[] adjunto;
            try
            {
                var base64 = input.ArchivoBase64;
                var coma = base64.IndexOf(',');
                if (coma >= 0) base64 = base64[(coma + 1)..];
                adjunto = Convert.FromBase64String(base64);
            }
            catch
            {
                return BadRequest(new { ok = false, mensaje = "El archivo adjunto no se pudo procesar. Vuelve a generarlo." });
            }

            const int maxAttachmentBytes = 20 * 1024 * 1024;
            if (adjunto.Length == 0 || adjunto.Length > maxAttachmentBytes)
                return BadRequest(new { ok = false, mensaje = "El archivo está vacío o supera el límite seguro de 20 MB para correo." });

            var nombreArchivo = string.IsNullOrWhiteSpace(input.NombreArchivo)
                ? $"DGMESNIE_reporte_territorial{(formato == "pdf16x9" ? "_16x9" : string.Empty)}_{DateTime.Now:yyyy-MM-dd}.pdf"
                : (input.NombreArchivo.EndsWith(".pdf", StringComparison.OrdinalIgnoreCase) ? input.NombreArchivo : $"{input.NombreArchivo}.pdf");

            var destinatarios = await _pamService.ObtenerDestinatariosAsync();
            var seleccionados = destinatarios.Where(d => input.UsuarioIds.Contains(d.IdUsuario)).ToList();
            if (seleccionados.Count == 0)
                return BadRequest(new { ok = false, mensaje = "Los destinatarios seleccionados no son válidos." });

            var perfilJson = HttpContext.Session.GetString("PerfilUsuario");
            var perfil = string.IsNullOrWhiteSpace(perfilJson) ? null : JsonConvert.DeserializeObject<PerfilUsuario>(perfilJson);
            var remitente = string.IsNullOrWhiteSpace(perfil?.Nombre) ? "el equipo de la DGMESNIE" : perfil.Nombre;
            string remitenteCargo = null;
            if (perfil != null && int.TryParse(perfil.IdUsuario, out var remitenteId))
                remitenteCargo = destinatarios.FirstOrDefault(d => d.IdUsuario == remitenteId)?.Cargo;
            if (string.IsNullOrWhiteSpace(remitenteCargo)) remitenteCargo = perfil?.Cargo;

            var enviados = new List<string>();
            var fallidos = new List<string>();
            var motivosFallo = new HashSet<string>(StringComparer.OrdinalIgnoreCase);
            foreach (var destino in seleccionados)
            {
                try
                {
                    var cuerpo = ConstruirCorreoReporte(destino.Nombre, descripcionFormato, remitente, remitenteCargo, input.MensajeAdicional);
                    await _emailService.EnviarCorreo(
                        destino.Correo,
                        "Reporte territorial DGMESNIE",
                        cuerpo,
                        adjunto,
                        nombreArchivo);
                    enviados.Add(destino.Nombre);
                }
                catch (Exception ex)
                {
                    _logger.LogError(ex, "No fue posible enviar el reporte territorial a {Correo}.", destino.Correo);
                    fallidos.Add(destino.Nombre);
                    motivosFallo.Add(DescribirFalloEnvio(ex));
                }
            }

            if (enviados.Count == 0)
            {
                var detalle = motivosFallo.Count > 0 ? $" {string.Join(" ", motivosFallo)}" : string.Empty;
                return StatusCode(StatusCodes.Status502BadGateway, new
                {
                    ok = false,
                    mensaje = $"No fue posible enviar el reporte a ningún destinatario.{detalle}"
                });
            }

            var mensaje = $"Reporte enviado a {enviados.Count} destinatario(s): {string.Join(", ", enviados)}.";
            if (fallidos.Count > 0)
            {
                var detalle = motivosFallo.Count > 0 ? $" Motivo: {string.Join(" ", motivosFallo)}" : string.Empty;
                mensaje += $" No se pudo enviar a: {string.Join(", ", fallidos)}.{detalle}";
            }
            return Ok(new { ok = true, mensaje });
        }

        private static string DescribirFalloEnvio(Exception ex)
        {
            var detalle = ex?.ToString() ?? string.Empty;
            var motivos = new List<string>();

            if (detalle.Contains("535", StringComparison.OrdinalIgnoreCase)
                || detalle.Contains("Autenticación SMTP rechazada", StringComparison.OrdinalIgnoreCase)
                || detalle.Contains("Authentication unsuccessful", StringComparison.OrdinalIgnoreCase))
            {
                motivos.Add("El servidor SMTP rechazó las credenciales (535).");
            }

            if (detalle.Contains("413", StringComparison.OrdinalIgnoreCase))
                motivos.Add("SendGrid rechazó el tamaño del mensaje o adjunto (413).");

            if (motivos.Count == 0)
                motivos.Add("Los proveedores de correo no aceptaron el envío; revisa el registro del servidor.");

            return string.Join(" ", motivos);
        }

        private static string ConstruirCorreoReporte(string nombreDestino, string descripcionFormato, string remitente, string remitenteCargo, string mensajeAdicional)
        {
            var cargo = string.IsNullOrWhiteSpace(remitenteCargo) ? "Secretaría de Energía · DGMESNIE" : System.Net.WebUtility.HtmlEncode(remitenteCargo);
            var saludo = string.IsNullOrWhiteSpace(nombreDestino) ? "Estimada(o)" : $"Estimada(o) {System.Net.WebUtility.HtmlEncode(nombreDestino)}";
            var extra = string.IsNullOrWhiteSpace(mensajeAdicional)
                ? string.Empty
                : $"<p style=\"margin:0 0 16px;color:#3a3a3a;font-size:14px;line-height:1.6\">{System.Net.WebUtility.HtmlEncode(mensajeAdicional)}</p>";

            return $@"
<div style=""font-family:'Segoe UI',Arial,sans-serif;max-width:600px;margin:0 auto;border:1px solid #ece8e2;border-radius:12px;overflow:hidden"">
  <div style=""background:#9B2247;padding:22px 28px"">
    <div style=""color:#fff;font-size:13px;font-weight:700;letter-spacing:.12em;text-transform:uppercase"">Secretaría de Energía · DGMESNIE</div>
    <div style=""color:#F5D9E2;font-size:12px;margin-top:4px"">Dashboard de análisis territorial</div>
  </div>
  <div style=""padding:26px 28px"">
    <p style=""margin:0 0 16px;color:#1c1b1a;font-size:15px"">{saludo}:</p>
    <p style=""margin:0 0 16px;color:#3a3a3a;font-size:14px;line-height:1.6"">
      {System.Net.WebUtility.HtmlEncode(remitente)} le comparte el <strong>reporte de análisis territorial</strong>
      generado desde el Dashboard de Proyectos Energéticos de la DGMESNIE.
    </p>
    {extra}
    <p style=""margin:0 0 16px;color:#3a3a3a;font-size:14px;line-height:1.6"">
      Encontrará el reporte adjunto en formato <strong>{System.Net.WebUtility.HtmlEncode(descripcionFormato)}</strong>, con índice navegable,
      métricas territoriales y trazabilidad de las capas analizadas.
    </p>
    <div style=""margin:20px 0;padding:14px 18px;border-left:4px solid #E0A12E;background:#faf8f5;color:#5f5954;font-size:13px"">
      Documento informativo de trabajo. Los resultados dependen de la fecha de corte y las fuentes vigentes.
    </div>
    <p style=""margin:16px 0 0;color:#6F6B66;font-size:13px"">Atentamente,<br><strong>{System.Net.WebUtility.HtmlEncode(remitente)}</strong><br>{cargo}</p>
  </div>
  <div style=""background:#faf8f5;padding:14px 28px;border-top:1px solid #ece8e2;color:#9A958E;font-size:11px"">
    Correo generado automáticamente por la plataforma DGMESNIE. Por favor no responda a este mensaje.
  </div>
</div>";
        }
    }
}
