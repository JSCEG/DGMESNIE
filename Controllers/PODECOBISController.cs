using iText.IO.Font.Constants;
using iText.Kernel.Font;
using iText.Kernel.Geom;
using iText.Kernel.Pdf;
using iText.Layout;
using iText.Layout.Element;
using iText.Layout.Properties;
using Microsoft.AspNetCore.Mvc;
using NSIE.Models;
using NSIE.Servicios;
using Newtonsoft.Json;

namespace NSIE.Controllers
{
    [ServiceFilter(typeof(ValidacionInputFiltro))]
    [AutorizacionFiltro]
    public class PODECOBISController : Controller
    {
        private readonly IRepositorioPODECOBIS _repositorio;

        public PODECOBISController(IRepositorioPODECOBIS repositorio)
        {
            _repositorio = repositorio;
        }

        public IActionResult Index()
        {
            return RedirectToAction(nameof(Agenda));
        }

        public async Task<IActionResult> Agenda(string busqueda = null)
        {
            var model = new PODECOBISAgendaViewModel
            {
                Header = ConstruirHeader(),
                Busqueda = busqueda,
                Registros = await _repositorio.ObtenerAgendaAsync(busqueda)
            };

            return View(model);
        }

        public IActionResult Crear()
        {
            PrepararVistaFormulario("Nuevo registro de agenda", "Guardar registro");
            return View(new PODECOBISAgendaItem { Activo = true });
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Crear(PODECOBISAgendaItem registro)
        {
            if (!ModelState.IsValid)
            {
                PrepararVistaFormulario("Nuevo registro de agenda", "Guardar registro");
                return View(registro);
            }

            await _repositorio.CrearAsync(registro);
            TempData["SuccessMessage"] = "El contacto se registró correctamente.";
            return RedirectToAction(nameof(Agenda));
        }

        public async Task<IActionResult> Editar(int id)
        {
            var registro = await _repositorio.ObtenerPorIdAsync(id);
            if (registro == null)
            {
                return NotFound();
            }

            PrepararVistaFormulario("Editar registro de agenda", "Guardar cambios");
            return View(registro);
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Editar(PODECOBISAgendaItem registro)
        {
            if (!ModelState.IsValid)
            {
                PrepararVistaFormulario("Editar registro de agenda", "Guardar cambios");
                return View(registro);
            }

            await _repositorio.ActualizarAsync(registro);
            TempData["SuccessMessage"] = "El registro se actualizó correctamente.";
            return RedirectToAction(nameof(Agenda));
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Eliminar(int id)
        {
            await _repositorio.EliminarAsync(id);
            TempData["SuccessMessage"] = "El registro se eliminó de la agenda.";
            return RedirectToAction(nameof(Agenda));
        }

        public async Task<IActionResult> ExportarAgendaPdf()
        {
            var registros = await _repositorio.ObtenerAgendaAsync();
            using var stream = new MemoryStream();
            using var writer = new PdfWriter(stream);
            using var pdf = new PdfDocument(writer);
            pdf.SetDefaultPageSize(PageSize.A4.Rotate());
            using var document = new Document(pdf);

            PdfFont bold = PdfFontFactory.CreateFont(StandardFonts.HELVETICA_BOLD);

            document.Add(new Paragraph("Agenda PODECOBIS")
                .SetFont(bold)
                .SetFontSize(16)
                .SetTextAlignment(TextAlignment.CENTER));

            document.Add(new Paragraph($"Fecha de emisión: {DateTime.Now:dd/MM/yyyy HH:mm}")
                .SetFontSize(9)
                .SetTextAlignment(TextAlignment.RIGHT));

            document.Add(new Paragraph("Directorio de contactos principales de Polos de Desarrollo Económico para el Bienestar.")
                .SetFontSize(10));

            var table = new Table(new float[] { 1.1f, 2.2f, 3.4f, 2.4f, 2.3f, 1.6f, 2.5f, 1.8f }).UseAllAvailableWidth();

            AgregarEncabezado(table, bold, "No.");
            AgregarEncabezado(table, bold, "Polo");
            AgregarEncabezado(table, bold, "Nombre oficial");
            AgregarEncabezado(table, bold, "Organismo");
            AgregarEncabezado(table, bold, "Contacto");
            AgregarEncabezado(table, bold, "Cargo");
            AgregarEncabezado(table, bold, "Correo");
            AgregarEncabezado(table, bold, "Teléfono");

            foreach (var registro in registros)
            {
                table.AddCell(CrearCelda(registro.Numero.ToString()));
                table.AddCell(CrearCelda(registro.Polo));
                table.AddCell(CrearCelda(registro.NombreOficialDeclaratoria));
                table.AddCell(CrearCelda(registro.OrganismoSeguimiento));
                table.AddCell(CrearCelda(registro.NombreContacto));
                table.AddCell(CrearCelda(registro.Cargo));
                table.AddCell(CrearCelda(registro.Correo));
                table.AddCell(CrearCelda(registro.NumeroTelefonico));
            }

            document.Add(table);
            document.Close();

            return File(stream.ToArray(), "application/pdf", $"PODECOBIS_Agenda_{DateTime.Now:yyyyMMdd}.pdf");
        }

        private static Cell CrearCelda(string valor)
        {
            return new Cell().Add(new Paragraph(string.IsNullOrWhiteSpace(valor) ? "-" : valor).SetFontSize(8));
        }

        private static void AgregarEncabezado(Table table, PdfFont bold, string texto)
        {
            table.AddHeaderCell(new Cell().Add(new Paragraph(texto).SetFont(bold).SetFontSize(9)));
        }

        private void PrepararVistaFormulario(string titulo, string accion)
        {
            ViewData["HeaderModel"] = ConstruirHeader();
            ViewData["FormTitle"] = titulo;
            ViewData["SubmitLabel"] = accion;
        }

        private HeaderViewModel ConstruirHeader()
        {
            var moduleInfo = new
            {
                title = "Agenda PODECOBIS",
                description = "Administra el directorio operativo de contactos para los Polos de Desarrollo Económico para el Bienestar.",
                stage = "Seguimiento y coordinación",
                order = new { step = 1, description = "Consulta, alta y actualización de contactos clave" },
                functionality = "Permite concentrar polos, organismos responsables y datos de contacto en un solo módulo con búsqueda rápida y salida a PDF.",
                highlights = new[]
                {
                    "Buscar polos o contactos en tiempo real",
                    "Registrar nuevos enlaces operativos",
                    "Actualizar correo, cargo y teléfono",
                    "Generar un reporte PDF de la agenda"
                },
                roles = new[]
                {
                    new { icon = "users", text = "Equipos administrativos y de seguimiento institucional" },
                    new { icon = "address-book", text = "Responsables de coordinación de polos" }
                },
                context = "Mantén esta agenda actualizada para asegurar contacto oportuno con cada organismo de seguimiento."
            };

            return new HeaderViewModel
            {
                Title = "Agenda PODECOBIS",
                IconPath = "proyecto.png",
                Description = "Directorio de contactos principales para los Polos de Desarrollo Económico para el Bienestar.",
                Section = "PODECOBIS",
                ModuleInfo = JsonConvert.SerializeObject(moduleInfo)
            };
        }
    }
}