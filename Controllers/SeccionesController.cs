using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Rendering;
using Microsoft.Data.SqlClient;
using NSIE.Models;
using NSIE.Servicios;
using System.Security.Cryptography;
using System.Text;
using System.Data;
using System.Configuration;

[ServiceFilter(typeof(ValidacionInputFiltro))]
[AutorizacionFiltro]
public class SeccionesController : Controller
{
    private readonly IRepositorioSecciones _repositorio;

    public SeccionesController(IRepositorioSecciones repositorio)
    {
        _repositorio = repositorio;
    }

    public async Task<IActionResult> Index()
    {
        try
        {
            Console.WriteLine(">>> Index - Cargando secciones con módulos");
            var secciones = await _repositorio.ObtenerSeccionesConModulosAsync(); // ✅ USAR ESTE MÉTODO
            Console.WriteLine($">>> Index - {secciones.Count} secciones encontradas");
            return View(secciones);
        }
        catch (Exception ex)
        {
            Console.WriteLine($">>> Error en Index: {ex.Message}");
            return View(new List<SeccionConModulos>());
        }
    }



    public async Task<IActionResult> EditarSeccion(int id)
    {
        var seccion = await _repositorio.ObtenerSeccionPorIdAsync(id);
        if (seccion == null) return NotFound();
        return View("EditarSeccion", seccion);
    }

    [HttpPost]
    public async Task<IActionResult> EditarSeccion(SeccionConModulos seccion)
    {
        if (!ModelState.IsValid)
            return View("EditarSeccion", seccion);

        await _repositorio.ActualizarSeccionAsync(seccion);
        return RedirectToAction("Index");
    }

    public async Task<IActionResult> EditarModulos(int id)
    {
        var seccion = await _repositorio.ObtenerSeccionPorIdAsync(id);
        if (seccion == null)
            return NotFound();

        return View("EditarModulos", seccion);
    }

    public IActionResult Crear()
    {
        return View(new SeccionConModulos());
    }

    [HttpPost]
    public async Task<IActionResult> Crear(SeccionConModulos seccion)
    {
        if (!ModelState.IsValid)
            return View(seccion);

        await _repositorio.CrearSeccionAsync(seccion);
        return RedirectToAction("Index");
    }

    [HttpPost]
    [ValidateAntiForgeryToken]
    public async Task<IActionResult> EliminarSeccion(int id)
    {
        var seccion = await _repositorio.ObtenerSeccionPorIdAsync(id);
        if (seccion == null)
            return NotFound("La sección no existe.");

        await _repositorio.EliminarSeccionAsync(id);

        return Json(new { mensaje = "La sección se eliminó correctamente." });
    }

    [HttpPost]
    // ✅ QUITAR TEMPORALMENTE [ValidateAntiForgeryToken]
    public async Task<IActionResult> EliminarSeccionAjax(int id)
    {
        try
        {
            Console.WriteLine($">>> EliminarSeccionAjax - ID: {id}");

            if (id <= 0)
            {
                Console.WriteLine($">>> ID inválido: {id}");
                return Json(new { error = "ID de sección inválido" });
            }

            // Verificar que la sección existe
            var seccion = await _repositorio.ObtenerSeccionPorIdAsync(id);
            if (seccion == null)
            {
                Console.WriteLine($">>> Sección con ID {id} no encontrada");
                return Json(new { error = "Sección no encontrada" });
            }

            Console.WriteLine($">>> Eliminando sección: {seccion.Titulo}");

            // Verificar si tiene módulos
            var modulos = await _repositorio.ObtenerModulosPorSeccionAsync(id);
            if (modulos != null && modulos.Any())
            {
                Console.WriteLine($">>> La sección tiene {modulos.Count} módulos asociados");
                return Json(new { error = $"No se puede eliminar la sección porque tiene {modulos.Count} módulos asociados. Elimine primero los módulos." });
            }

            // Eliminar la sección
            await _repositorio.EliminarSeccionAsync(id);

            Console.WriteLine($">>> Sección eliminada exitosamente");
            return Json(new
            {
                success = true,
                mensaje = $"La sección '{seccion.Titulo}' ha sido eliminada correctamente."
            });
        }
        catch (Exception ex)
        {
            Console.WriteLine($">>> ERROR en EliminarSeccionAjax: {ex.Message}");
            Console.WriteLine($">>> StackTrace: {ex.StackTrace}");
            return Json(new { error = "Error al eliminar la sección: " + ex.Message });
        }
    }


