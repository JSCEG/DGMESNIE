using Microsoft.AspNetCore.Mvc;
using NSIE.Models;
using NSIE.Models.Gestor;
using NSIE.Servicios;
using NSIE.Servicios.Interfaces;
using Newtonsoft.Json;
using System.Security.Claims;

namespace NSIE.Controllers
{
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

        // ── API: Temas ────────────────────────────────────────────────────────
        [HttpGet("Gestor/Api/Temas")]
        public async Task<IActionResult> ApiTemas()
        {
            try
            {
                var temas = await _repo.ObtenerTemasAsync();
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
                if (tema != null && form.ResponsablePrincipalId.HasValue)
                    await NotificarAsignacionTemaAsync(tema, form.ResponsablePrincipalId);
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
            form.TemaId = id;
            if (!ModelState.IsValid)
                return BadRequest(ModelState);
            try
            {
                var temaAnterior = await _repo.ObtenerTemaPorIdAsync(id);
                await _repo.ActualizarTemaAsync(form, GetCurrentUserId());
                var tema = await _repo.ObtenerTemaPorIdAsync(id);
                var responsableCambioSolicitado = form.ResponsablePrincipalId != temaAnterior?.ResponsablePrincipalId;
                if (tema != null && responsableCambioSolicitado && form.ResponsablePrincipalId.HasValue)
                    await NotificarAsignacionTemaAsync(tema, form.ResponsablePrincipalId);
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

        // ── API: Actividades ──────────────────────────────────────────────────
        [HttpGet("Gestor/Api/Actividades")]
        public async Task<IActionResult> ApiActividades([FromQuery] int? temaId = null)
        {
            try
            {
                var acts = await _repo.ObtenerActividadesAsync(temaId);
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
                    await NotificarAsignacionActividadAsync(act, form.ResponsableId);
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
            if (form == null)
                return BadRequest(new { error = "Payload de actividad vacio." });

            form.ActividadId = id;
            if (!ModelState.IsValid)
                return BadRequest(ModelState);
            try
            {
                var actividadAnterior = await _repo.ObtenerActividadPorIdAsync(id);
                await _repo.ActualizarActividadAsync(form, GetCurrentUserId());
                var act = await _repo.ObtenerActividadPorIdAsync(id);
                var responsableCambioSolicitado = form.ResponsableId != actividadAnterior?.ResponsableId;
                if (act != null && responsableCambioSolicitado && form.ResponsableId.HasValue)
                    await NotificarAsignacionActividadAsync(act, form.ResponsableId);
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

        [HttpPost("Gestor/Api/Actividades/{id:int}/Recordatorio")]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> ApiEnviarRecordatorio(int id)
        {
            try
            {
                var act = await _repo.ObtenerActividadPorIdAsync(id);
                if (act == null)
                    return NotFound(new { error = "No se encontró la actividad especificada." });

                if (!act.ResponsableId.HasValue)
                    return BadRequest(new { error = "La actividad no tiene un responsable asignado." });

                var responsable = await _repo.ObtenerUsuarioVigentePorIdAsync(act.ResponsableId.Value);
                if (responsable == null)
                    return BadRequest(new { error = "No se encontró el responsable de la actividad." });

                var correoResponsable = responsable.Correo?.Trim();
                if (string.IsNullOrWhiteSpace(correoResponsable))
                    return BadRequest(new { error = "El responsable no tiene correo electrónico registrado." });

                var asunto = $"🔔 RECORDATORIO: Actividad pendiente o por vencer - {act.Clave}";
                var portalUrl = Url.Action("Index", "Gestor", null, protocol: HttpContext.Request.Scheme) ?? string.Empty;
                var cuerpo = ConstruirCorreoRecordatorioActividad(responsable.Nombre, act, portalUrl);

                _logger.LogInformation("Enviando correo de recordatorio para la actividad {ActividadId} a {Correo}.", id, correoResponsable);
                await _servicioEmailSMTP.EnviarCorreo(correoResponsable, asunto, cuerpo);

                return Ok(new { success = true, mensaje = "Recordatorio enviado con éxito." });
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error enviando recordatorio para actividad {Id}.", id);
                return StatusCode(500, new { error = "No fue posible enviar el recordatorio." });
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
            var responsableId = actividad.ResponsableId ?? responsableIdOverride;
            if (!responsableId.HasValue)
            {
                _logger.LogInformation(
                    "Actividad {ActividadId} sin ResponsableId. No se envia correo.",
                    actividad.ActividadId);
                return;
            }

            var responsable = await _repo.ObtenerUsuarioVigentePorIdAsync(responsableId.Value);
            if (responsable == null)
            {
                _logger.LogWarning(
                    "No se encontro usuario vigente para ResponsableId {ResponsableId} en actividad {ActividadId}.",
                    responsableId.Value,
                    actividad.ActividadId);
                return;
            }

            var correoResponsable = responsable.Correo?.Trim();
            if (string.IsNullOrWhiteSpace(correoResponsable))
            {
                _logger.LogWarning(
                    "El responsable {ResponsableId} de la actividad {ActividadId} no tiene correo registrado.",
                    responsableId.Value,
                    actividad.ActividadId);
                return;
            }

            var asunto = $"Nueva asignacion de actividad: {actividad.Clave}";
            var portalUrl = Url.Action("Index", "Gestor", null, protocol: HttpContext.Request.Scheme) ?? string.Empty;
            var cuerpo = ConstruirCorreoAsignacionActividad(responsable.Nombre, actividad, portalUrl);

            try
            {
                _logger.LogInformation(
                    "Enviando correo de asignacion de actividad {ActividadId} a responsable {ResponsableId} ({Correo}).",
                    actividad.ActividadId,
                    responsableId.Value,
                    correoResponsable);

                await _servicioEmailSMTP.EnviarCorreo(correoResponsable, asunto, cuerpo);
            }
            catch (Exception ex)
            {
                _logger.LogWarning(ex,
                    "La actividad {ActividadId} se guardo, pero no fue posible enviar correo al responsable {ResponsableId}.",
                    actividad.ActividadId,
                    responsableId.Value);
            }
        }

        private async Task NotificarAsignacionTemaAsync(GestorTema tema, int? responsableIdOverride = null)
        {
            var responsableId = tema.ResponsablePrincipalId ?? responsableIdOverride;
            if (!responsableId.HasValue)
            {
                _logger.LogInformation(
                    "Tema {TemaId} sin ResponsablePrincipalId. No se envia correo.",
                    tema.TemaId);
                return;
            }

            var responsable = await _repo.ObtenerUsuarioVigentePorIdAsync(responsableId.Value);
            if (responsable == null)
            {
                _logger.LogWarning(
                    "No se encontro usuario vigente para ResponsablePrincipalId {ResponsableId} en tema {TemaId}.",
                    responsableId.Value,
                    tema.TemaId);
                return;
            }

            var correoResponsable = responsable.Correo?.Trim();
            if (string.IsNullOrWhiteSpace(correoResponsable))
            {
                _logger.LogWarning(
                    "El responsable {ResponsableId} del tema {TemaId} no tiene correo registrado.",
                    responsableId.Value,
                    tema.TemaId);
                return;
            }

            var asunto = $"Nueva asignacion de tema: {tema.Clave}";
            var portalUrl = Url.Action("Index", "Gestor", null, protocol: HttpContext.Request.Scheme) ?? string.Empty;
            var cuerpo = ConstruirCorreoAsignacionTema(responsable.Nombre, tema, portalUrl);

            try
            {
                _logger.LogInformation(
                    "Enviando correo de asignacion de tema {TemaId} a responsable {ResponsableId} ({Correo}).",
                    tema.TemaId,
                    responsableId.Value,
                    correoResponsable);

                await _servicioEmailSMTP.EnviarCorreo(correoResponsable, asunto, cuerpo);
            }
            catch (Exception ex)
            {
                _logger.LogWarning(ex,
                    "El tema {TemaId} se guardo, pero no fue posible enviar correo al responsable {ResponsableId}.",
                    tema.TemaId,
                    responsableId.Value);
            }
        }

        private static string ConstruirCorreoAsignacionTema(string nombreResponsable, GestorTema tema, string portalUrl)
        {
            var fechaCompromiso = tema.FechaCompromiso?.ToString("dd/MM/yyyy") ?? "Sin fecha definida";
            var fechaInicio = tema.FechaInicio?.ToString("dd/MM/yyyy") ?? "Sin fecha definida";
            var descripcion = string.IsNullOrWhiteSpace(tema.Descripcion)
                ? "Sin descripcion registrada."
                : tema.Descripcion;

            return $@"
                <html lang='es'>
                <head>
                    <meta charset='UTF-8'>
                    <title>Asignacion de tema</title>
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
                                <tr><td style='padding:10px 12px; border:1px solid #e5c7d4; background:#f7ecf1; color:#6b1034; font-weight:700;'>Tema</td><td style='padding:10px 12px; border:1px solid #eadde4;'>{tema.Tema}</td></tr>
                                <tr><td style='padding:10px 12px; border:1px solid #e5c7d4; background:#f7ecf1; color:#6b1034; font-weight:700;'>Descripcion</td><td style='padding:10px 12px; border:1px solid #eadde4;'>{descripcion}</td></tr>
                                <tr><td style='padding:10px 12px; border:1px solid #e5c7d4; background:#f7ecf1; color:#6b1034; font-weight:700;'>Categoria</td><td style='padding:10px 12px; border:1px solid #eadde4;'>{tema.Categoria ?? "Sin categoria"}</td></tr>
                                <tr><td style='padding:10px 12px; border:1px solid #e5c7d4; background:#f7ecf1; color:#6b1034; font-weight:700;'>Fecha de inicio</td><td style='padding:10px 12px; border:1px solid #eadde4;'>{fechaInicio}</td></tr>
                                <tr><td style='padding:10px 12px; border:1px solid #e5c7d4; background:#f7ecf1; color:#6b1034; font-weight:700;'>Fecha compromiso</td><td style='padding:10px 12px; border:1px solid #eadde4;'>{fechaCompromiso}</td></tr>
                                <tr><td style='padding:10px 12px; border:1px solid #e5c7d4; background:#f7ecf1; color:#6b1034; font-weight:700;'>Prioridad</td><td style='padding:10px 12px; border:1px solid #eadde4;'>{tema.Prioridad}</td></tr>
                                <tr><td style='padding:10px 12px; border:1px solid #e5c7d4; background:#f7ecf1; color:#6b1034; font-weight:700;'>Estatus</td><td style='padding:10px 12px; border:1px solid #eadde4;'>{tema.Estatus}</td></tr>
                            </table>
                            <div style='margin:18px 0 16px; text-align:center;'>
                                <a href='{portalUrl}' style='display:inline-block; padding:12px 20px; border-radius:8px; background:#8a0031; color:#ffffff; text-decoration:none; font-weight:700;'>
                                    Abrir Gestor de Actividades
                                </a>
                            </div>
                            <p style='margin:10px 0 0; font-size:13px; color:#555;'>Este aviso se envia automaticamente cuando un tema se asigna o cambia de responsable principal.</p>
                        </div>
                    </div>
                </body>
                </html>";
        }

        private static string ConstruirCorreoAsignacionActividad(string nombreResponsable, GestorActividad actividad, string portalUrl)
        {
            var fechaCompromiso = actividad.FechaCompromiso?.ToString("dd/MM/yyyy") ?? "Sin fecha definida";
            var fechaInicio = actividad.FechaInicio?.ToString("dd/MM/yyyy") ?? "Sin fecha definida";
            var descripcion = string.IsNullOrWhiteSpace(actividad.Descripcion)
                ? "Sin descripcion registrada."
                : actividad.Descripcion;

            return $@"
                <html lang='es'>
                <head>
                    <meta charset='UTF-8'>
                    <title>Asignacion de actividad</title>
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
                                <tr><td style='padding:10px 12px; border:1px solid #e5c7d4; background:#f7ecf1; color:#6b1034; font-weight:700;'>Tema</td><td style='padding:10px 12px; border:1px solid #eadde4;'>{actividad.TemaNombre ?? "Sin tema"}</td></tr>
                                <tr><td style='padding:10px 12px; border:1px solid #e5c7d4; background:#f7ecf1; color:#6b1034; font-weight:700;'>Actividad</td><td style='padding:10px 12px; border:1px solid #eadde4;'>{actividad.Actividad}</td></tr>
                                <tr><td style='padding:10px 12px; border:1px solid #e5c7d4; background:#f7ecf1; color:#6b1034; font-weight:700;'>Descripcion</td><td style='padding:10px 12px; border:1px solid #eadde4;'>{descripcion}</td></tr>
                                <tr><td style='padding:10px 12px; border:1px solid #e5c7d4; background:#f7ecf1; color:#6b1034; font-weight:700;'>Fecha de inicio</td><td style='padding:10px 12px; border:1px solid #eadde4;'>{fechaInicio}</td></tr>
                                <tr><td style='padding:10px 12px; border:1px solid #e5c7d4; background:#f7ecf1; color:#6b1034; font-weight:700;'>Fecha compromiso</td><td style='padding:10px 12px; border:1px solid #eadde4;'>{fechaCompromiso}</td></tr>
                                <tr><td style='padding:10px 12px; border:1px solid #e5c7d4; background:#f7ecf1; color:#6b1034; font-weight:700;'>Prioridad</td><td style='padding:10px 12px; border:1px solid #eadde4;'>{actividad.Prioridad}</td></tr>
                                <tr><td style='padding:10px 12px; border:1px solid #e5c7d4; background:#f7ecf1; color:#6b1034; font-weight:700;'>Estatus</td><td style='padding:10px 12px; border:1px solid #eadde4;'>{actividad.Estatus}</td></tr>
                            </table>
                            <div style='margin:18px 0 16px; text-align:center;'>
                                <a href='{portalUrl}' style='display:inline-block; padding:12px 20px; border-radius:8px; background:#8a0031; color:#ffffff; text-decoration:none; font-weight:700;'>
                                    Abrir Gestor de Actividades
                                </a>
                            </div>
                            <p style='margin:10px 0 0; font-size:13px; color:#555;'>Este aviso se envia automaticamente cuando una actividad se asigna o cambia de responsable.</p>
                        </div>
                    </div>
                </body>
                </html>";
        }

        private static string ConstruirCorreoRecordatorioActividad(string nombreResponsable, GestorActividad actividad, string portalUrl)
        {
            var fechaCompromiso = actividad.FechaCompromiso?.ToString("dd/MM/yyyy") ?? "Sin fecha definida";
            var fechaInicio = actividad.FechaInicio?.ToString("dd/MM/yyyy") ?? "Sin fecha definida";
            var descripcion = string.IsNullOrWhiteSpace(actividad.Descripcion)
                ? "Sin descripción registrada."
                : actividad.Descripcion;

            return $@"
                <html lang='es'>
                <head>
                    <meta charset='UTF-8'>
                    <title>Recordatorio de actividad</title>
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
                        <div style='background:#8a0031; color:#ffffff; padding:16px 20px; font-size:20px; font-weight:700;'>Recordatorio de Actividad Pendiente / Por Vencer</div>
                        <div style='padding:22px 20px;'>
                            <p style='margin:0 0 12px; font-size:18px; font-weight:700; color:#1f2937;'>Estimado(a) {nombreResponsable},</p>
                            <p>Le enviamos este recordatorio sobre una actividad asignada a su cargo en el Gestor de Actividades DGMESNIE que requiere de su atención:</p>
                            <table role='presentation' cellpadding='0' cellspacing='0' border='0' style='width:100%; border-collapse:collapse; margin:20px 0;'>
                                <tr><td style='padding:10px 12px; border:1px solid #e5c7d4; background:#f7ecf1; color:#6b1034; font-weight:700; width:32%;'>Clave</td><td style='padding:10px 12px; border:1px solid #eadde4;'>{actividad.Clave}</td></tr>
                                <tr><td style='padding:10px 12px; border:1px solid #e5c7d4; background:#f7ecf1; color:#6b1034; font-weight:700;'>Tema</td><td style='padding:10px 12px; border:1px solid #eadde4;'>{actividad.TemaNombre ?? "Sin tema"}</td></tr>
                                <tr><td style='padding:10px 12px; border:1px solid #e5c7d4; background:#f7ecf1; color:#6b1034; font-weight:700;'>Actividad</td><td style='padding:10px 12px; border:1px solid #eadde4;'>{actividad.Actividad}</td></tr>
                                <tr><td style='padding:10px 12px; border:1px solid #e5c7d4; background:#f7ecf1; color:#6b1034; font-weight:700;'>Descripción</td><td style='padding:10px 12px; border:1px solid #eadde4;'>{descripcion}</td></tr>
                                <tr><td style='padding:10px 12px; border:1px solid #e5c7d4; background:#f7ecf1; color:#6b1034; font-weight:700;'>Fecha compromiso</td><td style='padding:10px 12px; border:1px solid #eadde4; font-weight: 700; color: #8a0031;'>{fechaCompromiso}</td></tr>
                                <tr><td style='padding:10px 12px; border:1px solid #e5c7d4; background:#f7ecf1; color:#6b1034; font-weight:700;'>Estatus</td><td style='padding:10px 12px; border:1px solid #eadde4;'>{actividad.Estatus}</td></tr>
                                <tr><td style='padding:10px 12px; border:1px solid #e5c7d4; background:#f7ecf1; color:#6b1034; font-weight:700;'>Avance</td><td style='padding:10px 12px; border:1px solid #eadde4;'>{actividad.Avance}%</td></tr>
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

        private HeaderViewModel BuildHeader() => new()
        {
            Title = "Gestor de Actividades",
            IconPath = "proyecto.png",
            Description = "Control de temas y actividades de la Dirección General con semáforo, gantt, kanban y reportes institucionales.",
            Section = "Seguimiento de actividades",
            ModuleInfo = JsonConvert.SerializeObject(new
            {
                title = "Gestor de Actividades DGMESNIE",
                description = "Módulo para planear, dar seguimiento y reportar el avance de temas y actividades institucionales.",
                functionality = "Tablero ejecutivo con 9 vistas: dashboard, temas, kanban, tabla, gantt, calendario, responsables, alertas y reportes.",
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
                order = new { step = 1, description = "Seguimiento de actividades y compromisos" },
                context = "Todas las actividades se vinculan a los usuarios registrados en la plataforma NSIE.",
                manualUrl = string.Empty
            })
        };
    }
}
