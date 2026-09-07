using System.Globalization;
using System.Security.Cryptography;
using System.Text;
using System.Text.RegularExpressions;
using ClosedXML.Excel;
using NSIE.Models.ProyectosPrivados;
using NSIE.Servicios.Interfaces;

namespace NSIE.Servicios;

public sealed partial class CarteraConvocatoriaImportService : ICarteraConvocatoriaImportService
{
    private const string CatalogSheet = "CATALOGO DE PROYECTOS";
    private const string SourceSheet = "BD_MIXTOS_II";
    private const int CatalogHeaderRow = 3;
    private const int SourceHeaderRow = 2;

    public async Task<CarteraConvocatoriaImportDocument> LeerAsync(
        Stream stream,
        string fileName,
        CancellationToken cancellationToken = default)
    {
        ArgumentNullException.ThrowIfNull(stream);
        if (string.IsNullOrWhiteSpace(fileName))
            throw new InvalidDataException("El archivo debe tener nombre.");

        await using var buffer = new MemoryStream();
        await stream.CopyToAsync(buffer, cancellationToken);
        if (buffer.Length == 0)
            throw new InvalidDataException("El archivo está vacío.");

        var bytes = buffer.ToArray();
        if (bytes.Length < 4 || bytes[0] != 0x50 || bytes[1] != 0x4B)
            throw new InvalidDataException("El archivo debe ser un libro Excel .xlsx o .xlsm válido.");

        var sha256 = Convert.ToHexString(SHA256.HashData(bytes)).ToLowerInvariant();
        buffer.Position = 0;

        using var workbook = new XLWorkbook(buffer);
        if (!workbook.TryGetWorksheet(CatalogSheet, out var catalog) || catalog == null)
            throw new InvalidDataException($"Falta la hoja obligatoria «{CatalogSheet}».");
        if (!workbook.TryGetWorksheet(SourceSheet, out var source) || source == null)
            throw new InvalidDataException($"Falta la hoja obligatoria «{SourceSheet}».");

        var catalogHeaders = BuildHeaderMap(catalog, CatalogHeaderRow);
        var sourceHeaders = BuildHeaderMap(source, SourceHeaderRow);
        ValidateHeaders(catalogHeaders, "catálogo", "LLAVE", "Considerar", "Proyecto", "REGIÓN (GCR)", "MW NETOS", "ESTATUS UNIVERSO", "FOLIO CONSIDERADO (llave única)");
        ValidateHeaders(sourceHeaders, "base fuente", "Folio LLAVE", "Nombre", "Proyecto", "Tipo Tecnología", "Región", "Entidad Federativa", "Subestación Eléctrica de Interconexión", "Punto de Interconexión", "Capacidad Generación en MW netos", "Municipio/Alcaldía", "Latitud", "Longitud", "Archivo KMZ proyecto", "Archivo KMZ subestacion");

        var sourceRows = ReadSourceRows(source, sourceHeaders);
        var projects = new List<CarteraConvocatoriaProyecto>();
        var lastCatalogRow = catalog.LastRowUsed()?.RowNumber() ?? CatalogHeaderRow;

        for (var rowNumber = CatalogHeaderRow + 1; rowNumber <= lastCatalogRow; rowNumber++)
        {
            cancellationToken.ThrowIfCancellationRequested();
            var row = catalog.Row(rowNumber);
            var key = CellText(row, catalogHeaders, "LLAVE");
            var canonicalFolio = CellText(row, catalogHeaders, "FOLIO CONSIDERADO (llave única)");
            if (string.IsNullOrWhiteSpace(key)) continue;
            if (string.IsNullOrWhiteSpace(canonicalFolio)) canonicalFolio = key;

            if (!sourceRows.TryGetValue(key, out var raw))
                throw new InvalidDataException($"El folio {key} del catálogo no existe en {SourceSheet}.");

            var consider = CellText(row, catalogHeaders, "Considerar");
            var universeStatus = CellText(row, catalogHeaders, "ESTATUS UNIVERSO");
            var consideration = ResolveConsideration(consider, universeStatus);
            var project = new CarteraConvocatoriaProyecto
            {
                Folio = key,
                CanonicalFolio = canonicalFolio,
                Name = First(CellText(row, catalogHeaders, "Proyecto"), CellText(raw, sourceHeaders, "Proyecto"), key),
                Type = "Estratégico",
                Region = First(CellText(row, catalogHeaders, "REGIÓN (GCR)"), CellText(raw, sourceHeaders, "Región"), "Sin región"),
                State = First(CellText(raw, sourceHeaders, "Entidad Federativa"), "Sin información"),
                Technology = First(CellText(row, catalogHeaders, "TECNOLOGÍA"), CellText(raw, sourceHeaders, "Tipo Tecnología")),
                Company = CleanOptional(CellText(raw, sourceHeaders, "Nombre")),
                InterestGroup = CleanOptional(CellText(raw, sourceHeaders, "Grupo de Interés")),
                Substation = CleanOptional(CellText(raw, sourceHeaders, "Subestación Eléctrica de Interconexión")),
                InterconnectionPoint = CleanOptional(CellText(raw, sourceHeaders, "Punto de Interconexión")),
                Mw = CellNumber(row, catalogHeaders, "MW NETOS") ?? CellNumber(raw, sourceHeaders, "Capacidad Generación en MW netos") ?? 0,
                Rank = CellInteger(row, catalogHeaders, "Prelación") ?? rowNumber - CatalogHeaderRow,
                Priority = 4,
                Decision = consideration switch { "firme" => "continua", "no-va" => "no-continua", _ => "revision" },
                Consideration = consideration,
                UniverseStatus = CleanOptional(universeStatus),
                AnalysisClassification = CleanOptional(CellText(row, catalogHeaders, "CLASIFICACIÓN")),
                TechnicalViability = CleanOptional(CellText(row, catalogHeaders, "¿FACTIBLE?")),
                TechnicalAnalysis = CleanOptional(CellText(row, catalogHeaders, "OBSERVACIÓN DEL ÁREA")),
                DuplicateGroup = CleanOptional(CellText(row, catalogHeaders, "FOLIO(S) DUPLICADO(S)")),
                Municipality = CleanOptional(CellText(raw, sourceHeaders, "Municipio/Alcaldía")),
                Latitude = CellNumber(raw, sourceHeaders, "Latitud"),
                Longitude = CellNumber(raw, sourceHeaders, "Longitud"),
                SubstationLatitude = CellNumber(raw, sourceHeaders, "Latitud", 2),
                SubstationLongitude = CellNumber(raw, sourceHeaders, "Longitud", 2),
                ProjectKmlUrl = Url(CellText(raw, sourceHeaders, "Archivo KMZ proyecto")),
                SubstationKmlUrl = Url(CellText(raw, sourceHeaders, "Archivo KMZ subestacion")),
                NetworkCostUsd = CellNumber(row, catalogHeaders, "COSTO DE RED (MDD)"),
                NetworkCostMxn = CellNumber(row, catalogHeaders, "COSTO DE RED (MDP)"),
                WorksCount = CellInteger(row, catalogHeaders, "NÚMERO DE OBRAS"),
                WorksDescription = CleanOptional(CellText(row, catalogHeaders, "OBRAS")),
                ProjectSignedAt = CellDate(raw, sourceHeaders, "Fecha Firma Proyecto"),
                Source = "Mixtos II",
                SourceRow = rowNumber
            };

            NormalizeCoordinates(project);
            projects.Add(project);
        }

        if (projects.Count == 0)
            throw new InvalidDataException("El catálogo no contiene proyectos importables.");
        if (projects.Select(project => project.Folio).Distinct(StringComparer.OrdinalIgnoreCase).Count() != projects.Count)
            throw new InvalidDataException("El catálogo contiene folios considerados duplicados.");

        var cutoffDate = Date(catalog.Cell(2, 13));
        return new CarteraConvocatoriaImportDocument
        {
            FileName = Path.GetFileName(fileName),
            FileBytes = bytes.LongLength,
            Sha256 = sha256,
            SourceVersion = BuildSourceVersion(fileName, cutoffDate),
            CutoffDate = cutoffDate,
            SourceRows = sourceRows.Count,
            Dossiers = ReadDossiers(workbook, projects, Path.GetFileName(fileName), sha256, cutoffDate),
            Projects = projects
        };
    }

