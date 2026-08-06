using System;
using System.Collections.Generic;
using System.Linq;
using System.Security.Claims;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Mvc;
using Newtonsoft.Json;
using NSIE.Models;
using NSIE.Models.ProyectosPrivados;
using NSIE.Servicios.Interfaces;
using NSIE.Servicios;

namespace NSIE.Controllers
{
    [ServiceFilter(typeof(ValidacionInputFiltro))]
    [AutorizacionFiltro]
    public class ProyectosPrivadosController : Controller
    {
        private readonly IRepositorioProyectosPrivados _repo;
        private readonly IngestionService _ingestService;
        private readonly ILogger<ProyectosPrivadosController> _logger;
        private readonly IWebHostEnvironment _environment;

        public ProyectosPrivadosController(
            IRepositorioProyectosPrivados repo,
            IngestionService ingestService,
            ILogger<ProyectosPrivadosController> logger,
            IWebHostEnvironment environment)
        {
            _repo = repo;
            _ingestService = ingestService;
            _logger = logger;
            _environment = environment;
        }

        // ── Shell view: Index ────────────────────────────────────────────────
        [HttpGet]
        public async Task<IActionResult> Index()
        {
            ViewData["HeaderViewModel"] = BuildHeader("Control de Proyectos Privados Firmes", "proyecto.png", "Administración, seguimiento regulatorio y bitácora de la cartera de proyectos privados firmes.");
            
            // Pass catalogs to the view for filters dropdowns
            ViewBag.Clasificaciones = await _repo.ObtenerCatClasificacionAsync();
            ViewBag.Prioridades = await _repo.ObtenerCatPrioridadAsync();
            ViewBag.Semaforos = await _repo.ObtenerCatSemaforoAsync();
            ViewBag.Tecnologias = await _repo.ObtenerCatTecnologiaAsync();

            return View();
        }

        // ── View: Detalle con Pestañas ───────────────────────────────────────
        [HttpGet]
        public async Task<IActionResult> Detalle(int id)
        {
            var proj = await _repo.ObtenerProyectoPorIdAsync(id);
            if (proj == null) return NotFound();

            var vm = new ProyectoDetailVM
            {
                Proyecto = proj,
                Tramites = await _repo.ObtenerTramitesPorProyectoAsync(id),
                Bitacoras = await _repo.ObtenerBitacorasPorProyectoAsync(id),
                Acciones = await _repo.ObtenerAccionesPorProyectoAsync(id),
                Documentos = await _repo.ObtenerDocumentosPorProyectoAsync(id),
                Historial = await _repo.ObtenerHistorialPorProyectoAsync(id)
            };

            ViewData["HeaderViewModel"] = BuildHeader(proj.Nombre, "proyecto.png", $"Ficha de detalle técnico, trámites y bitácora del proyecto promovido por {proj.Promovente}.");
            return View(vm);
        }

