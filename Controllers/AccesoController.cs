using Microsoft.AspNetCore.Mvc;
using NSIE.Models;
using NSIE.Servicios;
using System.Security.Cryptography;
using System.Text;
using System.Threading.Tasks;
using Microsoft.Data.SqlClient;
using Newtonsoft.Json;
using Dapper;
using System.Data;
using Microsoft.AspNetCore.Authentication;
using Microsoft.AspNetCore.Authentication.Cookies;
using System.Security.Claims;

namespace NSIE.Controllers
{
    [ServiceFilter(typeof(ValidacionInputFiltro))]
    public class AccesoController : Controller
    {
        private readonly IServicioEmailSMTP _servicioEmailSMTP;
        //private readonly IServicioEmail _servicioEmail;
        private readonly IRepositorioAcceso _repositorioAcceso;
        private readonly IRepositorioUsuarios _repositorioUsuarios;
        private readonly string _connectionString;
        private readonly ILogger<AccesoController> _logger;
        public AccesoController(IRepositorioAcceso repositorioAcceso, IRepositorioUsuarios repositorioUsuarios, IConfiguration configuration, IServicioEmailSMTP servicioEmailSMTP, ILogger<AccesoController> logger)
        {
            _repositorioAcceso = repositorioAcceso;
            _repositorioUsuarios = repositorioUsuarios;
            _connectionString = configuration.GetConnectionString("DefaultConnection");
            //_servicioEmail = servicioEmail;
            _servicioEmailSMTP = servicioEmailSMTP;
            _logger = logger;
        }

        public IActionResult Enviar()
        {
            return View();
        }



        // [HttpPost]
        // public async Task<IActionResult> SendEmail(string nombre, string email, string mensaje)
        // {
        //     try
        //     {
        //         string cuerpoHtml = $@"
        //             <html>
        //             <body>
        //             <h1>Nuevo mensaje de contacto</h1>
        //             <p><strong>De:</strong> {nombre}</p>
        //             <p><strong>Email:</strong> {email}</p>
        //             <p><strong>Mensaje:</strong> {mensaje}</p>
        //             <img src='https://example.com/your-image.jpg' alt='Imagen' />
        //             </body>
        //             </html>";

        //         await _servicioEmailSMTP.EnviarCorreo(email, "Nuevo mensaje de contacto", cuerpoHtml, true);

        //         ViewBag.Message = "Su mensaje ha sido enviado con éxito.";
        //         return View("Gracias");
        //     }
        //     catch (Exception ex)
        //     {
        //         ViewBag.Error = "Error al enviar el mensaje: " + ex.Message;
        //         return View("Error");
        //     }
        // }

        //GET: Acceso
        public IActionResult Login()
        {
            return View();
        }

        [HttpPost]
        public IActionResult LoginGoogle(string returnUrl = "/")
        {
            var redirectUrl = Url.Action("GoogleResponse", "Acceso", new { ReturnUrl = returnUrl });
            var properties = new AuthenticationProperties { RedirectUri = redirectUrl };
            return Challenge(properties, "Google");
        }

        [HttpPost]
        public IActionResult LoginFacebook(string returnUrl = "/")
        {
            var redirectUrl = Url.Action("FacebookResponse", "Acceso", new { ReturnUrl = returnUrl });
            var properties = new AuthenticationProperties { RedirectUri = redirectUrl };
            return Challenge(properties, "Facebook");
        }

        public async Task<IActionResult> GoogleResponse(string returnUrl = "/")
        {
            var result = await HttpContext.AuthenticateAsync(CookieAuthenticationDefaults.AuthenticationScheme);
            if (!result.Succeeded) return RedirectToAction("Login");

            var claims = result.Principal.Identities.First().Claims;
            var email = claims.FirstOrDefault(c => c.Type == ClaimTypes.Email)?.Value;
            var name = claims.FirstOrDefault(c => c.Type == ClaimTypes.Name)?.Value;

            // Registrar o recuperar usuario
            var existingUser = await _repositorioUsuarios.BuscarPorCorreo(email);
            int userId;

            if (existingUser == null)
            {
                var nuevoUsuario = new UserViewModel
                {
                    Correo = email,
                    Nombre = name ?? "Usuario invitado",
                    Clave = ConvertirSha256("OAuthSocial"),
                    Vigente = true,
                    Unidad_de_Adscripcion = "Publico",
                    Cargo = "Visita",
                    SesionActiva = true,
                    UltimaActualizacion = DateTime.Now,
                    RFC = null,
                    ClaveEmpleado = null,
                    HoraInicioSesion = DateTime.Now
                };

                userId = await _repositorioUsuarios.RegistraUsuario(nuevoUsuario);

                if (userId > 0)
                {
                    var rolUsuario = new RolesUsuarioViewModel
                    {
                        IdUsuario = userId,
                        Rol_ID = 0,
                        Mercado_ID = 0,
                        RolUsuario_Vigente = 1,
                        RolUsuario_QuienRegistro = 1,
                        RolUsuario_FechaMod = DateTime.Now,
                        RolUsuario_Comentarios = "Registro automático por Google"
                    };

                    await _repositorioUsuarios.RegistraRolUsuario(rolUsuario);
                }
            }
            else
            {
                userId = existingUser.IdUsuario;
            }

            // 👉 Armas un Usuario para mandarlo al Login como invitado social
            var usuarioSocial = new Usuario
            {
                IdUsuario = userId,
                Correo = email,
                Clave = "OAuthSocial" // esta clave coincide con lo guardado en DB (encriptado en ProcesarLoginInvitado)
            };

            return Login(usuarioSocial, "social", true);

        }

