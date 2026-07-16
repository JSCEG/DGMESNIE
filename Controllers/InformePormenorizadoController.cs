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

        private const int TamanoPagina = 20;

        public async Task<IActionResult> Index(string busqueda = null, string etapa = null, string tipoFinanciamiento = null, string universo = null, int pagina = 1)
        {
            if (pagina < 1)
            {
                pagina = 1;
            }

            // Todo se resuelve en SQL: proyección ligera paginada + agregados con GROUP BY.
            // Los textos largos por proyecto se cargan bajo demanda vía Detalle(id).
            var registrosTask = _repositorio.ObtenerPaginaAsync(busqueda, etapa, tipoFinanciamiento, universo, pagina, TamanoPagina);
            var agregadosTask = _repositorio.ObtenerAgregadosAsync(busqueda, etapa, tipoFinanciamiento, universo);
            var etapasTask = _repositorio.ObtenerEtapasAsync();
            var financiamientosTask = _repositorio.ObtenerTiposFinanciamientoAsync();
            var universosTask = _repositorio.ObtenerUniversosAsync();

            await Task.WhenAll(registrosTask, agregadosTask, etapasTask, financiamientosTask, universosTask);

            var registros = registrosTask.Result;
            var agregados = agregadosTask.Result;
            var totalFiltrado = registros.FirstOrDefault()?.TotalFiltrado ?? agregados.TotalRegistros;

            // Si la página pedida quedó fuera de rango (p.ej. tras filtrar), regresar a la última válida.
            var totalPaginas = Math.Max(1, (int)Math.Ceiling((double)totalFiltrado / TamanoPagina));
            if (pagina > totalPaginas && totalFiltrado > 0)
            {
                return RedirectToAction(nameof(Index), new { busqueda, etapa, tipoFinanciamiento, universo, pagina = totalPaginas });
            }

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
                Pagina = pagina,
                TamanoPagina = TamanoPagina,
                TotalRegistros = totalFiltrado,
                MontoTotalMdp = agregados.MontoTotalMdp,
                AvancePromedio = agregados.AvancePromedio,
                ProyectosOperacion = agregados.ProyectosOperacion,
                ProyectosPriorizados = agregados.ProyectosPriorizados,
                ResumenEtapas = agregados.ResumenEtapas,
                ResumenFinanciamiento = agregados.ResumenFinanciamiento
            };

            return View(model);
        }

        /// <summary>Detalle largo de un proyecto (textos extensos), cargado bajo demanda al expandir la fila.</summary>
        [HttpGet]
        public async Task<IActionResult> Detalle(int id)
        {
            var registro = await _repositorio.ObtenerPorIdAsync(id);
            if (registro == null)
            {
                return NotFound();
            }

            return Json(new
            {
                tipoFinanciamiento = registro.TipoFinanciamiento,
                anioInstruccion = registro.AnioInstruccion,
                clasificacionSener = registro.ClasificacionSener,
                fechaProgramacionTrimestre = registro.FechaProgramacionTrimestre,
                quincenaPublicacion = registro.QuincenaPublicacion,
                fechaEstimadaInicio = registro.FechaEstimadaInicio,
                feoIndicadaOficioSener = registro.FeoIndicadaOficioSener,
                feoFactible = registro.FeoFactible,
                estadoRealProyecto = registro.EstadoRealProyecto,
                circunstanciasAtrasos = registro.CircunstanciasAtrasos,
                accionesMitigacionCorreccion = registro.AccionesMitigacionCorreccion,
                elementosEquiposAsociados = registro.ElementosEquiposAsociados,
                mva = registro.Mva,
                mvar = registro.Mvar,
                kmC = registro.KmC
            });
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