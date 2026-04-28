using Dapper;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Data.SqlClient;
using Newtonsoft.Json;
using NSIE.Models;
using NSIE.Servicios;
using System.Collections.Generic;
using System.IO;
using Microsoft.AspNetCore.Http;
using SendGrid.Helpers.Mail;


namespace NSIE.Controllers
{
    [ServiceFilter(typeof(ValidacionInputFiltro))]
    [AutorizacionFiltro]
    public class IndicadoresController : Controller
    {
        private readonly IRepositorioIndicadores repositorioIndicadores;

        public IndicadoresController(IRepositorioIndicadores repositorioIndicadores)
        {

            this.repositorioIndicadores = repositorioIndicadores;
        }

        //Devuelve la vista del Mecanismo desde el Menu
        public IActionResult Menu_Mecanismo()
        {
            var perfilUsuarioJson = HttpContext.Session.GetString("PerfilUsuario");
            var perfilUsuario = JsonConvert.DeserializeObject<PerfilUsuario>(perfilUsuarioJson);
            var tarjetas = new List<MenuMecanismoTarjetas>
                {
                    new MenuMecanismoTarjetas { MercadoId = "1", ImagenSrc = "6.png", Titulo = "Petrolíferos", Controlador = "Indicadores", Accion = "Mecanismo", EsDisabled = false,
                     ShouldDisplay = ((perfilUsuario.Rol == "1" && perfilUsuario.Mercado_ID == "0") ||
                                     (perfilUsuario.Rol == "7" && perfilUsuario.Mercado_ID == "0")||
                                     (perfilUsuario.Rol == "3" && perfilUsuario.Mercado_ID != "4")||
                                     (perfilUsuario.Rol == "5" && perfilUsuario.Mercado_ID == "1")||
                                     (perfilUsuario.Rol == "4" && perfilUsuario.Mercado_ID == "1")) &&
                                     int.Parse(perfilUsuario.IdUsuario) != 53
                    },
                    new MenuMecanismoTarjetas { MercadoId = "2", ImagenSrc = "5.png", Titulo = "Gas L.P.", Controlador = "Indicadores", Accion = "Mecanismo_GLP", EsDisabled = false,
                     ShouldDisplay = ((perfilUsuario.Rol == "1" && perfilUsuario.Mercado_ID == "0") ||
                                     (perfilUsuario.Rol == "7" && perfilUsuario.Mercado_ID == "0")||
                                     (perfilUsuario.Rol == "3" && perfilUsuario.Mercado_ID != "4")||
                                     (perfilUsuario.Rol == "5" && perfilUsuario.Mercado_ID == "2")||
                                     (perfilUsuario.Rol == "4" && perfilUsuario.Mercado_ID == "2")) &&
                                     int.Parse(perfilUsuario.IdUsuario) != 53
                    },
                    new MenuMecanismoTarjetas {
                        MercadoId = "4",
                        ImagenSrc = "14.jpg",
                        Titulo = "Electricidad",
                        Controlador = "Indicadores",
                        Accion = "Mecanismo_Electricidad",
                        EsDisabled = false,
                        ShouldDisplay = ((perfilUsuario.Rol == "1" && perfilUsuario.Mercado_ID == "0") ||
                                        (perfilUsuario.Rol == "3" && perfilUsuario.Mercado_ID == "4")||
                                        (perfilUsuario.Rol == "7" && perfilUsuario.Mercado_ID == "0")||
                                     (perfilUsuario.Rol == "5" && perfilUsuario.Mercado_ID == "4")||
                                     (perfilUsuario.Rol == "4" && perfilUsuario.Mercado_ID == "4")) &&
                                     int.Parse(perfilUsuario.IdUsuario) != 53

                    },
                    new MenuMecanismoTarjetas { MercadoId = "3", ImagenSrc = "12.png", Titulo = "Gas Natural", Controlador = "Indicadores", Accion = "Menu_Mecanismo", EsDisabled = true,
                     ShouldDisplay = ((perfilUsuario.Rol == "1" && perfilUsuario.Mercado_ID == "0") ||
                                     (perfilUsuario.Rol == "3" && perfilUsuario.Mercado_ID != "4")||
                                     (perfilUsuario.Rol == "7" && perfilUsuario.Mercado_ID == "0")||
                                     (perfilUsuario.Rol == "5" && perfilUsuario.Mercado_ID == "3")||
                                     (perfilUsuario.Rol == "4" && perfilUsuario.Mercado_ID == "3")) &&
                                     int.Parse(perfilUsuario.IdUsuario) != 53
                    },

                     new MenuMecanismoTarjetas
                    {
                        MercadoId = "0",
                        ImagenSrc = "14.jpg",
                        Titulo = "Solicitudes Evaluadas Petrolíferos y Gas L.P.",
                        Controlador = "Indicadores",
                        Accion = "Mecanismo_Especial",
                        EsDisabled = false,
                        ShouldDisplay = (perfilUsuario.Rol == "1") || // Administrador siempre puede verla
                                        (perfilUsuario.Rol == "5" && perfilUsuario.Mercado_ID == "0" && int.Parse(perfilUsuario.IdUsuario) == 53) // Director general con Id 53
                    }

                };


            ViewBag.Tarjetas = tarjetas;
            return View();



            // return View();
        }

        #region #1.-Vista de Mecanismos de los Diferentes Mercados Energéticos
        //#1
        //A)Vista del Mecanismo para Petrolíferos
        public async Task<IActionResult> Mecanismo()
        {
            var todoslosIndicadores = await repositorioIndicadores.Obtener_Indicadores();

            // var expendios = await repositorioIndicadores.ObtenerExpendios();

            //Como Json
            //var sp = await repositorioIndicadores.ObtenerSolicitudes_MapaPetroliferos();
            //  var sp_total = await repositorioIndicadores.ObtenerTotalSolicitudes_EvaluarPetroliferos();


            // Crear el modelo de vista y asignar las listas
            Mep modelo = new Mep()
            {
                Indicadores = todoslosIndicadores,

                //  ObtenerExpendios=expendios,
                // ObtenerSolicitudes_MapaPetroliferos = sp,
                // ObtenerTotalSolicitudes_EvaluarPetroliferos = sp_total,
            };
            // Pasando Datos al como Json al ViewBag
            // string expendiosJson = JsonConvert.SerializeObject(expendios);
            // string coloresImJson=JsonConvert.SerializeObject(todoslosIndicadores);
            // string spJson = JsonConvert.SerializeObject(sp);

            // Crear el diccionario de objetos JSON
            //Dictionary<string, object> jsonObjects = new Dictionary<string, object>
            //  {
            // { "ExpendiosJson", expendiosJson },
            //    { "ObtenerMapaSolicitudesPetroliferosJson", spJson },
            //       { "ColoresImun", coloresImJson}
            //  };

            // Asignar el diccionario al ViewBag
            //ViewBag.JsonObjects = jsonObjects;

            return View(modelo);

        }