        // ── View: Registrar Minuta / Reunión ────────────────────────────────
        [HttpGet]
        public async Task<IActionResult> Minuta()
        {
            ViewData["HeaderViewModel"] = BuildHeader("Registrar Minuta de Reunión", "agenda.png", "Cargar los acuerdos tomados en reuniones presenciales o virtuales, actualizar estatus de proyectos y asignar compromisos.");
            
            ViewBag.Valoraciones = await _repo.ObtenerCatValoracionMinutaAsync();
            ViewBag.Clasificaciones = await _repo.ObtenerCatClasificacionAsync();
            ViewBag.Prioridades = await _repo.ObtenerCatPrioridadAsync();
            ViewBag.Semaforos = await _repo.ObtenerCatSemaforoAsync();
            ViewBag.Usuarios = await _repo.ObtenerUsuariosVigentesAsync();
            
            return View();
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Minuta([FromBody] ReunionForm form)
        {
            if (!ModelState.IsValid)
                return BadRequest(ModelState);
            
            try
            {
                var reunionId = await _repo.GuardarReunionMinutaAsync(form, GetCurrentUserName());
                return Json(new { success = true, reunionId });
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error guardando minuta.");
                return StatusCode(500, new { error = "No fue posible registrar la minuta." });
            }
        }

        // ── View: Tablero de Seguimiento (Kanban) ───────────────────────────
        [HttpGet]
        public async Task<IActionResult> Seguimiento()
        {
            ViewData["HeaderViewModel"] = BuildHeader("Tablero de Seguimiento de Compromisos", "calendario.png", "Seguimiento operativo de los compromisos adquiridos en minutas de reuniones.");
            
            ViewBag.Semaforos = await _repo.ObtenerCatSemaforoAsync();
            ViewBag.Proyectos = (await _repo.ObtenerProyectosAsync(null, null, null, null, null))
                .Select(p => new CatalogoItem { Id = p.ProyectoId, Nombre = p.Nombre })
                .ToList();

            return View();
        }

        // ── View: Segunda Convocatoria ──────────────────────────────────────
        [HttpGet]
        public async Task<IActionResult> SegundaConvocatoria()
        {
            await SincronizarCarteraConvocatoriaInicialAsync();

            ViewData["HeaderViewModel"] = new HeaderViewModel
            {
                Title = "Cartera estratégica y 2.ª convocatoria",
                IconPath = "proyecto.png",
                Description = "Prelación, asignación territorial, sesiones y fichas de proyectos estratégicos y particulares.",
                Section = "Seguimiento de proyectos",
                ModuleInfo = JsonConvert.SerializeObject(new
                {
                    title = "Cartera estratégica y segunda convocatoria",
                    description = "Módulo para el seguimiento integrado de proyectos estratégicos y particulares de la segunda convocatoria.",
                    functionality = "Prelación por gerencia, entidad y prioridad; decisiones va/no va; sesiones fechadas, comentarios, mapa y fichas institucionales.",
                    stage = "Consulta y Seguimiento",
                    highlights = new[]
                    {
                        "Prelación y asignación territorial de la cartera.",
                        "Trazabilidad de sesiones, comentarios y decisiones.",
                        "Fichas particulares y presentación general institucional."
                    },
                    roles = new[]
                    {
                        new { icon = "eye", text = "Usuarios Autorizados: Consulta y visualización del reporte general." }
                    },
                    order = new { step = 3, description = "Seguimiento operativo y ejecutivo de la cartera" },
                    context = "Vista nativa integrada al Dashboard de Proyectos y preparada para persistencia SQL.",
                    manualUrl = string.Empty
                })
            };
            return View();
        }

        [HttpGet("ProyectosPrivados/Api/CarteraConvocatoria")]
        public async Task<IActionResult> ApiCarteraConvocatoria()
        {
            try
            {
                var data = await _repo.ObtenerCarteraConvocatoriaAsync();
                return Json(data);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error consultando la cartera estratégica y segunda convocatoria.");
                return StatusCode(500, new { error = "No fue posible consultar la cartera en la base de datos." });
            }
        }

        [HttpGet("ProyectosPrivados/SegundaConvocatoria/Reporte.pdf")]
        public async Task<IActionResult> ReporteCarteraConvocatoriaPdf()
        {
            try
            {
                await SincronizarCarteraConvocatoriaInicialAsync();
                var data = await _repo.ObtenerCarteraConvocatoriaAsync();
                var pdf = CarteraConvocatoriaPdfService.Generar(
                    data,
                    _environment.WebRootPath,
                    new DateTime(2026, 8, 5));
                return File(
                    pdf,
                    "application/pdf",
                    $"SENER_Cartera_Estrategicos_Segunda_Convocatoria_{DateTime.Now:yyyyMMdd}.pdf");
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error generando el reporte PDF de la cartera estratégica y segunda convocatoria.");
                return StatusCode(500, "No fue posible generar el reporte PDF institucional.");
            }
        }

        [HttpPut("ProyectosPrivados/Api/CarteraConvocatoria/Estado")]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> ApiActualizarEstadoConvocatoria(
            [FromBody] ActualizarEstadoConvocatoriaRequest request)
        {
            if (!ModelState.IsValid || request == null)
                return BadRequest(new { error = "Folio y estado son obligatorios." });

            var validStates = new[] { "continua", "revision", "no-continua" };
            var state = request.Estado.Trim().ToLowerInvariant();
            if (!validStates.Contains(state))
                return BadRequest(new { error = "El estado de seguimiento no es válido." });

            try
            {
                var updated = await _repo.ActualizarEstadoConvocatoriaAsync(
                    request.Folio,
                    state,
                    GetCurrentUserName());
                return updated
                    ? Ok(new { success = true, folio = request.Folio, decision = state })
                    : NotFound(new { error = "No se encontró el proyecto." });
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error actualizando el seguimiento de {Folio}.", request.Folio);
                return StatusCode(500, new { error = "No fue posible guardar el estado en la base de datos." });
            }
        }

        [HttpPost("ProyectosPrivados/Api/CarteraConvocatoria/Comentarios")]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> ApiAgregarComentarioConvocatoria(
            [FromBody] AgregarComentarioConvocatoriaRequest request)
        {
            if (!ModelState.IsValid || request == null)
                return BadRequest(new { error = "Proyecto, sesión, fecha y comentario son obligatorios." });

            try
            {
                var note = await _repo.AgregarComentarioConvocatoriaAsync(
                    request,
                    GetCurrentUserName());
                return note == null
                    ? NotFound(new { error = "No se encontró el proyecto." })
                    : Ok(new { success = true, note });
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error agregando comentario a {Folio}.", request.Folio);
                return StatusCode(500, new { error = "No fue posible guardar el comentario en la base de datos." });
            }
        }

        [HttpPut("ProyectosPrivados/Api/CarteraConvocatoria/Prioridad")]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> ApiActualizarPrioridadConvocatoria(
            [FromBody] ActualizarPrioridadConvocatoriaRequest request)
        {
            if (!ModelState.IsValid || request == null)
                return BadRequest(new { error = "Folio y prioridad entre 1 y 4 son obligatorios." });

            try
            {
                var updated = await _repo.ActualizarPrioridadConvocatoriaAsync(
                    request.Folio,
                    request.Prioridad,
                    GetCurrentUserName());
                return updated
                    ? Ok(new { success = true })
                    : NotFound(new { error = "No se encontró el proyecto." });
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error actualizando prioridad de {Folio}.", request.Folio);
                return StatusCode(500, new { error = "No fue posible guardar la prioridad." });
            }
        }

        [HttpPost("ProyectosPrivados/Api/CarteraConvocatoria/Proyectos")]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> ApiCrearProyectoConvocatoria(
            [FromBody] CrearProyectoConvocatoriaRequest request)
        {
            if (!ModelState.IsValid || request == null)
                return BadRequest(new { error = "Revise los datos obligatorios del proyecto." });

            if (request.Tipo != "Estratégico" && request.Tipo != "Particular 2")
                return BadRequest(new { error = "El tipo de proyecto no es válido." });

            try
            {
                var id = await _repo.CrearProyectoConvocatoriaAsync(request, GetCurrentUserName());
                return Ok(new { success = true, projectId = id });
            }
            catch (Microsoft.Data.SqlClient.SqlException ex) when (ex.Number == 2601 || ex.Number == 2627)
            {
                return Conflict(new { error = "Ya existe un proyecto con ese folio." });
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error creando proyecto de convocatoria {Folio}.", request.Folio);
                return StatusCode(500, new { error = "No fue posible guardar el proyecto en la base de datos." });
            }
        }

        private async Task SincronizarCarteraConvocatoriaInicialAsync()
        {
            var path = Path.Combine(
                _environment.WebRootPath,
                "data",
                "cartera-convocatoria-20260805.json");

            try
            {
                var json = await System.IO.File.ReadAllTextAsync(path);
                var seed = JsonConvert.DeserializeObject<CarteraConvocatoriaSeed>(json);
                if (seed != null)
                    await _repo.SincronizarCarteraConvocatoriaAsync(seed, GetCurrentUserName());
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "No fue posible sincronizar la fotografía inicial de la cartera desde {Path}.", path);
                throw;
            }
        }