        public async Task<IActionResult> FacebookResponse(string returnUrl = "/")
        {
            var result = await HttpContext.AuthenticateAsync(CookieAuthenticationDefaults.AuthenticationScheme);
            if (!result.Succeeded) return RedirectToAction("Login");

            var claims = result.Principal.Identities.First().Claims;
            var email = claims.FirstOrDefault(c => c.Type == ClaimTypes.Email)?.Value;
            var name = claims.FirstOrDefault(c => c.Type == ClaimTypes.Name)?.Value;

            // Registrar o recuperar usuario
            var existingUser = await _repositorioUsuarios.BuscarPorCorreo(email);
            int userId;

            if (existingUser == null)
            {
                var nuevoUsuario = new UserViewModel
                {
                    Correo = email ?? $"fbuser_{Guid.NewGuid()}@facebook.com", // fallback si no hay email
                    Nombre = name ?? "Usuario de Facebook",
                    Clave = ConvertirSha256("OAuthSocial"),
                    Vigente = true,
                    Unidad_de_Adscripcion = "Publico",
                    Cargo = "Visita",
                    SesionActiva = true,
                    UltimaActualizacion = DateTime.Now,
                    RFC = null,
                    ClaveEmpleado = null,
                    HoraInicioSesion = DateTime.Now
                };

                userId = await _repositorioUsuarios.RegistraUsuario(nuevoUsuario);

                if (userId > 0)
                {
                    var rolUsuario = new RolesUsuarioViewModel
                    {
                        IdUsuario = userId,
                        Rol_ID = 0,
                        Mercado_ID = 0,
                        RolUsuario_Vigente = 1,
                        RolUsuario_QuienRegistro = 1,
                        RolUsuario_FechaMod = DateTime.Now,
                        RolUsuario_Comentarios = "Registro automático por Facebook"
                    };

                    await _repositorioUsuarios.RegistraRolUsuario(rolUsuario);
                }
            }
            else
            {
                userId = existingUser.IdUsuario;
            }

            // 👉 Armas un Usuario para mandarlo al Login como invitado social
            var usuarioSocial = new Usuario
            {
                IdUsuario = userId,
                Correo = email,
                Clave = "OAuthSocial" // esta clave coincide con lo guardado en DB (encriptado en ProcesarLoginInvitado)
            };

            return Login(usuarioSocial, "social", true);
        }

        public async Task<IActionResult> Logout()
        {
            await HttpContext.SignOutAsync(CookieAuthenticationDefaults.AuthenticationScheme);
            return RedirectToAction("Login");
        }

        public async Task IniciarSesionInterna(int userId, string correo, string nombre, string rol = "Usuario")
        {
            // Definir los claims principales
            var claims = new List<Claim>
            {
                new Claim(ClaimTypes.NameIdentifier, userId.ToString()), // ID de usuario
                new Claim(ClaimTypes.Name, nombre ?? "Usuario"),         // Nombre visible
                new Claim(ClaimTypes.Email, correo),                     // Correo
                new Claim(ClaimTypes.Role, rol)                          // Rol en el sistema
            };

            // Crear identidad con esquema de cookies
            var claimsIdentity = new ClaimsIdentity(claims, CookieAuthenticationDefaults.AuthenticationScheme);

            // Opcional: definir propiedades de autenticación
            var authProperties = new AuthenticationProperties
            {
                IsPersistent = true,                 // Que la cookie sea persistente
                ExpiresUtc = DateTimeOffset.UtcNow.AddHours(2) // Expira en 2 horas
            };

            // Firmar al usuario
            await HttpContext.SignInAsync(
                CookieAuthenticationDefaults.AuthenticationScheme,
                new ClaimsPrincipal(claimsIdentity),
                authProperties
            );
        }

        #region Acceso a consulta Pública
        public IActionResult AccesoComoInvitado()
        {
            Usuario oUsuario = new Usuario
            {
                Correo = "invitado@cre.gob.mx",
                Clave = "consulta_publica"
            };
            RegistrarAcceso(oUsuario.Correo, "Acceso como Consulta Pública");
            return ProcesarLoginInvitado(oUsuario);
        }

