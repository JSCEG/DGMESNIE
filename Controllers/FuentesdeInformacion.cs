using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Logging;
using NSIE.Models;
using NSIE.Servicios;
using System.Collections.Generic;
using System.Threading.Tasks;
using System;
using Microsoft.AspNetCore.Http;
using Newtonsoft.Json;

namespace NSIE.Controllers
{
    [ServiceFilter(typeof(ValidacionInputFiltro))]
    [AutorizacionFiltro]
    public class FuentesdeInformacion : Controller
    {
        private readonly IRepositorioFuentesdeInformacion repositorioFuentesdeInformacion;
        private readonly IRepositorioBitacora repositorioBitacora;
        private readonly ILogger<FuentesdeInformacion> _logger;

        public FuentesdeInformacion(IRepositorioFuentesdeInformacion repositorioFuentesdeInformacion,
                                   IRepositorioBitacora repositorioBitacora,
                                   ILogger<FuentesdeInformacion> logger)
        {
            this.repositorioFuentesdeInformacion = repositorioFuentesdeInformacion;
            this.repositorioBitacora = repositorioBitacora;
            _logger = logger;
        }

        // Vista principal del tablero
        public IActionResult Fuentes()
        {
            return View();
        }

        [HttpGet]
        public async Task<IActionResult> ObtenerFuentes(string filtro)
        {
            try
            {
                var datos = await repositorioFuentesdeInformacion.ObtenerFuentesAsync(filtro);
                return Json(datos);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error al obtener fuentes con filtro: {Filtro}", filtro);
                return Json(new { error = "Error interno del servidor al obtener fuentes." });
            }
        }

        [HttpGet]
        public async Task<IActionResult> ObtenerFuentesPorEntidad(string entidad)
        {
            try
            {
                // Decodificar las entidades HTML que pueden venir desde JavaScript
                var entidadDecodificada = System.Net.WebUtility.HtmlDecode(entidad);
                _logger.LogInformation("Entidad original: {EntidadOriginal}", entidad);
                _logger.LogInformation("Entidad decodificada: {EntidadDecodificada}", entidadDecodificada);

                var datos = await repositorioFuentesdeInformacion.ObtenerFuentesPorEntidadAsync(entidadDecodificada);
                _logger.LogInformation("Fuentes encontradas para '{Entidad}': {Count}", entidadDecodificada, datos.Count);

                return Json(datos);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error al obtener fuentes por entidad: {Entidad}", entidad);
                return Json(new { error = "Error interno del servidor al obtener fuentes." });
            }
        }

        [HttpGet]
        public async Task<IActionResult> ObtenerTotalesPorFuente()
        {
            try
            {
                var totales = await repositorioFuentesdeInformacion.ObtenerTotalesPorFuenteAsync();
                return Json(totales);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error al obtener totales por fuente");
                return Json(new { error = "Error interno del servidor al obtener totales." });
            }
        }

        [HttpGet]
        public async Task<IActionResult> ObtenerFuentePorId(int id)
        {
            try
            {
                var fuente = await repositorioFuentesdeInformacion.ObtenerFuentePorIdAsync(id);
                return Json(fuente);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error al obtener fuente por ID: {Id}", id);
                return Json(new { error = "Error interno del servidor al obtener fuente." });
            }
        }

        // Nueva vista para gestionar fuentes de una entidad específica
        public IActionResult GestionarFuentes(string entidad)
        {
            if (string.IsNullOrEmpty(entidad))
            {
                return RedirectToAction("Fuentes");
            }

            ViewBag.Entidad = entidad;
            return View();
        }

