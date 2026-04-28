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




namespace NSIE.Controllers
{
    [ServiceFilter(typeof(ValidacionInputFiltro))]
    [AutorizacionFiltro]
    public class AtlasController : Controller
    {
        private readonly IRepositorioAtlas repositorioAtlas;
        private readonly IHttpClientFactory _httpClientFactory;


        public AtlasController(IRepositorioAtlas repositorioAtlas, IHttpClientFactory httpClientFactory)
        {

            this.repositorioAtlas = repositorioAtlas;
            _httpClientFactory = httpClientFactory;
        }

        [HttpGet]
        [Route("Atlas/ProxyImagen")]
        public async Task<IActionResult> ProxyImagen([FromQuery] string url)
        {
            if (string.IsNullOrWhiteSpace(url))
                return BadRequest("Se requiere una URL");

            try
            {
                var client = _httpClientFactory.CreateClient();
                var response = await client.GetAsync(url);

                if (!response.IsSuccessStatusCode)
                    return StatusCode((int)response.StatusCode, "No se pudo obtener la imagen");

                var contentType = response.Content.Headers.ContentType?.ToString() ?? "image/png";
                var content = await response.Content.ReadAsByteArrayAsync();

                Response.Headers.Add("Access-Control-Allow-Origin", "*");
                return File(content, contentType);
            }
            catch (Exception ex)
            {
                return StatusCode(500, $"Error al obtener la imagen: {ex.Message}");
            }
        }


        public IActionResult AZEL()
        {
            return View();
        }

        public IActionResult AZEL_Publico()
        {
            return View();
        }

        

    }
}
