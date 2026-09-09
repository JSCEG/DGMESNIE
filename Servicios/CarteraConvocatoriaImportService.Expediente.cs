using System.Globalization;
using ClosedXML.Excel;
using NSIE.Models.ProyectosPrivados;

namespace NSIE.Servicios;

public sealed partial class CarteraConvocatoriaImportService
{
    // Encabezados normalizados (mayúsculas, sin acentos; duplicados llevan sufijo #n).
    private static readonly HashSet<string> ExcludedColumns = new(StringComparer.OrdinalIgnoreCase)
        { "ID", "PRE FOLIO", "RFC", "CAPTURISTA", "NUMERO DE CUENTA BANCARIA" };

    private static bool IsExcludedColumn(string sheet, string normalizedHeader)
    {
        var key = normalizedHeader.Split('#')[0];
        return (sheet is "BD_MIXTOS_II" or "CATALOGO DE PROYECTOS" or "BD_MIXTOS_I") && ExcludedColumns.Contains(key);
    }

    // "21.4306, -101.9770" → [lat, lon] normalizado al territorio nacional; null si no es coordenada.
    private static double[]? Vertex(string text)
    {
        if (string.IsNullOrWhiteSpace(text) || IsMissing(text)) return null;
        var parts = text.Split(',', StringSplitOptions.TrimEntries | StringSplitOptions.RemoveEmptyEntries);
        if (parts.Length != 2 ||
            !decimal.TryParse(parts[0], NumberStyles.Any, CultureInfo.InvariantCulture, out var latitude) ||
            !decimal.TryParse(parts[1], NumberStyles.Any, CultureInfo.InvariantCulture, out var longitude)) return null;
        var normalized = NormalizeCoordinatePair(latitude, longitude);
        return normalized.Latitude.HasValue && normalized.Longitude.HasValue
            ? new[] { decimal.ToDouble(normalized.Latitude.Value), decimal.ToDouble(normalized.Longitude.Value) }
            : null;
    }

    // Antecedente de la convocatoria anterior: BD_MIXTOS_I se liga por "Folio MIXTOS I" del catálogo
    // (o por su columna LLAVE cuando ya trae el folio Mixtos II conciliado).
    private static void AttachMixtosIBackground(XLWorkbook book, Dictionary<string, CarteraConvocatoriaExpediente> dossiers)
    {
        if (!book.TryGetWorksheet("BD_MIXTOS_I", out var sheet)) return;
        var (headerRow, headers) = FindHeaders(sheet, 1, "Folio");
        if (!headers.TryGetValue("FOLIO", out var folioColumn)) return;
        headers.TryGetValue("LLAVE", out var keyColumn);
        var byFolio = new Dictionary<string, IXLRow>(StringComparer.OrdinalIgnoreCase);
        var byKey = new Dictionary<string, IXLRow>(StringComparer.OrdinalIgnoreCase);
        var last = sheet.LastRowUsed()?.RowNumber() ?? headerRow;
        for (int number = headerRow + 1; number <= last; number++)
        {
            var row = sheet.Row(number);
            var folio = Text(row.Cell(folioColumn));
            if (!string.IsNullOrWhiteSpace(folio)) byFolio.TryAdd(folio, row);
            if (keyColumn > 0)
            {
                var key = Text(row.Cell(keyColumn));
                if (!string.IsNullOrWhiteSpace(key) && !IsMissing(key)) byKey.TryAdd(key, row);
            }
        }
        foreach (var dossier in dossiers.Values)
        {
            var background = dossier.Records.FirstOrDefault(r => r.Sheet == "CATALOGO DE PROYECTOS")?
                .Fields.FirstOrDefault(f => Normalize(f.Name) == "FOLIO MIXTOS I")?.Value;
            IXLRow? row = null;
            if (!byKey.TryGetValue(dossier.Folio, out row) && !string.IsNullOrWhiteSpace(background) && !IsMissing(background))
                byFolio.TryGetValue(background.Trim(), out row);
            if (row == null) continue;
            var record = new ConvocatoriaRegistroFuente { Sheet = "BD_MIXTOS_I", Row = row.RowNumber() };
            foreach (var header in headers)
            {
                if (IsExcludedColumn("BD_MIXTOS_I", header.Key)) continue;
                var cell = row.Cell(header.Value);
                var sourceValue = cell.HasFormula ? cell.CachedValue : cell.Value;
                var value = sourceValue.IsDateTime ? sourceValue.GetDateTime().ToString("yyyy-MM-dd") : Text(cell);
                record.Fields.Add(new ConvocatoriaCampoFuente {
                    Name = Text(sheet.Cell(1, header.Value)), Column = cell.Address.ColumnLetter,
                    Value = string.IsNullOrWhiteSpace(value) ? null : value
                });
            }
            dossier.Records.Add(record);
        }
    }

