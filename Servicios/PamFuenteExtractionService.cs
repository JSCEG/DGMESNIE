using System.Globalization;
using System.IO.Compression;
using System.Security.Cryptography;
using System.Text;
using System.Text.RegularExpressions;
using System.Xml.Linq;
using ClosedXML.Excel;
using UglyToad.PdfPig;

namespace NSIE.Servicios
{
    public interface IPamFuenteExtractionService
    {
        Task<PamFuenteExtractionResult> ExtraerAsync(PamFuenteAnalisisDescriptor fuente, CancellationToken cancellationToken);
    }

    public sealed class PamFuenteExtractionService : IPamFuenteExtractionService
    {
        private const long MaximoOfficeComprimido = 80L * 1024 * 1024;
        private const long MaximoOfficeDescomprimido = 300L * 1024 * 1024;
        private const int MaximoRegistros = 50000;
        private const int MaximoColumnas = 200;
        private static readonly Regex ClaveRegex = new(
            @"(?<![A-Z0-9])(?:[A-Z]{1,5}\d{2})-[A-Z0-9]{2,12}(?![A-Z0-9])",
            RegexOptions.Compiled | RegexOptions.IgnoreCase | RegexOptions.CultureInvariant);
        private static readonly Regex AnioRegex = new(@"\b(?:19|20)\d{2}\b", RegexOptions.Compiled);
        private static readonly Regex SlideNumberRegex = new(@"slide(?<n>\d+)\.xml$", RegexOptions.Compiled | RegexOptions.IgnoreCase);

        private readonly string _storageRoot;

        public PamFuenteExtractionService(IConfiguration configuration, IWebHostEnvironment environment)
        {
            var configuredRoot = configuration["PamActualizaciones:StorageRoot"];
            if (string.IsNullOrWhiteSpace(configuredRoot))
                configuredRoot = @"D:\DGMESNIE\RepositorioPAM\Actualizaciones";

            configuredRoot = Environment.ExpandEnvironmentVariables(configuredRoot.Trim());
            if (!Path.IsPathRooted(configuredRoot))
                throw new InvalidOperationException("La carpeta del repositorio PAM debe usar una ruta absoluta.");

            _storageRoot = Path.GetFullPath(configuredRoot);
            if (EsMismaOSubcarpeta(_storageRoot, environment.ContentRootPath)
                || (!string.IsNullOrWhiteSpace(environment.WebRootPath)
                    && EsMismaOSubcarpeta(_storageRoot, environment.WebRootPath)))
            {
                throw new InvalidOperationException("El repositorio documental PAM debe estar fuera de la aplicación y de wwwroot.");
            }
        }

        public async Task<PamFuenteExtractionResult> ExtraerAsync(
            PamFuenteAnalisisDescriptor fuente,
            CancellationToken cancellationToken)
        {
            var physicalPath = ResolverRuta(fuente.RutaArchivo);
            if (!File.Exists(physicalPath))
                throw new FileNotFoundException("No se encontró el original registrado para la fuente.", physicalPath);

            await ValidarHashAsync(physicalPath, fuente.HashSha256, cancellationToken);
            var extension = fuente.Extension?.Trim().ToLowerInvariant();

            return extension switch
            {
                ".xlsx" => ExtraerExcel(physicalPath, fuente.NombreOriginal, cancellationToken),
                ".pdf" => ExtraerPdf(physicalPath, fuente.NombreOriginal, cancellationToken),
                ".pptx" => ExtraerPowerPoint(physicalPath, fuente.NombreOriginal, cancellationToken),
                ".xls" or ".ppt" => throw new PamFuenteNoSoportadaException(
                    $"{extension} se preservó correctamente, pero requiere conversión interna antes de extraer datos."),
                _ => throw new PamFuenteNoSoportadaException($"No existe extractor para la extensión {extension}.")
            };
        }

