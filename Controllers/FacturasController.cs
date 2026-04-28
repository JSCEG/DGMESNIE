using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Http;
using NSIE.Models;
using NSIE.Servicios;
using System;
using System.Collections.Generic;
using System.IO;
using System.Threading.Tasks;
using Microsoft.Extensions.Logging;

namespace NSIE.Controllers
{
    [ServiceFilter(typeof(ValidacionInputFiltro))]
    [AutorizacionFiltro]
    public class FacturasController : Controller
    {
        private readonly IRepositorioFacturas _repositorioFacturas;
        private readonly FacturaExtractorService _facturaExtractorService;
        private readonly ILogger<FacturasController> _logger; // Inyectar ILogger
        private const int MaxFiles = 400; // Límite de archivos

        public FacturasController(IRepositorioFacturas repositorioFacturas, FacturaExtractorService facturaExtractorService, ILogger<FacturasController> logger)
        {
            _repositorioFacturas = repositorioFacturas;
            _facturaExtractorService = facturaExtractorService;
            _logger = logger; // Asignar ILogger
        }

        public IActionResult MenuEF()
        {
            return View();
        }

        public IActionResult CargaFactura()
        {
            return View();
        }

        [HttpPost]
        public async Task<IActionResult> Upload(List<IFormFile> files)
        {
            try
            {
                _logger.LogInformation("Iniciando la carga de archivos...");

                if (files == null || files.Count == 0)
                {
                    ModelState.AddModelError("", "Por favor, suba al menos un archivo.");
                    _logger.LogWarning("No se subieron archivos.");
                    return View("CargaFactura", new List<Factura>());
                }

                if (files.Count > MaxFiles)
                {
                    ModelState.AddModelError("", $"No puede subir más de {MaxFiles} archivos a la vez.");
                    _logger.LogWarning($"Se intentó subir más de {MaxFiles} archivos.");
                    return View("CargaFactura", new List<Factura>());
                }

                var facturas = new List<Factura>();
                int idCounter = 1;
                var errores = new List<string>();

                foreach (var file in files)
                {
                    if (file == null || file.Length == 0)
                    {
                        var error = "Uno o más archivos están vacíos o no son válidos.";
                        errores.Add(error);
                        _logger.LogWarning(error);
                        continue;
                    }

                    try
                    {
                        var filePath = Path.GetTempFileName();
                        _logger.LogInformation($"Creando archivo temporal en {filePath} para el archivo {file.FileName}.");

                        using (var stream = new FileStream(filePath, FileMode.Create))
                        {
                            await file.CopyToAsync(stream);
                            _logger.LogInformation($"Archivo {file.FileName} copiado al archivo temporal.");
                        }

                        _logger.LogInformation($"Procesando el archivo {file.FileName}...");

                        var extractedFacturas = _facturaExtractorService.ExtractFromPdf(filePath, file.FileName);
                        _logger.LogInformation($"Archivo {file.FileName} procesado. Se extrajeron {extractedFacturas.Count} facturas.");

                        foreach (var factura in extractedFacturas)
                        {
                            factura.Id = idCounter++;
                            facturas.Add(factura);
                        }

                        System.IO.File.Delete(filePath);
                        _logger.LogInformation($"Archivo temporal {filePath} eliminado.");
                    }
                    catch (Exception ex)
                    {
                        var errorMessage = $"Ocurrió un error al procesar el archivo {file.FileName}: {ex.Message}";
                        errores.Add(errorMessage);
                        _logger.LogError(ex, errorMessage);
                    }
                }

                if (facturas.Count == 0 && errores.Count == 0)
                {
                    var error = "No se extrajeron facturas válidas de los archivos subidos.";
                    ModelState.AddModelError("", error);
                    _logger.LogWarning(error);
                }

                ViewBag.Errores = errores;

                _logger.LogInformation("Carga de archivos completada.");

                return View("CargaFactura", facturas);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error durante la carga de archivos.");
                ModelState.AddModelError("", $"Error durante la carga de archivos: {ex.Message}");
                return View("CargaFactura", new List<Factura>());
            }
        }

    }
}