        //Mecanismo Especial

        public async Task<IActionResult> Mecanismo_Especial()
        {
            var solicitudesD = await repositorioIndicadores.Obtener_SolicitudesD();
            return View(solicitudesD);
        }

        [HttpPost]
        public async Task<IActionResult> CalificarSolicitud(int id, decimal calificacion, string comentario, string nombreEvaluador)
        {
            var fechaYHoraEvaluacion = DateTime.Now; // Captura la fecha y hora actual

            await repositorioIndicadores.ActualizarCalificacion(id, calificacion, comentario, nombreEvaluador, fechaYHoraEvaluacion);
            return RedirectToAction("Mecanismo_Especial");
        }



        //B)Vista del Mecanismo de Gas L.P.
        public async Task<IActionResult> Mecanismo_GLP()
        {
            var todoslosIndicadores = await repositorioIndicadores.Obtener_Indicadores_GLP();
            Mep modelo = new Mep()
            {
                Indicadores = todoslosIndicadores,
            };
            return View(modelo);
        }


        //C)Vista del Mecanismo de Electricidad
        public async Task<IActionResult> Mecanismo_Electricidad()
        {
            var todoslosIndicadores = await repositorioIndicadores.Obtener_Indicadores_Electricidad();
            Mep modelo = new Mep()
            {
                Indicadores = todoslosIndicadores,
            };
            return View(modelo);
        }

        #endregion

        #region #2.-Totales de Solicitudes a Evaluar

        //#2
        //Obtiene los totales de las Solicitudes a Evaluar
        //A)Petrolíferos
        public async Task<IActionResult> GetTotales_a_Evaluar()
        {
            try
            {
                var sp_total = await repositorioIndicadores.ObtenerTotalSolicitudes_EvaluarPetroliferos();

                if (sp_total == null)
                {
                    return NotFound(); // Manejar el caso en que regrese null
                }
                return Json(sp_total);
            }
            catch (Exception e)
            {
                Console.WriteLine(e.Message);
                Console.WriteLine(e.StackTrace);
                return StatusCode(500, "Error en el servidor");
            }

        }
        //B)Gas L.P.
        public async Task<IActionResult> GetTotales_a_Evaluar_GLP()
        {
            try
            {
                var sp_total = await repositorioIndicadores.ObtenerTotalSolicitudes_Evaluar_GLP();

                if (sp_total == null)
                {
                    return NotFound(); // Manejar el caso en que regrese null
                }
                return Json(sp_total);
            }
            catch (Exception e)
            {
                Console.WriteLine(e.Message);
                Console.WriteLine(e.StackTrace);
                return StatusCode(500, "Error en el servidor");
            }

        }
        //C)Electricidad
        public async Task<IActionResult> GetTotales_a_Evaluar_Electricidad()
        {
            try
            {
                var sp_total = await repositorioIndicadores.ObtenerTotalSolicitudes_Evaluar_Electricidad();

                if (sp_total == null)
                {
                    return NotFound(); // Manejar el caso en que regrese null
                }
                return Json(sp_total);
            }
            catch (Exception e)
            {
                Console.WriteLine(e.Message);
                Console.WriteLine(e.StackTrace);
                return StatusCode(500, "Error en el servidor");
            }

        }

        #endregion

        //#3
        //Obtiene las Solicitudes a evaluar
        //A)Petrolíferos
        public async Task<IActionResult> GetSolicitudes()
        {

            var sp = await repositorioIndicadores.ObtenerSolicitudes_MapaPetroliferos();


            if (sp == null)
            {
                return NotFound(); // Manejar el caso en queregrese null
            }
            return Json(sp);


        }
        //B)Gas L.P.
        public async Task<IActionResult> GetSolicitudes_GLP()
        {

            var sp = await repositorioIndicadores.ObtenerSolicitudes_Mapa_GLP();


            if (sp == null)
            {
                return NotFound(); // Manejar el caso en queregrese null
            }
            return Json(sp);


        }
        //C)Electricidad
        public async Task<IActionResult> GetSolicitudes_Electricidad()
        {

            var sp = await repositorioIndicadores.ObtenerSolicitudes_Mapa_Electricidad();


            if (sp == null)
            {
                return NotFound(); // Manejar el caso en queregrese null
            }
            return Json(sp);


        }


        //#4
        //Obtiene los expendios
        //A)Petrolíferos
        [HttpGet]
        public async Task<IActionResult> GetCamposVisibles()
        {
            var perfilUsuarioJson = HttpContext.Session.GetString("PerfilUsuario");
            var perfilUsuario = JsonConvert.DeserializeObject<PerfilUsuario>(perfilUsuarioJson);

            //pasando de String a Int
            int rolId = int.Parse(perfilUsuario.Rol);
            int mercadoId = int.Parse(perfilUsuario.Mercado_ID);


            var camposVisibles = await repositorioIndicadores.ObtenerCamposVisibles(rolId, mercadoId);

            return Json(camposVisibles);
        }
        public async Task<IActionResult> GetExpendiosAutorizados()
        {
            var ExpendiosJson2 = await repositorioIndicadores.ObtenerExpendiosAutorizados();
            if (ExpendiosJson2 == null)
            {
                return NotFound(); // Manejar el caso en queregrese null
            }
            return Json(ExpendiosJson2);
        }
        //Para el Mapa de Infraestructura de Petróliferos
        [HttpGet]
        public async Task<IActionResult> GetCamposVisiblesPET_Infra()
        {
            var perfilUsuarioJson = HttpContext.Session.GetString("PerfilUsuario");
            var perfilUsuario = JsonConvert.DeserializeObject<PerfilUsuario>(perfilUsuarioJson);

            //pasando de String a Int
            int rolId = int.Parse(perfilUsuario.Rol);
            int mercadoId = int.Parse(perfilUsuario.Mercado_ID);


            var camposVisibles = await repositorioIndicadores.ObtenerCamposVisiblesPET_Infra(rolId, mercadoId);

            return Json(camposVisibles);
        }
        public async Task<IActionResult> GetExpendiosAutorizadosPET_Infra()
        {
            var ExpendiosJson2 = await repositorioIndicadores.ObtenerExpendiosAutorizadosPET_Infra();
            if (ExpendiosJson2 == null)
            {
                return NotFound(); // Manejar el caso en queregrese null
            }
            return Json(ExpendiosJson2);
        }

