using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Http;
using Microsoft.Data.SqlClient;
using Newtonsoft.Json;
using System.Globalization;
using NSIE.Models;
using NSIE.Models.ProyectosPrivados;
using NSIE.Servicios;
using NSIE.Servicios.Interfaces;

namespace NSIE.Controllers
{
    [Authorize]
    public class DashboardProyectosController : Controller
    {
        private readonly IPamrntProyectosIdentificadosService _pamService;
        private readonly IServicioEmailSMTP _emailService;
        private readonly IInegiTerritorialService _inegiService;
        private readonly ICneFuelPriceService _cneFuelPriceService;
        private readonly IRepositorioTarifas _tarifasRepository;
        private readonly IServicioPermisosEnergeticos _permisosService;
        private readonly IPoliticaAccesoPermisosEnergeticos _permisosAccessPolicy;
        private readonly IRepositorioProyectosPrivados _proyectosPrivadosRepository;
        private readonly ILogger<DashboardProyectosController> _logger;

        [ActivatorUtilitiesConstructor]
        public DashboardProyectosController(
            IPamrntProyectosIdentificadosService pamService,
            IServicioEmailSMTP emailService,
            IInegiTerritorialService inegiService,
            ICneFuelPriceService cneFuelPriceService,
            IRepositorioTarifas tarifasRepository,
            IServicioPermisosEnergeticos permisosService,
            IPoliticaAccesoPermisosEnergeticos permisosAccessPolicy,
            IRepositorioProyectosPrivados proyectosPrivadosRepository,
            ILogger<DashboardProyectosController> logger)
        {
            _pamService = pamService;
            _emailService = emailService;
            _inegiService = inegiService;
            _cneFuelPriceService = cneFuelPriceService;
            _tarifasRepository = tarifasRepository;
            _permisosService = permisosService;
            _permisosAccessPolicy = permisosAccessPolicy;
            _proyectosPrivadosRepository = proyectosPrivadosRepository;
            _logger = logger;
        }

        [HttpGet("DashboardProyectos/PreciosCombustibles")]
        public async Task<IActionResult> PreciosCombustibles(
            [FromQuery] double minLat,
            [FromQuery] double minLon,
            [FromQuery] double maxLat,
            [FromQuery] double maxLon,
            CancellationToken cancellationToken)
        {
            var validBounds =
                double.IsFinite(minLat) &&
                double.IsFinite(minLon) &&
                double.IsFinite(maxLat) &&
                double.IsFinite(maxLon) &&
                minLat >= 14 &&
                maxLat <= 33.5 &&
                minLon >= -118 &&
                maxLon <= -86 &&
                minLat < maxLat &&
                minLon < maxLon;
            if (!validBounds)
            {
                return BadRequest(new
                {
                    ok = false,
                    code = "INVALID_FUEL_PRICE_BOUNDS",
                    mensaje = "La cobertura solicitada para precios de combustibles no es válida."
                });
            }

            try
            {
                var data = await _cneFuelPriceService.GetAsync(
                    minLat,
                    minLon,
                    maxLat,
                    maxLon,
                    cancellationToken);
                return Json(new { ok = true, data });
            }
            catch (Exception ex) when (
                ex is HttpRequestException or
                TaskCanceledException or
                System.Xml.XmlException or
                InvalidOperationException)
            {
                _logger.LogWarning(
                    ex,
                    "No fue posible consultar precios CNE para la cobertura {MinLat},{MinLon},{MaxLat},{MaxLon}.",
                    minLat,
                    minLon,
                    maxLat,
                    maxLon);
                return StatusCode(StatusCodes.Status502BadGateway, new
                {
                    ok = false,
                    code = "CNE_FUEL_PRICES_UNAVAILABLE",
                    mensaje = "La CNE no devolvió precios de combustibles en este momento."
                });
            }
        }

        public async Task<IActionResult> Index()
        {
            var perfilJson = HttpContext.Session.GetString("PerfilUsuario");
            if (!string.IsNullOrEmpty(perfilJson))
            {
                var perfilUsuario = JsonConvert.DeserializeObject<PerfilUsuario>(perfilJson);
                ViewData["NombreUsuario"] = perfilUsuario.Nombre;
                ViewData["RolUsuario"] = perfilUsuario.Rol;
            }
            else
            {
                ViewData["NombreUsuario"] = "Invitado";
                ViewData["RolUsuario"] = "Público";
            }

            try
            {
                var destinatarios = await _pamService.ObtenerDestinatariosAsync();
                ViewData["ReportDestinatariosJson"] = JsonConvert.SerializeObject(destinatarios ?? new List<PamUsuarioDestinatario>());
            }
            catch (Exception ex)
            {
                _logger.LogWarning(ex, "No fue posible cargar destinatarios para el envío del reporte territorial.");
                ViewData["ReportDestinatariosJson"] = "[]";
            }

            return View();
        }

