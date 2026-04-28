using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Rendering;
using Microsoft.Data.SqlClient;
using NSIE.Models;
using NSIE.Servicios;
using System.Security.Cryptography;
using System.Text;
using System.Data;
using System.Threading.Tasks;
using Newtonsoft.Json;
using System.Diagnostics;


namespace NSIE.Controllers
{
    [ServiceFilter(typeof(ValidacionInputFiltro))]
    [AutorizacionFiltro]
    public class ProyEstrategicosController : Controller
    {
        private readonly IRepositorioProyEstrategicos repositorioProyEstrategicos;


        public ProyEstrategicosController(IRepositorioProyEstrategicos repositorioProyEstrategicos)
        {

            this.repositorioProyEstrategicos = repositorioProyEstrategicos;
        }

        public IActionResult MenuPE()
        {
            return View();
        }

        public IActionResult FOTEASE()
        {
            return View();
        }


        // Lista de Proyectos
        [HttpGet]
        public async Task<IActionResult> ListaProyectos()
        {
            var proyectos = await repositorioProyEstrategicos.ObtenerTodosProyectosEstrategicos();
            var proyectosOrdenados = proyectos.OrderByDescending(p => p.FechaIngreso).ToList();
            return View(proyectosOrdenados);
        }

        // VISTA PARA LOS PROYECTOS ESTRATEGICOS
        [HttpGet]
        public async Task<IActionResult> AgregarProyecto()
        {
            var mercados = await repositorioProyEstrategicos.ObtenerTodosMercados();
            ViewBag.Mercados = new SelectList(mercados, "Mercado_ID", "Mercado_Nombre");
            return View();
        }
        // AGREGA UN PROYECTO ESTRATEGICO A LA LISTA DE PROYECTOS    


        [HttpPost]
        public async Task<IActionResult> AgregarProyecto(ProyectoEstrategico proyecto)
        {
            if (ModelState.IsValid)
            {
                proyecto.FechaIngreso = DateTime.Now; // Establecer la fecha de ingreso a la fecha actual
                await repositorioProyEstrategicos.AgregarProyecto(proyecto);
                return RedirectToAction("ProyectosEstrategicos");
            }

            var mercados = await repositorioProyEstrategicos.ObtenerTodosMercados();
            ViewBag.Mercados = new SelectList(mercados, "Mercado_ID", "Mercado_Nombre");
            return View(proyecto);
        }


        // Seguimiento de los Proyectos Estratégicos
        public async Task<IActionResult> ProyectosEstrategicos()
        {
            var perfilUsuario = JsonConvert.DeserializeObject<PerfilUsuario>(HttpContext.Session.GetString("PerfilUsuario"));

            int mercadoUsuario = int.Parse(perfilUsuario.Mercado_ID); // Convertir mercadoUsuario a int

            // Añadir mensaje de depuración para mercadoUsuario
            Debug.WriteLine($"MercadoUsuario: {mercadoUsuario}");

            // Si el mercado es 0, no filtrar por Mercado_ID
            int? mercadoId = mercadoUsuario == 0 ? (int?)null : mercadoUsuario;
            var proyectos = await repositorioProyEstrategicos.ObtenerTodosProyectosEstrategicos(mercadoId);

            // Añadir mensaje de depuración para verificar el número de proyectos
            Debug.WriteLine($"Número de proyectos obtenidos: {proyectos.Count()}");

            // Preparar los datos para el gráfico de columnas
            var chartData = proyectos.Select(p => new
            {
                NombreProyecto = p.NombreProyecto,
                Avance = p.CalcularAvance(),
                TotalTramites = p.Tramites.Count
            }).ToList();

            ViewBag.ChartData = JsonConvert.SerializeObject(chartData);

            // Añadir mensaje de depuración para chartData
            Debug.WriteLine($"ChartData: {ViewBag.ChartData}");

            // Preparar los datos para el gráfico de pastel
            var proyectosPorMercado = proyectos
                .GroupBy(p => p.Mercado)
                .Select(g => new { Mercado = g.Key, TotalProyectos = g.Count() })
                .ToList();

            ViewBag.ChartDataPie = JsonConvert.SerializeObject(proyectosPorMercado);

            // Añadir mensaje de depuración para chartDataPie
            Debug.WriteLine($"ChartDataPie: {ViewBag.ChartDataPie}");

            return View(proyectos);
        }




        public async Task<IActionResult> Detalle(int id, string vistaOrigen)
        {
            var perfilUsuario = JsonConvert.DeserializeObject<PerfilUsuario>(HttpContext.Session.GetString("PerfilUsuario"));

            // Convertir Mercado_ID a int
            int mercadoUsuario = int.Parse(perfilUsuario.Mercado_ID);

            // Obtener el proyecto por ID
            var proyecto = await repositorioProyEstrategicos.ObtenerProyectoPorId(id);

            // Verificar si el usuario tiene permiso para ver este proyecto
            // Si Mercado_ID del usuario es 0, puede ver todos los proyectos
            // Si no, solo puede ver proyectos de su mercado
            if (mercadoUsuario != 0 && proyecto.Mercado_ID != mercadoUsuario)
            {
                // Si el usuario no tiene permiso, redirigir a una página de error
                return RedirectToAction("AccesoDenegado", "Error");
            }

            if (proyecto == null)
            {
                return NotFound();
            }

            foreach (var tramite in proyecto.Tramites)
            {
                tramite.Comentarios = (await repositorioProyEstrategicos.ObtenerComentariosPorTramiteId(tramite.IDTramite)).ToList();
            }

            ViewBag.VistaOrigen = vistaOrigen;

            return View(proyecto);
        }