        [HttpPost]
        public async Task<IActionResult> GetExpendiosAutorizadosPET_Infra_SP([FromBody] ExpendioAutorizado expendioAutorizado)
        {
            try
            {
                var calificacion = await repositorioIndicadores.ObtenerExpendiosAutorizadosPET_Infra_SP(expendioAutorizado);

                if (calificacion == null)
                {
                    return NotFound(); // Manejar el caso en que no se encuentre el expendio
                }

                return Json(calificacion);
            }
            catch (Exception ex)
            {
                // Loguear la excepción si es necesario
                return StatusCode(500, new { message = ex.Message });
            }
        }


        //B)Gas L.P.
        [HttpGet]
        public async Task<IActionResult> GetCamposVisiblesGLP()
        {
            var perfilUsuarioJson = HttpContext.Session.GetString("PerfilUsuario");
            var perfilUsuario = JsonConvert.DeserializeObject<PerfilUsuario>(perfilUsuarioJson);

            //pasando de String a Int
            int rolId = int.Parse(perfilUsuario.Rol);
            int mercadoId = int.Parse(perfilUsuario.Mercado_ID);


            var camposVisibles = await repositorioIndicadores.ObtenerCamposVisiblesGLP(rolId, mercadoId);

            return Json(camposVisibles);
        }
        public async Task<IActionResult> GetExpendiosAutorizadosGLP()
        {
            var ExpendiosJson2 = await repositorioIndicadores.ObtenerExpendiosAutorizadosGLP();
            if (ExpendiosJson2 == null)
            {
                return NotFound(); // Manejar el caso en queregrese null
            }
            return Json(ExpendiosJson2);
        }
        //Para el Mapa de Infraestructura de GLP
        [HttpGet]
        public async Task<IActionResult> GetCamposVisiblesGLP_Infra()
        {
            var perfilUsuarioJson = HttpContext.Session.GetString("PerfilUsuario");
            var perfilUsuario = JsonConvert.DeserializeObject<PerfilUsuario>(perfilUsuarioJson);

            //pasando de String a Int
            int rolId = int.Parse(perfilUsuario.Rol);
            int mercadoId = int.Parse(perfilUsuario.Mercado_ID);


            var camposVisibles = await repositorioIndicadores.ObtenerCamposVisiblesGLP_Infra(rolId, mercadoId);

            return Json(camposVisibles);
        }
        public async Task<IActionResult> GetExpendiosAutorizadosGLP_Infra()
        {
            var ExpendiosJson2 = await repositorioIndicadores.ObtenerExpendiosAutorizadosGLP_Infra();
            if (ExpendiosJson2 == null)
            {
                return NotFound(); // Manejar el caso en queregrese null
            }
            return Json(ExpendiosJson2);
        }


        //C)Gas Natural
        [HttpGet]
        public async Task<IActionResult> GetCamposVisiblesGN_Infra()
        {
            var perfilUsuarioJson = HttpContext.Session.GetString("PerfilUsuario");
            var perfilUsuario = JsonConvert.DeserializeObject<PerfilUsuario>(perfilUsuarioJson);

            //pasando de String a Int
            int rolId = int.Parse(perfilUsuario.Rol);
            int mercadoId = int.Parse(perfilUsuario.Mercado_ID);


            var camposVisibles = await repositorioIndicadores.ObtenerCamposVisiblesGN_Infra(rolId, mercadoId);

            return Json(camposVisibles);
        }
        public async Task<IActionResult> GetExpendiosAutorizadosGN_Infra()
        {
            var ExpendiosJson2 = await repositorioIndicadores.ObtenerExpendiosAutorizadosGN_Infra();
            if (ExpendiosJson2 == null)
            {
                return NotFound(); // Manejar el caso en queregrese null
            }
            return Json(ExpendiosJson2);
        }


        //D)Electricidad
        [HttpGet]
        public async Task<IActionResult> GetCamposVisiblesElectricidad()
        {
            var perfilUsuarioJson = HttpContext.Session.GetString("PerfilUsuario");
            var perfilUsuario = JsonConvert.DeserializeObject<PerfilUsuario>(perfilUsuarioJson);

            //pasando de String a Int
            int rolId = int.Parse(perfilUsuario.Rol);
            int mercadoId = int.Parse(perfilUsuario.Mercado_ID);


            var camposVisibles = await repositorioIndicadores.ObtenerCamposVisiblesGLP(rolId, mercadoId);

            return Json(camposVisibles);
        }
        public async Task<IActionResult> GetExpendiosAutorizadosElectricidad()
        {
            var ExpendiosJson2 = await repositorioIndicadores.ObtenerExpendiosAutorizadosElectricidad();
            if (ExpendiosJson2 == null)
            {
                return NotFound(); // Manejar el caso en queregrese null
            }
            return Json(ExpendiosJson2);
        }
        //
        public async Task<IActionResult> GetCamposVisiblesElectricidad_Infra()
        {
            var perfilUsuarioJson = HttpContext.Session.GetString("PerfilUsuario");
            var perfilUsuario = JsonConvert.DeserializeObject<PerfilUsuario>(perfilUsuarioJson);

            //pasando de String a Int
            int rolId = int.Parse(perfilUsuario.Rol);
            int mercadoId = int.Parse(perfilUsuario.Mercado_ID);


            var camposVisibles = await repositorioIndicadores.ObtenerCamposVisiblesElectricidad_Infra(rolId, mercadoId);

            return Json(camposVisibles);
        }


