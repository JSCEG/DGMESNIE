using System;
using System.Collections.Generic;
using System.Data;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Data.SqlClient;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.Logging;
using Newtonsoft.Json;
using NSIE.Models;
using Dapper;

namespace NSIE.Controllers
{
    [ServiceFilter(typeof(ValidacionInputFiltro))]
    [AutorizacionFiltro]
    public class AsuntosComiteController : Controller
    {
        private readonly string _connectionString;
        private readonly ILogger<AsuntosComiteController> _logger;

        public AsuntosComiteController(IConfiguration configuration, ILogger<AsuntosComiteController> logger)
        {
            _connectionString = configuration.GetConnectionString("DefaultConnection");
            _logger = logger;
        }

        // --- MAIN VIEW ---
        [HttpGet]
        public async Task<IActionResult> Index()
        {
            try
            {
                ViewData["HeaderViewModel"] = BuildHeader();
                ViewData["IsAdmin"] = IsUserAdmin();

                using (var connection = new SqlConnection(_connectionString))
                {
                    await connection.OpenAsync();
                    
                    string query = "";
                    if (IsUserAdmin())
                    {
                        query = @"
                            SELECT 
                                SesionId, 
                                Titulo, 
                                CONVERT(VARCHAR(10), Fecha, 120) AS FechaTexto, 
                                Resumen, 
                                PdfUrl, 
                                PptUrl, 
                                CanvaEmbedUrl, 
                                Activo 
                            FROM dgmesnie.ComiteSesion 
                            ORDER BY Fecha DESC, SesionId DESC";
                    }
                    else
                    {
                        query = @"
                            SELECT 
                                SesionId, 
                                Titulo, 
                                CONVERT(VARCHAR(10), Fecha, 120) AS FechaTexto, 
                                Resumen, 
                                PdfUrl, 
                                PptUrl, 
                                CanvaEmbedUrl, 
                                Activo 
                            FROM dgmesnie.ComiteSesion 
                            WHERE Activo = 1 
                            ORDER BY Fecha DESC, SesionId DESC";
                    }

                    var sesiones = (await connection.QueryAsync<ComiteSesionDto>(query)).ToList();
                    return View(sesiones);
                }
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error al cargar la vista de Asuntos del Comité.");
                return View(new List<ComiteSesionDto>());
            }
        }

        // --- GET DETAIL BY ID (API) ---
        [HttpGet("AsuntosComite/Api/ObtenerDetalle/{id:int}")]
        public async Task<IActionResult> ObtenerDetalle(int id)
        {
            try
            {
                using (var connection = new SqlConnection(_connectionString))
                {
                    await connection.OpenAsync();
                    
                    string query = @"
                        SELECT 
                            SesionId, 
                            Titulo, 
                            CONVERT(VARCHAR(10), Fecha, 120) AS FechaTexto, 
                            Resumen, 
                            PdfUrl, 
                            PptUrl, 
                            CanvaEmbedUrl, 
                            Activo 
                        FROM dgmesnie.ComiteSesion 
                        WHERE SesionId = @Id";
                    
                    var sesion = await connection.QueryFirstOrDefaultAsync<ComiteSesionDto>(query, new { Id = id });
                    
                    if (sesion == null)
                    {
                        return NotFound(new { error = "Sesión no encontrada." });
                    }
                    
                    return Json(sesion);
                }
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error al obtener detalle de la sesión {Id}.", id);
                return StatusCode(500, new { error = "Error interno al obtener el detalle." });
            }
        }