        private IActionResult ProcesarLoginInvitado(Usuario oUsuario)
        {
            oUsuario.Clave = ConvertirSha256(oUsuario.Clave);

            Console.WriteLine("Clave: " + oUsuario.Clave);

            using (SqlConnection cn = new SqlConnection(_connectionString))
            {
                // Validar usuario por correo o RFC
                SqlCommand cmd = new SqlCommand("sp_ValidarUsuarioRFCEmail", cn);
                cmd.Parameters.AddWithValue("CorreoRFC", oUsuario.Correo);
                cmd.Parameters.AddWithValue("Clave", oUsuario.Clave);
                cmd.CommandType = CommandType.StoredProcedure;

                cn.Open();
                oUsuario.IdUsuario = Convert.ToInt32(cmd.ExecuteScalar());
            }

            if (oUsuario.IdUsuario != 0)
            {
                using (SqlConnection cn = new SqlConnection(_connectionString))
                {
                    // Verificar si el usuario está vigente
                    bool esVigente = cn.QuerySingleOrDefault<bool>(
                        "SELECT Vigente FROM USUARIO WHERE IdUsuario = @IdUsuario",
                        new { IdUsuario = oUsuario.IdUsuario }
                    );

                    if (!esVigente)
                    {
                        ViewData["MostrarModal"] = false;
                        ViewData["Mensaje"] = "Lo sentimos, su usuario no tiene acceso a la plataforma";
                        return View("Login");
                    }

                    // Activar sesión
                    cn.Execute(
                        "UPDATE USUARIO SET SesionActiva = 1, UltimaActualizacion = GETDATE(), HoraInicioSesion = GETDATE() WHERE IdUsuario = @IdUsuario",
                        new { IdUsuario = oUsuario.IdUsuario }
                    );

                    // Obtener perfil del usuario
                    PerfilUsuario perfilUsuario = cn.QuerySingleOrDefault<PerfilUsuario>(
                        "sp_ObtenerUsuarioSession",
                        new { IdUsuario = oUsuario.IdUsuario },
                        commandType: CommandType.StoredProcedure
                    );

                    if (perfilUsuario != null)
                    {
                        var perfilUsuarioJson = JsonConvert.SerializeObject(perfilUsuario);
                        HttpContext.Session.SetString("PerfilUsuario", perfilUsuarioJson);

                        // Obtener secciones y módulos permitidos agrupados correctamente
                        var seccionesDict = new Dictionary<int, SeccionSNIER>();

                        cn.Query<SeccionSNIER, ModuloSNIER, VistaSNIER, int>(
                            "sp_ObtenerSeccionesYModulosPorUsuario",
                            (seccion, modulo, vista) =>
                            {
                                if (!seccionesDict.TryGetValue(seccion.Id, out var seccionExistente))
                                {
                                    seccionExistente = seccion;
                                    seccionExistente.Modulos = new List<ModuloSNIER>();
                                    seccionesDict[seccion.Id] = seccionExistente;
                                }

                                var modExistente = seccionExistente.Modulos.FirstOrDefault(m => m.ModuloId == modulo.ModuloId);
                                if (modExistente == null)
                                {
                                    modExistente = modulo;
                                    modExistente.Vistas = new List<VistaSNIER>();

                                    // Si es externo
                                    if (modulo.Controller == "EXTERNA")
                                        modExistente.EsExterno = true;

                                    seccionExistente.Modulos.Add(modExistente);
                                }

                                if (vista != null && vista.VistaId != 0)
                                {
                                    modExistente.Vistas.Add(vista);
                                }

                                return seccionExistente.Id;
                            },
                            new { IdUsuario = oUsuario.IdUsuario },
                            splitOn: "ModuloId,VistaId",
                            commandType: CommandType.StoredProcedure
                        );

                        var seccionesAgrupadas = seccionesDict.Values.ToList();
                        var seccionesUsuarioJson = JsonConvert.SerializeObject(seccionesAgrupadas);
                        HttpContext.Session.SetString("SeccionesUsuario", seccionesUsuarioJson);

                        // Redireccionar si hay al menos un módulo externo (solo el primero)
                        var primerModuloExterno = seccionesAgrupadas
                            .SelectMany(s => s.Modulos)
                            .FirstOrDefault(m => m.EsExterno && !string.IsNullOrWhiteSpace(m.Action));

                        if (primerModuloExterno != null)
                        {
                            return Redirect(primerModuloExterno.Action);
                        }

                        return RedirectToAction("Index", "Home");
                    }
                }
            }
            else
            {
                ViewData["MostrarModal"] = false;
                ViewData["Mensaje"] = "Usuario no encontrado o contraseña incorrecta";
                return View("Login");
            }

            return View("Login");
        }
        #endregion