        //Refinerías, Despuntadoras y Complejos de Gas
        public async Task<IActionResult> GetCamposVisiblesCG_Infra()
        {
            var perfilUsuarioJson = HttpContext.Session.GetString("PerfilUsuario");
            var perfilUsuario = JsonConvert.DeserializeObject<PerfilUsuario>(perfilUsuarioJson);

            //pasando de String a Int
            int rolId = int.Parse(perfilUsuario.Rol);
            int mercadoId = int.Parse(perfilUsuario.Mercado_ID);


            var camposVisibles = await repositorioIndicadores.ObtenerCamposVisiblesCG_Infra(rolId, mercadoId);

            return Json(camposVisibles);
        }
        public async Task<IActionResult> GetCamposVisiblesRyS_Infra()
        {
            var perfilUsuarioJson = HttpContext.Session.GetString("PerfilUsuario");
            var perfilUsuario = JsonConvert.DeserializeObject<PerfilUsuario>(perfilUsuarioJson);

            //pasando de String a Int
            int rolId = int.Parse(perfilUsuario.Rol);
            int mercadoId = int.Parse(perfilUsuario.Mercado_ID);


            var camposVisibles = await repositorioIndicadores.ObtenerCamposVisiblesRyS_Infra(rolId, mercadoId);

            return Json(camposVisibles);
        }
        public async Task<IActionResult> GetCamposVisiblesPI_Infra()
        {
            var perfilUsuarioJson = HttpContext.Session.GetString("PerfilUsuario");
            var perfilUsuario = JsonConvert.DeserializeObject<PerfilUsuario>(perfilUsuarioJson);

            //pasando de String a Int
            int rolId = int.Parse(perfilUsuario.Rol);
            int mercadoId = int.Parse(perfilUsuario.Mercado_ID);


            var camposVisibles = await repositorioIndicadores.ObtenerCamposVisiblesPI_Infra(rolId, mercadoId);

            return Json(camposVisibles);
        }
        public async Task<IActionResult> GetExpendiosAutorizadosRyS_Infra()
        {
            var ExpendiosJson2 = await repositorioIndicadores.ObtenerExpendiosAutorizadosRyS_Infra();
            if (ExpendiosJson2 == null)
            {
                return NotFound(); // Manejar el caso en queregrese null
            }
            return Json(ExpendiosJson2);
        }
        public async Task<IActionResult> GetExpendiosAutorizadosCG_Infra()
        {
            var ExpendiosJson2 = await repositorioIndicadores.ObtenerExpendiosAutorizadosCG_Infra();
            if (ExpendiosJson2 == null)
            {
                return NotFound(); // Manejar el caso en queregrese null
            }
            return Json(ExpendiosJson2);
        }
        public async Task<IActionResult> GetExpendiosAutorizadosPI_Infra()
        {
            var ExpendiosJson2 = await repositorioIndicadores.ObtenerExpendiosAutorizadosPI_Infra();
            if (ExpendiosJson2 == null)
            {
                return NotFound(); // Manejar el caso en queregrese null
            }
            return Json(ExpendiosJson2);
        }
        public async Task<IActionResult> GetExpendiosAutorizadosElectricidad_Infra()
        {
            var ExpendiosJson2 = await repositorioIndicadores.ObtenerExpendiosAutorizadosElectricidad_Infra();
            if (ExpendiosJson2 == null)
            {
                return NotFound(); // Manejar el caso en queregrese null
            }
            return Json(ExpendiosJson2);
        }


        //#5
        //Boton Ejecutar EF
        //A)Petrolíferos
        //Totales Tarjetas
        [HttpPost]
        public async Task<IActionResult> calificacion_Nacional([FromBody] CalificacionFinal calificacionFinal)
        {
            var calificacion = await repositorioIndicadores.A_Calificacion_Nacional(calificacionFinal);

            if (calificacion == null)
            {
                return NotFound(); // Manejar el caso en que no se encuentre el expendio
            }

            return Json(calificacion);
        }
        //Totales Categorias
        [HttpPost]
        public async Task<IActionResult> calificacion_Total([FromBody] CalificacionFinal calificacionFinal)
        {
            var calificacion = await repositorioIndicadores.B_Calificacion_Total(calificacionFinal);

            if (calificacion == null)
            {
                return NotFound(); // Manejar el caso en que no se encuentre el expendio
            }

            return Json(calificacion);
        }
        //Grafico1 y Totales a nivel EF
        [HttpPost]
        public async Task<IActionResult> calificacion_EF([FromBody] CalificacionFinal calificacionFinal)
        {
            var calificacion = await repositorioIndicadores.D_Calificacion_por_EF(calificacionFinal);

            if (calificacion == null)
            {
                return NotFound(); // Manejar el caso en que no se encuentre el expendio
            }

            return Json(calificacion);
        }



        //B)Gas L.P.
        //Totales Tarjetas
        [HttpPost]
        public async Task<IActionResult> calificacion_Nacional_GLP([FromBody] CalificacionFinal calificacionFinal)
        {
            var calificacion = await repositorioIndicadores.A_Calificacion_Nacional_GLP(calificacionFinal);

            if (calificacion == null)
            {
                return NotFound(); // Manejar el caso en que no se encuentre el expendio
            }

            return Json(calificacion);
        }
        //Totales Categorias
        [HttpPost]
        public async Task<IActionResult> calificacion_Total_GLP([FromBody] CalificacionFinal calificacionFinal)
        {
            var calificacion = await repositorioIndicadores.B_Calificacion_Total_GLP(calificacionFinal);

            if (calificacion == null)
            {
                return NotFound(); // Manejar el caso en que no se encuentre el expendio
            }

            return Json(calificacion);
        }
        //Grafico1 y Totales a nivel EF
        [HttpPost]
        public async Task<IActionResult> calificacion_EF_GLP([FromBody] CalificacionFinal calificacionFinal)
        {
            var calificacion = await repositorioIndicadores.D_Calificacion_por_EF_GLP(calificacionFinal);

            if (calificacion == null)
            {
                return NotFound(); // Manejar el caso en que no se encuentre el expendio
            }

            return Json(calificacion);
        }



