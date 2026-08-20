using Microsoft.AspNetCore.Mvc;
using Newtonsoft.Json;
using NSIE.Models;
using NSIE.Models.PlaneacionVinculante;
using NSIE.Servicios;
using NSIE.Servicios.PlaneacionVinculante;

namespace NSIE.Controllers
{
    [ServiceFilter(typeof(ValidacionInputFiltro))]
    [AutorizacionFiltro]
    [Route("[controller]")]
    public sealed class PlaneacionVinculanteController : Controller
    {
        private const long MaximoArchivo = 25L * 1024 * 1024;
        private static readonly HashSet<string> ClasesPermitidas = new(StringComparer.OrdinalIgnoreCase)
        {
            "CORTE_BASE",
            "BORRADOR_REVISION",
            "PUBLICACION_OFICIAL",
            "ACTUALIZACION_OPERATIVA"
        };

        private readonly IPvircePreviewService _previewService;
        private readonly IPlaneacionVinculanteOverviewService _overviewService;
        private readonly IPamrntProyectosIdentificadosService _pamService;
        private readonly IServicioEmailSMTP _emailService;
        private readonly ILogger<PlaneacionVinculanteController> _logger;

        public PlaneacionVinculanteController(
            IPvircePreviewService previewService,
            IPlaneacionVinculanteOverviewService overviewService,
            IPamrntProyectosIdentificadosService pamService,
            IServicioEmailSMTP emailService,
            ILogger<PlaneacionVinculanteController> logger)
        {
            _previewService = previewService;
            _overviewService = overviewService;
            _pamService = pamService;
            _emailService = emailService;
            _logger = logger;
        }

        [HttpGet("")]
        [HttpGet("Index")]
        public async Task<IActionResult> Index(CancellationToken cancellationToken)
            => View(await BuildModelAsync(cancellationToken: cancellationToken));

        [HttpGet("Fuentes")]
        public IActionResult Fuentes()
            => View(BuildFuentesModel());

        [HttpGet("Ficha")]
        public async Task<IActionResult> Ficha(CancellationToken cancellationToken)
        {
            var model = new PlaneacionVinculanteFichaViewModel
            {
                Overview = await _overviewService.ObtenerAsync(cancellationToken)
            };

            try
            {
                model.Destinatarios = await _pamService.ObtenerDestinatariosAsync();
            }
            catch (Exception exception)
            {
                _logger.LogWarning(exception, "No fue posible cargar destinatarios para la ficha de Planeación Vinculante.");
            }

            return View(model);
        }

        [HttpPost("Ficha/Enviar")]
        [ValidateAntiForgeryToken]
        [RequestSizeLimit(67108864)]
        [RequestFormLimits(MultipartBodyLengthLimit = 67108864)]
        public async Task<IActionResult> EnviarFicha([FromBody] PamEnviarFichaInput input)
        {
            if (input == null || string.IsNullOrWhiteSpace(input.ArchivoBase64) || input.UsuarioIds is not { Count: > 0 })
            {
                return BadRequest(new { ok = false, mensaje = "Selecciona al menos un destinatario y espera a que la ficha termine de generarse." });
            }

            var formato = string.Equals(input.Formato, "pptx", StringComparison.OrdinalIgnoreCase) ? "pptx" : "pdf";
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

            if (adjunto.Length == 0 || adjunto.Length > 20 * 1024 * 1024)
            {
                return BadRequest(new { ok = false, mensaje = "La ficha está vacía o supera el límite seguro de 20 MB para correo." });
            }

            var nombreArchivo = string.IsNullOrWhiteSpace(input.NombreArchivo)
                ? $"Planeacion_Vinculante_SEN_{DateTime.Now:yyyy-MM-dd}.{formato}"
                : (input.NombreArchivo.EndsWith($".{formato}", StringComparison.OrdinalIgnoreCase)
                    ? input.NombreArchivo
                    : $"{input.NombreArchivo}.{formato}");

            var destinatarios = await _pamService.ObtenerDestinatariosAsync();
            var seleccionados = destinatarios.Where(destino => input.UsuarioIds.Contains(destino.IdUsuario)).ToList();
            if (seleccionados.Count == 0)
            {
                return BadRequest(new { ok = false, mensaje = "Los destinatarios seleccionados no son válidos." });
            }

            var perfilJson = HttpContext.Session.GetString("PerfilUsuario");
            var perfil = string.IsNullOrWhiteSpace(perfilJson)
                ? null
                : JsonConvert.DeserializeObject<PerfilUsuario>(perfilJson);
            var remitente = string.IsNullOrWhiteSpace(perfil?.Nombre) ? "el equipo de la DGMESNIE" : perfil.Nombre;
            var remitenteCargo = perfil?.Cargo;
            if (perfil != null && int.TryParse(perfil.IdUsuario, out var remitenteId))
            {
                remitenteCargo = destinatarios.FirstOrDefault(destino => destino.IdUsuario == remitenteId)?.Cargo ?? remitenteCargo;
            }

            var enviados = new List<string>();
            var fallidos = new List<string>();
            foreach (var destino in seleccionados)
            {
                try
                {
                    var cuerpo = ConstruirCorreoFicha(
                        destino.Nombre,
                        formato,
                        nombreArchivo,
                        remitente,
                        remitenteCargo,
                        input.MensajeAdicional);
                    await _emailService.EnviarCorreo(
                        destino.Correo,
                        "Ficha ejecutiva — Planeación Vinculante del SEN",
                        cuerpo,
                        adjunto,
                        nombreArchivo);
                    enviados.Add(destino.Nombre);
                }
                catch (Exception exception)
                {
                    _logger.LogError(exception, "No fue posible enviar la ficha de Planeación Vinculante a {Correo}.", destino.Correo);
                    fallidos.Add(destino.Nombre);
                }
            }

            if (enviados.Count == 0)
            {
                return StatusCode(StatusCodes.Status502BadGateway, new { ok = false, mensaje = "No fue posible enviar la ficha a ningún destinatario." });
            }

            var mensaje = $"Ficha enviada a {enviados.Count} destinatario(s): {string.Join(", ", enviados)}.";
            if (fallidos.Count > 0) mensaje += $" No se pudo enviar a: {string.Join(", ", fallidos)}.";
            return Ok(new { ok = true, mensaje });
        }

