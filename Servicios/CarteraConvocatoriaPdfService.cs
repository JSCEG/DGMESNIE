using System.Globalization;
using iText.IO.Font;
using iText.IO.Image;
using iText.Kernel.Colors;
using iText.Kernel.Events;
using iText.Kernel.Font;
using iText.Kernel.Geom;
using iText.Kernel.Pdf;
using iText.Kernel.Pdf.Canvas;
using iText.Layout;
using iText.Layout.Borders;
using iText.Layout.Element;
using iText.Layout.Properties;
using NSIE.Models.ProyectosPrivados;

namespace NSIE.Servicios;

/// <summary>
/// Genera el reporte ejecutivo 16:9 de la cartera con datos vivos de SQL.
/// Ref: identidad institucional SENER y estructura del reporte de Mesa Técnica del 5 de agosto de 2026.
/// </summary>
public static class CarteraConvocatoriaPdfService
{
    private static readonly PageSize Slide = new(960, 540);
    private static readonly DeviceRgb Guinda = new(155, 34, 71);
    private static readonly DeviceRgb GuindaOscuro = new(111, 22, 54);
    private static readonly DeviceRgb Verde = new(0, 139, 111);
    private static readonly DeviceRgb Azul = new(31, 158, 188);
    private static readonly DeviceRgb Dorado = new(229, 162, 31);
    private static readonly DeviceRgb Gris = new(117, 113, 108);
    private static readonly DeviceRgb GrisClaro = new(244, 242, 239);
    private static readonly DeviceRgb GrisLinea = new(224, 219, 213);
    private static readonly DeviceRgb Negro = new(29, 29, 29);
    private static readonly CultureInfo EsMx = CultureInfo.GetCultureInfo("es-MX");

    public static byte[] Generar(CarteraConvocatoriaDatos datos, string webRootPath, DateTime fechaCorte)
    {
        var projects = datos.Projects
            .OrderBy(project => project.Rank)
            .ThenBy(project => project.Folio, StringComparer.OrdinalIgnoreCase)
            .ToList();

        using var stream = new MemoryStream();
        using var writer = new PdfWriter(stream);
        using var pdf = new PdfDocument(writer);
        pdf.SetDefaultPageSize(Slide);
        using var document = new Document(pdf, Slide);
        document.SetMargins(26, 48, 32, 48);

        var fonts = LoadFonts(webRootPath);
        var logos = LoadLogos(webRootPath);
        pdf.AddEventHandler(PdfDocumentEvent.END_PAGE, new FooterEventHandler(fonts.Body, fechaCorte));

        AddCover(document, projects, fonts, logos, fechaCorte);
        AddIndex(document, fonts, logos);
        AddUniverse(document, projects, fonts, logos);
        AddAnalysis(document, projects, fonts, logos);
        AddFactibleComposition(document, projects, fonts, logos);
        AddRegionalDistribution(document, projects, fonts, logos);

        var factibles = projects.Where(IsFactible).ToList();
        foreach (var region in factibles.GroupBy(project => project.Region).OrderBy(group => RegionOrder(group.Key)))
        {
            AddProjectPages(
                document,
                region.ToList(),
                fonts,
                logos,
                $"Gerencia {region.Key}.",
                "SECCIÓN 02 · SELECCIÓN POR GERENCIA",
                $"{region.Count()} seleccionados · {FormatMw(region.Sum(project => project.Mw))} MW netos",
                Verde,
                rowsPerPage: 7,
                includeAnalysis: false);
        }

        AddNextSteps(document, fonts, logos);

        AddProjectPages(document, projects.Where(project => NormalizeClass(project.AnalysisClassification) == "Excluyente preseleccionado").ToList(), fonts, logos,
            "Excluyentes seleccionados.", "ANEXO", "Compiten por el mismo punto de interconexión", Azul, 6, true);

        AddProjectPages(document, projects.Where(project => NormalizeClass(project.AnalysisClassification) == "En análisis").ToList(), fonts, logos,
            "En análisis.", "ANEXO", "Requieren análisis adicional antes de definir su viabilidad", Dorado, 6, true, groupDuplicates: true);

        AddProjectPages(document, projects.Where(project => NormalizeClass(project.AnalysisClassification) == "Por analizar").ToList(), fonts, logos,
            "Por analizar.", "ANEXO", "Inscripciones en revisión", Gris, 7, true);

        AddProjectPages(document, projects.Where(project => NormalizeClass(project.AnalysisClassification) == "No viable").ToList(), fonts, logos,
            "No viables.", "ANEXO", "Obras onerosas o sin beneficio para el SEN", Guinda, 6, true);

        AddGlossary(document, fonts, logos);

        document.Flush();
        document.Close();
        return stream.ToArray();
    }

