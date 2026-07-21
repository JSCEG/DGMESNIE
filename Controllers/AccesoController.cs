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
        private const string SpValidarUsuario = "dgmesnie.sp_ValidarUsuario";
        private const string SpObtenerPerfilSesion = "dgmesnie.sp_ObtenerPerfilSesion";
        private const string SpObtenerMenuUsuario = "dgmesnie.sp_ObtenerSeccionesYModulosPorUsuario";
        private const string SpRegistrarSesion = "dgmesnie.sp_RegistrarSesion";
        private const string SpCerrarSesion = "dgmesnie.sp_CerrarSesion";
        private const string SpActualizarActividadSesion = "dgmesnie.sp_ActualizarActividadSesion";
        private const int MinutosInactividadSesion = 120; // 2 horas (antes 10)
        private const int MinutosDuracionSesion = 480; // 8 horas (antes 30)

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

        [HttpGet]
        public IActionResult DevBypass(int id)
        {
            using (var cn = new SqlConnection(_connectionString))
            {
                cn.Open();
                var exists = cn.QuerySingleOrDefault<int>("SELECT COUNT(1) FROM [dgmesnie].[Usuario] WHERE [IdUsuario] = @IdUsuario AND [Vigente] = 1", new { IdUsuario = id });
                if (exists == 0)
                {
                    return Content($"Usuario con ID {id} no existe o no está vigente.");
                }
            }
            return CompletarInicioSesion(id, false, null);
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
                ExpiresUtc = DateTimeOffset.UtcNow.AddHours(8) // Expira en 8 horas
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
            try
            {
                oUsuario.Clave = ConvertirSha256(oUsuario.Clave);

                Console.WriteLine("Clave: " + oUsuario.Clave);
                oUsuario.IdUsuario = ValidarUsuario(oUsuario.Correo, oUsuario.Clave);
            }
            catch (SqlException ex)
            {
                _logger.LogError(ex, "Error de conexión SQL durante el acceso como invitado.");
                ViewData["MostrarModal"] = false;
                ViewData["Mensaje"] = "No fue posible conectar con la base de datos. Verifica la configuración de acceso e inténtalo nuevamente.";
                return View("Login");
            }

            if (oUsuario.IdUsuario == 0)
            {
                ViewData["MostrarModal"] = false;
                ViewData["Mensaje"] = "Usuario no encontrado o contraseña incorrecta";
                return View("Login");
            }

            return CompletarInicioSesion(oUsuario.IdUsuario, false, null);
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

            try
            {
                oUsuario.Clave = ConvertirSha256(oUsuario.Clave);
                oUsuario.IdUsuario = ValidarUsuario(oUsuario.Correo, oUsuario.Clave);
            }
            catch (SqlException ex)
            {
                _logger.LogError(ex, "Error de conexión SQL durante el inicio de sesión para {Correo}.", oUsuario.Correo);
                ViewData["MostrarModal"] = false;
                ViewData["Mensaje"] = "No fue posible conectar con la base de datos. Verifica la configuración de acceso e inténtalo nuevamente.";
                return View("Login");
            }

            if (oUsuario.IdUsuario == 0)
            {
                ViewData["MostrarModal"] = false;
                ViewData["Mensaje"] = "Usuario no encontrado o contraseña incorrecta";
                return View("Login");
            }

            return CompletarInicioSesion(oUsuario.IdUsuario, registrarAcceso, "Inicio de sesión funcionario SENER");
        }
        #endregion

        private void RegistrarAcceso(string correoUsuario, string tipoAcceso)
        {
            using (SqlConnection cn = new SqlConnection(_connectionString))
            {
                cn.Open();

                var idUsuario = cn.QuerySingleOrDefault<int?>(
                    "SELECT [IdUsuario] FROM [dgmesnie].[Usuario] WHERE [Correo] = @Correo AND [Vigente] = 1",
                    new { Correo = correoUsuario }
                );

                if (idUsuario.HasValue)
                {
                    string sql = @"INSERT INTO [dgmesnie].[Acceso]
                                   ([IdUsuario], [Correo], [TipoAcceso], [FechaAcceso], [Ip], [Exitoso])
                                   VALUES (@IdUsuario, @Correo, @TipoAcceso, SYSUTCDATETIME(), @IP, 1)";
                    cn.Execute(sql, new
                    {
                        IdUsuario = idUsuario.Value,
                        Correo = correoUsuario,
                        TipoAcceso = tipoAcceso,
                        IP = ClienteIpHelper.ObtenerIpCliente(HttpContext)
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
            CerrarSesionActiva();

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
            CerrarSesionActiva();

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

                    var sesion = cn.QueryFirstOrDefault(
                        @"SELECT TOP (1) [UltimaActividad], [FechaInicio]
                          FROM [dgmesnie].[Sesion]
                          WHERE [SessionKey] = @SessionKey AND [Activa] = 1
                          ORDER BY [UltimaActividad] DESC",
                        new { SessionKey = HttpContext.Session.Id });

                    if (sesion == null)
                    {
                        _logger.LogWarning("No se encontró sesión activa en dgmesnie.");
                        return Unauthorized();
                    }

                    ultimaActualizacion = sesion.UltimaActividad;
                    horaInicioSesion = sesion.FechaInicio;

                    _logger.LogInformation($"Hora de última actualización: {ultimaActualizacion}");
                    _logger.LogInformation($"Hora de inicio de sesión: {horaInicioSesion}");

                    // Actualizar la última actividad del usuario
                    cn.Execute(
                        SpActualizarActividadSesion,
                        new { SessionKey = HttpContext.Session.Id },
                        commandType: CommandType.StoredProcedure
                    );
                    _logger.LogInformation("Última actualización del usuario registrada.");
                }

                // Verificar si la sesión ha expirado por inactividad
                if (servidorTime > ultimaActualizacion.AddMinutes(MinutosInactividadSesion))
                {
                    _logger.LogWarning("Sesión ha expirado (más de 10 minutos sin actividad).");
                    return Unauthorized(); // La sesión ha expirado
                }

                // Verificar si la sesión ha alcanzado el límite de tiempo total
                if (servidorTime > horaInicioSesion.AddMinutes(MinutosDuracionSesion))
                {
                    _logger.LogInformation("Sesión ha alcanzado el límite de tiempo (30 minutos).");
                    return Unauthorized(); // La sesión ha alcanzado el límite de tiempo
                }

                // Advertir si faltan 5 minutos para la expiración
                if (servidorTime > horaInicioSesion.AddMinutes(MinutosDuracionSesion - 5))
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
                RegistrarSesionActiva(int.Parse(perfilUsuario.IdUsuario));

                // Retornar una respuesta exitosa
                return Ok(); // Actualización exitosa
            }
            catch (Exception ex)
            {
                Console.WriteLine("Error al actualizar la hora de inicio de sesión: " + ex.Message);
                return StatusCode(500, "Error interno del servidor");
            }
        }

        private int ValidarUsuario(string correoRFC, string claveHash)
        {
            using var cn = new SqlConnection(_connectionString);
            cn.Open();

            return cn.QuerySingleOrDefault<int>(
                SpValidarUsuario,
                new { CorreoRFC = correoRFC, Clave = claveHash },
                commandType: CommandType.StoredProcedure
            );
        }

        private IActionResult CompletarInicioSesion(int idUsuario, bool registrarAcceso, string tipoAcceso)
        {
            using var cn = new SqlConnection(_connectionString);
            cn.Open();

            bool esVigente = cn.QuerySingleOrDefault<bool>(
                "SELECT [Vigente] FROM [dgmesnie].[Usuario] WHERE [IdUsuario] = @IdUsuario",
                new { IdUsuario = idUsuario }
            );

            if (!esVigente)
            {
                ViewData["MostrarModal"] = false;
                ViewData["Mensaje"] = "Lo sentimos, su usuario no tiene acceso a la plataforma";
                return View("Login");
            }

            if (registrarAcceso && !string.IsNullOrWhiteSpace(tipoAcceso))
            {
                var correoUsuario = cn.QuerySingleOrDefault<string>(
                    "SELECT [Correo] FROM [dgmesnie].[Usuario] WHERE [IdUsuario] = @IdUsuario",
                    new { IdUsuario = idUsuario }
                );

                if (!string.IsNullOrWhiteSpace(correoUsuario))
                {
                    RegistrarAcceso(correoUsuario, tipoAcceso);
                }
            }

            RegistrarSesionActiva(idUsuario);

            PerfilUsuario perfilUsuario = cn.QuerySingleOrDefault<PerfilUsuario>(
                SpObtenerPerfilSesion,
                new { IdUsuario = idUsuario },
                commandType: CommandType.StoredProcedure
            );

            if (perfilUsuario == null)
            {
                ViewData["MostrarModal"] = false;
                ViewData["Mensaje"] = "No fue posible cargar el perfil de sesión";
                return View("Login");
            }

            var perfilUsuarioJson = JsonConvert.SerializeObject(perfilUsuario);
            HttpContext.Session.SetString("PerfilUsuario", perfilUsuarioJson);

            var seccionesAgrupadas = ObtenerSeccionesUsuario(cn, idUsuario);
            var seccionesUsuarioJson = JsonConvert.SerializeObject(seccionesAgrupadas);
            HttpContext.Session.SetString("SeccionesUsuario", seccionesUsuarioJson);

            var primerModuloExterno = seccionesAgrupadas
                .SelectMany(s => s.Modulos)
                .FirstOrDefault(m => m.EsExterno && !string.IsNullOrWhiteSpace(m.Action));

            if (primerModuloExterno != null)
            {
                return Redirect(primerModuloExterno.Action);
            }

            bool tieneGestor = seccionesAgrupadas
                .SelectMany(s => s.Modulos)
                .Any(m => string.Equals(m.Controller, "Gestor", StringComparison.OrdinalIgnoreCase));

            if (tieneGestor)
            {
                return RedirectToAction("Index", "Gestor");
            }
            else
            {
                return RedirectToAction("Index", "Home");
            }
        }

        private List<SeccionSNIER> ObtenerSeccionesUsuario(SqlConnection cn, int idUsuario)
        {
            var seccionesDict = new Dictionary<int, SeccionSNIER>();

            cn.Query<SeccionSNIER, ModuloSNIER, VistaSNIER, int>(
                SpObtenerMenuUsuario,
                (seccion, modulo, vista) =>
                {
                    if (!seccionesDict.TryGetValue(seccion.Id, out var seccionExistente))
                    {
                        seccionExistente = seccion;
                        seccionExistente.Modulos = new List<ModuloSNIER>();
                        seccionesDict[seccion.Id] = seccionExistente;
                    }

                    if (modulo != null && modulo.ModuloId != 0)
                    {
                        var modExistente = seccionExistente.Modulos.FirstOrDefault(m => m.ModuloId == modulo.ModuloId);
                        if (modExistente == null)
                        {
                            modExistente = modulo;
                            modExistente.Vistas = new List<VistaSNIER>();

                            if (modulo.Controller == "EXTERNA")
                            {
                                modExistente.EsExterno = true;
                            }

                            seccionExistente.Modulos.Add(modExistente);
                        }

                        if (vista != null && vista.VistaId != 0 && !modExistente.Vistas.Any(v => v.VistaId == vista.VistaId))
                        {
                            modExistente.Vistas.Add(vista);
                        }
                    }

                    return seccionExistente.Id;
                },
                new { IdUsuario = idUsuario },
                splitOn: "ModuloId,VistaId",
                commandType: CommandType.StoredProcedure
            );

            return seccionesDict.Values.ToList();
        }

        private void RegistrarSesionActiva(int idUsuario)
        {
            using var cn = new SqlConnection(_connectionString);
            cn.Open();

            cn.Execute(
                SpRegistrarSesion,
                new
                {
                    IdUsuario = idUsuario,
                    SessionKey = HttpContext.Session.Id,
                    FechaExpiracion = DateTime.UtcNow.AddMinutes(MinutosDuracionSesion),
                    Ip = ClienteIpHelper.ObtenerIpCliente(HttpContext),
                    UserAgent = Request.Headers.UserAgent.ToString(),
                    OrigenAcceso = "WEB"
                },
                commandType: CommandType.StoredProcedure
            );
        }

        private void CerrarSesionActiva()
        {
            using var cn = new SqlConnection(_connectionString);
            cn.Open();

            cn.Execute(
                SpCerrarSesion,
                new { SessionKey = HttpContext.Session.Id },
                commandType: CommandType.StoredProcedure
            );
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
            using (SHA256 hash = SHA256.Create())
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
            _logger.LogInformation("ForgotPassword recibido para correo: {Correo}", Correo);

            var user = await _repositorioAcceso.GetUserByEmail(Correo);
            if (user == null)
            {
                _logger.LogWarning("ForgotPassword sin usuario asociado para correo: {Correo}", Correo);
                ViewData["Mensaje"] = "La dirección de correo no está asociada con una cuenta, verifica tus datos.";
                return View();
            }

            var token = GenerateToken();
            await SavePasswordResetToken(user.IdUsuario, token);

            var callbackUrl = Url.Action("ResetPassword", "Acceso", new { token }, protocol: HttpContext.Request.Scheme);
            var mensaje = EmailReinstatement(user.Nombre, callbackUrl);

            try
            {
                await _servicioEmailSMTP.EnviarCorreo(Correo, "Restablecer contraseña", mensaje);
                _logger.LogInformation("ForgotPassword: correo enviado correctamente a {Correo}", Correo);

                ViewData["EsExitoso"] = true;
                ViewData["Mensaje"] = "Se ha enviado un enlace de restablecimiento a su dirección de correo electrónico.";
                return View();
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "ForgotPassword: error al enviar correo a {Correo}", Correo);
                ViewData["EsExitoso"] = false;
                ViewData["Mensaje"] = "Hubo un error al enviar el correo electrónico. Por favor, inténtelo de nuevo más tarde.";
                ViewData["DetalleError"] = ex.Message;
                return View();
            }
        }

        private string EmailReinstatement(string nombre, string url)
        {
            var contenido = @"
                <p style='margin:0 0 14px;'>Hemos recibido una solicitud para restablecer tu contraseña en la plataforma NSIE.</p>
                <p style='margin:0 0 18px;'>Por seguridad, este enlace tendrá vigencia de 30 minutos.</p>";

            return BuildInstitutionalEmail(
                titulo: "Restablecimiento de contraseña",
                nombre: nombre,
                contenidoHtml: contenido,
                botonTexto: "Restablecer contraseña",
                botonUrl: url);
        }

        private string EmailExpiration(string nombre, string token, string url)
        {
            var detalle = $@"
                <table role='presentation' cellpadding='0' cellspacing='0' border='0' style='width:100%; border-collapse:collapse; margin:16px 0 6px;'>
                    <tr>
                        <td style='background:#f7ecf1; color:#6b1034; font-weight:700; padding:10px 12px; border:1px solid #e5c7d4; width:35%;'>Token nuevo</td>
                        <td style='padding:10px 12px; border:1px solid #eadde4; color:#2b2b2b;'>{token}</td>
                    </tr>
                </table>";

            var contenido = @"
                <p style='margin:0 0 14px;'>Tu token anterior expiró. Ya generamos uno nuevo para continuar con el proceso.</p>
                <p style='margin:0 0 18px;'>Utiliza el siguiente botón y completa el cambio de contraseña dentro de los próximos 30 minutos.</p>";

            return BuildInstitutionalEmail(
                titulo: "Nuevo enlace de restablecimiento",
                nombre: nombre,
                contenidoHtml: contenido,
                tablaHtml: detalle,
                botonTexto: "Restablecer contraseña",
                botonUrl: url);
        }

        private string EmailConfirmed(string nombre)
        {
            var contenido = @"
                <p style='margin:0 0 14px;'>La contraseña de tu cuenta fue restablecida correctamente.</p>
                <p style='margin:0 0 18px;'>Si no reconoces esta acción, repórtala de inmediato al equipo administrador del sistema.</p>";

            var loginUrl = Url.Action("Login", "Acceso", null, protocol: HttpContext.Request.Scheme) ?? string.Empty;
            return BuildInstitutionalEmail(
                titulo: "Contraseña actualizada",
                nombre: nombre,
                contenidoHtml: contenido,
                botonTexto: "Ir al inicio de sesión",
                botonUrl: loginUrl);
        }

        private string EmailPasswordChangedFromAccount(string nombre)
        {
            var contenido = @"
                <p style='margin:0 0 14px;'>La contraseña de tu cuenta fue actualizada desde el panel institucional.</p>
                <p style='margin:0 0 18px;'>Si no realizaste este cambio, notifica inmediatamente al administrador del sistema.</p>";

            var loginUrl = Url.Action("Login", "Acceso", null, protocol: HttpContext.Request.Scheme) ?? string.Empty;
            return BuildInstitutionalEmail(
                titulo: "Cambio de contraseña exitoso",
                nombre: nombre,
                contenidoHtml: contenido,
                botonTexto: "Ir al inicio de sesión",
                botonUrl: loginUrl);
        }

        private string BuildInstitutionalEmail(
            string titulo,
            string nombre,
            string contenidoHtml,
            string? tablaHtml = null,
            string? botonTexto = null,
            string? botonUrl = null)
        {
            var botonHtml = string.Empty;
            if (!string.IsNullOrWhiteSpace(botonTexto) && !string.IsNullOrWhiteSpace(botonUrl))
            {
                botonHtml = $@"
                    <div style='margin:18px 0 16px; text-align:center;'>
                        <a href='{botonUrl}' style='display:inline-block; padding:12px 20px; border-radius:8px; background:#8a0031; color:#ffffff; text-decoration:none; font-weight:700;'>
                            {botonTexto}
                        </a>
                    </div>";
            }

            return $@"
                <html lang='es'>
                <head>
                    <meta charset='UTF-8'>
                    <meta http-equiv='X-UA-Compatible' content='IE=edge' />
                    <meta name='viewport' content='width=device-width, initial-scale=1.0'/>
                    <title>{titulo}</title>
                </head>
                <body style='margin:0; padding:22px; background:#f2f2f2; font-family:Arial, Helvetica, sans-serif; color:#222;'>
                    <table role='presentation' cellpadding='0' cellspacing='0' border='0' style='width:100%; max-width:760px; margin:0 auto; background:#ffffff; border:1px solid #dfdfdf; border-radius:10px; overflow:hidden;'>
                        <tr>
                            <td style='padding:16px 20px; border-bottom:1px solid #eee;'>
                                <table role='presentation' cellpadding='0' cellspacing='0' border='0' style='width:100%;'>
                                    <tr>
                                        <td style='width:50%;'>
                                            <img src='https://cdn.sassoapps.com/dgmesnie/logo_gob.png' alt='Gobierno de México' style='max-height:40px; width:auto;'>
                                        </td>
                                        <td style='width:50%; text-align:right;'>
                                            <img src='https://cdn.sassoapps.com/dgmesnie/logo_sener.png' alt='Secretaría de Energía' style='max-height:42px; width:auto;'>
                                        </td>
                                    </tr>
                                </table>
                            </td>
                        </tr>
                        <tr>
                            <td style='background:#8a0031; color:#ffffff; padding:16px 20px; font-size:20px; font-weight:700;'>
                                {titulo}
                            </td>
                        </tr>
                        <tr>
                            <td style='padding:22px 20px;'>
                                <p style='margin:0 0 12px; font-size:18px; font-weight:700; color:#1f2937;'>Hola, {nombre}.</p>
                                {contenidoHtml}
                                {tablaHtml}
                                {botonHtml}
                                <p style='margin:18px 0 0; font-size:13px; color:#555;'>
                                    Este correo se genera automáticamente y no requiere respuesta.
                                </p>
                            </td>
                        </tr>
                    </table>
                </body>
                </html>";
        }

        private string GenerateToken()
        {
            using (var rng = RandomNumberGenerator.Create())
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
                var nuevoMensaje = EmailExpiration(usuario.Nombre, nuevoToken, nuevoCallbackUrl);
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
            var nuevoMensajeFinal = EmailConfirmed(usuarioFinal.Nombre);
            await _servicioEmailSMTP.EnviarCorreo(correoUsuarioFinal, "Restablecimiento de contraseña exitoso", nuevoMensajeFinal);

            await _repositorioAcceso.DeletePasswordResetToken(user.IdUsuario);

            ViewData["EsExitoso"] = true;
            ViewData["Mensaje"] = "La contraseña se ha restablecido correctamente.";

            return View();
        }

        [HttpPost]
        public async Task<IActionResult> ResetPasswordUser(string Clave, string ConfirmarClave, int IdUsuario)
        {
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
            var nuevoMensajeFinal = EmailPasswordChangedFromAccount(usuarioFinal.Nombre);

            try
            {
                await _servicioEmailSMTP.EnviarCorreo(correoUsuarioFinal, "Cambio de contraseña exitoso", nuevoMensajeFinal);
                return Ok("La contraseña se actualizó correctamente.");
            }
            catch (Exception ex)
            {
                _logger.LogWarning(ex, "La contraseña del usuario {IdUsuario} se actualizó, pero no fue posible enviar el correo de confirmación.", IdUsuario);
                return Ok("La contraseña se actualizó correctamente, pero no fue posible enviar el correo de confirmación.");
            }
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
