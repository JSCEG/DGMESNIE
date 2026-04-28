using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Rendering;
using NSIE.Models;
using NSIE.Servicios;
using System.Security.Cryptography;
using System.Text;

namespace NSIE.Controllers
{
    [ServiceFilter(typeof(ValidacionInputFiltro))]
    [AutorizacionFiltro]
    public class UsuariosController : Controller
    {
        private readonly IRepositorioUsuarios repositorioUsuarios;
        private readonly IRepositorioAcceso repositorioAcceso;

        public UsuariosController(IRepositorioUsuarios repositorioUsuarios, IRepositorioAcceso repositorioAcceso)
        {
            this.repositorioUsuarios = repositorioUsuarios;
            this.repositorioAcceso = repositorioAcceso;
        }

        // ============================
        // 1. ADMINISTRACIÓN DE USUARIOS
        // ============================

        // Lista todos los usuarios
        public async Task<IActionResult> AdministrarUsuarios()
        {
            var listaUsuarios = await repositorioUsuarios.ObtenerListadeUsuarios();
            if (listaUsuarios == null || !listaUsuarios.Any())
                return NotFound();
            return View(listaUsuarios);
        }

        // Elimina un usuario por ID
        public async Task<IActionResult> Eliminar(int id)
        {
            var wasDeleted = await repositorioUsuarios.EliminarUsuario(id);
            TempData["UserMessage"] = wasDeleted ? "Usuario eliminado exitosamente!" : "Hubo un problema y el usuario no se pudo eliminar.";
            TempData["IsSuccess"] = wasDeleted;
            return RedirectToAction("AdministrarUsuarios");
        }

        // ============================
        // 2. REGISTRO DE NUEVO USUARIO
        // ============================

        // Muestra el formulario de registro
        public async Task<IActionResult> NuevoUsuario()
        {
            await PoblarDropdowns();
            return View();
        }

        // Procesa el registro de un nuevo usuario
        [HttpPost]
        public async Task<IActionResult> NuevoUsuario(UserViewModel nuevoUsuario, int IDUsuario, int RolUsuario)
        {
            // Validación de contraseñas
            if (string.IsNullOrWhiteSpace(nuevoUsuario.Clave) || string.IsNullOrWhiteSpace(nuevoUsuario.ConfirmarClave))
            {
                ViewData["Mensaje"] = "La contraseña no puede estar vacía.";
                await PoblarDropdowns();
                return View();
            }

            if (nuevoUsuario.Clave != nuevoUsuario.ConfirmarClave)
            {
                ViewData["Mensaje"] = "Las contraseñas no coinciden";
                await PoblarDropdowns();
                return View();
            }

            // Encriptar contraseña
            nuevoUsuario.Clave = ConvertirSha256(nuevoUsuario.Clave);

            if (ModelState.IsValid)
            {
                // Registrar usuario y rol
                int newUserId = await repositorioUsuarios.RegistraUsuario(nuevoUsuario);
                if (newUserId > 0)
                {
                    var rolUsuario = new RolesUsuarioViewModel
                    {
                        IdUsuario = newUserId,
                        Rol_ID = nuevoUsuario.Rol_ID,
                        Mercado_ID = nuevoUsuario.Mercado_ID,
                        RolUsuario_Comentarios = nuevoUsuario.RolUsuario_Comentarios,
                        RolUsuario_Vigente = RolUsuario,
                        RolUsuario_QuienRegistro = IDUsuario,
                        RolUsuario_FechaMod = DateTime.Now
                    };

                    bool isRolUsuarioCreated = await repositorioUsuarios.RegistraRolUsuario(rolUsuario);
                    if (isRolUsuarioCreated)
                        return RedirectToAction("AdministrarUsuarios");
                }
            }

            ModelState.AddModelError(string.Empty, "Error al crear usuario y/o rol de usuario.");
            await PoblarDropdowns();
            return View(nuevoUsuario);
        }

        // ============================
        // 3. EDICIÓN DE USUARIOS
        // ============================