    private static void AddCover(Document document, IReadOnlyCollection<CarteraConvocatoriaProyecto> projects, Fonts fonts, Logos logos, DateTime fechaCorte)
    {
        AddLogoHeader(document, fonts, logos, "SUBSECRETARÍA DE PLANEACIÓN Y TRANSICIÓN ENERGÉTICA");
        document.Add(new Paragraph("SELECCIÓN DE PROYECTOS")
            .SetFont(fonts.BodyBold).SetFontSize(10).SetFontColor(Guinda).SetCharacterSpacing(1.4f)
            .SetMarginTop(68).SetMarginBottom(8));
        document.Add(new Paragraph("Selección de proyectos para\ndesarrollo en esquema mixto con CFE")
            .SetFont(fonts.TitleBold).SetFontSize(30).SetFontColor(Negro).SetFixedLeading(34)
            .SetMargin(0).SetWidth(660));
        document.Add(new Paragraph($"Resultados de Mesa Técnica al {fechaCorte:d 'de' MMMM 'de' yyyy}.")
            .SetFont(fonts.Body).SetFontSize(13).SetFontColor(Gris).SetMarginTop(10));

        var totalMw = projects.Sum(project => project.Mw);
        var factibles = projects.Where(IsFactible).ToList();
        var metrics = new Table(UnitValue.CreatePercentArray(new[] { 1f, 1.2f, 1.2f, 1.5f }))
            .UseAllAvailableWidth().SetMarginTop(32).SetBackgroundColor(Guinda);
        AddMetric(metrics, fonts, projects.Count.ToString(EsMx), "proyectos", true);
        AddMetric(metrics, fonts, FormatMw(totalMw), "MW netos", true);
        AddMetric(metrics, fonts, factibles.Count.ToString(EsMx), "proyectos factibles", true);
        AddMetric(metrics, fonts, FormatMw(factibles.Sum(project => project.Mw)), "MW factibles", true);
        document.Add(metrics);

        document.Add(new Paragraph("Dirección General de Metodología y Evaluación del Sistema Nacional de Información Energética")
            .SetFont(fonts.Body).SetFontSize(9).SetFontColor(Gris).SetMarginTop(42));
    }

    private static void AddIndex(Document document, Fonts fonts, Logos logos)
    {
        StartPage(document, fonts, logos, "CONTENIDO", "Índice de contenido.", "Reporte ejecutivo");
        var table = new Table(UnitValue.CreatePercentArray(new[] { .8f, 4.2f, 4.2f })).UseAllAvailableWidth().SetMarginTop(18);
        AddIndexCell(table, fonts, "01", "Panorama", "Universo de análisis\nResultado de viabilidad\nComposición de la cartera factible");
        AddIndexCell(table, fonts, "02", "Selección por gerencia", "Noreste · Occidental\nOriental · Central\nListado de proyectos seleccionados");
        AddIndexCell(table, fonts, "03", "Próximos pasos", "Mesas técnica, social, ambiental y financiera\nSeguimiento de acuerdos");
        AddIndexCell(table, fonts, "A", "Anexos", "Excluyentes seleccionados\nEn análisis · Por analizar\nNo viables · Glosario");
        document.Add(table);
    }

    private static void AddUniverse(Document document, IReadOnlyCollection<CarteraConvocatoriaProyecto> projects, Fonts fonts, Logos logos)
    {
        StartPage(document, fonts, logos, "UNIVERSO DE ANÁLISIS", "Universo de análisis.", "Las dos convocatorias");
        var strategic = projects.Where(project => project.Type == "Estratégico").ToList();
        var privateProjects = projects.Where(project => project.Type != "Estratégico").ToList();
        var cards = new Table(UnitValue.CreatePercentArray(new[] { 1f, 1f })).UseAllAvailableWidth().SetMarginTop(12);
        cards.AddCell(UniverseCard(fonts, "CFE-CM2", "Convocatoria de Proyectos Estratégicos", strategic, Guinda));
        cards.AddCell(UniverseCard(fonts, "VUPE-C2", "Convocatoria 2 de particulares", privateProjects, Dorado));
        document.Add(cards);

        var banner = new Table(UnitValue.CreatePercentArray(new[] { 1.1f, 1f })).UseAllAvailableWidth().SetMarginTop(18);
        banner.AddCell(new Cell().SetBorder(Border.NO_BORDER).SetBackgroundColor(Guinda).SetPadding(14)
            .Add(new Paragraph($"{projects.Count} proyectos por {FormatMw(projects.Sum(project => project.Mw))} MW")
                .SetFont(fonts.TitleBold).SetFontSize(20).SetFontColor(ColorConstants.WHITE).SetMargin(0)));
        banner.AddCell(new Cell().SetBorder(Border.NO_BORDER).SetBackgroundColor(Guinda).SetPadding(14)
            .Add(new Paragraph("Cartera integrada para priorización, seguimiento y evaluación institucional.")
                .SetFont(fonts.Body).SetFontSize(11).SetFontColor(ColorConstants.WHITE).SetMargin(0)));
        document.Add(banner);
    }

