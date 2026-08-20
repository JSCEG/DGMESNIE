using System.Globalization;
using System.IO.Compression;
using System.Security.Cryptography;
using System.Text;
using System.Text.RegularExpressions;
using System.Xml.Linq;
using ClosedXML.Excel;
using NSIE.Models.PlaneacionVinculante;

namespace NSIE.Servicios.PlaneacionVinculante
{
    public interface IPvircePreviewService
    {
        Task<PvircePreviewResult> PrevisualizarAsync(
            Stream archivo,
            string nombreArchivo,
            CancellationToken cancellationToken = default);
    }

    public sealed class PvircePreviewService : IPvircePreviewService
    {
        private const long MaximoArchivoComprimido = 25L * 1024 * 1024;
        private const long MaximoArchivoDescomprimido = 300L * 1024 * 1024;
        private const int MaximoPartesOffice = 10000;
        private const int MaximoMuestra = 25;

        private static readonly Regex HorizonRegex = new(
            @"(?<inicio>20\d{2})\s*[-–—]\s*(?<fin>20\d{2})",
            RegexOptions.Compiled | RegexOptions.CultureInvariant);

        public async Task<PvircePreviewResult> PrevisualizarAsync(
            Stream archivo,
            string nombreArchivo,
            CancellationToken cancellationToken = default)
        {
            if (archivo == null || !archivo.CanRead)
                throw new InvalidOperationException("El archivo PVIRCE no se puede leer.");

            if (!string.Equals(Path.GetExtension(nombreArchivo), ".xlsx", StringComparison.OrdinalIgnoreCase))
                throw new InvalidOperationException("La previsualización PVIRCE sólo admite archivos .xlsx.");

            await using var buffer = new MemoryStream();
            await archivo.CopyToAsync(buffer, cancellationToken);
            if (buffer.Length == 0)
                throw new InvalidOperationException("El archivo PVIRCE está vacío.");
            if (buffer.Length > MaximoArchivoComprimido)
                throw new InvalidOperationException("El archivo PVIRCE supera el límite de 25 MB para previsualización.");

            var bytes = buffer.ToArray();
            var hashSha256 = Convert.ToHexString(SHA256.HashData(bytes));
            ValidarPaqueteOffice(buffer);
            var comentariosNormalizados = NormalizarAutorComentariosSiEsNecesario(buffer);
            buffer.Position = 0;

            using var workbook = new XLWorkbook(buffer);
            var profile = DetectarHojaYEncabezados(workbook);
            var worksheet = profile.Worksheet;
            var usedRange = profile.UsedRange;
            var lastRow = usedRange.LastRowUsed().RowNumber();
            var firstColumn = usedRange.FirstColumnUsed().ColumnNumber();
            var lastColumn = usedRange.LastColumnUsed().ColumnNumber();

            var headers = new Dictionary<int, string>();
            var headerColumns = new Dictionary<string, int>(StringComparer.OrdinalIgnoreCase);
            for (var column = firstColumn; column <= lastColumn; column++)
            {
                var header = LeerTextoSeguro(worksheet.Cell(profile.HeaderRow, column));
                headers[column] = header;
                var normalized = NormalizarEncabezado(header);
                if (!string.IsNullOrWhiteSpace(normalized) && !headerColumns.ContainsKey(normalized))
                    headerColumns[normalized] = column;
            }

            var nameColumn = BuscarColumna(headerColumns, "NOMBREREAL")
                ?? throw new InvalidOperationException("La hoja PVIRCE no contiene la columna Nombre real.");
            var yearColumn = BuscarColumna(headerColumns, "ANOEOC", "ANO")
                ?? throw new InvalidOperationException("La hoja PVIRCE no contiene una columna de año reconocida.");
            var installedColumn = BuscarColumna(headerColumns, "CAPACIDADINSTALADAMWAC", "MW")
                ?? throw new InvalidOperationException("La hoja PVIRCE no contiene una columna de capacidad instalada reconocida.");

            var movementColumn = BuscarColumna(headerColumns, "ADICIONESOSUSTITUCIONES");
            var interconnectionColumn = BuscarColumna(headerColumns, "CAPACIDADDEINTERCONEXIONMW");
            var typeVfColumn = BuscarColumna(headerColumns, "TIPOVF");
            var gcrColumn = BuscarColumna(headerColumns, "GERENCIADECONTROL", "GCR");
            var statusColumn = BuscarColumna(headerColumns, "STATUSMACROFINAL", "STATUSVF", "STATUS");
            var statusMacroColumn = BuscarColumna(headerColumns, "STATUSMACROFINAL");
            var cenaceColumn = BuscarColumna(headerColumns, "ESTATUSCENACE");
            var firmColumn = BuscarColumna(headerColumns, "FIRMES");

            var title = DetectarTitulo(worksheet, usedRange.FirstRowUsed().RowNumber(), profile.HeaderRow - 1, firstColumn, lastColumn);
            var (horizonStart, horizonEnd) = DetectarHorizonte(title);
            var parsedRows = new List<ParsedRow>();
            var rowsWithoutName = new List<int>();
            var formulaCounts = new Dictionary<int, int>();
            var filledCounts = new Dictionary<int, int>();
            var brokenFormulaRows = new List<int>();
            var totalFormulas = 0;

            for (var rowNumber = profile.HeaderRow + 1; rowNumber <= lastRow; rowNumber++)
            {
                cancellationToken.ThrowIfCancellationRequested();
                var row = worksheet.Row(rowNumber);
                var hasData = false;

                for (var column = firstColumn; column <= lastColumn; column++)
                {
                    var cell = row.Cell(column);
                    var text = LeerTextoSeguro(cell);
                    if (!string.IsNullOrWhiteSpace(text) || cell.HasFormula)
                        hasData = true;
                    if (!string.IsNullOrWhiteSpace(text))
                        filledCounts[column] = filledCounts.GetValueOrDefault(column) + 1;
                    if (!cell.HasFormula)
                        continue;

                    totalFormulas++;
                    formulaCounts[column] = formulaCounts.GetValueOrDefault(column) + 1;
                    if (cell.FormulaA1.Contains("#REF!", StringComparison.OrdinalIgnoreCase))
                        brokenFormulaRows.Add(rowNumber);
                }

                if (!hasData)
                    continue;

                var name = LeerTextoSeguro(row.Cell(nameColumn));
                if (string.IsNullOrWhiteSpace(name))
                {
                    rowsWithoutName.Add(rowNumber);
                    continue;
                }

                parsedRows.Add(new ParsedRow
                {
                    RowNumber = rowNumber,
                    Name = name,
                    Year = LeerEntero(row.Cell(yearColumn)),
                    Movement = movementColumn.HasValue ? LeerTextoSeguro(row.Cell(movementColumn.Value)) : string.Empty,
                    TypeVf = typeVfColumn.HasValue ? LeerTextoSeguro(row.Cell(typeVfColumn.Value)) : string.Empty,
                    Gcr = gcrColumn.HasValue ? LeerTextoSeguro(row.Cell(gcrColumn.Value)) : string.Empty,
                    Status = statusColumn.HasValue ? LeerTextoSeguro(row.Cell(statusColumn.Value)) : string.Empty,
                    InstalledMw = LeerDecimal(row.Cell(installedColumn)),
                    InterconnectionMw = interconnectionColumn.HasValue ? LeerDecimal(row.Cell(interconnectionColumn.Value)) : null,
                    StatusMacro = statusMacroColumn.HasValue ? LeerTextoSeguro(row.Cell(statusMacroColumn.Value)) : string.Empty,
                    CenaceStatus = cenaceColumn.HasValue ? LeerTextoSeguro(row.Cell(cenaceColumn.Value)) : string.Empty,
                    FirmValue = firmColumn.HasValue ? LeerTextoSeguro(row.Cell(firmColumn.Value)) : string.Empty
                });
            }

            var result = CrearResultado(
                nombreArchivo,
                hashSha256,
                worksheet,
                usedRange,
                profile.HeaderRow,
                title,
                horizonStart,
                horizonEnd,
                headers,
                headerColumns,
                firstColumn,
                lastColumn,
                lastRow,
                parsedRows,
                rowsWithoutName,
                filledCounts,
                formulaCounts,
                totalFormulas,
                brokenFormulaRows,
                interconnectionColumn.HasValue,
                statusMacroColumn.HasValue,
                cenaceColumn.HasValue,
                comentariosNormalizados);

            return result;
        }