        [HttpGet("DashboardProyectos/Convocatorias/Dataset")]
        [ResponseCache(NoStore = true, Location = ResponseCacheLocation.None)]
        public async Task<IActionResult> ConvocatoriasDataset()
        {
            try
            {
                var cartera = await _proyectosPrivadosRepository
                    .ObtenerCarteraConvocatoriaAsync();

                var rows = cartera.Projects
                    .Where(project => project.Source == "Mixtos II" && project.Consideration == "firme")
                    .Select(project => new
                {
                    folio = project.Folio,
                    nombre_proyecto = project.Name,
                    convocatoria = project.Type == "Particular 2"
                        ? "Segunda convocatoria · particulares"
                        : "Convocatoria de proyectos estratégicos",
                    gerencia = project.Region,
                    entidad = project.State,
                    municipio = project.Municipality,
                    tecnologia = project.Technology,
                    promovente = project.Company,
                    grupo_interes = project.InterestGroup,
                    subestacion = project.Substation,
                    punto_interconexion = project.InterconnectionPoint,
                    latitud = project.Latitude,
                    longitud = project.Longitude,
                    latitud_subestacion = project.SubstationLatitude,
                    longitud_subestacion = project.SubstationLongitude,
                    capacidad_mw = project.Mw,
                    orden_prelacion = project.Rank,
                    prioridad = project.Priority,
                    estatus = project.Decision switch
                    {
                        "continua" => "Continúa",
                        "no-continua" => "No continúa",
                        _ => "En revisión"
                    },
                    decision_clave = project.Decision,
                    consideracion = project.Consideration,
                    estatus_universo = project.UniverseStatus,
                    clasificacion_analisis = project.AnalysisClassification,
                    viabilidad_tecnica = project.TechnicalViability,
                    grupo_duplicado = project.DuplicateGroup,
                    costo_red_mdd = project.NetworkCostUsd,
                    inversion_mdp = project.NetworkCostMxn,
                    numero_obras = project.WorksCount,
                    obras = project.WorksDescription,
                    kml_proyecto = string.IsNullOrWhiteSpace(project.ProjectKmlUrl)
                        ? null
                        : $"/ProyectosPrivados/Api/CarteraConvocatoria/Kml/{Uri.EscapeDataString(project.Folio)}/proyecto",
                    kml_subestacion = string.IsNullOrWhiteSpace(project.SubstationKmlUrl)
                        ? null
                        : $"/ProyectosPrivados/Api/CarteraConvocatoria/Kml/{Uri.EscapeDataString(project.Folio)}/subestacion",
                    same_kml_reference = project.SameKmlReference,
                    fuente = project.Source ?? "Cartera institucional DGMESNIE",
                    fecha_actualizacion = project.UpdatedAt
                });

                return Json(new
                {
                    name = "Cartera institucional de convocatorias",
                    sourceVersion = cartera.SourceVersion,
                    source = cartera.Source,
                    latestImport = cartera.LatestImport,
                    rows
                });
            }
            catch (SqlException exception)
            {
                _logger.LogWarning(
                    exception,
                    "No fue posible proyectar la cartera de convocatorias en el dashboard territorial.");
                return StatusCode(StatusCodes.Status503ServiceUnavailable, new
                {
                    error = "La cartera institucional de convocatorias no está disponible en este momento."
                });
            }
        }

        private static bool MarcaIgual(string? a, string? b) =>
            !string.IsNullOrWhiteSpace(a) && !string.IsNullOrWhiteSpace(b) && string.Equals(a.Trim(), b.Trim(), StringComparison.OrdinalIgnoreCase);

        // Integrantes de un clúster o grupo excluyente (marcas CFE) cruzados con la cartera vigente, ordenados por capacidad.
        private static List<CarteraConvocatoriaGrupoMiembro> GrupoConvocatoria(CarteraConvocatoriaDatos cartera, List<CarteraConvocatoriaMarca> marks, Func<CarteraConvocatoriaMarca, bool> filtro, string folioActual)
        {
            var byFolio = cartera.Projects.Where(item => item.Source == "Mixtos II")
                .GroupBy(item => item.Folio, StringComparer.OrdinalIgnoreCase)
                .ToDictionary(group => group.Key, group => group.First(), StringComparer.OrdinalIgnoreCase);
            return marks.Where(filtro)
                .Select(mark => byFolio.GetValueOrDefault(mark.Folio) is { } item ? new CarteraConvocatoriaGrupoMiembro
                {
                    Folio = item.Folio,
                    Name = item.Name,
                    Region = item.Region,
                    Substation = item.Substation,
                    InterestGroup = item.InterestGroup,
                    Technology = item.Technology,
                    Mw = item.Mw,
                    HasCfeCost = item.NetworkCostUsd.HasValue || item.NetworkCostMxn.HasValue,
                    NetworkCostUsd = item.NetworkCostUsd,
                    Viability = item.TechnicalViability,
                    Consideration = item.Consideration,
                    Cluster = mark.Cluster,
                    Excluyente1 = mark.Excluyente1,
                    Excluyente2 = mark.Excluyente2,
                    IsCurrent = string.Equals(item.Folio, folioActual, StringComparison.OrdinalIgnoreCase)
                } : null)
                .Where(member => member is not null).Select(member => member!)
                .OrderByDescending(member => member.Mw).ThenBy(member => member.Folio, StringComparer.Ordinal).ToList();
        }

        [HttpGet("DashboardProyectos/Convocatorias/Ficha")]
        [ResponseCache(NoStore = true, Location = ResponseCacheLocation.None)]
        public async Task<IActionResult> FichaCarteraConvocatoria([FromQuery] string folio)
        {
            var normalizedFolio = (folio ?? string.Empty).Trim();
            if (string.IsNullOrWhiteSpace(normalizedFolio) ||
                normalizedFolio.Length > 60 ||
                normalizedFolio.Any(char.IsControl))
            {
                return BadRequest("El folio del proyecto no es válido.");
            }

            try
            {
                var cartera = await _proyectosPrivadosRepository.ObtenerCarteraConvocatoriaAsync();
                var project = cartera.Projects.FirstOrDefault(item =>
                    item.Source == "Mixtos II" && item.Consideration == "firme" &&
                    string.Equals(item.Folio, normalizedFolio, StringComparison.OrdinalIgnoreCase));
                if (project is null)
                {
                    return NotFound();
                }

                List<CarteraConvocatoriaOperacionProgramada> schedule;
                try
                {
                    schedule = await _proyectosPrivadosRepository.ObtenerProgramaOperacionConvocatoriaAsync();
                }
                catch (Exception ex)
                {
                    // El programa por GCR es complementario: la ficha se sirve aunque el JSON del expediente falle.
                    _logger.LogWarning(ex, "No fue posible leer el programa de operación Mixtos II para {Folio}.", normalizedFolio);
                    schedule = new();
                }

                CarteraConvocatoriaMarca? marks = null;
                try
                {
                    marks = await _proyectosPrivadosRepository.ObtenerMarcaConvocatoriaAsync(project.Folio);
                }
                catch (Exception ex)
                {
                    _logger.LogWarning(ex, "No fue posible leer las marcas de clúster para {Folio}.", normalizedFolio);
                }

                List<CarteraConvocatoriaGrupoMiembro> clusterMembers = new(), exclusiveMembers = new();
                if (marks is not null && (!string.IsNullOrWhiteSpace(marks.Cluster) || !string.IsNullOrWhiteSpace(marks.Excluyente1)))
                {
                    try
                    {
                        var allMarks = await _proyectosPrivadosRepository.ObtenerMarcasConvocatoriaAsync();
                        clusterMembers = GrupoConvocatoria(cartera, allMarks, m => MarcaIgual(m.Cluster, marks.Cluster), project.Folio);
                        exclusiveMembers = GrupoConvocatoria(cartera, allMarks, m => MarcaIgual(m.Excluyente1, marks.Excluyente1) || MarcaIgual(m.Excluyente2, marks.Excluyente1), project.Folio);
                    }
                    catch (Exception ex)
                    {
                        _logger.LogWarning(ex, "No fue posible construir el clúster de {Folio}.", normalizedFolio);
                    }
                }

                var model = new CarteraConvocatoriaFichaViewModel
                {
                    OperationSchedule = schedule,
                    Marks = marks,
                    ClusterMembers = clusterMembers,
                    ExclusiveMembers = exclusiveMembers,
                    Dossier = await _proyectosPrivadosRepository.ObtenerExpedienteConvocatoriaAsync(project.Folio),
                    Portfolio = cartera.Projects
                        .Where(item => item.Source == "Mixtos II" && item.Consideration == "firme")
                        .GroupBy(item => item.Region.Trim(), StringComparer.OrdinalIgnoreCase)
                        .Select(group => new CarteraConvocatoriaResumenGcr {
                            Region = group.Key, Projects = group.Count(), Mw = group.Sum(item => item.Mw),
                            NetworkCostMxn = group.Sum(item => item.NetworkCostMxn ?? 0),
                            NetworkCostUsd = group.Sum(item => item.NetworkCostUsd ?? 0),
                            ProjectsWithCostMxn = group.Count(item => item.NetworkCostMxn.HasValue),
                            ProjectsWithCostUsd = group.Count(item => item.NetworkCostUsd.HasValue),
                            Works = group.Sum(item => item.WorksCount ?? 0),
                            ProjectsWithWorks = group.Count(item => item.WorksCount.HasValue)
                        }).OrderByDescending(group => group.Mw).ToList(),
                    Project = project,
                    Notes = cartera.Notes
                        .Where(note => string.Equals(note.Folio, project.Folio, StringComparison.OrdinalIgnoreCase))
                        .OrderByDescending(note => note.Date)
                        .ToList(),
                    Recipients = await _pamService.ObtenerDestinatariosAsync(),
                    SourceVersion = cartera.SourceVersion,
                    LatestImport = cartera.LatestImport
                };
                return View("CarteraConvocatoriaFicha", model);
            }
            catch (SqlException ex)
            {
                _logger.LogError(ex, "No fue posible construir la ficha Mixtos II del folio {Folio}.", normalizedFolio);
                return StatusCode(
                    StatusCodes.Status503ServiceUnavailable,
                    "La cartera institucional Mixtos II no está disponible en este momento.");
            }
        }