    private static Dictionary<string, IXLRow> ReadSourceRows(IXLWorksheet sheet, IReadOnlyDictionary<string, int> headers)
    {
        var rows = new Dictionary<string, IXLRow>(StringComparer.OrdinalIgnoreCase);
        var last = sheet.LastRowUsed()?.RowNumber() ?? SourceHeaderRow;
        for (var rowNumber = SourceHeaderRow + 1; rowNumber <= last; rowNumber++)
        {
            var row = sheet.Row(rowNumber);
            var key = CellText(row, headers, "Folio LLAVE");
            if (string.IsNullOrWhiteSpace(key)) continue;
            if (!rows.TryAdd(key, row))
                throw new InvalidDataException($"La base fuente contiene el folio duplicado {key}.");
        }
        return rows;
    }

    private static Dictionary<string, int> BuildHeaderMap(IXLWorksheet sheet, int headerRow)
    {
        var map = new Dictionary<string, int>(StringComparer.OrdinalIgnoreCase);
        var lastColumn = sheet.Row(headerRow).LastCellUsed()?.Address.ColumnNumber ?? 0;
        var occurrences = new Dictionary<string, int>(StringComparer.OrdinalIgnoreCase);
        for (var column = 1; column <= lastColumn; column++)
        {
            var normalized = Normalize(Text(sheet.Cell(headerRow, column)));
            if (string.IsNullOrWhiteSpace(normalized)) continue;
            occurrences[normalized] = occurrences.GetValueOrDefault(normalized) + 1;
            map[occurrences[normalized] == 1 ? normalized : $"{normalized}#{occurrences[normalized]}"] = column;
        }
        return map;
    }