        // Muestra el formulario de edición
        public async Task<IActionResult> Edit(int id)
        {
            var user = await repositorioUsuarios.ObtenerUsuarioPorId(id);
            if (user == null) return NotFound();

            var model = new EditUserViewModel
            {
                IdUsuario = user.IdUsuario,
                Correo = user.Correo,
                Clave = user.Clave,
                Nombre = user.Nombre,
                Unidad_de_Adscripcion = user.Unidad_de_Adscripcion,
                Cargo = user.Cargo,
                SesionActiva = user.SesionActiva,
                UltimaActualizacion = user.UltimaActualizacion,
                RFC = user.RFC,
                Vigente = user.Vigente,
                ClaveEmpleado = user.ClaveEmpleado,
                HoraInicioSesion = user.HoraInicioSesion,
                Mercado_ID = user.Mercado_ID,
                RolUsuario_Vigente = user.RolUsuario_Vigente,
                RolUsuario_QuienRegistro = user.RolUsuario_QuienRegistro,
                RolUsuario_FechaMod = user.RolUsuario_FechaMod,
                RolUsuario_Comentarios = user.RolUsuario_Comentarios,
                Rol_ID = user.Rol,
                Rol_Nombre = user.Rol_Nombre,
                Rol_Vigente = user.Rol_Vigente,
                Rol_FechaMod = user.Rol_FechaMod,
                Rol_Comentario = user.Rol_Comentario,
                Mercado_ID_M = user.Mercado_ID_M,
                Mercado_Nombre = user.Mercado_Nombre,
                Mercado_Vigente = user.Mercado_Vigente,
                Mercado_FechaMod = user.Mercado_FechaMod,
                Mercado_Comentario = user.Mercado_Comentario
            };

            // Poblar dropdowns
            var roles = await repositorioUsuarios.ObtenerTodosLosRoles();
            ViewBag.Roles = roles.Select(r => new SelectListItem
            {
                Value = r.Rol_ID.ToString(),
                Text = r.Rol_Nombre,
                Selected = r.Rol_ID == model.Rol_ID
            }).ToList();

            var mercados = await repositorioUsuarios.ObtenerTodosLosMercados();
            ViewBag.Mercados = mercados.Select(m => new SelectListItem
            {
                Value = m.Mercado_ID.ToString(),
                Text = m.Mercado_Nombre,
                Selected = m.Mercado_ID == model.Mercado_ID_M
            }).ToList();

            return View(model);
        }

        // Procesa la edición de usuario
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Edit(EditUserViewModel model)
        {
            if (!ModelState.IsValid)
            {
                //return View(model);


                var user = new UserViewModel
                {
                    IdUsuario = model.IdUsuario,
                    Nombre = model.Nombre,
                    RFC = model.RFC,
                    Correo = model.Correo,
                    Cargo = model.Cargo,
                    Unidad_de_Adscripcion = model.Unidad_de_Adscripcion,
                    ClaveEmpleado = model.ClaveEmpleado,
                    SesionActiva = model.SesionActiva,
                    Vigente = model.Vigente,
                };

                var rolUsuario = new RolesUsuarioViewModel
                {
                    IdUsuario = model.IdUsuario,
                    Rol_ID = model.Rol_ID,
                    Mercado_ID = model.Mercado_ID,
                    RolUsuario_Comentarios = model.RolUsuario_Comentarios
                };

                var userUpdateSuccess = await repositorioUsuarios.ActualizarUsuario(user);
                var rolUsuarioUpdateSuccess = await repositorioUsuarios.ActualizarRolUsuario(rolUsuario);

                if (userUpdateSuccess && rolUsuarioUpdateSuccess)
                {
                    return RedirectToAction("AdministrarUsuarios");
                }
            }
            ModelState.AddModelError(string.Empty, "Hubo un error al actualizar la información del usuario.");
            return View(model);
        }

        // ============================
        // 4. MODELO COMPUESTO DE CUENTA
        // ============================