        [HttpPost]
        public async Task<IActionResult> ActualizarEntidad([FromBody] ActualizarEntidadRequest request)
        {
            try
            {
                if (request == null)
                {
                    return Json(new { success = false, error = "Datos de solicitud requeridos." });
                }

                if (string.IsNullOrWhiteSpace(request.EntidadOriginal) || string.IsNullOrWhiteSpace(request.EntidadNueva))
                {
                    return Json(new { success = false, error = "Los nombres de entidad son requeridos." });
                }

                if (request.EntidadOriginal == request.EntidadNueva)
                {
                    return Json(new { success = false, error = "El nuevo nombre debe ser diferente al actual." });
                }

                var resultado = await repositorioFuentesdeInformacion.ActualizarEntidadAsync(request.EntidadOriginal, request.EntidadNueva);

                if (resultado)
                {
                    return Json(new { success = true, message = $"Entidad actualizada de '{request.EntidadOriginal}' a '{request.EntidadNueva}'" });
                }
                else
                {
                    return Json(new { success = false, error = "No se encontró la entidad a actualizar." });
                }
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, $"Error al actualizar entidad de {request?.EntidadOriginal} a {request?.EntidadNueva}");
                return Json(new { success = false, error = "Error interno del servidor al actualizar entidad." });
            }
        }

