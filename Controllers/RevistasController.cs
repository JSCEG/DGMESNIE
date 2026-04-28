using Microsoft.AspNetCore.Mvc;
using System.IO;
using System.Linq;
using Microsoft.AspNetCore.Hosting;
   
   [ServiceFilter(typeof(ValidacionInputFiltro))]
    [AutorizacionFiltro]
public class RevistasController : Controller
{
    private readonly IWebHostEnvironment _webHostEnvironment;

    public RevistasController(IWebHostEnvironment webHostEnvironment)
    {
        _webHostEnvironment = webHostEnvironment;
    }
    public IActionResult Menu_Revistas()
    {
        return View();
    }



   public IActionResult MapasConsumoAgricola()
    {
        var imageDirectory = Path.Combine(_webHostEnvironment.WebRootPath, "img/atlas/agricola");

        if (!Directory.Exists(imageDirectory))
        {
            return NotFound("El directorio de imágenes no existe.");
        }

        // Cargar todas las imágenes PNG
        var imageFiles = Directory.GetFiles(imageDirectory, "*.png")
                                  .Select(f => Path.GetFileName(f))
                                  .OrderBy(f => f)
                                  .ToList();

        // Cargar los archivos GIF por año
        var gifFiles = Directory.GetFiles(imageDirectory, "*.gif")
                                .Select(f => Path.GetFileName(f))
                                .OrderBy(f => f)
                                .ToList();

        // Verificar si existen archivos para evitar null
        var firstImage = imageFiles.FirstOrDefault();
        var firstGif = gifFiles.FirstOrDefault();

        // Enviar las listas y la primera imagen/GIF por defecto
        ViewBag.ImageFiles = imageFiles ?? new List<string>();  // Inicializar como lista vacía si no hay imágenes
        ViewBag.GifFiles = gifFiles ?? new List<string>();      // Inicializar como lista vacía si no hay GIFs
        ViewBag.FirstImageFile = firstImage;
        ViewBag.FirstGifFile = firstGif;

        return View();
    }



   public IActionResult MapasConsumoResidencial()
    {
        var imageDirectory = Path.Combine(_webHostEnvironment.WebRootPath, "img/atlas/residencial");

        if (!Directory.Exists(imageDirectory))
        {
            return NotFound("El directorio de imágenes no existe.");
        }

        // Cargar todas las imágenes PNG
        var imageFiles = Directory.GetFiles(imageDirectory, "*.png")
                                  .Select(f => Path.GetFileName(f))
                                  .OrderBy(f => f)
                                  .ToList();

        // Cargar los archivos GIF por año
        var gifFiles = Directory.GetFiles(imageDirectory, "*.gif")
                                .Select(f => Path.GetFileName(f))
                                .OrderBy(f => f)
                                .ToList();

        // Verificar si existen archivos para evitar null
        var firstImage = imageFiles.FirstOrDefault();
        var firstGif = gifFiles.FirstOrDefault();

        // Enviar las listas y la primera imagen/GIF por defecto
        ViewBag.ImageFiles = imageFiles ?? new List<string>();  // Inicializar como lista vacía si no hay imágenes
        ViewBag.GifFiles = gifFiles ?? new List<string>();      // Inicializar como lista vacía si no hay GIFs
        ViewBag.FirstImageFile = firstImage;
        ViewBag.FirstGifFile = firstGif;

        return View();
    }




   public IActionResult MapasConsumoIndustrial()
    {
        var imageDirectory = Path.Combine(_webHostEnvironment.WebRootPath, "img/atlas/industrial");

        if (!Directory.Exists(imageDirectory))
        {
            return NotFound("El directorio de imágenes no existe.");
        }

        // Cargar todas las imágenes PNG
        var imageFiles = Directory.GetFiles(imageDirectory, "*.png")
                                  .Select(f => Path.GetFileName(f))
                                  .OrderBy(f => f)
                                  .ToList();

        // Cargar los archivos GIF por año
        var gifFiles = Directory.GetFiles(imageDirectory, "*.gif")
                                .Select(f => Path.GetFileName(f))
                                .OrderBy(f => f)
                                .ToList();

        // Verificar si existen archivos para evitar null
        var firstImage = imageFiles.FirstOrDefault();
        var firstGif = gifFiles.FirstOrDefault();

        // Enviar las listas y la primera imagen/GIF por defecto
        ViewBag.ImageFiles = imageFiles ?? new List<string>();  // Inicializar como lista vacía si no hay imágenes
        ViewBag.GifFiles = gifFiles ?? new List<string>();      // Inicializar como lista vacía si no hay GIFs
        ViewBag.FirstImageFile = firstImage;
        ViewBag.FirstGifFile = firstGif;

        return View();
    }


   public IActionResult MapasConsumoServicioPublico()
    {
        var imageDirectory = Path.Combine(_webHostEnvironment.WebRootPath, "img/atlas/serviciopublico");

        if (!Directory.Exists(imageDirectory))
        {
            return NotFound("El directorio de imágenes no existe.");
        }

        // Cargar todas las imágenes PNG
        var imageFiles = Directory.GetFiles(imageDirectory, "*.png")
                                  .Select(f => Path.GetFileName(f))
                                  .OrderBy(f => f)
                                  .ToList();

        // Cargar los archivos GIF por año
        var gifFiles = Directory.GetFiles(imageDirectory, "*.gif")
                                .Select(f => Path.GetFileName(f))
                                .OrderBy(f => f)
                                .ToList();

        // Verificar si existen archivos para evitar null
        var firstImage = imageFiles.FirstOrDefault();
        var firstGif = gifFiles.FirstOrDefault();

        // Enviar las listas y la primera imagen/GIF por defecto
        ViewBag.ImageFiles = imageFiles ?? new List<string>();  // Inicializar como lista vacía si no hay imágenes
        ViewBag.GifFiles = gifFiles ?? new List<string>();      // Inicializar como lista vacía si no hay GIFs
        ViewBag.FirstImageFile = firstImage;
        ViewBag.FirstGifFile = firstGif;

        return View();
    }

    public IActionResult MapasConsumoSectorEnergia()
    {
        var imageDirectory = Path.Combine(_webHostEnvironment.WebRootPath, "img/atlas/sectorenergia");

        if (!Directory.Exists(imageDirectory))
        {
            return NotFound("El directorio de imágenes no existe.");
        }

        // Cargar todas las imágenes PNG
        var imageFiles = Directory.GetFiles(imageDirectory, "*.png")
                                  .Select(f => Path.GetFileName(f))
                                  .OrderBy(f => f)
                                  .ToList();

        // Cargar los archivos GIF por año
        var gifFiles = Directory.GetFiles(imageDirectory, "*.gif")
                                .Select(f => Path.GetFileName(f))
                                .OrderBy(f => f)
                                .ToList();

        // Verificar si existen archivos para evitar null
        var firstImage = imageFiles.FirstOrDefault();
        var firstGif = gifFiles.FirstOrDefault();

        // Enviar las listas y la primera imagen/GIF por defecto
        ViewBag.ImageFiles = imageFiles ?? new List<string>();  // Inicializar como lista vacía si no hay imágenes
        ViewBag.GifFiles = gifFiles ?? new List<string>();      // Inicializar como lista vacía si no hay GIFs
        ViewBag.FirstImageFile = firstImage;
        ViewBag.FirstGifFile = firstGif;

        return View();
    }

}



