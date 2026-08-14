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
            var hoy = DateTime.Today;
            var diasALunes = ((int)hoy.DayOfWeek - (int)DayOfWeek.Monday + 7) % 7;
            var lunes = hoy.AddDays(-diasALunes);
            var domingo = lunes.AddDays(6);
            var cultura = new System.Globalization.CultureInfo("es-MX");
            var rangoSemana = $"{lunes.ToString("dd 'de' MMMM", cultura)} al {domingo.ToString("dd 'de' MMMM 'de' yyyy", cultura)}";

            var kpis = request.Kpis;
            string Kpi(string clave, string porOmision) => kpis.GetValueOrDefault(clave, porOmision);

            // Las gráficas llegan como SVG o como data URI ya armado. Un SVG suelto
            // no se ve en la mayoría de los clientes de correo, así que sólo se
            // publica lo que ya viene como imagen.
            static string ComoImagen(string valor) =>
                !string.IsNullOrWhiteSpace(valor) && valor.StartsWith("data:image/") ? valor : null;

            var imagenes = new List<ImagenCorreo>();
            void Agregar(string titulo, string fuente)
            {
                var imagen = ComoImagen(fuente);
                if (imagen != null) imagenes.Add(new ImagenCorreo(titulo, imagen));
            }
            Agregar("Estatus general", request.EstatusSvg);
            Agregar("Distribución de prioridad", request.PrioridadSvg);
            Agregar("Avance global", request.AvanceSvg);
            Agregar("Carga por responsable", request.ResponsablesSvg);

            var tablaActividades = new TablaCorreo
            {
                Titulo = "Avance por actividad",
                Encabezados = new[] { "Actividad", "Responsable", "Total", "Concluidos", "Por vencer", "Vencidos", "Avance" },
                Filas = request.Actividades.Select(a => (IReadOnlyList<string>)new[]
                {
                    a.Actividad, a.Responsable, a.Total.ToString("N0"), a.Concluidos.ToString("N0"),
                    a.PorVencer.ToString("N0"), a.Vencidos.ToString("N0"), a.Avance + " %"
                }).ToList(),
                TextoVacio = "No hay actividades registradas en este periodo."
            };

            var tablaResponsables = new TablaCorreo
            {
                Titulo = "Avance por responsable",
                Encabezados = new[] { "Responsable", "Total temas", "Concluidos", "Por vencer", "Vencidos", "Avance" },
                Filas = request.Responsables.Select(r => (IReadOnlyList<string>)new[]
                {
                    r.Responsable, r.Total.ToString("N0"), r.Concluidos.ToString("N0"),
                    r.PorVencer.ToString("N0"), r.Vencidos.ToString("N0"), r.Avance + " %"
                }).ToList(),
                TextoVacio = "No hay temas registrados en este periodo."
            };

            return PlantillaCorreoInstitucional.Construir(new ContenidoCorreo
            {
                Antetitulo = "Gestor de actividades · DGMESNIE",
                Titulo = "Reporte de avance semanal",
                Metadatos = new[]
                {
                    new CampoCorreo("Semana", rangoSemana),
                    new CampoCorreo("Avance global", Kpi("avanceGlobal", "0%"))
                },
                Saludo = string.IsNullOrWhiteSpace(nombreUsuario) ? "Estimada(o)" : $"Estimada(o) {nombreUsuario}",
                Parrafos = new[]
                {
                    "Corte semanal de las actividades y temas críticos registrados en el Gestor de la DGMESNIE."
                },
                Datos = new[]
                {
                    new CampoCorreo("Actividades activas", Kpi("actividadesActivas", "0")),
                    new CampoCorreo("Temas registrados", Kpi("temasRegistrados", "0")),
                    new CampoCorreo("Temas concluidos", Kpi("temasConcluidos", "0")),
                    new CampoCorreo("Temas por vencer", Kpi("temasPorVencer", "0")),
                    new CampoCorreo("Temas vencidos", Kpi("temasVencidos", "0"))
                },
                Tablas = new[] { tablaActividades, tablaResponsables },
                ImagenesTitulo = imagenes.Count > 0 ? "Gráficas del periodo" : null,
                Imagenes = imagenes,
                Nota = "Corte automático de la semana en curso. El detalle vive en el Gestor de Actividades.",
                PieAviso = "Este correo se genera automáticamente y no requiere respuesta."
            });
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

        // ── Correos del gestor ────────────────────────────────────────────────
        // Los siete avisos del gestor decían lo mismo con distinto HTML: cabecera,
        // barra de título, saludo, tabla de datos y botón. Ahora comparten la
        // plantilla institucional y sólo aportan su texto y sus campos.
        private static string CorreoGestor(
            string antetitulo,
            string titulo,
            string nombre,
            string parrafo,
            IReadOnlyList<CampoCorreo> datos,
            string portalUrl,
            string nota)
        {
            return PlantillaCorreoInstitucional.Construir(new ContenidoCorreo
            {
                Antetitulo = antetitulo,
                Titulo = titulo,
                Saludo = string.IsNullOrWhiteSpace(nombre) ? "Estimada(o)" : $"Estimada(o) {nombre}",
                Parrafos = new[] { System.Net.WebUtility.HtmlEncode(parrafo) },
                Datos = datos,
                BotonTexto = string.IsNullOrWhiteSpace(portalUrl) ? null : "Abrir el Gestor de Actividades",
                BotonUrl = portalUrl,
                Nota = nota,
                PieAviso = "Este correo se genera automáticamente y no requiere respuesta."
            });
        }

        private static string Corresponsables(IEnumerable<GestorUsuarioDto> lista)
        {
            var nombres = lista?.Select(c => c.Nombre).Where(n => !string.IsNullOrWhiteSpace(n)).ToList();
            return nombres != null && nombres.Count > 0 ? string.Join(", ", nombres) : "Ninguno";
        }

        private static string Fecha(DateTime? valor) =>
            valor?.ToString("dd/MM/yyyy") ?? "Sin fecha definida";

        private static IReadOnlyList<CampoCorreo> DatosActividad(
            GestorActividad actividad, bool incluirCategoria, bool incluirEstatus, bool incluirResponsable)
        {
            var datos = new List<CampoCorreo>
            {
                new CampoCorreo("Clave", actividad.Clave),
                new CampoCorreo("Actividad", actividad.Actividad)
            };
            if (incluirResponsable)
                datos.Add(new CampoCorreo("Responsable", actividad.ResponsableNombre ?? "Sin responsable asignado"));
            datos.Add(new CampoCorreo("Descripción",
                string.IsNullOrWhiteSpace(actividad.Descripcion) ? "Sin descripción registrada." : actividad.Descripcion));
            if (incluirCategoria)
                datos.Add(new CampoCorreo("Categoría", actividad.Categoria ?? "Sin categoría"));
            datos.Add(new CampoCorreo("Fecha de inicio", Fecha(actividad.FechaInicio)));
            datos.Add(new CampoCorreo("Fecha compromiso", Fecha(actividad.FechaCompromiso)));
            datos.Add(new CampoCorreo("Prioridad", actividad.Prioridad));
            if (incluirEstatus)
                datos.Add(new CampoCorreo("Estatus", actividad.Estatus));
            datos.Add(new CampoCorreo("Corresponsables", Corresponsables(actividad.Corresponsables)));
            return datos;
        }

        private static IReadOnlyList<CampoCorreo> DatosTema(
            GestorTema tema, bool incluirInicio, bool incluirEstatus, bool incluirPrioridad, bool incluirAvance,
            string responsableEtiqueta)
        {
            var datos = new List<CampoCorreo>
            {
                new CampoCorreo("Clave", tema.Clave),
                new CampoCorreo("Actividad", tema.ActividadNombre ?? "Sin actividad"),
                new CampoCorreo("Tema", tema.Tema)
            };
            if (!string.IsNullOrWhiteSpace(responsableEtiqueta))
                datos.Add(new CampoCorreo(responsableEtiqueta, tema.ResponsableNombre ?? "Sin responsable asignado"));
            datos.Add(new CampoCorreo("Descripción",
                string.IsNullOrWhiteSpace(tema.Descripcion) ? "Sin descripción registrada." : tema.Descripcion));
            if (incluirInicio)
                datos.Add(new CampoCorreo("Fecha de inicio", Fecha(tema.FechaInicio)));
            datos.Add(new CampoCorreo("Fecha compromiso", Fecha(tema.FechaCompromiso)));
            if (incluirPrioridad)
                datos.Add(new CampoCorreo("Prioridad", tema.Prioridad));
            if (incluirEstatus)
                datos.Add(new CampoCorreo("Estatus", tema.Estatus));
            if (incluirAvance)
                datos.Add(new CampoCorreo("Avance", $"{tema.Avance}"));
            datos.Add(new CampoCorreo("Corresponsables", Corresponsables(tema.Corresponsables)));
            return datos;
        }

        private static string ConstruirCorreoAsignacionActividad(string nombreResponsable, GestorActividad actividad, string portalUrl)
        {
            return CorreoGestor(
                antetitulo: "Gestor de actividades · DGMESNIE",
                titulo: "Nueva actividad asignada",
                nombre: nombreResponsable,
                parrafo: "Se le asignó una actividad dentro del Gestor de Actividades de la DGMESNIE.",
                datos: DatosActividad(actividad, incluirCategoria: true, incluirEstatus: true, incluirResponsable: false),
                portalUrl: portalUrl,
                nota: "Este aviso se envía automáticamente cuando una actividad se asigna o cambia de responsable principal.");
        }

        private static string ConstruirCorreoAsignacionTema(string nombreResponsable, GestorTema tema, string portalUrl)
        {
            return CorreoGestor(
                antetitulo: "Gestor de actividades · DGMESNIE",
                titulo: "Nuevo tema asignado",
                nombre: nombreResponsable,
                parrafo: "Se le asignó un tema dentro del Gestor de Actividades de la DGMESNIE.",
                datos: DatosTema(tema, incluirInicio: true, incluirEstatus: true, incluirPrioridad: true, incluirAvance: false, responsableEtiqueta: null),
                portalUrl: portalUrl,
                nota: "Este aviso se envía automáticamente cuando un tema se asigna o cambia de responsable principal.");
        }

        private static string ConstruirCorreoRecordatorioTema(string nombreResponsable, GestorTema tema, string portalUrl)
        {
            return CorreoGestor(
                antetitulo: "Gestor de actividades · DGMESNIE",
                titulo: "Tema pendiente o por vencer",
                nombre: nombreResponsable,
                parrafo: "El siguiente tema sigue abierto y su fecha compromiso está próxima o vencida.",
                datos: DatosTema(tema, incluirInicio: false, incluirEstatus: true, incluirPrioridad: false, incluirAvance: true, responsableEtiqueta: null),
                portalUrl: portalUrl,
                nota: "Recordatorio automático del Gestor de Actividades.");
        }

        private static string ConstruirCorreoCompartirTema(string nombreDestinatario, GestorTema tema, string portalUrl)
        {
            return CorreoGestor(
                antetitulo: "Gestor de actividades · DGMESNIE",
                titulo: "Tema compartido",
                nombre: nombreDestinatario,
                parrafo: "Se le comparte el siguiente tema como copia de conocimiento.",
                datos: DatosTema(tema, incluirInicio: true, incluirEstatus: true, incluirPrioridad: true, incluirAvance: true, responsableEtiqueta: "Responsable"),
                portalUrl: portalUrl,
                nota: "Copia de conocimiento: no implica responsabilidad sobre el tema.");
        }

        private static string ConstruirCorreoCorresponsableActividad(string nombreUsuario, GestorActividad actividad, string portalUrl)
        {
            return CorreoGestor(
                antetitulo: "Gestor de actividades · DGMESNIE",
                titulo: "Asignación como corresponsable",
                nombre: nombreUsuario,
                parrafo: "Se le registró como corresponsable de la siguiente actividad.",
                datos: DatosActividad(actividad, incluirCategoria: true, incluirEstatus: false, incluirResponsable: true),
                portalUrl: portalUrl,
                nota: "Como corresponsable puede consultar y actualizar el avance de la actividad.");
        }

        private static string ConstruirCorreoCorresponsableTema(string nombreUsuario, GestorTema tema, string portalUrl)
        {
            return CorreoGestor(
                antetitulo: "Gestor de actividades · DGMESNIE",
                titulo: "Asignación como corresponsable",
                nombre: nombreUsuario,
                parrafo: "Se le registró como corresponsable del siguiente tema.",
                datos: DatosTema(tema, incluirInicio: false, incluirEstatus: false, incluirPrioridad: true, incluirAvance: false, responsableEtiqueta: "Responsable principal"),
                portalUrl: portalUrl,
                nota: "Como corresponsable puede consultar y actualizar el avance del tema.");
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
            return CorreoGestor(
                antetitulo: "Gestor de actividades · DGMESNIE",
                titulo: "Actividad compartida",
                nombre: nombreDestinatario,
                parrafo: "Se le comparte la siguiente actividad como copia de conocimiento.",
                datos: DatosActividad(actividad, incluirCategoria: false, incluirEstatus: true, incluirResponsable: true),
                portalUrl: portalUrl,
                nota: "Copia de conocimiento: no implica responsabilidad sobre la actividad.");
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