        private PamFuenteExtractionResult ExtraerExcel(
            string physicalPath,
            string fileName,
            CancellationToken cancellationToken)
        {
            var fileInfo = new FileInfo(physicalPath);
            if (fileInfo.Length > MaximoOfficeComprimido)
                throw new InvalidOperationException("El Excel supera el límite de 80 MB para análisis automático.");

            ValidarPaqueteOffice(physicalPath);
            using var workbook = new XLWorkbook(physicalPath);
            var result = new PamFuenteExtractionResult { Extractor = "XLSX estructurado", VersionExtractor = "1.1" };

            foreach (var worksheet in workbook.Worksheets)
            {
                cancellationToken.ThrowIfCancellationRequested();
                var used = worksheet.RangeUsed();
                if (used == null) continue;

                var firstRow = used.FirstRow().RowNumber();
                var lastRow = used.LastRow().RowNumber();
                var firstColumn = used.FirstColumn().ColumnNumber();
                var lastColumn = used.LastColumn().ColumnNumber();
                if (lastColumn - firstColumn + 1 > MaximoColumnas)
                    throw new InvalidOperationException($"La hoja {worksheet.Name} supera {MaximoColumnas} columnas utilizables.");

                var header = DetectarEncabezado(worksheet, firstRow, Math.Min(lastRow, firstRow + 20), firstColumn, lastColumn);
                if (header == null)
                {
                    ExtraerReferenciasDeHojaSinPerfil(worksheet, used, result, cancellationToken);
                    continue;
                }

                for (var rowNumber = header.RowNumber + 1; rowNumber <= lastRow; rowNumber++)
                {
                    cancellationToken.ThrowIfCancellationRequested();
                    if (result.Registros.Count >= MaximoRegistros)
                        throw new InvalidOperationException($"El análisis superó el límite de {MaximoRegistros:N0} registros.");

                    var row = worksheet.Row(rowNumber);
                    var claveRaw = LeerCelda(row, header, "CLAVE");
                    var nombre = LeerCelda(row, header, "NOMBRE");
                    if (string.IsNullOrWhiteSpace(claveRaw) && string.IsNullOrWhiteSpace(nombre)) continue;

                    var etapa = LeerCelda(row, header, "ETAPA");
                    var gcr = LeerCelda(row, header, "GRT");
                    var estadoDetalle = LeerCelda(row, header, "ESTADO_DETALLE");
                    var fechaNecesaria = LeerFecha(row, header, "FECHA_NECESARIA");
                    var fechaEntrada = LeerFecha(row, header, "FECHA_ENTRADA");
                    var anioRaw = LeerCelda(row, header, "ANIO_INSTRUCCION");
                    var monto = LeerDecimal(row, header, "MONTO");
                    var datos = new Dictionary<string, string>(StringComparer.OrdinalIgnoreCase);

                    foreach (var column in header.OriginalHeaders)
                    {
                        var value = row.Cell(column.Key).GetFormattedString()?.Trim();
                        if (!string.IsNullOrWhiteSpace(value)) datos[column.Value] = value;
                    }

                    if (string.IsNullOrWhiteSpace(claveRaw)
                        && Regex.IsMatch(nombre ?? string.Empty, @"^\d+\s*$", RegexOptions.CultureInvariant)
                        && datos.Count <= 2)
                    {
                        continue;
                    }

                    var claves = ClaveRegex.Matches(NormalizarGuiones(claveRaw ?? string.Empty))
                        .Select(match => new
                        {
                            Clave = NormalizarClave(match.Value),
                            Original = match.Value.Trim()
                        })
                        .Where(item => !string.IsNullOrWhiteSpace(item.Clave))
                        .GroupBy(item => item.Clave, StringComparer.OrdinalIgnoreCase)
                        .Select(group => group.First())
                        .ToList();
                    if (claves.Count == 0)
                    {
                        claves.Add(new
                        {
                            Clave = NormalizarClave(claveRaw),
                            Original = claveRaw?.Trim()
                        });
                    }

                    foreach (var clave in claves)
                    {
                        result.Registros.Add(new PamRegistroExtraido
                        {
                            MetodoDeteccion = "XLSX encabezados",
                            TipoHallazgo = "Fila estructurada",
                            Ubicacion = $"Hoja {worksheet.Name}, fila {rowNumber}",
                            NumeroReferencia = rowNumber,
                            Clave = clave.Clave,
                            ClaveOriginal = clave.Original,
                            Nombre = Limitar(nombre, 500),
                            Grt = Limitar(gcr, 200),
                            Etapa = Limitar(etapa, 500),
                            EstadoDetalle = Limitar(estadoDetalle, 1000),
                            FechaNecesaria = Limitar(fechaNecesaria, 100),
                            FechaEntradaOperacion = Limitar(fechaEntrada, 100),
                            AnioInstruccion = ExtraerAnioUnico(anioRaw),
                            AnioInstruccionRaw = Limitar(anioRaw, 100),
                            MontoMdp = monto,
                            Datos = datos,
                            Evidencia = Limitar(string.Join(" | ", new[]
                            {
                                clave.Original, nombre, etapa, estadoDetalle,
                                monto?.ToString("0.###", CultureInfo.InvariantCulture)
                            }.Where(value => !string.IsNullOrWhiteSpace(value))), 2000)
                        });
                    }
                }
            }

            if (result.Registros.Count == 0)
                result.Registros.Add(PamRegistroExtraido.Incidencia(fileName, "No se detectaron filas ni claves de proyecto en el Excel."));

            return result;
        }

