using System.Globalization;

namespace NSIE.Models.ProyectosPrivados;

/// <summary>
/// Acceso uniforme a los campos declarados en la solicitud (BD_MIXTOS_II) desde las láminas de la
/// ficha Mixtos II. Los encabezados repetidos del libro (etapas "Fecha"/"Capacidad", factor de planta
/// FV/eólico, RTE del SAE) se distinguen por su ocurrencia en el orden de columnas.
/// </summary>
public static class ConvocatoriaFichaCampos
{
    public const string HojaTecnica = "BD_MIXTOS_II";
    private static readonly CultureInfo Mx = CultureInfo.GetCultureInfo("es-MX");
    private static readonly string[] Vacios =
        { "SIN INFORMACIÓN", "SIN INFORMACION", "NO APLICA", "N/D", "S/D", "NULL", "NAN", "-", "0000-00-00" };

    public static bool Tiene(string? valor) =>
        !string.IsNullOrWhiteSpace(valor) && !Vacios.Contains(valor.Trim(), StringComparer.OrdinalIgnoreCase);

    public static bool EsNoAplica(string? valor) =>
        !string.IsNullOrWhiteSpace(valor) && string.Equals(valor.Trim(), "NO APLICA", StringComparison.OrdinalIgnoreCase);

    public static string Texto(string? valor, string vacio = "No informado") => Tiene(valor) ? valor!.Trim() : vacio;

    public static string Recorte(string? valor, int maximo, string vacio = "No informado")
    {
        var texto = Texto(valor, vacio).Replace("\r", " ").Replace("\n", " ");
        return texto.Length > maximo ? texto[..maximo].TrimEnd() + "…" : texto;
    }

    public static bool EsSi(string? valor) =>
        Tiene(valor) && valor!.Trim().ToUpperInvariant() is "SI" or "SÍ" or "YES" or "1" or "TRUE";

    public static bool EsNo(string? valor) =>
        Tiene(valor) && valor!.Trim().ToUpperInvariant() is "NO" or "0" or "FALSE";

    public static string Fecha(string? valor, string vacio = "No informada")
    {
        if (!Tiene(valor)) return vacio;
        return DateTime.TryParseExact(valor!.Trim(), new[] { "yyyy-MM-dd", "yyyy-MM-dd HH:mm:ss", "dd/MM/yyyy", "d/M/yyyy" },
            CultureInfo.InvariantCulture, DateTimeStyles.None, out var fecha)
            ? fecha.ToString("dd/MM/yyyy", Mx)
            : valor.Trim();
    }

    public static DateTime? FechaValor(string? valor) =>
        Tiene(valor) && DateTime.TryParseExact(valor!.Trim(), new[] { "yyyy-MM-dd", "yyyy-MM-dd HH:mm:ss", "dd/MM/yyyy", "d/M/yyyy" },
            CultureInfo.InvariantCulture, DateTimeStyles.None, out var fecha) ? fecha : null;

    public static decimal? NumeroValor(string? valor) =>
        Tiene(valor) && decimal.TryParse(valor!.Trim(), NumberStyles.Any, CultureInfo.InvariantCulture, out var numero) ? numero : null;

    public static string Numero(string? valor, int decimales = 2, string vacio = "No informado")
    {
        var numero = NumeroValor(valor);
        return numero.HasValue ? numero.Value.ToString("N" + decimales, Mx) : Texto(valor, vacio);
    }

    public static ConvocatoriaRegistroFuente? Registro(this CarteraConvocatoriaExpediente? expediente, string hoja = HojaTecnica) =>
        expediente?.Records.FirstOrDefault(registro => registro.Sheet == hoja);

    public static ConvocatoriaCampoFuente? Campo(this CarteraConvocatoriaExpediente? expediente, string nombre, int ocurrencia = 1, string hoja = HojaTecnica) =>
        expediente.Registro(hoja)?.Fields
            .Where(campo => string.Equals(campo.Name.Trim(), nombre, StringComparison.OrdinalIgnoreCase))
            .Skip(ocurrencia - 1)
            .FirstOrDefault();

    public static string? Valor(this CarteraConvocatoriaExpediente? expediente, string nombre, int ocurrencia = 1, string hoja = HojaTecnica) =>
        expediente.Campo(nombre, ocurrencia, hoja)?.Value;

    public static string Dato(this CarteraConvocatoriaExpediente? expediente, string nombre, int ocurrencia = 1, string vacio = "No informado") =>
        Texto(expediente.Valor(nombre, ocurrencia), vacio);

    /// <summary>Referencia de celda (columna+fila) para trazabilidad; vacío si el campo no viene en el corte.</summary>
    public static string Celda(this CarteraConvocatoriaExpediente? expediente, string nombre, int ocurrencia = 1, string hoja = HojaTecnica)
    {
        var registro = expediente.Registro(hoja);
        var campo = expediente.Campo(nombre, ocurrencia, hoja);
        return registro == null || campo == null ? string.Empty : $"{campo.Column}{registro.Row}";
    }

    /// <summary>Sólo ligas https de la ventanilla o del repositorio documental institucional.</summary>
    public static string? Enlace(this CarteraConvocatoriaExpediente? expediente, string nombre, int ocurrencia = 1)
    {
        var campo = expediente.Campo(nombre, ocurrencia);
        var valor = campo?.Link ?? campo?.Value;
        return Uri.TryCreate(valor, UriKind.Absolute, out var url) && url.Scheme == Uri.UriSchemeHttps && url.IsDefaultPort &&
               string.IsNullOrEmpty(url.UserInfo) &&
               (url.Host == "ventanillainstituciones.energia.gob.mx" || url.Host == "ti.energia.gob.mx")
            ? url.AbsoluteUri
            : null;
    }

    /// <summary>Fecha de corte de la convocatoria en formato institucional (sin nombres de archivo).</summary>
    public static string Corte(this CarteraConvocatoriaFichaViewModel model)
    {
        var fecha = model.LatestImport?.CutoffDate ?? model.Dossier?.CutoffDate;
        return fecha.HasValue ? "corte " + fecha.Value.ToString("dd 'de' MMMM 'de' yyyy", Mx) : "corte vigente";
    }

    /// <summary>Línea de fuente para el pie de lámina: "Fuente: {origen} · Convocatoria Mixtos II, corte …".</summary>
    public static string Fuente(this CarteraConvocatoriaFichaViewModel model, string origen) =>
        $"Fuente: {origen} · Convocatoria Mixtos II, {model.Corte()}.";

    /// <summary>Clase visual para respuestas Sí/No; "is-na" cuando la fuente no informa.</summary>
    public static string Bandera(string? valor) => EsSi(valor) ? "is-yes" : EsNo(valor) ? "is-no" : "is-na";

    public static string BanderaTexto(string? valor) => EsSi(valor) ? "Sí" : EsNo(valor) ? "No" : Texto(valor);
}