        // Ficha ejecutiva de toda la cartera Mixtos II (universo registrado y proyectos considerados).
        [HttpGet("DashboardProyectos/Convocatorias/Cartera")]
        [ResponseCache(NoStore = true, Location = ResponseCacheLocation.None)]
        public async Task<IActionResult> FichaCarteraConvocatoriaResumen()
        {
            try
            {
                var cartera = await _proyectosPrivadosRepository.ObtenerCarteraConvocatoriaAsync();
                var dossiers = await _proyectosPrivadosRepository.ObtenerExpedientesConvocatoriaAsync();
                List<CarteraConvocatoriaMarca> marks;
                try { marks = await _proyectosPrivadosRepository.ObtenerMarcasConvocatoriaAsync(); }
                catch (Exception ex) { _logger.LogWarning(ex, "No fue posible leer las marcas de clúster para la ficha de cartera."); marks = new(); }
                var model = CarteraConvocatoriaResumenBuilder.Build(cartera, dossiers, marks);
                model.Recipients = await _pamService.ObtenerDestinatariosAsync();
                return View("CarteraConvocatoriaResumen", model);
            }
            catch (SqlException ex)
            {
                _logger.LogError(ex, "No fue posible construir la ficha de cartera Mixtos II.");
                return StatusCode(StatusCodes.Status503ServiceUnavailable, "La cartera institucional Mixtos II no está disponible en este momento.");
            }
        }