        //C)Electricidad
        //Totales Tarjetas
        [HttpPost]
        public async Task<IActionResult> calificacion_Nacional_Electricidad([FromBody] CalificacionFinal calificacionFinal)
        {
            var calificacion = await repositorioIndicadores.A_Calificacion_Nacional_Electricidad(calificacionFinal);

            if (calificacion == null)
            {
                return NotFound(); // Manejar el caso en que no se encuentre el expendio
            }

            return Json(calificacion);
        }
        //Totales Categorias
        [HttpPost]
        public async Task<IActionResult> calificacion_Total_Electricidad([FromBody] CalificacionFinal calificacionFinal)
        {
            var calificacion = await repositorioIndicadores.B_Calificacion_Total_Electricidad(calificacionFinal);

            if (calificacion == null)
            {
                return NotFound(); // Manejar el caso en que no se encuentre el expendio
            }

            return Json(calificacion);
        }
        //Grafico1 y Totales a nivel EF
        [HttpPost]
        public async Task<IActionResult> calificacion_EF_Electricidad([FromBody] CalificacionFinal calificacionFinal)
        {
            var calificacion = await repositorioIndicadores.D_Calificacion_por_EF_Electricidad(calificacionFinal);

            if (calificacion == null)
            {
                return NotFound(); // Manejar el caso en que no se encuentre el expendio
            }

            return Json(calificacion);
        }



        //#6 
        //Boton Ejecutar a nivel Municipal
        //A)Petrolíferos
        [HttpPost]
        public async Task<IActionResult> calificacion_MUN([FromBody] ResultadoMunicipios resultadoMunicipio)
        {

            var calificacionMUN = await repositorioIndicadores.J_Salida_EFMUN_Aprobados_NAL(resultadoMunicipio);

            if (calificacionMUN == null)
            {
                return NotFound(); // Manejar el caso en queregrese null
            }

            return Json(calificacionMUN);

        }
        //B)GLP
        [HttpPost]
        public async Task<IActionResult> calificacion_MUN_GLP([FromBody] ResultadoMunicipios resultadoMunicipio)
        {

            var calificacionMUN = await repositorioIndicadores.J_Salida_EFMUN_Aprobados_NAL_GLP(resultadoMunicipio);

            if (calificacionMUN == null)
            {
                return NotFound(); // Manejar el caso en queregrese null
            }

            return Json(calificacionMUN);

        }
        //C)Electricidad
        [HttpPost]
        public async Task<IActionResult> calificacion_MUN_Electricidad([FromBody] ResultadoMunicipios resultadoMunicipio)
        {

            var calificacionMUN = await repositorioIndicadores.J_Salida_EFMUN_Aprobados_NAL_Electricidad(resultadoMunicipio);

            if (calificacionMUN == null)
            {
                return NotFound(); // Manejar el caso en queregrese null
            }

            return Json(calificacionMUN);

        }




        //#7 Colores Municipales
        //A)Petrolíferos
        public async Task<IActionResult> Colores()
        {
            try
            {
                var coloresIndicadores = await repositorioIndicadores.Obtener_Indicadores_Colores();
                if (coloresIndicadores == null || !coloresIndicadores.Any())
                {
                    return NotFound(); // Manejar el caso en que regrese null
                }
                return Json(coloresIndicadores);
            }
            catch (Exception e)
            {
                Console.WriteLine(e.Message);
                Console.WriteLine(e.StackTrace);
                return StatusCode(500, "Error en el servidor");
            }
        }
        //B)Gas L.P.
        public async Task<IActionResult> Colores_GLP()
        {
            try
            {
                var coloresIndicadores = await repositorioIndicadores.Obtener_Indicadores_Colores_GLP();
                if (coloresIndicadores == null || !coloresIndicadores.Any())
                {
                    return NotFound(); // Manejar el caso en que regrese null
                }
                return Json(coloresIndicadores);
            }
            catch (Exception e)
            {
                Console.WriteLine(e.Message);
                Console.WriteLine(e.StackTrace);
                return StatusCode(500, "Error en el servidor");
            }
        }
        //C)Electricidad
        public async Task<IActionResult> Colores_Electricidad()
        {
            try
            {
                var coloresIndicadores = await repositorioIndicadores.Obtener_Indicadores_Colores_Electricidad();
                if (coloresIndicadores == null || !coloresIndicadores.Any())
                {
                    return NotFound(); // Manejar el caso en que regrese null
                }
                return Json(coloresIndicadores);
            }
            catch (Exception e)
            {
                Console.WriteLine(e.Message);
                Console.WriteLine(e.StackTrace);
                return StatusCode(500, "Error en el servidor");
            }
        }






        //#8 Obtiene el Detalle por EF desepues de la primera evaluación
        //A)Petrolíferos
        public async Task<IActionResult> Detalle_EF([FromQuery] CalificacionFinal calificacionFinal)
        {

            var calificacion = await repositorioIndicadores.D_Detalle_Calificacion_por_EF(calificacionFinal);

            if (calificacion == null)
            {
                return NotFound(); // Manejar el caso en que no se encuentre el expendio
            }
            return View(calificacion);
            //return Json(calificacion);

        }
        //B)GAS L.P.
        public async Task<IActionResult> Detalle_EF_GLP([FromQuery] CalificacionFinal calificacionFinal)
        {

            var calificacion = await repositorioIndicadores.D_Detalle_Calificacion_por_EF_GLP(calificacionFinal);

            if (calificacion == null)
            {
                return NotFound(); // Manejar el caso en que no se encuentre el expendio
            }
            return View(calificacion);
            //return Json(calificacion);

        }
        //C)Electricidad
        public async Task<IActionResult> Detalle_EF_Electricidad([FromQuery] CalificacionFinal calificacionFinal)
        {

            var calificacion = await repositorioIndicadores.D_Detalle_Calificacion_por_EF_Electricidad(calificacionFinal);

            if (calificacion == null)
            {
                return NotFound(); // Manejar el caso en que no se encuentre el expendio
            }
            return View(calificacion);
            //return Json(calificacion);

        }