        #region Metodo Login
        [HttpPost]
        public IActionResult Login(Usuario oUsuario, string tipoAcceso = null, bool registrarAcceso = true)
        {
            // Si el tipo de acceso es público, procesar como invitado directamente
            if (tipoAcceso == "publico")
            {
                // Crear usuario invitado
                Usuario usuarioInvitado = new Usuario
                {
                    Correo = "invitado@cre.gob.mx",
                    Clave = "consulta_publica"
                };
                
                // Registrar acceso
                RegistrarAcceso(usuarioInvitado.Correo, "Acceso como Consulta Pública");
                
                // Procesar login como invitado (sin recursión)
                return ProcesarLoginInvitado(usuarioInvitado);
            }

            if (tipoAcceso == "social")
            {
                
                // Registrar acceso
                RegistrarAcceso(oUsuario.Correo, "Acceso como Consulta Pública");
                
                // Procesar login como invitado (sin recursión)
                return ProcesarLoginInvitado(oUsuario);
            }

            // Validar que se proporcionen credenciales para usuario registrado
            if (string.IsNullOrWhiteSpace(oUsuario.Correo) || string.IsNullOrWhiteSpace(oUsuario.Clave))
            {
                ViewData["MostrarModal"] = false;
                ViewData["Mensaje"] = "Por favor ingresa tu correo/RFC y contraseña";
                return View("Login");
            }

            oUsuario.Clave = ConvertirSha256(oUsuario.Clave);

            using (SqlConnection cn = new SqlConnection(_connectionString))
            {
                // Validar usuario por correo o RFC
                SqlCommand cmd = new SqlCommand("sp_ValidarUsuarioRFCEmail", cn);
                cmd.Parameters.AddWithValue("CorreoRFC", oUsuario.Correo);
                cmd.Parameters.AddWithValue("Clave", oUsuario.Clave);
                cmd.CommandType = CommandType.StoredProcedure;

                cn.Open();
                oUsuario.IdUsuario = Convert.ToInt32(cmd.ExecuteScalar());
            }

            if (oUsuario.IdUsuario != 0)
            {
                using (SqlConnection cn = new SqlConnection(_connectionString))
                {
                    // Verificar si el usuario está vigente
                    bool esVigente = cn.QuerySingleOrDefault<bool>(
                        "SELECT Vigente FROM USUARIO WHERE IdUsuario = @IdUsuario",
                        new { IdUsuario = oUsuario.IdUsuario }
                    );

                    if (!esVigente)
                    {
                        ViewData["MostrarModal"] = false;
                        ViewData["Mensaje"] = "Lo sentimos, su usuario no tiene acceso a la plataforma";
                        return View("Login");
                    }

                    if (registrarAcceso)
                    {
                        var correoUsuario = cn.QuerySingleOrDefault<string>(
                            "SELECT Correo FROM USUARIO WHERE IdUsuario = @IdUsuario",
                            new { IdUsuario = oUsuario.IdUsuario }
                        );
                        RegistrarAcceso(correoUsuario, "Inicio de sesión funcionario CRE");
                    }

                    // Activar sesión
                    cn.Execute(
                        "UPDATE USUARIO SET SesionActiva = 1, UltimaActualizacion = GETDATE(), HoraInicioSesion = GETDATE() WHERE IdUsuario = @IdUsuario",
                        new { IdUsuario = oUsuario.IdUsuario }
                    );

                    // Obtener perfil del usuario
                    PerfilUsuario perfilUsuario = cn.QuerySingleOrDefault<PerfilUsuario>(
                        "sp_ObtenerUsuarioSession",
                        new { IdUsuario = oUsuario.IdUsuario },
                        commandType: CommandType.StoredProcedure
                    );

                    if (perfilUsuario != null)
                    {
                        var perfilUsuarioJson = JsonConvert.SerializeObject(perfilUsuario);
                        HttpContext.Session.SetString("PerfilUsuario", perfilUsuarioJson);

                        // Obtener secciones y módulos permitidos agrupados correctamente
                        var seccionesDict = new Dictionary<int, SeccionSNIER>();

                        cn.Query<SeccionSNIER, ModuloSNIER, VistaSNIER, int>(
                            "sp_ObtenerSeccionesYModulosPorUsuario",
                            (seccion, modulo, vista) =>
                            {
                                if (!seccionesDict.TryGetValue(seccion.Id, out var seccionExistente))
                                {
                                    seccionExistente = seccion;
                                    seccionExistente.Modulos = new List<ModuloSNIER>();
                                    seccionesDict[seccion.Id] = seccionExistente;
                                }

                                var modExistente = seccionExistente.Modulos.FirstOrDefault(m => m.ModuloId == modulo.ModuloId);
                                if (modExistente == null)
                                {
                                    modExistente = modulo;
                                    modExistente.Vistas = new List<VistaSNIER>();

                                    // Si es externo
                                    if (modulo.Controller == "EXTERNA")
                                        modExistente.EsExterno = true;

                                    seccionExistente.Modulos.Add(modExistente);
                                }

                                if (vista != null && vista.VistaId != 0)
                                {
                                    modExistente.Vistas.Add(vista);
                                }

                                return seccionExistente.Id;
                            },
                            new { IdUsuario = oUsuario.IdUsuario },
                            splitOn: "ModuloId,VistaId",
                            commandType: CommandType.StoredProcedure
                        );

                        var seccionesAgrupadas = seccionesDict.Values.ToList();
                        var seccionesUsuarioJson = JsonConvert.SerializeObject(seccionesAgrupadas);
                        HttpContext.Session.SetString("SeccionesUsuario", seccionesUsuarioJson);

                        // Redireccionar si hay al menos un módulo externo (solo el primero)
                        var primerModuloExterno = seccionesAgrupadas
                            .SelectMany(s => s.Modulos)
                            .FirstOrDefault(m => m.EsExterno && !string.IsNullOrWhiteSpace(m.Action));

                        if (primerModuloExterno != null)
                        {
                            return Redirect(primerModuloExterno.Action);
                        }

                        return RedirectToAction("Index", "Home");
                    }
                }
            }
            else
            {
                ViewData["MostrarModal"] = false;
                ViewData["Mensaje"] = "Usuario no encontrado o contraseña incorrecta";
                return View("Login");
            }

            return View();
        }
        #endregion

        private void RegistrarAcceso(string correoUsuario, string tipoAcceso)
        {
            using (SqlConnection cn = new SqlConnection(_connectionString))
            {
                cn.Open();

                var idUsuario = cn.QuerySingleOrDefault<int?>(
                    "SELECT IdUsuario FROM USUARIO WHERE Correo = @Correo",
                    new { Correo = correoUsuario }
                );

                if (idUsuario.HasValue)
                {
                    string sql = "INSERT INTO Accesos (IdUsuario, FechaHora, TipoAcceso, IP) VALUES (@IdUsuario, GETDATE(), @TipoAcceso, @IP)";
                    cn.Execute(sql, new
                    {
                        IdUsuario = idUsuario.Value,
                        TipoAcceso = tipoAcceso,
                        IP = HttpContext.Connection.RemoteIpAddress.ToString()
                    });
                }
            }
        }