    // ✅ NUEVO: Renderizar vista parcial del modal
    public async Task<IActionResult> ModalEditarModulo(int id)
    {
        var modulo = await _repositorio.ObtenerModuloPorIdAsync(id);
        if (modulo == null)
            return NotFound();

        return PartialView("_EditarModuloPartial", modulo);
    }

    // ✅ NUEVO: Guardar edición del módulo desde el modal
    [HttpPost]
    public async Task<IActionResult> GuardarEdicionModulo(Modulo modulo)
    {
        if (!ModelState.IsValid)
            return BadRequest("Datos incompletos para actualizar el módulo.");

        await _repositorio.ActualizarModuloAsync(modulo);
        return Ok(new { mensaje = "Módulo actualizado correctamente." });
    }

    [HttpPost]
    public async Task<IActionResult> AgregarModulo(Modulo modulo)
    {
        if (!ModelState.IsValid)
            return BadRequest("Datos incompletos para el módulo.");

        await _repositorio.AgregarModuloAsync(modulo);
        return RedirectToAction("Editar", new { id = modulo.SeccionId });
    }

    [HttpPost]
    [ValidateAntiForgeryToken]
    public async Task<IActionResult> EliminarModulo(int id, int seccionId)
    {
        await _repositorio.EliminarModuloAsync(id);
        return Json(new { mensaje = "El módulo se eliminó correctamente." }); // ✅ funciona con AJAX
    }


    // ✅ NUEVO: Crear un nuevo módulo desde el modaldentro de una sección existente
    public IActionResult CrearModulo(int seccionId)
    {
        var nuevoModulo = new Modulo
        {
            SeccionId = seccionId,
            Activo = true, // por defecto
            Orden = 1      // valor sugerido
        };
        return PartialView("_CrearModuloPartial", nuevoModulo);
    }

    [HttpPost]
    public async Task<IActionResult> GuardarNuevoModulo(Modulo modulo)
    {
        Console.WriteLine(">>> Entró al método GuardarNuevoModulo");

        if (!ModelState.IsValid)
        {
            Console.WriteLine(">>> ModelState inválido");
            return BadRequest("Faltan datos para guardar el módulo.");
        }

        await _repositorio.AgregarModuloAsync(modulo);
        Console.WriteLine(">>> Insert ejecutado");

        return Ok(new { mensaje = "Módulo creado correctamente." });
    }

    //Crea y Guarda Nuevas Secciones desde el Index
    public IActionResult CrearSeccion()
    {
        var nueva = new SeccionSNIER { SeccionActiva = true };
        return PartialView("_CrearSeccionPartial", nueva);
    }

    [HttpPost]
    public async Task<IActionResult> GuardarNuevaSeccion(SeccionSNIER seccionSNIER)
    {
        try
        {
            Console.WriteLine($">>> GuardarNuevaSeccion - Título: {seccionSNIER.Titulo}");

            if (string.IsNullOrWhiteSpace(seccionSNIER.Titulo))
            {
                return BadRequest("El título es obligatorio.");
            }

            // Convertir SeccionSNIER a SeccionConModulos
            var seccion = new SeccionConModulos
            {
                Titulo = seccionSNIER.Titulo,
                FundamentoLegal = seccionSNIER.FundamentoLegal ?? "",
                Descripcion = seccionSNIER.Descripcion ?? "",
                Objetivo = seccionSNIER.Objetivo ?? "",
                ResponsableNormativo = seccionSNIER.ResponsableNormativo ?? "",
                PublicoObjetivo = seccionSNIER.PublicoObjetivo ?? "",
                Ayuda = seccionSNIER.Ayuda ?? "",
                Activo = seccionSNIER.SeccionActiva,
                Orden = 1 // valor por defecto para nuevas secciones
            };

            await _repositorio.CrearSeccionAsync(seccion);

            return Json(new { mensaje = "Sección creada correctamente." });
        }
        catch (Exception ex)
        {
            Console.WriteLine($">>> Error en GuardarNuevaSeccion: {ex.Message}");
            return Json(new { error = "Error al crear la sección: " + ex.Message });
        }
    }