        //#9 Obtiene Los grupos de Interés por Raz+on Social para el grafico de burbujas
        //A)Petrolíferos
        //#2
        //Obtiene los totales de las Solicitudes a Evaluar
        //A)Petrolíferos
        public async Task<IActionResult> GetGIE_Petroliferos()
        {
            try
            {
                var sp_gie = await repositorioIndicadores.ObtenerGIE_Petroliferos();

                if (sp_gie == null)
                {
                    return NotFound(); // Manejar el caso en que regrese null
                }
                return Json(sp_gie);
            }
            catch (Exception e)
            {
                Console.WriteLine(e.Message);
                Console.WriteLine(e.StackTrace);
                return StatusCode(500, "Error en el servidor");
            }

        }
        public async Task<IActionResult> GetGIE_Petroliferos_A()
        {
            try
            {
                var sp_gie = await repositorioIndicadores.ObtenerGIE_Petroliferos_A();

                if (sp_gie == null)
                {
                    return NotFound(); // Manejar el caso en que regrese null
                }
                return Json(sp_gie);
            }
            catch (Exception e)
            {
                Console.WriteLine(e.Message);
                Console.WriteLine(e.StackTrace);
                return StatusCode(500, "Error en el servidor");
            }

        }
        public async Task<IActionResult> GetGIE_Petroliferos_S()
        {
            try
            {
                var sp_gie = await repositorioIndicadores.ObtenerGIE_Petroliferos_S();

                if (sp_gie == null)
                {
                    return NotFound(); // Manejar el caso en que regrese null
                }
                return Json(sp_gie);
            }
            catch (Exception e)
            {
                Console.WriteLine(e.Message);
                Console.WriteLine(e.StackTrace);
                return StatusCode(500, "Error en el servidor");
            }

        }
        //a)Detalle GIE 
        public async Task<IActionResult> DetalleGIE(string razonSocial)
        {
            var (permisos, solicitudes) = await repositorioIndicadores.ObtenerGIEPorRazonSocial(razonSocial);

            // Si ambas listas están vacías, retorna NotFound
            if ((permisos == null || !permisos.Any()) && (solicitudes == null || !solicitudes.Any()))
            {
                return NotFound();
            }

            var viewModel = new GIEViewModel
            {
                Permisos = permisos ?? new List<Consulta_GIExRazonSocial>(),  // Si permisos es null, inicializa con una lista vacía
                Solicitudes = solicitudes ?? new List<Consulta_GIExRazonSocial>() // Si solicitudes es null, inicializa con una lista vacía
            };

            return View(viewModel);
        }

        //B)GLP
        //a)Detalle GIE 
        public async Task<IActionResult> DetalleGIEGLP(string razonSocial)
        {
            var (permisos, solicitudes) = await repositorioIndicadores.ObtenerGIEPorRazonSocialGLP(razonSocial);

            // Si ambas listas están vacías, retorna NotFound
            if ((permisos == null || !permisos.Any()) && (solicitudes == null || !solicitudes.Any()))
            {
                return NotFound();
            }

            var viewModel = new GIEViewModel
            {
                PermisosGLP = permisos ?? new List<ExpendioAutorizadoGLP>(),  // Si permisos es null, inicializa con una lista vacía
                Solicitudes = solicitudes ?? new List<Consulta_GIExRazonSocial>() // Si solicitudes es null, inicializa con una lista vacía
            };

            return View(viewModel);
        }
        public async Task<IActionResult> GetGIE_GLP_A()
        {
            try
            {
                var sp_gie = await repositorioIndicadores.ObtenerGIE_GLP_A();

                if (sp_gie == null)
                {
                    return NotFound(); // Manejar el caso en que regrese null
                }
                return Json(sp_gie);
            }
            catch (Exception e)
            {
                Console.WriteLine(e.Message);
                Console.WriteLine(e.StackTrace);
                return StatusCode(500, "Error en el servidor");
            }

        }
        public async Task<IActionResult> GetGIE_GLP_S()
        {
            try
            {
                var sp_gie = await repositorioIndicadores.ObtenerGIE_GLP_S();

                if (sp_gie == null)
                {
                    return NotFound(); // Manejar el caso en que regrese null
                }
                return Json(sp_gie);
            }
            catch (Exception e)
            {
                Console.WriteLine(e.Message);
                Console.WriteLine(e.StackTrace);
                return StatusCode(500, "Error en el servidor");
            }

        }




        //#10 Vista del Detalle de la Solicitud Deleccionada 3km
        //A)Petrolíferos
        public IActionResult DetalleSolicitud(int Id)
        {
            ViewBag.SolicitudId = Id; // Pasamos el ID a la vista para usarlo luego
            return View();
        }

        [HttpGet]
        public async Task<IActionResult> GetDetalleSolicitud(int Id)
        {
            var solicitud = await repositorioIndicadores.ObtenerSolicitudPorId(Id);
            if (solicitud == null)
            {
                return NotFound();
            }
            return Json(solicitud);
        }
        //GLP
        public IActionResult DetalleSolicitudGLP(int Id)
        {
            ViewBag.SolicitudId = Id; // Pasamos el ID a la vista para usarlo luego
            return View();
        }

        [HttpGet]
        public async Task<IActionResult> GetDetalleSolicitudGLP(int Id)
        {
            var solicitud = await repositorioIndicadores.ObtenerSolicitudPorIdGLP(Id);
            if (solicitud == null)
            {
                return NotFound();
            }
            return Json(solicitud);
        }



        //#11.-Precios de Gas LP

        public IActionResult Precio_Gas_LP()
        {
            return View();
        }

        //Obtiene el Numero de Semanas
        [HttpGet]
        public IActionResult GetNumeroDeSemanas(int year, int month)
        {
            var numSemanas = CalcularNumeroDeSemanas(year, month);
            return Json(new { numSemanas });
        }

        private int CalcularNumeroDeSemanas(int year, int month)
        {
            DateTime firstDayOfMonth = new DateTime(year, month, 1);
            DateTime lastDayOfMonth = firstDayOfMonth.AddMonths(1).AddDays(-1);

            int numSundays = 0;
            for (DateTime day = firstDayOfMonth; day <= lastDayOfMonth; day = day.AddDays(1))
            {
                if (day.DayOfWeek == DayOfWeek.Sunday)
                {
                    numSundays++;
                }
            }

            return numSundays;
        }






        [HttpGet]
        public async Task<IActionResult> GetPreciosGLP(int yearSelect, int mesSelect, int semanaSelect, string precioSelect)
        {
            var consulta = await repositorioIndicadores.ObtenerPreciosGLP(yearSelect, mesSelect, semanaSelect, precioSelect);
            if (consulta == null)
            {
                return NotFound();
            }
            return Json(consulta);
        }



        public async Task<IActionResult> GetExpendios()
        {
            var ExpendiosJson2 = await repositorioIndicadores.ObtenerExpendios();
            if (ExpendiosJson2 == null)
            {
                return NotFound(); // Manejar el caso en queregrese null
            }
            return Json(ExpendiosJson2);
        }

