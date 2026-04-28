using System;
using System.Collections.Generic;
using System.Globalization;
using System.Text.RegularExpressions;
using UglyToad.PdfPig;
using UglyToad.PdfPig.Content;
using NSIE.Models;

namespace NSIE.Servicios
{
    public class FacturaExtractorService
    {
        public List<Factura> ExtractFromPdf(string pdfPath, string pdfFileName)
        {
            var facturas = new List<Factura>();
            int idCounter = 1;

            using (var document = PdfDocument.Open(pdfPath))
            {
                foreach (var page in document.GetPages())
                {
                    var text = page.Text;

                    var factura = new Factura
                    {

                        Id = idCounter++,  // Asigna el ID de orden
                        PdfFileName = pdfFileName,  // Asigna el nombre real del archivo PDF
                        Proveedor = ExtraerProveedor(text),
                        RFCProveedor = ExtraerRFCProveedor(text),
                        // RegimenFiscalProveedor = ExtraerCampo(text, new[] { "Regimen Fiscal:" }, new[] { "Lugar de Expedición:" }),
                        // LugarExpedicion = ExtraerLugarExpedicion(text), 

                        // UUID = ExtraerCampo(text, new[] { "FOLIO FISCAL (UUID):" }, new[] { "NO. SERIE DEL CSD:" }),
                        // FechaHoraCertificacion = ExtractDateTimeField(text, "FECHA Y HORA DE CERTIFICACIÓN:"),
                        FechaHoraEmision = ExtractDateTimeField(text, "FECHA Y HORA DE EMISIÓN DE CFDI:"),

                        Cliente = ExtraerCampo(text, new[] { "CLIENTE:" }, new[] { "RFC:" }),
                        //UsoCFDI = ExtraerCampo(text, new[] { "USO CFDI:" }, new[] { "Forma de Pago:" }),
                        //EfectoComprobante = ExtraerCampo(text, new[] { "Efecto" }, new[] { "Folio y serie:" }),

                        RFCCliente = ExtraerCampo(text, new[] { "RFC:" }, new[] { "Regimen fiscal:", "USO CFDI:" }),
                        //FormaPago = ExtraerCampo(text, new[] { "Forma de Pago:" }, new[] { "Método de Pago:" }),

                        ClaveMoneda = ExtraerCampo(text, new[] { "Clave de" }, new[] { "C. PRODUCTO" }),
                        // RegimenFiscalCliente = ExtraerCampo(text, new[] { "Regimen fiscal:" }, new[] { "Domicilio:" }),
                        // MetodoPago = ExtraerCampo(text, new[] { "Método de Pago:" }, new[] { "Exportación :" }),
                        FolioSerie = ExtraerCampo(text, new[] { "Folio y serie:" }, new[] { "Clave de" }),

                        ProductoDescripcion = ExtractTextBetweenDelimiters(ExtractDescripcion(text, "DESCRIPCIÓN", "PRECIO UNITARIO")),


                        // Exportacion = ExtractExportacion(text),
                        // ProductoClave = ExtractProductoClave(text),
                        // ProductoCantidad = ExtractProductoCantidad(text),
                        // ProductoUnidad = ExtractProductoUnidad(text),
                        // ProductoPrecioUnitario = ExtractProductoPrecioUnitario(text),
                        // ProductoImporte = ExtractProductoImporte(text),
                        // ProductoDescuento = ExtractProductoDescuento(text),
                        Subtotal = ExtraerCampo(text, new[] { "SUBTOTAL:" }, new[] { "TRASLADOS" }),
                        IVA = ExtraerCampo(text, new[] { "002 (IVA 16%):" }, new[] { "TOTAL:" }),
                        Total = CalculateTotal(ExtraerCampo(text, new[] { "SUBTOTAL:" }, new[] { "TRASLADOS" }), ExtraerCampo(text, new[] { "002 (IVA 16%):" }, new[] { "TOTAL:" })),
                        // SelloDigitalCFDI = ExtractSelloDigitalCFDI(text),
                        // SelloDigitalSAT = ExtractSelloDigitalSAT(text),
                        // CadenaOriginalSAT = ExtractCadenaOriginalSAT(text),
                        //CondicionesPago = ExtractCondicionesPago(text)
                        //CondicionesPago = ExtractDescripcion(text, new[] { "Condiciones de Pago:" }, new[] { "Otro Delimitador Final" })
                    };
                    facturas.Add(factura);
                }
            }

            return facturas;
        }