    private static void AddAnalysis(Document document, IReadOnlyCollection<CarteraConvocatoriaProyecto> projects, Fonts fonts, Logos logos)
    {
        StartPage(document, fonts, logos, "RESULTADO DEL ANÁLISIS", "Resultado del análisis de viabilidad.", "Clasificación por viabilidad");
        document.Add(new Paragraph("Los proyectos factibles pasan a las mesas técnica, social, ambiental y financiera.")
            .SetFont(fonts.Body).SetFontSize(12).SetFontColor(Gris).SetMarginTop(0).SetMarginBottom(9));

        var categories = new[]
        {
            ("Factible", "Pasan a las mesas de evaluación", Verde),
            ("Excluyente preseleccionado", "Compite por el mismo punto de red", Azul),
            ("En análisis", "Requiere análisis adicional", Dorado),
            ("Por analizar", "Inscripción en revisión", Gris),
            ("No viable", "Obras onerosas o sin beneficio al SEN", Guinda),
        };
        var table = new Table(UnitValue.CreatePercentArray(new[] { .15f, 3.5f, 2.5f, 1.4f, 1.25f })).UseAllAvailableWidth();
        foreach (var category in categories)
        {
            var items = projects.Where(project => NormalizeClass(project.AnalysisClassification) == category.Item1).ToList();
            var count = category.Item1 == "En análisis" ? CountDistinctProjects(items) : items.Count;
            table.AddCell(new Cell().SetBorder(Border.NO_BORDER).SetBorderBottom(new SolidBorder(GrisLinea, .6f)).SetPadding(8).SetBackgroundColor(category.Item3));
            table.AddCell(new Cell().SetBorder(Border.NO_BORDER).SetBorderBottom(new SolidBorder(GrisLinea, .6f)).SetPadding(7)
                .Add(new Paragraph(category.Item1).SetFont(fonts.BodyBold).SetFontSize(12).SetMargin(0))
                .Add(new Paragraph(category.Item2).SetFont(fonts.Body).SetFontSize(8.5f).SetFontColor(Gris).SetMargin(0)));
            table.AddCell(new Cell().SetBorder(Border.NO_BORDER).SetBorderBottom(new SolidBorder(GrisLinea, .6f)).SetPadding(7)
                .Add(new Paragraph(new string('=', Math.Max(1, (int)Math.Round(items.Sum(project => project.Mw) / 260m))))
                    .SetFont(fonts.Body).SetFontSize(10).SetFontColor(category.Item3).SetMargin(0)));
            table.AddCell(TextCell(fonts, $"{FormatMw(items.Sum(project => project.Mw))} MW", 11, true, TextAlignment.RIGHT));
            table.AddCell(TextCell(fonts, $"{count} proyectos", 9, false, TextAlignment.RIGHT).SetFontColor(Gris));
        }
        document.Add(table);

        document.Add(new Paragraph($"UNIVERSO DE ANÁLISIS     {FormatMw(projects.Sum(project => project.Mw))} MW · {projects.Count} registros")
            .SetFont(fonts.BodyBold).SetFontSize(11).SetFontColor(Guinda).SetTextAlignment(TextAlignment.RIGHT).SetMarginTop(10));
    }

    private static void AddFactibleComposition(Document document, IReadOnlyCollection<CarteraConvocatoriaProyecto> projects, Fonts fonts, Logos logos)
    {
        StartPage(document, fonts, logos, "PROYECTOS FACTIBLES", "Composición de la cartera factible.", "Tecnología, convocatoria y entidad");
        var factibles = projects.Where(IsFactible).ToList();
        var columns = new Table(UnitValue.CreatePercentArray(new[] { 1f, 1f })).UseAllAvailableWidth();

        var technologyCell = new Cell().SetBorder(Border.NO_BORDER).SetPaddingRight(18);
        technologyCell.Add(SectionLabel(fonts, "POR TECNOLOGÍA"));
        foreach (var group in factibles.GroupBy(project => project.Technology ?? "Sin tecnología").OrderByDescending(group => group.Sum(project => project.Mw)))
            technologyCell.Add(DistributionRow(fonts, group.Key, group.Count(), group.Sum(project => project.Mw), Verde));

        technologyCell.Add(SectionLabel(fonts, "POR CONVOCATORIA").SetMarginTop(18));
        foreach (var group in factibles.GroupBy(project => project.Type).OrderBy(group => group.Key))
            technologyCell.Add(DistributionRow(fonts, group.Key, group.Count(), group.Sum(project => project.Mw), Dorado));

        var stateCell = new Cell().SetBorder(Border.NO_BORDER).SetPaddingLeft(18);
        stateCell.Add(SectionLabel(fonts, "POR ENTIDAD FEDERATIVA"));
        foreach (var group in factibles.GroupBy(project => NormalizeState(project.State)).OrderByDescending(group => group.Sum(project => project.Mw)))
            stateCell.Add(DistributionRow(fonts, group.Key, group.Count(), group.Sum(project => project.Mw), Guinda));

        columns.AddCell(technologyCell);
        columns.AddCell(stateCell);
        document.Add(columns);
    }