        [HttpPost]
        public async Task<IActionResult> EliminarEntidad([FromBody] EliminarEntidadRequest request)
        {
            try
            {
                if (request == null)
                {
                    return Json(new { success = false, error = "Datos de solicitud requeridos." });
                }

                if (string.IsNullOrWhiteSpace(request.Entidad))
                {
                    return Json(new { success = false, error = "El nombre de la entidad es requerido." });
                }

                var resultado = await repositorioFuentesdeInformacion.EliminarEntidadAsync(request.Entidad);

                if (resultado)
                {
                    return Json(new { success = true, message = $"Entidad '{request.Entidad}' eliminada exitosamente" });
                }
                else
                {
                    return Json(new { success = false, error = "No se encontró la entidad a eliminar." });
                }
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, $"Error al eliminar entidad: {request?.Entidad}");
                return Json(new { success = false, error = "Error interno del servidor al eliminar entidad." });
            }
        }

        [HttpPost]
        public async Task<IActionResult> CrearEntidad([FromBody] CrearEntidadRequest request)
        {
            try
            {
                if (request == null)
                {
                    return Json(new { success = false, error = "Datos de solicitud requeridos." });
                }

                if (string.IsNullOrWhiteSpace(request.Nombre))
                {
                    return Json(new { success = false, error = "El nombre de la entidad es requerido." });
                }

                if (request.Nombre.Length < 3)
                {
                    return Json(new { success = false, error = "El nombre debe tener al menos 3 caracteres." });
                }

                var resultado = await repositorioFuentesdeInformacion.CrearEntidadAsync(request.Nombre);

                if (resultado)
                {
                    return Json(new { success = true, message = $"Entidad '{request.Nombre}' creada exitosamente" });
                }
                else
                {
                    return Json(new { success = false, error = "La entidad ya existe o no se pudo crear." });
                }
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, $"Error al crear entidad: {request?.Nombre}");
                return Json(new { success = false, error = "Error interno del servidor al crear entidad." });
            }
        }

        [HttpPost]
        public async Task<IActionResult> CrearFuente([FromBody] FuenteInformacionModel nuevaFuente)
        {
            try
            {
                if (nuevaFuente == null)
                {
                    return Json(new { success = false, error = "Los datos de la fuente son requeridos." });
                }

                // --- INICIO: Validar sesión de usuario ---
                var perfilUsuarioJson = HttpContext.Session.GetString("PerfilUsuario");
                if (string.IsNullOrEmpty(perfilUsuarioJson))
                {
                    return Json(new { success = false, error = "Sesión de usuario no encontrada. Por favor, inicie sesión de nuevo." });
                }
                var perfilUsuario = JsonConvert.DeserializeObject<PerfilUsuario>(perfilUsuarioJson);
                // --- FIN: Validar sesión de usuario ---

                // Validación de seguridad contra inyección de código
                var validacionSeguridad = ValidarSeguridadContenido(nuevaFuente);
                if (!validacionSeguridad.EsValido)
                {
                    _logger.LogWarning("Intento de inyección de código detectado por usuario {Usuario}: {Mensaje}", 
                        perfilUsuario.Nombre, validacionSeguridad.Mensaje);
                    return Json(new { success = false, error = $"Contenido no permitido detectado: {validacionSeguridad.Mensaje}" });
                }

                // Validaciones básicas
                if (string.IsNullOrWhiteSpace(nuevaFuente.Entidad))
                {
                    return Json(new { success = false, error = "La entidad es requerida." });
                }

                if (string.IsNullOrWhiteSpace(nuevaFuente.Tipo))
                {
                    return Json(new { success = false, error = "El tipo es requerido." });
                }

                if (string.IsNullOrWhiteSpace(nuevaFuente.Rubro))
                {
                    return Json(new { success = false, error = "El rubro es requerido." });
                }

                if (string.IsNullOrWhiteSpace(nuevaFuente.Etiqueta))
                {
                    return Json(new { success = false, error = "La etiqueta es requerida." });
                }

                // Crear la fuente en la base de datos
                var idNuevaFuente = await repositorioFuentesdeInformacion.CrearFuenteAsync(nuevaFuente);

                if (idNuevaFuente > 0)
                {
                    // Registrar la actividad en la bitácora centralizada
                    await repositorioBitacora.RegistrarActividadAsync(
                        userId: perfilUsuario.IdUsuario.ToString(),
                        userName: perfilUsuario.Nombre,
                        actionName: "Crear",
                        controllerName: "FuentesdeInformacion",
                        pageName: "Fuentes de Información",
                        tipo: "Entidad",
                        elemento: "Fuente",
                        idElemento: idNuevaFuente.ToString(),
                        valor: nuevaFuente.Etiqueta,
                        additionalData: JsonConvert.SerializeObject(new
                        {
                            Entidad = nuevaFuente.Entidad,
                            Tipo = nuevaFuente.Tipo,
                            Rubro = nuevaFuente.Rubro
                        })
                    );

                    return Json(new
                    {
                        success = true,
                        message = $"Fuente '{nuevaFuente.Etiqueta}' creada exitosamente en la entidad '{nuevaFuente.Entidad}'",
                        id = idNuevaFuente
                    });
                }
                else
                {
                    return Json(new { success = false, error = "No se pudo crear la fuente." });
                }
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error al crear fuente: {Etiqueta}", nuevaFuente?.Etiqueta ?? "null");
                return Json(new { success = false, error = "Error interno del servidor al crear fuente." });
            }
        }

        [HttpPost]
        public async Task<IActionResult> ActualizarFuente([FromBody] FuenteInformacionModel fuenteActualizada)
        {
            try
            {
                if (fuenteActualizada == null)
                {
                    return Json(new { success = false, error = "Los datos de la fuente son requeridos." });
                }

                // --- INICIO: Validar sesión de usuario ---
                var perfilUsuarioJson = HttpContext.Session.GetString("PerfilUsuario");
                if (string.IsNullOrEmpty(perfilUsuarioJson))
                {
                    return Json(new { success = false, error = "Sesión de usuario no encontrada. Por favor, inicie sesión de nuevo." });
                }
                var perfilUsuario = JsonConvert.DeserializeObject<PerfilUsuario>(perfilUsuarioJson);
                // --- FIN: Validar sesión de usuario ---

                // Validación de seguridad contra inyección de código
                var validacionSeguridad = ValidarSeguridadContenido(fuenteActualizada);
                if (!validacionSeguridad.EsValido)
                {
                    _logger.LogWarning("Intento de inyección de código detectado en actualización por usuario {Usuario}: {Mensaje}", 
                        perfilUsuario.Nombre, validacionSeguridad.Mensaje);
                    return Json(new { success = false, error = $"Contenido no permitido detectado: {validacionSeguridad.Mensaje}" });
                }

                // Obtener la fuente original para auditoría
                var fuenteOriginal = await repositorioFuentesdeInformacion.ObtenerFuentePorIdAsync(fuenteActualizada.ID);
                if (fuenteOriginal == null)
                {
                    return Json(new { success = false, error = "Fuente no encontrada." });
                }

                var resultado = await repositorioFuentesdeInformacion.ActualizarFuenteAsync(fuenteActualizada);

                if (resultado)
                {
                    // Registrar en auditoría
                    await repositorioBitacora.RegistrarActividadAsync(
                        userId: perfilUsuario.IdUsuario.ToString(),
                        userName: perfilUsuario.Nombre,
                        actionName: "Actualizar",
                        controllerName: "FuentesdeInformacion",
                        pageName: "Fuentes de Información",
                        tipo: "Entidad",
                        elemento: "Fuente",
                        idElemento: fuenteActualizada.ID.ToString(),
                        valor: fuenteActualizada.Etiqueta,
                        additionalData: JsonConvert.SerializeObject(new
                        {
                            Entidad = fuenteActualizada.Entidad,
                            Tipo = fuenteActualizada.Tipo,
                            Rubro = fuenteActualizada.Rubro,
                            ValorAnterior = fuenteOriginal.Etiqueta
                        })
                    );

                    return Json(new
                    {
                        success = true,
                        message = $"Fuente '{fuenteActualizada.Etiqueta}' actualizada exitosamente"
                    });
                }
                else
                {
                    return Json(new { success = false, error = "No se pudo actualizar la fuente." });
                }
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error al actualizar fuente ID: {Id}", fuenteActualizada?.ID ?? 0);
                return Json(new { success = false, error = "Error interno del servidor al actualizar fuente." });
            }
        }

        [HttpPost]
        public async Task<IActionResult> EliminarFuente([FromBody] EliminarFuenteRequest request)
        {
            try
            {
                if (request?.Id <= 0)
                {
                    return Json(new { success = false, error = "ID de fuente inválido." });
                }

                // --- INICIO: Validar sesión de usuario ---
                var perfilUsuarioJson = HttpContext.Session.GetString("PerfilUsuario");
                if (string.IsNullOrEmpty(perfilUsuarioJson))
                {
                    return Json(new { success = false, error = "Sesión de usuario no encontrada. Por favor, inicie sesión de nuevo." });
                }
                var perfilUsuario = JsonConvert.DeserializeObject<PerfilUsuario>(perfilUsuarioJson);
                // --- FIN: Validar sesión de usuario ---

                // Obtener la fuente antes de eliminarla para auditoría
                var fuente = await repositorioFuentesdeInformacion.ObtenerFuentePorIdAsync(request.Id);
                if (fuente == null)
                {
                    return Json(new { success = false, error = "Fuente no encontrada." });
                }

                var resultado = await repositorioFuentesdeInformacion.EliminarFuenteAsync(request.Id);

                if (resultado)
                {
                    // Registrar en auditoría
                    await repositorioBitacora.RegistrarActividadAsync(
                        userId: perfilUsuario.IdUsuario.ToString(),
                        userName: perfilUsuario.Nombre,
                        actionName: "Eliminar",
                        controllerName: "FuentesdeInformacion",
                        pageName: "Fuentes de Información",
                        tipo: "Entidad",
                        elemento: "Fuente",
                        idElemento: request.Id.ToString(),
                        valor: fuente.Etiqueta,
                        additionalData: JsonConvert.SerializeObject(new
                        {
                            Entidad = fuente.Entidad,
                            Tipo = fuente.Tipo,
                            Rubro = fuente.Rubro
                        })
                    );

                    return Json(new
                    {
                        success = true,
                        message = $"Fuente '{fuente.Etiqueta}' eliminada exitosamente"
                    });
                }
                else
                {
                    return Json(new { success = false, error = "No se pudo eliminar la fuente." });
                }
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error al eliminar fuente ID: {Id}", request?.Id ?? 0);
                return Json(new { success = false, error = "Error interno del servidor al eliminar fuente." });
            }
        }

        [HttpGet]
        public async Task<IActionResult> ObtenerHistorialFuente(int id)
        {
            try
            {
                IEnumerable<UserActivityModel> historial;

                if (id == 0)
                {
                    // Si id es 0, obtener todo el historial de la página
                    historial = await repositorioBitacora.ObtenerHistorialPorElementoAsync(
                        pageName: "Fuentes de Información"
                    );
                }
                else
                {
                    // Si id es específico, obtener historial de esa fuente
                    historial = await repositorioBitacora.ObtenerHistorialPorElementoAsync(
                        pageName: "Fuentes de Información",
                        idElemento: id.ToString()
                    );
                }

                return Json(new { success = true, data = historial });
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error al obtener historial de fuente: {Id}", id);
                return Json(new { success = false, error = "Error interno del servidor al obtener historial." });
            }
        }

        // Método privado para validar seguridad del contenido
        private ValidacionSeguridadResult ValidarSeguridadContenido(FuenteInformacionModel fuente)
        {
            var camposAValidar = new Dictionary<string, string>
            {
                { "Entidad", fuente.Entidad },
                { "Tipo", fuente.Tipo },
                { "Rubro", fuente.Rubro },
                { "Etiqueta", fuente.Etiqueta },
                { "Dato_Informacion", fuente.Dato_Informacion },
                { "Desagregacion", fuente.Desagregacion },
                { "Sub_desagregacion", fuente.Sub_desagregacion },
                { "Unidades", fuente.Unidades },
                { "Periodicidad_Corte_de_Informacion", fuente.Periodicidad_Corte_de_Informacion },
                { "Fuente_Link", fuente.Fuente_Link },
                { "Comentario", fuente.Comentario }
            };

            // Patrones peligrosos
            var patronesPeligrosos = new[]
            {
                @"<script\b[^<]*(?:(?!<\/script>)<[^<]*)*<\/script>",
                @"<iframe\b[^<]*(?:(?!<\/iframe>)<[^<]*)*<\/iframe>",
                @"<object\b[^<]*(?:(?!<\/object>)<[^<]*)*<\/object>",
                @"<embed\b[^<]*(?:(?!<\/embed>)<[^<]*)*<\/embed>",
                @"<link\b[^>]*>",
                @"<meta\b[^>]*>",
                @"javascript:",
                @"vbscript:",
                @"onload\s*=",
                @"onclick\s*=",
                @"onerror\s*=",
                @"onmouseover\s*=",
                @"onfocus\s*=",
                @"onblur\s*=",
                @"onchange\s*=",
                @"onsubmit\s*=",
                @"<\s*\/?\s*(script|iframe|object|embed|form|input|select|textarea|button|link|meta|style|base|applet|body|html|head|title)\b[^>]*>",
                @"expression\s*\(",
                @"url\s*\(\s*javascript:",
                @"data\s*:\s*text\/html",
                @"eval\s*\(",
                @"setTimeout\s*\(",
                @"setInterval\s*\(",
                @"Function\s*\(",
                @"alert\s*\(",
                @"confirm\s*\(",
                @"prompt\s*\("
            };

            foreach (var campo in camposAValidar)
            {
                if (string.IsNullOrEmpty(campo.Value)) continue;

                foreach (var patron in patronesPeligrosos)
                {
                    if (System.Text.RegularExpressions.Regex.IsMatch(campo.Value, patron, 
                        System.Text.RegularExpressions.RegexOptions.IgnoreCase))
                    {
                        return new ValidacionSeguridadResult
                        {
                            EsValido = false,
                            Mensaje = $"Contenido peligroso detectado en el campo '{campo.Key}'"
                        };
                    }
                }

                // Verificar caracteres sospechosos
                if (campo.Value.Contains('<') && campo.Value.Contains('>') && 
                    !System.Text.RegularExpressions.Regex.IsMatch(campo.Value, @"^https?:\/\/[^\s<>""']+$"))
                {
                    return new ValidacionSeguridadResult
                    {
                        EsValido = false,
                        Mensaje = $"Caracteres HTML sospechosos detectados en el campo '{campo.Key}'"
                    };
                }
            }

            return new ValidacionSeguridadResult { EsValido = true, Mensaje = "" };
        }
    }

    // DTOs para las requests
    public class ActualizarEntidadRequest
    {
        public string EntidadOriginal { get; set; }
        public string EntidadNueva { get; set; }
    }

    public class EliminarEntidadRequest
    {
        public string Entidad { get; set; }
    }

    public class CrearEntidadRequest
    {
        public string Nombre { get; set; }
    }

    public class EliminarFuenteRequest
    {
        public int Id { get; set; }
    }

    // Clase para resultado de validación de seguridad
    public class ValidacionSeguridadResult
    {
        public bool EsValido { get; set; }
        public string Mensaje { get; set; }
    }
}