        private PamFuenteExtractionResult ExtraerPdf(
            string physicalPath,
            string fileName,
            CancellationToken cancellationToken)
        {
            var result = new PamFuenteExtractionResult { Extractor = "PdfPig texto por página", VersionExtractor = "1.0" };
            var pagesWithoutText = 0;

            using var document = PdfDocument.Open(physicalPath);
            foreach (var page in document.GetPages())
            {
                cancellationToken.ThrowIfCancellationRequested();
                var text = NormalizarEspacios(string.Join(" ", page.GetWords().Select(word => word.Text)));
                if (text.Length < 10) pagesWithoutText++;

                var normalizedText = NormalizarGuiones(text);
                var keys = ClaveRegex.Matches(normalizedText)
                    .Select(match => new { Key = NormalizarClave(match.Value), match.Index })
                    .Where(item => !string.IsNullOrWhiteSpace(item.Key))
                    .GroupBy(item => item.Key, StringComparer.OrdinalIgnoreCase)
                    .Select(group => group.First())
                    .ToList();

                foreach (var key in keys)
                {
                    if (result.Registros.Count >= MaximoRegistros)
                        throw new InvalidOperationException($"El análisis superó el límite de {MaximoRegistros:N0} referencias.");

                    result.Registros.Add(new PamRegistroExtraido
                    {
                        MetodoDeteccion = "PDF texto",
                        TipoHallazgo = "Referencia documental",
                        Ubicacion = $"Página {page.Number}",
                        NumeroReferencia = page.Number,
                        Clave = key.Key,
                        ClaveOriginal = key.Key,
                        Evidencia = CrearFragmento(normalizedText, key.Index, key.Key.Length),
                        Datos = new Dictionary<string, string> { ["Archivo"] = fileName, ["Página"] = page.Number.ToString(CultureInfo.InvariantCulture) }
                    });
                }
            }

            if (pagesWithoutText > 0)
                result.Advertencias.Add($"{pagesWithoutText} página(s) no contenían texto suficiente y pueden requerir OCR.");
            if (result.Registros.Count == 0)
                result.Registros.Add(PamRegistroExtraido.Incidencia(fileName, "No se detectaron claves de proyecto en el PDF; puede requerir OCR o revisión manual."));

            return result;
        }

