using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Net;
using System.Net.Mail;
using System.Security.Claims;
using System.Threading.Tasks;
using ClosedXML.Excel;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Logging;
using Newtonsoft.Json;
using NSIE.Models;
using NSIE.Models.Gestor;
using NSIE.Servicios;
using NSIE.Servicios.Interfaces;


namespace NSIE.Controllers
{
    [ServiceFilter(typeof(ValidacionInputFiltro))]
    [AutorizacionFiltro]
    public class GruposInteresController : Controller
    {
        private readonly IRepositorioGruposInteres _repo;
        private readonly IServicioEmailSMTP _servicioEmailSMTP;
        private readonly ILogger<GruposInteresController> _logger;

        public GruposInteresController(
            IRepositorioGruposInteres repo, 
            IServicioEmailSMTP servicioEmailSMTP, 
            ILogger<GruposInteresController> logger)
        {
            _repo = repo;
            _servicioEmailSMTP = servicioEmailSMTP;
            _logger = logger;
        }

        [HttpGet]
        public IActionResult Index()
        {
            ViewData["HeaderViewModel"] = BuildHeader();
            return View();
        }

        [HttpGet("GruposInteres/Api/ObtenerTodos")]
        public async Task<IActionResult> ObtenerTodos()
        {
            try
            {
                var geis = await _repo.ObtenerTodosAsync();
                return Json(geis);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error en API ObtenerTodos.");
                return StatusCode(500, new { error = "Error interno al obtener los Grupos de Interés." });
            }
        }

        [HttpGet("GruposInteres/Api/ObtenerDetalle/{id:int}")]
        public async Task<IActionResult> ObtenerDetalle(int id)
        {
            try
            {
                var gei = await _repo.ObtenerPorIdAsync(id);
                if (gei == null) return NotFound(new { error = "Grupo de Interés no encontrado." });
                return Json(gei);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error en API ObtenerDetalle para ID {Id}.", id);
                return StatusCode(500, new { error = "Error interno al obtener el detalle." });
            }
        }

        [HttpPost("GruposInteres/Api/Guardar")]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Guardar([FromBody] GrupoInteresForm form)
        {
            if (!ModelState.IsValid)
                return BadRequest(ModelState);

            try
            {
                var userId = GetCurrentUserId();
                if (form.GrupoInteresId.HasValue && form.GrupoInteresId.Value > 0)
                {
                    await _repo.ActualizarAsync(form, userId);
                    var updated = await _repo.ObtenerPorIdAsync(form.GrupoInteresId.Value);
                    return Json(new { success = true, data = updated, mensaje = "Grupo de Interés actualizado con éxito." });
                }
                else
                {
                    var newId = await _repo.CrearAsync(form, userId);
                    var created = await _repo.ObtenerPorIdAsync(newId);
                    return CreatedAtAction(nameof(ObtenerDetalle), new { id = newId }, new { success = true, data = created, mensaje = "Grupo de Interés creado con éxito." });
                }
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error en API Guardar.");
                return StatusCode(500, new { error = "Ocurrió un error al guardar la información en la base de datos." });
            }
        }

        [HttpDelete("GruposInteres/Api/Eliminar/{id:int}")]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Eliminar(int id)
        {
            try
            {
                await _repo.EliminarAsync(id);
                return Ok(new { success = true, mensaje = "Registro eliminado con éxito." });
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error en API Eliminar para ID {Id}.", id);
                return StatusCode(500, new { error = "Ocurrió un error al intentar eliminar el registro." });
            }
        }

        [HttpGet("GruposInteres/Api/Dashboard")]
        public async Task<IActionResult> ObtenerDashboard()
        {
            try
            {
                var data = await _repo.ObtenerDashboardDataAsync();
                return Json(data);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error en API Dashboard.");
                return StatusCode(500, new { error = "Error al cargar la información del Dashboard." });
            }
        }

