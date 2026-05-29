using Microsoft.AspNetCore.Mvc;
using NSIE.Models;
using NSIE.Servicios;
using Newtonsoft.Json;
using Microsoft.Data.SqlClient;
using System.Security.Claims;

namespace NSIE.Controllers
{
    [ServiceFilter(typeof(ValidacionInputFiltro))]
    [AutorizacionFiltro]
    public class ReunionesController : Controller
    {
        private readonly IRepositorioReuniones _repositorioReuniones;
        private readonly ManualSharePointImportService _manualSharePointImportService;
        private readonly ILogger<ReunionesController> _logger;

        public ReunionesController(
            IRepositorioReuniones repositorioReuniones,
            ManualSharePointImportService manualSharePointImportService,
            ILogger<ReunionesController> logger)
        {
            _repositorioReuniones = repositorioReuniones;
            _manualSharePointImportService = manualSharePointImportService;
            _logger = logger;
        }

        [HttpGet]
        public IActionResult Index()
        {
            return RedirectToAction(nameof(Seguimiento));
        }

        [HttpGet]
        public async Task<IActionResult> Seguimiento()
        {
            return View(await BuildSeguimientoViewModelAsync());
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> CrearAsunto(ReunionesCrearAsuntoInput input)
        {
            if (!ModelState.IsValid)
            {
                TempData["ReunionesError"] = "No fue posible crear el asunto. Revisa los campos obligatorios.";
                return RedirectToAction(nameof(Seguimiento));
            }

            try
            {
                await _repositorioReuniones.CrearAsuntoAsync(input, GetCurrentUserId(), GetCurrentUserName());
                TempData["ReunionesSuccess"] = "Asunto creado correctamente.";
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error al crear asunto de reuniones.");
                TempData["ReunionesError"] = "No fue posible crear el asunto.";
            }

            return RedirectToAction(nameof(Seguimiento));
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> ActualizarEstatus(ReunionesActualizarEstatusInput input)
        {
            if (!ModelState.IsValid)
            {
                TempData["ReunionesError"] = "No fue posible actualizar el estatus del asunto.";
                return RedirectToAction(nameof(Seguimiento));
            }

            try
            {
                await _repositorioReuniones.ActualizarEstatusAsync(input, GetCurrentUserId(), GetCurrentUserName());
                TempData["ReunionesSuccess"] = "Seguimiento actualizado correctamente.";
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error al actualizar estatus de asunto de reuniones.");
                TempData["ReunionesError"] = "No fue posible actualizar el asunto.";
            }

            return RedirectToAction(nameof(Seguimiento));
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> AgregarComentario(ReunionesAgregarComentarioInput input)
        {
            if (!ModelState.IsValid)
            {
                TempData["ReunionesError"] = "No fue posible agregar el comentario.";
                return RedirectToAction(nameof(Seguimiento));
            }

            try
            {
                await _repositorioReuniones.AgregarComentarioAsync(input, GetCurrentUserId(), GetCurrentUserName());
                TempData["ReunionesSuccess"] = "Comentario agregado correctamente.";
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error al agregar comentario de asunto de reuniones.");
                TempData["ReunionesError"] = "No fue posible agregar el comentario.";
            }

            return RedirectToAction(nameof(Seguimiento));
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> MarcarEnvioMonica(ReunionesMarcarEnvioMonicaInput input)
        {
            if (!ModelState.IsValid)
            {
                TempData["ReunionesError"] = "No fue posible marcar el envío a Mónica.";
                return RedirectToAction(nameof(Seguimiento));
            }

            try
            {
                await _repositorioReuniones.MarcarEnvioMonicaAsync(input, GetCurrentUserId(), GetCurrentUserName());
                TempData["ReunionesSuccess"] = "Se marcó el envío a Mónica correctamente.";
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error al marcar envío a Mónica en reuniones.");
                TempData["ReunionesError"] = "No fue posible marcar el envío a Mónica.";
            }

            return RedirectToAction(nameof(Seguimiento));
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> DesactivarAsunto(ReunionesDesactivarAsuntoInput input)
        {
            if (!ModelState.IsValid)
            {
                TempData["ReunionesError"] = "No fue posible desactivar el asunto.";
                return RedirectToAction(nameof(Seguimiento));
            }

            try
            {
                await _repositorioReuniones.DesactivarAsuntoAsync(input.AsuntoId, GetCurrentUserId(), GetCurrentUserName());
                TempData["ReunionesSuccess"] = "Asunto desactivado correctamente.";
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error al desactivar asunto de reuniones.");
                TempData["ReunionesError"] = "No fue posible desactivar el asunto.";
            }

            return RedirectToAction(nameof(Seguimiento));
        }

        [HttpGet]
        public IActionResult ImportarExpedientes()
        {
            return View(BuildViewModel());
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> ImportarExpedientes(IFormFile archivo)
        {
            var viewModel = BuildViewModel();

            try
            {
                var result = await _manualSharePointImportService.ParseAsync(archivo);
                viewModel.IsSuccess = true;
                viewModel.StatusMessage = "Archivo procesado correctamente. Ya puedes revisar la previsualización manual.";
                viewModel.FileName = result.FileName;
                viewModel.SourceType = result.SourceType;
                viewModel.WorksheetName = result.WorksheetName;
                viewModel.TotalRows = result.TotalRows;
                viewModel.PreviewRows = result.PreviewRows;
                viewModel.Columns = result.Columns;
                viewModel.Rows = result.Rows;
                viewModel.Notes = result.Notes;
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error al procesar la carga manual de expedientes desde SharePoint.");
                viewModel.IsSuccess = false;
                viewModel.StatusMessage = ex.Message;
            }

            return View(viewModel);
        }

        private static ManualSharePointImportViewModel BuildViewModel()
        {
            return new ManualSharePointImportViewModel
            {
                Header = new HeaderViewModel
                {
                    Title = "Carga manual de expedientes",
                    IconPath = "database.png",
                    Description = "Carga de respaldo para validar Excel o CSV de SharePoint cuando sea necesario revisar insumos manuales.",
                    Section = "Reuniones y expedientes",
                    ModuleInfo = JsonConvert.SerializeObject(new
                    {
                        title = "Carga manual de expedientes",
                        description = "Vista de respaldo para inspeccionar archivos exportados desde SharePoint cuando el seguimiento ya vive en el portal pero se requiere validar insumos históricos.",
                        functionality = "Sube un archivo .xlsx o .csv, identifica columnas disponibles y muestra una previsualización inmediata de los registros.",
                        stage = "Respaldo manual",
                        highlights = new[]
                        {
                            "Aceptar archivos exportados desde SharePoint sin usar Graph.",
                            "Previsualizar columnas y filas antes de una carga extraordinaria.",
                            "Mantener un respaldo si hace falta revisar históricos fuera del dashboard principal."
                        },
                        roles = new[]
                        {
                            new { icon = "folder-tree", text = "Equipos operativos que concentran expedientes y reportes." },
                            new { icon = "table", text = "Personal que valida la estructura documental antes de armar el tablero de reuniones." }
                        },
                        order = new { step = 1, description = "Carga manual de insumos documentales" },
                        context = "El flujo principal será el dashboard de asuntos con expediente y liga a carpeta SharePoint. Esta vista queda como herramienta de apoyo.",
                        manualUrl = "#"
                    })
                }
            };
        }

        private async Task<ReunionesSeguimientoViewModel> BuildSeguimientoViewModelAsync()
        {
            try
            {
                var asuntos = await _repositorioReuniones.ObtenerDashboardAsync();

                return new ReunionesSeguimientoViewModel
                {
                    Header = BuildSeguimientoHeader(),
                    Asuntos = asuntos,
                    TotalAsuntos = asuntos.Count,
                    TotalAtendidos = asuntos.Count(EsAtendido),
                    TotalPendientes = asuntos.Count(x => !EsAtendido(x)),
                    TotalPorVencer = asuntos.Count(x => string.Equals(x.Semaforo, "Amarillo", StringComparison.OrdinalIgnoreCase)),
                    TotalVencidos = asuntos.Count(x => string.Equals(x.Semaforo, "Rojo", StringComparison.OrdinalIgnoreCase)),
                    TotalPendientesEnvioMonica = asuntos.Count(x => string.Equals(x.AlertaEnvioMonica, "Pendiente de envío a Mónica", StringComparison.OrdinalIgnoreCase)),
                    Notes = new List<string>
                    {
                        "El tablero ya consume la vista y el procedimiento del esquema dgmesnie para mostrar seguimiento real.",
                        "Cuando la carpeta proviene como clave histórica, el vínculo se construye automáticamente con la ruta base de SharePoint.",
                        "Si existe semáforo manual en la tabla, el dashboard lo respeta por encima del cálculo automático por fechas."
                    }
                };
            }
            catch (SqlException ex)
            {
                _logger.LogError(ex, "Error SQL al cargar el dashboard de reuniones.");
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error inesperado al cargar el dashboard de reuniones.");
            }

            return new ReunionesSeguimientoViewModel
            {
                Header = BuildSeguimientoHeader(),
                Notes = new List<string>
                {
                    "No fue posible cargar el dashboard desde la base de datos.",
                    "Verifica que el script REUNIONES_SEGUIMIENTO_ASUNTOS.sql ya se haya ejecutado en la base activa del portal."
                }
            };
        }

        private static HeaderViewModel BuildSeguimientoHeader()
        {
            return new HeaderViewModel
            {
                Title = "Seguimiento de asuntos",
                IconPath = "proyecto.png",
                Description = "Tablero operativo para controlar reuniones, expedientes y acceso directo a carpetas SharePoint desde un solo lugar.",
                Section = "Reuniones y expedientes",
                ModuleInfo = JsonConvert.SerializeObject(new
                {
                    title = "Seguimiento de asuntos",
                    description = "Concentra el estado operativo de cada asunto, su expediente y la liga a la carpeta documental correspondiente.",
                    functionality = "Permite visualizar semáforo, fechas compromiso, responsables y acceso directo a SharePoint sin depender de importaciones recurrentes.",
                    stage = "Base operativa",
                    highlights = new[]
                    {
                        "Semáforo para detectar atendidos, pendientes y vencidos.",
                        "Columna de expediente para homologar el seguimiento con la carpeta documental.",
                        "Botón directo a SharePoint por cada asunto."
                    },
                    roles = new[]
                    {
                        new { icon = "clipboard-list", text = "Equipos que coordinan seguimiento de asuntos y reuniones." },
                        new { icon = "folder-open", text = "Personal que necesita abrir la carpeta documental del expediente en un clic." }
                    },
                    order = new { step = 1, description = "Control operativo del seguimiento" },
                    context = "El objetivo es que el dashboard viva en el portal y SharePoint quede como repositorio documental vinculado por liga.",
                    manualUrl = string.Empty
                })
            };
        }

        private static bool EsAtendido(ReunionesSeguimientoItem asunto)
        {
            if (string.Equals(asunto.Semaforo, "Verde", StringComparison.OrdinalIgnoreCase))
            {
                return true;
            }

            if (string.Equals(asunto.Estatus, "Atendido", StringComparison.OrdinalIgnoreCase))
            {
                return true;
            }

            return !string.IsNullOrWhiteSpace(asunto.EstadoActual)
                && asunto.EstadoActual.Contains("finalizado", StringComparison.OrdinalIgnoreCase);
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

        private string GetCurrentUserName()
        {
            var perfilJson = HttpContext.Session.GetString("PerfilUsuario");
            if (string.IsNullOrEmpty(perfilJson)) return "Usuario portal";
            try
            {
                var perfil = JsonConvert.DeserializeObject<PerfilUsuario>(perfilJson);
                return perfil?.Nombre ?? "Usuario portal";
            }
            catch
            {
                return "Usuario portal";
            }
        }
    }
}