        //Métodos Generales
        private string ExtraerCampo(string texto, string[] camposInicio, string[] camposFinal)
        {
            foreach (var campoInicio in camposInicio)
            {
                var indiceInicio = texto.IndexOf(campoInicio);
                if (indiceInicio == -1)
                {
                    continue; // Si no se encuentra campoInicio, intenta con el siguiente
                }

                indiceInicio += campoInicio.Length;

                foreach (var campoFinal in camposFinal)
                {
                    var indiceFinal = campoFinal == null ? texto.Length : texto.IndexOf(campoFinal, indiceInicio);
                    if (indiceFinal == -1 || indiceFinal < indiceInicio)
                    {
                        continue; // Si no se encuentra campoFinal, intenta con el siguiente
                    }

                    var valorExtraido = texto.Substring(indiceInicio, indiceFinal - indiceInicio).Trim();
                    Console.WriteLine($"Campo {campoInicio}: {valorExtraido}");

                    return LimpiarValorExtraido(valorExtraido);
                }
            }

            Console.WriteLine("No se encontró el campo con los delimitadores proporcionados");
            return string.Empty; // Si no se encuentra con ningún par de delimitadores
        }

        private string LimpiarValorExtraido(string valorExtraido)
        {
            var indiceDosPuntos = valorExtraido.IndexOf(':');
            if (indiceDosPuntos != -1)
            {
                valorExtraido = valorExtraido.Substring(indiceDosPuntos + 1).Trim();
            }

            return valorExtraido.Trim();
        }


        // Métodos individuales para cada campo
        // Método para extraer el nombre del proveedor

        private string ExtraerProveedor(string texto)
        {
            // Encuentra el índice donde comienza "Regimen Fiscal:"
            var indiceFinal = texto.IndexOf("Regimen Fiscal:");
            if (indiceFinal == -1)
            {
                // Si no se encuentra "Regimen Fiscal:", devuelve una cadena vacía
                Console.WriteLine("No se encontró 'Regimen Fiscal:'");
                return string.Empty;
            }

            // Substrae el texto desde el inicio hasta "Regimen Fiscal:"
            var proveedor = texto.Substring(0, indiceFinal).Trim();

            // Patrón para encontrar el RFC (número de 6 dígitos)
            var patronRFC = @"\d{6}";
            var coincidenciaRFC = Regex.Match(proveedor, patronRFC, RegexOptions.RightToLeft);
            if (coincidenciaRFC.Success)
            {
                // Si se encuentra un RFC válido, extrae el nombre del proveedor hasta el comienzo del RFC
                var indiceRFC = coincidenciaRFC.Index;
                var rfc = proveedor.Substring(indiceRFC - 3, 9);
                proveedor = proveedor.Substring(0, indiceRFC - 3).Trim();
                // Console.WriteLine($"RFC del Proveedor: {rfc}");
            }
            else
            {
                // Si no se encuentra un RFC válido, devuelve una cadena vacía
                Console.WriteLine("No se encontró un RFC válido y no se pudo limpiar el RFC del Nombre del Proveedor!");
                return string.Empty;
            }

            // Imprime el nombre del proveedor extraído
            Console.WriteLine($"Nombre del Proveedor: {proveedor}");
            return proveedor;
        }

        // Método para extraer el RFC del proveedor
        private string ExtraerRFCProveedor(string texto)
        {
            // Encuentra el índice donde comienza "Regimen Fiscal:"
            var indiceFinal = texto.IndexOf("Regimen Fiscal:");
            if (indiceFinal == -1)
            {
                // Si no se encuentra "Regimen Fiscal:", devuelve una cadena vacía
                Console.WriteLine("No se encontró 'Regimen Fiscal:'");
                return string.Empty;
            }

            // Substrae el texto desde el inicio hasta "Regimen Fiscal:"
            var proveedor = texto.Substring(0, indiceFinal).Trim();

            // Patrón para encontrar el RFC (número de 6 dígitos)
            var patronRFC = @"\d{6}";
            var coincidenciaRFC = Regex.Match(proveedor, patronRFC, RegexOptions.RightToLeft);
            if (coincidenciaRFC.Success)
            {
                // Si se encuentra un RFC válido, ajusta el índice del RFC
                var indiceRFC = coincidenciaRFC.Index;
                var rfc = proveedor.Substring(indiceRFC - 3, 9);
                Console.WriteLine($"RFC del Proveedor: {rfc}");
                return rfc;
            }

            // Si no se encuentra un RFC válido, devuelve una cadena vacía
            return string.Empty;
        }