    private static void AddRegionalDistribution(Document document, IReadOnlyCollection<CarteraConvocatoriaProyecto> projects, Fonts fonts, Logos logos)
    {
        StartPage(document, fonts, logos, "PROYECTOS FACTIBLES", "Distribución por gerencia de control regional.", "Gerencias de Control Regional");
        var factibles = projects.Where(IsFactible).ToList();
        var table = new Table(UnitValue.CreatePercentArray(new[] { 2.3f, 1.1f, 1f, 1.4f, 1.6f })).UseAllAvailableWidth().SetMarginTop(14);
        AddHeaderCell(table, fonts, "GERENCIA");
        AddHeaderCell(table, fonts, "EST.");
        AddHeaderCell(table, fonts, "PART.");
        AddHeaderCell(table, fonts, "PROY.");
        AddHeaderCell(table, fonts, "MW NETOS");
        foreach (var group in factibles.GroupBy(project => project.Region).OrderBy(group => RegionOrder(group.Key)))
        {
            table.AddCell(TextCell(fonts, group.Key, 13, true));
            table.AddCell(TextCell(fonts, group.Count(project => project.Type == "Estratégico").ToString(EsMx), 11, false, TextAlignment.CENTER));
            table.AddCell(TextCell(fonts, group.Count(project => project.Type != "Estratégico").ToString(EsMx), 11, false, TextAlignment.CENTER));
            table.AddCell(TextCell(fonts, group.Count().ToString(EsMx), 11, true, TextAlignment.CENTER).SetFontColor(Verde));
            table.AddCell(TextCell(fonts, FormatMw(group.Sum(project => project.Mw)), 12, true, TextAlignment.RIGHT));
        }
        table.AddCell(TextCell(fonts, "Total", 13, true).SetFontColor(Verde).SetBorderTop(new SolidBorder(Verde, 1.2f)));
        table.AddCell(TextCell(fonts, factibles.Count(project => project.Type == "Estratégico").ToString(EsMx), 11, true, TextAlignment.CENTER).SetBorderTop(new SolidBorder(Verde, 1.2f)));
        table.AddCell(TextCell(fonts, factibles.Count(project => project.Type != "Estratégico").ToString(EsMx), 11, true, TextAlignment.CENTER).SetBorderTop(new SolidBorder(Verde, 1.2f)));
        table.AddCell(TextCell(fonts, factibles.Count.ToString(EsMx), 11, true, TextAlignment.CENTER).SetBorderTop(new SolidBorder(Verde, 1.2f)));
        table.AddCell(TextCell(fonts, FormatMw(factibles.Sum(project => project.Mw)), 12, true, TextAlignment.RIGHT).SetFontColor(Verde).SetBorderTop(new SolidBorder(Verde, 1.2f)));
        document.Add(table);

        document.Add(new Paragraph("La cartera factible cubre 3,305 MW en el Sistema Interconectado Nacional. Los sistemas de Baja California permanecen pendientes de cobertura.")
            .SetFont(fonts.Body).SetFontSize(10).SetFontColor(Gris).SetMarginTop(16));
    }

    private static void AddNextSteps(Document document, Fonts fonts, Logos logos)
    {
        StartPage(document, fonts, logos, "SECCIÓN 03 · PRÓXIMOS PASOS", "Siguientes pasos.", "Ruta de trabajo");
        var steps = new[]
        {
            ("01", "Constar las obras de interconexión y refuerzo identificadas.", "CFE"),
            ("02", "Continuar los estudios de interconexión pendientes.", "CENACE"),
            ("03", "Evaluar los proyectos en las mesas técnica, social, ambiental y financiera.", "MESAS"),
            ("04", "Revisar la distribución regional y el crecimiento armónico de la red.", "SENER"),
            ("05", "Registrar acuerdos, comentarios y decisión de continuidad en la plataforma.", "SEGUIMIENTO"),
        };
        var table = new Table(UnitValue.CreatePercentArray(new[] { .5f, 5.5f, 1.3f })).UseAllAvailableWidth().SetMarginTop(12);
        foreach (var step in steps)
        {
            table.AddCell(TextCell(fonts, step.Item1, 10, true, TextAlignment.CENTER).SetFontColor(Guinda));
            table.AddCell(TextCell(fonts, step.Item2, 11, false));
            table.AddCell(TextCell(fonts, step.Item3, 8, true, TextAlignment.CENTER).SetBackgroundColor(GrisClaro).SetFontColor(Guinda));
        }
        document.Add(table);
    }