        [HttpPost("Previsualizar")]
        [ValidateAntiForgeryToken]
        [RequestSizeLimit(MaximoArchivo)]
        public async Task<IActionResult> Previsualizar(
            IFormFile? archivo,
            string? claseVersion,
            CancellationToken cancellationToken)
        {
            var claseNormalizada = NormalizarClase(claseVersion);
            var model = BuildFuentesModel(claseNormalizada);

            if (archivo == null || archivo.Length == 0)
            {
                model.ErrorMessage = "Selecciona un archivo PVIRCE en formato .xlsx.";
                return View(nameof(Fuentes), model);
            }

            if (archivo.Length > MaximoArchivo)
            {
                model.ErrorMessage = "El archivo supera el límite de 25 MB para previsualización.";
                return View(nameof(Fuentes), model);
            }

            try
            {
                await using var stream = archivo.OpenReadStream();
                model.Preview = await _previewService.PrevisualizarAsync(
                    stream,
                    archivo.FileName,
                    cancellationToken);
            }
            catch (OperationCanceledException) when (cancellationToken.IsCancellationRequested)
            {
                return new StatusCodeResult(StatusCodes.Status499ClientClosedRequest);
            }
            catch (InvalidOperationException exception)
            {
                model.ErrorMessage = exception.Message;
            }
            catch
            {
                model.ErrorMessage = "No fue posible leer el archivo. Verifica que sea un libro XLSX válido y vuelve a intentarlo.";
            }

            return View(nameof(Fuentes), model);
        }

        private static string NormalizarClase(string? claseVersion)
            => !string.IsNullOrWhiteSpace(claseVersion) && ClasesPermitidas.Contains(claseVersion)
                ? claseVersion.ToUpperInvariant()
                : "CORTE_BASE";

        private async Task<PlaneacionVinculanteViewModel> BuildModelAsync(
            CancellationToken cancellationToken = default)
            => new()
            {
                Header = BuildHeader(),
                Overview = await _overviewService.ObtenerAsync(cancellationToken)
            };

        private static PlaneacionFuentesViewModel BuildFuentesModel(string claseVersion = "CORTE_BASE")
            => new()
            {
                Header = BuildFuentesHeader(),
                ClaseVersion = claseVersion
            };