        [HttpPost("DashboardProyectos/Convocatorias/Cartera/Enviar")]
        [ValidateAntiForgeryToken]
        [RequestSizeLimit(67108864)]
        [RequestFormLimits(MultipartBodyLengthLimit = 67108864)]
        public async Task<IActionResult> EnviarFichaCarteraConvocatoriaResumen([FromBody] CarteraConvocatoriaEnviarFichaInput input)
        {
            if (input is null || string.IsNullOrWhiteSpace(input.ArchivoBase64) || input.UsuarioIds is not { Count: > 0 })
                return BadRequest(new { ok = false, mensaje = "Selecciona al menos un destinatario y espera a que la ficha termine de generarse." });

            var formato = string.Equals(input.Formato, "pptx", StringComparison.OrdinalIgnoreCase) ? "pptx" : "pdf";
            byte[] attachment;
            try
            {
                var base64 = input.ArchivoBase64;
                var comma = base64.IndexOf(',');
                if (comma >= 0) base64 = base64[(comma + 1)..];
                attachment = Convert.FromBase64String(base64);
            }
            catch
            {
                return BadRequest(new { ok = false, mensaje = "El archivo adjunto no se pudo procesar. Vuelve a generarlo." });
            }
            const int maxAttachmentBytes = 20 * 1024 * 1024;
            if (attachment.Length == 0 || attachment.Length > maxAttachmentBytes)
                return BadRequest(new { ok = false, mensaje = "La ficha está vacía o supera el límite seguro de 20 MB para correo." });
            if (!FichaAdjuntoValidador.EsValido(attachment, formato, minPaginasPdf: 2))
                return BadRequest(new { ok = false, mensaje = "El adjunto está incompleto o no es un PDF o PowerPoint válido. Regenera la ficha completa." });

            var cartera = await _proyectosPrivadosRepository.ObtenerCarteraConvocatoriaAsync();
            var corte = cartera.LatestImport?.CutoffDate?.ToString("dd 'de' MMMM 'de' yyyy", System.Globalization.CultureInfo.GetCultureInfo("es-MX")) ?? "corte vigente";
            var fileName = $"Mixtos_II_Convocatoria_Proyectos_Estrategicos_{DateTime.Now:yyyyMMdd}.{formato}";
            var recipients = await _pamService.ObtenerDestinatariosAsync();
            var selectedIds = input.UsuarioIds.Distinct().ToHashSet();
            var selected = recipients.Where(recipient => selectedIds.Contains(recipient.IdUsuario)).ToList();
            if (selected.Count == 0)
                return BadRequest(new { ok = false, mensaje = "Los destinatarios seleccionados no pertenecen al catálogo institucional." });

            var profile = ObtenerPerfilUsuarioActual();
            var sender = string.IsNullOrWhiteSpace(profile?.Nombre) ? "el equipo de la DGMESNIE" : profile.Nombre;
            var sent = new List<string>();
            var failed = new List<string>();
            foreach (var recipient in selected)
            {
                try
                {
                    var nota = string.IsNullOrWhiteSpace(input.MensajeAdicional) ? "" :
                        $"<p style=\"margin:12px 0;padding:10px 14px;border-left:4px solid #a57f2c;background:#faf8f5\">{System.Net.WebUtility.HtmlEncode(input.MensajeAdicional.Trim())}</p>";
                    var body = $@"<div style=""font-family:'Noto Sans',Arial,sans-serif;color:#1c1b1a;font-size:14px;line-height:1.5"">
<p>Estimado(a) {System.Net.WebUtility.HtmlEncode(recipient.Nombre)}:</p>
<p>Se comparte el <strong>panorama ejecutivo de la Convocatoria de Proyectos Estratégicos · Mixtos II</strong> (corte {corte}), en formato {formato.ToUpperInvariant()}, con el universo registrado y los proyectos considerados: capacidad por gerencia, tecnología y almacenamiento, modalidad de asociación, entradas en operación, interconexión y trámites, costos de red CFE, inversión declarada y evaluaciones.</p>
{nota}
<p>Envía: {System.Net.WebUtility.HtmlEncode(sender)}<br/>Dirección General de Metodologías y Estadísticas del Sistema Nacional de Información Energética · Secretaría de Energía</p>
</div>";
                    await _emailService.EnviarCorreo(recipient.Correo, $"Convocatoria de Proyectos Estratégicos · Mixtos II · panorama ejecutivo · corte {corte}", body, attachment, fileName);
                    sent.Add(recipient.Nombre);
                }
                catch (Exception ex)
                {
                    _logger.LogError(ex, "No fue posible enviar la ficha de cartera Mixtos II a {Correo}.", recipient.Correo);
                    failed.Add(recipient.Nombre);
                }
            }
            if (sent.Count == 0)
                return StatusCode(StatusCodes.Status502BadGateway, new { ok = false, mensaje = "No fue posible enviar la ficha a ningún destinatario." });
            var message = $"Ficha enviada a {sent.Count} destinatario(s): {string.Join(", ", sent)}.";
            if (failed.Count > 0) message += $" No se pudo enviar a: {string.Join(", ", failed)}.";
            return Ok(new { ok = true, mensaje = message });
        }

        [HttpGet("DashboardProyectos/Convocatorias/Calculadora")]
        public async Task<IActionResult> CalculadoraConvocatoria([FromQuery] string folio, [FromServices] IWebHostEnvironment environment)
        {
            if (string.IsNullOrWhiteSpace(folio) || folio.Length > 60) return BadRequest();
            var dossier = await _proyectosPrivadosRepository.ObtenerExpedienteConvocatoriaAsync(folio.Trim());
            var file = dossier?.CalculatorFile;
            if (file == null || !System.Text.RegularExpressions.Regex.IsMatch(file,@"^[a-f0-9]{64}\.(xlsm|xlsx)$")) return NotFound();
            var path = Path.Combine(environment.ContentRootPath,"App_Data","ConvocatoriaCalculadoras",file);
            if (!System.IO.File.Exists(path)) return NotFound();
            Response.Headers["X-Content-Type-Options"] = "nosniff";
            return PhysicalFile(path,"application/octet-stream","Calculadora_" + dossier!.Folio + Path.GetExtension(file));
        }

        [HttpPost("DashboardProyectos/Convocatorias/Ficha/Enviar")]
        [ValidateAntiForgeryToken]
        [RequestSizeLimit(67108864)]
        [RequestFormLimits(MultipartBodyLengthLimit = 67108864)]
        public async Task<IActionResult> EnviarFichaCarteraConvocatoria(
            [FromBody] CarteraConvocatoriaEnviarFichaInput input)
        {
            if (input is null || !ModelState.IsValid ||
                string.IsNullOrWhiteSpace(input.ArchivoBase64) ||
                input.UsuarioIds is not { Count: > 0 })
            {
                return BadRequest(new { ok = false, mensaje = "Selecciona al menos un destinatario y espera a que la ficha termine de generarse." });
            }

            var folio = input.Folio.Trim();
            if (folio.Length > 60 || folio.Any(char.IsControl))
                return BadRequest(new { ok = false, mensaje = "El folio de la ficha no es válido." });

            var cartera = await _proyectosPrivadosRepository.ObtenerCarteraConvocatoriaAsync();
            var project = cartera.Projects.FirstOrDefault(item =>
                item.Source == "Mixtos II" && item.Consideration == "firme" &&
                string.Equals(item.Folio, folio, StringComparison.OrdinalIgnoreCase));
            if (project is null)
                return NotFound(new { ok = false, mensaje = "El proyecto ya no forma parte de la cartera firme vigente." });

            var formato = string.Equals(input.Formato, "pptx", StringComparison.OrdinalIgnoreCase) ? "pptx" : "pdf";
            byte[] attachment;
            try
            {
                var base64 = input.ArchivoBase64;
                var comma = base64.IndexOf(',');
                if (comma >= 0) base64 = base64[(comma + 1)..];
                attachment = Convert.FromBase64String(base64);
            }
            catch
            {
                return BadRequest(new { ok = false, mensaje = "El archivo adjunto no se pudo procesar. Vuelve a generarlo." });
            }

            const int maxAttachmentBytes = 20 * 1024 * 1024;
            if (attachment.Length == 0 || attachment.Length > maxAttachmentBytes)
                return BadRequest(new { ok = false, mensaje = "La ficha está vacía o supera el límite seguro de 20 MB para correo." });
            // Mixtos II es una ficha de varias láminas: una muestra de una página no es el entregable.
            if (!FichaAdjuntoValidador.EsValido(attachment, formato, minPaginasPdf: 2))
                return BadRequest(new { ok = false, mensaje = "El adjunto está incompleto o no es un PDF o PowerPoint válido. Regenera la ficha completa; no se aceptan muestras PDF de una sola página." });

            var safeFolio = string.Concat(folio.Select(character =>
                char.IsLetterOrDigit(character) || character is '-' or '_' ? character : '_'));
            var fileName = $"Ficha_MixtosII_{safeFolio}_{DateTime.Now:yyyyMMdd}.{formato}";

            var recipients = await _pamService.ObtenerDestinatariosAsync();
            var selectedIds = input.UsuarioIds.Distinct().ToHashSet();
            var selected = recipients.Where(recipient => selectedIds.Contains(recipient.IdUsuario)).ToList();
            if (selected.Count == 0)
                return BadRequest(new { ok = false, mensaje = "Los destinatarios seleccionados no pertenecen al catálogo institucional." });

            var profile = ObtenerPerfilUsuarioActual();
            var sender = string.IsNullOrWhiteSpace(profile?.Nombre) ? "el equipo de la DGMESNIE" : profile.Nombre;
            var sent = new List<string>();
            var failed = new List<string>();
            foreach (var recipient in selected)
            {
                try
                {
                    var body = ConstruirCorreoFichaConvocatoria(recipient.Nombre, project, formato, sender, input.MensajeAdicional);
                    await _emailService.EnviarCorreo(
                        recipient.Correo,
                        $"Ficha territorial Mixtos II · {project.Folio}",
                        body,
                        attachment,
                        fileName);
                    sent.Add(recipient.Nombre);
                }
                catch (Exception ex)
                {
                    _logger.LogError(ex, "No fue posible enviar la ficha Mixtos II {Folio} a {Correo}.", project.Folio, recipient.Correo);
                    failed.Add(recipient.Nombre);
                }
            }

            if (sent.Count == 0)
                return StatusCode(StatusCodes.Status502BadGateway, new { ok = false, mensaje = "No fue posible enviar la ficha a ningún destinatario." });

            var message = $"Ficha enviada a {sent.Count} destinatario(s): {string.Join(", ", sent)}.";
            if (failed.Count > 0) message += $" No se pudo enviar a: {string.Join(", ", failed)}.";
            return Ok(new { ok = true, mensaje = message });
        }