        [HttpGet("GruposInteres/ExportarExcel")]
        public async Task<IActionResult> ExportarExcel()
        {
            try
            {
                var geis = await _repo.ObtenerTodosAsync();

                using var workbook = new XLWorkbook();
                var worksheet = workbook.Worksheets.Add("Grupos de Interés");

                // Encabezado institucional (Guinda, negrita, texto blanco)
                var headerRow = worksheet.Row(1);
                headerRow.Style.Fill.BackgroundColor = XLColor.FromHtml("#9B2247");
                headerRow.Style.Font.Bold = true;
                headerRow.Style.Font.FontColor = XLColor.White;

                worksheet.Cell(1, 1).Value = "ID";
                worksheet.Cell(1, 2).Value = "Grupo de Interés / Desarrollador";
                worksheet.Cell(1, 3).Value = "País de Origen";
                worksheet.Cell(1, 4).Value = "Contacto Principal";
                worksheet.Cell(1, 5).Value = "Correo";
                worksheet.Cell(1, 6).Value = "Teléfono";
                worksheet.Cell(1, 7).Value = "Total Proyectos / Empresas";

                int rowIdx = 2;
                foreach (var g in geis)
                {
                    worksheet.Cell(rowIdx, 1).Value = g.GrupoInteresId;
                    worksheet.Cell(rowIdx, 2).Value = g.Nombre;
                    worksheet.Cell(rowIdx, 3).Value = string.IsNullOrWhiteSpace(g.Origen) ? "No especificado" : g.Origen;
                    worksheet.Cell(rowIdx, 4).Value = string.IsNullOrWhiteSpace(g.Contacto) ? "-" : g.Contacto;
                    worksheet.Cell(rowIdx, 5).Value = string.IsNullOrWhiteSpace(g.Correo) ? "-" : g.Correo;
                    worksheet.Cell(rowIdx, 6).Value = string.IsNullOrWhiteSpace(g.Telefono) ? "-" : g.Telefono;
                    worksheet.Cell(rowIdx, 7).Value = g.TotalProyectos;
                    rowIdx++;
                }

                worksheet.Columns().AdjustToContents();

                using var stream = new MemoryStream();
                workbook.SaveAs(stream);
                var content = stream.ToArray();

                return File(content, "application/vnd.openxmlformats-officedocument.spreadsheetml.sheet", "Grupos_de_Interes_DGMESNIE.xlsx");
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error al exportar Excel de Grupos de Interés.");
                return StatusCode(500, "Error interno al generar el archivo Excel.");
            }
        }

        [HttpPost("GruposInteres/Api/EnviarReporte")]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> EnviarReporte([FromBody] EnviarReporteRequest request)
        {
            if (request == null || string.IsNullOrWhiteSpace(request.Destinatario))
                return BadRequest(new { error = "El correo electrónico del destinatario es obligatorio." });

            try
            {
                var dbData = await _repo.ObtenerDashboardDataAsync();
                var portalUrl = Url.Action("Index", "GruposInteres", null, protocol: HttpContext.Request.Scheme) ?? string.Empty;
                var cuerpoHtml = ConstruirCorreoReporteDashboard(dbData, portalUrl);
                var asunto = "📊 Reporte Ejecutivo: Grupos de Interés y Desarrolladores DGMESNIE";

                await _servicioEmailSMTP.EnviarCorreo(request.Destinatario, asunto, cuerpoHtml);
                return Ok(new { success = true, mensaje = "El reporte ha sido enviado con éxito." });
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error al enviar reporte de correo.");
                return StatusCode(500, new { error = "No fue posible enviar el correo. Verifique la configuración." });
            }
        }

        [HttpGet("GruposInteres/Api/UsuariosCorreo")]
        public async Task<IActionResult> ObtenerUsuariosCorreo()
        {
            try
            {
                var usuarios = await _repo.ObtenerUsuariosCorreoAsync();
                return Json(usuarios);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error en API UsuariosCorreo.");
                return StatusCode(500, new { error = "No fue posible cargar la lista de usuarios." });
            }
        }

        [HttpPost("GruposInteres/Api/EnviarRegistro/{id:int}")]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> EnviarRegistro(int id, [FromBody] EnviarRegistroRequest request)
        {
            var destinatarios = NormalizarDestinatarios(request?.Destinatarios);
            if (!destinatarios.Any())
                return BadRequest(new { error = "Debe seleccionar al menos un correo válido." });

            try
            {
                var gei = await _repo.ObtenerPorIdAsync(id);
                if (gei == null) return NotFound(new { error = "Grupo de Interés no encontrado." });

                var portalUrl = Url.Action("Index", "GruposInteres", null, protocol: HttpContext.Request.Scheme) ?? string.Empty;
                var cuerpoHtml = ConstruirCorreoRegistroGrupo(gei, portalUrl);
                var asunto = $"Registro de Grupo de Interés: {gei.Nombre}";

                foreach (var correo in destinatarios)
                {
                    await _servicioEmailSMTP.EnviarCorreo(correo, asunto, cuerpoHtml);
                }

                return Ok(new { success = true, mensaje = $"Registro enviado a {destinatarios.Count} destinatario(s)." });
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error al enviar registro de Grupo de Interés {Id}.", id);
                return StatusCode(500, new { error = "No fue posible enviar el registro. Verifique la configuración de correo." });
            }
        }

