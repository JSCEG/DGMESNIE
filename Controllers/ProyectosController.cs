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
using System.Text.RegularExpressions;
using iText.Kernel.Pdf;
using iText.Layout;
using iText.Layout.Element;
using iText.Layout.Properties;
using iText.Layout.Borders;
using iText.Kernel.Colors;
using iText.Kernel.Font;
using iText.IO.Font.Constants;


namespace NSIE.Controllers
{
    [ServiceFilter(typeof(ValidacionInputFiltro))]
    [AutorizacionFiltro]
    public class ProyectosController : Controller
    {
        private readonly IRepositorioProyEstrategicos repositorioProyEstrategicos;


        public ProyectosController(IRepositorioProyEstrategicos repositorioProyEstrategicos)
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

        public IActionResult FondoPetroleo()
        {
            // Checamos si el archivo ya fue subido
            var ruta = Path.Combine(Directory.GetCurrentDirectory(), "wwwroot", "documentos", "necesidades", "Listado_Necesidades.pdf");
            bool listadoPublicado = System.IO.File.Exists(ruta);

            //Secretario administrativo
            string rutaBase = Path.Combine(Directory.GetCurrentDirectory(), "wwwroot", "documentos", "revision_secretario");

            var propuestas = new List<dynamic>();

            var propuestasEvaluacion = new List<dynamic>();

            if (Directory.Exists(rutaBase))
            {
                var carpetas = Directory.GetDirectories(rutaBase);

                foreach (var carpeta in carpetas)
                {
                    var nombreCarpeta = Path.GetFileName(carpeta);
                    var nombreProyecto = nombreCarpeta.Split("_")[0]; // Puedes ajustar esto según convención

                    var tieneDemanda = System.IO.File.Exists(Path.Combine(carpeta, "Demanda_Especifica.pdf"));
                    var tieneCuestionario = System.IO.File.Exists(Path.Combine(carpeta, "Cuestionario.pdf"));
                    var tienePropuesta = System.IO.File.Exists(Path.Combine(carpeta, "Propuesta_Tecnica.pdf"));
                    var tieneEvaluacion = System.IO.File.Exists(Path.Combine(carpeta, "Cuestionario_Evaluado.pdf"));
                    var tieneFormatoEvaluacion = System.IO.File.Exists(Path.Combine(carpeta, "Formato_Evaluacion.pdf"));
                    var rutaEvaluacion = Path.Combine(carpeta, "Evaluacion.txt");
                    bool esViable = false;
                    if (System.IO.File.Exists(rutaEvaluacion))
                    {
                        var contenido = System.IO.File.ReadAllText(rutaEvaluacion);
                        esViable = contenido.Contains("VIABLE");
                    }
                    bool fueEnviado = System.IO.File.Exists(Path.Combine(carpeta, "EnviadoAFinanciamiento.txt"));
                    var aprobado = System.IO.File.Exists(Path.Combine(carpeta, "Oficio_Aprobacion.pdf"));
                    var observaciones = Path.Combine(carpeta, "Observaciones.txt");
                    var observacionesArchivo = Path.Combine(carpeta, "Observaciones_Adjunto.pdf");
                    var notificadoSujeto = Path.Combine(carpeta, "NotificadoASujeto.txt");
                    var tieneCorreccion = Path.Combine(carpeta, "Correccion_Propuesta.pdf");
                    var rutaCorreccionEnviada = Path.Combine(carpeta, "Correccion_Enviada.txt");
                    var correccionEnviada = System.IO.File.Exists(rutaCorreccionEnviada);

                    var convocatoriaAutorizada = System.IO.File.Exists(Path.Combine(carpeta, "Convocatoria_Autorizada.txt"));
                    var convocatoriaNotificada = System.IO.File.Exists(Path.Combine(carpeta, "NotificadoConvocatoria.txt"));
                    var propuestaOficioEnviada = System.IO.File.Exists(Path.Combine(carpeta, "Propuesta_Oficio.pdf"))
                              && System.IO.File.Exists(Path.Combine(carpeta, "Propuesta_Detalles.pdf"));
                    var formatoEvaluacionSubido = System.IO.File.Exists(Path.Combine(carpeta, "Formato_Evaluacion.pdf"));
                    var enviadoComite = System.IO.File.Exists(Path.Combine(carpeta, "EnviadoComite.txt"));
                    var susceptiblePresupuesto = System.IO.File.Exists(Path.Combine(carpeta, "SusceptiblePresupuesto.txt"));
                    var rutaEvaluacionAbierta = Path.Combine(carpeta, "Viable.txt");
                    var esViableAbierta = false;
                    if (System.IO.File.Exists(rutaEvaluacionAbierta))
                    {
                        var contenido = System.IO.File.ReadAllText(rutaEvaluacionAbierta);
                        esViable = contenido.Contains("VIABLE");
                    }
                    var documentoConvenioSubido = System.IO.File.Exists(Path.Combine(carpeta, "Documentacion_Convenio.pdf"));
                    var oficioSolicitudCuentaSubido = System.IO.File.Exists(Path.Combine(carpeta, "Oficio_Solicitud_Cuenta.pdf"));
                    var documentosCuentaBancariaSubidos = System.IO.File.Exists(Path.Combine(carpeta, "Documentos_CuentaBancaria.pdf"));
                    var transferenciaConfirmada = System.IO.File.Exists(Path.Combine(carpeta, "Transferencia_Confirmada.txt"));
                    var tieneEvaluacionSustentabilidad = System.IO.File.Exists(Path.Combine(carpeta, "Formato_Evaluacion_Sustentabilidad.pdf"));

                    if (tieneDemanda && tieneCuestionario)
                    {
                        propuestas.Add(new
                        {
                            Nombre = nombreProyecto,
                            Carpeta = nombreCarpeta,
                            Tipo = tienePropuesta ? "integrada" : "abierta",
                            Evaluado = tieneEvaluacion,
                            TienePropuesta = tienePropuesta,
                            FormatoListo = tieneFormatoEvaluacion,
                            EsViable = esViable,
                            FueEnviado = fueEnviado,
                            Aprobado = aprobado,
                            Observaciones = System.IO.File.Exists(observaciones) ? System.IO.File.ReadAllText(observaciones) : null,
                            ObservacionesArchivo = System.IO.File.Exists(observacionesArchivo),
                            FueNotificadoSujeto = System.IO.File.Exists(notificadoSujeto),
                            TieneCorreccion = System.IO.File.Exists(tieneCorreccion),
                            CorreccionEnviada = correccionEnviada,

                            ConvocatoriaAutorizada = convocatoriaAutorizada,
                            ConvocatoriaNotificada = convocatoriaNotificada,
                            PropuestaOficioEnviada = propuestaOficioEnviada,
                            FormatoEvaluacionSubido = formatoEvaluacionSubido,
                            EnviadoComite = enviadoComite,
                            SusceptiblePresupuesto = susceptiblePresupuesto,

                            DocumentoConvenioSubido = documentoConvenioSubido,
                            OficioSolicitudCuentaSubido = oficioSolicitudCuentaSubido,
                            DocumentosCuentaBancariaSubidos = documentosCuentaBancariaSubidos,
                            TransferenciaConfirmada = transferenciaConfirmada,

                            TieneEvaluacionSustentabilidad = tieneEvaluacionSustentabilidad
                        });
                    }
                }

                foreach (var carpeta in carpetas)
                {
                    var nombreCarpeta = Path.GetFileName(carpeta);
                    var nombreProyecto = nombreCarpeta.Split("_")[0];

                    var demanda = System.IO.File.Exists(Path.Combine(carpeta, "Demanda_Especifica.pdf"));
                    var propuesta = System.IO.File.Exists(Path.Combine(carpeta, "Propuesta_Tecnica.pdf"));
                    var formato = System.IO.File.Exists(Path.Combine(carpeta, "Formato_Evaluacion.pdf"));
                    var rutaEvaluacion = Path.Combine(carpeta, "Evaluacion.txt");
                    string estadoEvaluacion = null;
                    bool esViable = false;

                    var tieneEvaluacionSustentabilidad = System.IO.File.Exists(Path.Combine(carpeta, "Formato_Evaluacion_Sustentabilidad.pdf"));

                    string estadoEvaluacionAbierta = null;
                    if (System.IO.File.Exists(Path.Combine(carpeta, "Viable.txt")))
                    {
                        estadoEvaluacionAbierta = "VIABLE";
                    }
                    else if (System.IO.File.Exists(Path.Combine(carpeta, "NoViable.txt")))
                    {
                        estadoEvaluacionAbierta = "NO VIABLE";
                    }

                    if (System.IO.File.Exists(rutaEvaluacion))
                    {
                        estadoEvaluacion = System.IO.File.ReadAllText(rutaEvaluacion);
                    }

                    if (formato && demanda) // mínimo requerido
                    {
                        propuestasEvaluacion.Add(new
                        {
                            Nombre = nombreProyecto,
                            Carpeta = nombreCarpeta,
                            TienePropuesta = propuesta,
                            EstadoEvaluacion = estadoEvaluacion,

                            EstadoEvaluacionAbierta = estadoEvaluacionAbierta,

                            TieneEvaluacionSustentabilidad = tieneEvaluacionSustentabilidad
                        });
                    }
                }
            }

            ViewData["PropuestasComite"] = propuestas;
            ViewData["PropuestasEvaluacion"] = propuestasEvaluacion;

            // Pasamos datos a la vista (ViewData o ViewBag)
            ViewData["ListadoPublicado"] = listadoPublicado;
            ViewData["UrlListado"] = "/documentos/necesidades/Listado_Necesidades.pdf";

            //PROYECTOS
            ViewData["ProyectosAprobados"] = propuestas
                .Where(p => p.TransferenciaConfirmada)
                .Select(p =>
                {
                    var carpetaProyecto = Path.Combine(rutaBase, p.Carpeta);

                    var transferenciaPath = Path.Combine(carpetaProyecto, "Transferencia_Confirmada.txt");
                    var inicioDate = System.IO.File.Exists(transferenciaPath)
                        ? System.IO.File.GetCreationTime(transferenciaPath)
                        : DateTime.Today;

                    var finDate = DateTime.Today.AddMonths(2);

                    // Cadenas para la vista
                    var inicio = inicioDate.ToString("yyyy-MM-dd");
                    var fin = finDate.ToString("yyyy-MM-dd");

                    var informeOficioGuardadoPath = Path.Combine(carpetaProyecto, "Informe_Oficio_Guardado.txt");
                    var informeOficioGuardado = System.IO.File.Exists(informeOficioGuardadoPath);

                    var fechaLimitePath = Path.Combine(carpetaProyecto, "Fecha_Limite.txt");
                    string fechaLimite = null;
                    string fechaLimiteStr = null;
                    if (System.IO.File.Exists(fechaLimitePath))
                    {
                        var contenido = System.IO.File.ReadAllText(fechaLimitePath);
                        Console.WriteLine("Contenido crudo: [" + contenido + "]");
                        fechaLimite = contenido.Replace("Limite el", "").Trim();
                        Console.WriteLine("Valor final: [" + fechaLimite + "]");
                        // Puedes parsear el contenido del archivo en vez de solo tomar la fecha de creación
                        fechaLimite = System.IO.File.ReadAllText(fechaLimitePath)
                            .Replace("Limite el", "", StringComparison.OrdinalIgnoreCase)
                            .Trim();
                        Console.WriteLine("Valor: " + fechaLimite);
                        if (DateTime.TryParse(fechaLimite, out var fechaParsed))
                        {
                            // 👇 Lo convertimos a ISO 8601 para JS
                            fechaLimiteStr = fechaParsed.ToString("yyyy-MM-ddTHH:mm:ss");
                        }
                        Console.WriteLine("Valor Final: " + fechaLimiteStr);
                    }

                    var fechaReunionPath = Path.Combine(carpetaProyecto, "Fecha_Reunion.txt");
                    string fechaReunion = null;
                    string fechaReunionStr = null;
                    string requiereDocumento = null;
                    if (System.IO.File.Exists(fechaReunionPath))
                    {
                        var contenido = System.IO.File.ReadAllText(fechaReunionPath).Trim();
                        Console.WriteLine("Contenido crudo: [" + contenido + "]");

                        // Ejemplo esperado: "2025-08-18T14:30:00|SI"
                        var partes = contenido.Split('|');
                        if (partes.Length >= 1)
                        {
                            var textoFecha = partes[0].Trim();
                            // Buscar la fecha en el texto
                            var match = Regex.Match(textoFecha, @"\d{2}/\d{2}/\d{4}\s+\d{2}:\d{2}");
                            if (match.Success)
                            {
                                fechaReunion = match.Value; // "27/08/2025 17:21"
                                if (DateTime.TryParse(fechaReunion, out var fechaParsed))
                                {
                                    fechaReunionStr = fechaParsed.ToString("yyyy-MM-ddTHH:mm:ss");
                                }
                            }
                        }

                        if (partes.Length >= 2)
                        {
                            requiereDocumento = partes[1].Replace("Requiere documento:", "").Trim();
                        }

                        Console.WriteLine("Fecha reunión: " + fechaReunionStr);
                        Console.WriteLine("¿Requiere documento?: " + requiereDocumento);
                    }

                    var informeFinalGuardadoPath = Path.Combine(carpetaProyecto, "Informe_Final_Guardado.txt");
                    var informeFinalGuardado = System.IO.File.Exists(informeFinalGuardadoPath);

                    var finalProyectoPath = Path.Combine(carpetaProyecto, "Final_Proyecto.txt");
                    var finalProyecto = System.IO.File.Exists(finalProyectoPath);

                    var finiquitoPath = Path.Combine(carpetaProyecto, "Finiquito.txt");
                    var finiquito = System.IO.File.Exists(finiquitoPath);

                    // 👇 lógica de progreso
                    int progreso = 0;
                    bool habilitarInformeFinal = false;
                    if (informeOficioGuardado)
                    {
                        progreso = 25;

                        // Solo si el fin es posterior al inicio calculamos la mitad
                        if (finDate > inicioDate)
                        {
                            var fechaMitad = inicioDate.AddDays((finDate - inicioDate).TotalDays / 2.0);
                            Console.WriteLine("Fecha mitad: " + fechaMitad);
                            if (DateTime.Now >= fechaMitad)
                            {
                                progreso = 50;
                            }
                            var diasRestantesProyecto = (finDate - DateTime.Today).TotalDays;
                            Console.WriteLine("Dias restantes: " + diasRestantesProyecto);
                            if (diasRestantesProyecto <= 14 && diasRestantesProyecto >= 0)
                            {
                                habilitarInformeFinal = true;
                                if (informeFinalGuardado)
                                {
                                    progreso = 75;
                                    if (finiquito)
                                    {
                                        progreso = 100;
                                    }
                                }
                            }
                        }
                    }


                    return new
                    {
                        p.Nombre,
                        Inicio = inicio,
                        Fin = DateTime.Now.AddMonths(2).ToString("yyyy-MM-dd"),
                        Progreso = progreso,
                        Estado = progreso >= 100 ? "Concluido" : "En curso",
                        p.Carpeta,
                        InformeOficioGuardado = informeOficioGuardado,
                        FechaEntregaInformes = informeOficioGuardado
                            ? System.IO.File.GetCreationTime(informeOficioGuardadoPath).ToString("yyyy-MM-dd")
                            : null,
                        FechaLimite = fechaLimiteStr,
                        FechaReunion = fechaReunionStr,
                        RequiereDocumento = requiereDocumento,
                        HabilitarInformeFinal = habilitarInformeFinal,
                        InformeFinalGuardado = informeFinalGuardado,
                        FechaEntregaFinal = informeFinalGuardado
                            ? System.IO.File.GetCreationTime(informeOficioGuardadoPath).ToString("yyyy-MM-dd")
                            : null,
                        FinalProyecto = finalProyecto,
                        Finiquito = finiquito
                    };
                })
                .ToList();

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

        public IActionResult Dashboard()
        {
            var estadisticas = new ProyectoFOTEASE.EstadisticasProyectos
            {
                ProyectosTerminados = 180,
                ProyectosEnCurso = 90,
                ProyectosCancelados = 30
            };

            var proyectosGantt = new List<ProyectoFOTEASE.ProyectoGantt>
            {
                new ProyectoFOTEASE.ProyectoGantt
                {
                    Id = "P1",
                    Nombre = "Diseño del Sistema",
                    Inicio = new DateTime(2025, 5, 1),
                    Fin = new DateTime(2025, 5, 10),
                    Progreso = 1.0
                },
                new ProyectoFOTEASE.ProyectoGantt
                {
                    Id = "P2",
                    Nombre = "Desarrollo Backend",
                    Dependencias = new[] { "P1" },
                    Inicio = new DateTime(2025, 5, 11),
                    Fin = new DateTime(2025, 5, 25),
                    Progreso = 0.6
                },
                new ProyectoFOTEASE.ProyectoGantt
                {
                    Id = "P3",
                    Nombre = "Integración Frontend",
                    Dependencias = new[] { "P2" },
                    Inicio = new DateTime(2025, 5, 26),
                    Fin = new DateTime(2025, 6, 5),
                    Progreso = 0.3
                },
                new ProyectoFOTEASE.ProyectoGantt
                {
                    Id = "P4",
                    Nombre = "Pruebas y QA",
                    Dependencias = new[] { "P3" },
                    Inicio = new DateTime(2025, 6, 6),
                    Fin = new DateTime(2025, 6, 15),
                    Progreso = 0.0
                },
                new ProyectoFOTEASE.ProyectoGantt
                {
                    Id = "P5",
                    Nombre = "Implementación",
                    Dependencias = new[] { "P4" },
                    Inicio = new DateTime(2025, 6, 16),
                    Fin = new DateTime(2025, 6, 25),
                    Progreso = 0.0
                }
            };

            var perfilUsuarioJson = HttpContext.Session.GetString("PerfilUsuario");
            var perfilUsuario = JsonConvert.DeserializeObject<PerfilUsuario>(perfilUsuarioJson);

            int idUsuarioParsed = 0;
            int.TryParse(perfilUsuario.IdUsuario, out idUsuarioParsed);

            var model = new DashboardViewModel
            {
                Estadisticas = estadisticas,
                ProyectosGantt = proyectosGantt,
                NombreUsuario = perfilUsuario.Nombre,
                RolUsuario = perfilUsuario.Rol,
                IdUsuario = idUsuarioParsed
            };

            return View(model);
        }


        // IMP

        [HttpPost]
        public IActionResult SubirListado(IFormFile ArchivoListado)
        {
            if (ArchivoListado != null && ArchivoListado.Length > 0)
            {
                var destino = Path.Combine(Directory.GetCurrentDirectory(), "wwwroot", "documentos", "necesidades");
                if (!Directory.Exists(destino))
                    Directory.CreateDirectory(destino);

                var rutaCompleta = Path.Combine(destino, "Listado_Necesidades.pdf");

                using (var stream = new FileStream(rutaCompleta, FileMode.Create))
                {
                    ArchivoListado.CopyTo(stream);
                }
            }

            return RedirectToAction("FondoPetroleo"); // O la vista que corresponda
        }

        [HttpPost]
        public IActionResult EliminarListado()
        {
            var ruta = Path.Combine(Directory.GetCurrentDirectory(), "wwwroot", "documentos", "necesidades", "Listado_Necesidades.pdf");

            if (System.IO.File.Exists(ruta))
            {
                System.IO.File.Delete(ruta);
            }

            return RedirectToAction("FondoPetroleo"); // Reemplaza con tu vista real
        }

        [HttpPost]
        public async Task<IActionResult> EnviarPropuesta(
            IFormFile anexo1_pdf,
            IFormFile anexo1_editable,
            IFormFile anexo2_pdf,
            IFormFile anexo2_editable,
            string nombreProyecto,
            string nombreSolicitante,
            string institucion,
            string responsable,
            string correo,
            string tipoDemanda,
            string descripcion
        )
        {
            // Creamos carpeta base para propuestas
            var baseRuta = Path.Combine(Directory.GetCurrentDirectory(), "wwwroot", "documentos", "propuestas");

            // Puedes usar timestamp o nombre del proyecto para subcarpeta
            var nombreCarpeta = $"{nombreProyecto}_{DateTime.Now:yyyyMMddHHmmss}";
            var rutaDestino = Path.Combine(baseRuta, nombreCarpeta);

            if (!Directory.Exists(rutaDestino))
                Directory.CreateDirectory(rutaDestino);

            // Guardamos los archivos
            async Task GuardarArchivo(IFormFile archivo, string nombre)
            {
                if (archivo != null && archivo.Length > 0)
                {
                    var path = Path.Combine(rutaDestino, nombre);
                    using (var stream = new FileStream(path, FileMode.Create))
                    {
                        await archivo.CopyToAsync(stream);
                    }
                }
            }

            await GuardarArchivo(anexo1_pdf, "Anexo1_PDF.pdf");
            await GuardarArchivo(anexo1_editable, "Anexo1_Editable.docx");
            await GuardarArchivo(anexo2_pdf, "Anexo2_PDF.pdf");
            await GuardarArchivo(anexo2_editable, "Anexo2_Editable.docx");

            // Aquí puedes guardar los metadatos en DB o log si lo deseas
            HttpContext.Session.SetString("PropuestaEnviada", "true");
            TempData["MensajeExito"] = "✅ Propuesta enviada correctamente.";
            return RedirectToAction("FondoPetroleo"); // O redirige a donde muestres feedback
        }

        [HttpPost]
        public async Task<IActionResult> EnviarSecretario(
            IFormFile archivoDemanda,
            IFormFile archivoCuestionario,
            IFormFile archivoPropuesta,
            string nombreProyecto,
            string tipoDemanda
        )
        {
            var baseRuta = Path.Combine(Directory.GetCurrentDirectory(), "wwwroot", "documentos", "revision_secretario");
            var nombreCarpeta = $"{nombreProyecto}_{DateTime.Now:yyyyMMddHHmmss}";
            var rutaDestino = Path.Combine(baseRuta, nombreCarpeta);

            if (!Directory.Exists(rutaDestino))
                Directory.CreateDirectory(rutaDestino);

            async Task GuardarArchivo(IFormFile archivo, string nombre)
            {
                if (archivo != null && archivo.Length > 0)
                {
                    var path = Path.Combine(rutaDestino, nombre);
                    using (var stream = new FileStream(path, FileMode.Create))
                    {
                        await archivo.CopyToAsync(stream);
                    }
                }
            }

            await GuardarArchivo(archivoDemanda, "Demanda_Especifica.pdf");
            await GuardarArchivo(archivoCuestionario, "Cuestionario.pdf");

            if (tipoDemanda == "integrada")
                await GuardarArchivo(archivoPropuesta, "Propuesta_Tecnica.pdf");

            TempData["MensajeExito"] = $"✅ Propuesta ({tipoDemanda}) enviada al comité correctamente.";
            return RedirectToAction("FondoPetroleo");
        }

        [HttpPost]
        public async Task<IActionResult> EnviarEvaluacion(IFormFile cuestionarioEvaluado, string carpetaProyecto)
        {
            if (string.IsNullOrEmpty(carpetaProyecto))
            {
                TempData["MensajeError"] = "Error: No se especificó la carpeta del proyecto.";
                return RedirectToAction("FondoPetroleo");
            }

            var rutaDestino = Path.Combine(Directory.GetCurrentDirectory(), "wwwroot", "documentos", "revision_secretario", carpetaProyecto);

            if (!Directory.Exists(rutaDestino))
            {
                TempData["MensajeError"] = "Error: No se encontró el proyecto especificado.";
                return RedirectToAction("FondoPetroleo");
            }

            if (cuestionarioEvaluado != null && cuestionarioEvaluado.Length > 0)
            {
                var rutaArchivo = Path.Combine(rutaDestino, "Cuestionario_Evaluado.pdf");
                using (var stream = new FileStream(rutaArchivo, FileMode.Create))
                {
                    await cuestionarioEvaluado.CopyToAsync(stream);
                }

                TempData["MensajeExito"] = "✅ Cuestionario evaluado enviado correctamente.";
            }
            else
            {
                TempData["MensajeError"] = "⚠️ Debes seleccionar un archivo PDF válido.";
            }

            return RedirectToAction("FondoPetroleo");
        }

        [HttpPost]
        public async Task<IActionResult> SubirFormatoEvaluacion(IFormFile formatoEvaluacion, string carpetaProyecto)
        {
            if (string.IsNullOrEmpty(carpetaProyecto))
            {
                TempData["MensajeError"] = "Error: Proyecto no especificado.";
                return RedirectToAction("FondoPetroleo");
            }

            var ruta = Path.Combine(Directory.GetCurrentDirectory(), "wwwroot", "documentos", "revision_secretario", carpetaProyecto);

            if (!Directory.Exists(ruta))
            {
                TempData["MensajeError"] = "Error: Carpeta del proyecto no encontrada.";
                return RedirectToAction("FondoPetroleo");
            }

            if (formatoEvaluacion != null && formatoEvaluacion.Length > 0)
            {
                var destino = Path.Combine(ruta, "Formato_Evaluacion.pdf");
                using (var stream = new FileStream(destino, FileMode.Create))
                {
                    await formatoEvaluacion.CopyToAsync(stream);
                }

                TempData["MensajeExito"] = "✅ Formato de evaluación enviado correctamente.";
            }
            else
            {
                TempData["MensajeError"] = "⚠️ Selecciona un archivo válido.";
            }

            return RedirectToAction("FondoPetroleo");
        }

        [HttpPost]
        public IActionResult EvaluarViabilidad(string carpeta, string decision)
        {
            if (string.IsNullOrWhiteSpace(carpeta) || string.IsNullOrWhiteSpace(decision))
            {
                TempData["MensajeError"] = "⚠️ Parámetros inválidos.";
                return RedirectToAction("FondoPetroleo");
            }

            var ruta = Path.Combine(Directory.GetCurrentDirectory(), "wwwroot", "documentos", "revision_secretario", carpeta);

            if (!Directory.Exists(ruta))
            {
                TempData["MensajeError"] = "⚠️ Carpeta del proyecto no encontrada.";
                return RedirectToAction("FondoPetroleo");
            }

            var archivo = Path.Combine(ruta, "Evaluacion.txt");
            System.IO.File.WriteAllText(archivo, $"Evaluado: {decision.ToUpper()} - {DateTime.Now}");

            TempData["MensajeExito"] = decision == "viable"
                ? "✅ Proyecto marcado como viable."
                : "❌ Proyecto marcado como NO viable.";

            return RedirectToAction("FondoPetroleo");
        }

        [HttpPost]
        public IActionResult EnviarAFinanciamiento(string carpeta)
        {
            if (string.IsNullOrWhiteSpace(carpeta))
            {
                TempData["MensajeError"] = "Proyecto no válido.";
                return RedirectToAction("FondoPetroleo");
            }

            var rutaProyecto = Path.Combine(Directory.GetCurrentDirectory(), "wwwroot", "documentos", "revision_secretario", carpeta);
            if (!Directory.Exists(rutaProyecto))
            {
                TempData["MensajeError"] = "No se encontró el proyecto especificado.";
                return RedirectToAction("FondoPetroleo");
            }

            // Marcamos que ya fue enviado (creamos un archivo marcador)
            var marcador = Path.Combine(rutaProyecto, "EnviadoAFinanciamiento.txt");
            System.IO.File.WriteAllText(marcador, $"Enviado al comité - {DateTime.Now}");

            TempData["MensajeExito"] = "✅ Proyecto enviado correctamente al Comité de Decisión para aprobación presupuestal.";
            return RedirectToAction("FondoPetroleo");
        }

        [HttpPost]
        public async Task<IActionResult> AprobarProyecto(string carpeta, IFormFile oficio)
        {
            if (string.IsNullOrWhiteSpace(carpeta) || oficio == null)
            {
                TempData["MensajeError"] = "❌ Datos inválidos. Selecciona un archivo y vuelve a intentarlo.";
                return RedirectToAction("FondoPetroleo");
            }

            if (Path.GetExtension(oficio.FileName).ToLower() != ".pdf")
            {
                TempData["MensajeError"] = "❌ El archivo debe estar en formato PDF.";
                return RedirectToAction("FondoPetroleo");
            }

            try
            {
                var ruta = Path.Combine(Directory.GetCurrentDirectory(), "wwwroot", "documentos", "revision_secretario", carpeta);
                if (!Directory.Exists(ruta)) Directory.CreateDirectory(ruta);

                var pathOficio = Path.Combine(ruta, "Oficio_Aprobacion.pdf");

                // Guardar el PDF
                using (var stream = new FileStream(pathOficio, FileMode.Create))
                {
                    await oficio.CopyToAsync(stream);
                }

                // Crear o actualizar el archivo de aprobación
                var aprobadoPath = Path.Combine(ruta, "Aprobado.txt");
                System.IO.File.WriteAllText(aprobadoPath, DateTime.Now.ToString("yyyy-MM-dd HH:mm:ss"));

                TempData["MensajeExito"] = "✅ Propuesta aprobada correctamente y oficio guardado.";
            }
            catch (Exception ex)
            {
                TempData["MensajeError"] = $"❌ Ocurrió un error al subir el oficio: {ex.Message}";
            }

            return RedirectToAction("FondoPetroleo");
        }

        [HttpPost]
        public async Task<IActionResult> EnviarObservacion(string carpeta, string observaciones, IFormFile oficio)
        {
            var rutaCarpeta = Path.Combine(Directory.GetCurrentDirectory(), "wwwroot", "documentos", "revision_secretario", carpeta);

            if (!Directory.Exists(rutaCarpeta))
                return NotFound("Carpeta no encontrada");

            string Archivo(string nombre) => Path.Combine(rutaCarpeta, nombre);

            // Eliminar archivos antiguos
            foreach (var archivo in new[] {
                Archivo("Observaciones.txt"),
                Archivo("Oficio_Observacion.pdf"),
                Archivo("Correccion_Propuesta.pdf"),
                Archivo("CorreccionEnviada.txt")
            })
            {
                if (System.IO.File.Exists(archivo))
                    System.IO.File.Delete(archivo);
            }

            if (string.IsNullOrWhiteSpace(observaciones) && (oficio == null || oficio.Length == 0))
            {
                TempData["MensajeError"] = "Debes escribir observaciones o adjuntar un oficio.";
                return RedirectToAction("FondoPetroleo");
            }

            if (!string.IsNullOrWhiteSpace(observaciones))
                await System.IO.File.WriteAllTextAsync(Archivo("Observaciones.txt"), observaciones);

            if (oficio != null && oficio.Length > 0)
            {
                var rutaArchivo = Archivo("Observaciones_Adjunto.pdf");
                using var stream = new FileStream(rutaArchivo, FileMode.Create);
                await oficio.CopyToAsync(stream);
            }

            TempData["MensajeExito"] = "🔴 Observación enviada correctamente al Secretario.";
            return RedirectToAction("FondoPetroleo");
        }

        [HttpPost]
        public IActionResult NotificarSujetoObservaciones(string carpeta)
        {
            var ruta = Path.Combine(Directory.GetCurrentDirectory(), "wwwroot", "documentos", "revision_secretario", carpeta);

            if (!Directory.Exists(ruta))
            {
                TempData["MensajeError"] = "❌ Carpeta no encontrada.";
                return RedirectToAction("FondoPetroleo");
            }

            // Crear archivo para marcar que fue notificado
            var rutaNotificado = Path.Combine(ruta, "NotificadoASujeto.txt");
            System.IO.File.WriteAllText(rutaNotificado, $"Notificado el {DateTime.Now:dd/MM/yyyy HH:mm}");

            TempData["MensajeExito"] = "✅ Observaciones notificadas al Sujeto de Apoyo.";
            return RedirectToAction("FondoPetroleo");
        }
        [HttpPost]
        public async Task<IActionResult> SubirCorreccion(string carpeta, IFormFile archivoCorreccion)
        {
            if (string.IsNullOrEmpty(carpeta) || archivoCorreccion == null || archivoCorreccion.Length == 0)
            {
                TempData["MensajeError"] = "❌ Debes seleccionar un archivo válido.";
                return RedirectToAction("FondoPetroleo");
            }

            var rutaDestino = Path.Combine(Directory.GetCurrentDirectory(), "wwwroot", "documentos", "revision_secretario", carpeta);
            if (!Directory.Exists(rutaDestino))
            {
                TempData["MensajeError"] = "❌ No se encontró la propuesta especificada.";
                return RedirectToAction("FondoPetroleo");
            }

            var rutaArchivo = Path.Combine(rutaDestino, "Correccion_Propuesta.pdf");

            using (var stream = new FileStream(rutaArchivo, FileMode.Create))
            {
                await archivoCorreccion.CopyToAsync(stream);
            }

            TempData["MensajeExito"] = "✅ Corrección enviada correctamente.";
            return RedirectToAction("FondoPetroleo");
        }

        [HttpPost]
        public IActionResult ReenviarCorreccionComite(string carpeta)
        {
            if (string.IsNullOrEmpty(carpeta))
            {
                TempData["MensajeError"] = "❌ Carpeta no válida.";
                return RedirectToAction("FondoPetroleo");
            }

            var rutaProyecto = Path.Combine(Directory.GetCurrentDirectory(), "wwwroot", "documentos", "revision_secretario", carpeta);
            var rutaArchivo = Path.Combine(rutaProyecto, "Correccion_Propuesta.pdf");

            if (!System.IO.File.Exists(rutaArchivo))
            {
                TempData["MensajeError"] = "❌ No se encontró la corrección a reenviar.";
                return RedirectToAction("FondoPetroleo");
            }

            // Marcamos como enviada
            var rutaMarca = Path.Combine(rutaProyecto, "Correccion_Enviada.txt");
            System.IO.File.WriteAllText(rutaMarca, "Reenviado al Comité: " + DateTime.Now.ToString("g"));

            TempData["MensajeExito"] = "✅ Corrección enviada al Comité exitosamente.";
            return RedirectToAction("FondoPetroleo");
        }

        [HttpPost]
        public IActionResult AutorizarConvocatoria(string carpeta)
        {
            if (string.IsNullOrWhiteSpace(carpeta))
            {
                TempData["MensajeError"] = "⚠️ Proyecto no especificado.";
                return RedirectToAction("FondoPetroleo");
            }

            var rutaProyecto = Path.Combine(Directory.GetCurrentDirectory(), "wwwroot", "documentos", "revision_secretario", carpeta);

            if (!Directory.Exists(rutaProyecto))
            {
                TempData["MensajeError"] = "⚠️ No se encontró la carpeta del proyecto.";
                return RedirectToAction("FondoPetroleo");
            }

            var pathArchivo = Path.Combine(rutaProyecto, "Convocatoria_Autorizada.txt");

            System.IO.File.WriteAllText(pathArchivo, $"Autorizado el {DateTime.Now:dd/MM/yyyy HH:mm}");

            TempData["MensajeExito"] = "✅ Convocatoria autorizada correctamente.";
            return RedirectToAction("FondoPetroleo");
        }

        [HttpPost]
        public IActionResult NotificarSujetoConvocatoria(string carpeta)
        {
            if (string.IsNullOrWhiteSpace(carpeta))
            {
                TempData["MensajeError"] = "⚠️ Proyecto no especificado.";
                return RedirectToAction("FondoPetroleo");
            }

            var rutaProyecto = Path.Combine(Directory.GetCurrentDirectory(), "wwwroot", "documentos", "revision_secretario", carpeta);

            if (!Directory.Exists(rutaProyecto))
            {
                TempData["MensajeError"] = "⚠️ No se encontró la carpeta del proyecto.";
                return RedirectToAction("FondoPetroleo");
            }

            // Marcamos como notificado
            var archivo = Path.Combine(rutaProyecto, "NotificadoConvocatoria.txt");
            System.IO.File.WriteAllText(archivo, $"Notificado el {DateTime.Now:dd/MM/yyyy HH:mm}");

            TempData["MensajeExito"] = "📤 Convocatoria notificada al Sujeto de Apoyo.";
            return RedirectToAction("FondoPetroleo");
        }

        [HttpPost]
        public IActionResult SubirPropuestaOficio(string carpeta, IFormFile archivoOficio, IFormFile archivoPropuesta)
        {
            if (string.IsNullOrWhiteSpace(carpeta))
            {
                TempData["MensajeError"] = "⚠️ Proyecto no especificado.";
                return RedirectToAction("FondoPetroleo");
            }

            var rutaProyecto = Path.Combine(Directory.GetCurrentDirectory(), "wwwroot", "documentos", "revision_secretario", carpeta);

            if (!Directory.Exists(rutaProyecto))
            {
                TempData["MensajeError"] = "⚠️ No se encontró la carpeta del proyecto.";
                return RedirectToAction("FondoPetroleo");
            }

            if (archivoOficio == null || archivoPropuesta == null)
            {
                TempData["MensajeError"] = "⚠️ Debes subir ambos archivos.";
                return RedirectToAction("FondoPetroleo");
            }

            if (Path.GetExtension(archivoPropuesta.FileName).ToLower() != ".docx")
            {
                TempData["MensajeError"] = "⚠️ La propuesta debe estar en formato DOCX.";
                return RedirectToAction("FondoPetroleo");
            }

            var rutaOficio = Path.Combine(rutaProyecto, "Propuesta_Oficio.pdf");
            var rutaPropuesta = Path.Combine(rutaProyecto, "Propuesta_Detalles.pdf");

            using (var stream = new FileStream(rutaOficio, FileMode.Create))
            {
                archivoOficio.CopyTo(stream);
            }
            using (var stream = new FileStream(rutaPropuesta, FileMode.Create))
            {
                archivoPropuesta.CopyTo(stream);
            }

            // Opcional: marcar con un txt que ya se envió
            System.IO.File.WriteAllText(Path.Combine(rutaProyecto, "Propuesta_Enviada.txt"), $"Enviado el {DateTime.Now:dd/MM/yyyy HH:mm}");

            TempData["MensajeExito"] = "📤 Propuesta y oficio enviados correctamente.";
            return RedirectToAction("FondoPetroleo");
        }

        [HttpPost]
        public IActionResult SubirFormatoEvaluacionSujeto(string carpetaProyecto, IFormFile formatoEvaluacion)
        {
            if (string.IsNullOrWhiteSpace(carpetaProyecto) || formatoEvaluacion == null || formatoEvaluacion.Length == 0)
            {
                TempData["MensajeError"] = "Debes seleccionar un archivo válido.";
                return RedirectToAction("FondoPetroleo");
            }

            try
            {
                // Ruta base del proyecto
                var rutaCarpeta = Path.Combine(Directory.GetCurrentDirectory(), "wwwroot", "documentos", "revision_secretario", carpetaProyecto);

                if (!Directory.Exists(rutaCarpeta))
                {
                    Directory.CreateDirectory(rutaCarpeta);
                }

                // Guardar formato de evaluación
                var rutaFormato = Path.Combine(rutaCarpeta, "Formato_Evaluacion.pdf");
                using (var stream = new FileStream(rutaFormato, FileMode.Create))
                {
                    formatoEvaluacion.CopyTo(stream);
                }

                // Crear marcador de que ya se envió al Comité
                var rutaEnviadoTxt = Path.Combine(rutaCarpeta, "EnviadoComite.txt");
                System.IO.File.WriteAllText(rutaEnviadoTxt, $"Enviado automáticamente al Comité el {DateTime.Now:dd/MM/yyyy HH:mm}");

                TempData["MensajeExito"] = "Formato subido y propuesta enviada al Comité correctamente.";
            }
            catch (Exception ex)
            {
                TempData["MensajeError"] = $"Ocurrió un error: {ex.Message}";
            }

            return RedirectToAction("FondoPetroleo");
        }

        [HttpPost]
        public IActionResult EvaluarViabilidadAbierta(string carpeta, string decision)
        {
            if (string.IsNullOrEmpty(carpeta) || string.IsNullOrEmpty(decision))
            {
                TempData["MensajeError"] = "Datos inválidos para la evaluación.";
                return RedirectToAction("FondoPetroleo");
            }

            try
            {
                var rutaCarpeta = Path.Combine(Directory.GetCurrentDirectory(), "wwwroot", "documentos", "revision_secretario", carpeta);
                if (!Directory.Exists(rutaCarpeta))
                {
                    TempData["MensajeError"] = "No se encontró la carpeta del proyecto.";
                    return RedirectToAction("FondoPetroleo");
                }

                string archivoEstado;
                string textoEstado;

                if (decision == "viable")
                {
                    archivoEstado = "Viable.txt";
                    textoEstado = "PROYECTO VIABLE para financiamiento";
                }
                else
                {
                    archivoEstado = "NoViable.txt";
                    textoEstado = "PROYECTO NO VIABLE para financiamiento";
                }

                // Guardar resultado en un archivo
                var rutaEstado = Path.Combine(rutaCarpeta, archivoEstado);
                System.IO.File.WriteAllText(rutaEstado, $"{textoEstado} - {DateTime.Now:dd/MM/yyyy HH:mm}");

                TempData["MensajeExito"] = "Evaluación registrada correctamente.";
            }
            catch (Exception ex)
            {
                TempData["MensajeError"] = $"Error al registrar evaluación: {ex.Message}";
            }

            return RedirectToAction("FondoPetroleo");
        }

        [HttpPost]
        public IActionResult MarcarSusceptiblePresupuesto(string carpeta)
        {
            if (string.IsNullOrEmpty(carpeta))
                return BadRequest("Carpeta no especificada.");

            var rutaProyecto = Path.Combine(Directory.GetCurrentDirectory(), "wwwroot", "documentos", "revision_secretario", carpeta);

            if (!Directory.Exists(rutaProyecto))
                return NotFound("Carpeta no encontrada.");

            // Crea el archivo de confirmación
            var rutaArchivo = Path.Combine(rutaProyecto, "SusceptiblePresupuesto.txt");
            System.IO.File.WriteAllText(rutaArchivo, $"Marcado como susceptible a presupuesto el {DateTime.Now}");

            TempData["MensajeExito"] = "El proyecto fue marcado como susceptible a presupuesto.";
            return RedirectToAction("FondoPetroleo");
        }

        [HttpPost]
        public async Task<IActionResult> SubirDocumentacionConvenio(string carpeta, IFormFile archivoConvenio)
        {
            if (string.IsNullOrWhiteSpace(carpeta) || archivoConvenio == null)
            {
                TempData["MensajeError"] = "❌ Datos inválidos.";
                return RedirectToAction("FondoPetroleo");
            }

            if (Path.GetExtension(archivoConvenio.FileName).ToLower() != ".pdf")
            {
                TempData["MensajeError"] = "❌ El archivo debe estar en formato PDF.";
                return RedirectToAction("FondoPetroleo");
            }

            try
            {
                var ruta = Path.Combine(Directory.GetCurrentDirectory(), "wwwroot", "documentos", "revision_secretario", carpeta);
                if (!Directory.Exists(ruta)) Directory.CreateDirectory(ruta);

                var pathArchivo = Path.Combine(ruta, "Documentacion_Convenio.pdf");

                using (var stream = new FileStream(pathArchivo, FileMode.Create))
                {
                    await archivoConvenio.CopyToAsync(stream);
                }

                System.IO.File.WriteAllText(Path.Combine(ruta, "DocumentoConvenioSubido.txt"), DateTime.Now.ToString("yyyy-MM-dd HH:mm:ss"));

                TempData["MensajeExito"] = "✅ Documentación para convenio subida correctamente.";
            }
            catch (Exception ex)
            {
                TempData["MensajeError"] = $"❌ Error al subir el documento: {ex.Message}";
            }

            return RedirectToAction("FondoPetroleo");
        }

        [HttpPost]
        public async Task<IActionResult> SubirOficioSolicitudCuenta(string carpeta, IFormFile archivoOficio)
        {
            if (archivoOficio == null || archivoOficio.Length == 0)
            {
                TempData["MensajeError"] = "❌ Debe seleccionar un archivo PDF.";
                return RedirectToAction("FondoPetroleo");
            }

            var rutaProyecto = Path.Combine(Directory.GetCurrentDirectory(),
                                            "wwwroot", "documentos", "revision_secretario", carpeta);

            if (!Directory.Exists(rutaProyecto))
                Directory.CreateDirectory(rutaProyecto);

            var pathDestino = Path.Combine(rutaProyecto, "Oficio_Solicitud_Cuenta.pdf");

            using (var stream = new FileStream(pathDestino, FileMode.Create))
            {
                await archivoOficio.CopyToAsync(stream);
            }

            TempData["MensajeExito"] = "✅ Oficio de solicitud de cuenta bancaria subido correctamente.";
            return RedirectToAction("FondoPetroleo");
        }

        [HttpPost]
        public async Task<IActionResult> SubirDocumentosCuentaBancaria(string carpeta, IFormFile archivoCuenta)
        {
            if (archivoCuenta == null || archivoCuenta.Length == 0)
            {
                TempData["MensajeError"] = "⚠️ Debe seleccionar un archivo PDF.";
                return RedirectToAction("FondoPetroleo");
            }

            // Ruta destino
            var ruta = Path.Combine(Directory.GetCurrentDirectory(), "wwwroot", "documentos", "revision_secretario", carpeta);
            if (!Directory.Exists(ruta))
                Directory.CreateDirectory(ruta);

            var pathArchivo = Path.Combine(ruta, "Documentos_CuentaBancaria.pdf");

            // Guardar archivo
            using (var stream = new FileStream(pathArchivo, FileMode.Create))
            {
                await archivoCuenta.CopyToAsync(stream);
            }

            // Crear archivo indicador de que ya se subió
            System.IO.File.WriteAllText(Path.Combine(ruta, "CuentaBancaria_Subida.txt"), DateTime.Now.ToString());

            TempData["MensajeExito"] = "✅ Documentos de apertura de cuenta bancaria enviados correctamente.";
            return RedirectToAction("FondoPetroleo");
        }

        [HttpPost]
        public IActionResult ConfirmarTransferencia(string carpeta)
        {
            if (string.IsNullOrEmpty(carpeta))
            {
                TempData["MensajeError"] = "⚠️ Carpeta inválida.";
                return RedirectToAction("FondoPetroleo");
            }

            var ruta = Path.Combine(Directory.GetCurrentDirectory(), "wwwroot", "documentos", "revision_secretario", carpeta);
            if (!Directory.Exists(ruta))
            {
                TempData["MensajeError"] = "⚠️ Carpeta no encontrada.";
                return RedirectToAction("FondoPetroleo");
            }

            System.IO.File.WriteAllText(Path.Combine(ruta, "Transferencia_Confirmada.txt"), DateTime.Now.ToString());

            TempData["MensajeExito"] = "✅ Transferencia confirmada correctamente.";
            return RedirectToAction("FondoPetroleo");
        }

        [HttpPost]
        public IActionResult SubirInformes(string carpeta, IFormFile informeTecnico, IFormFile informeFinanciero, IFormFile Oficio)
        {
            if (string.IsNullOrEmpty(carpeta))
            {
                TempData["MensajeError"] = "⚠️ Carpeta inválida.";
                return RedirectToAction("FondoPetroleo");
            }

            var ruta = Path.Combine(Directory.GetCurrentDirectory(), "wwwroot", "documentos", "revision_secretario", carpeta);

            if (!Directory.Exists(ruta))
            {
                Directory.CreateDirectory(ruta); // Crea la carpeta si no existe
            }

            // Guarda un archivo de control para indicar que ya se guardaron informes
            var nombreArchivo = $"Informe_Oficio_Guardado.txt";
            System.IO.File.WriteAllText(Path.Combine(ruta, nombreArchivo),
                $"Informe guardado el {DateTime.Now}");

            // 2. Guardar informe si se subió
            if (informeTecnico != null && informeTecnico.Length > 0)
            {
                var informeTecnicoPath = Path.Combine(ruta, "Informe_Tecnico_Proyecto.pdf");
                using (var stream = new FileStream(informeTecnicoPath, FileMode.Create))
                {
                    informeTecnico.CopyTo(stream);
                }
            }

            if (informeFinanciero != null && informeFinanciero.Length > 0)
            {
                var informeFinancieroPath = Path.Combine(ruta, "Informe_Financiero_Proyecto.pdf");
                using (var stream = new FileStream(informeFinancieroPath, FileMode.Create))
                {
                    informeFinanciero.CopyTo(stream);
                }
            }

            // 3. Guardar oficio si se subió
            if (Oficio != null && Oficio.Length > 0)
            {
                var oficioPath = Path.Combine(ruta, "Oficio_Proyecto.pdf");
                using (var stream = new FileStream(oficioPath, FileMode.Create))
                {
                    Oficio.CopyTo(stream);
                }
            }

            TempData["MensajeExito"] = "✅ Informes guardados correctamente.";
            return RedirectToAction("FondoPetroleo");
        }

        [HttpPost]
        public IActionResult AsignarPlazo(string carpeta, DateTime? fechaLimite)
        {
            if (string.IsNullOrEmpty(carpeta))
            {
                return BadRequest("La carpeta es obligatoria.");
            }

            // Validar fecha (que no sea pasada)
            if (fechaLimite.HasValue && fechaLimite.Value.Date < DateTime.Today)
            {
                TempData["Error"] = "La fecha límite no puede ser anterior a hoy.";
                return RedirectToAction("FondoPetroleo"); // O a la vista que uses
            }

            var ruta = Path.Combine(Directory.GetCurrentDirectory(), "wwwroot", "documentos", "revision_secretario", carpeta);
            // Guarda un archivo de control para indicar que ya se guardaron informes
            var nombreArchivo = $"Fecha_Limite.txt";
            System.IO.File.WriteAllText(Path.Combine(ruta, nombreArchivo),
                $"Limite el {fechaLimite}");

            TempData["Success"] = fechaLimite.HasValue
                ? $"Se asignó la fecha límite {fechaLimite.Value:dd/MM/yyyy}"
                : "Se dejó sin límite de tiempo.";

            return RedirectToAction("FondoPetroleo"); // o la vista donde muestras la tabla
        }

        [HttpPost]
        public IActionResult SubirReuniones(string carpeta, DateTime? fechaReunion, string requiereDoc)
        {
            if (string.IsNullOrEmpty(carpeta))
            {
                return BadRequest("La carpeta es obligatoria.");
            }

            // Validar fecha (que no sea pasada)
            if (fechaReunion.HasValue && fechaReunion.Value.Date < DateTime.Today)
            {
                TempData["Error"] = "La fecha de la reunión no puede ser anterior a hoy.";
                return RedirectToAction("FondoPetroleo");
            }

            var ruta = Path.Combine(Directory.GetCurrentDirectory(), "wwwroot", "documentos", "revision_secretario", carpeta);

            if (!Directory.Exists(ruta))
            {
                Directory.CreateDirectory(ruta);
            }

            var nombreArchivo = $"Fecha_Reunion.txt";
            var contenido = $"Reunión programada el {fechaReunion:dd/MM/yyyy HH:mm} | Requiere documento: {requiereDoc}";
            System.IO.File.WriteAllText(Path.Combine(ruta, nombreArchivo), contenido);

            TempData["Success"] = fechaReunion.HasValue
                ? $"📅 Se asignó la reunión para el {fechaReunion.Value:dd/MM/yyyy HH:mm}. " +
                $"Requiere documento: {(requiereDoc == "si" ? "Sí" : "No")}."
                : "No se asignó ninguna reunión.";

            return RedirectToAction("FondoPetroleo");
        }

        [HttpPost]
        public IActionResult SubirInformeFinal(string carpeta, IFormFile informeFinal)
        {
            if (string.IsNullOrEmpty(carpeta))
            {
                TempData["MensajeError"] = "⚠️ Carpeta inválida.";
                return RedirectToAction("FondoPetroleo");
            }

            var ruta = Path.Combine(Directory.GetCurrentDirectory(), "wwwroot", "documentos", "revision_secretario", carpeta);

            if (!Directory.Exists(ruta))
            {
                Directory.CreateDirectory(ruta); // Crea la carpeta si no existe
            }

            // Guarda un archivo de control para indicar que ya se guardaron informes
            var nombreArchivo = $"Informe_Final_Guardado.txt";
            System.IO.File.WriteAllText(Path.Combine(ruta, nombreArchivo),
                $"Informe guardado el {DateTime.Now}");

            // 2. Guardar informe si se subió
            if (informeFinal != null && informeFinal.Length > 0)
            {
                var informeFinalPath = Path.Combine(ruta, "Informe_Final_Proyecto.pdf");
                using (var stream = new FileStream(informeFinalPath, FileMode.Create))
                {
                    informeFinal.CopyTo(stream);
                }
            }

            TempData["MensajeExito"] = "✅ Informes guardados correctamente.";
            return RedirectToAction("FondoPetroleo");
        }

        [HttpPost]
        public IActionResult SubirReunionFinal(string carpeta)
        {
            if (string.IsNullOrEmpty(carpeta))
            {
                TempData["MensajeError"] = "⚠️ Carpeta inválida.";
                return RedirectToAction("FondoPetroleo");
            }

            var ruta = Path.Combine(Directory.GetCurrentDirectory(), "wwwroot", "documentos", "revision_secretario", carpeta);

            if (!Directory.Exists(ruta))
            {
                Directory.CreateDirectory(ruta); // Crea la carpeta si no existe
            }

            // Guarda un archivo de control para indicar que ya se guardaron informes
            var nombreArchivo = $"Final_Proyecto.txt";
            System.IO.File.WriteAllText(Path.Combine(ruta, nombreArchivo),
                $"Informe guardado el {DateTime.Now}");

            TempData["MensajeExito"] = "✅ Informes guardados correctamente.";
            return RedirectToAction("FondoPetroleo");
        }

        [HttpPost]
        public IActionResult SuscribirActaFiniquito(string carpeta)
        {
            if (string.IsNullOrEmpty(carpeta))
            {
                TempData["MensajeError"] = "⚠️ Carpeta inválida.";
                return RedirectToAction("FondoPetroleo");
            }

            var ruta = Path.Combine(Directory.GetCurrentDirectory(), "wwwroot", "documentos", "revision_secretario", carpeta);

            if (!Directory.Exists(ruta))
            {
                Directory.CreateDirectory(ruta); // Crea la carpeta si no existe
            }

            // Guarda un archivo de control para indicar que ya se guardaron informes
            var nombreArchivo = $"Finiquito.txt";
            System.IO.File.WriteAllText(Path.Combine(ruta, nombreArchivo),
                $"Informe guardado el {DateTime.Now}");

            TempData["MensajeExito"] = "✅ Informes guardados correctamente.";
            return RedirectToAction("FondoPetroleo");
        }

        //FORMULARIO
        [HttpPost]
        public IActionResult GuardarEvaluacion(string carpeta, IFormFile archivoEvaluacion)
        {
            if (string.IsNullOrEmpty(carpeta))
            {
                TempData["MensajeError"] = "⚠️ Carpeta inválida.";
                return RedirectToAction("FondoPetroleo");
            }

            var ruta = Path.Combine(Directory.GetCurrentDirectory(), "wwwroot", "documentos", "revision_secretario", carpeta);
            if (!Directory.Exists(ruta))
                Directory.CreateDirectory(ruta);

            try
            {
                var form = Request.Form;
                var calificaciones = new Dictionary<string, int>();
                var observaciones = new Dictionary<string, string>();

                // 🔹 Recorremos todos los campos del formulario
                foreach (var key in form.Keys)
                {
                    var valor = form[key];

                    // Solo tomamos los que tienen números (calificaciones)
                    if (int.TryParse(valor, out int numero))
                    {
                        calificaciones[key] = numero;
                    }
                    // Tomamos los campos de observaciones (empiezan con "obs_")
                    else if (key.StartsWith("obs_"))
                    {
                        observaciones[key] = valor;
                    }
                }

                Console.WriteLine("📋 Calificaciones recibidas:");
                foreach (var kv in calificaciones)
                    Console.WriteLine($"{kv.Key}: {kv.Value}");

                Console.WriteLine("📋 Observaciones recibidas:");
                foreach (var kv in observaciones)
                    Console.WriteLine($"{kv.Key}: {kv.Value}");

                Console.WriteLine("📄 Archivo: " + (archivoEvaluacion?.FileName ?? "Sin archivo"));

                // 🔹 Guardar archivo adjunto (si existe)
                if (archivoEvaluacion != null && archivoEvaluacion.Length > 0)
                {
                    var rutaArchivo = Path.Combine(ruta, archivoEvaluacion.FileName);
                    using (var stream = new FileStream(rutaArchivo, FileMode.Create))
                    {
                        archivoEvaluacion.CopyTo(stream);
                    }
                }

                // 🔹 Crear el PDF
                var rutaPdf = Path.Combine(ruta, "Formato_Evaluacion_Sustentabilidad.pdf");
                using (var writer = new PdfWriter(rutaPdf))
                using (var pdf = new PdfDocument(writer))
                using (var doc = new Document(pdf))
                {
                    PdfFont bold = PdfFontFactory.CreateFont(StandardFonts.HELVETICA_BOLD);

                    // Encabezado
                    doc.Add(new Paragraph("Formato de Evaluación del Proyecto")
                        .SetTextAlignment(TextAlignment.CENTER)
                        .SetFontSize(16)
                        .SetFont(bold));

                    doc.Add(new Paragraph($"Fecha: {DateTime.Now:dd/MM/yyyy}")
                        .SetTextAlignment(TextAlignment.RIGHT)
                        .SetFontSize(10));

                    doc.Add(new Paragraph("Instrucciones: Evalúe la propuesta considerando si atiende satisfactoriamente las preguntas relacionadas al cumplimiento de los criterios especificados; asignando un valor con base en la siguiente escala de referencia:")
                        .SetFontSize(10));

                    // Escala de referencia
                    Table tablaEscala = new Table(5).UseAllAvailableWidth();
                    tablaEscala.AddHeaderCell("Totalmente No Satisfactorio");
                    tablaEscala.AddHeaderCell("1");
                    tablaEscala.AddHeaderCell("2");
                    tablaEscala.AddHeaderCell("3");
                    tablaEscala.AddHeaderCell("4 - Totalmente Satisfactorio");
                    doc.Add(tablaEscala);

                    doc.Add(new Paragraph("\n"));

                    // 🔹 Tabla principal de evaluación
                    Table tabla = new Table(new float[] { 4, 2, 6 }).UseAllAvailableWidth();
                    tabla.AddHeaderCell(new Cell().Add(new Paragraph("Pregunta").SetFont(bold)));
                    tabla.AddHeaderCell(new Cell().Add(new Paragraph("Calificación").SetFont(bold)));
                    tabla.AddHeaderCell(new Cell().Add(new Paragraph("Observaciones").SetFont(bold)));

                    foreach (var kv in calificaciones)
                    {
                        tabla.AddCell(kv.Key); // Nombre de la pregunta
                        tabla.AddCell(kv.Value.ToString()); // Calificación

                        // Buscamos si hay observación relacionada
                        string obsKey = "obs_" + kv.Key;
                        observaciones.TryGetValue(obsKey, out string obs);
                        tabla.AddCell(obs ?? "—");
                    }

                    doc.Add(tabla);

                    // Pie
                    doc.Add(new Paragraph("\nEvaluación generada automáticamente.")
                        .SetTextAlignment(TextAlignment.CENTER)
                        .SetFontSize(10));

                    doc.Close();
                }

                var infoArchivo = new FileInfo(rutaPdf);
                Console.WriteLine($"✅ Archivo generado en: {infoArchivo.FullName} ({infoArchivo.Length} bytes)");

                TempData["MensajeExito"] = "✅ Evaluación guardada en PDF correctamente.";
            }
            catch (Exception ex)
            {
                TempData["MensajeError"] = $"⚠️ Error al generar PDF: {ex.Message}";
                Console.WriteLine("❌ Error: " + ex);
            }

            return RedirectToAction("FondoPetroleo");
        }
    }
}