        private static HeaderViewModel BuildHeader()
        {
            var moduleInfo = new
            {
                title = "Planeación Vinculante del SEN",
                description = "Integra las fotografías anuales de PLADESE, PVIRCE, PROSENER y PAM con su seguimiento verificable.",
                stage = "Tablero ejecutivo en construcción",
                order = new { step = 1, description = "Resumen de planeación y trazabilidad" },
                functionality = "Integra la meta publicada, las fotografías PVIRCE, la instrumentación VUPE y las obras PAM que habilitan el cumplimiento.",
                highlights = new[]
                {
                    "Distinguir PVIRCE oficial y cortes de trabajo",
                    "Calcular hash y cifras de control",
                    "Detectar fórmulas rotas y diferencias de capacidad",
                    "Conservar cada fotografía sin sobrescribir las anteriores"
                },
                roles = new[]
                {
                    new { icon = "calendar2-check", text = "Seguimiento de la planeación anual" },
                    new { icon = "shield-check", text = "Control de fuente, versión y calidad" }
                },
                context = "La portada combina controles documentales validados con componentes operativos leídos de la BD. La carga de fuentes permanece separada y no reemplaza datos automáticamente."
            };

            return new HeaderViewModel
            {
                Title = "Planeación Vinculante del SEN",
                IconPath = "proyecto.png",
                Description = "Metas, proyectos, red, inversión y riesgos de cumplimiento.",
                Section = "Seguimiento de metas y proyectos",
                ModuleInfo = JsonConvert.SerializeObject(moduleInfo)
            };
        }

        private static HeaderViewModel BuildFuentesHeader()
        {
            var moduleInfo = new
            {
                title = "Fuentes y cargas de planeación",
                description = "Previsualiza cortes documentales antes de registrarlos como fotografías versionadas.",
                stage = "Calidad y versionado",
                order = new { step = 1, description = "Validar estructura, totales y trazabilidad" },
                functionality = "Lee el archivo en memoria, calcula su huella y expone incidencias sin modificar la base institucional.",
                highlights = new[]
                {
                    "Separar publicación oficial y cortes de trabajo",
                    "Calcular SHA-256 y cifras de control",
                    "Detectar fórmulas rotas y diferencias de capacidad",
                    "Conservar la fuente original antes de transformar"
                },
                roles = new[]
                {
                    new { icon = "file-earmark-spreadsheet", text = "Previsualización de fuentes XLSX" },
                    new { icon = "shield-check", text = "Sin escritura automática en BD" }
                },
                context = "Esta vista concentra exclusivamente la entrada y validación de fuentes. El seguimiento ejecutivo permanece en Planeación Vinculante."
            };

            return new HeaderViewModel
            {
                Title = "Fuentes y cargas de planeación",
                IconPath = "proyecto.png",
                Description = "Ingreso controlado, calidad y versionado de las fotografías del proceso.",
                Section = "Calidad y fuentes",
                ModuleInfo = JsonConvert.SerializeObject(moduleInfo)
            };
        }

        private static string ConstruirCorreoFicha(
            string nombreDestino,
            string formato,
            string nombreArchivo,
            string remitente,
            string? remitenteCargo,
            string? mensajeAdicional)
        {
            var parrafos = new List<string>
            {
                $"{System.Net.WebUtility.HtmlEncode(remitente)} le comparte la <strong>ficha ejecutiva de Planeación Vinculante del Sistema Eléctrico Nacional</strong> generada desde el corte disponible en la plataforma."
            };
            if (!string.IsNullOrWhiteSpace(mensajeAdicional))
            {
                parrafos.Add(System.Net.WebUtility.HtmlEncode(mensajeAdicional));
            }

            return PlantillaCorreoInstitucional.Construir(new ContenidoCorreo
            {
                Adscripcion = "Subsecretaría de Planeación y Transición Energética",
                Antetitulo = "Planeación Vinculante del SEN",
                Titulo = "Ficha ejecutiva de metas, cartera y red",
                Metadatos = new[]
                {
                    new CampoCorreo("Periodo", "Administración 2025–2030"),
                    new CampoCorreo("Edición", "PLADESE 2025–2039"),
                    new CampoCorreo("Generado", DateTime.Now.ToString("dd/MM/yyyy HH:mm"))
                },
                Saludo = string.IsNullOrWhiteSpace(nombreDestino) ? "Estimada(o)" : $"Estimada(o) {nombreDestino}",
                Parrafos = parrafos,
                Puntos = new[]
                {
                    new CampoCorreo("PROSENER", "Prevalencia, energías limpias, acceso eléctrico y pobreza energética."),
                    new CampoCorreo("PLADESE / PVIRCE", "Requerimiento oficial, cortes de trabajo y movimiento de la cartera."),
                    new CampoCorreo("PAM / VUPE", "Red habilitante, folios, inversión y vínculos pendientes de validar.")
                },
                SeccionTitulo = "Contenido de la ficha",
                AdjuntoNombre = nombreArchivo,
                AdjuntoFormato = formato,
                Nota = "Las cifras documentales y operativas conservan su fuente y estado de validación; una estimación no se presenta como resultado oficial.",
                Firmante = remitente,
                FirmanteCargo = remitenteCargo
            });
        }
    }
}