    private static void AddGlossary(Document document, Fonts fonts, Logos logos)
    {
        StartPage(document, fonts, logos, "ANEXO", "Glosario.", "Siglas y términos");
        var terms = new[]
        {
            ("CFE-CM2", "Folio de la Convocatoria de Proyectos Estratégicos para desarrollo mixto con CFE."),
            ("VUPE-C2", "Folio de la Ventanilla Única de Proyectos Energéticos, segunda convocatoria."),
            ("GCR", "Gerencia de Control Regional del CENACE."),
            ("SEN", "Sistema Eléctrico Nacional."),
            ("MW netos", "Capacidad neta considerada para efectos de priorización."),
            ("Factible", "Proyecto que pasa a las mesas de evaluación técnica, social, ambiental y financiera."),
            ("Excluyente", "Proyecto que compite con otro por el mismo punto de interconexión."),
        };
        var table = new Table(UnitValue.CreatePercentArray(new[] { 1.4f, 5.6f })).UseAllAvailableWidth().SetMarginTop(12);
        foreach (var term in terms)
        {
            table.AddCell(TextCell(fonts, term.Item1, 10, true).SetFontColor(Guinda));
            table.AddCell(TextCell(fonts, term.Item2, 10, false));
        }
        document.Add(table);
    }

    private static void AddProjectPages(
        Document document,
        IReadOnlyCollection<CarteraConvocatoriaProyecto> source,
        Fonts fonts,
        Logos logos,
        string title,
        string section,
        string subtitle,
        Color accent,
        int rowsPerPage,
        bool includeAnalysis,
        bool groupDuplicates = false)
    {
        var rows = BuildRows(source, groupDuplicates).ToList();
        if (rows.Count == 0) return;
        var pages = (int)Math.Ceiling(rows.Count / (double)rowsPerPage);
        for (var pageIndex = 0; pageIndex < pages; pageIndex++)
        {
            var pageRows = rows.Skip(pageIndex * rowsPerPage).Take(rowsPerPage).ToList();
            var pageLabel = pages > 1 ? $"{subtitle} · {pageIndex + 1} de {pages}" : subtitle;
            StartPage(document, fonts, logos, section, title, pageLabel, accent);

            var table = new Table(UnitValue.CreatePercentArray(includeAnalysis
                ? new[] { 2.7f, 1.15f, 1.45f, .55f, .8f, 2.35f }
                : new[] { 2.9f, 1.35f, 1.65f, .65f, .85f }))
                .UseAllAvailableWidth().SetMarginTop(8);
            AddHeaderCell(table, fonts, "PROYECTO");
            AddHeaderCell(table, fonts, "ENTIDAD");
            AddHeaderCell(table, fonts, "GRUPO DE INTERÉS");
            AddHeaderCell(table, fonts, "TEC.");
            AddHeaderCell(table, fonts, "MW");
            if (includeAnalysis) AddHeaderCell(table, fonts, "OBSERVACIÓN");

            foreach (var row in pageRows)
            {
                var rowPadding = includeAnalysis ? 4f : 3.5f;
                var projectCell = new Cell().SetBorder(Border.NO_BORDER).SetBorderBottom(new SolidBorder(GrisLinea, .55f)).SetPadding(rowPadding);
                projectCell.Add(new Paragraph(row.Name).SetFont(fonts.BodyBold).SetFontSize(includeAnalysis ? 8.5f : 9f).SetMargin(0));
                projectCell.Add(new Paragraph($"{(row.Type == "Estratégico" ? "EST" : "PART")} · {row.Folios}")
                    .SetFont(fonts.BodyBold).SetFontSize(6.1f).SetFontColor(Guinda).SetMargin(0));
                table.AddCell(projectCell);
                table.AddCell(TextCell(fonts, NormalizeState(row.State), 8.5f, false).SetPadding(rowPadding));
                table.AddCell(TextCell(fonts, Shorten(row.InterestGroup, 38), 8.1f, false).SetFontColor(Gris).SetPadding(rowPadding));
                table.AddCell(TextCell(fonts, TechnologyCode(row.Technology), 8.5f, true, TextAlignment.CENTER).SetFontColor(accent).SetPadding(rowPadding));
                table.AddCell(TextCell(fonts, FormatMwDetail(row.Mw), 8.8f, true, TextAlignment.RIGHT).SetPadding(rowPadding));
                if (includeAnalysis)
                    table.AddCell(TextCell(fonts, Shorten(row.Analysis, 105), 7.2f, false).SetFontColor(Gris).SetPadding(rowPadding));
            }
            document.Add(table);

            if (pageIndex == pages - 1)
                document.Add(new Paragraph($"Total · {rows.Count} proyectos · {FormatMw(rows.Sum(row => row.Mw))} MW netos")
                    .SetFont(fonts.BodyBold).SetFontSize(11).SetFontColor(accent).SetTextAlignment(TextAlignment.RIGHT).SetMarginTop(7));
        }
    }

