using System.Data;
using Microsoft.AspNetCore.Mvc;
using Newtonsoft.Json;
using NSIE.Models;
using NSIE.Servicios;

namespace NSIE.Controllers
{
    [ServiceFilter(typeof(ValidacionInputFiltro))]
    [AutorizacionFiltro]
    [Route("InformePormenorizado/ProyectosIdentificados")]
    public class PamrntProyectosController : Controller
    {
        private readonly IPamrntProyectosIdentificadosService _service;
        private readonly IPamActualizacionService _actualizacionService;
        private readonly IPamAnalisisService _analisisService;
        private readonly IServicioEmailSMTP _emailService;
        private readonly ILogger<PamrntProyectosController> _logger;

        public PamrntProyectosController(
            IPamrntProyectosIdentificadosService service,
            IPamActualizacionService actualizacionService,
            IPamAnalisisService analisisService,
            IServicioEmailSMTP emailService,
            ILogger<PamrntProyectosController> logger)
        {
            _service = service;
            _actualizacionService = actualizacionService;
            _analisisService = analisisService;
            _emailService = emailService;
            _logger = logger;
        }

        [HttpGet("")]
        public async Task<IActionResult> Index([FromQuery] PamDashboardFiltro filtro)
        {
            var model = await _service.ObtenerProyectosAsync(filtro);
            model.Header = BuildHeader();
            model.PuedeActualizarFuentes = PuedeGestionarActualizaciones(ObtenerPerfilUsuario());
            return View(model);
        }

        [HttpGet("Detalle/{proyectoId:long}")]
        public async Task<IActionResult> Detalle(long proyectoId)
        {
            var model = await _service.ObtenerDetalleAsync(proyectoId);
            if (model == null) return NotFound();

            model.Header = BuildHeader();
            return View(model);
        }

        [HttpGet("Actualizacion/Nueva")]
        public async Task<IActionResult> NuevaActualizacion()
        {
            if (!PuedeGestionarActualizaciones(ObtenerPerfilUsuario()))
                return StatusCode(StatusCodes.Status403Forbidden);

            return View(new PamNuevaActualizacionViewModel
            {
                Header = BuildHeader(),
                Input = new PamNuevaActualizacionInput { FechaCorte = DateTime.Today },
                LotesRecientes = await _actualizacionService.ObtenerLotesRecientesAsync()
            });
        }

        [HttpPost("Actualizacion/Nueva")]
        [ValidateAntiForgeryToken]
        [RequestSizeLimit(268435456)]
        [RequestFormLimits(MultipartBodyLengthLimit = 268435456)]
        public async Task<IActionResult> NuevaActualizacion([Bind(Prefix = "Input")] PamNuevaActualizacionInput input)
        {
            var perfil = ObtenerPerfilUsuario();
            if (!PuedeGestionarActualizaciones(perfil))
                return StatusCode(StatusCodes.Status403Forbidden);

            if (!ModelState.IsValid)
            {
                return View(new PamNuevaActualizacionViewModel
                {
                    Header = BuildHeader(), Input = input,
                    LotesRecientes = await _actualizacionService.ObtenerLotesRecientesAsync()
                });
            }

            try
            {
                if (perfil == null || !int.TryParse(perfil.IdUsuario, out var usuarioId))
                    throw new InvalidOperationException("No fue posible identificar al usuario de la sesión. Vuelve a iniciar sesión antes de registrar fuentes.");

                var usuarioNombre = string.IsNullOrWhiteSpace(perfil.Nombre) ? $"Usuario {usuarioId}" : perfil.Nombre;
                var resultado = await _actualizacionService.CrearLoteAsync(input, usuarioId, usuarioNombre);
                TempData["PamActualizacionSuccess"] = $"Lote {resultado.LoteId} registrado con {resultado.TotalArchivos} archivo(s). Ningún proyecto fue modificado.";
                return RedirectToAction(nameof(ActualizacionDetalle), new { loteId = resultado.LoteId });
            }
            catch (InvalidOperationException ex)
            {
                _logger.LogWarning(ex, "El lote documental PAM no superó las validaciones de recepción.");
                ModelState.AddModelError(string.Empty, ex.Message);
                return View(new PamNuevaActualizacionViewModel
                {
                    Header = BuildHeader(), Input = input,
                    LotesRecientes = await _actualizacionService.ObtenerLotesRecientesAsync()
                });
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error al registrar un lote documental PAM.");
                ModelState.AddModelError(string.Empty, "No fue posible registrar la actualización. El incidente quedó registrado para revisión.");
                return View(new PamNuevaActualizacionViewModel
                {
                    Header = BuildHeader(), Input = input,
                    LotesRecientes = await _actualizacionService.ObtenerLotesRecientesAsync()
                });
            }
        }

