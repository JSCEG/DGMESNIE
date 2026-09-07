using System.IO.Compression;
using System.Text;
using System.Xml;
using System.Xml.Linq;
using iText.Kernel.Pdf;

namespace NSIE.Servicios;

/// <summary>Comprueba estructura de los archivos generados; no certifica su contenido ni su calidad visual.</summary>
public static class FichaAdjuntoValidador
{
    public static bool EsValido(byte[] archivo, string formato, int minPaginasPdf = 1)
    {
        if (archivo is not { Length: > 0 }) return false;
        try
        {
            return formato switch
            {
                "pdf" => EsPdfValido(archivo, Math.Max(1, minPaginasPdf)),
                "pptx" => EsPptxValido(archivo),
                _ => false
            };
        }
        catch (Exception exception) when (exception is not OutOfMemoryException)
        {
            // Un adjunto inválido debe rechazarse antes de invocar el servicio de correo.
            return false;
        }
    }

    private static bool EsPdfValido(byte[] archivo, int minimoPaginas)
    {
        if (archivo.Length < 5 || Encoding.ASCII.GetString(archivo, 0, 5) != "%PDF-") return false;
        var fin = archivo.Length;
        while (fin > 0 && archivo[fin - 1] is 9 or 10 or 12 or 13 or 32) fin--;
        if (fin < 5 || Encoding.ASCII.GetString(archivo, fin - 5, 5) != "%%EOF") return false;

        using var stream = new MemoryStream(archivo, writable: false);
        using var reader = new PdfReader(stream);
        using var pdf = new PdfDocument(reader);
        if (pdf.GetNumberOfPages() < minimoPaginas) return false;
        for (var numero = 1; numero <= pdf.GetNumberOfPages(); numero++)
        {
            var pagina = pdf.GetPage(numero);
            var dimensiones = pagina.GetMediaBox();
            if (dimensiones.GetWidth() <= 0 || dimensiones.GetHeight() <= 0) return false;
            // Fuerza la lectura de cada página y sus streams, no sólo de la cabecera.
            _ = pagina.GetContentBytes();
        }
        return !reader.HasRebuiltXref() && !reader.HasFixedXref();
    }

    private static bool EsPptxValido(byte[] archivo)
    {
        using var stream = new MemoryStream(archivo, writable: false);
        using var zip = new ZipArchive(stream, ZipArchiveMode.Read);
        XNamespace tipos = "http://schemas.openxmlformats.org/package/2006/content-types";
        XNamespace presentacion = "http://schemas.openxmlformats.org/presentationml/2006/main";
        XNamespace relaciones = "http://schemas.openxmlformats.org/package/2006/relationships";
        XNamespace referencia = "http://schemas.openxmlformats.org/officeDocument/2006/relationships";
        var contenido = LeerXml(zip, "[Content_Types].xml");
        if (contenido.Root?.Name != tipos + "Types" || !contenido.Root.Elements(tipos + "Override").Any(elemento =>
            (string?)elemento.Attribute("PartName") == "/ppt/presentation.xml" &&
            (string?)elemento.Attribute("ContentType") == "application/vnd.openxmlformats-officedocument.presentationml.presentation.main+xml")) return false;
        var documento = LeerXml(zip, "ppt/presentation.xml");
        if (documento.Root?.Name != presentacion + "presentation") return false;
        var diapositivas = documento.Root.Element(presentacion + "sldIdLst")?.Elements(presentacion + "sldId").ToList();
        if (diapositivas is not { Count: > 0 }) return false;
        var vinculos = LeerXml(zip, "ppt/_rels/presentation.xml.rels");
        if (vinculos.Root?.Name != relaciones + "Relationships") return false;
        foreach (var diapositiva in diapositivas)
        {
            var id = (string?)diapositiva.Attribute(referencia + "id");
            if (string.IsNullOrWhiteSpace(id)) return false;
            var relacion = vinculos.Root.Elements(relaciones + "Relationship").SingleOrDefault(elemento =>
                (string?)elemento.Attribute("Id") == id);
            if ((string?)relacion?.Attribute("Type") != referencia.NamespaceName + "/slide" ||
                string.Equals((string?)relacion?.Attribute("TargetMode"), "External", StringComparison.OrdinalIgnoreCase)) return false;
            var destino = (string?)relacion?.Attribute("Target");
            if (string.IsNullOrWhiteSpace(destino) || !System.Text.RegularExpressions.Regex.IsMatch(destino, @"^slides/slide\d+\.xml$")) return false;
            if (LeerXml(zip, "ppt/" + destino).Root?.Name != presentacion + "sld") return false;
        }
        return true;
    }

    private static XDocument LeerXml(ZipArchive zip, string nombre)
    {
        const int limiteXml = 4 * 1024 * 1024;
        var entrada = zip.GetEntry(nombre) ?? throw new InvalidDataException("Falta una parte del PowerPoint.");
        if (entrada.Length > limiteXml) throw new InvalidDataException("La parte XML excede el límite de validación.");
        using var stream = entrada.Open();
        using var reader = XmlReader.Create(stream, new XmlReaderSettings
        {
            DtdProcessing = DtdProcessing.Prohibit,
            XmlResolver = null,
            MaxCharactersInDocument = limiteXml
        });
        return XDocument.Load(reader);
    }
}