        // ── View: Energía Limpia ───────────────────────────────────────────
        [HttpGet]
        public IActionResult EnergiaLimpia()
        {
            if (!TieneAccesoEquipoDireccion(out var redireccion)) return redireccion;

            ViewData["HeaderViewModel"] = new HeaderViewModel
            {
                Title = "Energía Limpia",
                IconPath = "proyecto.png",
                Description = "Seguimiento de energía limpia del sector energético nacional.",
                Section = "Seguimiento de proyectos",
                ModuleInfo = JsonConvert.SerializeObject(new
                {
                    title = "Energía Limpia — SENER",
                    description = "Reporte interactivo de energía limpia del sector energético nacional.",
                    functionality = "Visualización del reporte dinámico e interactivo de energía limpia.",
                    stage = "Consulta y Seguimiento",
                    roles = new[]
                    {
                        new { icon = "eye", text = "Usuarios Autorizados: Consulta y visualización del reporte." }
                    },
                    manualUrl = string.Empty
                })
            };
            return View();
        }

        // ── View: Producción Energética Anual ──────────────────────────────
        [HttpGet]
        public IActionResult ProduccionEnergetica()
        {
            if (!TieneAccesoEquipoDireccion(out var redireccion)) return redireccion;

            ViewData["HeaderViewModel"] = new HeaderViewModel
            {
                Title = "Producción Energética Anual",
                IconPath = "proyecto.png",
                Description = "Seguimiento de la producción energética anual nacional.",
                Section = "Seguimiento de proyectos",
                ModuleInfo = JsonConvert.SerializeObject(new
                {
                    title = "Producción Energética Anual — SENER",
                    description = "Reporte interactivo de la producción energética anual.",
                    functionality = "Visualización del reporte dinámico e interactivo de producción energética.",
                    stage = "Consulta y Seguimiento",
                    roles = new[]
                    {
                        new { icon = "eye", text = "Usuarios Autorizados: Consulta y visualización del reporte." }
                    },
                    manualUrl = string.Empty
                })
            };
            return View();
        }

