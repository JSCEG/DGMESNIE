using System.Globalization;
using System.Text;
using ClosedXML.Excel;
using NSIE.Models;

namespace NSIE.Servicios
{
    public class PvirseImportService
    {
        public const string DefaultFileName = "PVIRCE2026-2040_SQ.xlsx";

        private static readonly Dictionary<string, Action<PvirseRegistro, string>> HeaderMap =
            new(StringComparer.OrdinalIgnoreCase)
            {
                ["STATUS"] = (item, value) => item.Status = NormalizeText(value),
                ["STATUSVF"] = (item, value) => item.StatusVf = NormalizeText(value),
                ["NOMBREREAL"] = (item, value) => item.NombreReal = NormalizeText(value),
                ["NOCONSIDERAR"] = (item, value) => item.NoConsiderar = NormalizeText(value),
                ["ANO"] = (item, value) => item.Anio = ParseInt(value),
                ["ADICIONESOSUSTITUCIONES"] = (item, value) => item.AdicionesOSustituciones = NormalizeText(value),
                ["CONTRATOOUNIDAD"] = (item, value) => item.ContratoOUnidad = NormalizeText(value),
                ["TIPO"] = (item, value) => item.Tipo = NormalizeText(value),
                ["TIPOVF"] = (item, value) => item.TipoVf = NormalizeText(value),
                ["RENOVABLE"] = (item, value) => item.Renovable = NormalizeText(value),
                ["MW"] = (item, value) => item.Mw = ParseDecimal(value),
                ["MES"] = (item, value) => item.Mes = NormalizeText(value),
                ["GERENCIADECONTROL"] = (item, value) => item.GerenciaDeControl = NormalizeText(value),
                ["REGIONDETRANSMISION"] = (item, value) => item.RegionDeTransmision = NormalizeText(value),
                ["ENTIDADFEDERATIVA"] = (item, value) => item.EntidadFederativa = NormalizeText(value),
                ["MUNICIPIO"] = (item, value) => item.Municipio = NormalizeText(value),
                ["FIRMES"] = (item, value) => item.Firmes = NormalizeText(value)
            };

        public Task<List<PvirseRegistro>> LeerBaseOficialAsync(string contentRootPath)
        {
            var filePath = Path.Combine(contentRootPath, "Documentacion", DefaultFileName);
            if (!File.Exists(filePath))
            {
                throw new FileNotFoundException("No se encontró el archivo Excel oficial de PVIRSE.", filePath);
            }

            using var stream = File.OpenRead(filePath);
            var registros = ParseWorkbook(stream, Path.GetFileName(filePath));
            return Task.FromResult(registros);
        }

        private static List<PvirseRegistro> ParseWorkbook(Stream stream, string sourceFileName)
        {
            using var workbook = new XLWorkbook(stream);
            var worksheet = workbook.Worksheet(1);
            var usedRange = worksheet.RangeUsed();
            if (usedRange == null)
            {
                throw new InvalidOperationException("La hoja de PVIRSE no contiene datos utilizables.");
            }

            var lastRow = usedRange.LastRowUsed().RowNumber();
            var lastColumn = usedRange.LastColumnUsed().ColumnNumber();
            IXLRow headerRow = null;

            for (var rowNumber = usedRange.FirstRowUsed().RowNumber(); rowNumber <= Math.Min(lastRow, 10); rowNumber++)
            {
                var candidateRow = worksheet.Row(rowNumber);
                var normalizedHeaders = Enumerable.Range(1, lastColumn)
                    .Select(column => NormalizeHeader(candidateRow.Cell(column).GetFormattedString()))
                    .ToList();

                if (normalizedHeaders.Contains("STATUS") && normalizedHeaders.Contains("ENTIDADFEDERATIVA") && normalizedHeaders.Contains("MW"))
                {
                    headerRow = candidateRow;
                    break;
                }
            }

            if (headerRow == null)
            {
                throw new InvalidOperationException("No fue posible localizar la fila de encabezados de PVIRSE.");
            }

            var headers = new Dictionary<int, string>();
            for (var column = 1; column <= lastColumn; column++)
            {
                headers[column] = NormalizeHeader(headerRow.Cell(column).GetFormattedString());
            }

            var registros = new List<PvirseRegistro>();
            for (var rowNumber = headerRow.RowNumber() + 1; rowNumber <= lastRow; rowNumber++)
            {
                var row = worksheet.Row(rowNumber);
                var registro = new PvirseRegistro
                {
                    Activo = true,
                    FuenteArchivo = sourceFileName,
                    FechaCarga = DateTime.UtcNow
                };

                var hasData = false;
                for (var column = 1; column <= lastColumn; column++)
                {
                    var value = row.Cell(column).GetFormattedString().Trim();
                    if (!string.IsNullOrWhiteSpace(value))
                    {
                        hasData = true;
                    }

                    if (HeaderMap.TryGetValue(headers[column], out var assign))
                    {
                        assign(registro, value);
                    }
                }

                if (!hasData || string.IsNullOrWhiteSpace(registro.NombreReal))
                {
                    continue;
                }

                registros.Add(registro);
            }

            return registros;
        }

        private static string NormalizeHeader(string value)
        {
            if (string.IsNullOrWhiteSpace(value))
            {
                return string.Empty;
            }

            var normalized = value.Normalize(NormalizationForm.FormD);
            var builder = new StringBuilder(normalized.Length);

            foreach (var character in normalized)
            {
                var unicodeCategory = CharUnicodeInfo.GetUnicodeCategory(character);
                if (unicodeCategory == UnicodeCategory.NonSpacingMark)
                {
                    continue;
                }

                if (char.IsLetterOrDigit(character))
                {
                    builder.Append(char.ToUpperInvariant(character));
                }
            }

            return builder.ToString();
        }

        private static string NormalizeText(string value)
        {
            return string.IsNullOrWhiteSpace(value) ? null : value.Trim();
        }

        private static int? ParseInt(string value)
        {
            if (string.IsNullOrWhiteSpace(value))
            {
                return null;
            }

            return int.TryParse(value.Trim(), NumberStyles.Integer, CultureInfo.InvariantCulture, out var result)
                ? result
                : null;
        }

        private static decimal? ParseDecimal(string value)
        {
            if (string.IsNullOrWhiteSpace(value))
            {
                return null;
            }

            var sanitized = value.Replace(",", string.Empty).Trim();
            return decimal.TryParse(sanitized, NumberStyles.Number, CultureInfo.InvariantCulture, out var result)
                ? decimal.Round(result, 2)
                : null;
        }
    }
}