        [HttpGet("DashboardProyectos/TarifasTerritoriales")]
        public async Task<IActionResult> TarifasTerritoriales(
            [FromQuery] string? divisiones,
            CancellationToken cancellationToken)
        {
            var seleccion = (divisiones ?? string.Empty)
                .Split(',', StringSplitOptions.RemoveEmptyEntries | StringSplitOptions.TrimEntries)
                .Where(nombre => nombre.Length is > 0 and <= 80)
                .Distinct(StringComparer.OrdinalIgnoreCase)
                .ToArray();

            if (seleccion.Length == 0)
            {
                return BadRequest(new
                {
                    ok = false,
                    code = "INVALID_TARIFF_DIVISION",
                    mensaje = "Se requiere al menos una división tarifaria."
                });
            }

            if (seleccion.Length > 17)
            {
                return BadRequest(new
                {
                    ok = false,
                    code = "TOO_MANY_TARIFF_DIVISIONS",
                    mensaje = "El análisis admite como máximo las 17 divisiones tarifarias nacionales."
                });
            }

            try
            {
                var data = await _tarifasRepository.ObtenerResumenTerritorialAsync(
                    seleccion,
                    cancellationToken);
                return Json(new { ok = true, data });
            }
            catch (SqlException exception)
            {
                _logger.LogWarning(
                    exception,
                    "No fue posible consultar tarifas territoriales para {DivisionCount} divisiones.",
                    seleccion.Length);
                return StatusCode(StatusCodes.Status503ServiceUnavailable, new
                {
                    ok = false,
                    code = "TARIFFS_UNAVAILABLE",
                    mensaje = "No fue posible recuperar el histórico tarifario institucional en este momento."
                });
            }
        }

        [HttpGet("DashboardProyectos/IndicadoresInegi")]
        public async Task<IActionResult> IndicadoresInegi(
            [FromQuery] string entidad,
            [FromQuery] string? municipio,
            [FromQuery] double? areaKm2,
            CancellationToken cancellationToken)
        {
            var entidadNormalizada = (entidad ?? string.Empty).Trim();
            var municipioNormalizado = (municipio ?? string.Empty).Trim();
            if (entidadNormalizada.Length != 2 ||
                !int.TryParse(entidadNormalizada, out var entidadClave) ||
                entidadClave is < 1 or > 32)
            {
                return BadRequest(new { ok = false, code = "INVALID_STATE", mensaje = "La clave de entidad debe estar entre 01 y 32." });
            }

            if (!string.IsNullOrEmpty(municipioNormalizado) &&
                (municipioNormalizado.Length != 3 ||
                 !int.TryParse(municipioNormalizado, out var municipioClave) ||
                 municipioClave is < 1 or > 999))
            {
                return BadRequest(new { ok = false, code = "INVALID_MUNICIPALITY", mensaje = "La clave municipal debe estar entre 001 y 999." });
            }

            if (areaKm2 is <= 0 or > 10_000_000)
            {
                return BadRequest(new { ok = false, code = "INVALID_AREA", mensaje = "La superficie territorial no es válida." });
            }

            if (!_inegiService.IsConfigured)
            {
                return StatusCode(StatusCodes.Status503ServiceUnavailable, new
                {
                    ok = false,
                    code = "INEGI_TOKEN_MISSING",
                    mensaje = "Configure Inegi:Token mediante secretos de usuario o la variable Inegi__Token."
                });
            }

            var geographicCode = entidadNormalizada + municipioNormalizado;
            try
            {
                var result = await _inegiService.GetAsync(geographicCode, areaKm2, cancellationToken);
                return Json(new { ok = true, data = result });
            }
            catch (HttpRequestException ex)
            {
                _logger.LogWarning(ex, "La consulta territorial INEGI falló para {GeographicCode}.", geographicCode);
                return StatusCode(StatusCodes.Status502BadGateway, new
                {
                    ok = false,
                    code = "INEGI_UNAVAILABLE",
                    mensaje = "INEGI no devolvió datos para el ámbito solicitado."
                });
            }
        }