        private string ExtraerLugarExpedicion(string texto)
        {
            var indiceInicio = texto.IndexOf("Lugar de Expedición:");
            if (indiceInicio == -1)
            {
                Console.WriteLine("No se encontró 'Lugar de Expedición:'");
                return string.Empty;
            }

            indiceInicio += "Lugar de Expedición:".Length;

            // Extraer solo los primeros 5 caracteres después de "Lugar de Expedición:"
            var valorExtraido = texto.Substring(indiceInicio, 5).Trim();
            Console.WriteLine($"Campo Lugar de Expedición: |{valorExtraido}|");

            return LimpiarValorExtraido(valorExtraido);
        }



        private string ExtractDescripcion(string text, string fieldName, string nextField = null)
        {
            var startIndex = text.IndexOf(fieldName);
            if (startIndex == -1)
            {
                Console.WriteLine($"No se encontró el campo: {fieldName}");
                return string.Empty;
            }
            startIndex += fieldName.Length;
            var endIndex = nextField == null ? text.Length : text.IndexOf(nextField, startIndex);
            if (endIndex == -1 || endIndex < startIndex)
            {
                endIndex = text.Length;
            }
            var extractedValue = text.Substring(startIndex, endIndex - startIndex).Trim();
            var patternsToRemove = new[] { "PRECIOUNITARIO", "IMPORTE", "DESCUENTO", "OBJETOIMPUESTO", "SERVICIO" };
            foreach (var pattern in patternsToRemove)
            {
                var patternIndex = extractedValue.IndexOf(pattern);
                if (patternIndex != -1)
                {
                    extractedValue = extractedValue.Substring(patternIndex + pattern.Length).Trim();
                }
            }
            var dollarIndex = extractedValue.IndexOf('$');
            if (dollarIndex != -1)
            {
                extractedValue = extractedValue.Substring(0, dollarIndex).Trim();
            }
            extractedValue = "|" + extractedValue.Replace("\r", " ").Replace("\n", " ").Trim() + "|";
            Console.WriteLine($"Campo {fieldName} (descripción multilínea): {extractedValue}");
            return extractedValue;
        }

        private string ExtractTextBetweenDelimiters(string text, char delimiter = '|')
        {
            int startIndex = text.IndexOf(delimiter);
            if (startIndex == -1)
            {
                return string.Empty;
            }
            int endIndex = text.IndexOf(delimiter, startIndex + 1);
            if (endIndex == -1)
            {
                return string.Empty;
            }
            return text.Substring(startIndex + 1, endIndex - startIndex - 1).Trim();
        }

        private string ExtractDateTimeField(string text, string fieldName)
        {
            var startIndex = text.IndexOf(fieldName);
            if (startIndex == -1)
            {
                Console.WriteLine($"No se encontró el campo: {fieldName}");
                return string.Empty;
            }
            startIndex += fieldName.Length;
            var endIndex = startIndex + 19; // DateTime length "yyyy-MM-ddTHH:mm:ss"
            if (endIndex > text.Length)
            {
                endIndex = text.Length;
            }
            var extractedValue = text.Substring(startIndex, endIndex - startIndex).Trim();
            Console.WriteLine($"Campo {fieldName}: {extractedValue}");
            return extractedValue;
        }

        private string CleanupExtractedValue(string extractedValue)
        {
            var colonIndex = extractedValue.IndexOf(':');
            if (colonIndex != -1)
            {
                return extractedValue.Substring(colonIndex + 1).Trim();
            }
            return extractedValue.Trim();
        }

        private string CalculateTotal(string subtotal, string iva)
        {
            if (decimal.TryParse(CleanupCurrencyValue(subtotal), NumberStyles.Currency, CultureInfo.InvariantCulture, out var subtotalValue) &&
                decimal.TryParse(CleanupCurrencyValue(iva), NumberStyles.Currency, CultureInfo.InvariantCulture, out var ivaValue))
            {
                var totalValue = subtotalValue + ivaValue;
                var formattedTotal = totalValue.ToString("C", CultureInfo.GetCultureInfo("es-MX"));
                Console.WriteLine($"Total calculado: {formattedTotal}");
                return formattedTotal;
            }
            return string.Empty;
        }

        private string CleanupCurrencyValue(string value)
        {
            return value.Replace("$", "").Replace(",", "").Replace("¤", "").Trim();
        }
    }
}