        private PamFuenteExtractionResult ExtraerPowerPoint(
            string physicalPath,
            string fileName,
            CancellationToken cancellationToken)
        {
            var fileInfo = new FileInfo(physicalPath);
            if (fileInfo.Length > MaximoOfficeComprimido)
                throw new InvalidOperationException("La presentación supera el límite de 80 MB para análisis automático.");

            ValidarPaqueteOffice(physicalPath);
            var result = new PamFuenteExtractionResult { Extractor = "PPTX Open XML", VersionExtractor = "1.0" };
            using var stream = new FileStream(physicalPath, FileMode.Open, FileAccess.Read, FileShare.Read);
            using var archive = new ZipArchive(stream, ZipArchiveMode.Read, leaveOpen: false);
            var slides = archive.Entries
                .Select(entry => new { Entry = entry, Match = SlideNumberRegex.Match(entry.FullName) })
                .Where(item => item.Match.Success && item.Entry.FullName.StartsWith("ppt/slides/", StringComparison.OrdinalIgnoreCase))
                .OrderBy(item => int.Parse(item.Match.Groups["n"].Value, CultureInfo.InvariantCulture))
                .ToList();

            foreach (var slide in slides)
            {
                cancellationToken.ThrowIfCancellationRequested();
                var slideNumber = int.Parse(slide.Match.Groups["n"].Value, CultureInfo.InvariantCulture);
                using var entryStream = slide.Entry.Open();
                var document = XDocument.Load(entryStream, System.Xml.Linq.LoadOptions.None);
                var text = NormalizarEspacios(string.Join(" ", document.Descendants()
                    .Where(node => node.Name.LocalName == "t")
                    .Select(node => node.Value)));
                var normalizedText = NormalizarGuiones(text);

                var keys = ClaveRegex.Matches(normalizedText)
                    .Select(match => new { Key = NormalizarClave(match.Value), match.Index })
                    .Where(item => !string.IsNullOrWhiteSpace(item.Key))
                    .GroupBy(item => item.Key, StringComparer.OrdinalIgnoreCase)
                    .Select(group => group.First());

                foreach (var key in keys)
                {
                    result.Registros.Add(new PamRegistroExtraido
                    {
                        MetodoDeteccion = "PPTX texto",
                        TipoHallazgo = "Referencia documental",
                        Ubicacion = $"Diapositiva {slideNumber}",
                        NumeroReferencia = slideNumber,
                        Clave = key.Key,
                        ClaveOriginal = key.Key,
                        Evidencia = CrearFragmento(normalizedText, key.Index, key.Key.Length),
                        Datos = new Dictionary<string, string> { ["Archivo"] = fileName, ["Diapositiva"] = slideNumber.ToString(CultureInfo.InvariantCulture) }
                    });
                }
            }

            if (result.Registros.Count == 0)
                result.Registros.Add(PamRegistroExtraido.Incidencia(fileName, "No se detectaron claves de proyecto en la presentación."));

            return result;
        }

        private static HeaderProfile DetectarEncabezado(
            IXLWorksheet worksheet,
            int firstRow,
            int lastCandidateRow,
            int firstColumn,
            int lastColumn)
        {
            HeaderProfile best = null;
            for (var rowNumber = firstRow; rowNumber <= lastCandidateRow; rowNumber++)
            {
                var canonical = new Dictionary<string, int>(StringComparer.OrdinalIgnoreCase);
                var original = new Dictionary<int, string>();
                for (var column = firstColumn; column <= lastColumn; column++)
                {
                    var headerText = worksheet.Cell(rowNumber, column).GetFormattedString()?.Trim();
                    if (string.IsNullOrWhiteSpace(headerText)) continue;
                    original[column] = Limitar(headerText, 200);
                    var canonicalName = CanonicalizarEncabezado(headerText);
                    if (canonicalName != null && !canonical.ContainsKey(canonicalName)) canonical[canonicalName] = column;
                }

                var score = (canonical.ContainsKey("CLAVE") ? 5 : 0)
                    + (canonical.ContainsKey("NOMBRE") ? 5 : 0)
                    + canonical.Keys.Count(key => key is not "CLAVE" and not "NOMBRE");
                if (score < 10 || (best != null && score <= best.Score)) continue;
                best = new HeaderProfile(rowNumber, score, canonical, original);
            }
            return best;
        }