        // ── View: Evolución Prevalencia (Generación GWh) ───────────────────
        [HttpGet]
        public IActionResult EvolucionPrevalencia()
        {
            if (!TieneAccesoEquipoDireccion(out var redireccion)) return redireccion;

            ViewData["HeaderViewModel"] = new HeaderViewModel
            {
                Title = "Evolución Prevalencia",
                IconPath = "proyecto.png",
                Description = "Evolución y prevalencia de la generación eléctrica (GWh).",
                Section = "Seguimiento de proyectos",
                ModuleInfo = JsonConvert.SerializeObject(new
                {
                    title = "Evolución Prevalencia — Generación GWh",
                    description = "Reporte interactivo de la evolución y prevalencia de la generación eléctrica en GWh.",
                    functionality = "Visualización del reporte dinámico e interactivo de generación en GWh.",
                    stage = "Consulta y Seguimiento",
                    roles = new[]
                    {
                        new { icon = "eye", text = "Usuarios Autorizados: Consulta y visualización del reporte." }
                    },
                    manualUrl = string.Empty
                })
            };
            return View();
        }

        // ── Acceso restringido al equipo de Dirección (Javier, Claudia, Raúl, Nahúm) ──
        private bool TieneAccesoEquipoDireccion(out IActionResult redireccion)
        {
            redireccion = null;
            var perfilUsuarioJson = HttpContext.Session.GetString("PerfilUsuario");
            if (string.IsNullOrEmpty(perfilUsuarioJson))
            {
                redireccion = RedirectToAction("SesionExpirada", "Acceso");
                return false;
            }

            var perfil = JsonConvert.DeserializeObject<PerfilUsuario>(perfilUsuarioJson);
            if (perfil == null)
            {
                redireccion = RedirectToAction("SesionExpirada", "Acceso");
                return false;
            }

            // Estrictamente IdUsuario in (1: Javier, 86: Claudia, 87: Raúl, 89: Nahúm)
            bool hasAccess = perfil.IdUsuario == "1" ||
                             perfil.IdUsuario == "86" ||
                             perfil.IdUsuario == "87" ||
                             perfil.IdUsuario == "89";

            if (!hasAccess)
            {
                redireccion = RedirectToAction("Index", "Home");
                return false;
            }

            return true;
        }

        // ── View: Programa Vinculante 2026 — 2040 ──────────────────────────
        [HttpGet]
        public IActionResult ProgramaVinculante()
        {
            var perfilUsuarioJson = HttpContext.Session.GetString("PerfilUsuario");
            if (string.IsNullOrEmpty(perfilUsuarioJson))
            {
                return RedirectToAction("SesionExpirada", "Acceso");
            }

            var perfil = JsonConvert.DeserializeObject<PerfilUsuario>(perfilUsuarioJson);
            if (perfil == null)
            {
                return RedirectToAction("SesionExpirada", "Acceso");
            }

            // Authorization: Rol_ID == 1 OR IdUsuario in (1, 86, 87, 89)
            // 1: Javier, 86: Claudia, 87: Raúl, 89: Nahúm
            bool hasAccess = perfil.Rol_ID == "1" ||
                             perfil.IdUsuario == "1" ||
                             perfil.IdUsuario == "86" ||
                             perfil.IdUsuario == "87" ||
                             perfil.IdUsuario == "89";

            if (!hasAccess)
            {
                var seccionesUsuarioJson = HttpContext.Session.GetString("SeccionesUsuario");
                bool tieneGestor = false;
                if (!string.IsNullOrEmpty(seccionesUsuarioJson))
                {
                    try
                    {
                        var seccionesUsuario = JsonConvert.DeserializeObject<List<SeccionSNIER>>(seccionesUsuarioJson);
                        tieneGestor = seccionesUsuario?
                            .SelectMany(s => s.Modulos)
                            .Any(m => string.Equals(m.Controller, "Gestor", StringComparison.OrdinalIgnoreCase)) ?? false;
                    }
                    catch (Exception)
                    {
                        // Fallback in case of deserialization issues
                    }
                }
                return RedirectToAction("Index", tieneGestor ? "Gestor" : "Home");
            }

            ViewData["HeaderViewModel"] = new HeaderViewModel
            {
                Title = "Programa Vinculante 2026 — 2040",
                IconPath = "proyecto.png",
                Description = "Programa Vinculante 2026 — 2040 - Instalación y Retiro de Centrales Eléctricas.",
                Section = "Seguimiento de proyectos",
                ModuleInfo = JsonConvert.SerializeObject(new
                {
                    title = "Programa Vinculante 2026 — 2040",
                    description = "Visualización del Programa Vinculante 2026 — 2040: Instalación y Retiro de Centrales Eléctricas.",
                    functionality = "Reporte interactivo para el monitoreo de la planeación y retiro de centrales de generación del Sistema Eléctrico Nacional.",
                    stage = "Planeación y Seguimiento",
                    highlights = new[]
                    {
                        "Seguimiento vinculante de adiciones de capacidad.",
                        "Programación de retiros de centrales obsoletas o ineficientes.",
                        "Monitoreo estratégico de la matriz de generación."
                    },
                    roles = new[]
                    {
                        new { icon = "shield-check", text = "Administradores y Directivos: Consulta estratégica y toma de decisiones." },
                        new { icon = "user-shield", text = "Usuarios Especiales: Acceso para consulta técnica del programa." }
                    },
                    order = new { step = 4, description = "Programa Vinculante 2026-2040" },
                    context = "Integración con el visualizador oficial del Programa Vinculante.",
                    manualUrl = string.Empty
                })
            };

            return View();
        }