        // Muestra el modelo compuesto de cuenta
        public async Task<IActionResult> ModeloCuentaCompuesto(int id)
        {
            var user = await repositorioUsuarios.ObtenerUsuarioPorId(id);
            if (user == null) return NotFound();

            var model = new ModeloCuentaCompuesto
            {
                CuentaUsuario = new Cuenta_UsuarioViewModel
                {
                    IdUsuario = user.IdUsuario,
                    Correo = user.Correo,
                    Clave = user.Clave,
                    Nombre = user.Nombre,
                    Unidad_de_Adscripcion = user.Unidad_de_Adscripcion,
                    Cargo = user.Cargo,
                    SesionActiva = user.SesionActiva,
                    UltimaActualizacion = user.UltimaActualizacion,
                    RFC = user.RFC,
                    Vigente = user.Vigente,
                    ClaveEmpleado = user.ClaveEmpleado,
                    HoraInicioSesion = user.HoraInicioSesion,
                    Mercado_ID = user.Mercado_ID,
                    RolUsuario_Vigente = user.RolUsuario_Vigente,
                    RolUsuario_QuienRegistro = user.RolUsuario_QuienRegistro,
                    RolUsuario_FechaMod = user.RolUsuario_FechaMod,
                    RolUsuario_Comentarios = user.RolUsuario_Comentarios,
                    Rol_ID = user.Rol,
                    Rol_Nombre = user.Rol_Nombre,
                    Rol_Vigente = user.Rol_Vigente,
                    Rol_FechaMod = user.Rol_FechaMod,
                    Rol_Comentario = user.Rol_Comentario,
                    Mercado_ID_M = user.Mercado_ID_M,
                    Mercado_Nombre = user.Mercado_Nombre,
                    Mercado_Vigente = user.Mercado_Vigente,
                    Mercado_FechaMod = user.Mercado_FechaMod,
                    Mercado_Comentario = user.Mercado_Comentario
                }
            };

            var roles = await repositorioUsuarios.ObtenerTodosLosRoles();
            ViewBag.Roles = roles.Select(r => new SelectListItem
            {
                Value = r.Rol_ID.ToString(),
                Text = r.Rol_Nombre,
                Selected = r.Rol_ID == model.CuentaUsuario.Rol_ID
            }).ToList();

            var mercados = await repositorioUsuarios.ObtenerTodosLosMercados();
            ViewBag.Mercados = mercados.Select(m => new SelectListItem
            {
                Value = m.Mercado_ID.ToString(),
                Text = m.Mercado_Nombre,
                Selected = m.Mercado_ID == model.CuentaUsuario.Mercado_ID_M
            }).ToList();

            return View(model);
        }

        // Procesa la edición del modelo compuesto
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> ModeloCuentaCompuesto(ModeloCuentaCompuesto model)
        {
            if (!ModelState.IsValid)
                return View(model);

            var user = new UserViewModel
            {
                IdUsuario = model.CuentaUsuario.IdUsuario,
                Nombre = model.CuentaUsuario.Nombre,
                RFC = model.CuentaUsuario.RFC,
                Correo = model.CuentaUsuario.Correo,
                Cargo = model.CuentaUsuario.Cargo,
                Unidad_de_Adscripcion = model.CuentaUsuario.Unidad_de_Adscripcion,
                ClaveEmpleado = model.CuentaUsuario.ClaveEmpleado,
                SesionActiva = model.CuentaUsuario.SesionActiva,
            };

            var rolUsuario = new RolesUsuarioViewModel
            {
                IdUsuario = model.CuentaUsuario.IdUsuario,
                Rol_ID = model.CuentaUsuario.Rol_ID,
                Mercado_ID = model.CuentaUsuario.Mercado_ID,
                RolUsuario_Comentarios = model.CuentaUsuario.RolUsuario_Comentarios
            };

            var userUpdateSuccess = await repositorioUsuarios.ActualizarUsuario(user);
            var rolUsuarioUpdateSuccess = await repositorioUsuarios.ActualizarRolUsuario(rolUsuario);

            if (userUpdateSuccess && rolUsuarioUpdateSuccess)
                return RedirectToAction("AdministrarUsuarios");

            ModelState.AddModelError(string.Empty, "Hubo un error al actualizar la información del usuario.");
            return View(model);
        }

        // ============================
        // 5. MONITOREO DE USUARIO
        // ============================

        // Devuelve datos de monitoreo de usuario en JSON
        public async Task<IActionResult> MonitoreoUsuario(int id, string nombre)
        {
            var userViewModel = await repositorioUsuarios.ObtenerUsuarioPorId(id);
            var usuario = ConvertToUsuario(userViewModel);
            var modeloCompuesto = await ObtenerModeloCompuesto(usuario, nombre);
            return Json(modeloCompuesto);
        }