        // --- CREATE / EDIT SESSION (API) ---
        [HttpPost("AsuntosComite/Api/Guardar")]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Guardar([FromBody] ComiteSesionForm form)
        {
            if (!IsUserAdmin())
            {
                return StatusCode(403, new { error = "No tienes permisos para realizar esta acción." });
            }

            if (form == null || !ModelState.IsValid)
            {
                return BadRequest(new { error = "Información del formulario no válida." });
            }

            try
            {
                var userId = GetCurrentUserId()?.ToString() ?? "UsuarioPortal";
                string pdfUrl = string.IsNullOrWhiteSpace(form.PdfUrl) ? null : form.PdfUrl.Trim();
                string pptUrl = string.IsNullOrWhiteSpace(form.PptUrl) ? null : form.PptUrl.Trim();
                string canvaEmbedUrl = SanitizarUrlEmbebida(form.CanvaEmbedUrl);

                using (var connection = new SqlConnection(_connectionString))
                {
                    await connection.OpenAsync();

                    if (form.SesionId.HasValue && form.SesionId.Value > 0)
                    {
                        // UPDATE
                        string queryUpdate = @"
                            UPDATE dgmesnie.ComiteSesion
                            SET Titulo = @Titulo,
                                Fecha = @Fecha,
                                Resumen = @Resumen,
                                PdfUrl = @PdfUrl,
                                PptUrl = @PptUrl,
                                CanvaEmbedUrl = @CanvaEmbedUrl,
                                Activo = @Activo,
                                ActualizadoEn = SYSUTCDATETIME(),
                                ActualizadoPor = @UserId
                            WHERE SesionId = @SesionId";

                        await connection.ExecuteAsync(queryUpdate, new
                        {
                            SesionId = form.SesionId.Value,
                            form.Titulo,
                            Fecha = form.Fecha,
                            form.Resumen,
                            PdfUrl = pdfUrl,
                            PptUrl = pptUrl,
                            CanvaEmbedUrl = canvaEmbedUrl,
                            Activo = form.Activo,
                            UserId = userId
                        });

                        return Json(new { success = true, mensaje = "Sesión actualizada correctamente." });
                    }
                    else
                    {
                        // INSERT
                        string queryInsert = @"
                            INSERT INTO dgmesnie.ComiteSesion 
                            (Titulo, Fecha, Resumen, PdfUrl, PptUrl, CanvaEmbedUrl, Activo, CreadoEn, CreadoPor)
                            VALUES 
                            (@Titulo, @Fecha, @Resumen, @PdfUrl, @PptUrl, @CanvaEmbedUrl, @Activo, SYSUTCDATETIME(), @UserId)";

                        await connection.ExecuteAsync(queryInsert, new
                        {
                            form.Titulo,
                            Fecha = form.Fecha,
                            form.Resumen,
                            PdfUrl = pdfUrl,
                            PptUrl = pptUrl,
                            CanvaEmbedUrl = canvaEmbedUrl,
                            Activo = form.Activo,
                            UserId = userId
                        });

                        return Json(new { success = true, mensaje = "Sesión registrada correctamente." });
                    }
                }
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error al guardar sesión del comité.");
                return StatusCode(500, new { error = "Ocurrió un error al guardar la información en la base de datos." });
            }
        }

        // --- DELETE (SOFT DELETE) SESSION (API) ---
        [HttpPost("AsuntosComite/Api/Eliminar/{id:int}")]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Eliminar(int id)
        {
            if (!IsUserAdmin())
            {
                return StatusCode(403, new { error = "No tienes permisos para realizar esta acción." });
            }

            try
            {
                var userId = GetCurrentUserId()?.ToString() ?? "UsuarioPortal";

                using (var connection = new SqlConnection(_connectionString))
                {
                    await connection.OpenAsync();

                    // Soft delete: sets Activo = 0
                    string query = @"
                        UPDATE dgmesnie.ComiteSesion 
                        SET Activo = 0, 
                            ActualizadoEn = SYSUTCDATETIME(), 
                            ActualizadoPor = @UserId 
                        WHERE SesionId = @Id";

                    await connection.ExecuteAsync(query, new { Id = id, UserId = userId });
                }

                return Json(new { success = true, mensaje = "Sesión eliminada correctamente." });
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error al eliminar sesión {Id}.", id);
                return StatusCode(500, new { error = "Ocurrió un error al intentar eliminar la sesión." });
            }
        }

        // ── Helpers Privados ──────────────────────────────────────────────────
        
