using Microsoft.AspNetCore.Mvc;
using NSIE.Models;
using NSIE.Servicios;
using Newtonsoft.Json;

namespace NSIE.Controllers
{
    [ServiceFilter(typeof(ValidacionInputFiltro))]
    [AutorizacionFiltro]
    public class InformePormenorizadoController : Controller
    {
        private readonly IRepositorioInformePormenorizado _repositorio;
        private readonly InformePormenorizadoImportService _importService;
        private readonly IWebHostEnvironment _environment;

        public InformePormenorizadoController(
            IRepositorioInformePormenorizado repositorio,
            InformePormenorizadoImportService importService,
            IWebHostEnvironment environment)
        {
            _repositorio = repositorio;
            _importService = importService;
            _environment = environment;
        }

        public async Task<IActionResult> Index(string busqueda = null, string etapa = null, string tipoFinanciamiento = null, string universo = null)
        {
            var registrosTask = _repositorio.ObtenerAsync(busqueda, etapa, tipoFinanciamiento, universo);
            var etapasTask = _repositorio.ObtenerEtapasAsync();
            var financiamientosTask = _repositorio.ObtenerTiposFinanciamientoAsync();
            var universosTask = _repositorio.ObtenerUniversosAsync();

            await Task.WhenAll(registrosTask, etapasTask, financiamientosTask, universosTask);

            var registros = registrosTask.Result;
            var model = new InformePormenorizadoViewModel
            {
                Header = BuildHeader(),
                Busqueda = busqueda,
                Etapa = etapa,
                TipoFinanciamiento = tipoFinanciamiento,
                Universo = universo,
                Registros = registros,
                EtapasDisponibles = etapasTask.Result,
                TiposFinanciamientoDisponibles = financiamientosTask.Result,
                UniversosDisponibles = universosTask.Result,
                TotalRegistros = registros.Count,
                MontoTotalMdp = registros.Sum(x => x.MontoProyectoMdp ?? 0),
                AvancePromedio = registros.Count == 0 ? 0 : Math.Round(registros.Where(x => x.PorcentajeAvanceEjecucion.HasValue).Select(x => x.PorcentajeAvanceEjecucion ?? 0).DefaultIfEmpty(0).Average(), 1),
                ProyectosOperacion = registros.Count(x => (x.EtapaProyecto ?? string.Empty).Contains("Operación", StringComparison.OrdinalIgnoreCase)),
                ProyectosPriorizados = registros.Count(x => string.Equals(x.UniversoPresentacionPresidencia, "Sí", StringComparison.OrdinalIgnoreCase))
            };

            model.ResumenEtapas = registros
                .GroupBy(x => string.IsNullOrWhiteSpace(x.EtapaProyecto) ? "Sin etapa" : x.EtapaProyecto.Trim())
                .OrderByDescending(group => group.Count())
                .Take(6)
                .Select(group => new InformePormenorizadoResumenItem
                {
                    Etiqueta = group.Key,
                    Total = group.Count(),
                    Monto = group.Sum(x => x.MontoProyectoMdp ?? 0)
                })
                .ToList();

            model.ResumenFinanciamiento = registros
                .GroupBy(x => string.IsNullOrWhiteSpace(x.TipoFinanciamiento) ? "Sin tipo" : x.TipoFinanciamiento.Trim())
                .OrderByDescending(group => group.Sum(x => x.MontoProyectoMdp ?? 0))
                .Take(4)
                .Select(group => new InformePormenorizadoResumenItem
                {
                    Etiqueta = group.Key,
                    Total = group.Count(),
                    Monto = group.Sum(x => x.MontoProyectoMdp ?? 0)
                })
                .ToList();

            return View(model);
        }

        public IActionResult Crear()
        {
            PrepareForm("Nuevo proyecto de modernización", "Guardar registro");
            return View(new ProyectoModernizacionRegistro { Activo = true });
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Crear(ProyectoModernizacionRegistro registro)
        {
            if (!ModelState.IsValid)
            {
                PrepareForm("Nuevo proyecto de modernización", "Guardar registro");
                return View(registro);
            }

            await _repositorio.CrearAsync(registro);
            TempData["SuccessMessage"] = "El proyecto se registró correctamente.";
            return RedirectToAction(nameof(Index));
        }

        public async Task<IActionResult> Editar(int id)
        {
            var registro = await _repositorio.ObtenerPorIdAsync(id);
            if (registro == null)
            {
                return NotFound();
            }

            PrepareForm("Editar proyecto de modernización", "Guardar cambios");
            return View(registro);
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Editar(ProyectoModernizacionRegistro registro)
        {
            if (!ModelState.IsValid)
            {
                PrepareForm("Editar proyecto de modernización", "Guardar cambios");
                return View(registro);
            }

            await _repositorio.ActualizarAsync(registro);
            TempData["SuccessMessage"] = "El proyecto se actualizó correctamente.";
            return RedirectToAction(nameof(Index));
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Eliminar(int id)
        {
            await _repositorio.EliminarAsync(id);
            TempData["SuccessMessage"] = "El proyecto se eliminó del informe.";
            return RedirectToAction(nameof(Index));
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> ImportarBaseOficial()
        {
            try
            {
                var registros = await _importService.LeerBaseOficialAsync(_environment.ContentRootPath);
                var totalImportados = await _repositorio.ReemplazarDatosAsync(registros, "INF Pormenorizado Tablas Rev 251125 SENER.xlsx");
                TempData["SuccessMessage"] = $"La base oficial se importó correctamente. Registros cargados: {totalImportados}.";
            }
            catch (Exception ex)
            {
                TempData["ErrorMessage"] = $"No fue posible importar la base oficial: {ex.Message}";
            }

            return RedirectToAction(nameof(Index));
        }

        private void PrepareForm(string title, string actionLabel)
        {
            ViewData["HeaderModel"] = BuildHeader();
            ViewData["FormTitle"] = title;
            ViewData["SubmitLabel"] = actionLabel;
        }

        private static HeaderViewModel BuildHeader()
        {
            var moduleInfo = new
            {
                title = "Informe pormenorizado de proyectos de modernización",
                description = "Seguimiento operativo, presupuestal y técnico del universo de proyectos de modernización con vista ejecutiva y administración detallada.",
                stage = "Planeación y seguimiento",
                order = new { step = 1, description = "Consulta, carga oficial y mantenimiento de proyectos" },
                functionality = "Concentra la base oficial del informe pormenorizado, permite filtrar etapas, revisar atrasos, acciones de mitigación y gestionar altas, bajas y cambios.",
                highlights = new[]
                {
                    "Importación directa desde el Excel oficial",
                    "Tablero con métricas y resumen por etapa",
                    "Tabla detallada con búsqueda y filtros",
                    "CRUD completo sobre el universo de proyectos"
                },
                roles = new[]
                {
                    new { icon = "diagram-project", text = "Equipos de planeación y modernización" },
                    new { icon = "chart-line", text = "Seguimiento ejecutivo de cartera y avance" }
                },
                context = "Usa la importación oficial cuando se actualice el archivo fuente y administra ajustes manuales desde el propio tablero."
            };

            return new HeaderViewModel
            {
                Title = "Informe Pormenorizado",
                IconPath = "proyecto.png",
                Description = "Seguimiento de proyectos de modernización con tablero ejecutivo, detalle técnico y CRUD operativo.",
                Section = "Proyectos de Modernización",
                ModuleInfo = JsonConvert.SerializeObject(moduleInfo)
            };
        }
    }
}