        // Convierte UserViewModel a Usuario
        private Usuario ConvertToUsuario(UserViewModel userViewModel)
        {
            return new Usuario
            {
                IdUsuario = userViewModel.IdUsuario,
                Correo = userViewModel.Correo,
                // ...otras propiedades si es necesario
            };
        }

        // Obtiene el modelo compuesto de monitoreo
        private async Task<ModeloCuentaCompuesto> ObtenerModeloCompuesto(Usuario usuario, string nombre)
        {
            var totalAccesos = await repositorioAcceso.GetTotalAccessCountAsync();
            var fechaInicio = new DateTime(2023, 1, 1);
            var fechaFin = new DateTime(2030, 12, 31);
            var detallesAcceso = await repositorioAcceso.GetDetallesAccesoPorUsuarioAsync(nombre, fechaInicio, fechaFin);
            var totalAccesosPorTipo = await repositorioAcceso.GetTotalAccessCountByTypeAsync(fechaInicio, fechaFin);
            var ultimoAccesoPorUsuario = detallesAcceso
                .GroupBy(da => da.Nombre)
                .Select(g => new UltimoAccesoUsuario
                {
                    Nombre = g.Key,
                    UltimoAcceso = g.Max(x => x.FechaHoraLocal)
                })
                .ToList();

            return new ModeloCuentaCompuesto
            {
                CuentaUsuario = new Cuenta_UsuarioViewModel
                {
                    IdUsuario = usuario.IdUsuario,
                    Correo = usuario.Correo,
                    // ...otras propiedades si es necesario
                },
                CuentaMonitoreo = new Cuenta_MonitoreoViewModel
                {
                    TotalAccesos = totalAccesos,
                    DetallesAcceso = detallesAcceso,
                    TotalAccesosPorTipo = totalAccesosPorTipo,
                    UltimoAccesoPorUsuario = ultimoAccesoPorUsuario
                }
            };
        }

        // ============================
        // 6. NOTIFICACIONES
        // ============================

        [HttpGet]
        public async Task<IActionResult> GetNotifications(int userId)
        {
            var totalUnreadCount = await repositorioUsuarios.GetUnreadNotificationsCountAsync(userId);
            var notifications = await repositorioUsuarios.GetNotificationsByUserIdAsync(userId);
            return Json(new { totalUnreadCount, notifications });
        }

        [HttpPost]
        public async Task<IActionResult> MarkNotificationAsRead(int notificationId)
        {
            try
            {
                bool success = await repositorioUsuarios.MarkNotificationAsReadAsync(notificationId);
                return Json(new { success, message = success ? null : "No se pudo actualizar la notificación." });
            }
            catch (Exception ex)
            {
                return Json(new { success = false, message = ex.Message });
            }
        }

        public IActionResult NotificationDetails(string titulo, string mensaje, DateTime fecha, string link, string imagen)
        {
            var model = new NotificationDetails
            {
                Titulo = titulo,
                Mensaje = mensaje,
                Fecha = fecha,
                Link = link,
                Imagen = imagen
            };
            return View(model);
        }

        public async Task<IActionResult> AllNotifications(int userId)
        {
            var notificaciones = await repositorioUsuarios.GetAllNotificationsAsync(userId);
            return View(notificaciones);
        }

        [HttpPost]
        public async Task<IActionResult> GenerateTestNotification(int userId)
        {
            try
            {
                bool success = await repositorioUsuarios.GenerateNotificationsScriptAsync();
                return Json(new { success });
            }
            catch (Exception ex)
            {
                return Json(new { success = false, message = ex.Message });
            }
        }

        public async Task<IActionResult> DetalleNotificacion(int id)
        {
            var notification = await repositorioUsuarios.ObtenerNotificacionPorId(id);
            if (notification == null)
                return NotFound();
            return View(notification);
        }

        //CREAR NOTIFICACION
        [HttpGet]
        public async Task<IActionResult> CrearNotificacion()
        {
            var usuarios = await repositorioUsuarios.ObtenerListadeUsuarios();
            var roles = await repositorioUsuarios.ObtenerTodosLosRoles();

            // 🔹 Aquí filtramos solo para esta vista
            var usuariosFiltrados = usuarios.Where(u => u.IdUsuario != 10).ToList();
            var rolesFiltrados = roles.Where(r => r.Rol_ID != 0).ToList();

            var rolesViewModel = rolesFiltrados.Select(r => new UserViewModel
            {
                Rol_ID = r.Rol_ID,
                Rol_Nombre = r.Rol_Nombre
            }).ToList();

            ViewBag.Usuarios = usuariosFiltrados;
            ViewBag.Rol = rolesViewModel;

            return View();
        }