        private static string CanonicalizarEncabezado(string value)
        {
            var normalized = NormalizarTextoClave(value);
            if (normalized is "PEM" or "CLAVEPEM" or "CLAVEPROYECTO") return "CLAVE";
            if (normalized is "PROYECTO" or "NOMBREDELPROYECTO") return "NOMBRE";
            if (normalized is "GCR" or "GRT" or "REGION") return "GRT";
            if (normalized.Contains("ESTATUSCFEPAMRNT2026PRIORIZACENACE")) return "ESTATUS_PRIORIZA";
            if (normalized is "ESTATUSCFEPAMRNT2026" or "ETAPADELPROYECTO" or "ESTATUS" or "ETAPA") return "ETAPA";
            if (normalized.Contains("PRIORIZACIONCFE")) return "PRIORIZACION_CFE";
            if (normalized.Contains("PROPUESTACENACE")) return "PROPUESTA_CENACE";
            if (normalized.Contains("FECHADENTRADAENOPERACION") || normalized.Contains("FECHADEENTRADAENOPERACION")) return "FECHA_ENTRADA";
            if (normalized.Contains("FECHANECESARIA")) return "FECHA_NECESARIA";
            if (normalized.Contains("ANODEINSTRUCCION")) return "ANIO_INSTRUCCION";
            if (normalized.Contains("FINANCIAMIENTO") && normalized.Contains("AVANCE")) return "ESTADO_DETALLE";
            if (normalized.Contains("MONTODEPROYECTO") || normalized.Contains("MONTODELPROYECTO")) return "MONTO";
            return null;
        }

        private static void ExtraerReferenciasDeHojaSinPerfil(
            IXLWorksheet worksheet,
            IXLRange used,
            PamFuenteExtractionResult result,
            CancellationToken cancellationToken)
        {
            var firstRow = used.FirstRow().RowNumber();
            var lastRow = Math.Min(used.LastRow().RowNumber(), firstRow + MaximoRegistros - 1);
            var firstColumn = used.FirstColumn().ColumnNumber();
            var lastColumn = Math.Min(used.LastColumn().ColumnNumber(), firstColumn + MaximoColumnas - 1);

            for (var rowNumber = firstRow; rowNumber <= lastRow; rowNumber++)
            {
                cancellationToken.ThrowIfCancellationRequested();
                var rowText = NormalizarEspacios(string.Join(" ", Enumerable.Range(firstColumn, lastColumn - firstColumn + 1)
                    .Select(column => worksheet.Cell(rowNumber, column).GetFormattedString())
                    .Where(value => !string.IsNullOrWhiteSpace(value))));
                var normalized = NormalizarGuiones(rowText);
                foreach (Match match in ClaveRegex.Matches(normalized))
                {
                    result.Registros.Add(new PamRegistroExtraido
                    {
                        MetodoDeteccion = "XLSX referencia",
                        TipoHallazgo = "Referencia documental",
                        Ubicacion = $"Hoja {worksheet.Name}, fila {rowNumber}",
                        NumeroReferencia = rowNumber,
                        Clave = NormalizarClave(match.Value),
                        ClaveOriginal = match.Value,
                        Evidencia = CrearFragmento(normalized, match.Index, match.Length),
                        Datos = new Dictionary<string, string> { ["Hoja"] = worksheet.Name, ["Fila"] = rowNumber.ToString(CultureInfo.InvariantCulture) }
                    });
                }
            }
        }

        private static string LeerCelda(IXLRow row, HeaderProfile header, string canonical)
        {
            return header.Columns.TryGetValue(canonical, out var column)
                ? row.Cell(column).GetFormattedString()?.Trim()
                : null;
        }

        private static string LeerFecha(IXLRow row, HeaderProfile header, string canonical)
        {
            if (!header.Columns.TryGetValue(canonical, out var column)) return null;
            var cell = row.Cell(column);
            if (cell.TryGetValue<DateTime>(out var date)) return date.ToString("yyyy-MM-dd", CultureInfo.InvariantCulture);
            return cell.GetFormattedString()?.Trim();
        }