    // Método para mostrar las vistas de un módulo
    public async Task<IActionResult> EditarVistas(int moduloId)
    {
        try
        {
            var modulo = await _repositorio.ObtenerModuloPorIdAsync(moduloId);
            if (modulo == null)
            {
                return NotFound("Módulo no encontrado");
            }

            var vistas = await _repositorio.ObtenerVistasPorModuloIdAsync(moduloId);

            ViewData["ModuloId"] = moduloId;
            ViewData["ModuloTitle"] = modulo.Title;
            ViewData["Vistas"] = vistas;

            return View(modulo);
        }
        catch (Exception ex)
        {
            Console.WriteLine($">>> Error en EditarVistas: {ex.Message}");
            return BadRequest("Error al cargar las vistas");
        }
    }

    // Método para crear nueva vista (modal)
    public async Task<IActionResult> CrearVista(int moduloId)
    {
        try
        {
            var modulo = await _repositorio.ObtenerModuloPorIdAsync(moduloId);
            if (modulo == null)
            {
                return NotFound("Módulo no encontrado");
            }

            var vista = new ModulosVista
            {
                ModuloId = moduloId,
                ModuloTitle = modulo.Title,
                Activa = true,
                Orden = 1,
                EsExterno = false
            };

            return PartialView("_CrearVistaPartial", vista);
        }
        catch (Exception ex)
        {
            Console.WriteLine($">>> Error en CrearVista: {ex.Message}");
            return BadRequest("Error al crear vista");
        }
    }

    // Método para guardar nueva vista
    [HttpPost]
    public async Task<IActionResult> GuardarNuevaVista(ModulosVista vista)
    {
        try
        {
            if (!ModelState.IsValid)
            {
                return BadRequest("Datos incompletos para guardar la vista.");
            }

            await _repositorio.CrearModuloVistaAsync(vista);

            // IMPORTANTE: Retornar JSON con mensaje
            return Json(new { mensaje = "Vista creada correctamente." });
        }
        catch (Exception ex)
        {
            Console.WriteLine($">>> Error en GuardarNuevaVista: {ex.Message}");
            return StatusCode(500, "Error al guardar la vista");
        }
    }

    // Método para editar vista (modal)
    public async Task<IActionResult> ModalEditarVista(int id)
    {
        try
        {
            var vista = await _repositorio.ObtenerModuloVistaPorIdAsync(id);
            if (vista == null)
            {
                return NotFound("Vista no encontrada");
            }

            return PartialView("_EditarVistaPartial", vista);
        }
        catch (Exception ex)
        {
            Console.WriteLine($">>> Error en ModalEditarVista: {ex.Message}");
            return BadRequest("Error al cargar vista");
        }
    }

    // Método para guardar edición de vista
    [HttpPost]
    public async Task<IActionResult> GuardarEdicionVista(ModulosVista vista)
    {
        try
        {
            if (!ModelState.IsValid)
            {
                return BadRequest("Datos incompletos para actualizar la vista.");
            }

            await _repositorio.ActualizarModuloVistaAsync(vista);

            // IMPORTANTE: Retornar JSON con mensaje
            return Json(new { mensaje = "Vista actualizada correctamente." });
        }
        catch (Exception ex)
        {
            Console.WriteLine($">>> Error en GuardarEdicionVista: {ex.Message}");
            return StatusCode(500, "Error al actualizar la vista");
        }
    }