        public IActionResult SesionExpirada()
        {
            return View();
        }



        public IActionResult ActividadSospechosa()
        {
            return View();
        }

        #region Metodo para cerrar la Sesión
        public IActionResult CerrarSesion()
        {
            var perfilUsuarioJson = HttpContext.Session.GetString("PerfilUsuario");
            var perfilUsuario = JsonConvert.DeserializeObject<PerfilUsuario>(perfilUsuarioJson);
            var idUsuario = perfilUsuario.IdUsuario;

            using (SqlConnection cn = new SqlConnection(_connectionString))
            {
                cn.Execute("UPDATE USUARIO SET SesionActiva = 0 WHERE IdUsuario = @IdUsuario", new { IdUsuario = idUsuario });
            }

            HttpContext.Session.Clear();

            return RedirectToAction("Login", "Acceso");
        }
        #endregion

        #region Metodo para Salir con el tache
        [HttpPost]
        public IActionResult CerrarNavegador()
        {
            var perfilUsuarioJson = HttpContext.Session.GetString("PerfilUsuario");
            var perfilUsuario = JsonConvert.DeserializeObject<PerfilUsuario>(perfilUsuarioJson);
            var idUsuario = perfilUsuario.IdUsuario;

            using (SqlConnection cn = new SqlConnection(_connectionString))
            {
                cn.Execute("UPDATE USUARIO SET SesionActiva = 0 WHERE IdUsuario = @IdUsuario", new { IdUsuario = idUsuario });
            }

            HttpContext.Session.Clear();

            return RedirectToAction("Login", "Acceso");
        }
        #endregion



        #region Metodo Heartbeat

        //También maneja la muerte y límite de sesiones
        [HttpPost]
        public IActionResult Heartbeat()
        {
            try
            {
                var perfilUsuarioJson = HttpContext.Session.GetString("PerfilUsuario");
                if (string.IsNullOrEmpty(perfilUsuarioJson))
                {
                    _logger.LogWarning("Usuario no autenticado. Sesión expirada o no iniciada.");
                    return Unauthorized();
                }

                var perfilUsuario = JsonConvert.DeserializeObject<PerfilUsuario>(perfilUsuarioJson);
                var idUsuario = perfilUsuario.IdUsuario;
                var connectionString = _connectionString;

                DateTime ultimaActualizacion, horaInicioSesion, servidorTime;
                using (SqlConnection cn = new SqlConnection(connectionString))
                {
                    // Obtener la hora del servidor
                    servidorTime = cn.QueryFirst<DateTime>("SELECT SYSDATETIME()");
                    _logger.LogInformation($"Hora actual del servidor: {servidorTime}");

                    // Obtener información del usuario
                    var usuario = cn.QueryFirst("SELECT UltimaActualizacion, HoraInicioSesion FROM USUARIO WHERE IdUsuario = @IdUsuario", new { IdUsuario = idUsuario });
                    ultimaActualizacion = usuario.UltimaActualizacion;
                    horaInicioSesion = usuario.HoraInicioSesion;

                    _logger.LogInformation($"Hora de última actualización: {ultimaActualizacion}");
                    _logger.LogInformation($"Hora de inicio de sesión: {horaInicioSesion}");

                    // Actualizar la última actividad del usuario
                    cn.Execute("UPDATE USUARIO SET UltimaActualizacion = GETDATE() WHERE IdUsuario = @IdUsuario", new { IdUsuario = idUsuario });
                    _logger.LogInformation("Última actualización del usuario registrada.");
                }

                // Verificar si la sesión ha expirado por inactividad
                if (servidorTime > ultimaActualizacion.AddMinutes(10))
                {
                    _logger.LogWarning("Sesión ha expirado (más de 10 minutos sin actividad).");
                    return Unauthorized(); // La sesión ha expirado
                }

                // Verificar si la sesión ha alcanzado el límite de tiempo total
                if (servidorTime > horaInicioSesion.AddMinutes(30))
                {
                    _logger.LogInformation("Sesión ha alcanzado el límite de tiempo (30 minutos).");
                    return Unauthorized(); // La sesión ha alcanzado el límite de tiempo
                }

                // Advertir si faltan 5 minutos para la expiración
                if (servidorTime > horaInicioSesion.AddMinutes(25))
                {
                    _logger.LogInformation("Advertencia: la sesión está a punto de expirar.");
                    return Ok(new { ExpiracionCercana = true });
                }

                _logger.LogInformation("Sesión aún activa, sin advertencias.");
                return Ok(new { ExpiracionCercana = false });
            }
            catch (Exception ex)
            {
                // Registrar el error
                _logger.LogError(ex, "Error en Heartbeat.");
                return StatusCode(500, "Error interno del servidor");
            }
        }

        #endregion


        [HttpPost]
        public IActionResult ActualizarInicioSesion()
        {
            try
            {
                // Obtener el perfil del usuario desde la sesión
                var perfilUsuarioJson = HttpContext.Session.GetString("PerfilUsuario");
                if (string.IsNullOrEmpty(perfilUsuarioJson))
                {
                    return Unauthorized(); // Sesión no válida
                }

                var perfilUsuario = JsonConvert.DeserializeObject<PerfilUsuario>(perfilUsuarioJson);
                var idUsuario = perfilUsuario.IdUsuario;

                using (SqlConnection cn = new SqlConnection(_connectionString))
                {
                    // Actualizar la hora de inicio de sesión a la hora actual del servidor
                    cn.Execute("UPDATE USUARIO SET HoraInicioSesion = GETDATE() WHERE IdUsuario = @IdUsuario", new { IdUsuario = idUsuario });
                }

                // Retornar una respuesta exitosa
                return Ok(); // Actualización exitosa
            }
            catch (Exception ex)
            {
                Console.WriteLine("Error al actualizar la hora de inicio de sesión: " + ex.Message);
                return StatusCode(500, "Error interno del servidor");
            }
        }




