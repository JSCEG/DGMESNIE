using System.Data;
using System.Diagnostics;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Data.SqlClient;
using NSIE.Models;
using NSIE.Servicios;
using NSIE.Servicios.Interfaces;  // Actualizar este using
using Newtonsoft.Json.Linq;
using System.Net;
using Newtonsoft.Json;

namespace NSIE.Controllers
{
    [ServiceFilter(typeof(ValidacionInputFiltro))]
    [AutorizacionFiltro]
    public class HomeController : Controller
    {
        private readonly ILogger<HomeController> _logger;
        private readonly IRepositorioProyectos _repositorioProyectos; // Usar _ consistentemente
                                                                      //  private readonly IServicioEmail _servicioEmail;               // Usar _ consistentemente
        private readonly string _connectionString;                    // Usar _ consistentemente
        private readonly HttpClient _client;
        private readonly IRepositorioHome _repositorioHome;



        public HomeController(
            ILogger<HomeController> logger,
            IRepositorioProyectos repositorioProyectos,
            IConfiguration configuration,
            // IServicioEmail servicioEmail,
            IRepositorioHome repositorioHome)
        {
            _logger = logger;
            _repositorioProyectos = repositorioProyectos;           // Sin this
                                                                    //   _servicioEmail = servicioEmail;                         // Sin this
            _connectionString = configuration.GetConnectionString("DefaultConnection");
            _client = new HttpClient();
            _repositorioHome = repositorioHome;                     // Sin this
        }



        public IActionResult ListaUsuarios()
        {
            try
            {

                #region Obtener DataTable de SQL - Lista Usuarios

                SqlConnection con = new SqlConnection(_connectionString);//cadenap
                string consulta = "SELECT [id],[Usuario],[Email],[EmailNormalizado],[PasswordHash] FROM[cre-db-2].[dbo].[Usuarios]";
                SqlDataAdapter da = new SqlDataAdapter(consulta, con);

                DataTable dt = new DataTable();
                da.Fill(dt);



                #endregion

                TempData["MSG"] = "Estos son todos los Usuarios";
                return View("ListaUsuarios", dt);

            }
            catch (Exception)
            {
                TempData["ERROR"] = "";

                return View();
            }
        }


        /// </returns>

        public async Task<IActionResult> Index(string section = null, string module = null)
        {
            var perfilUsuarioJson = HttpContext.Session.GetString("PerfilUsuario");
            var perfilUsuario = JsonConvert.DeserializeObject<PerfilUsuario>(perfilUsuarioJson);
            var rolUsuario = perfilUsuario?.Rol?.ToString() ?? "";

            // Log para depuración
            Console.WriteLine($"Rol que se manda al filtro: '{rolUsuario}'");

            // Trae todas las secciones y módulos activos (sin filtrar por rol)
            var secciones = await _repositorioHome.ObtenerSeccionesConModulos();

            // Filtra los módulos por el rol del usuario usando la columna Roles (IDs)
            foreach (var seccion in secciones)
            {
                var modulosFiltrados = new List<ModuloSNIER>();
                foreach (var m in seccion.Modulos)
                {
                    var rolesModulo = m.Roles ?? "";
                    var rolesArray = rolesModulo.Split(',').Select(r => r.Trim()).ToList();
                    Console.WriteLine($"Comparando módulo '{m.Title}' (Roles: '{rolesModulo}') con rol usuario '{rolUsuario}'");

                    if (string.IsNullOrEmpty(rolesModulo) || rolesArray.Contains(rolUsuario))
                    {
                        Console.WriteLine($"--> El módulo '{m.Title}' SÍ es visible para el usuario.");
                        modulosFiltrados.Add(m);
                    }
                    else
                    {
                        Console.WriteLine($"--> El módulo '{m.Title}' NO es visible para el usuario.");
                    }
                }
                seccion.Modulos = modulosFiltrados;
            }

            // Solo deja secciones con al menos un módulo visible
            var seccionesFiltradas = secciones.Where(s => s.Modulos.Any()).ToList();

            var modelo = new HomeViewModel
            {
                PerfilUsuario = perfilUsuario,
                Secciones = seccionesFiltradas
            };

            if (!string.IsNullOrEmpty(section))
            {
                ViewData["ActiveSection"] = section;
                ViewData["ActiveModule"] = module;
            }

            return View(modelo);
        }

        #region API Indicadores Financieros

        public async Task<IActionResult> GetStockData(string symbol)
        {
            var alphaVantageApiKey = Environment.GetEnvironmentVariable("ALPHAVANTAGE_API_KEY") ?? string.Empty;
            if (string.IsNullOrWhiteSpace(alphaVantageApiKey))
            {
                return StatusCode(StatusCodes.Status503ServiceUnavailable, "Alpha Vantage API key is not configured.");
            }

            var queryUrl = $"https://www.alphavantage.co/query?function=GLOBAL_QUOTE&symbol={symbol}&apikey={alphaVantageApiKey}";

            Uri queryUri = new Uri(queryUrl);

            using (WebClient client = new WebClient())
            {
                string response = await client.DownloadStringTaskAsync(queryUri);
                var data = JObject.Parse(response);

                // Comprobamos si "Global Quote" está vacío
                if (data["Global Quote"] == null || !data["Global Quote"].HasValues)
                {
                    return Json(new { error = "No quote data returned. Please check if the stock symbol is correct and try again later." });
                }

                // Devolvemos todos los datos de "Global Quote" en lugar de solo el precio
                return Json(data["Global Quote"]);
            }
        }


        #endregion




        public IActionResult Privacy()
        {
            return View();
        }

        public IActionResult Map()
        {

            //  var Usuarios = repositorioProyectos.ObtenerU().Take(3).ToList();
            var modelin = new HomeIndex()
            {
                //     AccesosLocales = Usuarios,
            };

            return View(modelin);
        }
        public IActionResult Mapas_por_Mercado()
        {
            return View();
        }

        public IActionResult Hidrocarburos()
        {
            return View();
        }


        public IActionResult Mapa_SEM()
        {
            return View();
        }

        [ResponseCache(Duration = 0, Location = ResponseCacheLocation.None, NoStore = true)]
        public IActionResult Error()
        {
            return View(new ErrorViewModel { RequestId = Activity.Current?.Id ?? HttpContext.TraceIdentifier });
        }

        #region Electricidad

        public IActionResult Electricidad()
        {
            return View();
        }



        #endregion


        #region Resultados
        public IActionResult MEP()
        {
            return View();
        }


        #endregion

        #region Votaciones
        public IActionResult Votaciones()
        {
            return View();
        }


        #endregion


        #region Contacto

        [HttpGet]
        public IActionResult Contacto()
        {
            return View();
        }

        [HttpPost]
        public IActionResult Contacto(ContactoViewModel contactoViewModel)
        //        public async Task<IActionResult> Contacto(ContactoViewModel contactoViewModel)

        {
            // await servicioEmail.Enviar(contactoViewModel);
            return RedirectToAction("Gracias");
        }
        #endregion


        #region Gracias

        public IActionResult Gracias()
        {
            return View();
        }
        #endregion


        #region En construcción

        public IActionResult EnConstruccion()
        {
            return View();
        }
        public IActionResult Senier_Secciones()
        {
            return View();
        }

        #endregion








    }
}

