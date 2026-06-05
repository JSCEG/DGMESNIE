using Microsoft.AspNetCore.Mvc;
using NSIE.Models;
using NSIE.Models.Gestor;
using NSIE.Servicios;
using NSIE.Servicios.Interfaces;
using Newtonsoft.Json;
using System.Security.Claims;
using System.Collections.Generic;
using System.Linq;

namespace NSIE.Controllers
{
    public class NotificarRequest
    {
        public List<int> UsuarioIds { get; set; } = [];
    }

    public class RecordatorioRequest
    {
        public List<int> UsuarioIds { get; set; } = [];
    }

    public class CompartirActividadRequest
    {
        public List<int> UsuarioIds  { get; set; } = [];
        public string?   CorreoLibre { get; set; }   // correo extra no registrado en el sistema
    }

    public class ReporteFilaActividad
    {
        public string Actividad { get; set; } = "";
        public string Responsable { get; set; } = "";
        public int Total { get; set; }
        public int Concluidos { get; set; }
        public int PorVencer { get; set; }
        public int Vencidos { get; set; }
        public int Avance { get; set; }
    }

    public class ReporteFilaResponsable
    {
        public string Responsable { get; set; } = "";
        public int Total { get; set; }
        public int Concluidos { get; set; }
        public int PorVencer { get; set; }
        public int Vencidos { get; set; }
        public int Avance { get; set; }
    }

    public class GestorReporteSemanalRequest
    {
        public string EstatusSvg { get; set; } = "";
        public string AvanceSvg { get; set; } = "";
        public string PrioridadSvg { get; set; } = "";
        public string ResponsablesSvg { get; set; } = "";
        public string TemasSvg { get; set; } = "";
        public string TreemapSvg { get; set; } = "";
        public Dictionary<string, string> Kpis { get; set; } = new();
        public List<ReporteFilaActividad> Actividades { get; set; } = new();
        public List<ReporteFilaResponsable> Responsables { get; set; } = new();
    }

    [ServiceFilter(typeof(ValidacionInputFiltro))]
    [AutorizacionFiltro]
    public class GestorController : Controller
    {
        private readonly IRepositorioGestor _repo;
        private readonly IServicioEmailSMTP _servicioEmailSMTP;
        private readonly ILogger<GestorController> _logger;

        public GestorController(IRepositorioGestor repo, IServicioEmailSMTP servicioEmailSMTP, ILogger<GestorController> logger)
        {
            _repo = repo;
            _servicioEmailSMTP = servicioEmailSMTP;
            _logger = logger;
        }

        // ── Shell view ───────────────────────────────────────────────────────
        [HttpGet]
        public IActionResult Index()
        {
            ViewData["HeaderViewModel"] = BuildHeader();
            return View();
        }

        // ── API: Usuarios ─────────────────────────────────────────────────────
        [HttpGet("Gestor/Api/Usuarios")]
        public async Task<IActionResult> ApiUsuarios()
        {
            try
            {
                var users = await _repo.ObtenerUsuariosVigentesAsync();
                return Json(users);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error obteniendo usuarios.");
                return StatusCode(500, new { error = "Error interno al obtener usuarios." });
            }
        }

        // ── API: Actividades (Padre) ──────────────────────────────────────────
        [HttpGet("Gestor/Api/Actividades")]
        public async Task<IActionResult> ApiActividades()
        {
            try
            {
                var acts = await _repo.ObtenerActividadesAsync(GetCurrentUserId());
                return Json(acts);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error obteniendo actividades.");
                return StatusCode(500, new { error = "Error interno al obtener actividades." });
            }
        }

        // ── API: Catálogo completo de Actividades (para selección al crear/editar Tema) ──
        /// <summary>
        /// Devuelve todas las actividades activas sin filtro de usuario.
        /// Solo se usa en el dropdown de "Actividad" al crear o editar un Tema,
        /// para que cualquier usuario pueda asociar su tema a cualquier actividad del catálogo.
        /// </summary>
        [HttpGet("Gestor/Api/Actividades/Catalogo")]
        public async Task<IActionResult> ApiActividadesCatalogo()
        {
            try
            {
                // Pasar null para omitir el filtro de usuario → devuelve todo el catálogo
                var acts = await _repo.ObtenerActividadesAsync(usuarioId: null);
                return Json(acts);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error obteniendo catálogo de actividades.");
                return StatusCode(500, new { error = "Error interno al obtener el catálogo de actividades." });
            }
        }

        [HttpGet("Gestor/Api/Actividades/{id:int}")]
        public async Task<IActionResult> ApiActividad(int id)
        {
            var act = await _repo.ObtenerActividadPorIdAsync(id);
            if (act == null) return NotFound();

            var userId = GetCurrentUserId();
            if (userId.HasValue && userId.Value != 1 && userId.Value != 86)
            {
                var isAuthorized = act.ResponsablePrincipalId == userId.Value ||
                                   (act.Corresponsables != null && act.Corresponsables.Any(c => c.IdUsuario == userId.Value));
                if (!isAuthorized)
                {
                    var userTemas = await _repo.ObtenerTemasAsync(id, userId.Value);
                    if (userTemas == null || !userTemas.Any())
                    {
                        return NotFound();
                    }
                }
            }
            return Json(act);
        }

        [HttpPost("Gestor/Api/Actividades")]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> ApiCrearActividad([FromBody] GestorActividadForm form)
        {
            var currentUserId = GetCurrentUserId();
            if (currentUserId != 1)
            {
                return StatusCode(403, new { error = "Solo el administrador de desarrollo puede crear, editar o eliminar actividades." });
            }
            if (!ModelState.IsValid)
                return BadRequest(ModelState);
            try
            {
                var id = await _repo.CrearActividadAsync(form, GetCurrentUserId());
                var act = await _repo.ObtenerActividadPorIdAsync(id);
                if (act != null)
                {
                    if (form.ResponsablePrincipalId.HasValue)
                        await NotificarAsignacionActividadAsync(act, form.ResponsablePrincipalId);
                    if (form.CorresponsablesIds != null && form.CorresponsablesIds.Any())
                        await NotificarCorresponsablesActividadAsync(act, form.CorresponsablesIds);
                }
                return CreatedAtAction(nameof(ApiActividad), new { id }, act);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error creando actividad.");
                return StatusCode(500, new { error = "No fue posible crear la actividad." });
            }
        }

        [HttpPut("Gestor/Api/Actividades/{id:int}")]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> ApiActualizarActividad(int id, [FromBody] GestorActividadForm form)
        {
            var currentUserId = GetCurrentUserId();
            if (currentUserId != 1)
            {
                return StatusCode(403, new { error = "Solo el administrador de desarrollo puede crear, editar o eliminar actividades." });
            }
            form.ActividadId = id;
            if (!ModelState.IsValid)
                return BadRequest(ModelState);
            try
            {
                var actividadAnterior = await _repo.ObtenerActividadPorIdAsync(id);
                await _repo.ActualizarActividadAsync(form, GetCurrentUserId());
                var act = await _repo.ObtenerActividadPorIdAsync(id);
                var responsableCambioSolicitado = form.ResponsablePrincipalId != actividadAnterior?.ResponsablePrincipalId;
                if (act != null)
                {
                    if (responsableCambioSolicitado && form.ResponsablePrincipalId.HasValue)
                        await NotificarAsignacionActividadAsync(act, form.ResponsablePrincipalId);

                    var nuevosCorresponsables = form.CorresponsablesIds.Except(actividadAnterior?.Corresponsables.Select(c => c.IdUsuario) ?? Enumerable.Empty<int>()).ToList();
                    if (nuevosCorresponsables.Any())
                        await NotificarCorresponsablesActividadAsync(act, nuevosCorresponsables);
                }
                return Json(act);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error actualizando actividad {Id}.", id);
                return StatusCode(500, new { error = "No fue posible actualizar la actividad." });
            }
        }

        [HttpDelete("Gestor/Api/Actividades/{id:int}")]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> ApiEliminarActividad(int id)
        {
            var currentUserId = GetCurrentUserId();
            if (currentUserId != 1)
            {
                return StatusCode(403, new { error = "Solo el administrador de desarrollo puede crear, editar o eliminar actividades." });
            }
            try
            {
                await _repo.EliminarActividadAsync(id);
                return Ok(new { success = true });
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error eliminando actividad {Id}.", id);
                return StatusCode(500, new { error = "No fue posible eliminar la actividad." });
            }
        }

        [HttpPost("Gestor/Api/Actividades/{id:int}/Compartir")]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> ApiCompartirActividad(int id, [FromBody] CompartirActividadRequest? request = null)
        {
            try
            {
                var actividad = await _repo.ObtenerActividadPorIdAsync(id);
                if (actividad == null)
                    return NotFound(new { error = "No se encontró la actividad especificada." });

                var portalUrl = Url.Action("Index", "Gestor", null, protocol: HttpContext.Request.Scheme) ?? string.Empty;
                int enviados = 0;

                // ── Enviar a usuarios registrados ──────────────────────────────
                var ids = request?.UsuarioIds?.Distinct().ToList() ?? [];
                foreach (var uid in ids)
                {
                    var dest = await _repo.ObtenerUsuarioVigentePorIdAsync(uid);
                    if (dest == null) continue;
                    var correo = dest.Correo?.Trim();
                    if (string.IsNullOrWhiteSpace(correo)) continue;

                    var asunto = $"📋 Actividad compartida: {actividad.Actividad}";
                    var cuerpo = ConstruirCorreoCompartirActividad(dest.Nombre, actividad, portalUrl);
                    await _servicioEmailSMTP.EnviarCorreo(correo, asunto, cuerpo);
                    enviados++;
                }

                // ── Enviar a correo libre ──────────────────────────────────────
                var correoLibre = request?.CorreoLibre?.Trim();
                if (!string.IsNullOrWhiteSpace(correoLibre))
                {
                    var asunto = $"📋 Actividad compartida: {actividad.Actividad}";
                    var cuerpo = ConstruirCorreoCompartirActividad(correoLibre, actividad, portalUrl);
                    await _servicioEmailSMTP.EnviarCorreo(correoLibre, asunto, cuerpo);
                    enviados++;
                }

                if (enviados == 0)
                    return BadRequest(new { error = "No se especificaron destinatarios válidos." });

                return Ok(new { success = true, mensaje = $"Correo enviado a {enviados} destinatario(s)." });
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error compartiendo actividad {Id}.", id);
                return StatusCode(500, new { error = "No fue posible enviar el correo." });
            }
        }

        // ── API: Temas (Hijo) ─────────────────────────────────────────────────
        [HttpGet("Gestor/Api/Temas")]
        public async Task<IActionResult> ApiTemas([FromQuery] int? actividadId = null)
        {
            try
            {
                var temas = await _repo.ObtenerTemasAsync(actividadId, GetCurrentUserId());
                return Json(temas);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error obteniendo temas.");
                return StatusCode(500, new { error = "Error interno al obtener temas." });
            }
        }