        #region ListaIndicador1

        public async Task<IActionResult> Index_I1()  //Por convección se usa Index para la vista que devuelve usuario elementos
        {
            var indicador1 = await repositorioIndicadores.Obtener_I1();
            return Json(indicador1);
            //return View(indicador1);

        }
        #endregion

        #region ListaIndicador2

        public async Task<IActionResult> Index_I2()   //Por convección se usa Index para la vista que devuelve usuario elementos
        {
            var indicador2 = await repositorioIndicadores.Obtener_I2();
            return View(indicador2);

        }
        #endregion

        #region ListaIndicador3

        public async Task<IActionResult> Index_I3()   //Por convección se usa Index para la vista que devuelve usuario elementos
        {
            var indicador3 = await repositorioIndicadores.Obtener_I3();
            return View(indicador3);

        }
        #endregion

        #region ListaIndicador4

        public async Task<IActionResult> Index_I4()   //Por convección se usa Index para la vista que devuelve usuario elementos
        {
            var indicador4 = await repositorioIndicadores.Obtener_I4();
            return View(indicador4);

        }
        #endregion

        #region Obtiene el Expendio desde el Mapa leaflet

        public async Task<IActionResult> DetalleExpendio(string numeroPermiso)
        {
            var expendio = await repositorioIndicadores.ObtenerExpendioPorNumeroPermiso(numeroPermiso);
            if (expendio == null)
            {
                return NotFound(); // Manejar el caso en que no se encuentre el expendio
            }

            // Verifica si el permiso pertenece al mercado de electricidad
            if (expendio.Mercado == "Electricidad")
            {
                // Obtiene los datos del dashboard
                var datosDashboard = await repositorioIndicadores.ObtenerDatosPorPermisoYAnio(numeroPermiso);
                expendio.DatosDashboard = datosDashboard.ToList(); // Asigna los datos al modelo
            }

            return View(expendio);
        }

        #endregion




        #region Acciones de Ejemplo manejo de Usuarios
        public async Task<ActionResult> _ResultadosEF_CFPartial(CalificacionFinal calificacionFinal)
        {
            var model = await repositorioIndicadores.D_Calificacion_por_EF(calificacionFinal);
            if (model == null)
            {
                return NotFound(); // Manejar el caso en que no se encuentre el expendio
            }

            return View(model);

        }

        [HttpPost]
        public async Task<IActionResult> Crear(Indicador2_Clase indicador2_Clase)
        {
            //1.-Validaciones del Modelo
            if (!ModelState.IsValid) //Preguntamos si el modelo no es valido
            {
                return View(indicador2_Clase);//Regresamos ala vista para que al usuario le salgan los mensajes de error,
                                              //al pasar "indicador2_Clase" evitamos que se borre la data del form cada
                                              //el usuario le de enviar
            }


            //  indicador2_Clase.id = 1;

            //Valida Usuarios Duplicados
            var yaexisteindicador = await repositorioIndicadores.Existe(indicador2_Clase.Usuario, indicador2_Clase.Email);
            if (yaexisteindicador)
            {
                ModelState.AddModelError(nameof(indicador2_Clase.Usuario), $"El nombre {indicador2_Clase.Usuario} ya existe.");
                return View(indicador2_Clase);
            }

            await repositorioIndicadores.Crear(indicador2_Clase);//Aqui esta la magía
            return View();
        }


        [HttpGet]
        public async Task<IActionResult> VerificarExisteUsuario(string usuario, string email)
        {
            //usuario = "U_Pruebaasync";
            email = "async@hotmail.com";

            var yaexisteindicador = await repositorioIndicadores.Existe(usuario, email);
            if (yaexisteindicador)
            {
                return Json($"El nombre {usuario} ya existe");
            }

            return Json(true);
        }
        public IActionResult Crear()
        {
            return View();
        }

        #endregion

        public IActionResult Datos()
        {
            return View();
        }
        public async Task<IActionResult> ObtieneTodoslosIndicadores()
        {
            var todoslosIndicadores = await repositorioIndicadores.Obtener_Indicadores();


            if (todoslosIndicadores == null)
            {
                return NotFound(); // Manejar el caso en que no se encuentre el expendio
            }
            //return View(todoslosIndicadores);
            return Json(todoslosIndicadores);

        }

        #region Municipios
        // Obtiene la Calificación de los Municipios Tabla
        [HttpPost]
        public async Task<IActionResult> C_Calificacion_por_MUN([FromBody] CalificacionFinal calificacionFinal)
        {
            var calificacion = await repositorioIndicadores.C_Calificacion_por_MUN(calificacionFinal);

            if (calificacion == null)
            {
                return NotFound(); // Manejar el caso en que no se encuentren los datos
            }

            return Json(calificacion);
        }


        // Obtiene la Calificación de los Totales
        [HttpPost]
        public async Task<IActionResult> B_Calificacion_Total_por_MUN([FromBody] CalificacionFinal calificacionFinal)
        {
            var calificacion = await repositorioIndicadores.B_Calificacion_Total_por_MUN(calificacionFinal);

            if (calificacion == null)
            {
                return NotFound(); // Manejar el caso en que no se encuentren los datos
            }

            return Json(calificacion);
        }


        //Detalle de Cada Municipio
        public async Task<IActionResult> Detalle_Mun([FromQuery] CalificacionFinal calificacionFinal)
        {

            var calificacion = await repositorioIndicadores.Detalle_MUN(calificacionFinal);

            if (calificacion == null)
            {
                return NotFound(); // Manejar el caso en que no se encuentre el expendio
            }
            return View(calificacion);
            //return Json(calificacion);

        }


        #endregion





        #region Evalución de Solicitud al Pública
        public IActionResult MapaSPP()
        {
            return View();
        }

        [HttpPost]
        public async Task<IActionResult> evalua_SolicitudPublica([FromBody] CalificacionFinal calificacionFinal)
        {
            try
            {
                var calificacion = await repositorioIndicadores.PRE_Calificacion_Solicitud_Pública(calificacionFinal);

                if (calificacion == null)
                {
                    return NotFound(); // Manejar el caso en que no se encuentre el expendio
                }

                return Json(calificacion);
            }
            catch (Exception ex)
            {
                // Loguear la excepción si es necesario
                return StatusCode(500, new { message = ex.Message });
            }
        }