        private static PvircePreviewResult CrearResultado(
            string fileName,
            string hashSha256,
            IXLWorksheet worksheet,
            IXLRange usedRange,
            int headerRow,
            string title,
            int? horizonStart,
            int? horizonEnd,
            IReadOnlyDictionary<int, string> headers,
            IReadOnlyDictionary<string, int> headerColumns,
            int firstColumn,
            int lastColumn,
            int lastRow,
            IReadOnlyList<ParsedRow> rows,
            IReadOnlyList<int> rowsWithoutName,
            IReadOnlyDictionary<int, int> filledCounts,
            IReadOnlyDictionary<int, int> formulaCounts,
            int totalFormulas,
            IReadOnlyList<int> brokenFormulaRows,
            bool hasInterconnectionColumn,
            bool hasStatusMacroColumn,
            bool hasCenaceColumn,
            bool commentsNormalized)
        {
            var installedTotal = rows.Sum(row => row.InstalledMw ?? 0m);
            var interconnectionTotal = hasInterconnectionColumn
                ? rows.Sum(row => row.InterconnectionMw ?? 0m)
                : (decimal?)null;
            var mismatches = rows
                .Where(row => row.InstalledMw.HasValue && row.InterconnectionMw.HasValue
                    && Math.Abs(row.InstalledMw.Value - row.InterconnectionMw.Value) > 0.000001m)
                .ToList();
            var duplicateGroups = rows
                .GroupBy(row => NormalizarClave(row.Name), StringComparer.OrdinalIgnoreCase)
                .Where(group => group.Count() > 1)
                .OrderByDescending(group => group.Count())
                .ThenBy(group => group.First().Name, StringComparer.OrdinalIgnoreCase)
                .ToList();
            var outsideHorizon = rows
                .Where(row => row.Year.HasValue
                    && ((horizonStart.HasValue && row.Year.Value < horizonStart.Value)
                        || (horizonEnd.HasValue && row.Year.Value > horizonEnd.Value)))
                .ToList();
            var numericFirmRows = rows
                .Where(row => !string.IsNullOrWhiteSpace(row.FirmValue)
                    && decimal.TryParse(row.FirmValue, NumberStyles.Number, CultureInfo.InvariantCulture, out _))
                .ToList();

            var availableYears = rows
                .Where(row => row.Year.HasValue)
                .Select(row => row.Year!.Value)
                .ToList();

            var result = new PvircePreviewResult
            {
                Archivo = Path.GetFileName(fileName),
                HashSha256 = hashSha256,
                Hoja = worksheet.Name,
                RangoUsado = usedRange.RangeAddress.ToStringRelative(),
                TituloDetectado = title,
                FilaEncabezados = headerRow,
                TotalFilasFisicas = lastRow - headerRow,
                TotalRegistros = rows.Count,
                FilasIgnoradasSinNombre = rowsWithoutName.Count,
                TotalColumnas = lastColumn - firstColumn + 1,
                TotalFormulas = totalFormulas,
                FormulasConReferenciaRota = brokenFormulaRows.Count,
                NombresUnicos = rows.Select(row => NormalizarClave(row.Name)).Distinct(StringComparer.OrdinalIgnoreCase).Count(),
                HorizonteInicio = horizonStart,
                HorizonteFin = horizonEnd,
                AnioMinimo = availableYears.Count > 0 ? availableYears.Min() : null,
                AnioMaximo = availableYears.Count > 0 ? availableYears.Max() : null,
                CapacidadInstaladaMw = decimal.Round(installedTotal, 6),
                CapacidadInterconexionMw = interconnectionTotal.HasValue ? decimal.Round(interconnectionTotal.Value, 6) : null,
                AdicionesMw = decimal.Round(rows.Where(row => EsAdicion(row.Movement)).Sum(row => row.InstalledMw ?? 0m), 6),
                SustitucionesMw = decimal.Round(rows.Where(row => EsSustitucion(row.Movement)).Sum(row => row.InstalledMw ?? 0m), 6),
                FilasCapacidadDiferente = mismatches.Count,
                DiferenciaInstaladaInterconexionMw = interconnectionTotal.HasValue
                    ? decimal.Round(installedTotal - interconnectionTotal.Value, 6)
                    : null,
                MetadatosComentariosNormalizados = commentsNormalized,
                PuedeRegistrarComoCorte = rows.Count > 0,
                PuedeMarcarseValidado = rowsWithoutName.Count == 0 && brokenFormulaRows.Count == 0
            };

            for (var column = firstColumn; column <= lastColumn; column++)
            {
                var header = headers.GetValueOrDefault(column) ?? string.Empty;
                result.Columnas.Add(new PvircePreviewColumn
                {
                    Numero = column,
                    Letra = ObtenerLetraColumna(column),
                    Encabezado = header,
                    EncabezadoNormalizado = NormalizarEncabezado(header),
                    FilasConValor = filledCounts.GetValueOrDefault(column),
                    Formulas = formulaCounts.GetValueOrDefault(column)
                });
            }

            result.ResumenAnual = rows
                .Where(row => row.Year.HasValue)
                .GroupBy(row => row.Year!.Value)
                .OrderBy(group => group.Key)
                .Select(group => new PvircePreviewYearSummary
                {
                    Anio = group.Key,
                    Registros = group.Count(),
                    CapacidadInstaladaMw = decimal.Round(group.Sum(row => row.InstalledMw ?? 0m), 6),
                    AdicionesMw = decimal.Round(group.Where(row => EsAdicion(row.Movement)).Sum(row => row.InstalledMw ?? 0m), 6),
                    SustitucionesMw = decimal.Round(group.Where(row => EsSustitucion(row.Movement)).Sum(row => row.InstalledMw ?? 0m), 6)
                })
                .ToList();

            result.NombresDuplicados = duplicateGroups
                .Take(50)
                .Select(group => new PvircePreviewDuplicateName
                {
                    Nombre = group.First().Name,
                    Repeticiones = group.Count(),
                    Filas = group.Select(row => row.RowNumber).Take(20).ToList()
                })
                .ToList();

            result.Muestra = rows.Take(MaximoMuestra).Select(row => new PvircePreviewRow
            {
                Fila = row.RowNumber,
                Nombre = row.Name,
                Anio = row.Year,
                Movimiento = row.Movement,
                TipoVf = row.TypeVf,
                Gcr = row.Gcr,
                Estatus = row.Status,
                CapacidadInstaladaMw = row.InstalledMw,
                CapacidadInterconexionMw = row.InterconnectionMw
            }).ToList();

            AgregarIncidencias(
                result,
                headerColumns,
                rows,
                rowsWithoutName,
                brokenFormulaRows,
                duplicateGroups,
                outsideHorizon,
                mismatches,
                numericFirmRows,
                hasStatusMacroColumn,
                hasCenaceColumn,
                commentsNormalized);

            return result;
        }

