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
using NuGet.Protocol;
using System.Threading.Tasks;
using Newtonsoft.Json;
using Dapper;




namespace NSIE.Controllers
{
    [ServiceFilter(typeof(ValidacionInputFiltro))]
    [AutorizacionFiltro]
    public class BitacoraController : Controller
    {
        private readonly IRepositorioBitacora repositorioBitacora;

        public BitacoraController(IRepositorioBitacora repositorioBitacora)
        {
            this.repositorioBitacora = repositorioBitacora;
        }


        // Registra la Actividad de los Usuarios

        [HttpPost]
        public async Task<IActionResult> RegistrarActividad([FromBody] UserActivityModel model)
        {
            if (model == null)
            {
                Console.WriteLine("El modelo es nulo.");
                return BadRequest("El modelo no puede ser nulo.");
            }

            // Aquí se llama a un único método en el repositorio para registrar cualquier actividad
            await repositorioBitacora.RegistrarActividadAsync(
                model.UserId,
                model.UserName,
                model.ActionName,
                model.ControllerName,
                model.PageName,
                model.Tipo,
                model.Elemento,
                model.IdElemento,
                model.Valor,
                model.AdditionalData
            );

            return Ok(new { success = true });
        }


        public IActionResult Dashboard()
        {
            return View();
        }


        // Método para Obtener Usuarios Activos en Tiempo Real
        [HttpGet]
        public async Task<IActionResult> GetUsuariosActivos()
        {
            try
            {
                var timeThreshold = DateTime.Now.AddMinutes(-5); // Últimos 5 minutos
                var usuariosActivos = await repositorioBitacora.ObtenerUsuariosActivosAsync(timeThreshold);

                return PartialView("_UsuariosActivos", usuariosActivos);
            }
            catch (Exception ex)
            {
                Console.WriteLine($"Error al obtener usuarios activos: {ex.Message}");
                return StatusCode(500, "Error interno del servidor.");
            }
        }

        // Método para Obtener la Actividad por Periodo

        [HttpGet]
        public async Task<IActionResult> GetActividadPorPeriodo(string periodo)
        {
            DateTime startDate;
            DateTime endDate = DateTime.Now;

            switch (periodo.ToLower())
            {
                case "dia":
                    startDate = endDate.Date;
                    break;
                case "semana":
                    startDate = endDate.AddDays(-7);
                    break;
                case "mes":
                    startDate = endDate.AddMonths(-1);
                    break;
                default:
                    startDate = endDate.Date;
                    break;
            }

            var actividad = await repositorioBitacora.ObtenerActividadPorPeriodoAsync(startDate, endDate);

            // Preparar los datos para la gráfica
            var categorias = actividad.Select(a => a.UserName).Distinct().ToList();
            var series = actividad.GroupBy(a => a.UserName)
                                  .Select(g => g.Count())
                                  .ToList();

            // Preparar los datos para la tabla
            var actividadData = actividad.Select(a => new
            {
                a.UserName,
                a.PageName,
                a.ActionName,
                FechaHora = a.Timestamp.ToString("yyyy-MM-dd HH:mm:ss"), // Incluir fecha y hora completas
                a.Tipo,
                a.Elemento,
                a.IdElemento,
                a.Valor
            }).ToList();

            return Json(new
            {
                categorias,
                series,
                actividad = actividadData
            });
        }


        // Método para Generar y Enviar el Reporte Diario
        [HttpPost]
        public async Task<IActionResult> GenerarReporteDiario()
        {
            try
            {
                // Lógica para obtener la actividad del día
                var startDate = DateTime.Now.Date;
                var endDate = startDate.AddDays(1);

                var actividadDiaria = await repositorioBitacora.ObtenerActividadPorPeriodoAsync(startDate, endDate);

                // Llamada al repositorio para generar y enviar el reporte
                await repositorioBitacora.GenerarReporteDiarioAsync(actividadDiaria);

                return Json(new { success = true });
            }
            catch (Exception ex)
            {
                return Json(new { success = false, message = ex.Message });
            }
        }



        // Actividad por Usuario
        [HttpGet]
        public async Task<IActionResult> GetUsuarios()
        {
            var usuarios = await repositorioBitacora.ObtenerUsuariosAsync();
            return Json(usuarios.Select(u => new
            {
                userId = u.IdUsuario,
                userName = u.Nombre
            }).ToList());
        }