        private static decimal? LeerDecimal(IXLRow row, HeaderProfile header, string canonical)
        {
            if (!header.Columns.TryGetValue(canonical, out var column)) return null;
            var cell = row.Cell(column);
            if (cell.TryGetValue<double>(out var number)) return decimal.Round((decimal)number, 3);
            var text = cell.GetFormattedString()?.Trim();
            if (string.IsNullOrWhiteSpace(text)) return null;
            var sanitized = text.Replace("$", string.Empty).Replace(" ", string.Empty);
            if (decimal.TryParse(sanitized, NumberStyles.Number, CultureInfo.InvariantCulture, out var invariant)) return decimal.Round(invariant, 3);
            return decimal.TryParse(sanitized, NumberStyles.Number, new CultureInfo("es-MX"), out var local)
                ? decimal.Round(local, 3)
                : null;
        }

        private static int? ExtraerAnioUnico(string value)
        {
            if (string.IsNullOrWhiteSpace(value)) return null;
            var years = AnioRegex.Matches(value).Select(match => match.Value).Distinct().ToList();
            return years.Count == 1 && int.TryParse(years[0], out var year) ? year : null;
        }

        private void ValidarPaqueteOffice(string physicalPath)
        {
            using var stream = new FileStream(physicalPath, FileMode.Open, FileAccess.Read, FileShare.Read);
            using var archive = new ZipArchive(stream, ZipArchiveMode.Read, leaveOpen: false);
            if (archive.Entries.Count > 10000)
                throw new InvalidOperationException("El archivo Office contiene demasiadas partes internas.");

            long expanded = 0;
            foreach (var entry in archive.Entries)
            {
                expanded = checked(expanded + entry.Length);
                if (expanded > MaximoOfficeDescomprimido)
                    throw new InvalidOperationException("El contenido descomprimido del archivo Office supera 300 MB.");
            }
        }

        private async Task ValidarHashAsync(string physicalPath, string expectedHash, CancellationToken cancellationToken)
        {
            if (string.IsNullOrWhiteSpace(expectedHash) || expectedHash.Length != 64)
                throw new InvalidOperationException("La fuente no tiene una huella SHA-256 válida.");

            await using var stream = new FileStream(physicalPath, FileMode.Open, FileAccess.Read, FileShare.Read, 81920, useAsync: true);
            var actualHash = Convert.ToHexString(await SHA256.HashDataAsync(stream, cancellationToken));
            if (!string.Equals(actualHash, expectedHash, StringComparison.OrdinalIgnoreCase))
                throw new InvalidOperationException("El archivo original ya no coincide con la huella SHA-256 registrada.");
        }

        private string ResolverRuta(string relativePath)
        {
            if (string.IsNullOrWhiteSpace(relativePath) || Path.IsPathRooted(relativePath))
                throw new InvalidOperationException("La fuente no tiene una ruta administrada válida.");

            var normalized = relativePath.Replace('/', Path.DirectorySeparatorChar);
            var physicalPath = Path.GetFullPath(Path.Combine(_storageRoot, normalized));
            if (!EsMismaOSubcarpeta(physicalPath, _storageRoot))
                throw new InvalidOperationException("La ruta de la fuente está fuera del repositorio documental.");
            return physicalPath;
        }

        private static string NormalizarClave(string value)
        {
            if (string.IsNullOrWhiteSpace(value)) return null;
            var normalized = NormalizarGuiones(value).ToUpperInvariant();
            normalized = Regex.Replace(normalized, @"\s+", string.Empty);
            var match = ClaveRegex.Match(normalized);
            return match.Success && match.Value.Length == normalized.Length ? match.Value : normalized;
        }

        private static string NormalizarGuiones(string value)
        {
            return (value ?? string.Empty)
                .Replace('‐', '-').Replace('‑', '-').Replace('‒', '-')
                .Replace('–', '-').Replace('—', '-').Replace('−', '-');
        }