        [HttpGet("Gestor/Api/Temas/{id:int}")]
        public async Task<IActionResult> ApiTema(int id)
        {
            var tema = await _repo.ObtenerTemaPorIdAsync(id);
            if (tema == null) return NotFound();

            var userId = GetCurrentUserId();
            if (userId.HasValue && userId.Value != 1 && userId.Value != 86)
            {
                var isAuthorized = tema.ResponsableId == userId.Value ||
                                   (tema.Corresponsables != null && tema.Corresponsables.Any(c => c.IdUsuario == userId.Value)) ||
                                   (tema.Etapas != null && tema.Etapas.Any(e => e.ResponsableId == userId.Value ||
                                                                                (e.Corresponsables != null && e.Corresponsables.Any(c => c.IdUsuario == userId.Value))));
                if (!isAuthorized)
                {
                    var act = await _repo.ObtenerActividadPorIdAsync(tema.ActividadId);
                    if (act != null)
                    {
                        var isActAuthorized = act.ResponsablePrincipalId == userId.Value ||
                                              (act.Corresponsables != null && act.Corresponsables.Any(c => c.IdUsuario == userId.Value));
                        if (isActAuthorized)
                        {
                            isAuthorized = true;
                        }
                    }
                }

                if (!isAuthorized)
                {
                    return NotFound();
                }
            }
            return Json(tema);
        }

        [HttpPost("Gestor/Api/Temas")]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> ApiCrearTema([FromBody] GestorTemaForm form)
        {
            if (!ModelState.IsValid)
                return BadRequest(ModelState);
            try
            {
                var id = await _repo.CrearTemaAsync(form, GetCurrentUserId());
                var tema = await _repo.ObtenerTemaPorIdAsync(id);
                if (tema != null)
                {
                    var responsablesAsignados = ObtenerResponsablesEtapas(form);
                    foreach (var responsableId in responsablesAsignados)
                    {
                        await NotificarAsignacionTemaAsync(tema, responsableId);
                    }

                    var etapaCorresponsables = ObtenerCorresponsablesEtapas(form);
                    if (etapaCorresponsables.Any())
                    {
                        await NotificarCorresponsablesTemaAsync(tema, etapaCorresponsables);
                    }
                    if (form.NotificarUsuariosIds != null && form.NotificarUsuariosIds.Any())
                    {
                        await NotificarCompartirTemaAsync(tema, form.NotificarUsuariosIds);
                    }
                }
                return CreatedAtAction(nameof(ApiTema), new { id }, tema);
            }
            catch (ArgumentException ex)
            {
                return BadRequest(new { error = ex.Message });
            }
            catch (InvalidOperationException ex)
            {
                return BadRequest(new { error = ex.Message });
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error creando tema.");
                return StatusCode(500, new { error = "No fue posible crear el tema." });
            }
        }

        [HttpPut("Gestor/Api/Temas/{id:int}")]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> ApiActualizarTema(int id, [FromBody] GestorTemaForm form)
        {
            if (form == null)
                return BadRequest(new { error = "Payload de tema vacío." });

            var currentUserId = GetCurrentUserId();
            if (!currentUserId.HasValue)
            {
                return Unauthorized(new { error = "Sesión no iniciada." });
            }

            var temaAnterior = await _repo.ObtenerTemaPorIdAsync(id);
            if (temaAnterior == null)
            {
                return NotFound(new { error = "No se encontró el tema especificado." });
            }

            var isAuthorized = currentUserId.Value == 1 ||
                               temaAnterior.ResponsableId == currentUserId.Value ||
                               (temaAnterior.Corresponsables != null && temaAnterior.Corresponsables.Any(c => c.IdUsuario == currentUserId.Value));

            if (!isAuthorized)
            {
                return StatusCode(403, new { error = "No tiene permisos para modificar este tema." });
            }

            form.TemaId = id;
            if (!ModelState.IsValid)
                return BadRequest(ModelState);
            try
            {
                await _repo.ActualizarTemaAsync(form, GetCurrentUserId());
                var tema = await _repo.ObtenerTemaPorIdAsync(id);
                if (tema != null)
                {
                    var responsablesNuevosOCambiados = ObtenerResponsablesEtapasNuevasOCambiadas(form, temaAnterior);
                    foreach (var responsableId in responsablesNuevosOCambiados)
                    {
                        await NotificarAsignacionTemaAsync(tema, responsableId);
                    }

                    var anterioresCorresponsables = temaAnterior?.Etapas
                        .SelectMany(e => e.Corresponsables.Select(c => c.IdUsuario))
                        .Distinct() ?? Enumerable.Empty<int>();
                    var nuevosCorresponsables = ObtenerCorresponsablesEtapas(form)
                        .Except(anterioresCorresponsables)
                        .ToList();
                    if (nuevosCorresponsables.Any())
                    {
                        await NotificarCorresponsablesTemaAsync(tema, nuevosCorresponsables);
                    }

                    if (form.NotificarUsuariosIds != null && form.NotificarUsuariosIds.Any())
                    {
                        await NotificarCompartirTemaAsync(tema, form.NotificarUsuariosIds);
                    }
                }
                return Json(tema);
            }
            catch (ArgumentException ex)
            {
                return BadRequest(new { error = ex.Message });
            }
            catch (InvalidOperationException ex)
            {
                return BadRequest(new { error = ex.Message });
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error actualizando tema {Id}.", id);
                return StatusCode(500, new { error = "No fue posible actualizar el tema." });
            }
        }

        private static List<int> ObtenerCorresponsablesEtapas(GestorTemaForm form)
        {
            return form.Etapas?
                .SelectMany(e => e.CorresponsablesIds ?? [])
                .Distinct()
                .ToList() ?? [];
        }

        private static List<int> ObtenerResponsablesEtapas(GestorTemaForm form)
        {
            var responsables = form.Etapas?
                .Where(e => e.ResponsableId > 0)
                .Select(e => e.ResponsableId)
                .Distinct()
                .ToList() ?? [];

            if (!responsables.Any() && form.ResponsableId.HasValue)
            {
                responsables.Add(form.ResponsableId.Value);
            }

            return responsables;
        }

        private static List<int> ObtenerResponsablesEtapasNuevasOCambiadas(GestorTemaForm form, GestorTema? temaAnterior)
        {
            if (form.Etapas == null || !form.Etapas.Any())
            {
                return form.ResponsableId.HasValue && form.ResponsableId != temaAnterior?.ResponsableId
                    ? [form.ResponsableId.Value]
                    : [];
            }

            var anteriores = temaAnterior?.Etapas.ToDictionary(e => e.EtapaId, e => e.ResponsableId)
                ?? new Dictionary<int, int>();

            return form.Etapas
                .Where(e => e.ResponsableId > 0)
                .Where(e => !e.EtapaId.HasValue || e.EtapaId.Value == 0 ||
                            !anteriores.TryGetValue(e.EtapaId.Value, out var previo) ||
                            previo != e.ResponsableId)
                .Select(e => e.ResponsableId)
                .Distinct()
                .ToList();
        }

        [HttpDelete("Gestor/Api/Temas/{id:int}")]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> ApiEliminarTema(int id)
        {
            var currentUserId = GetCurrentUserId();
            if (!currentUserId.HasValue)
            {
                return Unauthorized(new { error = "Sesión no iniciada." });
            }

            var tema = await _repo.ObtenerTemaPorIdAsync(id);
            if (tema == null)
            {
                return NotFound(new { error = "No se encontró el tema especificado." });
            }

            var isAuthorized = currentUserId.Value == 1 ||
                               tema.ResponsableId == currentUserId.Value ||
                               (tema.Corresponsables != null && tema.Corresponsables.Any(c => c.IdUsuario == currentUserId.Value));

            if (!isAuthorized)
            {
                return StatusCode(403, new { error = "No tiene permisos para eliminar este tema." });
            }

            try
            {
                await _repo.EliminarTemaAsync(id);
                return Ok(new { success = true });
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error eliminando tema {Id}.", id);
                return StatusCode(500, new { error = "No fue posible eliminar el tema." });
            }
        }

        [HttpPost("Gestor/Api/Temas/{id:int}/Recordatorio")]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> ApiEnviarRecordatorio(int id, [FromBody] RecordatorioRequest? request = null)
        {
            try
            {
                var tema = await _repo.ObtenerTemaPorIdAsync(id);
                if (tema == null)
                    return NotFound(new { error = "No se encontró el tema especificado." });

                List<int> idsToSend = [];
                if (request?.UsuarioIds != null && request.UsuarioIds.Any())
                {
                    idsToSend = request.UsuarioIds.Distinct().ToList();
                }
                else
                {
                    if (tema.ResponsableId.HasValue)
                        idsToSend.Add(tema.ResponsableId.Value);
                    if (tema.Corresponsables != null)
                    {
                        foreach (var corr in tema.Corresponsables)
                        {
                            idsToSend.Add(corr.IdUsuario);
                        }
                    }
                    idsToSend = idsToSend.Distinct().ToList();

                    if (!idsToSend.Any())
                        return BadRequest(new { error = "El tema no tiene un responsable ni corresponsables asignados." });
                }

                foreach (var uid in idsToSend)
                {
                    var destinatario = await _repo.ObtenerUsuarioVigentePorIdAsync(uid);
                    if (destinatario == null)
                    {
                        _logger.LogWarning("No se encontró usuario vigente para recordatorio con ID {UsuarioId}.", uid);
                        continue;
                    }

                    var correoDestinatario = destinatario.Correo?.Trim();
                    if (string.IsNullOrWhiteSpace(correoDestinatario))
                    {
                        _logger.LogWarning("El destinatario {UsuarioId} no tiene correo registrado para recordatorio.", uid);
                        continue;
                    }

                    var asunto = $"🔔 RECORDATORIO: Tema pendiente o por vencer - {tema.Clave}";
                    var portalUrl = Url.Action("Index", "Gestor", null, protocol: HttpContext.Request.Scheme) ?? string.Empty;
                    var cuerpo = ConstruirCorreoRecordatorioTema(destinatario.Nombre, tema, portalUrl);

                    _logger.LogInformation("Enviando correo de recordatorio para el tema {TemaId} a {Correo}.", id, correoDestinatario);
                    await _servicioEmailSMTP.EnviarCorreo(correoDestinatario, asunto, cuerpo);
                }

                return Ok(new { success = true, mensaje = "Recordatorio enviado con éxito." });
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error enviando recordatorio para tema {Id}.", id);
                return StatusCode(500, new { error = "No fue posible enviar el recordatorio." });
            }
        }

        [HttpPost("Gestor/Api/Temas/{id:int}/Notificar")]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> ApiNotificarTema(int id, [FromBody] NotificarRequest request)
        {
            try
            {
                var tema = await _repo.ObtenerTemaPorIdAsync(id);
                if (tema == null)
                    return NotFound(new { error = "No se encontró el tema especificado." });

                if (request?.UsuarioIds == null || !request.UsuarioIds.Any())
                    return BadRequest(new { error = "Debe proporcionar al menos un usuario para notificar." });

                await NotificarCompartirTemaAsync(tema, request.UsuarioIds);

                return Ok(new { success = true, mensaje = "Tema notificado con éxito." });
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error notificando tema {Id} a usuarios.", id);
                return StatusCode(500, new { error = "No fue posible enviar la notificación." });
            }
        }