    private static IEnumerable<ReportRow> BuildRows(IEnumerable<CarteraConvocatoriaProyecto> projects, bool groupDuplicates)
    {
        if (!groupDuplicates)
        {
            return projects.OrderByDescending(project => project.Mw).Select(ToReportRow);
        }

        return projects
            .GroupBy(project => string.IsNullOrWhiteSpace(project.DuplicateGroup) ? project.Folio : project.DuplicateGroup, StringComparer.OrdinalIgnoreCase)
            .OrderByDescending(group => group.Sum(project => project.Mw))
            .Select(group =>
            {
                var first = group.First();
                return new ReportRow(
                    string.IsNullOrWhiteSpace(first.DuplicateGroup) ? first.Name : first.DuplicateGroup!,
                    string.Join(" / ", group.Select(project => project.Folio)),
                    first.Type,
                    first.State,
                    first.Technology,
                    first.InterestGroup,
                    group.Sum(project => project.Mw),
                    string.Join(" ", group.Select(project => project.TechnicalAnalysis).Where(value => !string.IsNullOrWhiteSpace(value)).Distinct()));
            });
    }

    private static ReportRow ToReportRow(CarteraConvocatoriaProyecto project) => new(
        project.Name, project.Folio, project.Type, project.State, project.Technology, project.InterestGroup,
        project.Mw, project.TechnicalAnalysis ?? string.Empty);

    private static void StartPage(Document document, Fonts fonts, Logos logos, string section, string title, string corner, Color? accent = null)
    {
        document.Add(new AreaBreak(AreaBreakType.NEXT_PAGE));
        AddLogoHeader(document, fonts, logos, corner);
        document.Add(new Paragraph(section)
            .SetFont(fonts.BodyBold).SetFontSize(9).SetCharacterSpacing(1.2f).SetFontColor(Guinda)
            .SetBorderBottom(new SolidBorder(GrisLinea, .6f)).SetPaddingBottom(6).SetMarginBottom(7));

        var titleTable = new Table(UnitValue.CreatePercentArray(new[] { .08f, 4.92f })).UseAllAvailableWidth();
        titleTable.AddCell(new Cell().SetBorder(Border.NO_BORDER).SetBackgroundColor(accent ?? Guinda).SetPadding(0));
        titleTable.AddCell(new Cell().SetBorder(Border.NO_BORDER).SetPaddingLeft(9).SetPaddingTop(0).SetPaddingBottom(2)
            .Add(new Paragraph(title).SetFont(fonts.TitleBold).SetFontSize(23).SetFontColor(Negro).SetMargin(0)));
        document.Add(titleTable);
    }

    private static void AddLogoHeader(Document document, Fonts fonts, Logos logos, string corner)
    {
        var header = new Table(UnitValue.CreatePercentArray(new[] { .9f, .9f, 6.2f })).UseAllAvailableWidth().SetMarginBottom(2);
        header.AddCell(LogoCell(logos.Gobierno));
        header.AddCell(LogoCell(logos.Sener));
        header.AddCell(new Cell().SetBorder(Border.NO_BORDER).SetVerticalAlignment(VerticalAlignment.MIDDLE)
            .Add(new Paragraph(corner.ToUpperInvariant()).SetFont(fonts.Body).SetFontSize(7.5f).SetFontColor(Gris).SetTextAlignment(TextAlignment.RIGHT).SetMargin(0)));
        document.Add(header);
    }

    private static Cell UniverseCard(Fonts fonts, string code, string title, IReadOnlyCollection<CarteraConvocatoriaProyecto> projects, Color accent)
    {
        var cell = new Cell().SetBorder(Border.NO_BORDER).SetBorderTop(new SolidBorder(accent, 2f)).SetBackgroundColor(GrisClaro).SetPadding(16).SetMargin(4);
        cell.Add(new Paragraph(code).SetFont(fonts.Body).SetFontSize(9).SetCharacterSpacing(1.5f).SetFontColor(Gris).SetMargin(0));
        cell.Add(new Paragraph(title).SetFont(fonts.TitleBold).SetFontSize(18).SetFixedLeading(20).SetMarginTop(4).SetMarginBottom(8));
        cell.Add(new Paragraph("Proyectos inscritos para desarrollo bajo esquema mixto con la Comisión Federal de Electricidad.")
            .SetFont(fonts.Body).SetFontSize(9.5f).SetFontColor(Gris).SetFixedLeading(12));
        cell.Add(new Paragraph($"{projects.Count} proyectos     {FormatMw(projects.Sum(project => project.Mw))} MW netos")
            .SetFont(fonts.BodyBold).SetFontSize(15).SetFontColor(Guinda).SetMarginTop(16).SetMarginBottom(0));
        return cell;
    }