        [HttpGet("Actualizacion/{loteId:long}")]
        public async Task<IActionResult> ActualizacionDetalle(
            long loteId,
            [FromQuery] PamAnalisisFiltro filtro,
            CancellationToken cancellationToken)
        {
            if (!PuedeGestionarActualizaciones(ObtenerPerfilUsuario()))
                return StatusCode(StatusCodes.Status403Forbidden);

            var model = await _analisisService.ObtenerDetalleAsync(loteId, filtro, cancellationToken);
            if (model == null) return NotFound();
            model.Header = BuildHeader();
            return View(model);
        }

        [HttpGet("Actualizacion/{loteId:long}/PrepararAplicacion")]
        public async Task<IActionResult> PrepararAplicacion(
            long loteId,
            CancellationToken cancellationToken)
        {
            if (!PuedeGestionarActualizaciones(ObtenerPerfilUsuario()))
                return StatusCode(StatusCodes.Status403Forbidden);

            var model = await _analisisService.ObtenerPreparacionAplicacionAsync(loteId, cancellationToken);
            if (model == null) return NotFound();
            model.Header = BuildHeader();
            return View(model);
        }

        [HttpPost("Actualizacion/{loteId:long}/PrepararAplicacion/Congelar")]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> CongelarPaqueteCierre(
            long loteId,
            PamCongelarPaqueteCierreInput input,
            CancellationToken cancellationToken)
        {
            var perfil = ObtenerPerfilUsuario();
            if (!PuedeGestionarActualizaciones(perfil))
                return StatusCode(StatusCodes.Status403Forbidden);

            if (!input.Confirmado)
                ModelState.AddModelError(nameof(input.Confirmado), "Debes confirmar que revisaste el resumen final.");

            if (!ModelState.IsValid)
            {
                TempData["PamPaqueteError"] = string.Join(" ", ModelState.Values
                    .SelectMany(valor => valor.Errors)
                    .Select(error => error.ErrorMessage)
                    .Where(mensaje => !string.IsNullOrWhiteSpace(mensaje)));
                return RedirectToAction(nameof(PrepararAplicacion), new { loteId });
            }

            try
            {
                if (!int.TryParse(perfil.IdUsuario, out var usuarioId))
                    throw new PamPaqueteCierreException("No fue posible identificar al usuario de la sesión.");

                var usuarioNombre = string.IsNullOrWhiteSpace(perfil.Nombre)
                    ? $"Usuario {usuarioId}"
                    : perfil.Nombre;
                var resultado = await _analisisService.CongelarPaqueteCierreAsync(
                    loteId, input, usuarioId, usuarioNombre, cancellationToken);

                TempData["PamPaqueteSuccess"] = resultado.Existente
                    ? $"El paquete {resultado.PaqueteUid.ToString()[..8]} ya estaba congelado con el mismo contenido."
                    : $"Paquete {resultado.PaqueteUid.ToString()[..8]} congelado con {resultado.TotalDetalles:N0} elementos. La cartera maestra no fue modificada.";
            }
            catch (PamPaqueteCierreException ex)
            {
                TempData["PamPaqueteError"] = ex.Message;
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "No fue posible congelar el paquete de cierre PAM para el lote {LoteId}.", loteId);
                TempData["PamPaqueteError"] = "No fue posible congelar el paquete. Intenta nuevamente o revisa el registro de aplicación.";
            }

            return RedirectToAction(nameof(PrepararAplicacion), new { loteId });
        }