    // Método para eliminar vista
    [HttpPost]
    public async Task<IActionResult> EliminarVista(int id)
    {
        try
        {
            await _repositorio.EliminarModuloVistaAsync(id);
            return Ok(new { mensaje = "Vista eliminada correctamente." });
        }
        catch (Exception ex)
        {
            Console.WriteLine($">>> Error en EliminarVista: {ex.Message}");
            return BadRequest("Error al eliminar la vista");
        }
    }

    // Método para contar vistas (AJAX)
    public async Task<IActionResult> ContarVistas(int moduloId)
    {
        try
        {
            Console.WriteLine($">>> ContarVistas - Inicio con moduloId: {moduloId}");

            if (moduloId <= 0)
            {
                Console.WriteLine($">>> ModuloId inválido: {moduloId}");
                return BadRequest("ModuloId inválido");
            }

            var count = await _repositorio.ContarVistasPorModuloAsync(moduloId);
            Console.WriteLine($">>> ContarVistas - Resultado: {count} vistas para módulo {moduloId}");

            // Retornar directamente el número, no un objeto JSON
            return Content(count.ToString(), "text/plain");
        }
        catch (Exception ex)
        {
            Console.WriteLine($">>> Error en ContarVistas: {ex.Message}");
            Console.WriteLine($">>> StackTrace: {ex.StackTrace}");
            return Content("0", "text/plain");
        }
    }

    // Método para contar módulos (AJAX)
    public async Task<IActionResult> ContarModulos(int seccionId)
    {
        try
        {
            Console.WriteLine($">>> ContarModulos - Inicio con seccionId: {seccionId}");

            if (seccionId <= 0)
            {
                Console.WriteLine($">>> SeccionId inválido: {seccionId}");
                return BadRequest("SeccionId inválido");
            }

            var count = await _repositorio.ContarModulosPorSeccionAsync(seccionId);
            Console.WriteLine($">>> ContarModulos - Resultado: {count} módulos para sección {seccionId}");

            return Content(count.ToString(), "text/plain");
        }
        catch (Exception ex)
        {
            Console.WriteLine($">>> Error en ContarModulos: {ex.Message}");
            Console.WriteLine($">>> StackTrace: {ex.StackTrace}");
            return Content("0", "text/plain");
        }
    }

    [HttpPost]
    public async Task<IActionResult> CambiarOrdenSeccion(int seccionId, int nuevoOrden)
    {
        try
        {
            await _repositorio.ActualizarOrdenSeccionAsync(seccionId, nuevoOrden);
            return Json(new { mensaje = "Orden actualizado correctamente." });
        }
        catch (Exception ex)
        {
            Console.WriteLine($">>> Error en CambiarOrdenSeccion: {ex.Message}");
            return BadRequest("Error al cambiar el orden");
        }
    }

    // AGREGAR este método que falta
    public async Task<IActionResult> ModalEditarSeccion(int id)
    {
        try
        {
            Console.WriteLine($">>> ModalEditarSeccion - ID: {id}");

            if (id <= 0)
            {
                Console.WriteLine(">>> ID inválido");
                return BadRequest("ID de sección inválido");
            }

            var seccion = await _repositorio.ObtenerSeccionPorIdAsync(id);
            if (seccion == null)
            {
                Console.WriteLine($">>> Sección con ID {id} no encontrada");
                return NotFound("Sección no encontrada");
            }

            Console.WriteLine($">>> Sección encontrada: {seccion.Titulo}");

            // Convertir SeccionConModulos a SeccionSNIER para el modal
            var seccionSNIER = new SeccionSNIER
            {
                Id = seccion.Id,
                Titulo = seccion.Titulo,
                FundamentoLegal = seccion.FundamentoLegal,
                Descripcion = seccion.Descripcion,
                Objetivo = seccion.Objetivo,
                ResponsableNormativo = seccion.ResponsableNormativo,
                PublicoObjetivo = seccion.PublicoObjetivo,
                Ayuda = seccion.Ayuda,
                SeccionActiva = seccion.Activo,
                Orden = seccion.Orden
            };

            return PartialView("_EditarSeccionPartial", seccionSNIER);
        }
        catch (Exception ex)
        {
            Console.WriteLine($">>> Error en ModalEditarSeccion: {ex.Message}");
            Console.WriteLine($">>> StackTrace: {ex.StackTrace}");
            return BadRequest($"Error al cargar el modal: {ex.Message}");
        }
    }

