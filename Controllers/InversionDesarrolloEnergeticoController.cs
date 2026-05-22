using Microsoft.AspNetCore.Mvc;
using Newtonsoft.Json;
using NSIE.Models;
using NSIE.Servicios;

namespace NSIE.Controllers
{
    [ServiceFilter(typeof(ValidacionInputFiltro))]
    [AutorizacionFiltro]
    public class InversionDesarrolloEnergeticoController : Controller
    {
        private readonly IRepositorioInversionDesarrolloEnergetico _repositorio;
        private readonly PvirseImportService _importService;
        private readonly IWebHostEnvironment _environment;

        public InversionDesarrolloEnergeticoController(
            IRepositorioInversionDesarrolloEnergetico repositorio,
            PvirseImportService importService,
            IWebHostEnvironment environment)
        {
            _repositorio = repositorio;
            _importService = importService;
            _environment = environment;
        }

        public async Task<IActionResult> Index(string entidad = null, string tipoInversion = null)
        {
            await _repositorio.EnsureTableAsync();

            if (!await _repositorio.TieneDatosAsync())
            {
                try
                {
                    var registrosImportados = await _importService.LeerBaseOficialAsync(_environment.ContentRootPath);
                    await _repositorio.ReemplazarDatosAsync(registrosImportados, PvirseImportService.DefaultFileName);
                }
                catch
                {
                }
            }

            var registrosTask = _repositorio.ObtenerAsync(entidad, tipoInversion);
            var entidadesTask = _repositorio.ObtenerEntidadesAsync();
            var tiposTask = _repositorio.ObtenerTiposInversionAsync();
            await Task.WhenAll(registrosTask, entidadesTask, tiposTask);

            var registros = registrosTask.Result;

            var model = new InversionDesarrolloEnergeticoViewModel
            {
                Header = BuildHeader(),
                EntidadSeleccionada = entidad,
                TipoInversionSeleccionado = tipoInversion,
                EntidadesDisponibles = entidadesTask.Result,
                TiposInversionDisponibles = tiposTask.Result,
                Registros = registros,
                TotalRegistros = registros.Count,
                CapacidadTotalMw = registros.Sum(x => x.Mw ?? 0),
                TotalAdiciones = registros.Count(x => (x.AdicionesOSustituciones ?? string.Empty).Contains("Adición", StringComparison.OrdinalIgnoreCase)),
                TotalSustituciones = registros.Count(x => (x.AdicionesOSustituciones ?? string.Empty).Contains("Sustit", StringComparison.OrdinalIgnoreCase) || (x.AdicionesOSustituciones ?? string.Empty).Contains("Retiro", StringComparison.OrdinalIgnoreCase)),
                TotalCfe = registros.Count(x => string.Equals(x.StatusVf, "CFE", StringComparison.OrdinalIgnoreCase)),
                TotalParticulares = registros.Count(x => string.Equals(x.StatusVf, "Particulares", StringComparison.OrdinalIgnoreCase)),
                FechaActualizacion = registros.Any() ? registros.Max(x => x.FechaCarga) : DateTime.Today,
                FuenteDatos = PvirseImportService.DefaultFileName
            };

            return View(model);
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> ImportarBaseOficial()
        {
            try
            {
                var registros = await _importService.LeerBaseOficialAsync(_environment.ContentRootPath);
                var totalImportados = await _repositorio.ReemplazarDatosAsync(registros, PvirseImportService.DefaultFileName);
                TempData["SuccessMessage"] = $"La base PVIRSE se importó correctamente. Registros cargados: {totalImportados}.";
            }
            catch (Exception ex)
            {
                TempData["ErrorMessage"] = $"No fue posible importar la base PVIRSE: {ex.Message}";
            }

            return RedirectToAction(nameof(Index));
        }

        private static HeaderViewModel BuildHeader()
        {
            var moduleInfo = new
            {
                title = "PVIRSE",
                description = "Programa Vinculante para la Instalación y Retiro de Centrales Eléctricas 2026-2040 con consulta por entidad federativa y tipo de inversión.",
                stage = "Planeación ejecutiva",
                order = new { step = 1, description = "Consulta de primera lámina ejecutiva" },
                functionality = "Permite seleccionar una entidad federativa y un tipo de inversión para mostrar el corte real de centrales desde la base PVIRSE.",
                highlights = new[]
                {
                    "Filtro por entidad federativa",
                    "Filtro por tipo de inversión",
                    "Primera lámina simplificada",
                    "Base oficial importable desde Excel"
                },
                roles = new[]
                {
                    new { icon = "chart-pie", text = "Consulta ejecutiva del programa" },
                    new { icon = "map", text = "Lectura por entidad federativa" }
                },
                context = "La base se alimenta desde el archivo PVIRCE2026-2040_SQ.xlsx y se persiste en la tabla dgmesnie.PvirseCentralesElectricas."
            };

            return new HeaderViewModel
            {
                Title = "PVIRSE",
                IconPath = "proyecto.png",
                Description = "Programa Vinculante para la Instalación y Retiro de Centrales Eléctricas 2026-2040.",
                Section = "Primera Lámina por Entidad Federativa",
                ModuleInfo = JsonConvert.SerializeObject(moduleInfo)
            };
        }
    }
}