        [HttpGet("DashboardProyectos/PermisosEnergeticos/Ficha")]
        [ResponseCache(NoStore = true, Location = ResponseCacheLocation.None)]
        public async Task<IActionResult> FichaPermisoEnergetico(
            [FromQuery] string tipo,
            [FromQuery] string numeroPermiso,
            CancellationToken cancellationToken)
        {
            var normalizedType = (tipo ?? string.Empty).Trim().ToLowerInvariant();
            var normalizedPermit = (numeroPermiso ?? string.Empty).Trim();
            if (!_permisosService.TiposSoportados.Contains(normalizedType, StringComparer.Ordinal) ||
                string.IsNullOrWhiteSpace(normalizedPermit) ||
                normalizedPermit.Length > 120 ||
                normalizedPermit.Any(char.IsControl))
            {
                return BadRequest("El tipo o número de permiso no es válido.");
            }

            try
            {
                var acceso = _permisosAccessPolicy.Resolver(ObtenerPerfilUsuarioActual());
                var detalle = await _permisosService.ObtenerDetalleAsync(
                    normalizedType,
                    normalizedPermit,
                    acceso,
                    cancellationToken);
                if (detalle is null)
                {
                    return NotFound();
                }

                var model = new PermisoEnergeticoFichaViewModel
                {
                    Detalle = detalle,
                    Destinatarios = await _pamService.ObtenerDestinatariosAsync()
                };
                return View("PermisoEnergeticoFicha", model);
            }
            catch (SqlException ex)
            {
                _logger.LogError(
                    ex,
                    "No fue posible construir la ficha del permiso {NumeroPermiso} ({Tipo}).",
                    normalizedPermit,
                    normalizedType);
                return StatusCode(
                    StatusCodes.Status503ServiceUnavailable,
                    "El inventario institucional de permisos no está disponible en este momento.");
            }
        }

        [HttpPost("DashboardProyectos/PermisosEnergeticos/Ficha/Enviar")]
        [ValidateAntiForgeryToken]
        [RequestSizeLimit(67108864)]
        [RequestFormLimits(MultipartBodyLengthLimit = 67108864)]
        public async Task<IActionResult> EnviarFichaPermisoEnergetico(
            [FromBody] PermisoEnergeticoEnviarFichaInput input,
            CancellationToken cancellationToken)
        {
            if (input is null ||
                string.IsNullOrWhiteSpace(input.ArchivoBase64) ||
                input.UsuarioIds is not { Count: > 0 })
            {
                return BadRequest(new
                {
                    ok = false,
                    mensaje = "Selecciona al menos un destinatario y espera a que la ficha termine de generarse."
                });
            }

            var normalizedType = (input.Tipo ?? string.Empty).Trim().ToLowerInvariant();
            var normalizedPermit = (input.NumeroPermiso ?? string.Empty).Trim();
            if (!_permisosService.TiposSoportados.Contains(normalizedType, StringComparer.Ordinal) ||
                string.IsNullOrWhiteSpace(normalizedPermit) ||
                normalizedPermit.Length > 120)
            {
                return BadRequest(new { ok = false, mensaje = "La ficha de permiso solicitada no es válida." });
            }

            var acceso = _permisosAccessPolicy.Resolver(ObtenerPerfilUsuarioActual());
            var detalle = await _permisosService.ObtenerDetalleAsync(
                normalizedType,
                normalizedPermit,
                acceso,
                cancellationToken);
            if (detalle is null)
            {
                return NotFound(new { ok = false, mensaje = "El permiso ya no está disponible en el inventario." });
            }

            var formato = string.Equals(input.Formato, "pptx", StringComparison.OrdinalIgnoreCase)
                ? "pptx"
                : "pdf";
            byte[] adjunto;
            try
            {
                var base64 = input.ArchivoBase64;
                var comma = base64.IndexOf(',');
                if (comma >= 0)
                {
                    base64 = base64[(comma + 1)..];
                }

                adjunto = Convert.FromBase64String(base64);
            }
            catch
            {
                return BadRequest(new
                {
                    ok = false,
                    mensaje = "El archivo adjunto no se pudo procesar. Vuelve a generarlo."
                });
            }

            const int maxAttachmentBytes = 20 * 1024 * 1024;
            if (adjunto.Length == 0 || adjunto.Length > maxAttachmentBytes)
            {
                return BadRequest(new
                {
                    ok = false,
                    mensaje = "La ficha está vacía o supera el límite seguro de 20 MB para correo."
                });
            }

            var safePermit = string.Concat(
                normalizedPermit.Select(character =>
                    char.IsLetterOrDigit(character) || character is '-' or '_'
                        ? character
                        : '_'));
            var nombreArchivo = string.IsNullOrWhiteSpace(input.NombreArchivo)
                ? $"Ficha_permiso_{safePermit}_{DateTime.Now:yyyyMMdd}.{formato}"
                : (input.NombreArchivo.EndsWith($".{formato}", StringComparison.OrdinalIgnoreCase)
                    ? input.NombreArchivo
                    : $"{input.NombreArchivo}.{formato}");

            var destinatarios = await _pamService.ObtenerDestinatariosAsync();
            var seleccionados = destinatarios
                .Where(destino => input.UsuarioIds.Contains(destino.IdUsuario))
                .ToList();
            if (seleccionados.Count == 0)
            {
                return BadRequest(new { ok = false, mensaje = "Los destinatarios seleccionados no son válidos." });
            }

            var perfil = ObtenerPerfilUsuarioActual();
            var remitente = string.IsNullOrWhiteSpace(perfil?.Nombre)
                ? "el equipo de la DGMESNIE"
                : perfil.Nombre;
            var enviados = new List<string>();
            var fallidos = new List<string>();

            foreach (var destino in seleccionados)
            {
                try
                {
                    var cuerpo = ConstruirCorreoFichaPermiso(
                        destino.Nombre,
                        detalle,
                        formato,
                        remitente,
                        input.MensajeAdicional);
                    await _emailService.EnviarCorreo(
                        destino.Correo,
                        $"Ficha territorial del permiso {detalle.NumeroPermiso}",
                        cuerpo,
                        adjunto,
                        nombreArchivo);
                    enviados.Add(destino.Nombre);
                }
                catch (Exception ex)
                {
                    _logger.LogError(
                        ex,
                        "No fue posible enviar la ficha del permiso {NumeroPermiso} a {Correo}.",
                        detalle.NumeroPermiso,
                        destino.Correo);
                    fallidos.Add(destino.Nombre);
                }
            }

            if (enviados.Count == 0)
            {
                return StatusCode(StatusCodes.Status502BadGateway, new
                {
                    ok = false,
                    mensaje = "No fue posible enviar la ficha a ningún destinatario."
                });
            }

            var message = $"Ficha enviada a {enviados.Count} destinatario(s): {string.Join(", ", enviados)}.";
            if (fallidos.Count > 0)
            {
                message += $" No se pudo enviar a: {string.Join(", ", fallidos)}.";
            }

            return Ok(new { ok = true, mensaje = message });
        }