        [HttpPost("Actualizacion/{loteId:long}/PrepararAplicacion/Aplicar")]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> AplicarPaquete(
            long loteId,
            PamAplicarPaqueteInput input,
            CancellationToken cancellationToken)
        {
            var perfil = ObtenerPerfilUsuario();
            if (!PuedeGestionarActualizaciones(perfil))
                return StatusCode(StatusCodes.Status403Forbidden);

            if (!input.Confirmado)
                ModelState.AddModelError(nameof(input.Confirmado), "Debes confirmar expresamente la aplicación definitiva.");

            if (!ModelState.IsValid)
            {
                TempData["PamAplicacionError"] = string.Join(" ", ModelState.Values
                    .SelectMany(valor => valor.Errors)
                    .Select(error => error.ErrorMessage)
                    .Where(mensaje => !string.IsNullOrWhiteSpace(mensaje)));
                return RedirectToAction(nameof(PrepararAplicacion), new { loteId });
            }

            try
            {
                if (!int.TryParse(perfil.IdUsuario, out var usuarioId))
                    throw new PamAplicacionException("No fue posible identificar al usuario de la sesión.");

                var usuarioNombre = string.IsNullOrWhiteSpace(perfil.Nombre)
                    ? $"Usuario {usuarioId}"
                    : perfil.Nombre;
                var resultado = await _analisisService.AplicarPaqueteAsync(
                    loteId, input, usuarioId, usuarioNombre, cancellationToken);

                TempData["PamAplicacionSuccess"] = resultado.Existente
                    ? $"El paquete {resultado.PaqueteUid.ToString()[..8]} ya se encontraba aplicado."
                    : $"Paquete {resultado.PaqueteUid.ToString()[..8]} aplicado: {resultado.TotalVersionesNuevas:N0} versiones, {resultado.TotalProyectosNuevos:N0} proyectos y {resultado.TotalClavesAlternas:N0} claves alternas.";
            }
            catch (PamAplicacionException ex)
            {
                TempData["PamAplicacionError"] = ex.Message;
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "No fue posible aplicar el paquete PAM del lote {LoteId}.", loteId);
                TempData["PamAplicacionError"] = "No fue posible aplicar el paquete. La transacción fue revertida íntegramente.";
            }