        private static string NormalizarTextoClave(string value)
        {
            if (string.IsNullOrWhiteSpace(value)) return string.Empty;
            var decomposed = value.Normalize(NormalizationForm.FormD);
            var builder = new StringBuilder(decomposed.Length);
            foreach (var character in decomposed)
            {
                if (CharUnicodeInfo.GetUnicodeCategory(character) == UnicodeCategory.NonSpacingMark) continue;
                if (char.IsLetterOrDigit(character)) builder.Append(char.ToUpperInvariant(character));
            }
            return builder.ToString();
        }

        private static string NormalizarEspacios(string value)
            => Regex.Replace(value ?? string.Empty, @"\s+", " ").Trim();

        private static string CrearFragmento(string text, int index, int length)
        {
            if (string.IsNullOrWhiteSpace(text)) return null;
            var start = Math.Max(0, index - 180);
            var end = Math.Min(text.Length, index + length + 260);
            return Limitar(text[start..end].Trim(), 2000);
        }

        private static string Limitar(string value, int maxLength)
            => string.IsNullOrWhiteSpace(value) ? null : value.Trim()[..Math.Min(value.Trim().Length, maxLength)];

        private static bool EsMismaOSubcarpeta(string candidatePath, string rootPath)
        {
            var candidate = Path.GetFullPath(candidatePath);
            var root = Path.GetFullPath(rootPath);
            var relative = Path.GetRelativePath(root, candidate);
            return !Path.IsPathRooted(relative)
                && relative != ".."
                && !relative.StartsWith($"..{Path.DirectorySeparatorChar}", StringComparison.Ordinal)
                && !relative.StartsWith($"..{Path.AltDirectorySeparatorChar}", StringComparison.Ordinal);
        }

        private sealed record HeaderProfile(
            int RowNumber,
            int Score,
            Dictionary<string, int> Columns,
            Dictionary<int, string> OriginalHeaders);
    }

    public sealed class PamFuenteNoSoportadaException : InvalidOperationException
    {
        public PamFuenteNoSoportadaException(string message) : base(message) { }
    }

    public sealed class PamFuenteAnalisisDescriptor
    {
        public long LoteFuenteId { get; set; }
        public long FuenteId { get; set; }
        public string NombreOriginal { get; set; }
        public string Extension { get; set; }
        public string RutaArchivo { get; set; }
        public string HashSha256 { get; set; }
    }

    public sealed class PamFuenteExtractionResult
    {
        public string Extractor { get; set; }
        public string VersionExtractor { get; set; }
        public List<PamRegistroExtraido> Registros { get; } = new();
        public List<string> Advertencias { get; } = new();
    }

    public sealed class PamRegistroExtraido
    {
        public string MetodoDeteccion { get; set; }
        public string TipoHallazgo { get; set; }
        public string Ubicacion { get; set; }
        public int? NumeroReferencia { get; set; }
        public string Clave { get; set; }
        public string ClaveOriginal { get; set; }
        public string Nombre { get; set; }
        public string Grt { get; set; }
        public string Etapa { get; set; }
        public string EstadoDetalle { get; set; }
        public string FechaNecesaria { get; set; }
        public string FechaEntradaOperacion { get; set; }
        public int? AnioInstruccion { get; set; }
        public string AnioInstruccionRaw { get; set; }
        public decimal? MontoMdp { get; set; }
        public string Evidencia { get; set; }
        public Dictionary<string, string> Datos { get; set; } = new(StringComparer.OrdinalIgnoreCase);

        public static PamRegistroExtraido Incidencia(string fileName, string message) => new()
        {
            MetodoDeteccion = "Sistema",
            TipoHallazgo = "Incidencia",
            Ubicacion = "Archivo completo",
            Evidencia = message,
            Datos = new Dictionary<string, string> { ["Archivo"] = fileName ?? string.Empty, ["Incidencia"] = message }
        };
    }
}
