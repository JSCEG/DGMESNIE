using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Hosting;
using Microsoft.AspNetCore.Http;
using Newtonsoft.Json;
using System;
using System.IO;
using System.Collections.Generic;
using System.Threading.Tasks;
using System.Linq;
using NSIE.Models;
using Dapper;
using Microsoft.Data.SqlClient;
using System.Data;
using Microsoft.Extensions.Configuration;

namespace NSIE.Controllers
{
    [ServiceFilter(typeof(ValidacionInputFiltro))]
    [AutorizacionFiltro]
    public class TransparenciaController : Controller
    {
        private readonly IWebHostEnvironment _webHostEnvironment;
        private readonly string _connectionString;

        public TransparenciaController(IWebHostEnvironment webHostEnvironment, IConfiguration configuration)
        {
            _webHostEnvironment = webHostEnvironment;
            _connectionString = configuration.GetConnectionString("DefaultConnection");
        }

        // --- PUBLIC VIEW ---
        public async Task<IActionResult> Index()
        {
            try
            {
                using (var connection = new SqlConnection(_connectionString))
                {
                    await connection.OpenAsync();

                    var queryAsuntos = @"
                        SELECT 
                            AsuntoId AS Id,
                            Titulo,
                            CONVERT(VARCHAR(10), Fecha, 120) AS Fecha,
                            Estatus,
                            FolioSolicitud,
                            NumeroExpediente,
                            Descripcion,
                            SharePointUrl,
                            AudioEmbedUrl,
                            InfografiaEmbedUrl,
                            PresentacionEmbedUrl,
                            Activo
                        FROM dgmesnie.TransparenciaAsunto
                        WHERE Activo = 1
                        ORDER BY Fecha DESC, AsuntoId DESC";

                    var asuntos = (await connection.QueryAsync<TransparenciaAsunto>(queryAsuntos)).ToList();

                    var queryHitos = @"
                        SELECT 
                            HitoId,
                            AsuntoId,
                            CONVERT(VARCHAR(10), Fecha, 120) AS Fecha,
                            Titulo,
                            Descripcion,
                            Orden
                        FROM dgmesnie.TransparenciaHito
                        WHERE Activo = 1
                        ORDER BY Orden ASC, Fecha ASC";

                    var hitos = (await connection.QueryAsync<TimelineEvent>(queryHitos)).ToList();

                    // Map hitos to their respective asuntos
                    foreach (var asunto in asuntos)
                    {
                        asunto.Timeline = hitos.Where(h => h.AsuntoId == asunto.Id).ToList();
                    }

                    return View(asuntos);
                }
            }
            catch (Exception ex)
            {
                Console.WriteLine("Error in Transparencia/Index: " + ex.Message);
                return View(new List<TransparenciaAsunto>());
            }
        }

        // --- ADMIN DASHBOARD ---
        [HttpGet]
        public async Task<IActionResult> Administrar()
        {
            if (!IsUserAdmin())
            {
                return RedirectToAction("Index", "Home");
            }

            try
            {
                using (var connection = new SqlConnection(_connectionString))
                {
                    var query = @"
                        SELECT 
                            AsuntoId AS Id,
                            Titulo,
                            CONVERT(VARCHAR(10), Fecha, 120) AS Fecha,
                            Estatus,
                            FolioSolicitud,
                            NumeroExpediente,
                            Descripcion,
                            SharePointUrl,
                            AudioEmbedUrl,
                            InfografiaEmbedUrl,
                            PresentacionEmbedUrl,
                            Activo
                        FROM dgmesnie.TransparenciaAsunto
                        WHERE Activo = 1
                        ORDER BY Fecha DESC, AsuntoId DESC";
                    var asuntos = (await connection.QueryAsync<TransparenciaAsunto>(query)).ToList();
                    return View(asuntos);
                }
            }
            catch (Exception ex)
            {
                ViewData["ErrorMessage"] = "Error al obtener la lista de asuntos: " + ex.Message;
                return View(new List<TransparenciaAsunto>());
            }
        }