        private static void AgregarIncidencias(
            PvircePreviewResult result,
            IReadOnlyDictionary<string, int> headerColumns,
            IReadOnlyList<ParsedRow> rows,
            IReadOnlyList<int> rowsWithoutName,
            IReadOnlyList<int> brokenFormulaRows,
            IReadOnlyCollection<IGrouping<string, ParsedRow>> duplicateGroups,
            IReadOnlyList<ParsedRow> outsideHorizon,
            IReadOnlyList<ParsedRow> mismatches,
            IReadOnlyList<ParsedRow> numericFirmRows,
            bool hasStatusMacroColumn,
            bool hasCenaceColumn,
            bool commentsNormalized)
        {
            if (rowsWithoutName.Count > 0)
                AgregarIncidencia(result, "Bloqueante", "FILAS_SIN_NOMBRE", "Hay filas con contenido que no tienen Nombre real.", rowsWithoutName.Count, rowsWithoutName);

            if (brokenFormulaRows.Count > 0)
                AgregarIncidencia(result, "Bloqueante", "FORMULA_REF_ROTA", "Existen fórmulas con referencia #REF!; el corte puede preservarse como borrador, pero no validarse.", brokenFormulaRows.Count, brokenFormulaRows);

            if (duplicateGroups.Count > 0)
                AgregarIncidencia(result, "Advertencia", "NOMBRE_NO_ES_LLAVE", "Nombre real no es único y no debe utilizarse como identificador del proyecto.", duplicateGroups.Count, duplicateGroups.SelectMany(group => group.Select(row => row.RowNumber)));

            var hasStableId = headerColumns.Keys.Any(header =>
                header.Contains("FOLIO", StringComparison.OrdinalIgnoreCase)
                || header.Contains("VUPE", StringComparison.OrdinalIgnoreCase)
                || header.Contains("IDPROYECTO", StringComparison.OrdinalIgnoreCase)
                || header.Contains("IDENTIFICADOR", StringComparison.OrdinalIgnoreCase));
            if (!hasStableId)
                AgregarIncidencia(result, "Advertencia", "IDENTIFICADOR_ESTABLE_AUSENTE", "No se detectó folio VUPE ni otro identificador estable.", rows.Count, Array.Empty<int>());

            if (outsideHorizon.Count > 0)
                AgregarIncidencia(result, "Advertencia", "ANIO_FUERA_HORIZONTE", "Hay registros fuera del horizonte indicado en el título; deben clasificarse como línea base o continuidad.", outsideHorizon.Count, outsideHorizon.Select(row => row.RowNumber));

            if (mismatches.Count > 0)
                AgregarIncidencia(result, "Informativa", "CAPACIDAD_INSTALADA_INTERCONEXION_DIFIERE", "Capacidad instalada y capacidad de interconexión difieren; ambas se conservarán.", mismatches.Count, mismatches.Select(row => row.RowNumber));

            if (hasStatusMacroColumn)
            {
                var missing = rows.Where(row => string.IsNullOrWhiteSpace(row.StatusMacro)).ToList();
                if (missing.Count > 0)
                    AgregarIncidencia(result, "Advertencia", "STATUS_MACRO_INCOMPLETO", "Status Macro Final está incompleto y no puede usarse como universo total.", missing.Count, missing.Select(row => row.RowNumber));
            }

            if (hasCenaceColumn)
            {
                var missing = rows.Where(row => string.IsNullOrWhiteSpace(row.CenaceStatus)).ToList();
                if (missing.Count > 0)
                    AgregarIncidencia(result, "Advertencia", "ESTATUS_CENACE_INCOMPLETO", "Estatus CENACE está incompleto.", missing.Count, missing.Select(row => row.RowNumber));
            }

            if (numericFirmRows.Count > 0)
                AgregarIncidencia(result, "Advertencia", "FIRMES_TIPO_INCONSISTENTE", "La columna Firmes mezcla marcadores y valores numéricos.", numericFirmRows.Count, numericFirmRows.Select(row => row.RowNumber));

            if (result.TotalFormulas > 0)
                AgregarIncidencia(result, "Informativa", "FORMULAS_EN_FUENTE", "La carga confirmada deberá conservar fórmula y valor calculado por separado.", result.TotalFormulas, Array.Empty<int>());

            if (commentsNormalized)
                AgregarIncidencia(result, "Advertencia", "AUTOR_COMENTARIOS_INCOMPLETO", "El paquete tenía comentarios sin autor; sólo se normalizó una copia en memoria para la lectura.", 1, Array.Empty<int>());
        }

