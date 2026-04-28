using Microsoft.AspNetCore.Mvc;
using Microsoft.Data.SqlClient;
using NSIE.Models;
using NSIE.Servicios;
using System.Collections.Generic;
using System.Data.SqlClient;
using System.Text.Json;

namespace NSIE.Controllers
{
    [ServiceFilter(typeof(ValidacionInputFiltro))]
    [AutorizacionFiltro]
    public class MapController : Controller
    {

        private readonly IRepositorioMap repositorioMap;
        private readonly string _connectionString;

        public MapController(IRepositorioMap repositorioMap, IConfiguration configuration)
        {

            this.repositorioMap = repositorioMap;
            _connectionString = configuration.GetConnectionString("DefaultConnection") ?? string.Empty;
        }


        public ActionResult Mapa()
        {
            List<Coordenada> coordenadas = new List<Coordenada>();

            string query = "SELECT Latitud, Longitud, Titulo,Descripcion,Imagen FROM Coordenadas";

            using (SqlConnection connection = new SqlConnection(_connectionString))
            {
                SqlCommand command = new SqlCommand(query, connection);
                connection.Open();
                SqlDataReader reader = command.ExecuteReader();

                while (reader.Read())
                {
                    double latitud = (double)reader["Latitud"];
                    double longitud = (double)reader["Longitud"];
                    string titulo = (string)reader["Titulo"];
                    string descripcion = (string)reader["Descripcion"];
                    string imagen = (string)reader["Imagen"];

                    Coordenada coordenada = new Coordenada { Latitud = latitud, Longitud = longitud, Titulo = titulo, Descripcion = descripcion, Imagen = imagen };
                    coordenadas.Add(coordenada);
                }

                reader.Close();
                connection.Close();
            }

            string coordenadasJson = JsonSerializer.Serialize(coordenadas);

            ViewBag.CoordenadasJson = coordenadasJson;

            return View(coordenadas);
        }
        // Huracanes

        public IActionResult Huracan_Jhon_2024()
        {
            return View();
        }
        public IActionResult Huracan_Eleven_E()
        {
            return View();
        }
        // Fin Huracanes

        public IActionResult Gas_Natural()
        {
            return View();
        }
        public IActionResult Carbon()
        {
            return View();
        }

        public IActionResult Yacimientos_Geotermicos()
        {
            return View();
        }

        public IActionResult Biomasa()
        {
            return View();
        }

        public IActionResult Reserva_de_Crudo()
        {
            return View();
        }
        public IActionResult Reserva_de_Uranio()
        {
            return View();
        }
        public IActionResult Reserva_de_Agua()
        {
            return View();
        }
        public IActionResult Velocidad_de_Viento()
        {
            return View();
        }
        public IActionResult Radiacion_Solar()
        {
            return View();
        }
        //Infras 
        public IActionResult I_Radiacion_Solar()
        {
            return View();
        }
        public IActionResult I_Eolica()
        {
            return View();
        }
        public IActionResult I_Hidroelectrica()
        {
            return View();
        }
        public IActionResult I_NGCC()
        {
            return View();
        }
        public IActionResult I_Refinerias()
        {
            return View();
        }
        public IActionResult I_Complejos()
        {
            return View();
        }
        public IActionResult I_Nuclear()
        {
            return View();
        }
        public IActionResult I_Yacimientos_Geotermicos()
        {
            return View();
        }

        public IActionResult I_Biomasa()
        {
            return View();
        }


        public IActionResult I_CHP()
        {
            return View();
        }
        public IActionResult I_CE()
        {
            return View();
        }

        public IActionResult I_TurboGas()
        {
            return View();
        }
        public IActionResult I_CI()
        {
            return View();
        }
        public IActionResult I_Carbo()
        {
            return View();
        }
        public IActionResult I_Gas_Natural()
        {
            return View();
        }
        public IActionResult I_Gas_LP()
        {
            return View();
        }
        public IActionResult I_Petroliferos()
        {
            return View();
        }