    // AGREGAR este método que falta
    [HttpPost]
    public async Task<IActionResult> GuardarEdicionSeccion(SeccionSNIER seccionSNIER)
    {
        try
        {
            Console.WriteLine($">>> GuardarEdicionSeccion - ID: {seccionSNIER.Id}, Título: {seccionSNIER.Titulo}");

            if (string.IsNullOrWhiteSpace(seccionSNIER.Titulo))
            {
                return BadRequest("El título es obligatorio.");
            }

            // Convertir SeccionSNIER a SeccionConModulos
            var seccion = new SeccionConModulos
            {
                Id = seccionSNIER.Id,
                Titulo = seccionSNIER.Titulo,
                FundamentoLegal = seccionSNIER.FundamentoLegal ?? "",
                Descripcion = seccionSNIER.Descripcion ?? "",
                Objetivo = seccionSNIER.Objetivo ?? "",
                ResponsableNormativo = seccionSNIER.ResponsableNormativo ?? "",
                PublicoObjetivo = seccionSNIER.PublicoObjetivo ?? "",
                Ayuda = seccionSNIER.Ayuda ?? "",
                Activo = seccionSNIER.SeccionActiva,
                Orden = seccionSNIER.Orden
            };

            await _repositorio.ActualizarSeccionAsync(seccion);

            return Json(new { mensaje = "Sección actualizada correctamente." });
        }
        catch (Exception ex)
        {
            Console.WriteLine($">>> Error en GuardarEdicionSeccion: {ex.Message}");
            return Json(new { error = "Error al actualizar la sección: " + ex.Message });
        }
    }

    // Método para actualizar orden de múltiples secciones
    [HttpPost]
    public async Task<IActionResult> ActualizarOrdenSecciones([FromBody] List<CambioOrden> cambios)
    {
        try
        {
            foreach (var cambio in cambios)
            {
                await _repositorio.ActualizarOrdenSeccionAsync(cambio.SeccionId, cambio.NuevoOrden);
            }

            return Json(new { mensaje = "Orden actualizado correctamente." });
        }
        catch (Exception ex)
        {
            Console.WriteLine($">>> Error en ActualizarOrdenSecciones: {ex.Message}");
            return Json(new { error = "Error al actualizar el orden" });
        }
    }

    // Método para activar/desactivar sección
    [HttpPost]
    public async Task<IActionResult> ToggleActivarSeccion(int id, bool activar)
    {
        try
        {
            var seccion = await _repositorio.ObtenerSeccionPorIdAsync(id);
            if (seccion == null)
            {
                return Json(new { error = "Sección no encontrada" });
            }

            seccion.Activo = activar;
            await _repositorio.ActualizarSeccionAsync(seccion);

            string mensaje = activar ? "Sección activada correctamente" : "Sección desactivada correctamente";
            return Json(new { mensaje = mensaje });
        }
        catch (Exception ex)
        {
            Console.WriteLine($">>> Error en ToggleActivarSeccion: {ex.Message}");
            return Json(new { error = "Error al cambiar el estado de la sección" });
        }
    }

    // Clase para recibir cambios de orden
    public class CambioOrden
    {
        public int SeccionId { get; set; }
        public int NuevoOrden { get; set; }
    }
}