        private static void AgregarIncidencia(
            PvircePreviewResult result,
            string severity,
            string code,
            string message,
            int count,
            IEnumerable<int> rows)
        {
            result.Incidencias.Add(new PvircePreviewIssue
            {
                Severidad = severity,
                Codigo = code,
                Mensaje = message,
                Cantidad = count,
                FilasEjemplo = rows.Distinct().Take(10).ToList()
            });
        }

        private static WorkbookProfile DetectarHojaYEncabezados(XLWorkbook workbook)
        {
            WorkbookProfile? best = null;
            foreach (var worksheet in workbook.Worksheets)
            {
                var used = worksheet.RangeUsed();
                if (used == null)
                    continue;

                var firstRow = used.FirstRowUsed().RowNumber();
                var lastCandidateRow = Math.Min(used.LastRowUsed().RowNumber(), firstRow + 20);
                var firstColumn = used.FirstColumnUsed().ColumnNumber();
                var lastColumn = used.LastColumnUsed().ColumnNumber();

                for (var rowNumber = firstRow; rowNumber <= lastCandidateRow; rowNumber++)
                {
                    var normalizedHeaders = Enumerable.Range(firstColumn, lastColumn - firstColumn + 1)
                        .Select(column => NormalizarEncabezado(LeerTextoSeguro(worksheet.Cell(rowNumber, column))))
                        .Where(header => !string.IsNullOrWhiteSpace(header))
                        .ToHashSet(StringComparer.OrdinalIgnoreCase);

                    var hasName = normalizedHeaders.Contains("NOMBREREAL");
                    var hasYear = normalizedHeaders.Contains("ANO") || normalizedHeaders.Contains("ANOEOC");
                    var hasCapacity = normalizedHeaders.Contains("MW") || normalizedHeaders.Contains("CAPACIDADINSTALADAMWAC");
                    if (!hasName || !hasYear || !hasCapacity)
                        continue;

                    var score = 100 + normalizedHeaders.Count;
                    if (best == null || score > best.Score)
                        best = new WorkbookProfile(worksheet, used, rowNumber, score);
                }
            }

            return best ?? throw new InvalidOperationException("No fue posible localizar una hoja y encabezados compatibles con PVIRCE.");
        }