            return RedirectToAction(nameof(PrepararAplicacion), new { loteId });
        }

        [HttpPost("Actualizacion/{loteId:long}/Analizar")]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> SolicitarAnalisis(long loteId, CancellationToken cancellationToken)
        {
            var perfil = ObtenerPerfilUsuario();
            if (!PuedeGestionarActualizaciones(perfil))
                return StatusCode(StatusCodes.Status403Forbidden);

            try
            {
                if (!int.TryParse(perfil.IdUsuario, out var usuarioId))
                    throw new InvalidOperationException("No fue posible identificar al usuario de la sesión.");

                var usuarioNombre = string.IsNullOrWhiteSpace(perfil.Nombre) ? $"Usuario {usuarioId}" : perfil.Nombre;
                var analysisId = await _analisisService.SolicitarAnalisisAsync(
                    loteId, usuarioId, usuarioNombre, cancellationToken);
                TempData["PamAnalisisSuccess"] = $"Análisis {analysisId} encolado. La cartera no será modificada.";
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "No fue posible solicitar el análisis del lote PAM {LoteId}.", loteId);
                TempData["PamAnalisisError"] = ex is InvalidOperationException
                    ? ex.Message
                    : "No fue posible iniciar el análisis del lote.";
            }

            return RedirectToAction(nameof(ActualizacionDetalle), new { loteId });
        }

        [HttpGet("Actualizacion/{loteId:long}/Estado")]
        public async Task<IActionResult> EstadoAnalisis(long loteId, CancellationToken cancellationToken)
        {
            if (!PuedeGestionarActualizaciones(ObtenerPerfilUsuario()))
                return StatusCode(StatusCodes.Status403Forbidden);

            var status = await _analisisService.ObtenerEstadoAsync(loteId, cancellationToken);
            if (status == null) return NotFound();
            return Json(new
            {
                estado = status.Estado,
                totalFuentes = status.TotalFuentes,
                fuentesProcesadas = status.FuentesProcesadas,
                progresoPorcentaje = status.ProgresoPorcentaje,
                mensaje = status.Mensaje
            });
        }

        [HttpPost("Actualizacion/{loteId:long}/DecisionPreliminar")]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> GuardarDecisionPreliminar(
            long loteId,
            PamGuardarDecisionPreliminarInput input,
            CancellationToken cancellationToken)
        {
            var perfil = ObtenerPerfilUsuario();
            if (!PuedeGestionarActualizaciones(perfil))
                return StatusCode(StatusCodes.Status403Forbidden);

            var routeValues = new
            {
                loteId,
                pagina = Math.Clamp(input?.PaginaRetorno ?? 1, 1, 100000),
                categoria = NormalizarCategoriaRetorno(input?.CategoriaRetorno),
                busqueda = NormalizarBusquedaRetorno(input?.BusquedaRetorno),
                cambio = input?.CambioPropuestoId > 0 ? input.CambioPropuestoId : (long?)null,
                estadoRevision = "Pendiente",
                tipo = string.Equals(input?.TipoRetorno?.Trim(), "Alta", StringComparison.OrdinalIgnoreCase)
                    ? "Alta"
                    : null
            };

            if (!ModelState.IsValid || input == null)
            {
                var errores = ModelState
                    .Where(item => item.Value?.Errors.Count > 0)
                    .SelectMany(item => item.Value.Errors.Select(error => new
                    {
                        Campo = item.Key,
                        Mensaje = error.ErrorMessage
                    }))
                    .ToList();
                _logger.LogWarning(
                    "Decisión preliminar inválida para el lote {LoteId}: {Errores}",
                    loteId,
                    string.Join(" | ", errores.Select(error => $"{error.Campo}: {error.Mensaje}")));
                TempData["PamDecisionError"] = errores
                    .Select(error => error.Mensaje)
                    .FirstOrDefault(mensaje => !string.IsNullOrWhiteSpace(mensaje))
                    ?? "La decisión preliminar contiene datos incompletos o inválidos.";
                return RedirectToAction(nameof(ActualizacionDetalle), routeValues);
            }

            try
            {
                if (!int.TryParse(perfil.IdUsuario, out var usuarioId))
                    throw new PamDecisionPreliminarException("No fue posible identificar al usuario de la sesión.");

                var usuarioNombre = string.IsNullOrWhiteSpace(perfil.Nombre)
                    ? $"Usuario {usuarioId}"
                    : perfil.Nombre;
                await _analisisService.GuardarDecisionPreliminarAsync(
                    loteId, input, usuarioId, usuarioNombre, cancellationToken);

                TempData["PamDecisionSuccess"] =
                    "La clasificación quedó guardada y la revisión pasó a Revisadas. La cartera maestra no fue modificada.";

                return RedirectToAction(nameof(ActualizacionDetalle), new
                {
                    loteId,
                    pagina = 1,
                    routeValues.categoria,
                    routeValues.busqueda,
                    estadoRevision = "Pendiente",
                    routeValues.tipo
                });
            }
            catch (OperationCanceledException) when (cancellationToken.IsCancellationRequested)
            {
                throw;
            }
            catch (DBConcurrencyException ex)
            {
                _logger.LogWarning(ex,
                    "Conflicto de concurrencia en la decisión preliminar PAM del lote {LoteId}.", loteId);
                TempData["PamDecisionError"] =
                    "La decisión cambió mientras la revisabas. Recarga la pantalla antes de intentarlo nuevamente.";
            }
            catch (PamDecisionPreliminarException ex)
            {
                _logger.LogWarning(ex,
                    "No fue posible guardar la decisión preliminar PAM del lote {LoteId}.", loteId);
                TempData["PamDecisionError"] = ex.Message;
            }
            catch (ArgumentException ex)
            {
                _logger.LogWarning(ex,
                    "Datos inválidos en la decisión preliminar PAM del lote {LoteId}.", loteId);
                TempData["PamDecisionError"] =
                    "La decisión preliminar contiene datos incompletos o inválidos.";
            }
            catch (Exception ex)
            {
                _logger.LogError(ex,
                    "Error al guardar la decisión preliminar PAM del lote {LoteId}.", loteId);
                TempData["PamDecisionError"] =
                    "No fue posible guardar la decisión preliminar. Intenta nuevamente.";
            }

            return RedirectToAction(nameof(ActualizacionDetalle), routeValues);
        }

        [HttpPost("Actualizacion/{loteId:long}/Hallazgo/DecisionPreliminar")]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> GuardarDecisionHallazgo(
            long loteId,
            PamGuardarHallazgoPreliminarInput input,
            CancellationToken cancellationToken)
        {
            var perfil = ObtenerPerfilUsuario();
            if (!PuedeGestionarActualizaciones(perfil))
                return StatusCode(StatusCodes.Status403Forbidden);

            var routeValues = new
            {
                loteId,
                pagina = Math.Clamp(input?.PaginaRetorno ?? 1, 1, 100000),
                categoria = NormalizarCategoriaRetorno(input?.CategoriaRetorno),
                busqueda = NormalizarBusquedaRetorno(input?.BusquedaRetorno),
                estadoRevision = string.Equals(input?.EstadoRevisionRetorno, "Revisada", StringComparison.OrdinalIgnoreCase)
                    ? "Revisada"
                    : "Pendiente",
                tipo = string.Equals(input?.TipoRetorno?.Trim(), "Alta", StringComparison.OrdinalIgnoreCase)
                    ? "Alta"
                    : null
            };

            if (!ModelState.IsValid || input == null)
            {
                TempData["PamDecisionError"] = ModelState
                    .Where(item => item.Value?.Errors.Count > 0)
                    .SelectMany(item => item.Value.Errors)
                    .Select(error => error.ErrorMessage)
                    .FirstOrDefault(message => !string.IsNullOrWhiteSpace(message))
                    ?? "La decisión de la cancelación contiene datos incompletos o inválidos.";
                return RedirectToAction(nameof(ActualizacionDetalle), routeValues);
            }

            try
            {
                if (!int.TryParse(perfil.IdUsuario, out var usuarioId))
                    throw new PamDecisionPreliminarException("No fue posible identificar al usuario de la sesión.");

                var usuarioNombre = string.IsNullOrWhiteSpace(perfil.Nombre)
                    ? $"Usuario {usuarioId}"
                    : perfil.Nombre;
                await _analisisService.GuardarDecisionHallazgoAsync(
                    loteId, input, usuarioId, usuarioNombre, cancellationToken);

                TempData["PamDecisionSuccess"] =
                    "La cancelación quedó clasificada y salió de pendientes. La cartera maestra todavía no fue modificada.";
            }
            catch (OperationCanceledException) when (cancellationToken.IsCancellationRequested)
            {
                throw;
            }
            catch (DBConcurrencyException ex)
            {
                _logger.LogWarning(ex,
                    "Conflicto de concurrencia al clasificar el hallazgo PAM del lote {LoteId}.", loteId);
                TempData["PamDecisionError"] =
                    "La revisión cambió mientras la consultabas. Recarga la pantalla antes de intentarlo nuevamente.";
            }
            catch (PamDecisionPreliminarException ex)
            {
                _logger.LogWarning(ex,
                    "No fue posible clasificar el hallazgo PAM del lote {LoteId}.", loteId);
                TempData["PamDecisionError"] = ex.Message;
            }
            catch (Exception ex)
            {
                _logger.LogError(ex,
                    "Error al clasificar el hallazgo PAM del lote {LoteId}.", loteId);
                TempData["PamDecisionError"] =
                    "No fue posible guardar la decisión. Intenta nuevamente.";
            }

            return RedirectToAction(nameof(ActualizacionDetalle), routeValues);
        }

        [HttpPost("Actualizacion/{loteId:long}/DecisionesCampo")]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> GuardarDecisionesCampo(
            long loteId,
            PamGuardarDecisionesCampoInput input,
            CancellationToken cancellationToken)
        {
            var perfil = ObtenerPerfilUsuario();
            if (!PuedeGestionarActualizaciones(perfil))
                return StatusCode(StatusCodes.Status403Forbidden);

            var estadoRetorno = string.Equals(input?.EstadoRevisionRetorno, "Revisada", StringComparison.OrdinalIgnoreCase)
                ? "Revisada"
                : "Pendiente";
            var routeValues = new
            {
                loteId,
                pagina = Math.Clamp(input?.PaginaRetorno ?? 1, 1, 100000),
                categoria = NormalizarCategoriaRetorno(input?.CategoriaRetorno),
                busqueda = NormalizarBusquedaRetorno(input?.BusquedaRetorno),
                estadoRevision = estadoRetorno,
                tipo = string.Equals(input?.TipoRetorno?.Trim(), "Alta", StringComparison.OrdinalIgnoreCase)
                    ? "Alta"
                    : null
            };

            if (!ModelState.IsValid || input == null)
            {
                TempData["PamDecisionError"] = ModelState
                    .Where(item => item.Value?.Errors.Count > 0)
                    .SelectMany(item => item.Value.Errors)
                    .Select(error => error.ErrorMessage)
                    .FirstOrDefault(message => !string.IsNullOrWhiteSpace(message))
                    ?? "La revisión de campos contiene decisiones incompletas o inválidas.";
                return RedirectToAction(nameof(ActualizacionDetalle), routeValues);
            }

            try
            {
                if (!int.TryParse(perfil.IdUsuario, out var usuarioId))
                    throw new PamDecisionPreliminarException("No fue posible identificar al usuario de la sesión.");

                var usuarioNombre = string.IsNullOrWhiteSpace(perfil.Nombre)
                    ? $"Usuario {usuarioId}"
                    : perfil.Nombre;
                await _analisisService.GuardarDecisionesCampoAsync(
                    loteId, input, usuarioId, usuarioNombre, cancellationToken);

                TempData["PamDecisionSuccess"] =
                    $"Se guardaron {input.Cambios.Count:N0} decisiones de campo. El proyecto pasó a Revisadas y la cartera maestra no fue modificada.";

                return RedirectToAction(nameof(ActualizacionDetalle), new
                {
                    loteId,
                    pagina = estadoRetorno == "Revisada" ? routeValues.pagina : 1,
                    routeValues.categoria,
                    routeValues.busqueda,
                    estadoRevision = estadoRetorno,
                    routeValues.tipo
                });
            }
            catch (OperationCanceledException) when (cancellationToken.IsCancellationRequested)
            {
                throw;
            }
            catch (DBConcurrencyException ex)
            {
                _logger.LogWarning(ex,
                    "Conflicto de concurrencia al guardar decisiones de campo PAM del lote {LoteId}.", loteId);
                TempData["PamDecisionError"] =
                    "Otra persona actualizó este proyecto. Recarga la pantalla antes de guardar.";
            }
            catch (PamDecisionPreliminarException ex)
            {
                _logger.LogWarning(ex,
                    "No fue posible guardar decisiones de campo PAM del lote {LoteId}.", loteId);
                TempData["PamDecisionError"] = ex.Message;
            }
            catch (Exception ex)
            {
                _logger.LogError(ex,
                    "Error al guardar decisiones de campo PAM del lote {LoteId}.", loteId);
                TempData["PamDecisionError"] =
                    "No fue posible guardar la revisión de campos. Intenta nuevamente.";
            }

            return RedirectToAction(nameof(ActualizacionDetalle), routeValues);
        }

        [HttpPost("Actualizacion/{loteId:long}/DecisionPreliminar/Reabrir")]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> ReabrirDecisionPreliminar(
            long loteId,
            PamReabrirDecisionPreliminarInput input,
            CancellationToken cancellationToken)
        {
            var perfil = ObtenerPerfilUsuario();
            if (!PuedeGestionarActualizaciones(perfil))
                return StatusCode(StatusCodes.Status403Forbidden);

            var routeValues = new
            {
                loteId,
                pagina = Math.Clamp(input?.PaginaRetorno ?? 1, 1, 100000),
                categoria = NormalizarCategoriaRetorno(input?.CategoriaRetorno),
                busqueda = NormalizarBusquedaRetorno(input?.BusquedaRetorno),
                cambio = input?.CambioPropuestoId > 0 ? input.CambioPropuestoId : (long?)null,
                estadoRevision = "Revisada",
                tipo = string.Equals(input?.TipoRetorno?.Trim(), "Alta", StringComparison.OrdinalIgnoreCase)
                    ? "Alta"
                    : null
            };

            if (!ModelState.IsValid || input == null)
            {
                TempData["PamDecisionError"] = ModelState
                    .Where(item => item.Value?.Errors.Count > 0)
                    .SelectMany(item => item.Value.Errors)
                    .Select(error => error.ErrorMessage)
                    .FirstOrDefault(message => !string.IsNullOrWhiteSpace(message))
                    ?? "No fue posible identificar la revisión que se desea reabrir.";
                return RedirectToAction(nameof(ActualizacionDetalle), routeValues);
            }

            try
            {
                if (!int.TryParse(perfil.IdUsuario, out var usuarioId))
                    throw new PamDecisionPreliminarException("No fue posible identificar al usuario de la sesión.");

                var usuarioNombre = string.IsNullOrWhiteSpace(perfil.Nombre)
                    ? $"Usuario {usuarioId}"
                    : perfil.Nombre;
                await _analisisService.ReabrirDecisionPreliminarAsync(
                    loteId, input, usuarioId, usuarioNombre, cancellationToken);

                TempData["PamDecisionSuccess"] =
                    "La revisión se reabrió para corregirla. La decisión anterior permanece en el historial y la cartera maestra no fue modificada.";

                return RedirectToAction(nameof(ActualizacionDetalle), new
                {
                    loteId,
                    pagina = 1,
                    routeValues.categoria,
                    routeValues.busqueda,
                    cambio = input.CambioPropuestoId,
                    estadoRevision = "Pendiente",
                    routeValues.tipo
                });
            }
            catch (OperationCanceledException) when (cancellationToken.IsCancellationRequested)
            {
                throw;
            }
            catch (DBConcurrencyException ex)
            {
                _logger.LogWarning(ex,
                    "Conflicto al reabrir la revisión preliminar PAM del lote {LoteId}.", loteId);
                TempData["PamDecisionError"] =
                    "La revisión cambió mientras la consultabas. Recarga la pantalla antes de intentarlo nuevamente.";
            }
            catch (PamDecisionPreliminarException ex)
            {
                _logger.LogWarning(ex,
                    "No fue posible reabrir la revisión preliminar PAM del lote {LoteId}.", loteId);
                TempData["PamDecisionError"] = ex.Message;
            }
            catch (Exception ex)
            {
                _logger.LogError(ex,
                    "Error al reabrir la revisión preliminar PAM del lote {LoteId}.", loteId);
                TempData["PamDecisionError"] =
                    "No fue posible reabrir la revisión. Intenta nuevamente.";
            }

            return RedirectToAction(nameof(ActualizacionDetalle), routeValues);
        }

        [HttpGet("Ficha/{clavePem}")]
        public async Task<IActionResult> Ficha(string clavePem)
        {
            var model = await _service.ObtenerFichaAsync(clavePem);
            if (model == null)
            {
                return NotFound();
            }

            model.Header = BuildHeader();
            model.Destinatarios = await _service.ObtenerDestinatariosAsync();
            return View(model);
        }

        [HttpPost("Ficha/Enviar")]
        [ValidateAntiForgeryToken]
        [RequestSizeLimit(67108864)]
        [RequestFormLimits(MultipartBodyLengthLimit = 67108864)]
        public async Task<IActionResult> EnviarFicha([FromBody] PamEnviarFichaInput input)
        {
            if (input == null || string.IsNullOrWhiteSpace(input.ArchivoBase64) || input.UsuarioIds is not { Count: > 0 })
                return BadRequest(new { ok = false, mensaje = "Selecciona al menos un destinatario y espera a que el archivo se genere." });

            var formato = string.Equals(input.Formato, "pptx", StringComparison.OrdinalIgnoreCase) ? "pptx" : "pdf";
            var clave = string.IsNullOrWhiteSpace(input.ClavePem) ? "PAMRNT" : input.ClavePem.Trim();

            byte[] adjunto;
            try
            {
                var base64 = input.ArchivoBase64;
                var coma = base64.IndexOf(',');
                if (coma >= 0) base64 = base64[(coma + 1)..]; // quita el prefijo data:...;base64,
                adjunto = Convert.FromBase64String(base64);
            }
            catch
            {
                return BadRequest(new { ok = false, mensaje = "El archivo adjunto no se pudo procesar. Vuelve a generarlo." });
            }

            if (adjunto.Length == 0 || adjunto.Length > 25 * 1024 * 1024)
                return BadRequest(new { ok = false, mensaje = "El archivo está vacío o supera el límite de 25 MB." });

            var nombreArchivo = string.IsNullOrWhiteSpace(input.NombreArchivo)
                ? $"Ficha_{clave}.{formato}"
                : (input.NombreArchivo.EndsWith($".{formato}", StringComparison.OrdinalIgnoreCase) ? input.NombreArchivo : $"{input.NombreArchivo}.{formato}");

            var destinatarios = await _service.ObtenerDestinatariosAsync();
            var seleccionados = destinatarios.Where(d => input.UsuarioIds.Contains(d.IdUsuario)).ToList();
            if (seleccionados.Count == 0)
                return BadRequest(new { ok = false, mensaje = "Los destinatarios seleccionados no son válidos." });

            var perfil = ObtenerPerfilUsuario();
            var remitente = string.IsNullOrWhiteSpace(perfil?.Nombre) ? "el equipo de la DGMESNIE" : perfil.Nombre;
            // Cargo actualizado desde BD (el de la sesión puede ser previo a un cambio); fallback al perfil.
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
                    var cuerpo = ConstruirCorreoFicha(destino.Nombre, clave, formato, remitente, remitenteCargo, input.MensajeAdicional);
                    await _emailService.EnviarCorreo(destino.Correo,
                        $"Ficha ejecutiva del proyecto {clave} — PAM/PAMRNT",
                        cuerpo, adjunto, nombreArchivo);
                    enviados.Add(destino.Nombre);
                }
                catch (Exception ex)
                {
                    _logger.LogError(ex, "No fue posible enviar la ficha {Clave} a {Correo}.", clave, destino.Correo);
                    fallidos.Add(destino.Nombre);
                }
            }

            if (enviados.Count == 0)
                return StatusCode(500, new { ok = false, mensaje = "No fue posible enviar la ficha a ningún destinatario." });

            var mensaje = $"Ficha enviada a {enviados.Count} destinatario(s): {string.Join(", ", enviados)}.";
            if (fallidos.Count > 0) mensaje += $" No se pudo enviar a: {string.Join(", ", fallidos)}.";
            return Ok(new { ok = true, mensaje });
        }

        private static string ConstruirCorreoFicha(string nombreDestino, string clave, string formato, string remitente, string remitenteCargo, string mensajeAdicional)
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
    <div style=""color:#F5D9E2;font-size:12px;margin-top:4px"">Repositorio maestro PAM / PAMRNT</div>
  </div>
  <div style=""padding:26px 28px"">
    <p style=""margin:0 0 16px;color:#1c1b1a;font-size:15px"">{saludo}:</p>
    <p style=""margin:0 0 16px;color:#3a3a3a;font-size:14px;line-height:1.6"">
      {System.Net.WebUtility.HtmlEncode(remitente)} le comparte la <strong>ficha ejecutiva del proyecto de transmisión
      {System.Net.WebUtility.HtmlEncode(clave)}</strong> del Programa de Ampliación y Modernización de la Red Nacional de Transmisión (PAM/PAMRNT).
    </p>
    {extra}
    <p style=""margin:0 0 16px;color:#3a3a3a;font-size:14px;line-height:1.6"">
      Encontrará la ficha adjunta en formato <strong>{formato.ToUpperInvariant()}</strong>, con el diagnóstico, las metas físicas,
      el análisis de riesgos, la alineación de cronograma y la trazabilidad del proyecto, generada directamente desde la base vigente.
    </p>
    <div style=""margin:20px 0;padding:14px 18px;border-left:4px solid #E0A12E;background:#faf8f5;color:#5f5954;font-size:13px"">
      Documento informativo. Los datos provienen del Informe Pormenorizado vigente y pueden actualizarse en cada corte.
    </div>
    <p style=""margin:16px 0 0;color:#6F6B66;font-size:13px"">Atentamente,<br><strong>{System.Net.WebUtility.HtmlEncode(remitente)}</strong><br>{cargo}</p>
  </div>
  <div style=""background:#faf8f5;padding:14px 28px;border-top:1px solid #ece8e2;color:#9A958E;font-size:11px"">
    Correo generado automáticamente por la plataforma DGMESNIE. Por favor no responda a este mensaje.
  </div>