        [HttpPost]
        public async Task<IActionResult> bitacoraIndicadoresRegistro([FromBody] LogEvaluaciones datos)
        {
            try
            {
                Console.WriteLine($"Hasta aquí llegamos: {datos.IdLog}");
                // Guardar los datos en la base de datos
                await repositorioIndicadores.devuelveBitacoraRegistro(datos);


                return Ok("Datos guardados exitosamente");
            }
            catch (Exception ex)
            {
                // Loguear la excepción si es necesario
                return StatusCode(500, new { message = ex.Message });
            }
        }

        [HttpPost]
        public async Task<IActionResult> bitacoraIndicadoresRegistroCRE([FromBody] LogEvaluacionesCRE datos)
        {
            try
            {
                Console.WriteLine($"Hasta aquí llegamos: {datos.IdLog}");
                // Guardar los datos en la base de datos
                await repositorioIndicadores.devuelveBitacoraRegistroCRE(datos);


                return Ok("Datos guardados exitosamente");
            }
            catch (Exception ex)
            {
                // Loguear la excepción si es necesario
                return StatusCode(500, new { message = ex.Message });
            }
        }
        //ISOCRONA
        [HttpPost]
        public async Task<IActionResult> getIsocrona([FromBody] Isocrona isocrona)
        {
            try
            {
                var resultado = await repositorioIndicadores.GetIsocronaTotales(isocrona);

                if (resultado == null)
                {
                    return NotFound(); // Manejar el caso en que no se encuentre el expendio
                }

                return Json(resultado);
            }
            catch (Exception ex)
            {
                // Loguear la excepción si es necesario
                return StatusCode(500, new { message = ex.Message });
            }
        }


        #endregion








        #region Carga de Solicitudes
        //A)Petroliferos
        public IActionResult Cargar_SE()
        {
            return View();
        }
        [HttpPost]
        public async Task<IActionResult> Cargar_SE(IFormFile file)
        {
            if (file != null && Path.GetExtension(file.FileName) == ".csv")
            {
                List<CargaSolicitudExpendio> solicitudes = new List<CargaSolicitudExpendio>();

                try
                {
                    using (var reader = new StreamReader(file.OpenReadStream()))
                    {
                        while (!reader.EndOfStream)
                        {
                            var line = reader.ReadLine();
                            var values = line.Split(',');

                            // Suponiendo que las columnas están en el mismo orden que en la tabla
                            CargaSolicitudExpendio solicitud = new CargaSolicitudExpendio();

                            if (!int.TryParse(values[0], out int id))
                                throw new FormatException("ID no es válido.");
                            solicitud.ID = id;

                            solicitud.EF_ID = values[1];
                            solicitud.MPO_ID = values[2];

                            if (!DateTime.TryParse(values[3], out DateTime fechaSolicitud))
                                throw new FormatException("Fecha de solicitud no es válida.");
                            solicitud.Fecha_solicitud = fechaSolicitud;

                            solicitud.Turno = values[4];
                            solicitud.Expediente = values[5];
                            solicitud.Razon_social = values[6];
                            solicitud.Actividad_regulada = values[7];
                            solicitud.Direccion = values[8];

                            if (!double.TryParse(values[9], out double xGeo))
                                throw new FormatException("El formato de X_Geo no es válido.");
                            solicitud.X_Geo = xGeo;

                            if (!double.TryParse(values[10], out double yGeo))
                                throw new FormatException("El formato de Y_Geo no es válido.");
                            solicitud.Y_Geo = yGeo;

                            solicitud.Marca_solicitada = values[11];

                            if (!bool.TryParse(values[12], out bool documentosCompletos))
                                throw new FormatException("El formato de Documentos Completos no es válido.");
                            solicitud.Documentos_completos = documentosCompletos;

                            if (!bool.TryParse(values[13], out bool analisisRiesgo))
                                throw new FormatException("El formato de Análisis de Riesgo no es válido.");
                            solicitud.Analisis_riesgo = analisisRiesgo;

                            if (!bool.TryParse(values[14], out bool vecinosPemex))
                                throw new FormatException("El formato de vecinos PEMEX no es válido.");
                            solicitud.vecinos_PEMEX = vecinosPemex;

                            if (!bool.TryParse(values[15], out bool vecinosOtras))
                                throw new FormatException("El formato de vecinos OTRAS no es válido.");
                            solicitud.vecinos_OTRAS = vecinosOtras;

                            solicitud.Tipo_Persona = values[16];
                            solicitud.RFC = values[17];
                            solicitud.Codigo_Postal = values[18];
                            solicitud.Comentarios = values[19];

                            solicitudes.Add(solicitud);
                        }
                    }

                    int registrosInsertados = await repositorioIndicadores.InsertarSolicitudesBulk(solicitudes);

                    if (registrosInsertados > 0)
                    {
                        // Mostrar mensaje de éxito o redirigir a otra vista.
                        return RedirectToAction("Index");
                    }
                    else
                    {
                        ModelState.AddModelError("file", "Hubo un error al procesar el archivo. Inténtalo de nuevo.");
                        return View();
                    }
                }
                catch (Exception ex)
                {
                    // Aquí puedes registrar la excepción si tienes un sistema de registro.
                    ModelState.AddModelError("file", $"Ocurrió un error al procesar el archivo: {ex.Message}");
                    return View();
                }
            }

            ModelState.AddModelError("file", "Por favor, carga un archivo CSV válido.");
            return View();
        }


        [HttpGet]
        public async Task<IActionResult> ObtenerDatosGrafico()
        {
            var datos = await repositorioIndicadores.ObtenerPermisosPorEntidad();
            return Json(datos);
        }


        //Evalua Solicitud
        //    public IActionResult EvaluaSolicitud(int Id)
        // {
        //     ViewBag.SolicitudId = Id; // Pasamos el ID a la vista para usarlo luego
        //     return View();
        // }public IActionResult EvaluaSolicitud(int Id, string mercado)
        public IActionResult EvaluaSolicitud(int Id, string mercado)
        {
            ViewBag.SolicitudId = Id;
            ViewBag.Mercado = mercado; // Pasas el mercado a la vista
            return View();
        }


    }
    #endregion






}