        private static string DetectarTitulo(
            IXLWorksheet worksheet,
            int firstRow,
            int lastRow,
            int firstColumn,
            int lastColumn)
        {
            if (lastRow < firstRow)
                return string.Empty;

            var candidates = new List<string>();
            for (var row = firstRow; row <= lastRow; row++)
            {
                for (var column = firstColumn; column <= lastColumn; column++)
                {
                    var value = LeerTextoSeguro(worksheet.Cell(row, column));
                    if (!string.IsNullOrWhiteSpace(value))
                        candidates.Add(value);
                }
            }

            return candidates
                .OrderByDescending(value => value.Contains("Programa Vinculante", StringComparison.OrdinalIgnoreCase))
                .ThenByDescending(value => value.Length)
                .FirstOrDefault() ?? string.Empty;
        }

        private static (int? Start, int? End) DetectarHorizonte(string title)
        {
            var match = HorizonRegex.Match(title ?? string.Empty);
            if (!match.Success)
                return (null, null);

            return int.TryParse(match.Groups["inicio"].Value, NumberStyles.Integer, CultureInfo.InvariantCulture, out var start)
                && int.TryParse(match.Groups["fin"].Value, NumberStyles.Integer, CultureInfo.InvariantCulture, out var end)
                ? (start, end)
                : (null, null);
        }