        private HeaderViewModel BuildHeader() => new HeaderViewModel
        {
            Title = "Comité Técnico de la Comisión Nacional de Energía",
            IconPath = "organigrama.png",
            Description = "Seguimiento de las sesiones del comité técnico, visualización de minutas, presentaciones e integración de Canva.",
            Section = "Seguimiento",
            ModuleInfo = JsonConvert.SerializeObject(new
            {
                title = "Comité Técnico de la Comisión Nacional de Energía",
                description = "Espacio de control y consulta de las actas, resúmenes y presentaciones del Comité Técnico.",
                functionality = "Visualización interactiva de minutas, descarga de archivos fuente en PDF y PowerPoint, y visualización de diapositivas embebidas.",
                stage = "Seguimiento y Control",
                highlights = new[]
                {
                    "Acceso rápido a los resúmenes y acuerdos de cada sesión ordinaria y extraordinaria.",
                    "Visualización directa de las presentaciones de Canva embebidas.",
                    "Descarga directa de los archivos originales PDF y PowerPoint desde el CDN."
                },
                roles = new[]
                {
                    new { icon = "people", text = "Integrantes del Comité Técnico de la CNE." },
                    new { icon = "shield-lock", text = "Administradores autorizados para registrar y editar sesiones." }
                },
                order = new { step = 3, description = "Seguimiento de Acuerdos del Comité" },
                context = "Concentra el archivo histórico de sesiones oficiales del Comité.",
                manualUrl = string.Empty
            })
        };

        private string SanitizarUrlEmbebida(string url)
        {
            if (string.IsNullOrWhiteSpace(url)) return null;
            
            url = url.Trim();
            
            // 1. Si el usuario pegó el código HTML completo (iframe o div con iframe)
            if (url.Contains("<iframe") || url.Contains("<div"))
            {
                var match = System.Text.RegularExpressions.Regex.Match(url, @"src=[""']([^""']+)[""']", System.Text.RegularExpressions.RegexOptions.IgnoreCase);
                if (match.Success)
                {
                    url = match.Groups[1].Value;
                }
            }

            // 2. Quitar comillas remanentes del atributo src
            if (url.Contains("\""))
            {
                url = url.Split('"')[0];
            }
            if (url.Contains("'"))
            {
                url = url.Split('\'')[0];
            }

            url = url.Trim();

            // 3. Auto-conversión de links de Canva a embed
            if (url.Contains("canva.com/design/") && !url.Contains("embed"))
            {
                if (url.Contains("?"))
                {
                    url += "&embed";
                }
                else
                {
                    url += "?embed";
                }
            }

            // 4. Auto-conversión de Google Slides pub a embed
            if (url.Contains("docs.google.com/presentation/") && url.Contains("/pub"))
            {
                url = url.Replace("/pub", "/embed");
            }

            return url;
        }

        private bool IsUserAdmin()
        {
            var perfilJson = HttpContext.Session.GetString("PerfilUsuario");
            if (string.IsNullOrEmpty(perfilJson)) return false;
            try
            {
                var perfil = JsonConvert.DeserializeObject<PerfilUsuario>(perfilJson);
                if (perfil == null) return false;
                
                return perfil.IdUsuario == "1" || perfil.IdUsuario == "86" || perfil.Rol == "Administrador" || perfil.Rol_Nombre == "Administrador";
            }
            catch
            {
                return false;
            }
        }

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
    }

    // --- DTOs y Modelos de Entrada ---
    
    public class ComiteSesionDto
    {
        public int SesionId { get; set; }
        public string Titulo { get; set; }
        public string FechaTexto { get; set; }
        public string Resumen { get; set; }
        public string PdfUrl { get; set; }
        public string PptUrl { get; set; }
        public string CanvaEmbedUrl { get; set; }
        public bool Activo { get; set; }
    }

    public class ComiteSesionForm
    {
        public int? SesionId { get; set; }
        
        [System.ComponentModel.DataAnnotations.Required(ErrorMessage = "El título es obligatorio.")]
        public string Titulo { get; set; }
        
        [System.ComponentModel.DataAnnotations.Required(ErrorMessage = "La fecha es obligatoria.")]
        public DateTime Fecha { get; set; }
        
        public string Resumen { get; set; }
        
        public string PdfUrl { get; set; }
        
        public string PptUrl { get; set; }
        
        public string CanvaEmbedUrl { get; set; }
        
        public bool Activo { get; set; } = true;
    }
}