        [HttpPost("Gestor/Api/EnviarReporteSemanal")]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> ApiEnviarReporteSemanal([FromBody] GestorReporteSemanalRequest request)
        {
            try
            {
                var perfilJson = HttpContext.Session.GetString("PerfilUsuario");
                if (string.IsNullOrEmpty(perfilJson))
                    return Unauthorized("Sesión no iniciada.");

                var perfil = JsonConvert.DeserializeObject<PerfilUsuario>(perfilJson);
                // De momento, forzar a que el correo le llegue únicamente a Javier Sasso
                var correo = "jsasso@energia.gob.mx";

                // Solo permitir a los usuarios autorizados (por ejemplo ID 1 y 86)
                if (perfil.IdUsuario != "1" && perfil.IdUsuario != "86")
                {
                    return Forbid("No tiene permisos para enviar este reporte.");
                }

                var asunto = $"📊 Reporte Semanal Gestor DGMESNIE - {DateTime.Now:dd/MM/yyyy}";
                var cuerpo = ConstruirCorreoReporteSemanal(perfil.Nombre, request);

                await _servicioEmailSMTP.EnviarCorreo(correo, asunto, cuerpo);

                return Ok(new { success = true, mensaje = $"Reporte semanal enviado con éxito a {correo}." });
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error enviando reporte semanal por correo.");
                return StatusCode(500, new { error = "No fue posible enviar el reporte: " + ex.Message });
            }
        }