        private static int? BuscarColumna(IReadOnlyDictionary<string, int> headers, params string[] aliases)
        {
            foreach (var alias in aliases)
            {
                if (headers.TryGetValue(alias, out var column))
                    return column;
            }
            return null;
        }

        private static string LeerTextoSeguro(IXLCell cell)
        {
            try
            {
                return cell.GetFormattedString()?.Trim() ?? string.Empty;
            }
            catch
            {
                return string.Empty;
            }
        }

        private static int? LeerEntero(IXLCell cell)
        {
            try
            {
                if (cell.TryGetValue<int>(out var integer))
                    return integer;
                if (cell.TryGetValue<double>(out var number))
                    return Convert.ToInt32(Math.Round(number, MidpointRounding.AwayFromZero));
            }
            catch
            {
            }

            var text = LeerTextoSeguro(cell).Replace(",", string.Empty, StringComparison.Ordinal).Trim();
            return int.TryParse(text, NumberStyles.Integer, CultureInfo.InvariantCulture, out var result) ? result : null;
        }

        private static decimal? LeerDecimal(IXLCell cell)
        {
            try
            {
                if (cell.TryGetValue<decimal>(out var decimalValue))
                    return decimal.Round(decimalValue, 6);
                if (cell.TryGetValue<double>(out var number))
                    return decimal.Round((decimal)number, 6);
            }
            catch
            {
            }

            var text = LeerTextoSeguro(cell)
                .Replace("$", string.Empty, StringComparison.Ordinal)
                .Replace("%", string.Empty, StringComparison.Ordinal)
                .Replace(" ", string.Empty, StringComparison.Ordinal)
                .Trim();
            if (decimal.TryParse(text, NumberStyles.Number, CultureInfo.InvariantCulture, out var invariant))
                return decimal.Round(invariant, 6);
            return decimal.TryParse(text, NumberStyles.Number, new CultureInfo("es-MX"), out var local)
                ? decimal.Round(local, 6)
                : null;
        }

        private static bool EsAdicion(string movement)
            => (movement ?? string.Empty).Contains("Adici", StringComparison.OrdinalIgnoreCase);

        private static bool EsSustitucion(string movement)
            => (movement ?? string.Empty).Contains("Sustit", StringComparison.OrdinalIgnoreCase)
                || (movement ?? string.Empty).Contains("Retiro", StringComparison.OrdinalIgnoreCase);

