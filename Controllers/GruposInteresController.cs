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
            var filas = data.TopGruposProyectos
                .Take(5)
                .Select(g => (IReadOnlyList<string>)new[]
                {
                    g.Nombre ?? string.Empty,
                    g.Origen ?? "N/E",
                    g.TotalProyectos.ToString("N0")
                })
                .ToList();

            return PlantillaCorreoInstitucional.Construir(new ContenidoCorreo
            {
                Antetitulo = "Grupos de interés y desarrolladores",
                Titulo = "Reporte ejecutivo de grupos de interés",
                Metadatos = new[]
                {
                    new CampoCorreo("Grupos económicos", data.TotalGrupos.ToString("N0")),
                    new CampoCorreo("Proyectos / empresas", data.TotalProyectos.ToString("N0")),
                    new CampoCorreo("Países de origen", data.TotalPaises.ToString("N0"))
                },
                Parrafos = new[]
                {
                    "Corte del padrón de grupos de interés y desarrolladores registrados en la plataforma de la DGMESNIE."
                },
                Tabla = new TablaCorreo
                {
                    Titulo = "Grupos con más proyectos",
                    Encabezados = new[] { "Grupo", "Origen", "Proyectos" },
                    Filas = filas,
                    TextoVacio = "Sin grupos con proyectos registrados."
                },
                BotonTexto = string.IsNullOrWhiteSpace(portalUrl) ? null : "Abrir el padrón completo",
                BotonUrl = portalUrl,
                Nota = "Las cifras corresponden al momento de generación del reporte.",
                PieAviso = "Este correo se genera automáticamente y no requiere respuesta."
            });
        }

        private static string ConstruirCorreoRegistroGrupo(GrupoInteres gei, string portalUrl)
        {
            var proyectos = gei.Proyectos ?? [];
            var filas = proyectos
                .Select(p => (IReadOnlyList<string>)new[]
                {
                    p.RazonSocial ?? string.Empty,
                    p.NombreProyecto ?? string.Empty,
                    p.Contacto ?? string.Empty,
                    p.Correo ?? string.Empty,
                    p.Telefono ?? string.Empty
                })
                .ToList();

            return PlantillaCorreoInstitucional.Construir(new ContenidoCorreo
            {
                Antetitulo = "Grupos de interés y desarrolladores",
                Titulo = string.IsNullOrWhiteSpace(gei.Nombre) ? "Registro de grupo de interés" : gei.Nombre,
                Parrafos = new[] { "Se registró el siguiente grupo de interés en la plataforma de la DGMESNIE." },
                Datos = new[]
                {
                    new CampoCorreo("Grupo / desarrollador", gei.Nombre),
                    new CampoCorreo("País de origen", gei.Origen),
                    new CampoCorreo("Contacto principal", gei.Contacto),
                    new CampoCorreo("Correo", gei.Correo),
                    new CampoCorreo("Teléfono", gei.Telefono)
                },
                Tabla = new TablaCorreo
                {
                    Titulo = "Proyectos y empresas asociadas",
                    Encabezados = new[] { "Razón social", "Proyecto", "Contacto", "Correo", "Teléfono" },
                    Filas = filas,
                    TextoVacio = "Sin proyectos o razones sociales asociadas."
                },
                BotonTexto = string.IsNullOrWhiteSpace(portalUrl) ? null : "Abrir el padrón",
                BotonUrl = portalUrl,
                PieAviso = "Este correo se genera automáticamente y no requiere respuesta."
            });
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
