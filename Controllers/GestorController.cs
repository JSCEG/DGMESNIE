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
                var acts = await _repo.ObtenerActividadesAsync();
                return Json(acts);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error obteniendo actividades.");
                return StatusCode(500, new { error = "Error interno al obtener actividades." });
            }
        }

        [HttpGet("Gestor/Api/Actividades/{id:int}")]
        public async Task<IActionResult> ApiActividad(int id)
        {
            var act = await _repo.ObtenerActividadPorIdAsync(id);
            if (act == null) return NotFound();
            return Json(act);
        }

        [HttpPost("Gestor/Api/Actividades")]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> ApiCrearActividad([FromBody] GestorActividadForm form)
        {
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
                var temas = await _repo.ObtenerTemasAsync(actividadId);
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
                    await NotificarAsignacionTemaAsync(tema, form.ResponsableId);
                    if (form.CorresponsablesIds != null && form.CorresponsablesIds.Any())
                    {
                        await NotificarCorresponsablesTemaAsync(tema, form.CorresponsablesIds);
                    }
                    if (form.NotificarUsuariosIds != null && form.NotificarUsuariosIds.Any())
                    {
                        await NotificarCompartirTemaAsync(tema, form.NotificarUsuariosIds);
                    }
                }
                return CreatedAtAction(nameof(ApiTema), new { id }, tema);
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

            form.TemaId = id;
            if (!ModelState.IsValid)
                return BadRequest(ModelState);
            try
            {
                var temaAnterior = await _repo.ObtenerTemaPorIdAsync(id);
                await _repo.ActualizarTemaAsync(form, GetCurrentUserId());
                var tema = await _repo.ObtenerTemaPorIdAsync(id);
                var responsableCambioSolicitado = form.ResponsableId != temaAnterior?.ResponsableId;
                if (tema != null)
                {
                    if (responsableCambioSolicitado && form.ResponsableId.HasValue)
                        await NotificarAsignacionTemaAsync(tema, form.ResponsableId);

                    var nuevosCorresponsables = form.CorresponsablesIds.Except(temaAnterior?.Corresponsables.Select(c => c.IdUsuario) ?? Enumerable.Empty<int>()).ToList();
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
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error actualizando tema {Id}.", id);
                return StatusCode(500, new { error = "No fue posible actualizar el tema." });
            }
        }

        [HttpDelete("Gestor/Api/Temas/{id:int}")]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> ApiEliminarTema(int id)
        {
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

        // ── Helpers ───────────────────────────────────────────────────────────
        private int? GetCurrentUserId()
        {
            var claim = User.FindFirstValue(ClaimTypes.NameIdentifier);
            return int.TryParse(claim, out var id) ? id : null;
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
            var responsableId = tema.ResponsableId ?? responsableIdOverride;
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

                var asunto = $"Notificación de tema: {tema.Clave} - {tema.Tema}";
                var portalUrl = Url.Action("Index", "Gestor", null, protocol: HttpContext.Request.Scheme) ?? string.Empty;
                var cuerpo = ConstruirCorreoCompartirTema(usuario.Nombre, tema, portalUrl);

                try
                {
                    _logger.LogInformation("Enviando correo de notificación de tema {TemaId} a {Correo}.", tema.TemaId, correo);
                    await _servicioEmailSMTP.EnviarCorreo(correo, asunto, cuerpo);
                }
                catch (Exception ex)
                {
                    _logger.LogError(ex, "Error enviando notificación de tema {TemaId} a {Correo}.", tema.TemaId, correo);
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
                    <title>Compartir tema</title>
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
                        <div style='background:#8a0031; color:#ffffff; padding:16px 20px; font-size:20px; font-weight:700;'>Tema Compartido / Notificación</div>
                        <div style='padding:22px 20px;'>
                            <p style='margin:0 0 12px; font-size:18px; font-weight:700; color:#1f2937;'>Estimado(a) {nombreDestinatario},</p>
                            <p>Le compartimos los detalles del siguiente tema registrado en el Gestor de Actividades DGMESNIE:</p>
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
                            <div style='margin:18px 0 16px; text-align:center;'>
                                <a href='{portalUrl}' style='display:inline-block; padding:12px 20px; border-radius:8px; background:#8a0031; color:#ffffff; text-decoration:none; font-weight:700;'>
                                    Abrir Gestor de Actividades
                                </a>
                            </div>
                            <p style='margin:10px 0 0; font-size:13px; color:#555;'>Este aviso se envía automáticamente para compartir el estatus del tema.</p>
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
