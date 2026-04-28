using Microsoft.AspNetCore.Mvc;
using NSIE.Servicios;
using NSIE.Models;
using System.Threading.Tasks;
using Newtonsoft.Json;
using Dapper;
using Microsoft.Data.SqlClient;
using System.Data;


namespace NSIE.Controllers
{
    public class InscripcionController : Controller
    {
        private readonly IRepositorioInscripcion _repositorioInscripcion;
        private readonly string _connectionString;

        public InscripcionController(IRepositorioInscripcion repositorioInscripcion, IConfiguration configuration)
        {
            _repositorioInscripcion = repositorioInscripcion;
            _connectionString = configuration.GetConnectionString("DefaultConnection");
        }

        // Acción para cargar la vista de pre-registro como usuario invitado
        public IActionResult PreRegistroComoInvitado()
        {
            // Crear el usuario invitado "Consulta Pública"
            Usuario oUsuario = new Usuario
            {
                Correo = "invitado@cre.gob.mx",
                Clave = "consulta_publica"
            };

            // Registrar el acceso como "Consulta Pública"
            RegistrarAcceso(oUsuario.Correo, "Acceso como Consulta Pública a Inscripción S-CEL");

            // Simular el login como usuario invitado sin validar la contraseña
            return LoginConsultaPublica(oUsuario);
        }

        // Método para registrar el acceso en la tabla 'Accesos'
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

        // Método personalizado para login de "Consulta Pública"
        private IActionResult LoginConsultaPublica(Usuario oUsuario)
        {
            // Aquí no se aplica la lógica de contraseña porque es un usuario invitado
            // Obtener el ID de usuario invitado desde la base de datos
            using (SqlConnection cn = new SqlConnection(_connectionString))
            {
                cn.Open();

                // Obtener el IdUsuario de la tabla USUARIO
                int idUsuario = cn.QuerySingleOrDefault<int>(
                    "SELECT IdUsuario FROM USUARIO WHERE Correo = @Correo",
                    new { Correo = oUsuario.Correo }
                );

                // Obtener el perfil del usuario "Consulta Pública" utilizando el IdUsuario
                PerfilUsuario perfilUsuario = cn.QuerySingleOrDefault<PerfilUsuario>(
                    "sp_ObtenerUsuarioSession",
                    new { IdUsuario = idUsuario },  // Ahora pasamos IdUsuario
                    commandType: CommandType.StoredProcedure
                );

                if (perfilUsuario != null)
                {
                    // Almacenar el perfil del usuario en la sesión
                    var perfilUsuarioJson = JsonConvert.SerializeObject(perfilUsuario);
                    HttpContext.Session.SetString("PerfilUsuario", perfilUsuarioJson);

                    // Redirigir a la vista de pre-registro
                    return RedirectToAction("PreRegistro");
                }
            }

            // Si no se encuentra el usuario, regresar al login
            ViewData["Mensaje"] = "No se pudo acceder como invitado. Intente nuevamente.";
            return View("Login");
        }

        // Acción para cargar la vista de pre-registro
        public async Task<IActionResult> PreRegistro()
        {
            // Obtener las modalidades desde el repositorio
            var modalidades = await _repositorioInscripcion.ObtenerModalidadesAsync();

            // Verificar que las modalidades no sean null
            if (modalidades == null || modalidades.Count == 0)
            {
                ViewData["Mensaje"] = "No se encontraron modalidades disponibles.";
                return View();
            }

            // Pasar la lista de modalidades a la vista utilizando ViewData
            ViewData["Modalidades"] = modalidades;

            // Retornar la vista
            return View();
        }

        //Carga de Formularios
        // Acción para cargar el formulario basado en la categoría seleccionada


        [HttpGet]
        public IActionResult CargarFormulario(string modalidad, string categoria)
        {
            try
            {
                if (string.IsNullOrEmpty(categoria))
                {
                    return BadRequest("La categoría no puede estar vacía.");
                }
                // Mapear las categorías largas a abreviadas
                var categoriaAbreviada = categoria switch
                {
                    "Suministrador GLD" => "SGLD",
                    "Generador" => "GEN",
                    "Participante Obligado" => "PO",
                    "Entidad Voluntaria" => "EV",
                    _ => null
                };
                // Validar y mapear las categorías
                switch (categoria)
                {
                    case "GEN":
                        return PartialView("_FormularioGeneradorLimpio_A");
                    case "SGLD":
                        return PartialView("_FormularioSGLD_B");
                    case "PO":
                        return PartialView("_FormularioParticipanteObligado_C");
                    case "EV":
                        return PartialView("_FormularioEntidadVoluntaria_D");
                    default:
                        return BadRequest("Categoría no válida: " + categoria); // Manejar error si la categoría no es válida
                }
            }
            catch (Exception ex)
            {
                // Registrar el error y devolver un mensaje
                return StatusCode(500, "Error interno del servidor: " + ex.Message);
            }
        }




        [HttpPost]
        public IActionResult ProcesarFormulario()
        {
            // Aquí puedes procesar los datos del formulario si es necesario

            // Redirigir a la vista InscripcionExitosa
            return RedirectToAction("SolicitudExitosa");
        }

        public IActionResult SolicitudExitosa()
        {
            // Esta es la vista a la que redirigirá después de procesar el formulario
            return View();
        }
    }
}