    private static void ValidateHeaders(IReadOnlyDictionary<string, int> headers, string label, params string[] required)
    {
        var missing = required.Where(header => !headers.ContainsKey(Normalize(header))).ToArray();
        if (missing.Length > 0)
            throw new InvalidDataException($"La {label} no contiene las columnas requeridas: {string.Join(", ", missing)}.");
    }

    private static IXLCell? Cell(IXLRow row, IReadOnlyDictionary<string, int> headers, string header, int occurrence = 1)
    {
        var key = Normalize(header) + (occurrence > 1 ? $"#{occurrence}" : string.Empty);
        return headers.TryGetValue(key, out var column) ? row.Cell(column) : null;
    }

    private static string CellText(IXLRow row, IReadOnlyDictionary<string, int> headers, string header, int occurrence = 1) =>
        Text(Cell(row, headers, header, occurrence));

    private static decimal? CellNumber(IXLRow row, IReadOnlyDictionary<string, int> headers, string header, int occurrence = 1) =>
        Number(Cell(row, headers, header, occurrence));

    private static int? CellInteger(IXLRow row, IReadOnlyDictionary<string, int> headers, string header, int occurrence = 1) =>
        Integer(Cell(row, headers, header, occurrence));

    private static DateTime? CellDate(IXLRow row, IReadOnlyDictionary<string, int> headers, string header) =>
        Date(Cell(row, headers, header));

    private static string Text(IXLCell? cell)
    {
        if (cell == null) return string.Empty;
        var value = cell.HasFormula ? cell.CachedValue : cell.Value;
        return value.ToString(CultureInfo.InvariantCulture).Trim();
    }

    private static decimal? Number(IXLCell? cell)
    {
        var text = Text(cell);
        if (string.IsNullOrWhiteSpace(text) || IsMissing(text)) return null;
        if (decimal.TryParse(text, NumberStyles.Any, CultureInfo.InvariantCulture, out var invariant)) return invariant;
        if (decimal.TryParse(text, NumberStyles.Any, CultureInfo.GetCultureInfo("es-MX"), out var spanish)) return spanish;
        return null;
    }

    private static int? Integer(IXLCell? cell)
    {
        var value = Number(cell);
        return value.HasValue && value >= int.MinValue && value <= int.MaxValue
            ? decimal.ToInt32(decimal.Truncate(value.Value))
            : null;
    }