        // --- CREATE MATTER ---
        [HttpGet]
        public IActionResult Crear()
        {
            if (!IsUserAdmin()) return RedirectToAction("Index", "Home");
            
            var model = new TransparenciaAsunto
            {
                Fecha = DateTime.Today.ToString("yyyy-MM-dd"),
                Estatus = "Pendiente",
                Activo = true
            };
            return View(model);
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Crear(TransparenciaAsunto model, string timelineJson)
        {
            if (!IsUserAdmin()) return RedirectToAction("Index", "Home");

            if (!ModelState.IsValid)
            {
                return View(model);
            }

            using (var connection = new SqlConnection(_connectionString))
            {
                await connection.OpenAsync();
                using (var transaction = connection.BeginTransaction())
                {
                    try
                     {
                        var insertAsunto = @"
                            INSERT INTO dgmesnie.TransparenciaAsunto
                            (Titulo, Fecha, Estatus, FolioSolicitud, NumeroExpediente, Descripcion, SharePointUrl, AudioEmbedUrl, InfografiaEmbedUrl, PresentacionEmbedUrl, Activo, CreadoEn, CreadoPor)
                            VALUES
                            (@Titulo, @Fecha, @Estatus, @FolioSolicitud, @NumeroExpediente, @Descripcion, @SharePointUrl, @AudioEmbedUrl, @InfografiaEmbedUrl, @PresentacionEmbedUrl, 1, SYSUTCDATETIME(), @CreadoPor);
                            SELECT SCOPE_IDENTITY();";
                        
                        DateTime.TryParse(model.Fecha, out var parsedDate);

                        var id = await connection.ExecuteScalarAsync<int>(insertAsunto, new {
                            model.Titulo,
                            Fecha = parsedDate,
                            model.Estatus,
                            model.FolioSolicitud,
                            model.NumeroExpediente,
                            model.Descripcion,
                            SharePointUrl = SanitizarUrlEmbebida(model.SharePointUrl),
                            AudioEmbedUrl = SanitizarUrlEmbebida(model.AudioEmbedUrl),
                            InfografiaEmbedUrl = SanitizarUrlEmbebida(model.InfografiaEmbedUrl),
                            PresentacionEmbedUrl = SanitizarUrlEmbebida(model.PresentacionEmbedUrl),
                            CreadoPor = GetCurrentUserId()?.ToString()
                        }, transaction);

                        if (!string.IsNullOrEmpty(timelineJson))
                        {
                            var hitos = JsonConvert.DeserializeObject<List<TimelineEvent>>(timelineJson);
                            if (hitos != null && hitos.Any())
                            {
                                var insertHito = @"
                                    INSERT INTO dgmesnie.TransparenciaHito
                                    (AsuntoId, Fecha, Titulo, Descripcion, Orden, Activo, CreadoEn, CreadoPor)
                                    VALUES
                                    (@AsuntoId, @Fecha, @Titulo, @Descripcion, @Orden, 1, SYSUTCDATETIME(), @CreadoPor)";
                                
                                int orden = 0;
                                foreach (var hito in hitos)
                                {
                                    DateTime.TryParse(hito.Fecha, out var parsedHitoDate);
                                    await connection.ExecuteAsync(insertHito, new {
                                        AsuntoId = id,
                                        Fecha = parsedHitoDate,
                                        hito.Titulo,
                                        hito.Descripcion,
                                        Orden = orden++,
                                        CreadoPor = GetCurrentUserId()?.ToString()
                                    }, transaction);
                                }
                            }
                        }

                        transaction.Commit();
                        return RedirectToAction("Administrar");
                    }
                    catch (Exception ex)
                    {
                        transaction.Rollback();
                        ModelState.AddModelError("", "Error al guardar el asunto: " + ex.Message);
                        return View(model);
                    }
                }
            }
        }

        // --- EDIT MATTER ---
        [HttpGet]
        public async Task<IActionResult> Editar(int id)
        {
            if (!IsUserAdmin()) return RedirectToAction("Index", "Home");

            try
            {
                using (var connection = new SqlConnection(_connectionString))
                {
                    var queryAsunto = @"
                        SELECT 
                            AsuntoId AS Id,
                            Titulo,
                            CONVERT(VARCHAR(10), Fecha, 120) AS Fecha,
                            Estatus,
                            FolioSolicitud,
                            NumeroExpediente,
                            Descripcion,
                            SharePointUrl,
                            AudioEmbedUrl,
                            InfografiaEmbedUrl,
                            PresentacionEmbedUrl,
                            Activo
                        FROM dgmesnie.TransparenciaAsunto
                        WHERE AsuntoId = @Id";
                    
                    var model = await connection.QueryFirstOrDefaultAsync<TransparenciaAsunto>(queryAsunto, new { Id = id });
                    if (model == null) return NotFound();

                    var queryHitos = @"
                        SELECT 
                            HitoId,
                            AsuntoId,
                            CONVERT(VARCHAR(10), Fecha, 120) AS Fecha,
                            Titulo,
                            Descripcion,
                            Orden
                        FROM dgmesnie.TransparenciaHito
                        WHERE AsuntoId = @Id AND Activo = 1
                        ORDER BY Orden ASC, Fecha ASC";
                    
                    model.Timeline = (await connection.QueryAsync<TimelineEvent>(queryHitos, new { Id = id })).ToList();

                    return View(model);
                }
            }
            catch (Exception ex)
            {
                return BadRequest("Error al cargar asunto: " + ex.Message);
            }
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Editar(int id, TransparenciaAsunto model, string timelineJson)
        {
            if (!IsUserAdmin()) return RedirectToAction("Index", "Home");

            if (!ModelState.IsValid)
            {
                return View(model);
            }

            using (var connection = new SqlConnection(_connectionString))
            {
                await connection.OpenAsync();
                using (var transaction = connection.BeginTransaction())
                {
                    try
                    {
                        var updateAsunto = @"
                            UPDATE dgmesnie.TransparenciaAsunto
                            SET Titulo = @Titulo,
                                Fecha = @Fecha,
                                Estatus = @Estatus,
                                FolioSolicitud = @FolioSolicitud,
                                NumeroExpediente = @NumeroExpediente,
                                Descripcion = @Descripcion,
                                SharePointUrl = @SharePointUrl,
                                AudioEmbedUrl = @AudioEmbedUrl,
                                InfografiaEmbedUrl = @InfografiaEmbedUrl,
                                PresentacionEmbedUrl = @PresentacionEmbedUrl,
                                Activo = @Activo,
                                ActualizadoEn = SYSUTCDATETIME(),
                                ActualizadoPor = @ActualizadoPor
                            WHERE AsuntoId = @Id";
                        
                        DateTime.TryParse(model.Fecha, out var parsedDate);

                        await connection.ExecuteAsync(updateAsunto, new {
                            Id = id,
                            model.Titulo,
                            Fecha = parsedDate,
                            model.Estatus,
                            model.FolioSolicitud,
                            model.NumeroExpediente,
                            model.Descripcion,
                            SharePointUrl = SanitizarUrlEmbebida(model.SharePointUrl),
                            AudioEmbedUrl = SanitizarUrlEmbebida(model.AudioEmbedUrl),
                            InfografiaEmbedUrl = SanitizarUrlEmbebida(model.InfografiaEmbedUrl),
                            PresentacionEmbedUrl = SanitizarUrlEmbebida(model.PresentacionEmbedUrl),
                            model.Activo,
                            ActualizadoPor = GetCurrentUserId()?.ToString()
                        }, transaction);

                        // Delete old hitos and insert new ones
                        var deleteHitos = "DELETE FROM dgmesnie.TransparenciaHito WHERE AsuntoId = @AsuntoId";
                        await connection.ExecuteAsync(deleteHitos, new { AsuntoId = id }, transaction);

                        if (!string.IsNullOrEmpty(timelineJson))
                        {
                            var hitos = JsonConvert.DeserializeObject<List<TimelineEvent>>(timelineJson);
                            if (hitos != null && hitos.Any())
                            {
                                var insertHito = @"
                                    INSERT INTO dgmesnie.TransparenciaHito
                                    (AsuntoId, Fecha, Titulo, Descripcion, Orden, Activo, CreadoEn, CreadoPor)
                                    VALUES
                                    (@AsuntoId, @Fecha, @Titulo, @Descripcion, @Orden, 1, SYSUTCDATETIME(), @CreadoPor)";
                                
                                int orden = 0;
                                foreach (var hito in hitos)
                                {
                                    DateTime.TryParse(hito.Fecha, out var parsedHitoDate);
                                    await connection.ExecuteAsync(insertHito, new {
                                        AsuntoId = id,
                                        Fecha = parsedHitoDate,
                                        hito.Titulo,
                                        hito.Descripcion,
                                        Orden = orden++,
                                        CreadoPor = GetCurrentUserId()?.ToString()
                                    }, transaction);
                                }
                            }
                        }

                        transaction.Commit();
                        return RedirectToAction("Administrar");
                    }
                    catch (Exception ex)
                    {
                        transaction.Rollback();
                        ModelState.AddModelError("", "Error al actualizar el asunto: " + ex.Message);
                        return View(model);
                    }
                }
            }
        }

        // --- DELETE MATTER ---
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Eliminar(int id)
        {
            if (!IsUserAdmin()) return Json(new { success = false, message = "No autorizado" });

            try
            {
                using (var connection = new SqlConnection(_connectionString))
                {
                    // Soft delete: set Activo = 0
                    var sql = "UPDATE dgmesnie.TransparenciaAsunto SET Activo = 0, ActualizadoEn = SYSUTCDATETIME() WHERE AsuntoId = @Id";
                    await connection.ExecuteAsync(sql, new { Id = id });
                }
                return RedirectToAction("Administrar");
            }
            catch (Exception ex)
            {
                return BadRequest("Error al eliminar: " + ex.Message);
            }
        }

        private string SanitizarUrlEmbebida(string url)
        {
            if (string.IsNullOrWhiteSpace(url)) return url;
            
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

            // 2. Si el usuario copió atributos adicionales del iframe (ej. src="url" allow...)
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

        // --- AUTHORIZATION HELPERS ---
        private bool IsUserAdmin()
        {
            var perfilJson = HttpContext.Session.GetString("PerfilUsuario");
            if (string.IsNullOrEmpty(perfilJson)) return false;
            try
            {
                var perfil = JsonConvert.DeserializeObject<PerfilUsuario>(perfilJson);
                if (perfil == null) return false;
                
                // Allow user ID 1 or 86, or role "Administrador"
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

    public class TransparenciaAsunto
    {
        public int Id { get; set; }
        public string Titulo { get; set; }
        public string Fecha { get; set; }
        public string Estatus { get; set; }
        public string FolioSolicitud { get; set; }
        public string NumeroExpediente { get; set; }
        public string Descripcion { get; set; }
        public string SharePointUrl { get; set; }
        public string AudioEmbedUrl { get; set; }
        public string InfografiaEmbedUrl { get; set; }
        public string PresentacionEmbedUrl { get; set; }
        public bool Activo { get; set; }
        public List<TimelineEvent> Timeline { get; set; } = new List<TimelineEvent>();
    }

    public class TimelineEvent
    {
        public int HitoId { get; set; }
        public int AsuntoId { get; set; }
        public string Fecha { get; set; }
        public string Titulo { get; set; }
        public string Descripcion { get; set; }
        public int Orden { get; set; }
    }
}
