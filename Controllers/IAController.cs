using Microsoft.AspNetCore.Mvc;
using NSIE.Servicios;
using System.Threading.Tasks;
using System.Net.Http.Headers;

namespace NSIE.Controllers
{
    [ServiceFilter(typeof(ValidacionInputFiltro))]
    [AutorizacionFiltro]
    public class IAController : Controller
    {
        private readonly IRepositorioIA _repositorioIA;

        public IAController(IRepositorioIA repositorioIA)
        {
            _repositorioIA = repositorioIA;
        }

        public IActionResult Asistente()
        {
            return View();
        }

        [HttpPost]
        public async Task<IActionResult> Ask(string prompt)
        {
            if (string.IsNullOrWhiteSpace(prompt))
            {
                return View("Asistente", model: "Por favor, ingrese algo para preguntar.");
            }

            var response = await _repositorioIA.AskIA(prompt);
            return View("Asistente", model: response);
        }

        [HttpPost]
        public async Task<IActionResult> QueryDatabase(string prompt)
        {
            if (string.IsNullOrWhiteSpace(prompt))
            {
                return View("Asistente", model: "Por favor, ingrese algo para preguntar.");
            }

            try
            {
                // Generar la consulta SQL usando ChatGPT
                var sqlQuery = await _repositorioIA.AskIA($"Genera una consulta SQL para la siguiente pregunta: {prompt}");

                // Ejecutar la consulta en la base de datos
                var result = await _repositorioIA.ExecuteQueryAsync(sqlQuery);

                return View("Asistente", model: result);
            }
            catch (System.Exception ex)
            {
                return View("Asistente", model: $"Error al realizar la consulta: {ex.Message}");
            }
        }
    }
}
