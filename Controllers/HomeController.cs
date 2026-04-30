using System.Diagnostics;
using Microsoft.AspNetCore.Mvc;
using NSIE.Models;
using NSIE.Servicios;
using NSIE.Servicios.Interfaces;  // Actualizar este using
using Newtonsoft.Json.Linq;
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
        private readonly HttpClient _client;
        private readonly IRepositorioHome _repositorioHome;



        public HomeController(
            ILogger<HomeController> logger,
            IRepositorioProyectos repositorioProyectos,
            // IServicioEmail servicioEmail,
            IRepositorioHome repositorioHome)
        {
            _logger = logger;
            _repositorioProyectos = repositorioProyectos;           // Sin this
                                                                    //   _servicioEmail = servicioEmail;                         // Sin this
            _client = new HttpClient();
            _repositorioHome = repositorioHome;                     // Sin this
        }


        /// </returns>

        public async Task<IActionResult> Index(string section = null, string module = null)
        {
            var perfilUsuarioJson = HttpContext.Session.GetString("PerfilUsuario");
            var perfilUsuario = JsonConvert.DeserializeObject<PerfilUsuario>(perfilUsuarioJson);
            var seccionesUsuarioJson = HttpContext.Session.GetString("SeccionesUsuario");
            var seccionesFiltradas = string.IsNullOrWhiteSpace(seccionesUsuarioJson)
                ? await _repositorioHome.ObtenerSeccionesConModulos()
                : JsonConvert.DeserializeObject<List<SeccionSNIER>>(seccionesUsuarioJson) ?? new List<SeccionSNIER>();

            seccionesFiltradas = seccionesFiltradas.Where(s => s.Modulos != null && s.Modulos.Any()).ToList();

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

            var queryUrl = $"https://www.alphavantage.co/query?function=GLOBAL_QUOTE&symbol={Uri.EscapeDataString(symbol ?? string.Empty)}&apikey={alphaVantageApiKey}";

            Uri queryUri = new Uri(queryUrl);

            string response = await _client.GetStringAsync(queryUri);
            var data = JObject.Parse(response);

            // Comprobamos si "Global Quote" está vacío
            if (data["Global Quote"] == null || !data["Global Quote"].HasValues)
            {
                return Json(new { error = "No quote data returned. Please check if the stock symbol is correct and try again later." });
            }

            // Devolvemos todos los datos de "Global Quote" en lugar de solo el precio
            return Json(data["Global Quote"]);
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