        [HttpGet]
        public async Task<IActionResult> GetActividadPorUsuario(int usuarioId, string periodo)
        {
            DateTime startDate;
            DateTime endDate = DateTime.Now;

            switch (periodo.ToLower())
            {
                case "dia":
                    startDate = endDate.Date;
                    break;
                case "semana":
                    startDate = endDate.AddDays(-7);
                    break;
                case "mes":
                    startDate = endDate.AddMonths(-1);
                    break;
                case "año":
                    startDate = endDate.AddYears(-1);
                    break;
                default:
                    startDate = endDate.Date;
                    break;
            }

            var actividad = await repositorioBitacora.ObtenerActividadPorUsuarioAsync(usuarioId, startDate, endDate);

            // Preparar los datos para la gráfica
            var categorias = actividad.Select(a => a.PageName).Distinct().ToList();
            var series = actividad.GroupBy(a => a.PageName)
                                  .Select(g => g.Count())
                                  .ToList();

            // Preparar los datos para la tabla
            var actividadData = actividad.Select(a => new
            {
                a.PageName,
                a.ActionName,
                FechaHora = a.Timestamp.ToString("yyyy-MM-dd HH:mm:ss"), // Incluir fecha y hora completas
                a.Tipo,
                a.Elemento,
                a.IdElemento,
                a.Valor
            }).ToList();

            var userName = actividad.FirstOrDefault()?.UserName ?? "Usuario Desconocido";

            return Json(new
            {
                userName,
                categorias,
                series,
                actividad = actividadData
            });
        }


        //Monitoreo
        [HttpGet]
        public async Task<IActionResult> ObtenerTotalAccesos()
        {
            var totalAccesos = await repositorioBitacora.GetTotalAccessCountAsync();
            return Json(new { TotalAccesos = totalAccesos });
        }

        [HttpGet]
        public async Task<IActionResult> ObtenerTotalesPorTipo()
        {
            var accesosPorTipo = await repositorioBitacora.GetTotalAccessCountByTypeAsync();
            return Json(accesosPorTipo);
        }


        [HttpGet]
        public async Task<IActionResult> ObtenerAccesosPorFecha()
        {
            var fechaInicio = new DateTime(2023, 1, 1);
            var fechaFin = DateTime.Now;

            var accesosPorFecha = await repositorioBitacora.GetAccesosPorFechaAsync(fechaInicio, fechaFin);

            var resultado = accesosPorFecha.Select(a => new
            {
                Fecha = a.Fecha.ToString("yyyy-MM-dd"), // Convertir la fecha a string en formato yyyy-MM-dd
                TotalAccesos = a.TotalAccesos
            }).ToList();

            return Json(resultado);
        }



        [HttpGet]
        public async Task<IActionResult> ObtenerAccesosPorUsuario()
        {
            var accesosPorUsuario = await repositorioBitacora.GetAccesosPorUsuarioAsync();
            return Json(accesosPorUsuario);
        }
        [HttpGet]
        public async Task<IActionResult> ObtenerAccesosPorCargo()
        {
            var accesosPorCargo = await repositorioBitacora.GetAccesosPorCargoAsync();
            return Json(accesosPorCargo);
        }
        [HttpGet]
        public async Task<IActionResult> ObtenerAccesosPorUnidad()
        {
            var accesosPorUnidad = await repositorioBitacora.GetAccesosPorUnidadAsync();
            return Json(accesosPorUnidad);
        }
        [HttpGet]
        public async Task<IActionResult> ObtenerAccesosHoyPorUsuario()
        {
            var accesosHoyPorUsuario = await repositorioBitacora.GetAccesosHoyPorUsuarioAsync();
            return Json(accesosHoyPorUsuario);
        }


        [HttpGet]
        public async Task<IActionResult> ObtenerUsuariosActivos()
        {
            var usuariosActivos = await repositorioBitacora.ObtenerUsuariosActivosAsync();

            // Formato de los datos a enviar
            var usuariosActivosDatos = usuariosActivos.Select(u => new
            {
                nombre = u.Nombre,
                cargo = u.Cargo,
                area = u.Area,
                horaConexion = u.UltimaActividad.AddHours(-7).ToString("yyyy-MM-dd HH:mm:ss"), // Ajuste a la hora de CDMX
                estado = u.EsActivo ? "activo" : "inactivo"
            });

            return Json(usuariosActivosDatos);
        }



        // ----------  Últimas acciones del usuario (TOP n) ----------
        [HttpGet]
        public async Task<IActionResult> GetUltimasAcciones(int top = 5)
        {
            // 1. Perfil desde sesión
            var perfilJson = HttpContext.Session.GetString("PerfilUsuario");
            if (string.IsNullOrWhiteSpace(perfilJson))
                return Unauthorized();               // sesión expirada

            var perfil = JsonConvert.DeserializeObject<PerfilUsuario>(perfilJson);

            // 2. Convertir IdUsuario a int de forma segura
            var idUsuario = int.TryParse(perfil.IdUsuario, out var tmp) ? tmp : 0;
            if (idUsuario == 0) return BadRequest("IdUsuario inválido");

            // 3. Consultar repositorio usando el entero
            var acciones = await repositorioBitacora
                             .ObtenerUltimasAccionesAsync(idUsuario, top);

            // 4. Reducir payload para la tarjeta
            var result = acciones.Select(a => new
            {
                fecha = a.Timestamp.ToString("dd/MM/yyyy HH:mm"),
                pagina = a.PageName,
                controller = a.ControllerName,
                action = a.ActionName
            });


            return Json(result);
        }



    }


}