        public IActionResult Registrar()
        {
            return View();
        }

        [HttpPost]
        public IActionResult Registrar(Usuario oUsuario)
        {
            if (oUsuario.Clave != oUsuario.ConfirmarClave)
            {
                ViewData["Mensaje"] = "Las contraseñas no coinciden";
                return View();
            }

            oUsuario.Clave = ConvertirSha256(oUsuario.Clave);

            using (SqlConnection cn = new SqlConnection(_connectionString))
            {
                SqlCommand cmd = new SqlCommand("sp_RegistrarUsuario", cn);
                cmd.Parameters.AddWithValue("Correo", oUsuario.Correo);
                cmd.Parameters.AddWithValue("Clave", oUsuario.Clave);
                cmd.Parameters.Add("Registrado", SqlDbType.Bit).Direction = ParameterDirection.Output;
                cmd.Parameters.Add("Mensaje", SqlDbType.VarChar, 200).Direction = ParameterDirection.Output;
                cmd.CommandType = CommandType.StoredProcedure;

                cn.Open();
                cmd.ExecuteNonQuery();

                bool registrado = Convert.ToBoolean(cmd.Parameters["Registrado"].Value);
                string mensaje = cmd.Parameters["Mensaje"].Value.ToString();

                ViewData["Mensaje"] = mensaje;

                if (registrado)
                {
                    return RedirectToAction("Login", "Acceso");
                }
                else
                {
                    return View();
                }
            }
        }

        public static string ConvertirSha256(string texto)
        {
            using (SHA256 hash = SHA256Managed.Create())
            {
                byte[] result = hash.ComputeHash(Encoding.UTF8.GetBytes(texto));
                return string.Concat(result.Select(b => b.ToString("x2")));
            }
        }

        public IActionResult ForgotPassword()
        {
            return View();
        }

        [HttpPost]
        public async Task<IActionResult> ForgotPassword(string Correo)
        {
            var user = await _repositorioAcceso.GetUserByEmail(Correo);
            var logo = "https://cdn.sassoapps.com/img_snier/login/logo_snier.png";

            if (user == null)
            {
                ViewData["Mensaje"] = "La dirección de correo no está asociada con una cuenta, verifica tus datos.";
                return View();
            }

            var token = GenerateToken();
            await SavePasswordResetToken(user.IdUsuario, token);

            var callbackUrl = Url.Action("ResetPassword", "Acceso", new { token }, protocol: HttpContext.Request.Scheme);
            var mensaje = EmailReinstatement(logo, user.Nombre, token, callbackUrl);

            try
            {
                await _servicioEmailSMTP.EnviarCorreo(Correo, "Restablecer contraseña", mensaje);

                ViewData["EsExitoso"] = true;
                ViewData["Mensaje"] = "Se ha enviado un enlace de restablecimiento a su dirección de correo electrónico.";
                return View();
            }
            catch (Exception ex)
            {
                ViewData["EsExitoso"] = false;
                ViewData["Mensaje"] = "Hubo un error al enviar el correo electrónico. Por favor, inténtelo de nuevo más tarde.";
                return View();
            }
        }

