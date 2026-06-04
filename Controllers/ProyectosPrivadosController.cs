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

        public ProyectosPrivadosController(IRepositorioProyectosPrivados repo, IngestionService ingestService, ILogger<ProyectosPrivadosController> logger)
        {
            _repo = repo;
            _ingestService = ingestService;
            _logger = logger;
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
        public IActionResult SegundaConvocatoria()
        {
            ViewData["HeaderViewModel"] = new HeaderViewModel
            {
                Title = "Segunda Convocatoria",
                IconPath = "proyecto.png",
                Description = "Seguimiento a proyectos registrados para la segunda convocatoria.",
                Section = "Seguimiento de proyectos",
                ModuleInfo = JsonConvert.SerializeObject(new
                {
                    title = "Segunda Convocatoria - Proyectos Particulares",
                    description = "Módulo para el seguimiento a proyectos registrados para la segunda convocatoria.",
                    functionality = "Visualización del reporte dinámico e interactivo de la segunda convocatoria de proyectos particulares.",
                    stage = "Consulta y Seguimiento",
                    highlights = new[]
                    {
                        "Reporte interactivo de proyectos particulares.",
                        "Monitoreo de estado de registros y documentación.",
                        "Acceso directo a la información cargada en el portal."
                    },
                    roles = new[]
                    {
                        new { icon = "eye", text = "Usuarios Autorizados: Consulta y visualización del reporte general." }
                    },
                    order = new { step = 3, description = "Consulta de reporte de segunda convocatoria" },
                    context = "Visualizador integrado con el repositorio de datos de proyectos particulares de la segunda convocatoria.",
                    manualUrl = string.Empty
                })
            };
            return View();
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
        public async Task<IActionResult> ApiActualizarEstatusAccion(int id, [FromBody] dynamic body)
        {
            try
            {
                // Deserialize body to extract status and comments
                string json = body.ToString();
                var data = JsonConvert.DeserializeAnonymousType(json, new { estatus = "", comentarios = "" });

                var ok = await _repo.ActualizarEstatusAccionAsync(id, data.estatus, data.comentarios, GetCurrentUserName());
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
        public async Task<IActionResult> ApiCrearAccion([FromBody] dynamic body)
        {
            try
            {
                string json = body.ToString();
                var data = JsonConvert.DeserializeAnonymousType(json, new {
                    proyectoId = 0,
                    titulo = "",
                    descripcion = "",
                    responsableId = "",
                    responsableNombre = "",
                    fechaCompromiso = (DateTime?)null
                });

                var id = await _repo.CrearAccionAsync(data.proyectoId, data.titulo, data.descripcion, data.responsableNombre, data.responsableId, data.fechaCompromiso, GetCurrentUserName());
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