        private static string NormalizarEncabezado(string value)
        {
            if (string.IsNullOrWhiteSpace(value))
                return string.Empty;

            var decomposed = value.Normalize(NormalizationForm.FormD);
            var builder = new StringBuilder(decomposed.Length);
            foreach (var character in decomposed)
            {
                if (CharUnicodeInfo.GetUnicodeCategory(character) == UnicodeCategory.NonSpacingMark)
                    continue;
                if (char.IsLetterOrDigit(character))
                    builder.Append(char.ToUpperInvariant(character));
            }
            return builder.ToString();
        }

        private static string NormalizarClave(string value)
        {
            if (string.IsNullOrWhiteSpace(value))
                return string.Empty;

            var decomposed = value.Normalize(NormalizationForm.FormD);
            var builder = new StringBuilder(decomposed.Length);
            foreach (var character in decomposed)
            {
                if (CharUnicodeInfo.GetUnicodeCategory(character) == UnicodeCategory.NonSpacingMark)
                    continue;
                if (char.IsLetterOrDigit(character))
                    builder.Append(char.ToUpperInvariant(character));
                else if (char.IsWhiteSpace(character))
                    builder.Append(' ');
            }
            return Regex.Replace(builder.ToString(), @"\s+", " ").Trim();
        }

        private static string ObtenerLetraColumna(int columnNumber)
        {
            var result = string.Empty;
            var current = columnNumber;
            while (current > 0)
            {
                current--;
                result = (char)('A' + current % 26) + result;
                current /= 26;
            }
            return result;
        }

        private static void ValidarPaqueteOffice(MemoryStream stream)
        {
            stream.Position = 0;
            using var archive = new ZipArchive(stream, ZipArchiveMode.Read, leaveOpen: true);
            if (archive.Entries.Count > MaximoPartesOffice)
                throw new InvalidOperationException("El archivo PVIRCE contiene demasiadas partes internas.");

            long expanded = 0;
            foreach (var entry in archive.Entries)
            {
                expanded = checked(expanded + entry.Length);
                if (expanded > MaximoArchivoDescomprimido)
                    throw new InvalidOperationException("El contenido descomprimido del archivo PVIRCE supera 300 MB.");
            }
            stream.Position = 0;
        }

        private static bool NormalizarAutorComentariosSiEsNecesario(MemoryStream stream)
        {
            stream.Position = 0;
            using var archive = new ZipArchive(stream, ZipArchiveMode.Update, leaveOpen: true);
            var entry = archive.GetEntry("xl/persons/person.xml");
            if (entry == null)
                return false;

            XDocument document;
            using (var entryStream = entry.Open())
                document = XDocument.Load(entryStream, System.Xml.Linq.LoadOptions.None);

            if (document.Root == null || document.Root.Elements().Any(node => node.Name.LocalName == "person"))
                return false;

            var xmlNamespace = document.Root.Name.Namespace;
            document.Root.Add(new XElement(
                xmlNamespace + "person",
                new XAttribute("displayName", "Autor no identificado"),
                new XAttribute("id", "{00000000-0000-0000-0000-000000000000}"),
                new XAttribute("userId", "Autor no identificado"),
                new XAttribute("providerId", "None")));

            entry.Delete();
            var replacement = archive.CreateEntry("xl/persons/person.xml", CompressionLevel.Optimal);
            using (var replacementStream = replacement.Open())
                document.Save(replacementStream, System.Xml.Linq.SaveOptions.DisableFormatting);

            stream.Position = 0;
            return true;
        }

        private sealed record WorkbookProfile(
            IXLWorksheet Worksheet,
            IXLRange UsedRange,
            int HeaderRow,
            int Score);

        private sealed class ParsedRow
        {
            public int RowNumber { get; set; }
            public string Name { get; set; } = string.Empty;
            public int? Year { get; set; }
            public string Movement { get; set; } = string.Empty;
            public string TypeVf { get; set; } = string.Empty;
            public string Gcr { get; set; } = string.Empty;
            public string Status { get; set; } = string.Empty;
            public decimal? InstalledMw { get; set; }
            public decimal? InterconnectionMw { get; set; }
            public string StatusMacro { get; set; } = string.Empty;
            public string CenaceStatus { get; set; } = string.Empty;
            public string FirmValue { get; set; } = string.Empty;
        }
    }
}