    // Requerimiento de capacidad por sistema (SIN, SIBC, SIBCS, TOTAL) de la hoja MAPA REQUERIMIENTOS.
    private static List<CarteraConvocatoriaRequerimientoSistema> ReadSystemRequirements(XLWorkbook book)
    {
        var requirements = new List<CarteraConvocatoriaRequerimientoSistema>();
        if (!book.TryGetWorksheet("MAPA REQUERIMIENTOS", out var sheet)) return requirements;
        var (headerRow, headers) = FindHeaders(sheet, 5, "Sistema");
        if (!headers.TryGetValue("SISTEMA", out var systemColumn)) return requirements;
        decimal? Value(IXLRow row, string header) => headers.TryGetValue(Normalize(header), out var column) ? Number(row.Cell(column)) : null;
        var last = Math.Min(sheet.LastRowUsed()?.RowNumber() ?? headerRow, headerRow + 35);
        for (int number = headerRow + 1; number <= last; number++)
        {
            var row = sheet.Row(number);
            var system = Text(row.Cell(systemColumn));
            if (string.IsNullOrWhiteSpace(system)) break;
            if (Normalize(system) is not ("SIN" or "SIBC" or "SIBCS" or "TOTAL")) break;
            requirements.Add(new CarteraConvocatoriaRequerimientoSistema
            {
                System = system.Trim(),
                RequirementMw = Value(row, "REQUERIMIENTO [MW]"),
                WindMw = Value(row, "EÓLICO"),
                SolarMw = Value(row, "FOTOVOLTAICO"),
                FeasibleMw = Value(row, "FACTIBLE REGISTRADO Y PRESELECCIONADO [MW]") ?? Value(row, "FACTIBLE REGISTRADO Y PRESELECCIONADO"),
                ShortfallMw = Value(row, "FALTANTE [MW]"),
                PreselectedMw = Value(row, "PRELIMINAR PRESELECCIONADO [MW]"),
                TotalMw = Value(row, "TOTAL")
            });
        }
        return requirements;
    }

