using System.Security.Cryptography;
using ClosedXML.Excel;
using NSIE.Models.ProyectosPrivados;

namespace NSIE.Servicios;

public sealed partial class CarteraConvocatoriaImportService
{
    // Libro complementario de CFE: una hoja con Folio, Cluster, Excluyente1, Excluyente2 y la marca
    // Considerar de su propio corte. Sólo se leen esas columnas; el resto del formulario ya vive en
    // el expediente de la cartera.
    public async Task<CarteraConvocatoriaMarcasDocument> LeerMarcasAsync(Stream stream, string fileName, CancellationToken cancellationToken = default)
    {
        ArgumentNullException.ThrowIfNull(stream);
        await using var buffer = new MemoryStream();
        await stream.CopyToAsync(buffer, cancellationToken);
        var bytes = buffer.ToArray();
        if (bytes.Length < 4 || bytes[0] != 0x50 || bytes[1] != 0x4B)
            throw new InvalidDataException("El archivo debe ser un libro Excel .xlsx válido.");
        buffer.Position = 0;

        using var workbook = new XLWorkbook(buffer);
        var document = new CarteraConvocatoriaMarcasDocument
        {
            FileName = Path.GetFileName(fileName),
            Sha256 = Convert.ToHexString(SHA256.HashData(bytes)).ToLowerInvariant()
        };

        foreach (var sheet in workbook.Worksheets)
        {
            // Encabezado: primera fila (entre las 10 primeras) que contenga Folio y Cluster.
            int headerRow = 0;
            Dictionary<string, int>? headers = null;
            for (var row = 1; row <= 10 && headers == null; row++)
            {
                var candidate = BuildHeaderMap(sheet, row);
                if (candidate.ContainsKey("FOLIO") && candidate.ContainsKey("CLUSTER")) { headerRow = row; headers = candidate; }
            }
            if (headers == null) continue;

            var last = sheet.LastRowUsed()?.RowNumber() ?? headerRow;
            for (var number = headerRow + 1; number <= last; number++)
            {
                cancellationToken.ThrowIfCancellationRequested();
                var row = sheet.Row(number);
                var folio = CellText(row, headers, "Folio");
                if (string.IsNullOrWhiteSpace(folio) || !folio.StartsWith("CFE-", StringComparison.OrdinalIgnoreCase)) continue;
                var considerar = CellText(row, headers, "Considerar");
                document.Marks.Add(new CarteraConvocatoriaMarca
                {
                    Folio = folio.Trim(),
                    Cluster = CleanOptional(CellText(row, headers, "Cluster")),
                    Excluyente1 = CleanOptional(CellText(row, headers, "Excluyente1")),
                    Excluyente2 = CleanOptional(CellText(row, headers, "Excluyente2")),
                    Considerar = string.IsNullOrWhiteSpace(considerar) ? null : considerar.Trim() == "1",
                    FileName = document.FileName,
                    Sha256 = document.Sha256
                });
            }
            document.Rows = document.Marks.Count;
            break;
        }

        if (document.Marks.Count == 0)
            throw new InvalidDataException("El libro no contiene una hoja con las columnas Folio y Cluster.");
        var duplicates = document.Marks.GroupBy(m => m.Folio, StringComparer.OrdinalIgnoreCase).Where(g => g.Count() > 1).Select(g => g.Key).ToList();
        if (duplicates.Count > 0)
            throw new InvalidDataException("El libro repite folios: " + string.Join(", ", duplicates.Take(10)) + ".");
        return document;
    }
}
