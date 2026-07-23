using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Http;
using Newtonsoft.Json;
using NSIE.Models;
using NSIE.Servicios;

namespace NSIE.Controllers
{
    public class DashboardProyectosController : Controller
    {
        private readonly IPamrntProyectosIdentificadosService _pamService;
        private readonly IServicioEmailSMTP _emailService;
        private readonly ILogger<DashboardProyectosController> _logger;

        public DashboardProyectosController(
            IPamrntProyectosIdentificadosService pamService,
            IServicioEmailSMTP emailService,
            ILogger<DashboardProyectosController> logger)
        {
            _pamService = pamService;
            _emailService = emailService;
            _logger = logger;
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

            if (adjunto.Length == 0 || adjunto.Length > 25 * 1024 * 1024)
                return BadRequest(new { ok = false, mensaje = "El archivo está vacío o supera el límite de 25 MB." });

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
                }
            }

            if (enviados.Count == 0)
                return StatusCode(500, new { ok = false, mensaje = "No fue posible enviar el reporte a ningún destinatario." });

            var mensaje = $"Reporte enviado a {enviados.Count} destinatario(s): {string.Join(", ", enviados)}.";
            if (fallidos.Count > 0) mensaje += $" No se pudo enviar a: {string.Join(", ", fallidos)}.";
            return Ok(new { ok = true, mensaje });
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