    private static Dictionary<string, CarteraConvocatoriaExpediente> ReadDossiers(XLWorkbook book,
        List<CarteraConvocatoriaProyecto> projects, string fileName, string sha256, DateTime? cutoff)
    {
        var dossiers = projects.ToDictionary(p => p.Folio, p => new CarteraConvocatoriaExpediente
        { Folio = p.Folio, FileName = fileName, Sha256 = sha256, CutoffDate = cutoff }, StringComparer.OrdinalIgnoreCase);
        // Esquema v5: el expediente conserva el catálogo completo (antecedentes Mixtos I / CVP2, origen,
        // preselección, depuración, comentarios) y la solicitud completa de BD_MIXTOS_II —modalidad,
        // representante y datos de contacto, capacidad técnica y financiera, interconexión, etapas,
        // datos técnicos por tecnología, SAE, CAPEX/OPEX, trámites y validación CENACE— porque la
        // ficha ejecutiva los presenta. Se excluyen identificadores fiscales/bancarios y el capturista;
        // los vértices se compactan como polígonos (ProjectPolygon / SubstationPolygon).
        var sources = new (string Sheet, int Header, string Key, string[]? Fields)[] {
            ("CATALOGO DE PROYECTOS",3,"LLAVE",null),
            ("BD_MIXTOS_II",2,"Folio LLAVE",null),
            ("BD_PRESELECCIONADOS",1,"Folio",null),
            ("BD_COSTOS CFE",1,"Folio LLAVE",null),
            ("BD_COSTOS_OBRAS",1,"Folio LLAVE",null),
            ("BD_IMPACTO_SOCIAL",1,"Folio LLAVE",null),
            ("BD_FACTIBILIDAD",1,"Folio LLAVE",null),
            ("BD_INTERES_CFE",4,"Folio LLAVE",null)
        };
        foreach (var source in sources)
        {
            if (!book.TryGetWorksheet(source.Sheet, out var sheet)) continue;
            var (sheetHeaderRow, headers) = FindHeaders(sheet, source.Header, source.Key);
            if (!headers.ContainsKey(Normalize(source.Key))) continue;
            // Vértices de BD_MIXTOS_II: el segundo bloque "Latitud/Longitud/Vértice" corresponde a la subestación.
            var substationStart = source.Sheet == "BD_MIXTOS_II" && headers.TryGetValue("LATITUD#2", out var secondLatitude) ? secondLatitude : int.MaxValue;
            var included = source.Fields?.Select(Normalize).ToHashSet(StringComparer.OrdinalIgnoreCase);
            var last = sheet.LastRowUsed()?.RowNumber() ?? sheetHeaderRow;
            for (int number = sheetHeaderRow + 1; number <= last; number++)
            {
                var row = sheet.Row(number);
                var key = CellText(row, headers, source.Key);
                if (!dossiers.TryGetValue(key, out var dossier)) continue;
                var record = new ConvocatoriaRegistroFuente { Sheet = source.Sheet, Row = number };
                var projectVertices = new List<double[]>();
                var substationVertices = new List<double[]>();
                foreach (var header in headers)
                {
                    if (included != null && !included.Contains(header.Key)) continue;
                    if (source.Sheet == "BD_MIXTOS_II" && header.Key.StartsWith("VERTICE ", StringComparison.OrdinalIgnoreCase))
                    {
                        var vertex = Vertex(Text(row.Cell(header.Value)));
                        if (vertex != null) (header.Value < substationStart ? projectVertices : substationVertices).Add(vertex);
                        continue;
                    }
                    if (IsExcludedColumn(source.Sheet, header.Key)) continue;
                    // El tipo de cambio es metadato del libro, no columna de proyecto.
                    if (source.Sheet == "BD_COSTOS CFE" && header.Value > 32) continue;
                    var cell = row.Cell(header.Value);
                    var sourceValue = cell.HasFormula ? cell.CachedValue : cell.Value;
                    var value = sourceValue.IsDateTime ? sourceValue.GetDateTime().ToString("yyyy-MM-dd") : Text(cell);
                    var link = cell.HasHyperlink ? cell.GetHyperlink().ExternalAddress?.OriginalString : null;
                    if (cell.HasFormula && cell.FormulaA1.StartsWith("HYPERLINK(", StringComparison.OrdinalIgnoreCase))
                    {
                        var match = System.Text.RegularExpressions.Regex.Match(cell.FormulaA1, "^HYPERLINK\\(\"([^\"]+)\"", System.Text.RegularExpressions.RegexOptions.IgnoreCase);
                        if (match.Success) link = match.Groups[1].Value;
                    }
                    record.Fields.Add(new ConvocatoriaCampoFuente {
                        Name = Text(sheet.Cell(source.Header, header.Value)), Column = cell.Address.ColumnLetter,
                        Value = string.IsNullOrWhiteSpace(value) ? null : value, Link = link
                    });
                }
                dossier.Records.Add(record); // Se conservan registros múltiples de evaluación por folio.
                if (source.Sheet == "BD_MIXTOS_II")
                {
                    // Un polígono requiere al menos tres vértices válidos; con menos se conserva sólo la coordenada.
                    if (projectVertices.Count >= 3) dossier.ProjectPolygon = projectVertices;
                    if (substationVertices.Count >= 3) dossier.SubstationPolygon = substationVertices;
                }
            }
        }
        AttachMixtosIBackground(book, dossiers);
        var requirements = ReadSystemRequirements(book);
        foreach (var dossier in dossiers.Values) dossier.SystemRequirements = requirements;
        if (book.TryGetWorksheet("BD_COSTOS CFE", out var costs))
        {
            var rateLabel = costs.Row(1).CellsUsed().FirstOrDefault(c => Normalize(Text(c)) == "TIPO DE CAMBIO ($/USD)");
            var rateCell = rateLabel?.CellRight();
            foreach (var dossier in dossiers.Values)
            {
                dossier.ExchangeRate = Number(rateCell);
                dossier.ExchangeRateSource = rateCell == null ? "" : "BD_COSTOS CFE!" + rateCell.Address.ToStringRelative();
            }
        }
        foreach (var project in projects)
        {
            var dossier = dossiers[project.Folio];
            var works = dossier.Records.Where(r => r.Sheet == "BD_COSTOS_OBRAS").ToList();
            if (project.WorksCount.HasValue && project.WorksCount.Value != works.Count)
                dossier.Warnings.Add($"La fuente CFE reporta {project.WorksCount} obras; la hoja de desglose contiene {works.Count}. Pendiente de conciliación.");
            var amounts = works.Select(r => r.Fields.FirstOrDefault(f => f.Name == "Costo de red de la obra (MDD)")?.Value)
                .Select(v => decimal.TryParse(v,System.Globalization.NumberStyles.Any,System.Globalization.CultureInfo.InvariantCulture,out var n) ? (decimal?)n : null).ToList();
            if (project.NetworkCostUsd.HasValue && amounts.Any(v => v.HasValue) && Math.Abs(amounts.Sum(v => v ?? 0)-project.NetworkCostUsd.Value) > 0.0001m)
                dossier.Warnings.Add("El total de la ficha CFE no coincide con la suma de importes disponibles por obra. No se ajustan ni se prorratean los datos de origen.");
        }
        return dossiers;
    }
}