        private string EmailReinstatement(string logo, string nombre, string token, string url)
        {
            return $@"
                <html lang='es'>
                <head>
                    <meta charset='UTF-8'>
                    <meta http-equiv='X-UA-Compatible' content='IE=edge' />
                    <title>Restablecimiento de Contraseña</title>
                    <meta name='viewport' content='width=device-width, initial-scale=1.0'/>
                    <style>
                        .ReadMsgBody {{width: 100%; background-color: #ffffff;}}
                        .ExternalClass {{width: 100%; background-color: #ffffff;}}
                        @-ms-viewport {{width: device-width;}}
                    </style>
                </head>
                <body style='background: #ffffff; width: 100%; height: 100%; margin: 0; padding: 0; font-family: Montserrat, sans-serif;'>
                    <center class='wrapper' style='padding-top: 5%; width: 100%; max-width: 960px;'>
                        <div class='webkit'>
                            <table cellpadding='0' cellspacing='0' border='0' bgcolor='#ffffff' style='width: 100%; max-width: 960px;'>
                                <tbody>
                                    <tr>
                                        <td align='center'>
                                            <img src='{logo}' alt='Logo' width='120px' height='100px'>
                                            <h1>¡Hola, {nombre}!</h1>
                                            <p>Por favor, usa el siguiente enlace para restablecer tu contraseña. Recuerda que el enlace expirará en 30 minutos.</p>
                                            <a href='{url}' style='padding: 10px; background-color: #8BC34A; color: white; text-decoration: none; border-radius: 5px; display: inline-block;'>Restablecer Contraseña</a>
                                            <p>*Este correo se genera automáticamente y no requiere respuesta.</p>
                                            <p>Sin otro particular, reciba un cordial saludo.</p>
                                        </td>
                                    </tr>
                                </tbody>
                            </table>
                        </div>
                    </center>
                </body>
                </html>";
        }

        private string EmailExpiration(string logo, string nombre, string token, string url)
        {
            return $@"
                <html lang='es'>
                <head>
                    <meta charset='UTF-8'>
                    <meta http-equiv='X-UA-Compatible' content='IE=edge' />
                    <title>Restablecimiento de Contraseña</title>
                    <meta name='viewport' content='width=device-width, initial-scale=1.0'/>
                    <style>
                        .ReadMsgBody {{width: 100%; background-color: #ffffff;}}
                        .ExternalClass {{width: 100%; background-color: #ffffff;}}
                        @-ms-viewport {{width: device-width;}}
                    </style>
                </head>
                <body style='background: #ffffff; width: 100%; height: 100%; margin: 0; padding: 0; font-family: Montserrat, sans-serif;'>
                    <center class='wrapper' style='padding-top: 5%; width: 100%; max-width: 960px;'>
                        <div class='webkit'>
                            <table cellpadding='0' cellspacing='0' border='0' bgcolor='#ffffff' style='width: 100%; max-width: 960px;'>
                                <tbody>
                                    <tr>
                                        <td align='center'>
                                            <img src='{logo}' alt='Logo' width='120px' height='100px'>
                                            <h1>¡Hola, {nombre}!</h1>
                                            <p>Su Token anterior ha expirado. Use el siguiente enlace para restablecer su contraseña (Recuerde que tiene 30 minutos antes de que su Token expire):</p>
                                            <a href='{url}' style='padding: 10px; background-color: #8BC34A; color: white; text-decoration: none; border-radius: 5px; display: inline-block;'>Restablecer Contraseña</a>
                                            <p>Tu Token de Seguridad es:<strong> {token}</strong></p>
                                            <p>Deberás usarlo para recuperar tu contraseña.</p>
                                            <p>*Este correo se genera automáticamente y no requiere respuesta.</p>
                                            <p>Sin otro particular, reciba un cordial saludo.</p>
                                        </td>
                                    </tr>
                                </tbody>
                            </table>
                        </div>
                    </center>
                </body>
                </html>";
        }

        private string EmailConfirmed(string logo, string nombre)
        {
            return $@"
                <html lang='es'>
                <head>
                    <meta charset='UTF-8'>
                    <meta http-equiv='X-UA-Compatible' content='IE=edge' />
                    <title>Restablecimiento de Contraseña</title>
                    <meta name='viewport' content='width=device-width, initial-scale=1.0'/>
                    <style>
                        .ReadMsgBody {{width: 100%; background-color: #ffffff;}}
                        .ExternalClass {{width: 100%; background-color: #ffffff;}}
                        @-ms-viewport {{width: device-width;}}
                    </style>
                </head>
                <body style='background: #ffffff; width: 100%; height: 100%; margin: 0; padding: 0; font-family: Montserrat, sans-serif;'>
                    <center class='wrapper' style='padding-top: 5%; width: 100%; max-width: 960px;'>
                        <div class='webkit'>
                            <table cellpadding='0' cellspacing='0' border='0' bgcolor='#ffffff' style='width: 100%; max-width: 960px;'>
                                <tbody>
                                    <tr>
                                        <td align='center'>
                                            <img src='{logo}' alt='Logo' width='120px' height='100px'>
                                            <h1>¡Hola, {nombre}!</h1>
                                            <p>Le informamos que su contraseña ha sido restablecida correctamente.</p>
                                            <p>*Este correo se genera automáticamente y no requiere respuesta.</p>
                                            <p>Sin otro particular, reciba un cordial saludo.</p>
                                        </td>
                                    </tr>
                                </tbody>
                            </table>
                        </div>
                    </center>
                </body>
                </html>";
        }

        private string GenerateToken()
        {
            using (var rng = new RNGCryptoServiceProvider())
            {
                byte[] tokenData = new byte[32];
                rng.GetBytes(tokenData);
                return Convert.ToBase64String(tokenData);
            }
        }

        private async Task SavePasswordResetToken(int userId, string token)
        {
            await _repositorioAcceso.SavePasswordResetToken(userId, token, DateTime.Now);
        }

        public IActionResult ResetPassword(string token)
        {
            ViewData["Token"] = token;
            Console.WriteLine("Token: " + token);
            return View();
        }

        [HttpPost]
        public async Task<IActionResult> ResetPassword(string Token, string Clave, string ConfirmarClave)
        {
            var user = await _repositorioAcceso.GetUserByPasswordResetToken(Token);
            var logo = "https://cdn.sassoapps.com/img_snier/login/logo_snier.png";

            if (user == null)
            {
                ViewData["Mensaje"] = "El token es inválido o ha sido utilizado.";
                return View();
            }

            if (DateTime.Now.Subtract(user.Fecha).TotalMinutes > 30)
            {
                ViewData["Mensaje"] = "El Token ha expirado, le enviaremos uno nuevo a su correo electrónico.";

                var nuevoToken = GenerateToken();
                await SavePasswordResetToken(user.IdUsuario, nuevoToken);

                var usuario = await _repositorioAcceso.GetUserById(user.IdUsuario);
                if (usuario == null)
                {
                    return View("Error");
                }

                var correoUsuario = usuario.Correo;
                var nuevoCallbackUrl = Url.Action("ResetPassword", "Acceso", new { token = nuevoToken }, protocol: HttpContext.Request.Scheme);
                var nuevoMensaje = EmailExpiration(logo, usuario.Nombre, nuevoToken, nuevoCallbackUrl);
                await _servicioEmailSMTP.EnviarCorreo(correoUsuario, "Nuevo restablecimiento de contraseña", nuevoMensaje);

                return View();
            }

            if (Token != user.Token)
            {
                ViewData["Mensaje"] = "El token ingresado no coincide.";
                return View();
            }

            if (Clave != ConfirmarClave)
            {
                ViewData["Mensaje"] = "Las contraseñas no coinciden.";
                return View();
            }

            var hashedPassword = ConvertirSha256(Clave);
            await _repositorioAcceso.UpdatePassword(user.IdUsuario, hashedPassword);

            var usuarioFinal = await _repositorioAcceso.GetUserById(user.IdUsuario);
            if (usuarioFinal == null)
            {
                return View("Error");
            }

            var correoUsuarioFinal = usuarioFinal.Correo;
            var nuevoMensajeFinal = EmailConfirmed(logo, usuarioFinal.Nombre);
            await _servicioEmailSMTP.EnviarCorreo(correoUsuarioFinal, "Restablecimiento de contraseña exitoso", nuevoMensajeFinal);

            await _repositorioAcceso.DeletePasswordResetToken(user.IdUsuario);

            ViewData["EsExitoso"] = true;
            ViewData["Mensaje"] = "La contraseña se ha restablecido correctamente.";

            return View();
        }

        [HttpPost]
        public async Task<IActionResult> ResetPasswordUser(string Clave, string ConfirmarClave, int IdUsuario)
        {
            var logo = "https://cdn.sassoapps.com/img_snier/login/logo_snier.png";

            if (Clave != ConfirmarClave)
            {
                return BadRequest("Las contraseñas no coinciden.");
            }

            var hashedPassword = ConvertirSha256(Clave);
            await _repositorioAcceso.UpdatePassword(IdUsuario, hashedPassword);

            var usuarioFinal = await _repositorioAcceso.GetUserById(IdUsuario);
            if (usuarioFinal == null)
            {
                return NotFound("No se encontró el usuario.");
            }

            var correoUsuarioFinal = usuarioFinal.Correo;
            var nuevoMensajeFinal = EmailConfirmed(logo, usuarioFinal.Nombre);
            await _servicioEmailSMTP.EnviarCorreo(correoUsuarioFinal, "Restablecimiento de contraseña exitoso", nuevoMensajeFinal);

            return Ok("La contraseña se ha restablecido correctamente.");
        }

        public async Task<IActionResult> Monitoreo()
        {
            var totalAccesos = await _repositorioAcceso.GetTotalAccessCountAsync();
            var fechaInicio = new DateTime(2023, 1, 1);
            var fechaFin = new DateTime(2030, 12, 31);
            var detallesAcceso = await _repositorioAcceso.GetDetallesAccesoAsync(fechaInicio, fechaFin);
            var totalAccesosPorTipo = await _repositorioAcceso.GetTotalAccessCountByTypeAsync(fechaInicio, fechaFin);

            var ultimoAccesoPorUsuario = detallesAcceso
                .GroupBy(da => da.Nombre)
                .Select(g => new UltimoAccesoUsuario
                {
                    Nombre = g.Key,
                    UltimoAcceso = g.Max(x => x.FechaHoraLocal)
                })
                .ToList();

            var accesosPorCargo = detallesAcceso
                .GroupBy(da => da.Cargo)
                .Select(g => new { Cargo = g.Key, TotalAccesos = g.Count() })
                .ToList();

            var accesosPorFecha = detallesAcceso
                .GroupBy(da => da.FechaHoraLocal.Date)
                .Select(g => new { Fecha = g.Key, TotalAccesos = g.Count() })
                .OrderBy(g => g.Fecha)
                .ToList();

            ViewBag.AccesosPorFechaJson = JsonConvert.SerializeObject(accesosPorFecha);

            var accesosPorUnidad = detallesAcceso
                 .GroupBy(da => da.UnidadDeAdscripcion)
                 .Select(g => new { UnidadDeAdscripcion = g.Key, TotalAccesos = g.Count() })
                 .ToList();

            ViewBag.AccesosPorUnidadJson = JsonConvert.SerializeObject(accesosPorUnidad);
            var ipsUnicas = detallesAcceso.Select(da => da.IP).Distinct().ToList();
            ViewBag.IpsUnicas = ipsUnicas;

            var viewModel = new MonitoreoViewModel
            {
                TotalAccesos = totalAccesos,
                DetallesAcceso = detallesAcceso,
                TotalAccesosPorTipo = totalAccesosPorTipo,
                UltimoAccesoPorUsuario = ultimoAccesoPorUsuario
            };

            ViewBag.JsonModel = JsonConvert.SerializeObject(viewModel);
            ViewBag.AccesosPorCargoJson = JsonConvert.SerializeObject(accesosPorCargo);

            return View(viewModel);
        }

        public IActionResult Registro()
        {
            return View();
        }

        [HttpPost]
        public IActionResult ProcesarRegistro(IFormCollection form, IFormFileCollection files)
        {
            // Aquí iría la lógica de procesamiento del registro
            // Por ahora solo redirigimos con un mensaje de éxito
            TempData["RegistroExitoso"] = "Registro enviado correctamente. En breve recibirá un correo de confirmación.";
            return RedirectToAction("Login");
        }
    }
}