    private static Paragraph DistributionRow(Fonts fonts, string label, int count, decimal mw, Color color)
    {
        return new Paragraph()
            .Add(new Text($"-  {label}").SetFont(fonts.BodyBold).SetFontColor(color))
            .Add(new Text($"     {FormatMw(mw)} MW · {count} proy.").SetFont(fonts.Body).SetFontColor(Gris))
            .SetFontSize(8.5f).SetBorderBottom(new SolidBorder(GrisLinea, .5f)).SetPaddingTop(2.5f).SetPaddingBottom(2.5f).SetMargin(0);
    }

    private static Paragraph SectionLabel(Fonts fonts, string text) => new Paragraph(text)
        .SetFont(fonts.BodyBold).SetFontSize(9).SetCharacterSpacing(1.1f).SetFontColor(Guinda)
        .SetBorderBottom(new SolidBorder(Guinda, .8f)).SetPaddingBottom(4).SetMarginBottom(2);

    private static void AddMetric(Table table, Fonts fonts, string value, string label, bool light)
    {
        table.AddCell(new Cell().SetBorder(Border.NO_BORDER).SetPadding(13)
            .Add(new Paragraph(value).SetFont(fonts.TitleBold).SetFontSize(21).SetFontColor(light ? ColorConstants.WHITE : Guinda).SetMargin(0))
            .Add(new Paragraph(label).SetFont(fonts.Body).SetFontSize(8).SetFontColor(light ? ColorConstants.WHITE : Gris).SetMargin(0)));
    }

    private static void AddIndexCell(Table table, Fonts fonts, string number, string title, string detail)
    {
        table.AddCell(new Cell().SetBorder(Border.NO_BORDER).SetPadding(14).SetBackgroundColor(GrisClaro)
            .Add(new Paragraph(number).SetFont(fonts.TitleBold).SetFontSize(20).SetFontColor(Guinda).SetMargin(0)));
        table.AddCell(new Cell().SetBorder(Border.NO_BORDER).SetPadding(14).SetBackgroundColor(GrisClaro)
            .Add(new Paragraph(title).SetFont(fonts.BodyBold).SetFontSize(12).SetMargin(0)));
        table.AddCell(new Cell().SetBorder(Border.NO_BORDER).SetPadding(14).SetBackgroundColor(GrisClaro)
            .Add(new Paragraph(detail).SetFont(fonts.Body).SetFontSize(9).SetFontColor(Gris).SetFixedLeading(12).SetMargin(0)));
    }

    private static void AddHeaderCell(Table table, Fonts fonts, string text)
    {
        table.AddHeaderCell(new Cell().SetBorder(Border.NO_BORDER).SetBorderBottom(new SolidBorder(Guinda, 1f)).SetPadding(5)
            .Add(new Paragraph(text).SetFont(fonts.BodyBold).SetFontSize(7.5f).SetFontColor(Guinda).SetMargin(0)));
    }

    private static Cell TextCell(Fonts fonts, string text, float size, bool bold, TextAlignment alignment = TextAlignment.LEFT)
    {
        return new Cell().SetBorder(Border.NO_BORDER).SetBorderBottom(new SolidBorder(GrisLinea, .55f)).SetPadding(7)
            .SetTextAlignment(alignment).SetVerticalAlignment(VerticalAlignment.MIDDLE)
            .Add(new Paragraph(string.IsNullOrWhiteSpace(text) ? "-" : text).SetFont(bold ? fonts.BodyBold : fonts.Body).SetFontSize(size).SetMargin(0));
    }

    private static Cell LogoCell(ImageData? data)
    {
        var cell = new Cell().SetBorder(Border.NO_BORDER).SetVerticalAlignment(VerticalAlignment.MIDDLE).SetPadding(0);
        if (data != null) cell.Add(new Image(data).SetAutoScale(true).SetMaxHeight(20));
        return cell;
    }