        // ── View: Grupo de Atención Técnica de Proyectos Mixtos ─────────────
        [HttpGet]
        public IActionResult GrupoAtencionTecnica()
        {
            var perfilUsuarioJson = HttpContext.Session.GetString("PerfilUsuario");
            if (string.IsNullOrEmpty(perfilUsuarioJson))
            {
                return RedirectToAction("SesionExpirada", "Acceso");
            }

            var perfil = JsonConvert.DeserializeObject<PerfilUsuario>(perfilUsuarioJson);
            if (perfil == null)
            {
                return RedirectToAction("SesionExpirada", "Acceso");
            }

            // Authorization: Rol_ID == 1 OR IdUsuario in (1, 86, 87, 89)
            // 1: Javier, 86: Claudia, 87: Raúl, 89: Nahúm
            bool hasAccess = perfil.Rol_ID == "1" ||
                             perfil.IdUsuario == "1" ||
                             perfil.IdUsuario == "86" ||
                             perfil.IdUsuario == "87" ||
                             perfil.IdUsuario == "89";

            if (!hasAccess)
            {
                var seccionesUsuarioJson = HttpContext.Session.GetString("SeccionesUsuario");
                bool tieneGestor = false;
                if (!string.IsNullOrEmpty(seccionesUsuarioJson))
                {
                    try
                    {
                        var seccionesUsuario = JsonConvert.DeserializeObject<List<SeccionSNIER>>(seccionesUsuarioJson);
                        tieneGestor = seccionesUsuario?
                            .SelectMany(s => s.Modulos)
                            .Any(m => string.Equals(m.Controller, "Gestor", StringComparison.OrdinalIgnoreCase)) ?? false;
                    }
                    catch (Exception)
                    {
                        // Fallback in case of deserialization issues
                    }
                }
                return RedirectToAction("Index", tieneGestor ? "Gestor" : "Home");
            }

            ViewData["HeaderViewModel"] = new HeaderViewModel
            {
                Title = "Grupo de Atención Técnica de Proyectos Mixtos",
                IconPath = "proyecto.png",
                Description = "Grupo de Atención Técnica de Proyectos Mixtos - Seguimiento e Integración.",
                Section = "Seguimiento de proyectos",
                ModuleInfo = JsonConvert.SerializeObject(new
                {
                    title = "Grupo de Atención Técnica (GAT Mixto)",
                    description = "Visualización de la bitácora y reporte de seguimiento del Grupo de Atención Técnica para proyectos mixtos.",
                    functionality = "Reporte dinámico e interactivo de los acuerdos, avances y estatus del GAT Mixto de proyectos de infraestructura.",
                    stage = "Reunión y Acuerdos",
                    highlights = new[]
                    {
                        "Seguimiento técnico a proyectos de coinversión y desarrollo mixto.",
                        "Estatus de acuerdos tomados en las sesiones del GAT.",
                        "Trazabilidad de la documentación y compromisos vinculados."
                    },
                    roles = new[]
                    {
                        new { icon = "shield-check", text = "Administradores y Directivos: Consulta general del tablero y acuerdos." },
                        new { icon = "users", text = "Integrantes del GAT: Consulta y seguimiento de compromisos técnicos." }
                    },
                    order = new { step = 5, description = "Grupo de Atención Técnica Mixtos" },
                    context = "Integración con el visualizador del reporte general de seguimiento GAT Mixto.",
                    manualUrl = string.Empty
                })
            };

            return View();
        }