        ///Infraestructura_SEM
        public IActionResult Infraestructura_SEM()
        {
            return View();
        }
        // Infraestrtrutura del SEN
        public IActionResult I_EConvencional()
        {
            return View();
        }
        public IActionResult I_ELimpia()
        {
            return View();
        }
        public IActionResult TE_Petroliferos()
        {
            return View();
        }
        public IActionResult TE_GasSeco()
        {
            return View();
        }
        public IActionResult TE_GasLP()
        {
            return View();
        }
        public IActionResult TE_CargaPico()
        {
            return View();
        }
        public IActionResult TE_CargaBase()
        {
            return View();
        }
        public IActionResult TE_Intermitente()
        {
            return View();
        }
        public IActionResult UF_CO2e()
        {
            return View();
        }
        public IActionResult TE_Hogares()
        {
            return View();
        }
        public IActionResult TE_Transporte()
        {
            return View();
        }
        public IActionResult TE_SPyC()
        {
            return View();
        }
        public IActionResult TE_Agricultura()
        {
            return View();
        }
        public IActionResult TE_Industria()
        {
            return View();
        }
        public IActionResult TE_SE()
        {
            return View();
        }
        public IActionResult PYP_Calor()
        {
            return View();
        }
        public IActionResult PYP_Combustible()
        {
            return View();
        }
        public IActionResult PYP_Electricidad()
        {
            return View();
        }
        public IActionResult RS_RefyDes()
        {
            return View();
        }
        public IActionResult RS_CG()
        {
            return View();
        }
        public IActionResult RS_Autoconsumo()
        {
            return View();
        }

        public IActionResult Electricidad_CO2e()
        {
            return View();
        }


        public IActionResult Hidrocarburos_CO2e()
        {
            return View();
        }
        public async Task<IActionResult> DetalleUsosFinales(int año, string tipo)
        {
            var usofinal = await repositorioMap.ObtenerDetalleUsosFinales(año, tipo);
            if (usofinal == null)
            {
                return NotFound(); // Manejar el caso en que no se encuentre el expendio
            }

            // Personaliza el título de la página
            ViewData["Title"] = $"Detalle de Consumo {tipo} en {año}";
            ViewData["AñoSeleccionado"] = año;
            ViewData["TipoSeleccionado"] = tipo;

            return View(usofinal);
        }

        public async Task<IActionResult> Obtener_TiposRS()  //Por convección se usa Index para la vista que devuelve usuario elementos
        {
            var tiposRS = await repositorioMap.Obtener_TiposRS();

            if (tiposRS == null)
            {
                return NotFound(); // Manejar el caso en queregrese null
            }
            return Json(tiposRS);
        }


        public async Task<IActionResult> Obtener_TiposEnergia()  //Por convección se usa Index para la vista que devuelve usuario elementos
        {
            var tiposEnergia = await repositorioMap.Obtener_TiposEnergia();

            if (tiposEnergia == null)
            {
                return NotFound(); // Manejar el caso en queregrese null
            }
            return Json(tiposEnergia);
        }

        public async Task<IActionResult> Obtener_Produccion_y_Provision()  //Por convección se usa Index para la vista que devuelve usuario elementos
        {
            var tiposPYP = await repositorioMap.Obtener_TiposPYP();

            if (tiposPYP == null)
            {
                return NotFound(); // Manejar el caso en queregrese null
            }
            return Json(tiposPYP);
        }


        public async Task<IActionResult> Obtener_UsosFinales()  //Por convección se usa Index para la vista que devuelve usuario elementos
        {
            var tiposEnergia = await repositorioMap.Obtener_UsosFinales();

            if (tiposEnergia == null)
            {
                return NotFound(); // Manejar el caso en queregrese null
            }
            return Json(tiposEnergia);
        }


        public async Task<IActionResult> Obtener_EmisionesxSector()  //Por convección se usa Index para la vista que devuelve usuario elementos
        {
            var emisiones = await repositorioMap.Obtener_EmisionesxSector();

            if (emisiones == null)
            {
                return NotFound(); // Manejar el caso en queregrese null
            }
            return Json(emisiones);
        }




    }
    public class Coordenada
    {
        public double Latitud { get; set; }
        public double Longitud { get; set; }
        public string Titulo { get; set; }
        public string Descripcion { get; set; }
        public string Imagen { get; set; }
    }

}




