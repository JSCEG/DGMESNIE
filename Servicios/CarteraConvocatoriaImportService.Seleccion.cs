using System.Security.Cryptography;
using ClosedXML.Excel;
using NSIE.Models.ProyectosPrivados;

namespace NSIE.Servicios;

public sealed partial class CarteraConvocatoriaImportService
{
    // Libro de selección del área ("Actualización de 246"): una hoja con el formulario completo de ventanilla
    // precedido por las columnas de decisión (Descarte, Considerar, Motivo, Preferente, CENACE debe realizar
    // estudios, etc.). Sólo se leen las columnas de decisión; el formulario ya vive en el expediente.
    public async Task<CarteraConvocatoriaSeleccionDocument> LeerSeleccionAsync(Stream stream, string fileName, CancellationToken cancellationToken = default)
    {
        ArgumentNullException.ThrowIfNull(stream);
        await using var buffer = new MemoryStream();
        await stream.CopyToAsync(buffer, cancellationToken);
        var bytes = buffer.ToArray();
        if (bytes.Length < 4 || bytes[0] != 0x50 || bytes[1] != 0x4B)
            throw new InvalidDataException("El archivo debe ser un libro Excel .xlsx válido.");
        buffer.Position = 0;

        using var workbook = new XLWorkbook(buffer);
        var document = new CarteraConvocatoriaSeleccionDocument
        {
            FileName = Path.GetFileName(fileName),
            Sha256 = Convert.ToHexString(SHA256.HashData(bytes)).ToLowerInvariant()
        };

        foreach (var sheet in workbook.Worksheets)
        {
            int headerRow = 0;
            Dictionary<string, int>? headers = null;
            for (var row = 1; row <= 10 && headers == null; row++)
            {
                var candidate = BuildHeaderMap(sheet, row);
                if (candidate.ContainsKey("FOLIO") && candidate.ContainsKey("CONSIDERAR") && candidate.ContainsKey("PREFERENTE")) { headerRow = row; headers = candidate; }
            }
            if (headers == null) continue;

            var last = sheet.LastRowUsed()?.RowNumber() ?? headerRow;
            for (var number = headerRow + 1; number <= last; number++)
            {
                cancellationToken.ThrowIfCancellationRequested();
                var row = sheet.Row(number);
                var folio = CellText(row, headers, "Folio");
                if (string.IsNullOrWhiteSpace(folio) || !folio.StartsWith("CFE-", StringComparison.OrdinalIgnoreCase)) continue;
                document.Rows.Add(new CarteraConvocatoriaSeleccion
                {
                    Folio = folio.Trim(),
                    Considerar = Flag(CellText(row, headers, "Considerar")),
                    Descarte = Flag(CellText(row, headers, "Descarte")),
                    Motivo = CleanOptional(CellText(row, headers, "Motivo")),
                    CoincideReferencia = CleanOptional(CellText(row, headers, "Coincide con 241 de Ramón")),
                    Preseleccionados = CleanOptional(CellText(row, headers, "Preseleccionados")),
                    Factibles = CleanOptional(CellText(row, headers, "Factibles")),
                    ApoyaSen = CleanOptional(CellText(row, headers, "Apoya al SEN (Si/No)")),
                    ObrasOnerosas = CleanOptional(CellText(row, headers, "Se prevee obras onerosas (Si/No)")),
                    ExcluyenteConOtros = CleanOptional(CellText(row, headers, "Excluyente con otros proyectos (Si/No)")),
                    ProyectosQueExcluye = CleanOptional(CellText(row, headers, "Nombre de proyectos que excluye")),
                    PreferenteEntreExcluyentes = CleanOptional(CellText(row, headers, "Proyecto preferente entre los excluyentes (Si/No)")),
                    Preferente = Flag(CellText(row, headers, "Preferente")),
                    CenaceEstudios = Flag(CellText(row, headers, "CENACE debe realizar estudios")),
                    ProyectosSustitutos = CleanOptional(CellText(row, headers, "Proyectos sustitutos")),
                    MixtosI = Flag(CellText(row, headers, "Mixtos 1")),
                    Sistema = CleanOptional(CellText(row, headers, "Sistema")),
                    FileName = document.FileName,
                    Sha256 = document.Sha256
                });
            }
            break;
        }

        if (document.Rows.Count == 0)
            throw new InvalidDataException("El libro no contiene una hoja con las columnas Folio, Considerar y Preferente.");
        var duplicates = document.Rows.GroupBy(r => r.Folio, StringComparer.OrdinalIgnoreCase).Where(g => g.Count() > 1).Select(g => g.Key).ToList();
        if (duplicates.Count > 0)
            throw new InvalidDataException("El libro repite folios: " + string.Join(", ", duplicates.Take(10)) + ".");
        return document;

        // "1", "Si", "Sí", "X" cuentan como marca; vacío es falso.
        static bool Flag(string? value)
        {
            var v = (value ?? "").Trim().ToUpperInvariant();
            return v is "1" or "SI" or "SÍ" or "X" or "TRUE" or "VERDADERO";
        }
    }
}
