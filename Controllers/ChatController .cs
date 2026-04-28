using Microsoft.AspNetCore.Mvc;
using NSIE.Servicios;
using System.Threading.Tasks;
using System.Data;

namespace NSIE.Controllers
{
    [ServiceFilter(typeof(ValidacionInputFiltro))]
    [AutorizacionFiltro]
    public class ChatController : Controller
    {
        private readonly IRepositorioChat _repositorioChat;

        public ChatController(IRepositorioChat repositorioChat)
        {
            _repositorioChat = repositorioChat;
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
                // Manejar el caso en que el prompt esté vacío
                return View("Asistente", model: "Por favor, ingrese algo para preguntar.");
            }

            var response = await _repositorioChat.AskGPTAsync(prompt);
            // Enviar la respuesta a la vista
            return View("Asistente", model: response);
        }
        public IActionResult A_VV()
        {
            return View();
        }
        [HttpPost]
        public async Task<IActionResult> AskAVV(string prompt)
        {
            if (string.IsNullOrWhiteSpace(prompt))
            {
                // Manejar el caso en que el prompt esté vacío
                return View("Asistente", model: "Por favor, ingrese algo para preguntar.");
            }

            var response = await _repositorioChat.AskVisitasdeVerificaciónAsync(prompt);
            // Enviar la respuesta a la vista
            return View("Asistente", model: response);
        }

        public IActionResult ConsultaPermisos()
        {
            return View();
        }
        public IActionResult MarcoRegulatorioH()
        {
            return View();
        }

        //    Consulta a BD
        public IActionResult ConsultaBD()
        {
            return View();
        }


        [HttpPost]
        public async Task<IActionResult> EjecutarConsulta(string prompt)
        {
            if (string.IsNullOrWhiteSpace(prompt))
            {
                return View("ConsultaBD", model: "Por favor, ingrese una consulta SQL.");
            }

            var resultado = await _repositorioChat.ConsultarBDAsync(prompt);
            return View("ConsultaBD", model: resultado);
        }




        public IActionResult ConsultaNatural()
        {
            return View();
        }

        [HttpPost]
        public async Task<IActionResult> EjecutarConsultaNatural(string prompt)
        {
            if (string.IsNullOrWhiteSpace(prompt))
            {
                return View("ConsultaNatural", model: "Por favor, ingrese una pregunta.");
            }

            var resultado = await _repositorioChat.GenerarConsultaSQLAsync(prompt);
            return View("ConsultaNatural", model: resultado);
        }

    }
}