        // ── Helpers privados ──────────────────────────────────────────────────
        private int? GetCurrentUserId()
        {
            var perfilJson = HttpContext.Session.GetString("PerfilUsuario");
            if (string.IsNullOrEmpty(perfilJson)) return null;
            try
            {
                var perfil = JsonConvert.DeserializeObject<PerfilUsuario>(perfilJson);
                return perfil != null && int.TryParse(perfil.IdUsuario, out var id) ? id : null;
            }
            catch
            {
                return null;
            }
        }

        private static List<string> NormalizarDestinatarios(IEnumerable<string>? destinatarios)
        {
            if (destinatarios == null) return [];

            return destinatarios
                .SelectMany(x => (x ?? string.Empty).Split(new[] { ',', ';', '\n', '\r' }, StringSplitOptions.RemoveEmptyEntries))
                .Select(x => x.Trim())
                .Where(EsCorreoValido)
                .Distinct(StringComparer.OrdinalIgnoreCase)
                .ToList();
        }

        private static bool EsCorreoValido(string correo)
        {
            if (string.IsNullOrWhiteSpace(correo)) return false;
            try
            {
                var address = new MailAddress(correo);
                return string.Equals(address.Address, correo, StringComparison.OrdinalIgnoreCase);
            }
            catch
            {
                return false;
            }
        }

        private static string H(string? value) => WebUtility.HtmlEncode(string.IsNullOrWhiteSpace(value) ? "-" : value);

        private static string ConstruirCorreoReporteDashboard(GruposInteresDashboardVM data, string portalUrl)
        {
            var topRows = string.Empty;
            foreach (var g in data.TopGruposProyectos.Take(5))
            {
                topRows += $@"
                    <tr>
                        <td style='padding:8px 10px; border:1px solid #ddd; font-weight:700;'>{g.Nombre}</td>
                        <td style='padding:8px 10px; border:1px solid #ddd; text-align:center;'>{g.Origen ?? "N/E"}</td>
                        <td style='padding:8px 10px; border:1px solid #ddd; text-align:right; font-weight:700; color:#8a0031;'>{g.TotalProyectos}</td>
                    </tr>";
            }

            return $@"
                <html lang='es'>
                <head>
                    <meta charset='UTF-8'>
                    <title>Reporte de Grupos de Interés</title>
                </head>
                <body style='margin:0; padding:20px; background:#f5f5f5; font-family:Arial, sans-serif; color:#333;'>
                    <div style='max-width:700px; margin:0 auto; background:#ffffff; border:1px solid #ddd; border-radius:8px; overflow:hidden;'>
                        <div style='padding:15px 20px; border-bottom:1px solid #eee;'>
                            <table role='presentation' cellpadding='0' cellspacing='0' border='0' style='width:100%;'>
                                <tr>
                                    <td><img src='https://cdn.sassoapps.com/dgmesnie/logo_gob.png' alt='GobMX' style='max-height:36px;'></td>
                                    <td style='text-align:right;'><img src='https://cdn.sassoapps.com/dgmesnie/logo_sener.png' alt='SENER' style='max-height:38px;'></td>
                                </tr>
                            </table>
                        </div>
                        <div style='background:#9B2247; color:#ffffff; padding:15px 20px; font-size:18px; font-weight:bold;'>
                            Reporte Ejecutivo: Grupos de Interés y Desarrolladores
                        </div>
                        <div style='padding:20px;'>
                            <p style='margin:0 0 15px; font-size:15px;'>Se adjunta el resumen del estado actual de los Grupos Económicos de Interés registrados en el sistema:</p>
                            
                            <!-- Indicadores principales -->
                            <table role='presentation' style='width:100%; margin-bottom:20px;'>
                                <tr>
                                    <td style='width:33%; padding:10px; background:#fcf8f9; border:1px solid #e5c7d4; border-radius:5px; text-align:center;'>
                                        <div style='font-size:12px; color:#6b1034; text-transform:uppercase; font-weight:bold;'>Grupos Económicos</div>
                                        <div style='font-size:24px; font-weight:bold; color:#9B2247; margin-top:5px;'>{data.TotalGrupos}</div>
                                    </td>
                                    <td style='width:33%; padding:10px; background:#f4f7f6; border:1px solid #c9d7d4; border-radius:5px; text-align:center;'>
                                        <div style='font-size:12px; color:#143e36; text-transform:uppercase; font-weight:bold;'>Proyectos / Empresas</div>
                                        <div style='font-size:24px; font-weight:bold; color:#1E5B4F; margin-top:5px;'>{data.TotalProyectos}</div>
                                    </td>
                                    <td style='width:33%; padding:10px; background:#faf9f5; border:1px solid #f2edd5; border-radius:5px; text-align:center;'>
                                        <div style='font-size:12px; color:#70561e; text-transform:uppercase; font-weight:bold;'>Países Origen</div>
                                        <div style='font-size:24px; font-weight:bold; color:#A57F2C; margin-top:5px;'>{data.TotalPaises}</div>
                                    </td>
                                </tr>
                            </table>

                            <h4 style='color:#9B2247; border-bottom:1px solid #eee; padding-bottom:5px; margin:20px 0 10px;'>Top 5 Grupos con Mayor Cantidad de Proyectos</h4>
                            <table role='presentation' style='width:100%; border-collapse:collapse; font-size:13px;'>
                                <thead style='background:#f5f5f5;'>
                                    <tr>
                                        <th style='padding:8px; border:1px solid #ddd; text-align:left;'>Grupo de Interés</th>
                                        <th style='padding:8px; border:1px solid #ddd; text-align:center; width:25%;'>Origen</th>
                                        <th style='padding:8px; border:1px solid #ddd; text-align:right; width:20%;'>Proyectos</th>
                                    </tr>
                                </thead>
                                <tbody>
                                    {topRows}
                                </tbody>
                            </table>

                            <div style='margin-top:25px; text-align:center;'>
                                <a href='{portalUrl}' style='display:inline-block; padding:10px 18px; border-radius:4px; background:#9B2247; color:#ffffff; text-decoration:none; font-weight:bold; font-size:14px;'>
                                    Ver en el Portal SNIER
                                </a>
                            </div>
                        </div>
                    </div>
                </body>
                </html>";
        }