        private static string ConstruirCorreoReporteSemanal(string nombreUsuario, GestorReporteSemanalRequest request)
        {
            var today = DateTime.Today;
            int daysToMonday = ((int)today.DayOfWeek - (int)DayOfWeek.Monday + 7) % 7;
            var lunes = today.AddDays(-daysToMonday);
            var domingo = lunes.AddDays(6);
            var culture = new System.Globalization.CultureInfo("es-MX");
            var rangoSemana = $"{lunes.ToString("dd 'de' MMMM", culture)} al {domingo.ToString("dd 'de' MMMM 'de' yyyy", culture)}";

            var kpis = request.Kpis;
            var total = kpis.GetValueOrDefault("temasRegistrados", "0");
            var concluidas = kpis.GetValueOrDefault("temasConcluidos", "0");
            var porVencer = kpis.GetValueOrDefault("temasPorVencer", "0");
            var vencidas = kpis.GetValueOrDefault("temasVencidos", "0");
            var avance = kpis.GetValueOrDefault("avanceGlobal", "0%");
            var activas = kpis.GetValueOrDefault("actividadesActivas", "0");

            string SanitizarSvg(string base64OrSvg) {
                if (string.IsNullOrWhiteSpace(base64OrSvg)) return "<div style='color:#667085;text-align:center;padding:10px;border:1px dashed #ddd;'>Gráfico no disponible</div>";
                if (base64OrSvg.StartsWith("data:image/")) {
                    return $"<img src='{base64OrSvg}' style='max-width:100%; height:auto; border:none; display:inline-block;' alt='Gráfico' />";
                }
                return base64OrSvg.Replace("<svg", "<svg style='max-width:100%; height:auto;'");
            }

            return $@"
                <html lang='es'>
                <head>
                    <meta charset='UTF-8'>
                    <title>Reporte Semanal Gestor</title>
                </head>
                <body style='margin:0; padding:20px; background-color:#f9fafb; font-family:Arial, Helvetica, sans-serif; color:#1f2937;'>
                    <div style='max-width:800px; margin:0 auto; background-color:#ffffff; border:1px solid #e5e7eb; border-radius:12px; overflow:hidden; box-shadow:0 4px 6px rgba(0,0,0,0.05);'>
                        
                        <div style='padding:16px 24px; border-bottom:1px solid #f3f4f6;'>
                            <table role='presentation' cellpadding='0' cellspacing='0' border='0' style='width:100%;'>
                                <tr>
                                    <td style='width:50%;'>
                                        <img src='https://cdn.sassoapps.com/dgmesnie/logo_gob.png' alt='Gobierno de México' style='max-height:36px; width:auto;'>
                                    </td>
                                    <td style='width:50%; text-align:right;'>
                                        <img src='https://cdn.sassoapps.com/dgmesnie/logo_sener.png' alt='Secretaría de Energía' style='max-height:38px; width:auto;'>
                                    </td>
                                </tr>
                            </table>
                        </div>

                        <div style='background-color:#8a0031; background:linear-gradient(135deg, #8a0031 0%, #6b0024 100%); color:#ffffff; padding:24px; text-align:center;'>
                            <h1 style='margin:0; font-size:22px; font-weight:700; letter-spacing:0.02em;'>Reporte de Avance Semanal</h1>
                            <p style='margin:6px 0 0; font-size:14px; font-weight:bold; color:rgba(255,255,255,0.95);'>Semana del {rangoSemana}</p>
                            <p style='margin:4px 0 0; font-size:12px; color:rgba(255,255,255,0.8);'>Gestor de Actividades y Temas Críticos — DGMESNIE</p>
                        </div>

                        <div style='padding:24px;'>
                            <p style='margin:0 0 16px; font-size:16px; font-weight:700;'>Estimado Equipo,</p>

                            <div style='margin-bottom:28px;'>
                                <table role='presentation' cellpadding='0' cellspacing='10' border='0' style='width:100%; margin:-10px;'>
                                    <tr>
                                        <td style='width:33%; background:#f8fafc; border:1px solid #e2e8f0; border-radius:8px; padding:12px; text-align:center;'>
                                            <span style='display:block; font-size:11px; text-transform:uppercase; color:#64748b; font-weight:700;'>Actividades Activas</span>
                                            <strong style='display:block; font-size:24px; color:#0f172a; margin-top:4px;'>{activas}</strong>
                                        </td>
                                        <td style='width:33%; background:#f8fafc; border:1px solid #e2e8f0; border-radius:8px; padding:12px; text-align:center;'>
                                            <span style='display:block; font-size:11px; text-transform:uppercase; color:#64748b; font-weight:700;'>Temas Registrados</span>
                                            <strong style='display:block; font-size:24px; color:#0f172a; margin-top:4px;'>{total}</strong>
                                        </td>
                                        <td style='width:33%; background:#ecfdf5; border:1px solid #d1fae5; border-radius:8px; padding:12px; text-align:center;'>
                                            <span style='display:block; font-size:11px; text-transform:uppercase; color:#065f46; font-weight:700;'>Temas Concluidos</span>
                                            <strong style='display:block; font-size:24px; color:#047857; margin-top:4px;'>{concluidas}</strong>
                                        </td>
                                    </tr>
                                    <tr>
                                        <td style='background:#fffbeb; border:1px solid #fef3c7; border-radius:8px; padding:12px; text-align:center;'>
                                            <span style='display:block; font-size:11px; text-transform:uppercase; color:#92400e; font-weight:700;'>Por Vencer</span>
                                            <strong style='display:block; font-size:24px; color:#d97706; margin-top:4px;'>{porVencer}</strong>
                                        </td>
                                        <td style='background:#fef2f2; border:1px solid #fee2e2; border-radius:8px; padding:12px; text-align:center;'>
                                            <span style='display:block; font-size:11px; text-transform:uppercase; color:#991b1b; font-weight:700;'>Vencidos</span>
                                            <strong style='display:block; font-size:24px; color:#b91c1c; margin-top:4px;'>{vencidas}</strong>
                                        </td>
                                        <td style='background:#f5f3ff; border:1px solid #ede9fe; border-radius:8px; padding:12px; text-align:center;'>
                                            <span style='display:block; font-size:11px; text-transform:uppercase; color:#5b21b6; font-weight:700;'>Avance Promedio</span>
                                            <strong style='display:block; font-size:24px; color:#6d28d9; margin-top:4px;'>{avance}</strong>
                                        </td>
                                    </tr>
                                </table>
                            </div>

                            <h2 style='font-size:16px; font-weight:700; border-bottom:2px solid #8a0031; padding-bottom:6px; margin:24px 0 12px 0; color:#8a0031;'>
                                Avance por Actividad
                            </h2>
                            <div style='margin-bottom:24px; overflow-x:auto;'>
                                <table cellpadding='6' cellspacing='0' style='width:100%; border-collapse:collapse; font-size:12px; text-align:left; border:1px solid #e5e7eb;'>
                                    <thead>
                                        <tr style='background-color:#8a0031; color:#ffffff;'>
                                            <th style='padding:8px; border:1px solid #e5e7eb;'>Actividad</th>
                                            <th style='padding:8px; border:1px solid #e5e7eb;'>Responsable</th>
                                            <th style='padding:8px; border:1px solid #e5e7eb; text-align:center;'>Total</th>
                                            <th style='padding:8px; border:1px solid #e5e7eb; text-align:center;'>Concluidos</th>
                                            <th style='padding:8px; border:1px solid #e5e7eb; text-align:center;'>Por Vencer</th>
                                            <th style='padding:8px; border:1px solid #e5e7eb; text-align:center;'>Vencidos</th>
                                            <th style='padding:8px; border:1px solid #e5e7eb; text-align:center; width:80px;'>Avance</th>
                                        </tr>
                                    </thead>
                                    <tbody>
                                        {string.Join("", request.Actividades.Select(a => $@"
                                            <tr style='border-bottom:1px solid #e5e7eb;'>
                                                <td style='padding:8px; border:1px solid #e5e7eb; font-weight:bold; color:#1f2937;'>{System.Net.WebUtility.HtmlEncode(a.Actividad)}</td>
                                                <td style='padding:8px; border:1px solid #e5e7eb; color:#4b5563;'>{System.Net.WebUtility.HtmlEncode(a.Responsable)}</td>
                                                <td style='padding:8px; border:1px solid #e5e7eb; text-align:center; font-weight:bold;'>{a.Total}</td>
                                                <td style='padding:8px; border:1px solid #e5e7eb; text-align:center; color:#047857; font-weight:bold;'>{a.Concluidos}</td>
                                                <td style='padding:8px; border:1px solid #e5e7eb; text-align:center; color:#d97706; font-weight:bold;'>{a.PorVencer}</td>
                                                <td style='padding:8px; border:1px solid #e5e7eb; text-align:center; color:#b91c1c; font-weight:bold;'>{a.Vencidos}</td>
                                                <td style='padding:8px; border:1px solid #e5e7eb; text-align:center; font-weight:bold; color:#8a0031;'>{a.Avance}%</td>
                                            </tr>
                                        "))}
                                        {(!request.Actividades.Any() ? "<tr><td colspan='7' style='text-align:center; padding:12px; color:#6b7280;'>No hay actividades registradas en este periodo.</td></tr>" : "")}
                                    </tbody>
                                </table>
                            </div>

                            <h2 style='font-size:16px; font-weight:700; border-bottom:2px solid #8a0031; padding-bottom:6px; margin:24px 0 12px 0; color:#8a0031;'>
                                Avance por Responsable
                            </h2>
                            <div style='margin-bottom:24px; overflow-x:auto;'>
                                <table cellpadding='6' cellspacing='0' style='width:100%; border-collapse:collapse; font-size:12px; text-align:left; border:1px solid #e5e7eb;'>
                                    <thead>
                                        <tr style='background-color:#1e5b4f; color:#ffffff;'>
                                            <th style='padding:8px; border:1px solid #e5e7eb;'>Responsable</th>
                                            <th style='padding:8px; border:1px solid #e5e7eb; text-align:center;'>Total Temas</th>
                                            <th style='padding:8px; border:1px solid #e5e7eb; text-align:center;'>Concluidos</th>
                                            <th style='padding:8px; border:1px solid #e5e7eb; text-align:center;'>Por Vencer</th>
                                            <th style='padding:8px; border:1px solid #e5e7eb; text-align:center;'>Vencidos</th>
                                            <th style='padding:8px; border:1px solid #e5e7eb; text-align:center; width:80px;'>Avance</th>
                                        </tr>
                                    </thead>
                                    <tbody>
                                        {string.Join("", request.Responsables.Select(r => $@"
                                            <tr style='border-bottom:1px solid #e5e7eb;'>
                                                <td style='padding:8px; border:1px solid #e5e7eb; font-weight:bold; color:#1f2937;'>{System.Net.WebUtility.HtmlEncode(r.Responsable)}</td>
                                                <td style='padding:8px; border:1px solid #e5e7eb; text-align:center; font-weight:bold;'>{r.Total}</td>
                                                <td style='padding:8px; border:1px solid #e5e7eb; text-align:center; color:#047857; font-weight:bold;'>{r.Concluidos}</td>
                                                <td style='padding:8px; border:1px solid #e5e7eb; text-align:center; color:#d97706; font-weight:bold;'>{r.PorVencer}</td>
                                                <td style='padding:8px; border:1px solid #e5e7eb; text-align:center; color:#b91c1c; font-weight:bold;'>{r.Vencidos}</td>
                                                <td style='padding:8px; border:1px solid #e5e7eb; text-align:center; font-weight:bold; color:#1e5b4f;'>{r.Avance}%</td>
                                            </tr>
                                        "))}
                                        {(!request.Responsables.Any() ? "<tr><td colspan='6' style='text-align:center; padding:12px; color:#6b7280;'>No hay responsables registrados en este periodo.</td></tr>" : "")}
                                    </tbody>
                                </table>
                            </div>

                            <h2 style='font-size:16px; font-weight:700; border-bottom:2px solid #8a0031; padding-bottom:6px; margin:24px 0 16px 0; color:#8a0031;'>
                                Gráficos Analíticos
                            </h2>

                            <table role='presentation' cellpadding='0' cellspacing='12' border='0' style='width:100%;'>
                                <tr>
                                    <td style='width:50%; background:#ffffff; border:1px solid #e5e7eb; border-radius:10px; padding:16px; text-align:center;'>
                                        <h3 style='margin:0 0 10px 0; font-size:13px; font-weight:700; color:#4b5563;'>Estatus General</h3>
                                        <div style='display:inline-block; text-align:center;'>
                                            {SanitizarSvg(request.EstatusSvg)}
                                        </div>
                                    </td>
                                    <td style='width:50%; background:#ffffff; border:1px solid #e5e7eb; border-radius:10px; padding:16px; text-align:center;'>
                                        <h3 style='margin:0 0 10px 0; font-size:13px; font-weight:700; color:#4b5563;'>Distribución de Prioridad</h3>
                                        <div style='display:inline-block; text-align:center;'>
                                            {SanitizarSvg(request.PrioridadSvg)}
                                        </div>
                                    </td>
                                </tr>
                                <tr>
                                    <td style='width:50%; background:#ffffff; border:1px solid #e5e7eb; border-radius:10px; padding:16px; text-align:center;'>
                                        <h3 style='margin:0 0 10px 0; font-size:13px; font-weight:700; color:#4b5563;'>Avance Global</h3>
                                        <div style='display:inline-block; text-align:center;'>
                                            {SanitizarSvg(request.AvanceSvg)}
                                        </div>
                                    </td>
                                    <td style='width:50%; background:#ffffff; border:1px solid #e5e7eb; border-radius:10px; padding:16px; text-align:center;'>
                                        <h3 style='margin:0 0 10px 0; font-size:13px; font-weight:700; color:#4b5563;'>Carga por Responsable</h3>
                                        <div style='display:inline-block; text-align:center;'>
                                            {SanitizarSvg(request.ResponsablesSvg)}
                                        </div>
                                    </td>
                                </tr>
                            </table>

                        </div>

                        <div style='background-color:#f9fafb; border-top:1px solid #e5e7eb; padding:16px; text-align:center; font-size:11px; color:#6b7280;'>
                            Este correo ha sido generado y enviado de manera automatizada a solicitud del administrador del sistema.<br>
                            <strong>DGMESNIE — Secretaría de Energía — Gobierno de México</strong>
                        </div>
                    </div>
                </body>
                </html>";
        }

        // ── Helpers ───────────────────────────────────────────────────────────
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

        private async Task NotificarAsignacionActividadAsync(GestorActividad actividad, int? responsableIdOverride = null)
        {
            var responsableId = actividad.ResponsablePrincipalId ?? responsableIdOverride;
            if (!responsableId.HasValue)
            {
                _logger.LogInformation("Actividad {ActividadId} sin ResponsablePrincipalId. No se envía correo.", actividad.ActividadId);
                return;
            }

            var responsable = await _repo.ObtenerUsuarioVigentePorIdAsync(responsableId.Value);
            if (responsable == null)
            {
                _logger.LogWarning("No se encontró usuario vigente para ResponsablePrincipalId {ResponsableId} en actividad {ActividadId}.", responsableId.Value, actividad.ActividadId);
                return;
            }

            var correoResponsable = responsable.Correo?.Trim();
            if (string.IsNullOrWhiteSpace(correoResponsable))
            {
                _logger.LogWarning("El responsable {ResponsableId} de la actividad {ActividadId} no tiene correo registrado.", responsableId.Value, actividad.ActividadId);
                return;
            }

            var asunto = $"Nueva asignación de actividad: {actividad.Clave}";
            var portalUrl = Url.Action("Index", "Gestor", null, protocol: HttpContext.Request.Scheme) ?? string.Empty;
            var cuerpo = ConstruirCorreoAsignacionActividad(responsable.Nombre, actividad, portalUrl);

            try
            {
                _logger.LogInformation("Enviando correo de asignación de actividad {ActividadId} a responsable {ResponsableId} ({Correo}).", actividad.ActividadId, responsableId.Value, correoResponsable);
                await _servicioEmailSMTP.EnviarCorreo(correoResponsable, asunto, cuerpo);
            }
            catch (Exception ex)
            {
                _logger.LogWarning(ex, "La actividad {ActividadId} se guardó, pero no fue posible enviar correo al responsable {ResponsableId}.", actividad.ActividadId, responsableId.Value);
            }
        }

        private async Task NotificarAsignacionTemaAsync(GestorTema tema, int? responsableIdOverride = null)
        {
            var responsableId = responsableIdOverride ?? tema.ResponsableId;
            if (!responsableId.HasValue)
            {
                _logger.LogInformation("Tema {TemaId} sin ResponsableId. No se envía correo.", tema.TemaId);
                return;
            }

            var responsable = await _repo.ObtenerUsuarioVigentePorIdAsync(responsableId.Value);
            if (responsable == null)
            {
                _logger.LogWarning("No se encontró usuario vigente para ResponsableId {ResponsableId} en tema {TemaId}.", responsableId.Value, tema.TemaId);
                return;
            }

            var correoResponsable = responsable.Correo?.Trim();
            if (string.IsNullOrWhiteSpace(correoResponsable))
            {
                _logger.LogWarning("El responsable {ResponsableId} del tema {TemaId} no tiene correo registrado.", responsableId.Value, tema.TemaId);
                return;
            }

            var asunto = $"Nueva asignación de tema: {tema.Clave}";
            var portalUrl = Url.Action("Index", "Gestor", null, protocol: HttpContext.Request.Scheme) ?? string.Empty;
            var cuerpo = ConstruirCorreoAsignacionTema(responsable.Nombre, tema, portalUrl);

            try
            {
                _logger.LogInformation("Enviando correo de asignación de tema {TemaId} a responsable {ResponsableId} ({Correo}).", tema.TemaId, responsableId.Value, correoResponsable);
                await _servicioEmailSMTP.EnviarCorreo(correoResponsable, asunto, cuerpo);
            }
            catch (Exception ex)
            {
                _logger.LogWarning(ex, "El tema {TemaId} se guardó, pero no fue posible enviar correo al responsable {ResponsableId}.", tema.TemaId, responsableId.Value);
            }
        }

        private async Task NotificarCorresponsablesActividadAsync(GestorActividad actividad, List<int> corresponsablesIds)
        {
            if (corresponsablesIds == null || !corresponsablesIds.Any()) return;

            foreach (var uid in corresponsablesIds.Distinct())
            {
                var usuario = await _repo.ObtenerUsuarioVigentePorIdAsync(uid);
                if (usuario == null) continue;

                var correo = usuario.Correo?.Trim();
                if (string.IsNullOrWhiteSpace(correo))
                {
                    _logger.LogWarning("El corresponsable {UsuarioId} no tiene correo registrado.", uid);
                    continue;
                }

                var asunto = $"Asignación como corresponsable de actividad: {actividad.Clave}";
                var portalUrl = Url.Action("Index", "Gestor", null, protocol: HttpContext.Request.Scheme) ?? string.Empty;
                var cuerpo = ConstruirCorreoCorresponsableActividad(usuario.Nombre, actividad, portalUrl);

                try
                {
                    _logger.LogInformation("Enviando correo de corresponsabilidad de actividad {ActividadId} a {Correo}.", actividad.ActividadId, correo);
                    await _servicioEmailSMTP.EnviarCorreo(correo, asunto, cuerpo);
                }
                catch (Exception ex)
                {
                    _logger.LogError(ex, "Error enviando correo de corresponsabilidad de actividad {ActividadId} a {Correo}.", actividad.ActividadId, correo);
                }
            }
        }

        private async Task NotificarCorresponsablesTemaAsync(GestorTema tema, List<int> corresponsablesIds)
        {
            if (corresponsablesIds == null || !corresponsablesIds.Any()) return;

            foreach (var uid in corresponsablesIds.Distinct())
            {
                var usuario = await _repo.ObtenerUsuarioVigentePorIdAsync(uid);
                if (usuario == null) continue;

                var correo = usuario.Correo?.Trim();
                if (string.IsNullOrWhiteSpace(correo))
                {
                    _logger.LogWarning("El corresponsable {UsuarioId} no tiene correo registrado.", uid);
                    continue;
                }

                var asunto = $"Asignación como corresponsable de tema: {tema.Clave}";
                var portalUrl = Url.Action("Index", "Gestor", null, protocol: HttpContext.Request.Scheme) ?? string.Empty;
                var cuerpo = ConstruirCorreoCorresponsableTema(usuario.Nombre, tema, portalUrl);

                try
                {
                    _logger.LogInformation("Enviando correo de corresponsabilidad de tema {TemaId} a {Correo}.", tema.TemaId, correo);
                    await _servicioEmailSMTP.EnviarCorreo(correo, asunto, cuerpo);
                }
                catch (Exception ex)
                {
                    _logger.LogError(ex, "Error enviando correo de corresponsabilidad de tema {TemaId} a {Correo}.", tema.TemaId, correo);
                }
            }
        }

        private async Task NotificarCompartirTemaAsync(GestorTema tema, List<int> usuarioIds)
        {
            if (usuarioIds == null || !usuarioIds.Any()) return;

            foreach (var uid in usuarioIds.Distinct())
            {
                var usuario = await _repo.ObtenerUsuarioVigentePorIdAsync(uid);
                if (usuario == null) continue;

                var correo = usuario.Correo?.Trim();
                if (string.IsNullOrWhiteSpace(correo))
                {
                    _logger.LogWarning("El usuario {UsuarioId} no tiene correo registrado.", uid);
                    continue;
                }

                var asunto = $"Copia de conocimiento: {tema.Clave} - {tema.Tema}";
                var portalUrl = Url.Action("Index", "Gestor", null, protocol: HttpContext.Request.Scheme) ?? string.Empty;
                var cuerpo = ConstruirCorreoCompartirTema(usuario.Nombre, tema, portalUrl);

                try
                {
                    _logger.LogInformation("Enviando copia de conocimiento de tema {TemaId} a {Correo}.", tema.TemaId, correo);
                    await _servicioEmailSMTP.EnviarCorreo(correo, asunto, cuerpo);
                }
                catch (Exception ex)
                {
                    _logger.LogError(ex, "Error enviando copia de conocimiento de tema {TemaId} a {Correo}.", tema.TemaId, correo);
                }
            }
        }

        // ── Email template builders ───────────────────────────────────────────
        private static string ConstruirCorreoAsignacionActividad(string nombreResponsable, GestorActividad actividad, string portalUrl)
        {
            var fechaCompromiso = actividad.FechaCompromiso?.ToString("dd/MM/yyyy") ?? "Sin fecha definida";
            var fechaInicio = actividad.FechaInicio?.ToString("dd/MM/yyyy") ?? "Sin fecha definida";
            var descripcion = string.IsNullOrWhiteSpace(actividad.Descripcion) ? "Sin descripcion registrada." : actividad.Descripcion;
            var coResps = actividad.Corresponsables != null && actividad.Corresponsables.Any() 
                ? string.Join(", ", actividad.Corresponsables.Select(c => c.Nombre)) 
                : "Ninguno";

            return $@"
                <html lang='es'>
                <head>
                    <meta charset='UTF-8'>
                    <title>Asignación de actividad</title>
                </head>
                <body style='margin:0; padding:22px; background:#f2f2f2; font-family:Arial, Helvetica, sans-serif; color:#222;'>
                    <div style='max-width:760px; margin:0 auto; background:#ffffff; border:1px solid #dfdfdf; border-radius:10px; overflow:hidden;'>
                        <div style='padding:16px 20px; border-bottom:1px solid #eee;'>
                            <table role='presentation' cellpadding='0' cellspacing='0' border='0' style='width:100%;'>
                                <tr>
                                    <td style='width:50%;'>
                                        <img src='https://cdn.sassoapps.com/dgmesnie/logo_gob.png' alt='Gobierno de México' style='max-height:40px; width:auto;'>
                                    </td>
                                    <td style='width:50%; text-align:right;'>
                                        <img src='https://cdn.sassoapps.com/dgmesnie/logo_sener.png' alt='Secretaría de Energía' style='max-height:42px; width:auto;'>
                                    </td>
                                </tr>
                            </table>
                        </div>
                        <div style='background:#8a0031; color:#ffffff; padding:16px 20px; font-size:20px; font-weight:700;'>Nueva actividad asignada</div>
                        <div style='padding:22px 20px;'>
                            <p style='margin:0 0 12px; font-size:18px; font-weight:700; color:#1f2937;'>Hola, {nombreResponsable}.</p>
                            <p>Se te ha asignado una actividad dentro del Gestor de Actividades DGMESNIE.</p>
                            <table role='presentation' cellpadding='0' cellspacing='0' border='0' style='width:100%; border-collapse:collapse; margin:20px 0;'>
                                <tr><td style='padding:10px 12px; border:1px solid #e5c7d4; background:#f7ecf1; color:#6b1034; font-weight:700; width:32%;'>Clave</td><td style='padding:10px 12px; border:1px solid #eadde4;'>{actividad.Clave}</td></tr>
                                <tr><td style='padding:10px 12px; border:1px solid #e5c7d4; background:#f7ecf1; color:#6b1034; font-weight:700;'>Actividad</td><td style='padding:10px 12px; border:1px solid #eadde4;'>{actividad.Actividad}</td></tr>
                                <tr><td style='padding:10px 12px; border:1px solid #e5c7d4; background:#f7ecf1; color:#6b1034; font-weight:700;'>Descripción</td><td style='padding:10px 12px; border:1px solid #eadde4;'>{descripcion}</td></tr>
                                <tr><td style='padding:10px 12px; border:1px solid #e5c7d4; background:#f7ecf1; color:#6b1034; font-weight:700;'>Categoría</td><td style='padding:10px 12px; border:1px solid #eadde4;'>{actividad.Categoria ?? "Sin categoria"}</td></tr>
                                <tr><td style='padding:10px 12px; border:1px solid #e5c7d4; background:#f7ecf1; color:#6b1034; font-weight:700;'>Fecha de inicio</td><td style='padding:10px 12px; border:1px solid #eadde4;'>{fechaInicio}</td></tr>
                                <tr><td style='padding:10px 12px; border:1px solid #e5c7d4; background:#f7ecf1; color:#6b1034; font-weight:700;'>Fecha compromiso</td><td style='padding:10px 12px; border:1px solid #eadde4;'>{fechaCompromiso}</td></tr>
                                <tr><td style='padding:10px 12px; border:1px solid #e5c7d4; background:#f7ecf1; color:#6b1034; font-weight:700;'>Prioridad</td><td style='padding:10px 12px; border:1px solid #eadde4;'>{actividad.Prioridad}</td></tr>
                                <tr><td style='padding:10px 12px; border:1px solid #e5c7d4; background:#f7ecf1; color:#6b1034; font-weight:700;'>Estatus</td><td style='padding:10px 12px; border:1px solid #eadde4;'>{actividad.Estatus}</td></tr>
                                <tr><td style='padding:10px 12px; border:1px solid #e5c7d4; background:#f7ecf1; color:#6b1034; font-weight:700;'>Corresponsables</td><td style='padding:10px 12px; border:1px solid #eadde4;'>{coResps}</td></tr>
                            </table>
                            <div style='margin:18px 0 16px; text-align:center;'>
                                <a href='{portalUrl}' style='display:inline-block; padding:12px 20px; border-radius:8px; background:#8a0031; color:#ffffff; text-decoration:none; font-weight:700;'>
                                    Abrir Gestor de Actividades
                                </a>
                            </div>
                            <p style='margin:10px 0 0; font-size:13px; color:#555;'>Este aviso se envía automáticamente cuando una actividad se asigna o cambia de responsable principal.</p>
                        </div>
                    </div>
                </body>
                </html>";
        }

        private static string ConstruirCorreoAsignacionTema(string nombreResponsable, GestorTema tema, string portalUrl)
        {
            var fechaCompromiso = tema.FechaCompromiso?.ToString("dd/MM/yyyy") ?? "Sin fecha definida";
            var fechaInicio = tema.FechaInicio?.ToString("dd/MM/yyyy") ?? "Sin fecha definida";
            var descripcion = string.IsNullOrWhiteSpace(tema.Descripcion) ? "Sin descripcion registrada." : tema.Descripcion;
            var coResps = tema.Corresponsables != null && tema.Corresponsables.Any() 
                ? string.Join(", ", tema.Corresponsables.Select(c => c.Nombre)) 
                : "Ninguno";

            return $@"
                <html lang='es'>
                <head>
                    <meta charset='UTF-8'>
                    <title>Asignación de tema</title>
                </head>
                <body style='margin:0; padding:22px; background:#f2f2f2; font-family:Arial, Helvetica, sans-serif; color:#222;'>
                    <div style='max-width:760px; margin:0 auto; background:#ffffff; border:1px solid #dfdfdf; border-radius:10px; overflow:hidden;'>
                        <div style='padding:16px 20px; border-bottom:1px solid #eee;'>
                            <table role='presentation' cellpadding='0' cellspacing='0' border='0' style='width:100%;'>
                                <tr>
                                    <td style='width:50%;'>
                                        <img src='https://cdn.sassoapps.com/dgmesnie/logo_gob.png' alt='Gobierno de México' style='max-height:40px; width:auto;'>
                                    </td>
                                    <td style='width:50%; text-align:right;'>
                                        <img src='https://cdn.sassoapps.com/dgmesnie/logo_sener.png' alt='Secretaría de Energía' style='max-height:42px; width:auto;'>
                                    </td>
                                </tr>
                            </table>
                        </div>
                        <div style='background:#8a0031; color:#ffffff; padding:16px 20px; font-size:20px; font-weight:700;'>Nuevo tema asignado</div>
                        <div style='padding:22px 20px;'>
                            <p style='margin:0 0 12px; font-size:18px; font-weight:700; color:#1f2937;'>Hola, {nombreResponsable}.</p>
                            <p>Se te ha asignado un tema dentro del Gestor de Actividades DGMESNIE.</p>
                            <table role='presentation' cellpadding='0' cellspacing='0' border='0' style='width:100%; border-collapse:collapse; margin:20px 0;'>
                                <tr><td style='padding:10px 12px; border:1px solid #e5c7d4; background:#f7ecf1; color:#6b1034; font-weight:700; width:32%;'>Clave</td><td style='padding:10px 12px; border:1px solid #eadde4;'>{tema.Clave}</td></tr>
                                <tr><td style='padding:10px 12px; border:1px solid #e5c7d4; background:#f7ecf1; color:#6b1034; font-weight:700;'>Actividad</td><td style='padding:10px 12px; border:1px solid #eadde4;'>{tema.ActividadNombre ?? "Sin actividad"}</td></tr>
                                <tr><td style='padding:10px 12px; border:1px solid #e5c7d4; background:#f7ecf1; color:#6b1034; font-weight:700;'>Tema</td><td style='padding:10px 12px; border:1px solid #eadde4;'>{tema.Tema}</td></tr>
                                <tr><td style='padding:10px 12px; border:1px solid #e5c7d4; background:#f7ecf1; color:#6b1034; font-weight:700;'>Descripción</td><td style='padding:10px 12px; border:1px solid #eadde4;'>{descripcion}</td></tr>
                                <tr><td style='padding:10px 12px; border:1px solid #e5c7d4; background:#f7ecf1; color:#6b1034; font-weight:700;'>Fecha de inicio</td><td style='padding:10px 12px; border:1px solid #eadde4;'>{fechaInicio}</td></tr>
                                <tr><td style='padding:10px 12px; border:1px solid #e5c7d4; background:#f7ecf1; color:#6b1034; font-weight:700;'>Fecha compromiso</td><td style='padding:10px 12px; border:1px solid #eadde4;'>{fechaCompromiso}</td></tr>
                                <tr><td style='padding:10px 12px; border:1px solid #e5c7d4; background:#f7ecf1; color:#6b1034; font-weight:700;'>Prioridad</td><td style='padding:10px 12px; border:1px solid #eadde4;'>{tema.Prioridad}</td></tr>
                                <tr><td style='padding:10px 12px; border:1px solid #e5c7d4; background:#f7ecf1; color:#6b1034; font-weight:700;'>Estatus</td><td style='padding:10px 12px; border:1px solid #eadde4;'>{tema.Estatus}</td></tr>
                                <tr><td style='padding:10px 12px; border:1px solid #e5c7d4; background:#f7ecf1; color:#6b1034; font-weight:700;'>Corresponsables</td><td style='padding:10px 12px; border:1px solid #eadde4;'>{coResps}</td></tr>
                            </table>
                            {RenderEtapasHtml(tema.Etapas)}
                            <div style='margin:18px 0 16px; text-align:center;'>
                                <a href='{portalUrl}' style='display:inline-block; padding:12px 20px; border-radius:8px; background:#8a0031; color:#ffffff; text-decoration:none; font-weight:700;'>
                                    Abrir Gestor de Actividades
                                </a>
                            </div>
                            <p style='margin:10px 0 0; font-size:13px; color:#555;'>Este aviso se envía automáticamente cuando un tema se asigna o cambia de responsable.</p>
                        </div>
                    </div>
                </body>
                </html>";
        }

        private static string ConstruirCorreoRecordatorioTema(string nombreResponsable, GestorTema tema, string portalUrl)
        {
            var fechaCompromiso = tema.FechaCompromiso?.ToString("dd/MM/yyyy") ?? "Sin fecha definida";
            var descripcion = string.IsNullOrWhiteSpace(tema.Descripcion) ? "Sin descripción registrada." : tema.Descripcion;
            var coResps = tema.Corresponsables != null && tema.Corresponsables.Any() 
                ? string.Join(", ", tema.Corresponsables.Select(c => c.Nombre)) 
                : "Ninguno";

            return $@"
                <html lang='es'>
                <head>
                    <meta charset='UTF-8'>
                    <title>Recordatorio de tema</title>
                </head>
                <body style='margin:0; padding:22px; background:#f2f2f2; font-family:Arial, Helvetica, sans-serif; color:#222;'>
                    <div style='max-width:760px; margin:0 auto; background:#ffffff; border:1px solid #dfdfdf; border-radius:10px; overflow:hidden;'>
                        <div style='padding:16px 20px; border-bottom:1px solid #eee;'>
                            <table role='presentation' cellpadding='0' cellspacing='0' border='0' style='width:100%;'>
                                <tr>
                                    <td style='width:50%;'>
                                        <img src='https://cdn.sassoapps.com/dgmesnie/logo_gob.png' alt='Gobierno de México' style='max-height:40px; width:auto;'>
                                    </td>
                                    <td style='width:50%; text-align:right;'>
                                        <img src='https://cdn.sassoapps.com/dgmesnie/logo_sener.png' alt='Secretaría de Energía' style='max-height:42px; width:auto;'>
                                    </td>
                                </tr>
                            </table>
                        </div>
                        <div style='background:#8a0031; color:#ffffff; padding:16px 20px; font-size:20px; font-weight:700;'>Recordatorio de Tema Pendiente / Por Vencer</div>
                        <div style='padding:22px 20px;'>
                            <p style='margin:0 0 12px; font-size:18px; font-weight:700; color:#1f2937;'>Estimado(a) {nombreResponsable},</p>
                            <p>Le enviamos este recordatorio sobre un tema asignado a su cargo en el Gestor de Actividades DGMESNIE que requiere de su atención:</p>
                            <table role='presentation' cellpadding='0' cellspacing='0' border='0' style='width:100%; border-collapse:collapse; margin:20px 0;'>
                                <tr><td style='padding:10px 12px; border:1px solid #e5c7d4; background:#f7ecf1; color:#6b1034; font-weight:700; width:32%;'>Clave</td><td style='padding:10px 12px; border:1px solid #eadde4;'>{tema.Clave}</td></tr>
                                <tr><td style='padding:10px 12px; border:1px solid #e5c7d4; background:#f7ecf1; color:#6b1034; font-weight:700;'>Actividad</td><td style='padding:10px 12px; border:1px solid #eadde4;'>{tema.ActividadNombre ?? "Sin actividad"}</td></tr>
                                <tr><td style='padding:10px 12px; border:1px solid #e5c7d4; background:#f7ecf1; color:#6b1034; font-weight:700;'>Tema</td><td style='padding:10px 12px; border:1px solid #eadde4;'>{tema.Tema}</td></tr>
                                <tr><td style='padding:10px 12px; border:1px solid #e5c7d4; background:#f7ecf1; color:#6b1034; font-weight:700;'>Descripción</td><td style='padding:10px 12px; border:1px solid #eadde4;'>{descripcion}</td></tr>
                                <tr><td style='padding:10px 12px; border:1px solid #e5c7d4; background:#f7ecf1; color:#6b1034; font-weight:700;'>Fecha compromiso</td><td style='padding:10px 12px; border:1px solid #e5c7d4; font-weight: 700; color: #8a0031;'>{fechaCompromiso}</td></tr>
                                <tr><td style='padding:10px 12px; border:1px solid #e5c7d4; background:#f7ecf1; color:#6b1034; font-weight:700;'>Estatus</td><td style='padding:10px 12px; border:1px solid #eadde4;'>{tema.Estatus}</td></tr>
                                <tr><td style='padding:10px 12px; border:1px solid #e5c7d4; background:#f7ecf1; color:#6b1034; font-weight:700;'>Avance</td><td style='padding:10px 12px; border:1px solid #eadde4;'>{tema.Avance}%</td></tr>
                                <tr><td style='padding:10px 12px; border:1px solid #e5c7d4; background:#f7ecf1; color:#6b1034; font-weight:700;'>Corresponsables</td><td style='padding:10px 12px; border:1px solid #eadde4;'>{coResps}</td></tr>
                            </table>
                            {RenderEtapasHtml(tema.Etapas)}
                            <p style='margin-bottom: 20px;'>Agradecemos de antemano su valiosa colaboración para mantener al día el seguimiento de estos compromisos institucionales.</p>
                            <div style='margin:18px 0 16px; text-align:center;'>
                                <a href='{portalUrl}' style='display:inline-block; padding:12px 20px; border-radius:8px; background:#8a0031; color:#ffffff; text-decoration:none; font-weight:700;'>
                                    Abrir Gestor de Actividades
                                </a>
                            </div>
                            <p style='margin:10px 0 0; font-size:13px; color:#555;'>Este aviso se envía a solicitud del administrador o coordinador del seguimiento en la plataforma.</p>
                        </div>
                    </div>
                </body>
                </html>";
        }

        private static string ConstruirCorreoCompartirTema(string nombreDestinatario, GestorTema tema, string portalUrl)
        {
            var fechaCompromiso = tema.FechaCompromiso?.ToString("dd/MM/yyyy") ?? "Sin fecha definida";
            var fechaInicio = tema.FechaInicio?.ToString("dd/MM/yyyy") ?? "Sin fecha definida";
            var descripcion = string.IsNullOrWhiteSpace(tema.Descripcion) ? "Sin descripción registrada." : tema.Descripcion;
            var coResps = tema.Corresponsables != null && tema.Corresponsables.Any() 
                ? string.Join(", ", tema.Corresponsables.Select(c => c.Nombre)) 
                : "Ninguno";

            return $@"
                <html lang='es'>
                <head>
                    <meta charset='UTF-8'>
                    <title>Copia de conocimiento</title>
                </head>
                <body style='margin:0; padding:22px; background:#f2f2f2; font-family:Arial, Helvetica, sans-serif; color:#222;'>
                    <div style='max-width:760px; margin:0 auto; background:#ffffff; border:1px solid #dfdfdf; border-radius:10px; overflow:hidden;'>
                        <div style='padding:16px 20px; border-bottom:1px solid #eee;'>
                            <table role='presentation' cellpadding='0' cellspacing='0' border='0' style='width:100%;'>
                                <tr>
                                    <td style='width:50%;'>
                                        <img src='https://cdn.sassoapps.com/dgmesnie/logo_gob.png' alt='Gobierno de México' style='max-height:40px; width:auto;'>
                                    </td>
                                    <td style='width:50%; text-align:right;'>
                                        <img src='https://cdn.sassoapps.com/dgmesnie/logo_sener.png' alt='Secretaría de Energía' style='max-height:42px; width:auto;'>
                                    </td>
                                </tr>
                            </table>
                        </div>
                        <div style='background:#8a0031; color:#ffffff; padding:16px 20px; font-size:20px; font-weight:700;'>Copia de conocimiento</div>
                        <div style='padding:22px 20px;'>
                            <p style='margin:0 0 12px; font-size:18px; font-weight:700; color:#1f2937;'>Estimado(a) {nombreDestinatario},</p>
                            <p>Se le comparte este tema para su conocimiento. No requiere una acción directa, salvo que se le indique por otro medio.</p>
                            <table role='presentation' cellpadding='0' cellspacing='0' border='0' style='width:100%; border-collapse:collapse; margin:20px 0;'>
                                <tr><td style='padding:10px 12px; border:1px solid #e5c7d4; background:#f7ecf1; color:#6b1034; font-weight:700; width:32%;'>Clave</td><td style='padding:10px 12px; border:1px solid #eadde4;'>{tema.Clave}</td></tr>
                                <tr><td style='padding:10px 12px; border:1px solid #e5c7d4; background:#f7ecf1; color:#6b1034; font-weight:700;'>Actividad</td><td style='padding:10px 12px; border:1px solid #eadde4;'>{tema.ActividadNombre ?? "Sin actividad"}</td></tr>
                                <tr><td style='padding:10px 12px; border:1px solid #e5c7d4; background:#f7ecf1; color:#6b1034; font-weight:700;'>Tema</td><td style='padding:10px 12px; border:1px solid #eadde4;'>{tema.Tema}</td></tr>
                                <tr><td style='padding:10px 12px; border:1px solid #e5c7d4; background:#f7ecf1; color:#6b1034; font-weight:700;'>Responsable</td><td style='padding:10px 12px; border:1px solid #eadde4;'>{tema.ResponsableNombre ?? "Sin responsable asignado"}</td></tr>
                                <tr><td style='padding:10px 12px; border:1px solid #e5c7d4; background:#f7ecf1; color:#6b1034; font-weight:700;'>Descripción</td><td style='padding:10px 12px; border:1px solid #eadde4;'>{descripcion}</td></tr>
                                <tr><td style='padding:10px 12px; border:1px solid #e5c7d4; background:#f7ecf1; color:#6b1034; font-weight:700;'>Fecha de inicio</td><td style='padding:10px 12px; border:1px solid #eadde4;'>{fechaInicio}</td></tr>
                                <tr><td style='padding:10px 12px; border:1px solid #e5c7d4; background:#f7ecf1; color:#6b1034; font-weight:700;'>Fecha compromiso</td><td style='padding:10px 12px; border:1px solid #eadde4;'>{fechaCompromiso}</td></tr>
                                <tr><td style='padding:10px 12px; border:1px solid #e5c7d4; background:#f7ecf1; color:#6b1034; font-weight:700;'>Estatus</td><td style='padding:10px 12px; border:1px solid #eadde4;'>{tema.Estatus}</td></tr>
                                <tr><td style='padding:10px 12px; border:1px solid #e5c7d4; background:#f7ecf1; color:#6b1034; font-weight:700;'>Avance</td><td style='padding:10px 12px; border:1px solid #eadde4;'>{tema.Avance}%</td></tr>
                                <tr><td style='padding:10px 12px; border:1px solid #e5c7d4; background:#f7ecf1; color:#6b1034; font-weight:700;'>Prioridad</td><td style='padding:10px 12px; border:1px solid #eadde4;'>{tema.Prioridad}</td></tr>
                                <tr><td style='padding:10px 12px; border:1px solid #e5c7d4; background:#f7ecf1; color:#6b1034; font-weight:700;'>Corresponsables</td><td style='padding:10px 12px; border:1px solid #eadde4;'>{coResps}</td></tr>
                            </table>
                            {RenderEtapasHtml(tema.Etapas)}
                            <div style='margin:18px 0 16px; text-align:center;'>
                                <a href='{portalUrl}' style='display:inline-block; padding:12px 20px; border-radius:8px; background:#8a0031; color:#ffffff; text-decoration:none; font-weight:700;'>
                                    Abrir Gestor de Actividades
                                </a>
                            </div>
                            <p style='margin:10px 0 0; font-size:13px; color:#555;'>Este aviso se envía solo para conocimiento. No lo agrega como responsable ni corresponsable del tema.</p>
                        </div>
                    </div>
                </body>
                </html>";
        }

        private static string ConstruirCorreoCorresponsableActividad(string nombreUsuario, GestorActividad actividad, string portalUrl)
        {
            var fechaCompromiso = actividad.FechaCompromiso?.ToString("dd/MM/yyyy") ?? "Sin fecha definida";
            var fechaInicio = actividad.FechaInicio?.ToString("dd/MM/yyyy") ?? "Sin fecha definida";
            var descripcion = string.IsNullOrWhiteSpace(actividad.Descripcion) ? "Sin descripcion registrada." : actividad.Descripcion;

            return $@"
                <html lang='es'>
                <head>
                    <meta charset='UTF-8'>
                    <title>Asignación como corresponsable de actividad</title>
                </head>
                <body style='margin:0; padding:22px; background:#f2f2f2; font-family:Arial, Helvetica, sans-serif; color:#222;'>
                    <div style='max-width:760px; margin:0 auto; background:#ffffff; border:1px solid #dfdfdf; border-radius:10px; overflow:hidden;'>
                        <div style='padding:16px 20px; border-bottom:1px solid #eee;'>
                            <table role='presentation' cellpadding='0' cellspacing='0' border='0' style='width:100%;'>
                                <tr>
                                    <td style='width:50%;'>
                                        <img src='https://cdn.sassoapps.com/dgmesnie/logo_gob.png' alt='Gobierno de México' style='max-height:40px; width:auto;'>
                                    </td>
                                    <td style='width:50%; text-align:right;'>
                                        <img src='https://cdn.sassoapps.com/dgmesnie/logo_sener.png' alt='Secretaría de Energía' style='max-height:42px; width:auto;'>
                                    </td>
                                </tr>
                            </table>
                        </div>
                        <div style='background:#8a0031; color:#ffffff; padding:16px 20px; font-size:20px; font-weight:700;'>Asignación como Corresponsable de Actividad</div>
                        <div style='padding:22px 20px;'>
                            <p style='margin:0 0 12px; font-size:18px; font-weight:700; color:#1f2937;'>Hola, {nombreUsuario}.</p>
                            <p>Se te ha asignado como <strong>corresponsable</strong> (trabajo en conjunto) en la siguiente actividad dentro del Gestor de Actividades DGMESNIE:</p>
                            <table role='presentation' cellpadding='0' cellspacing='0' border='0' style='width:100%; border-collapse:collapse; margin:20px 0;'>
                                <tr><td style='padding:10px 12px; border:1px solid #e5c7d4; background:#f7ecf1; color:#6b1034; font-weight:700; width:32%;'>Clave</td><td style='padding:10px 12px; border:1px solid #eadde4;'>{actividad.Clave}</td></tr>
                                <tr><td style='padding:10px 12px; border:1px solid #e5c7d4; background:#f7ecf1; color:#6b1034; font-weight:700;'>Actividad</td><td style='padding:10px 12px; border:1px solid #eadde4;'>{actividad.Actividad}</td></tr>
                                <tr><td style='padding:10px 12px; border:1px solid #e5c7d4; background:#f7ecf1; color:#6b1034; font-weight:700;'>Responsable Principal</td><td style='padding:10px 12px; border:1px solid #eadde4;'>{actividad.ResponsableNombre ?? "Sin responsable asignado"}</td></tr>
                                <tr><td style='padding:10px 12px; border:1px solid #e5c7d4; background:#f7ecf1; color:#6b1034; font-weight:700;'>Descripción</td><td style='padding:10px 12px; border:1px solid #eadde4;'>{descripcion}</td></tr>
                                <tr><td style='padding:10px 12px; border:1px solid #e5c7d4; background:#f7ecf1; color:#6b1034; font-weight:700;'>Categoría</td><td style='padding:10px 12px; border:1px solid #eadde4;'>{actividad.Categoria ?? "Sin categoria"}</td></tr>
                                <tr><td style='padding:10px 12px; border:1px solid #e5c7d4; background:#f7ecf1; color:#6b1034; font-weight:700;'>Fecha compromiso</td><td style='padding:10px 12px; border:1px solid #eadde4;'>{fechaCompromiso}</td></tr>
                                <tr><td style='padding:10px 12px; border:1px solid #e5c7d4; background:#f7ecf1; color:#6b1034; font-weight:700;'>Prioridad</td><td style='padding:10px 12px; border:1px solid #eadde4;'>{actividad.Prioridad}</td></tr>
                            </table>
                            <div style='margin:18px 0 16px; text-align:center;'>
                                <a href='{portalUrl}' style='display:inline-block; padding:12px 20px; border-radius:8px; background:#8a0031; color:#ffffff; text-decoration:none; font-weight:700;'>
                                    Abrir Gestor de Actividades
                                </a>
                            </div>
                            <p style='margin:10px 0 0; font-size:13px; color:#555;'>Este aviso se envía automáticamente cuando eres asignado como corresponsable de una actividad.</p>
                        </div>
                    </div>
                </body>
                </html>";
        }

        private static string ConstruirCorreoCorresponsableTema(string nombreUsuario, GestorTema tema, string portalUrl)
        {
            var fechaCompromiso = tema.FechaCompromiso?.ToString("dd/MM/yyyy") ?? "Sin fecha definida";
            var descripcion = string.IsNullOrWhiteSpace(tema.Descripcion) ? "Sin descripcion registrada." : tema.Descripcion;

            return $@"
                <html lang='es'>
                <head>
                    <meta charset='UTF-8'>
                    <title>Asignación como corresponsable de tema</title>
                </head>
                <body style='margin:0; padding:22px; background:#f2f2f2; font-family:Arial, Helvetica, sans-serif; color:#222;'>
                    <div style='max-width:760px; margin:0 auto; background:#ffffff; border:1px solid #dfdfdf; border-radius:10px; overflow:hidden;'>
                        <div style='padding:16px 20px; border-bottom:1px solid #eee;'>
                            <table role='presentation' cellpadding='0' cellspacing='0' border='0' style='width:100%;'>
                                <tr>
                                    <td style='width:50%;'>
                                        <img src='https://cdn.sassoapps.com/dgmesnie/logo_gob.png' alt='Gobierno de México' style='max-height:40px; width:auto;'>
                                    </td>
                                    <td style='width:50%; text-align:right;'>
                                        <img src='https://cdn.sassoapps.com/dgmesnie/logo_sener.png' alt='Secretaría de Energía' style='max-height:42px; width:auto;'>
                                    </td>
                                </tr>
                            </table>
                        </div>
                        <div style='background:#8a0031; color:#ffffff; padding:16px 20px; font-size:20px; font-weight:700;'>Asignación como Corresponsable de Tema</div>
                        <div style='padding:22px 20px;'>
                            <p style='margin:0 0 12px; font-size:18px; font-weight:700; color:#1f2937;'>Hola, {nombreUsuario}.</p>
                            <p>Se te ha asignado como <strong>corresponsable</strong> (trabajo en conjunto) en el siguiente tema dentro del Gestor de Actividades DGMESNIE:</p>
                            <table role='presentation' cellpadding='0' cellspacing='0' border='0' style='width:100%; border-collapse:collapse; margin:20px 0;'>
                                <tr><td style='padding:10px 12px; border:1px solid #e5c7d4; background:#f7ecf1; color:#6b1034; font-weight:700; width:32%;'>Clave</td><td style='padding:10px 12px; border:1px solid #eadde4;'>{tema.Clave}</td></tr>
                                <tr><td style='padding:10px 12px; border:1px solid #e5c7d4; background:#f7ecf1; color:#6b1034; font-weight:700;'>Actividad</td><td style='padding:10px 12px; border:1px solid #eadde4;'>{tema.ActividadNombre ?? "Sin actividad"}</td></tr>
                                <tr><td style='padding:10px 12px; border:1px solid #e5c7d4; background:#f7ecf1; color:#6b1034; font-weight:700;'>Tema</td><td style='padding:10px 12px; border:1px solid #eadde4;'>{tema.Tema}</td></tr>
                                <tr><td style='padding:10px 12px; border:1px solid #e5c7d4; background:#f7ecf1; color:#6b1034; font-weight:700;'>Responsable Principal</td><td style='padding:10px 12px; border:1px solid #eadde4;'>{tema.ResponsableNombre ?? "Sin responsable asignado"}</td></tr>
                                <tr><td style='padding:10px 12px; border:1px solid #e5c7d4; background:#f7ecf1; color:#6b1034; font-weight:700;'>Descripción</td><td style='padding:10px 12px; border:1px solid #eadde4;'>{descripcion}</td></tr>
                                <tr><td style='padding:10px 12px; border:1px solid #e5c7d4; background:#f7ecf1; color:#6b1034; font-weight:700;'>Fecha compromiso</td><td style='padding:10px 12px; border:1px solid #eadde4;'>{fechaCompromiso}</td></tr>
                                <tr><td style='padding:10px 12px; border:1px solid #e5c7d4; background:#f7ecf1; color:#6b1034; font-weight:700;'>Prioridad</td><td style='padding:10px 12px; border:1px solid #eadde4;'>{tema.Prioridad}</td></tr>
                            </table>
                            {RenderEtapasHtml(tema.Etapas)}
                            <div style='margin:18px 0 16px; text-align:center;'>
                                <a href='{portalUrl}' style='display:inline-block; padding:12px 20px; border-radius:8px; background:#8a0031; color:#ffffff; text-decoration:none; font-weight:700;'>
                                    Abrir Gestor de Actividades
                                </a>
                            </div>
                            <p style='margin:10px 0 0; font-size:13px; color:#555;'>Este aviso se envía automáticamente cuando eres asignado como corresponsable de un tema.</p>
                        </div>
                    </div>
                </body>
                </html>";
        }

        private static string RenderEtapasHtml(List<GestorEtapa> etapas)
        {
            if (etapas == null || !etapas.Any()) return "";

            var activeStage = etapas.FirstOrDefault(e => e.Avance < 100) ?? etapas.Last();

            var sb = new System.Text.StringBuilder();
            sb.Append("<div style='margin-top:20px; border-top:1px solid #dfdfdf; padding-top:16px;'>");
            sb.Append("<h3 style='font-size:16px; font-weight:700; color:#8a0031; margin:0 0 12px 0;'>Secuencia y Trazabilidad de Etapas</h3>");
            sb.Append("<div style='display:flex; flex-direction:column; gap:10px;'>");

            int index = 1;
            foreach (var e in etapas.OrderBy(x => x.Orden).ThenBy(x => x.EtapaId))
            {
                var isCompleted = e.Avance == 100 || e.Estatus == "Concluida";
                var isActive = e == activeStage;

                var bgStyle = isActive ? "background:#fdfaf6; border-left:4px solid #d97706;" : "background:#f8fafc; border-left:4px solid #cbd5e1;";
                var badgeBg = isCompleted ? "#d1fae5" : isActive ? "#fef3c7" : "#f1f5f9";
                var badgeColor = isCompleted ? "#065f46" : isActive ? "#92400e" : "#475569";
                var statusText = isCompleted ? "Concluida" : isActive ? "En curso" : "Pendiente";
                var badgeText = isActive ? $"{statusText} &middot; &iexcl;Te toca!" : statusText;

                var dateRange = e.FechaInicio.HasValue 
                    ? $"{e.FechaInicio.Value:dd/MM/yyyy} al {e.FechaCompromiso.Value:dd/MM/yyyy}"
                    : e.FechaCompromiso.HasValue ? $"Compromiso: {e.FechaCompromiso.Value:dd/MM/yyyy}" : "Sin fechas";

                sb.Append($@"
                    <div style='padding:12px; border:1px solid #e2e8f0; border-radius:8px; {bgStyle}'>
                        <div style='display:flex; align-items:center; justify-content:space-between; margin-bottom:6px;'>
                            <span style='font-weight:700; font-size:14px; color:#0f172a;'>Etapa {index}: {System.Net.WebUtility.HtmlEncode(e.Nombre)}</span>
                            <span style='padding:2px 8px; border-radius:6px; font-size:11px; font-weight:700; background:{badgeBg}; color:{badgeColor};'>{badgeText}</span>
                        </div>
                        <div style='font-size:12px; color:#64748b;'>
                            <strong>Asignado a:</strong> {System.Net.WebUtility.HtmlEncode(e.ResponsableNombre ?? "Sin asignar")} &nbsp;|&nbsp; 
                            <strong>Periodo:</strong> {dateRange} &nbsp;|&nbsp; 
                            <strong>Avance:</strong> {e.Avance}%
                        </div>
                    </div>");
                index++;
            }

            sb.Append("</div>");
            sb.Append("</div>");
            return sb.ToString();
        }

        private static string ConstruirCorreoCompartirActividad(string nombreDestinatario, GestorActividad actividad, string portalUrl)
        {
            var fechaCompromiso = actividad.FechaCompromiso?.ToString("dd/MM/yyyy") ?? "Sin fecha definida";
            var fechaInicio     = actividad.FechaInicio?.ToString("dd/MM/yyyy")     ?? "Sin fecha definida";
            var descripcion     = string.IsNullOrWhiteSpace(actividad.Descripcion) ? "Sin descripción registrada." : actividad.Descripcion;
            var coResps         = actividad.Corresponsables != null && actividad.Corresponsables.Any()
                ? string.Join(", ", actividad.Corresponsables.Select(c => c.Nombre))
                : "Ninguno";

            return $@"
                <html lang='es'>
                <head>
                    <meta charset='UTF-8'>
                    <title>Actividad compartida</title>
                </head>
                <body style='margin:0; padding:22px; background:#f2f2f2; font-family:Arial, Helvetica, sans-serif; color:#222;'>
                    <div style='max-width:760px; margin:0 auto; background:#ffffff; border:1px solid #dfdfdf; border-radius:10px; overflow:hidden;'>
                        <div style='padding:16px 20px; border-bottom:1px solid #eee;'>
                            <table role='presentation' cellpadding='0' cellspacing='0' border='0' style='width:100%;'>
                                <tr>
                                    <td style='width:50%;'><img src='https://cdn.sassoapps.com/dgmesnie/logo_gob.png' alt='Gobierno de México' style='max-height:40px; width:auto;'></td>
                                    <td style='width:50%; text-align:right;'><img src='https://cdn.sassoapps.com/dgmesnie/logo_sener.png' alt='Secretaría de Energía' style='max-height:42px; width:auto;'></td>
                                </tr>
                            </table>
                        </div>
                        <div style='background:#8a0031; color:#ffffff; padding:16px 20px; font-size:20px; font-weight:700;'>📋 Actividad Compartida — Gestor DGMESNIE</div>
                        <div style='padding:22px 20px;'>
                            <p style='margin:0 0 12px; font-size:18px; font-weight:700; color:#1f2937;'>Estimado(a) {nombreDestinatario},</p>
                            <p>Le compartimos los detalles de la siguiente actividad registrada en el Gestor de Actividades DGMESNIE:</p>
                            <table role='presentation' cellpadding='0' cellspacing='0' border='0' style='width:100%; border-collapse:collapse; margin:20px 0;'>
                                <tr><td style='padding:10px 12px; border:1px solid #e5c7d4; background:#f7ecf1; color:#6b1034; font-weight:700; width:32%;'>Clave</td><td style='padding:10px 12px; border:1px solid #eadde4;'>{actividad.Clave}</td></tr>
                                <tr><td style='padding:10px 12px; border:1px solid #e5c7d4; background:#f7ecf1; color:#6b1034; font-weight:700;'>Actividad</td><td style='padding:10px 12px; border:1px solid #eadde4; font-weight:700;'>{actividad.Actividad}</td></tr>
                                <tr><td style='padding:10px 12px; border:1px solid #e5c7d4; background:#f7ecf1; color:#6b1034; font-weight:700;'>Responsable</td><td style='padding:10px 12px; border:1px solid #eadde4;'>{actividad.ResponsableNombre ?? "Sin responsable"}</td></tr>
                                <tr><td style='padding:10px 12px; border:1px solid #e5c7d4; background:#f7ecf1; color:#6b1034; font-weight:700;'>Corresponsables</td><td style='padding:10px 12px; border:1px solid #eadde4;'>{coResps}</td></tr>
                                <tr><td style='padding:10px 12px; border:1px solid #e5c7d4; background:#f7ecf1; color:#6b1034; font-weight:700;'>Descripción</td><td style='padding:10px 12px; border:1px solid #eadde4;'>{descripcion}</td></tr>
                                <tr><td style='padding:10px 12px; border:1px solid #e5c7d4; background:#f7ecf1; color:#6b1034; font-weight:700;'>Fecha de inicio</td><td style='padding:10px 12px; border:1px solid #eadde4;'>{fechaInicio}</td></tr>
                                <tr><td style='padding:10px 12px; border:1px solid #e5c7d4; background:#f7ecf1; color:#6b1034; font-weight:700;'>Fecha compromiso</td><td style='padding:10px 12px; border:1px solid #e5c7d4; font-weight:700; color:#8a0031;'>{fechaCompromiso}</td></tr>
                                <tr><td style='padding:10px 12px; border:1px solid #e5c7d4; background:#f7ecf1; color:#6b1034; font-weight:700;'>Estatus</td><td style='padding:10px 12px; border:1px solid #eadde4;'>{actividad.Estatus}</td></tr>
                                <tr><td style='padding:10px 12px; border:1px solid #e5c7d4; background:#f7ecf1; color:#6b1034; font-weight:700;'>Prioridad</td><td style='padding:10px 12px; border:1px solid #eadde4;'>{actividad.Prioridad}</td></tr>
                            </table>
                            <div style='margin:18px 0 16px; text-align:center;'>
                                <a href='{portalUrl}' style='display:inline-block; padding:12px 20px; border-radius:8px; background:#8a0031; color:#ffffff; text-decoration:none; font-weight:700;'>
                                    Abrir Gestor de Actividades
                                </a>
                            </div>
                            <p style='margin:10px 0 0; font-size:13px; color:#555;'>Este correo se envió automáticamente a través del Gestor de Actividades DGMESNIE.</p>
                        </div>
                    </div>
                </body>
                </html>";
        }

        private HeaderViewModel BuildHeader() => new()
        {
            Title = "Gestor de Actividades",
            IconPath = "proyecto.png",
            Description = "Control de actividades y temas de la Dirección General con semáforo, gantt, kanban y reportes institucionales.",
            Section = "Seguimiento de actividades",
            ModuleInfo = JsonConvert.SerializeObject(new
            {
                title = "Gestor de Actividades DGMESNIE",
                description = "Módulo para planear, dar seguimiento y reportar el avance de actividades y temas institucionales.",
                functionality = "Tablero ejecutivo con 9 vistas: dashboard, actividades, kanban, tabla, gantt, calendario, responsables, alertas y reportes.",
                stage = "Gestión operativa",
                highlights = new[]
                {
                    "Semáforo automático por fecha compromiso y estatus.",
                    "Exportación a PDF, PPT y Excel con plantilla institucional.",
                    "Rastreo de corresponsables con FK a tabla de usuarios NSIE."
                },
                roles = new[]
                {
                    new { icon = "clipboard-list", text = "Directivos y coordinadores de seguimiento." },
                    new { icon = "users", text = "Responsables de actividades y temas institucionales." }
                },
                order = new { step = 1, description = "Seguimiento de actividades y compromisos." }
            })
        };
    }
}