        [HttpPost("DashboardProyectos/EnviarReporte")]
        [ValidateAntiForgeryToken]
        [RequestSizeLimit(67108864)]
        [RequestFormLimits(MultipartBodyLengthLimit = 67108864)]
        public async Task<IActionResult> EnviarReporte([FromBody] PamEnviarFichaInput input)
        {
            if (input == null || string.IsNullOrWhiteSpace(input.ArchivoBase64) || input.UsuarioIds is not { Count: > 0 })
                return BadRequest(new { ok = false, mensaje = "Selecciona al menos un destinatario y genera primero el archivo." });

            var formato = string.Equals(input.Formato, "pdf16x9", StringComparison.OrdinalIgnoreCase) ? "pdf16x9" : "pdf";
            var descripcionFormato = formato == "pdf16x9" ? "PDF horizontal 16:9" : "PDF tamaño carta";
            var clave = string.IsNullOrWhiteSpace(input.ClavePem) ? "ReporteTerritorial" : input.ClavePem.Trim();

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

            const int maxAttachmentBytes = 20 * 1024 * 1024;
            if (adjunto.Length == 0 || adjunto.Length > maxAttachmentBytes)
                return BadRequest(new { ok = false, mensaje = "El archivo está vacío o supera el límite seguro de 20 MB para correo." });

            var nombreArchivo = string.IsNullOrWhiteSpace(input.NombreArchivo)
                ? $"DGMESNIE_reporte_territorial{(formato == "pdf16x9" ? "_16x9" : string.Empty)}_{DateTime.Now:yyyy-MM-dd}.pdf"
                : (input.NombreArchivo.EndsWith(".pdf", StringComparison.OrdinalIgnoreCase) ? input.NombreArchivo : $"{input.NombreArchivo}.pdf");

            var destinatarios = await _pamService.ObtenerDestinatariosAsync();
            var seleccionados = destinatarios.Where(d => input.UsuarioIds.Contains(d.IdUsuario)).ToList();
            if (seleccionados.Count == 0)
                return BadRequest(new { ok = false, mensaje = "Los destinatarios seleccionados no son válidos." });

            var perfilJson = HttpContext.Session.GetString("PerfilUsuario");
            var perfil = string.IsNullOrWhiteSpace(perfilJson) ? null : JsonConvert.DeserializeObject<PerfilUsuario>(perfilJson);
            var remitente = string.IsNullOrWhiteSpace(perfil?.Nombre) ? "el equipo de la DGMESNIE" : perfil.Nombre;
            string remitenteCargo = null;
            if (perfil != null && int.TryParse(perfil.IdUsuario, out var remitenteId))
                remitenteCargo = destinatarios.FirstOrDefault(d => d.IdUsuario == remitenteId)?.Cargo;
            if (string.IsNullOrWhiteSpace(remitenteCargo)) remitenteCargo = perfil?.Cargo;

            var enviados = new List<string>();
            var fallidos = new List<string>();
            var motivosFallo = new HashSet<string>(StringComparer.OrdinalIgnoreCase);
            foreach (var destino in seleccionados)
            {
                try
                {
                    var cuerpo = ConstruirCorreoReporte(destino.Nombre, descripcionFormato, remitente, remitenteCargo, input.MensajeAdicional, nombreArchivo);
                    await _emailService.EnviarCorreo(
                        destino.Correo,
                        "Reporte territorial DGMESNIE",
                        cuerpo,
                        adjunto,
                        nombreArchivo);
                    enviados.Add(destino.Nombre);
                }
                catch (Exception ex)
                {
                    _logger.LogError(ex, "No fue posible enviar el reporte territorial a {Correo}.", destino.Correo);
                    fallidos.Add(destino.Nombre);
                    motivosFallo.Add(DescribirFalloEnvio(ex));
                }
            }

            if (enviados.Count == 0)
            {
                var detalle = motivosFallo.Count > 0 ? $" {string.Join(" ", motivosFallo)}" : string.Empty;
                return StatusCode(StatusCodes.Status502BadGateway, new
                {
                    ok = false,
                    mensaje = $"No fue posible enviar el reporte a ningún destinatario.{detalle}"
                });
            }

            var mensaje = $"Reporte enviado a {enviados.Count} destinatario(s): {string.Join(", ", enviados)}.";
            if (fallidos.Count > 0)
            {
                var detalle = motivosFallo.Count > 0 ? $" Motivo: {string.Join(" ", motivosFallo)}" : string.Empty;
                mensaje += $" No se pudo enviar a: {string.Join(", ", fallidos)}.{detalle}";
            }
            return Ok(new { ok = true, mensaje });
        }

        private PerfilUsuario? ObtenerPerfilUsuarioActual()
        {
            var json = HttpContext.Session.GetString("PerfilUsuario");
            if (string.IsNullOrWhiteSpace(json))
            {
                return null;
            }

            try
            {
                return JsonConvert.DeserializeObject<PerfilUsuario>(json);
            }
            catch (JsonException ex)
            {
                _logger.LogWarning(ex, "No fue posible interpretar el perfil de sesión.");
                return null;
            }
        }