        private static string ConstruirCorreoRegistroGrupo(GrupoInteres gei, string portalUrl)
        {
            var proyectos = gei.Proyectos ?? [];
            var rows = proyectos.Any()
                ? string.Concat(proyectos.Select(p => $@"
                    <tr>
                        <td style='padding:8px 10px; border:1px solid #ddd;'>{H(p.RazonSocial)}</td>
                        <td style='padding:8px 10px; border:1px solid #ddd;'>{H(p.NombreProyecto)}</td>
                        <td style='padding:8px 10px; border:1px solid #ddd;'>{H(p.Contacto)}</td>
                        <td style='padding:8px 10px; border:1px solid #ddd;'>{H(p.Correo)}</td>
                        <td style='padding:8px 10px; border:1px solid #ddd;'>{H(p.Telefono)}</td>
                    </tr>"))
                : @"
                    <tr>
                        <td colspan='5' style='padding:12px; border:1px solid #ddd; color:#666; text-align:center;'>Sin proyectos o razones sociales asociadas.</td>
                    </tr>";

            return $@"
                <html lang='es'>
                <head>
                    <meta charset='UTF-8'>
                    <title>Registro de Grupo de Interés</title>
                </head>
                <body style='margin:0; padding:20px; background:#f5f5f5; font-family:Arial, sans-serif; color:#333;'>
                    <div style='max-width:820px; margin:0 auto; background:#ffffff; border:1px solid #ddd; border-radius:8px; overflow:hidden;'>
                        <div style='padding:15px 20px; border-bottom:1px solid #eee;'>
                            <table role='presentation' cellpadding='0' cellspacing='0' border='0' style='width:100%;'>
                                <tr>
                                    <td><img src='https://cdn.sassoapps.com/dgmesnie/logo_gob.png' alt='GobMX' style='max-height:36px;'></td>
                                    <td style='text-align:right;'><img src='https://cdn.sassoapps.com/dgmesnie/logo_sener.png' alt='SENER' style='max-height:38px;'></td>
                                </tr>
                            </table>
                        </div>
                        <div style='background:#9B2247; color:#ffffff; padding:15px 20px; font-size:18px; font-weight:bold;'>
                            Registro de Grupo de Interés
                        </div>
                        <div style='padding:20px;'>
                            <table role='presentation' style='width:100%; border-collapse:collapse; font-size:13px; margin-bottom:20px;'>
                                <tbody>
                                    <tr>
                                        <th style='padding:8px 10px; border:1px solid #ddd; text-align:left; width:28%; background:#f5f5f5;'>Grupo / Desarrollador</th>
                                        <td style='padding:8px 10px; border:1px solid #ddd; font-weight:700;'>{H(gei.Nombre)}</td>
                                    </tr>
                                    <tr>
                                        <th style='padding:8px 10px; border:1px solid #ddd; text-align:left; background:#f5f5f5;'>País de origen</th>
                                        <td style='padding:8px 10px; border:1px solid #ddd;'>{H(gei.Origen)}</td>
                                    </tr>
                                    <tr>
                                        <th style='padding:8px 10px; border:1px solid #ddd; text-align:left; background:#f5f5f5;'>Contacto principal</th>
                                        <td style='padding:8px 10px; border:1px solid #ddd;'>{H(gei.Contacto)}</td>
                                    </tr>
                                    <tr>
                                        <th style='padding:8px 10px; border:1px solid #ddd; text-align:left; background:#f5f5f5;'>Correo</th>
                                        <td style='padding:8px 10px; border:1px solid #ddd;'>{H(gei.Correo)}</td>
                                    </tr>
                                    <tr>
                                        <th style='padding:8px 10px; border:1px solid #ddd; text-align:left; background:#f5f5f5;'>Teléfono</th>
                                        <td style='padding:8px 10px; border:1px solid #ddd;'>{H(gei.Telefono)}</td>
                                    </tr>
                                </tbody>
                            </table>

                            <h4 style='color:#9B2247; border-bottom:1px solid #eee; padding-bottom:5px; margin:20px 0 10px;'>Proyectos y empresas asociadas</h4>
                            <table role='presentation' style='width:100%; border-collapse:collapse; font-size:13px;'>
                                <thead style='background:#9B2247; color:#ffffff;'>
                                    <tr>
                                        <th style='padding:8px; border:1px solid #8a1e3f; text-align:left;'>Razón social</th>
                                        <th style='padding:8px; border:1px solid #8a1e3f; text-align:left;'>Proyecto</th>
                                        <th style='padding:8px; border:1px solid #8a1e3f; text-align:left;'>Contacto</th>
                                        <th style='padding:8px; border:1px solid #8a1e3f; text-align:left;'>Correo</th>
                                        <th style='padding:8px; border:1px solid #8a1e3f; text-align:left;'>Teléfono</th>
                                    </tr>
                                </thead>
                                <tbody>
                                    {rows}
                                </tbody>
                            </table>

                            <div style='margin-top:25px; text-align:center;'>
                                <a href='{H(portalUrl)}' style='display:inline-block; padding:10px 18px; border-radius:4px; background:#9B2247; color:#ffffff; text-decoration:none; font-weight:bold; font-size:14px;'>
                                    Ver módulo en el Portal SNIER
                                </a>
                            </div>
                        </div>
                    </div>
                </body>
                </html>";
        }

        private HeaderViewModel BuildHeader() => new()
        {
            Title = "Grupos de Interés",
            IconPath = "organigrama.png",
            Description = "Administración, control y visualización de Grupos Económicos de Interés y Desarrolladores del sector energético.",
            Section = "Grupos de Interés",
            ModuleInfo = JsonConvert.SerializeObject(new
            {
                title = "Grupos de Interés y Desarrolladores DGMESNIE",
                description = "Módulo especializado para el registro, consulta y distribución de los grupos económicos y sus proyectos asociados.",
                functionality = "Administración CRUD completa de empresas, segmentación por países, e indicadores interactivos de distribución.",
                stage = "Gestión Estratégica",
                highlights = new[]
                {
                    "Carga inicial directa desde bases de datos consolidadas de GEIs.",
                    "Distribución geopolítica de desarrolladores internacionales.",
                    "Exportación a formatos estructurados Excel y PDF.",
                    "Envío integrado de reportes a través de correo electrónico."
                },
                roles = new[]
                {
                    new { icon = "clipboard-check", text = "Planeación y análisis estratégico del sector." },
                    new { icon = "globe", text = "Coordinación internacional y seguimiento de inversiones." }
                },
                order = new { step = 2, description = "Control de desarrolladores y grupos económicos" },
                context = "Módulo enlazado dinámicamente al sistema de permisos y roles del SNIER.",
                manualUrl = string.Empty
            })
        };
    }

    public class EnviarReporteRequest
    {
        public string Destinatario { get; set; } = string.Empty;
    }

    public class EnviarRegistroRequest
    {
        public List<string> Destinatarios { get; set; } = [];
    }
}