</div>";
        }

        private static HeaderViewModel BuildHeader()
        {
            var moduleInfo = new
            {
                title = "Dashboard maestro PAM/PAMRNT",
                description = "Cartera consolidada de proyectos vigentes y cancelados con origen, versión, fuente y fecha de corte.",
                stage = "Trazabilidad y actualización",
                functionality = "Consulta la vista SQL vigente y conserva las versiones históricas y fuentes documentales.",
                highlights = new[]
                {
                    "279 proyectos vigentes consolidados",
                    "Consulta directa del repositorio histórico SQL",
                    "Separación de PAM, PAMRNT, cancelados y cambios recientes"
                }
            };

            return new HeaderViewModel
            {
                Title = "Dashboard PAM/PAMRNT",
                IconPath = "proyecto.png",
                Description = "Cartera maestra, trazabilidad de fuentes y preparación de fichas ejecutivas.",
                Section = "Informe Pormenorizado",
                ModuleInfo = JsonConvert.SerializeObject(moduleInfo)
            };
        }

        private PerfilUsuario ObtenerPerfilUsuario()
        {
            var perfilJson = HttpContext.Session.GetString("PerfilUsuario");
            if (string.IsNullOrWhiteSpace(perfilJson)) return null;
            try { return JsonConvert.DeserializeObject<PerfilUsuario>(perfilJson); }
            catch { return null; }
        }

        private static bool PuedeGestionarActualizaciones(PerfilUsuario perfil)
        {
            if (perfil == null) return false;
            return perfil.IdUsuario == "1"
                || perfil.IdUsuario == "86"
                || string.Equals(perfil.Rol, "Administrador", StringComparison.OrdinalIgnoreCase)
                || string.Equals(perfil.Rol_Nombre, "Administrador", StringComparison.OrdinalIgnoreCase);
        }

        private static string NormalizarCategoriaRetorno(string categoria)
        {
            return categoria?.Trim().ToUpperInvariant() switch
            {
                "PRIORITARIO" => "Prioritario",
                "INFORMATIVO" => "Informativo",
                "COMPLETAR" => "Completar",
                "EQUIVALENTE" => "Equivalente",
                "OTROS" => "Otros",
                "TODOS" => "Todos",
                _ => "Prioritario"
            };
        }

        private static string NormalizarBusquedaRetorno(string busqueda)
        {
            if (string.IsNullOrWhiteSpace(busqueda)) return null;
            var valor = busqueda.Trim();
            return valor[..Math.Min(valor.Length, 200)];
        }
    }
}