    private static DateTime? Date(IXLCell? cell)
    {
        if (cell == null) return null;
        var value = cell.HasFormula ? cell.CachedValue : cell.Value;
        if (value.IsDateTime) return value.GetDateTime();
        if (value.IsNumber && value.GetNumber() is > 1 and < 2958466) return DateTime.FromOADate(value.GetNumber());
        var text = value.ToString(CultureInfo.InvariantCulture);
        return DateTime.TryParse(text, CultureInfo.GetCultureInfo("es-MX"), DateTimeStyles.AssumeLocal, out var parsed)
            ? parsed.Date
            : null;
    }

    private static string ResolveConsideration(string consider, string universeStatus)
    {
        if (consider.Trim() == "1") return "firme";
        var normalized = Normalize(universeStatus);
        if (normalized.Contains("DESECHADO", StringComparison.Ordinal) || normalized.Contains("DUPLICADO RETIRADO", StringComparison.Ordinal))
            return "no-va";
        return "revision";
    }

    private static string? Url(string value)
    {
        if (!Uri.TryCreate(value, UriKind.Absolute, out var uri)) return null;
        return uri.Scheme == Uri.UriSchemeHttps && uri.Host.Equals("ventanillainstituciones.energia.gob.mx", StringComparison.OrdinalIgnoreCase)
            ? uri.AbsoluteUri
            : null;
    }

    private static void NormalizeCoordinates(CarteraConvocatoriaProyecto project)
    {
        (project.Latitude, project.Longitude) = NormalizeCoordinatePair(project.Latitude, project.Longitude);
        (project.SubstationLatitude, project.SubstationLongitude) = NormalizeCoordinatePair(
            project.SubstationLatitude,
            project.SubstationLongitude);

        if (!project.Latitude.HasValue && project.SubstationLatitude.HasValue)
        {
            project.Latitude = project.SubstationLatitude;
            project.Longitude = project.SubstationLongitude;
        }
    }

    private static (decimal? Latitude, decimal? Longitude) NormalizeCoordinatePair(
        decimal? latitude,
        decimal? longitude)
    {
        if (!latitude.HasValue || !longitude.HasValue ||
            (latitude.Value == 0 && longitude.Value == 0))
            return (null, null);

        var normalizedLatitude = latitude.Value;
        var normalizedLongitude = longitude.Value;
        if (normalizedLatitude is <= -14 and >= -33.5m)
            normalizedLatitude = Math.Abs(normalizedLatitude);
        if (normalizedLongitude is >= 86 and <= 119)
            normalizedLongitude = -normalizedLongitude;

        return normalizedLatitude is >= 14 and <= 33.5m &&
               normalizedLongitude is >= -119 and <= -86
            ? (normalizedLatitude, normalizedLongitude)
            : (null, null);
    }

    private static string BuildSourceVersion(string fileName, DateTime? cutoffDate)
    {
        var stem = Path.GetFileNameWithoutExtension(fileName).Trim();
        return cutoffDate.HasValue ? $"{stem} · corte {cutoffDate:yyyy-MM-dd}" : stem;
    }

    private static string Normalize(string value)
    {
        if (string.IsNullOrWhiteSpace(value)) return string.Empty;
        var decomposed = value.Trim().Normalize(NormalizationForm.FormD);
        var builder = new StringBuilder(decomposed.Length);
        foreach (var character in decomposed)
        {
            if (CharUnicodeInfo.GetUnicodeCategory(character) != UnicodeCategory.NonSpacingMark)
                builder.Append(char.ToUpperInvariant(character));
        }
        return Regex.Replace(builder.ToString().Normalize(NormalizationForm.FormC), @"\s+", " ");
    }

    private static string First(params string?[] values) =>
        values.Select(CleanOptional).FirstOrDefault(value => !string.IsNullOrWhiteSpace(value)) ?? string.Empty;

    private static string? CleanOptional(string? value)
    {
        var clean = value?.Trim();
        return string.IsNullOrWhiteSpace(clean) || IsMissing(clean) ? null : clean;
    }

    private static bool IsMissing(string value)
    {
        var normalized = Normalize(value);
        return normalized is "-" or "SIN INFORMACION" or "NO APLICA";
    }
}