    private sealed class FooterEventHandler(PdfFont font, DateTime fechaCorte) : IEventHandler
    {
        public void HandleEvent(Event currentEvent)
        {
            var documentEvent = (PdfDocumentEvent)currentEvent;
            var pdf = documentEvent.GetDocument();
            var page = documentEvent.GetPage();
            var index = pdf.GetPageNumber(page);
            var canvas = new PdfCanvas(page.NewContentStreamAfter(), page.GetResources(), pdf);
            canvas.SetStrokeColor(GrisLinea).SetLineWidth(.5f).MoveTo(48, 23).LineTo(912, 23).Stroke();
            canvas.BeginText().SetFontAndSize(font, 6.8f).SetColor(Gris, true).MoveText(48, 10)
                .ShowText($"Fuente: SENER · CFE · CENACE. Resultados de Mesa Técnica al {fechaCorte:dd/MM/yyyy}. Capacidad en MW netos.")
                .EndText();
            canvas.BeginText().SetFontAndSize(font, 7).SetColor(Guinda, true).MoveText(895, 10)
                .ShowText(index.ToString("00", EsMx)).EndText();
            canvas.Release();
        }
    }

    private static Fonts LoadFonts(string webRootPath)
    {
        var title = System.IO.Path.Combine(webRootPath, "tablero", "fonts", "patria", "Patria-Regular.ttf");
        var titleBold = System.IO.Path.Combine(webRootPath, "tablero", "fonts", "patria", "Patria-Bold.ttf");
        var body = System.IO.Path.Combine(webRootPath, "fonts", "Noto Sans ENERGIA", "NotoSans-Regular.ttf");
        var bodyBold = System.IO.Path.Combine(webRootPath, "fonts", "Noto Sans ENERGIA", "NotoSans-Bold.ttf");
        return new Fonts(
            PdfFontFactory.CreateFont(title, PdfEncodings.IDENTITY_H, PdfFontFactory.EmbeddingStrategy.PREFER_EMBEDDED),
            PdfFontFactory.CreateFont(titleBold, PdfEncodings.IDENTITY_H, PdfFontFactory.EmbeddingStrategy.PREFER_EMBEDDED),
            PdfFontFactory.CreateFont(body, PdfEncodings.IDENTITY_H, PdfFontFactory.EmbeddingStrategy.PREFER_EMBEDDED),
            PdfFontFactory.CreateFont(bodyBold, PdfEncodings.IDENTITY_H, PdfFontFactory.EmbeddingStrategy.PREFER_EMBEDDED));
    }

    private static Logos LoadLogos(string webRootPath)
    {
        ImageData? Load(string file)
        {
            var path = System.IO.Path.Combine(webRootPath, "tablero", "assets", file);
            return File.Exists(path) ? ImageDataFactory.Create(path) : null;
        }
        return new Logos(Load("logo_gob.png"), Load("logo_sener.png"));
    }

    private static bool IsFactible(CarteraConvocatoriaProyecto project) => NormalizeClass(project.AnalysisClassification) == "Factible";

    private static string NormalizeClass(string? value) => value switch
    {
        "Más factible" => "Factible",
        "Pendiente en análisis" => "En análisis",
        _ => value?.Trim() ?? "Sin clasificación"
    };

    private static int CountDistinctProjects(IEnumerable<CarteraConvocatoriaProyecto> projects) => projects
        .Select(project => string.IsNullOrWhiteSpace(project.DuplicateGroup) ? project.Folio : project.DuplicateGroup)
        .Distinct(StringComparer.OrdinalIgnoreCase)
        .Count();

    private static int RegionOrder(string? region) => region switch
    {
        "Noreste" => 1,
        "Occidental" => 2,
        "Oriental" => 3,
        "Central" => 4,
        _ => 99
    };

    private static string NormalizeState(string? state) => state switch
    {
        "Estado De México" => "México",
        "Coahuila De Zaragoza" => "Coahuila",
        "Michoacán De Ocampo" => "Michoacán",
        "San Luis Potosí" => "S. L. Potosí",
        _ => state ?? "Sin entidad"
    };

    private static string TechnologyCode(string? technology) => technology?.ToLowerInvariant() switch
    {
        var value when value != null && value.Contains("eólic") => "EO",
        var value when value != null && value.Contains("hidrául") => "HID",
        _ => "FV"
    };

    private static string FormatMw(decimal value) => decimal.Round(value, 0, MidpointRounding.AwayFromZero).ToString("N0", EsMx);

    private static string FormatMwDetail(decimal value) => value.ToString("N1", EsMx);

    private static string Shorten(string? value, int length)
    {
        if (string.IsNullOrWhiteSpace(value)) return "-";
        var clean = value.Trim();
        return clean.Length <= length ? clean : clean[..(length - 1)] + "…";
    }

    private sealed record Fonts(PdfFont Title, PdfFont TitleBold, PdfFont Body, PdfFont BodyBold);
    private sealed record Logos(ImageData? Gobierno, ImageData? Sener);
    private sealed record ReportRow(string Name, string Folios, string Type, string State, string? Technology, string? InterestGroup, decimal Mw, string Analysis);
}
