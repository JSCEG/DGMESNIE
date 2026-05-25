using Microsoft.AspNetCore.Mvc;
using System.Net.Http;
using System.Text;
using System.Threading.Tasks;
using System;

namespace NSIE.Controllers
{
    [ServiceFilter(typeof(ValidacionInputFiltro))]
    [AutorizacionFiltro]
    public class CNEController : Controller
    {
        private readonly IHttpClientFactory _httpClientFactory;

        public CNEController(IHttpClientFactory httpClientFactory)
        {
            _httpClientFactory = httpClientFactory;
        }

        public IActionResult Index()
        {
            return View();
        }

        [HttpPost]
        public async Task<IActionResult> ObtenerPermisosPaginados([FromBody] System.Text.Json.JsonElement parameters)
        {
            try
            {
                var client = _httpClientFactory.CreateClient();
                
                // Construir el payload esperado por la CNE: { "parameters": ... }
                var payload = new { parameters = parameters };
                var jsonPayload = System.Text.Json.JsonSerializer.Serialize(payload);
                
                var content = new StringContent(jsonPayload, Encoding.UTF8);
                content.Headers.ContentType = new System.Net.Http.Headers.MediaTypeHeaderValue("application/json") 
                { 
                    CharSet = "utf-8" 
                };

                // Configurar headers para emular el sitio oficial y evitar bloqueos
                client.DefaultRequestHeaders.Clear();
                client.DefaultRequestHeaders.Add("User-Agent", "Mozilla/5.0 (Windows NT 10.0; Win64; x64) AppleWebKit/537.36 (KHTML, like Gecko) Chrome/120.0.0.0 Safari/537.36");
                client.DefaultRequestHeaders.Add("Origin", "https://www.cne.gob.mx");
                client.DefaultRequestHeaders.Referrer = new Uri("https://www.cne.gob.mx/");

                var response = await client.PostAsync("https://api-creweb.cne.gob.mx/api/Permisos/ObtenerPermisosPaginados", content);
                
                var responseString = await response.Content.ReadAsStringAsync();
                
                if (!response.IsSuccessStatusCode)
                {
                    return StatusCode((int)response.StatusCode, responseString);
                }

                return Content(responseString, "application/json");
            }
            catch (System.Exception ex)
            {
                return StatusCode(500, new { Message = "Error interno al consultar el servicio de la CNE.", Details = ex.Message });
            }
        }

        [HttpGet]
        public async Task<IActionResult> ObtenerResoluciones(string resolucionId)
        {
            try
            {
                if (string.IsNullOrWhiteSpace(resolucionId))
                {
                    return BadRequest("El id de resolución es requerido.");
                }

                var client = _httpClientFactory.CreateClient();
                
                client.DefaultRequestHeaders.Clear();
                client.DefaultRequestHeaders.Add("User-Agent", "Mozilla/5.0 (Windows NT 10.0; Win64; x64) AppleWebKit/537.36 (KHTML, like Gecko) Chrome/120.0.0.0 Safari/537.36");
                client.DefaultRequestHeaders.Add("Origin", "https://www.cne.gob.mx");
                client.DefaultRequestHeaders.Referrer = new Uri("https://www.cne.gob.mx/");

                var url = $"https://api-publico.cne.gob.mx/api/Resoluciones/?resolucionId={resolucionId}&tipoEntidad=2";
                var response = await client.GetAsync(url);
                
                var responseString = await response.Content.ReadAsStringAsync();

                if (!response.IsSuccessStatusCode)
                {
                    return StatusCode((int)response.StatusCode, responseString);
                }

                return Content(responseString, "application/json");
            }
            catch (System.Exception ex)
            {
                return StatusCode(500, new { Message = "Error interno al obtener resoluciones.", Details = ex.Message });
            }
        }
    }
}