        private static string ConstruirCorreoFichaPermiso(
            string nombreDestino,
            PermisoEnergeticoDetalle detalle,
            string formato,
            string remitente,
            string mensajeAdicional)
        {
            static string Cod(string valor) => System.Net.WebUtility.HtmlEncode(valor ?? string.Empty);
            var formatoLabel = formato == "pptx" ? "PowerPoint" : "PDF";

            var parrafos = new List<string>
            {
                $"{Cod(remitente)} le comparte la ficha territorial del permiso <strong>{Cod(detalle?.NumeroPermiso)}</strong>, correspondiente a <strong>{Cod(detalle?.Nombre)}</strong>."
            };
            if (!string.IsNullOrWhiteSpace(mensajeAdicional))
                parrafos.Add(Cod(mensajeAdicional));

            var metadatos = new List<CampoCorreo>
            {
                new CampoCorreo("Número de permiso", detalle?.NumeroPermiso ?? "—"),
                new CampoCorreo("Formato", formatoLabel)
            };

            return PlantillaCorreoInstitucional.Construir(new ContenidoCorreo
            {
                Antetitulo = "Ficha territorial · Permiso energético",
                Titulo = string.IsNullOrWhiteSpace(detalle?.Nombre) ? "Ficha territorial de permiso" : detalle.Nombre,
                Metadatos = metadatos,
                Saludo = string.IsNullOrWhiteSpace(nombreDestino) ? "Estimada(o)" : $"Estimada(o) {nombreDestino}",
                Parrafos = parrafos,
                SeccionTitulo = "Contenido de la ficha",
                Puntos = new[]
                {
                    new CampoCorreo("Identificación", "Titular, tipo de permiso, clasificación y estatus vigente."),
                    new CampoCorreo("Localización", "Entidad, municipio y coordenadas registradas."),
                    new CampoCorreo("Trazabilidad", "Fuente institucional y fecha de corte del inventario.")
                },
                AdjuntoFormato = formatoLabel,
                Nota = "Documento informativo de trabajo. La información corresponde al inventario institucional vigente a su fecha de corte.",
                Firmante = remitente,
                FirmanteCargo = "Secretaría de Energía · DGMESNIE"
            });
        }

        private static string ConstruirCorreoFichaConvocatoria(
            string recipientName,
            CarteraConvocatoriaProyecto project,
            string formato,
            string sender,
            string? additionalMessage)
        {
            static string Cod(string value) => System.Net.WebUtility.HtmlEncode(value ?? string.Empty);
            var formatLabel = formato == "pptx" ? "PowerPoint" : "PDF";
            var paragraphs = new List<string>
            {
                $"{Cod(sender)} le comparte la ficha territorial del proyecto <strong>{Cod(project.Folio)}</strong>, correspondiente a <strong>{Cod(project.Name)}</strong>."
            };
            if (!string.IsNullOrWhiteSpace(additionalMessage)) paragraphs.Add(Cod(additionalMessage));

            return PlantillaCorreoInstitucional.Construir(new ContenidoCorreo
            {
                Antetitulo = "Planeación vinculante · Mixtos II",
                Titulo = project.Name,
                Metadatos = new[]
                {
                    new CampoCorreo("Folio", project.Folio),
                    new CampoCorreo("Capacidad", $"{project.Mw:N1} MW"),
                    new CampoCorreo("Gerencia", project.Region),
                    new CampoCorreo("Formato", formatLabel)
                },
                Saludo = string.IsNullOrWhiteSpace(recipientName) ? "Estimada(o)" : $"Estimada(o) {recipientName}",
                Parrafos = paragraphs,
                SeccionTitulo = "Contenido de la ficha",
                Puntos = new[]
                {
                    new CampoCorreo("Proyecto", "Capacidad, tecnología, promovente, prelación y resultado de análisis."),
                    new CampoCorreo("Territorio", "Ubicación, geometrías KML del proyecto y punto de interconexión."),
                    new CampoCorreo("Red", "Obras habilitantes, costos registrados y trazabilidad del corte.")
                },
                AdjuntoNombre = $"Ficha Mixtos II {project.Folio}",
                AdjuntoFormato = formatLabel,
                Nota = "Documento informativo de trabajo. La cartera no equivale por sí sola a capacidad cubierta y está sujeta a conciliación institucional.",
                Firmante = sender,
                FirmanteCargo = "Secretaría de Energía · DGMESNIE"
            });
        }

        private static string DescribirFalloEnvio(Exception ex)
        {
            var detalle = ex?.ToString() ?? string.Empty;
            var motivos = new List<string>();

            if (detalle.Contains("535", StringComparison.OrdinalIgnoreCase)
                || detalle.Contains("Autenticación SMTP rechazada", StringComparison.OrdinalIgnoreCase)
                || detalle.Contains("Authentication unsuccessful", StringComparison.OrdinalIgnoreCase))
            {
                motivos.Add("El servidor SMTP rechazó las credenciales (535).");
            }

            if (detalle.Contains("413", StringComparison.OrdinalIgnoreCase))
                motivos.Add("SendGrid rechazó el tamaño del mensaje o adjunto (413).");

            if (motivos.Count == 0)
                motivos.Add("Los proveedores de correo no aceptaron el envío; revisa el registro del servidor.");

            return string.Join(" ", motivos);
        }

        private static string ConstruirCorreoReporte(string nombreDestino, string descripcionFormato, string remitente, string remitenteCargo, string mensajeAdicional, string nombreArchivo)
        {
            static string Cod(string valor) => System.Net.WebUtility.HtmlEncode(valor ?? string.Empty);

            var parrafos = new List<string>
            {
                $"{Cod(remitente)} le comparte el <strong>reporte de análisis territorial</strong> generado desde el Dashboard de Proyectos Energéticos de la DGMESNIE."
            };
            if (!string.IsNullOrWhiteSpace(mensajeAdicional))
                parrafos.Add(Cod(mensajeAdicional));

            return PlantillaCorreoInstitucional.Construir(new ContenidoCorreo
            {
                Antetitulo = "Análisis territorial · DGMESNIE",
                Titulo = "Reporte de análisis territorial",
                Metadatos = new[]
                {
                    new CampoCorreo("Fecha de corte", DateTime.Now.ToString("dd 'de' MMMM 'de' yyyy", new CultureInfo("es-MX"))),
                    new CampoCorreo("Formato", descripcionFormato ?? "PDF")
                },
                Saludo = string.IsNullOrWhiteSpace(nombreDestino) ? "Estimada(o)" : $"Estimada(o) {nombreDestino}",
                Parrafos = parrafos,
                SeccionTitulo = "Contenido del reporte",
                Puntos = new[]
                {
                    new CampoCorreo("Cobertura", "Zona analizada, radio y superficie, con el centroide de referencia."),
                    new CampoCorreo("Capas", "Elementos hallados por capa, con la trazabilidad de cada fuente."),
                    new CampoCorreo("Índice", "Secciones numeradas y navegables dentro del documento.")
                },
                AdjuntoNombre = nombreArchivo,
                AdjuntoFormato = (descripcionFormato ?? string.Empty).Contains("16:9") ? "PDF" : "PDF",
                Nota = "Documento informativo de trabajo. Los resultados dependen de la fecha de corte y de las fuentes vigentes al momento de la corrida.",
                Firmante = remitente,
                FirmanteCargo = remitenteCargo
            });
        }
    }
}
