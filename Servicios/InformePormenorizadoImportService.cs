using System.Globalization;
using System.Text;
using ClosedXML.Excel;
using NSIE.Models;

namespace NSIE.Servicios
{
    public class InformePormenorizadoImportService
    {
        private const string SheetName = "BASE";
        private const string DefaultFileName = "INF Pormenorizado Tablas Rev 251125 SENER.xlsx";

        private static readonly Dictionary<string, Action<ProyectoModernizacionRegistro, string>> HeaderMap =
            new(StringComparer.OrdinalIgnoreCase)
            {
                ["NO"] = (item, value) => item.Numero = ParseInt(value) ?? 0,
                ["NOORIGINAL"] = (item, value) => item.NumeroOriginal = ParseInt(value),
                ["GRT"] = (item, value) => item.GRT = NormalizeText(value),
                ["NOMBREDELPROYECTO"] = (item, value) => item.NombreProyecto = NormalizeText(value),
                ["TIPODEFINANCIAMIENTO"] = (item, value) => item.TipoFinanciamiento = NormalizeText(value),
                ["ANODEINSTRUCCION"] = (item, value) => item.AnioInstruccion = ParseInt(value),
                ["ETAPADELPROYECTO"] = (item, value) => item.EtapaProyecto = NormalizeText(value),
                ["MONTODELPROYECTOMDP"] = (item, value) => item.MontoProyectoMdp = ParseDecimal(value),
                ["ELEMENTOSYEQUIPOSASOCIADOS"] = (item, value) => item.ElementosEquiposAsociados = NormalizeText(value),
                ["FECHAESTIMADADEINICIO"] = (item, value) => item.FechaEstimadaInicio = NormalizeText(value),
                ["FEOINDICADAENOFICIOSENER"] = (item, value) => item.FeoIndicadaOficioSener = NormalizeText(value),
                ["FEOFACTIBLE"] = (item, value) => item.FeoFactible = NormalizeText(value),
                ["AVANCEDEEJECUCION"] = (item, value) => item.PorcentajeAvanceEjecucion = ParseDecimal(value),
                ["CIRCUNSTANCIASQUEHAYANOCASIONADOATRASOSENLASOBRAS"] = (item, value) => item.CircunstanciasAtrasos = NormalizeText(value),
                ["ACCIONESDEMITIGACIONOCORRECCIONLLEVADASACABOPARACORREGIRLOSATRASOS"] = (item, value) => item.AccionesMitigacionCorreccion = NormalizeText(value),
                ["ESTADOREALQUEGUARDAELPROYECTO"] = (item, value) => item.EstadoRealProyecto = NormalizeText(value),
                ["COMENTARIOSSOBREELNIVELDEPRIORIZACION"] = (item, value) => item.ComentariosNivelPriorizacion = NormalizeText(value),
                ["CLAVEPEM"] = (item, value) => item.ClavePem = NormalizeText(value),
                ["CLASIFICACIONSENER"] = (item, value) => item.ClasificacionSener = NormalizeText(value),
                ["FECHADEPROGRAMACIONTRIMESTRE"] = (item, value) => item.FechaProgramacionTrimestre = NormalizeText(value),
                ["QUINCENADEPUBLICACIONQUINCENA"] = (item, value) => item.QuincenaPublicacion = NormalizeText(value),
                ["UNIVERSOPRESENTACIONPRESIDENCIA"] = (item, value) => item.UniversoPresentacionPresidencia = NormalizeText(value),
                ["MVA"] = (item, value) => item.Mva = ParseDecimal(value),
                ["MVAR"] = (item, value) => item.Mvar = ParseDecimal(value),
                ["KMC"] = (item, value) => item.KmC = ParseDecimal(value)
            };

        public Task<List<ProyectoModernizacionRegistro>> LeerBaseOficialAsync(string contentRootPath)
        {
            var filePath = Path.Combine(contentRootPath, "Documentacion", DefaultFileName);

            if (!File.Exists(filePath))
            {
                throw new FileNotFoundException("No se encontró el archivo oficial del informe pormenorizado.", filePath);
            }

            using var stream = File.OpenRead(filePath);
            var registros = ParseWorkbook(stream, Path.GetFileName(filePath));
            return Task.FromResult(registros);
        }

        private static List<ProyectoModernizacionRegistro> ParseWorkbook(Stream stream, string sourceFileName)
        {
            using var workbook = new XLWorkbook(stream);
            var worksheet = workbook.Worksheet(SheetName);
            var usedRange = worksheet.RangeUsed();

            if (usedRange == null)
            {
                throw new InvalidOperationException("La hoja BASE no contiene datos utilizables.");
            }

            var headerRow = usedRange.FirstRowUsed();
            var lastRow = usedRange.LastRowUsed().RowNumber();
            var lastColumn = usedRange.LastColumnUsed().ColumnNumber();
            var headers = new Dictionary<int, string>();

            for (var column = 1; column <= lastColumn; column++)
            {
                headers[column] = NormalizeHeader(headerRow.Cell(column).GetFormattedString());
            }

            var missingHeaders = new[]
            {
                "NO",
                "NOMBREDELPROYECTO",
                "TIPODEFINANCIAMIENTO",
                "ETAPADELPROYECTO",
                "MONTODELPROYECTOMDP"
            }.Where(required => headers.Values.All(existing => !string.Equals(existing, required, StringComparison.OrdinalIgnoreCase)));

            if (missingHeaders.Any())
            {
                throw new InvalidOperationException($"La hoja BASE no contiene todas las columnas esperadas: {string.Join(", ", missingHeaders)}.");
            }

            var registros = new List<ProyectoModernizacionRegistro>();

            for (var rowNumber = headerRow.RowNumber() + 1; rowNumber <= lastRow; rowNumber++)
            {
                var row = worksheet.Row(rowNumber);
                var registro = new ProyectoModernizacionRegistro
                {
                    FuenteArchivo = sourceFileName,
                    Activo = true,
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

                    if (!HeaderMap.TryGetValue(headers[column], out var assign))
                    {
                        continue;
                    }

                    assign(registro, value);
                }

                if (!hasData)
                {
                    continue;
                }

                if (registro.Numero <= 0 || string.IsNullOrWhiteSpace(registro.NombreProyecto))
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
                var unicodeCategory = System.Globalization.CharUnicodeInfo.GetUnicodeCategory(character);
                if (unicodeCategory == System.Globalization.UnicodeCategory.NonSpacingMark)
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

            var sanitized = value.Replace(",", string.Empty).Trim();
            return int.TryParse(sanitized, NumberStyles.Integer, CultureInfo.InvariantCulture, out var result)
                ? result
                : null;
        }

        private static decimal? ParseDecimal(string value)
        {
            if (string.IsNullOrWhiteSpace(value))
            {
                return null;
            }

            var sanitized = value.Replace("$", string.Empty).Replace("%", string.Empty).Replace(" ", string.Empty).Trim();

            if (decimal.TryParse(sanitized, NumberStyles.Number | NumberStyles.AllowCurrencySymbol, CultureInfo.InvariantCulture, out var invariantValue))
            {
                return decimal.Round(invariantValue, 2);
            }

            return decimal.TryParse(sanitized, NumberStyles.Number | NumberStyles.AllowCurrencySymbol, new CultureInfo("es-MX"), out var esValue)
                ? decimal.Round(esValue, 2)
                : null;
        }
    }
}