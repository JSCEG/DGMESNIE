using System.Globalization;
using System.Security.Cryptography;
using ClosedXML.Excel;
using NSIE.Models.ProyectosPrivados;

namespace NSIE.Servicios;

public sealed partial class CarteraConvocatoriaImportService
{
    // Consolidado de las calculadoras financieras entregadas por los promoventes: una fila por folio con los
    // supuestos y resultados del modelo (CapEx, retornos, PPA, deuda y flujos acumulados). Se guardan los campos
    // clave con tipo y el resto del renglón como pares nombre/valor para no perder ningún supuesto del corte.
    public async Task<CarteraConvocatoriaCalculadoraDocument> LeerCalculadorasAsync(Stream stream, string fileName, CancellationToken cancellationToken = default)
    {
        ArgumentNullException.ThrowIfNull(stream);
        await using var buffer = new MemoryStream();
        await stream.CopyToAsync(buffer, cancellationToken);
        var bytes = buffer.ToArray();
        if (bytes.Length < 4 || bytes[0] != 0x50 || bytes[1] != 0x4B)
            throw new InvalidDataException("El archivo debe ser un libro Excel .xlsx válido.");
        buffer.Position = 0;

        using var workbook = new XLWorkbook(buffer);
        var document = new CarteraConvocatoriaCalculadoraDocument
        {
            FileName = Path.GetFileName(fileName),
            Sha256 = Convert.ToHexString(SHA256.HashData(bytes)).ToLowerInvariant()
        };

        foreach (var sheet in workbook.Worksheets)
        {
            cancellationToken.ThrowIfCancellationRequested();
            var (headerRow, headers) = FindHeaders(sheet, 1, "Folio", "CapEx inicial total (USD)");
            if (!headers.ContainsKey("FOLIO") || !headers.ContainsKey(Normalize("CapEx inicial total (USD)"))) continue;

            var byColumn = headers.Where(h => !h.Key.Contains('#')).ToDictionary(h => h.Value, h => h.Key);
            var last = sheet.LastRowUsed()?.RowNumber() ?? headerRow;
            for (var number = headerRow + 1; number <= last; number++)
            {
                cancellationToken.ThrowIfCancellationRequested();
                var row = sheet.Row(number);
                var folio = CellText(row, headers, "Folio");
                if (string.IsNullOrWhiteSpace(folio) || !folio.StartsWith("CFE-", StringComparison.OrdinalIgnoreCase)) continue;

                decimal? Num(string header) => Number(Cell(row, headers, header));
                var item = new CarteraConvocatoriaCalculadora
                {
                    Folio = folio.Trim(),
                    Proyecto = CleanOptional(CellText(row, headers, "Proyecto")),
                    Tecnologia = CleanOptional(CellText(row, headers, "Tecnología")),
                    Inversionista = CleanOptional(CellText(row, headers, "Inversionista")),
                    MwAc = Num("Capacidad instalada AC (MWac)"),
                    MwDc = Num("Capacidad instalada DC (MWp)"),
                    SaeMw = Num("Potencia SAE (MW)"),
                    SaeMwh = Num("Energía SAE (MWh)"),
                    SaeHoras = Num("Duración SAE (horas)"),
                    Cod = CellDate(row, headers, "Entrada en operación (COD)"),
                    CapexTotal = Num("CapEx inicial total (USD)"),
                    CapexCentral = Num("CapEx central (USD)"),
                    CapexBaterias = Num("CapEx baterías (USD)"),
                    CapexInterconexion = Num("CapEx interconexión (USD)"),
                    DevEx = Num("DevEx (USD)"),
                    RetornoProyecto = Num("Retorno del proyecto reportado (%)"),
                    RetornoPrivado = Num("Retorno del inversionista privado (%)"),
                    RetornoObjetivo = Num("Retorno objetivo (%)"),
                    RetornoInterconexion = Num("Retorno implícito de interconexión (%)"),
                    ParticipacionPrivada = Num("Participación inversionista privado (%)"),
                    ContribucionCfe = Num("Contribución monetaria CFE (%)"),
                    PrecioEnergia = Num("Precio inicial energía y CEL (USD/MWh)"),
                    PlazoPpa = Num("Plazo PPA energía (años)"),
                    PlazoReversion = Num("Plazo de reversión del activo (años)"),
                    Apalancamiento = Num("Apalancamiento máximo construcción (%)"),
                    PlazoDeuda = Num("Plazo deuda (años)"),
                    TirAntesIsr = Num("TIR sin deuda antes de ISR (%)"),
                    TirDespuesIsr = Num("TIR sin deuda después de ISR (%)"),
                    MoicProyecto = Num("MOIC proyecto (veces)"),
                    MoicPrivado = Num("MOIC inversionista privado (veces)"),
                    EbitdaAcumulado = Num("EBITDA acumulado (millones USD)"),
                    IngresosAcumulados = Num("Ingresos acumulados (millones USD)"),
                    UtilidadAcumulada = Num("Utilidad neta acumulada (millones USD)"),
                    GeneracionAcumulada = Num("Generación neta en nodo acumulada (GWh)"),
                    OpexAnio1 = Num("OPEX total año 1 (USD/año)"),
                    Observaciones = CleanOptional(CellText(row, headers, "Observaciones")),
                    FileName = document.FileName,
                    Sha256 = document.Sha256
                };

                foreach (var (column, header) in byColumn.OrderBy(c => c.Key))
                {
                    var cell = row.Cell(column);
                    var text = Text(cell);
                    if (string.IsNullOrWhiteSpace(text)) continue;
                    var label = Text(sheet.Cell(headerRow, column));
                    if (string.IsNullOrWhiteSpace(label)) continue;
                    item.Campos.Add(new ConvocatoriaCampoFuente
                    {
                        Name = label.Trim(),
                        Value = cell.DataType == XLDataType.DateTime && cell.TryGetValue<DateTime>(out var fecha)
                            ? fecha.ToString("yyyy-MM-dd", CultureInfo.InvariantCulture)
                            : text.Trim()
                    });
                }
                document.Rows.Add(item);
            }
            if (document.Rows.Count > 0) break;
        }

        if (document.Rows.Count == 0)
            throw new InvalidDataException("El libro no contiene una hoja con las columnas Folio y CapEx inicial total (USD).");
        var duplicates = document.Rows.GroupBy(r => r.Folio, StringComparer.OrdinalIgnoreCase).Where(g => g.Count() > 1).Select(g => g.Key).ToList();
        if (duplicates.Count > 0)
            throw new InvalidDataException("El libro repite folios: " + string.Join(", ", duplicates.Take(10)) + ".");
        return document;
    }
}