        // Tramites
        [HttpGet]
        public async Task<IActionResult> AgregarTramite(int idProyecto)
        {
            var proyecto = await repositorioProyEstrategicos.ObtenerProyectoPorId(idProyecto);
            if (proyecto == null)
            {
                return NotFound();
            }

            ViewBag.IDProyecto = idProyecto;
            ViewBag.NombreProyecto = proyecto.NombreProyecto; // Pasar el nombre del proyecto a la vista
            return View();
        }

        [HttpPost]
        public async Task<IActionResult> AgregarTramite(TramiteProyectoEstrategico agregartramite)
        {
            if (ModelState.IsValid)
            {
                // Validar FechaIngreso
                if (agregartramite.FechaIngreso == DateTime.MinValue || agregartramite.FechaIngreso < new DateTime(1753, 1, 1))
                {
                    agregartramite.FechaIngreso = DateTime.Now; // Establecer una fecha válida
                }

                await repositorioProyEstrategicos.AgregarTramite(agregartramite);
                return RedirectToAction("Detalle", new { id = agregartramite.IDProyecto });
            }

            ViewBag.IDProyecto = agregartramite.IDProyecto;
            ViewBag.Errores = ModelState.Values.SelectMany(v => v.Errors).Select(e => e.ErrorMessage).ToList();
            return View(agregartramite);
        }

        [HttpGet]
        public async Task<IActionResult> EditarTramite(int id)
        {
            var perfilUsuario = JsonConvert.DeserializeObject<PerfilUsuario>(HttpContext.Session.GetString("PerfilUsuario"));

            // Convertir Mercado_ID a int
            int mercadoUsuario = int.Parse(perfilUsuario.Mercado_ID);

            // Obtener el trámite por ID
            var tramite = await repositorioProyEstrategicos.ObtenerTramitePorId(id);
            if (tramite == null)
            {
                return NotFound();
            }

            // Obtener el proyecto relacionado para verificar el mercado
            var proyecto = await repositorioProyEstrategicos.ObtenerProyectoPorId(tramite.IDProyecto);

            // Verificar si el usuario tiene permiso para ver este trámite
            if (perfilUsuario.Mercado_ID != "0" && proyecto.Mercado_ID != mercadoUsuario)
            {
                // Si el usuario no tiene permiso, redirigir a una página de error
                return RedirectToAction("AccesoDenegado", "Error");
            }

            tramite.NombreProyecto = proyecto.NombreProyecto;
            // Obtener los comentarios asociados con el trámite y convertir a List
            tramite.Comentarios = (await repositorioProyEstrategicos.ObtenerComentariosPorTramiteId(id)).ToList();

            return View(tramite);
        }

        [HttpPost]
        public async Task<IActionResult> EditarTramite(TramiteProyectoEstrategico tramiteEditado)
        {
            var perfilUsuario = JsonConvert.DeserializeObject<PerfilUsuario>(HttpContext.Session.GetString("PerfilUsuario"));

            // Convertir Mercado_ID a int
            int mercadoUsuario = int.Parse(perfilUsuario.Mercado_ID);

            // Obtener el proyecto relacionado para verificar el mercado
            var proyecto = await repositorioProyEstrategicos.ObtenerProyectoPorId(tramiteEditado.IDProyecto);

            // Verificar si el usuario tiene permiso para editar este trámite
            if (perfilUsuario.Mercado_ID != "0" && proyecto.Mercado_ID != mercadoUsuario)
            {
                // Si el usuario no tiene permiso, redirigir a una página de error
                return RedirectToAction("AccesoDenegado", "Error");
            }

            if (ModelState.IsValid)
            {
                await repositorioProyEstrategicos.ActualizarTramite(tramiteEditado);
                return RedirectToAction("EditarTramite", new { id = tramiteEditado.IDTramite }); // Redirigir de nuevo a la vista de edición para recargar los comentarios
            }

            tramiteEditado.Comentarios = (await repositorioProyEstrategicos.ObtenerComentariosPorTramiteId(tramiteEditado.IDTramite)).ToList(); // Recargar los comentarios si la validación falla
            return View(tramiteEditado);
        }



        [HttpPost]
        public async Task<IActionResult> AgregarComentario(ComentarioProyectoEstrategico comentario, string NombreUsuario)
        {
            if (!ModelState.IsValid)
            {
                var tramite = await repositorioProyEstrategicos.ObtenerTramitePorId(comentario.IDTramite);
                tramite.Comentarios = (await repositorioProyEstrategicos.ObtenerComentariosPorTramiteId(comentario.IDTramite)).ToList();

                var referer = Request.Headers["Referer"].ToString();
                return Redirect(referer);
            }

            comentario.FechaComentario = DateTime.Now;
            comentario.NombreUsuario = NombreUsuario;
            await repositorioProyEstrategicos.AgregarComentario(comentario);

            var refererUrl = Request.Headers["Referer"].ToString();
            return Redirect(refererUrl);
        }



    }
}