        [HttpPost]
        public async Task<IActionResult> GuardarNotificacion(Notificacion model)
        {
            try
            {
                if (model.ImagenFile != null && model.ImagenFile.Length > 0)
                {
                    // Carpeta de destino
                    var uploadsFolder = Path.Combine(Directory.GetCurrentDirectory(), "wwwroot/img/notificaciones");

                    // Crear carpeta si no existe
                    if (!Directory.Exists(uploadsFolder))
                        Directory.CreateDirectory(uploadsFolder);

                    // Nombre único para evitar choques
                    var uniqueFileName = Guid.NewGuid().ToString() + Path.GetExtension(model.ImagenFile.FileName);

                    // Ruta completa
                    var filePath = Path.Combine(uploadsFolder, uniqueFileName);

                    // Guardar el archivo
                    using (var stream = new FileStream(filePath, FileMode.Create))
                    {
                        await model.ImagenFile.CopyToAsync(stream);
                    }

                    // Guardamos la ruta relativa para usarla después en la vista
                    model.Imagen = "/img/notificaciones/" + uniqueFileName;
                }
                bool success = await repositorioUsuarios.GuardarNotificacionScriptAsync(model);
                return Json(new { success, message = success ? "Notificación creada." : "No se pudo crear." });
            }
            catch (Exception ex)
            {
                return Json(new { success = false, message = ex.Message });
            }
        }

        [HttpPost]
        public async Task<IActionResult> DeleteNotification(int notificationId)
        {
            try
            {
                var result = await repositorioUsuarios.DeleteNotificationAsync(notificationId);

                if (result)
                    return Json(new { success = true });
                else
                    return Json(new { success = false, message = "No se pudo eliminar la notificación." });
            }
            catch (Exception ex)
            {
                return Json(new { success = false, message = ex.Message });
            }
        }

        // ============================
        // 7. UTILIDADES Y AYUDA
        // ============================

        // Llena los dropdowns de roles y mercados para las vistas
        private async Task PoblarDropdowns()
        {
            var roles = await repositorioUsuarios.ObtenerTodosLosRoles();
            ViewBag.Roles = roles.Select(r => new SelectListItem
            {
                Value = r.Rol_ID.ToString(),
                Text = r.Rol_Nombre,
            }).ToList();

            var mercados = await repositorioUsuarios.ObtenerTodosLosMercados();
            ViewBag.Mercados = mercados.Select(m => new SelectListItem
            {
                Value = m.Mercado_ID.ToString(),
                Text = m.Mercado_Nombre,
            }).ToList();
        }

        // Encripta texto usando SHA256
        public static string ConvertirSha256(string texto)
        {
            StringBuilder Sb = new StringBuilder();
            using (SHA256 hash = SHA256Managed.Create())
            {
                Encoding enc = Encoding.UTF8;
                byte[] result = hash.ComputeHash(enc.GetBytes(texto));
                foreach (byte b in result)
                    Sb.Append(b.ToString("x2"));
            }
            return Sb.ToString();
        }

        // ============================
        // 8. VISTAS DE AYUDA Y ENCUESTAS
        // ============================

        public IActionResult Ayuda() => View();

        public async Task<IActionResult> Creditos()
        {
            var creditos = await repositorioUsuarios.ObtenerCreditos();
            return View(creditos.ToList());
        }

        public IActionResult Guia_Consulta_Publica() => View();

        public async Task<IActionResult> Resena(int id)
        {
            var credito = await repositorioUsuarios.ObtenerCreditoPorId(id);
            if (credito == null)
                return NotFound();
            return View(credito);
        }

        public IActionResult Encuesta() => View();

        [ServiceFilter(typeof(ValidacionInputFiltro))]
        [HttpPost]
        public IActionResult EnviarEncuesta(Encuesta encuesta)
        {
            if (!ModelState.IsValid)
                return View(encuesta);

            repositorioUsuarios.InsertarEncuesta(encuesta);
            return RedirectToAction("Gracias");
        }

        public IActionResult Gracias() => View();
    }
}
