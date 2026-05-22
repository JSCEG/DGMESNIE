using System.Globalization;
using System.Text;
using ClosedXML.Excel;
using Microsoft.VisualBasic.FileIO;
using NSIE.Models;

namespace NSIE.Servicios
{
    public class ManualSharePointImportService
    {
        private const int PreviewLimit = 200;

        public async Task<ManualSharePointImportResult> ParseAsync(IFormFile file)
        {
            if (file == null || file.Length == 0)
            {
                throw new InvalidOperationException("Debes seleccionar un archivo válido para importar.");
            }

            var extension = Path.GetExtension(file.FileName).ToLowerInvariant();

            return extension switch
            {
                ".xlsx" => await ParseExcelAsync(file),
                ".csv" => await ParseCsvAsync(file),
                _ => throw new InvalidOperationException("Solo se admiten archivos .xlsx o .csv para la carga manual.")
            };
        }

        private static async Task<ManualSharePointImportResult> ParseExcelAsync(IFormFile file)
        {
            await using var stream = new MemoryStream();
            await file.CopyToAsync(stream);
            stream.Position = 0;

            using var workbook = new XLWorkbook(stream);
            var worksheet = workbook.Worksheets.FirstOrDefault(ws => !ws.IsEmpty())
                ?? workbook.Worksheets.First();

            var usedRange = worksheet.RangeUsed();
            if (usedRange == null)
            {
                throw new InvalidOperationException("El archivo Excel no contiene datos utilizables.");
            }

            var headerRow = usedRange.FirstRowUsed();
            var lastRowNumber = usedRange.LastRowUsed().RowNumber();
            var lastColumnNumber = usedRange.LastColumnUsed().ColumnNumber();

            var result = new ManualSharePointImportResult
            {
                FileName = file.FileName,
                SourceType = "Excel",
                WorksheetName = worksheet.Name
            };

            for (var column = 1; column <= lastColumnNumber; column++)
            {
                var header = headerRow.Cell(column).GetFormattedString();
                result.Columns.Add(string.IsNullOrWhiteSpace(header) ? $"Columna {column}" : header.Trim());
            }

            for (var rowNumber = headerRow.RowNumber() + 1; rowNumber <= lastRowNumber; rowNumber++)
            {
                var row = worksheet.Row(rowNumber);
                var values = new List<string>();

                for (var column = 1; column <= lastColumnNumber; column++)
                {
                    values.Add(GetCellValue(row.Cell(column)));
                }

                if (values.All(string.IsNullOrWhiteSpace))
                {
                    continue;
                }

                result.TotalRows++;
                if (result.Rows.Count < PreviewLimit)
                {
                    result.Rows.Add(values);
                }
            }

            result.PreviewRows = result.Rows.Count;
            result.Notes.Add("Se tomó la primera hoja con datos del archivo.");
            if (result.TotalRows > PreviewLimit)
            {
                result.Notes.Add($"La previsualización muestra solo las primeras {PreviewLimit.ToString(CultureInfo.InvariantCulture)} filas.");
            }

            return result;
        }

        private static async Task<ManualSharePointImportResult> ParseCsvAsync(IFormFile file)
        {
            await using var stream = file.OpenReadStream();
            using var parser = new TextFieldParser(stream, Encoding.UTF8)
            {
                TextFieldType = FieldType.Delimited,
                HasFieldsEnclosedInQuotes = true,
                TrimWhiteSpace = true
            };
            parser.SetDelimiters(",", ";", "\t");

            if (parser.EndOfData)
            {
                throw new InvalidOperationException("El archivo CSV no contiene datos utilizables.");
            }

            var headers = parser.ReadFields() ?? Array.Empty<string>();
            var result = new ManualSharePointImportResult
            {
                FileName = file.FileName,
                SourceType = "CSV",
                WorksheetName = "Archivo CSV"
            };

            for (var index = 0; index < headers.Length; index++)
            {
                var header = headers[index]?.Trim();
                result.Columns.Add(string.IsNullOrWhiteSpace(header) ? $"Columna {index + 1}" : header);
            }

            while (!parser.EndOfData)
            {
                var fields = parser.ReadFields() ?? Array.Empty<string>();
                var row = new List<string>();

                for (var index = 0; index < result.Columns.Count; index++)
                {
                    row.Add(index < fields.Length ? (fields[index] ?? string.Empty).Trim() : string.Empty);
                }

                if (row.All(string.IsNullOrWhiteSpace))
                {
                    continue;
                }

                result.TotalRows++;
                if (result.Rows.Count < PreviewLimit)
                {
                    result.Rows.Add(row);
                }
            }

            result.PreviewRows = result.Rows.Count;
            result.Notes.Add("Para CSV se detectaron delimitadores coma, punto y coma o tabulación.");
            if (result.TotalRows > PreviewLimit)
            {
                result.Notes.Add($"La previsualización muestra solo las primeras {PreviewLimit.ToString(CultureInfo.InvariantCulture)} filas.");
            }

            return result;
        }

        private static string GetCellValue(IXLCell cell)
        {
            if (cell.IsEmpty())
            {
                return string.Empty;
            }

            return cell.GetFormattedString().Trim();
        }
    }
}