        // ── CRUD Views ───────────────────────────────────────────────────────
        [HttpGet]
        public async Task<IActionResult> Crear()
        {
            ViewData["HeaderViewModel"] = BuildHeader("Registrar Nuevo Proyecto", "proyecto.png", "Dar de alta un nuevo proyecto de generación o almacenamiento privado en el sistema.");
            
            await CargarCatalogosParaEdicion();
            return View(new ProyectoForm());
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Crear(ProyectoForm form)
        {
            if (!ModelState.IsValid)
            {
                await CargarCatalogosParaEdicion();
                return View(form);
            }

            try
            {
                var id = await _repo.CrearProyectoAsync(form, GetCurrentUserName());
                return RedirectToAction(nameof(Detalle), new { id });
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error creando proyecto.");
                ModelState.AddModelError("", "No fue posible crear el proyecto. Verifique los datos.");
                await CargarCatalogosParaEdicion();
                return View(form);
            }
        }

        [HttpGet]
        public async Task<IActionResult> Editar(int id)
        {
            var proj = await _repo.ObtenerProyectoPorIdAsync(id);
            if (proj == null) return NotFound();

            var form = new ProyectoForm
            {
                ProyectoId = proj.ProyectoId,
                Nombre = proj.Nombre,
                Status = proj.Status,
                EmpresaId = proj.EmpresaId,
                Promovente = proj.Promovente,
                GrupoEconomico = proj.GrupoEconomico,
                Origen = proj.Origen,
                Anio = proj.Anio,
                AdicionesSustituciones = proj.AdicionesSustituciones,
                ContratoUnidad = proj.ContratoUnidad,
                Tipo = proj.Tipo,
                Renovable = proj.Renovable,
                CapacidadMW = proj.CapacidadMW,
                Mes = proj.Mes,
                GerenciaControl = proj.GerenciaControl,
                RegionTransmision = proj.RegionTransmision,
                EntidadFederativa = proj.EntidadFederativa,
                Municipio = proj.Municipio,
                Longitud = proj.Longitud,
                Latitud = proj.Latitud,
                Firmes = proj.Firmes,
                PorcentajeConstruccion = proj.PorcentajeConstruccion,
                FolioEvIS_MISSE = proj.FolioEvIS_MISSE,
                StatusEvIS_MISSE = proj.StatusEvIS_MISSE,
                StatusCPLI = proj.StatusCPLI,
                ConflictosSociales = proj.ConflictosSociales,
                NumeroPermiso = proj.NumeroPermiso,
                FechaInicioObras = proj.FechaInicioObras,
                FechaTerminacionObras = proj.FechaTerminacionObras,
                FechaEntradaOperacion = proj.FechaEntradaOperacion,
                EstadoProgramaObras = proj.EstadoProgramaObras,
                TramiteCNE = proj.TramiteCNE,
                ObservacionesCNE = proj.ObservacionesCNE,
                Categoria = proj.Categoria,
                InteresadaEnContinuar = proj.InteresadaEnContinuar,
                ObservacionesUEVISPI = proj.ObservacionesUEVISPI,
                ResumenCaso = proj.ResumenCaso,
                PropuestaAtencion = proj.PropuestaAtencion,
                RequiereAlmacenamiento = proj.RequiereAlmacenamiento,
                SiguientesPasos = proj.SiguientesPasos,
                ClasificacionId = proj.ClasificacionId,
                PrioridadId = proj.PrioridadId,
                SemaforoId = proj.SemaforoId,
                RazonesBreves = proj.RazonesBreves,
                TramitesSemarnat = proj.TramitesSemarnat,
                EstatusSemarnat = proj.EstatusSemarnat,
                ObservacionesSemarnat = proj.ObservacionesSemarnat,
                Fuente = proj.Fuente,
                SemaforoPPT = proj.SemaforoPPT,
                FechaUltimaActualizacion = proj.FechaUltimaActualizacion,
                FuenteUltimaActualizacion = proj.FuenteUltimaActualizacion
            };

            ViewData["HeaderViewModel"] = BuildHeader($"Editar Proyecto: {proj.Nombre}", "proyecto.png", "Modificar la ficha de información técnica y regulatoria del proyecto.");
            await CargarCatalogosParaEdicion();
            return View(form);
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Editar(int id, ProyectoForm form)
        {
            form.ProyectoId = id;
            if (!ModelState.IsValid)
            {
                await CargarCatalogosParaEdicion();
                return View(form);
            }

            try
            {
                var ok = await _repo.ActualizarProyectoAsync(form, GetCurrentUserName());
                if (ok)
                    return RedirectToAction(nameof(Detalle), new { id });
                
                ModelState.AddModelError("", "No se pudo actualizar el proyecto. Es posible que ya no exista.");
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error editando proyecto {Id}.", id);
                ModelState.AddModelError("", "Error interno al guardar los cambios.");
            }

            await CargarCatalogosParaEdicion();
            return View(form);
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Eliminar(int id)
        {
            try
            {
                var ok = await _repo.EliminarProyectoAsync(id);
                if (ok) return Ok(new { success = true });
                return NotFound(new { error = "El proyecto no fue encontrado." });
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error eliminando proyecto {Id}.", id);
                return StatusCode(500, new { error = "Error interno al eliminar el proyecto." });
            }
        }

        public sealed class ActualizarEstatusAccionRequest
        {
            public string Estatus { get; set; } = string.Empty;
            public string Comentarios { get; set; } = string.Empty;
        }

        public sealed class CrearAccionRequest
        {
            public int ProyectoId { get; set; }
            public string Titulo { get; set; } = string.Empty;
            public string Descripcion { get; set; } = string.Empty;
            public string ResponsableId { get; set; } = string.Empty;
            public string ResponsableNombre { get; set; } = string.Empty;
            public DateTime? FechaCompromiso { get; set; }
        }

        // ── API: Endpoints JSON para Frontend ────────────────────────────────
        [HttpGet("ProyectosPrivados/Api/Proyectos")]
        public async Task<IActionResult> ApiProyectos([FromQuery] string buscar = null, [FromQuery] string tecnologia = null, [FromQuery] int? clasificacionId = null, [FromQuery] int? prioridadId = null, [FromQuery] int? semaforoId = null)
        {
            try
            {
                var list = await _repo.ObtenerProyectosAsync(buscar, tecnologia, clasificacionId, prioridadId, semaforoId);
                return Json(list, new System.Text.Json.JsonSerializerOptions { PropertyNamingPolicy = null });
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error obteniendo API Proyectos.");
                return StatusCode(500, new { error = "Error al consultar los proyectos." });
            }
        }

        [HttpGet("ProyectosPrivados/Api/Proyectos/{id:int}")]
        public async Task<IActionResult> ApiProyecto(int id)
        {
            var proj = await _repo.ObtenerProyectoPorIdAsync(id);
            if (proj == null) return NotFound();
            return Json(proj, new System.Text.Json.JsonSerializerOptions { PropertyNamingPolicy = null });
        }

        [HttpGet("ProyectosPrivados/Api/Acciones")]
        public async Task<IActionResult> ApiAcciones([FromQuery] int? proyectoId = null, [FromQuery] string estatus = null, [FromQuery] int? semaforoId = null)
        {
            try
            {
                var list = await _repo.ObtenerAccionesSeguimientoAsync(proyectoId, estatus, semaforoId);
                return Json(list, new System.Text.Json.JsonSerializerOptions { PropertyNamingPolicy = null });
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error obteniendo API Acciones.");
                return StatusCode(500, new { error = "Error al consultar las acciones." });
            }
        }

        [HttpPut("ProyectosPrivados/Api/Acciones/{id:int}/Estatus")]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> ApiActualizarEstatusAccion(int id, [FromBody] ActualizarEstatusAccionRequest body)
        {
            try
            {
                if (body is null || string.IsNullOrWhiteSpace(body.Estatus))
                {
                    return BadRequest(new { error = "El estatus es obligatorio." });
                }

                var ok = await _repo.ActualizarEstatusAccionAsync(id, body.Estatus.Trim(), body.Comentarios?.Trim(), GetCurrentUserName());
                if (ok) return Ok(new { success = true });
                return BadRequest(new { error = "No fue posible actualizar la acción." });
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error actualizando estatus de acción {Id}.", id);
                return StatusCode(500, new { error = "Error interno al actualizar la acción." });
            }
        }

        [HttpPost("ProyectosPrivados/Api/Acciones")]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> ApiCrearAccion([FromBody] CrearAccionRequest body)
        {
            try
            {
                if (body is null || body.ProyectoId <= 0 || string.IsNullOrWhiteSpace(body.Titulo))
                {
                    return BadRequest(new { error = "Proyecto y título son obligatorios." });
                }

                var id = await _repo.CrearAccionAsync(
                    body.ProyectoId,
                    body.Titulo.Trim(),
                    body.Descripcion?.Trim(),
                    body.ResponsableNombre?.Trim(),
                    body.ResponsableId,
                    body.FechaCompromiso,
                    GetCurrentUserName());

                return Json(new { success = true, id });
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error creando acción.");
                return StatusCode(500, new { error = "Error interno al registrar la acción." });
            }
        }

        // ── Helpers ──────────────────────────────────────────────────────────
        private string GetCurrentUserName()
        {
            var perfilUsuarioJson = HttpContext.Session.GetString("PerfilUsuario");
            if (!string.IsNullOrEmpty(perfilUsuarioJson))
            {
                var perfil = JsonConvert.DeserializeObject<PerfilUsuario>(perfilUsuarioJson);
                return perfil?.Nombre ?? "Sistema";
            }
            return User.Identity?.Name ?? "Sistema";
        }

        // ── View: Resolución de Conflictos ───────────────────────────────────
        [HttpGet]
        public async Task<IActionResult> Conflictos()
        {
            ViewData["HeaderViewModel"] = BuildHeader("Bandeja de Resolución de Conflictos", "proyecto.png", "Revisar y resolver manualmente las coincidencias dudosas de proyectos detectadas durante la importación.");
            var list = await _ingestService.ObtenerConflictosVUPEAsync();
            return View(list);
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> ResolverConflicto([FromBody] ConflictResolutionActionDto model)
        {
            if (model == null) return BadRequest("Datos inválidos.");

            try
            {
                if (string.Equals(model.Accion, "FUSIONAR", StringComparison.OrdinalIgnoreCase))
                {
                    if (!model.ProyectoCanonicoId.HasValue) return BadRequest("Debe especificar el proyecto canónico.");
                    await _ingestService.ResolverConflictoFusionarAsync(model.StagingId, model.ProyectoCanonicoId.Value, GetCurrentUserName());
                }
                else if (string.Equals(model.Accion, "NUEVO", StringComparison.OrdinalIgnoreCase))
                {
                    await _ingestService.ResolverConflictoNuevoAsync(model.StagingId, GetCurrentUserName());
                }
                else
                {
                    return BadRequest("Acción no válida.");
                }

                return Json(new { success = true });
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error al resolver conflicto {StagingId}.", model.StagingId);
                return StatusCode(500, new { error = $"Error al resolver conflicto: {ex.Message}" });
            }
        }

        // ── View: Importar Excel ─────────────────────────────────────────────
        [HttpGet]
        public IActionResult Importar()
        {
            ViewData["HeaderViewModel"] = BuildHeader("Importar Consolidado Excel", "proyecto.png", "Cargar hojas del archivo Excel consolidado y ejecutar reglas de deduplicación y homologación.");
            return View();
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Importar(IFormFile file)
        {
            if (file == null || file.Length == 0)
            {
                ModelState.AddModelError("", "Por favor, seleccione un archivo Excel válido.");
                return View();
            }

            try
            {
                var result = await _ingestService.IngestarVUPEWorksheetAsync(file.OpenReadStream(), file.FileName, GetCurrentUserName());
                return View("ResultadoImportacion", result);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error al importar Excel.");
                ModelState.AddModelError("", $"Error al procesar el archivo: {ex.Message}");
                return View();
            }
        }

        private async Task CargarCatalogosParaEdicion()
        {
            ViewBag.Empresas = await _repo.ObtenerEmpresasAsync();
            ViewBag.Clasificaciones = await _repo.ObtenerCatClasificacionAsync();
            ViewBag.Prioridades = await _repo.ObtenerCatPrioridadAsync();
            ViewBag.Semaforos = await _repo.ObtenerCatSemaforoAsync();
            ViewBag.Tecnologias = await _repo.ObtenerCatTecnologiaAsync();
        }

        private HeaderViewModel BuildHeader(string title, string iconPath, string desc) => new()
        {
            Title = title,
            IconPath = iconPath,
            Description = desc,
            Section = "Seguimiento de proyectos",
            ModuleInfo = JsonConvert.SerializeObject(new
            {
                title = "Proyectos Privados Firmes DGMESNIE",
                description = "Módulo estratégico para registrar, dar seguimiento regulatorio y auditar la evolución de la cartera de proyectos de generación y almacenamiento privados firmes.",
                functionality = "Ficha de control de 49 columnas, bitácora de reuniones (minutas), tracking de compromisos por funcionario con semáforo y trazabilidad completa de cambios con foto histórica instantánea.",
                stage = "Control Directivo",
                highlights = new[]
                {
                    "Comparador de campos integrado para auditoría de trazabilidad.",
                    "Snapshot JSON histórico completo por actualización.",
                    "Panel Kanban interactivo para compromisos operativos.",
                    "Soporte integrado para SharePoint."
                },
                roles = new[]
                {
                    new { icon = "shield-check", text = "Directores Generales: Consulta ejecutiva y auditoría." },
                    new { icon = "user-edit", text = "Funcionarios y gestores: Captura de minutas, edición y tareas." }
                },
                order = new { step = 2, description = "Seguimiento regulatorio de proyectos privados firmes" },
                context = "Mapeado contra catálogos nacionales e institucionalmente adaptado a la Guía de Estilos SENER 2025.",
                manualUrl = string.Empty
            })
        };
    }
}
