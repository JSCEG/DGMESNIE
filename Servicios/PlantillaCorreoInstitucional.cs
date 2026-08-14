using System;
using System.Collections.Generic;
using System.Linq;
using System.Net;
using System.Text;

namespace NSIE.Servicios
{
    /// <summary>
    /// Cuerpo HTML de los correos institucionales de la DGMESNIE.
    ///
    /// Está armado con tablas y estilos en línea a propósito: Outlook renderiza
    /// con el motor de Word y descarta flex, grid, border-radius y las hojas de
    /// estilo externas, así que un cuerpo hecho con &lt;div&gt; y clases se ve
    /// bien en el navegador y se desarma en el cliente donde más se lee.
    ///
    /// El texto entra en claro y se codifica aquí: quien arma un correo no debería
    /// tener que acordarse de escapar cada campo.
    /// </summary>
    public static class PlantillaCorreoInstitucional
    {
        // Los logos tienen que vivir en una URL pública: el cliente de correo no
        // trae la sesión del usuario, así que una ruta protegida de la plataforma
        // llegaría como imagen rota.
        private const string LogoGobierno = "https://cdn.sassoapps.com/dgmesnie/logo_gob.png";
        private const string LogoSener = "https://cdn.sassoapps.com/dgmesnie/logo_sener.png";

        private const string Guinda = "#9B2247";
        private const string Dorado = "#E0A12E";
        private const string Papel = "#faf8f5";
        private const string Tinta = "#1c1b1a";
        private const string Cuerpo = "#3a3835";
        private const string Gris = "#6F6B66";
        private const string GrisTenue = "#9A958E";
        private const string Linea = "#ece8e2";
        private const string Verde = "#0E7C5A";

        private const string Serif = "Georgia,'Times New Roman',serif";
        private const string Sans = "'Segoe UI',Arial,Helvetica,sans-serif";
        private const string Mono = "'Courier New',Courier,monospace";

        private const string Denominacion =
            "DGMESNIE · Dirección General de Metodología y Estadísticas del Sistema Nacional de Información Energética";

        public static string Construir(ContenidoCorreo contenido)
        {
            if (contenido is null) throw new ArgumentNullException(nameof(contenido));

            var html = new StringBuilder();
            html.Append($@"<table role=""presentation"" width=""100%"" cellpadding=""0"" cellspacing=""0"" border=""0"" style=""background:{Papel};margin:0;padding:26px 12px"">
<tr><td align=""center"">
<table role=""presentation"" width=""640"" cellpadding=""0"" cellspacing=""0"" border=""0"" style=""width:640px;max-width:100%;background:#ffffff;border:1px solid {Linea}"">");

            html.Append($@"
<tr><td style=""height:4px;line-height:4px;font-size:0;background:{Dorado}"">&nbsp;</td></tr>");

            EscribirEncabezado(html, contenido.Adscripcion);
            EscribirTitulo(html, contenido.Antetitulo, contenido.Titulo);
            EscribirMetadatos(html, contenido.Metadatos);

            html.Append($@"
<tr><td style=""height:1px;line-height:1px;font-size:0;background:{Dorado}"">&nbsp;</td></tr>");

            EscribirCuerpo(html, contenido);
            EscribirPuntos(html, contenido.SeccionTitulo, contenido.Puntos);
            EscribirAdjunto(html, contenido.AdjuntoNombre, contenido.AdjuntoFormato);
            EscribirNota(html, contenido.Nota);
            EscribirFirma(html, contenido.Firmante, contenido.FirmanteCargo);
            EscribirPie(html, contenido.PieAviso);

            html.Append(@"
</table>
</td></tr>
</table>");
            return html.ToString();
        }

        private static void EscribirEncabezado(StringBuilder html, string adscripcion)
        {
            var derecha = string.IsNullOrWhiteSpace(adscripcion)
                ? "Subsecretaría de Planeación y Transición Energética"
                : adscripcion;

            html.Append($@"
<tr><td style=""padding:26px 34px 0"">
  <table role=""presentation"" width=""100%"" cellpadding=""0"" cellspacing=""0"" border=""0"">
    <tr>
      <td align=""left"" valign=""middle"" style=""width:auto"">
        <img src=""{LogoGobierno}"" width=""104"" alt=""Gobierno de México"" style=""display:inline-block;height:auto;border:0;vertical-align:middle"">
        <img src=""{LogoSener}"" width=""86"" alt=""Secretaría de Energía"" style=""display:inline-block;height:auto;border:0;vertical-align:middle;margin-left:14px"">
      </td>
      <td align=""right"" valign=""middle"" style=""font-family:{Sans};font-size:9px;font-weight:700;letter-spacing:.1em;text-transform:uppercase;color:{Guinda};line-height:1.5"">
        {Cod(derecha)}
      </td>
    </tr>
  </table>
</td></tr>
<tr><td style=""padding:20px 34px 0""><table role=""presentation"" width=""100%"" cellpadding=""0"" cellspacing=""0"" border=""0""><tr><td style=""height:1px;line-height:1px;font-size:0;background:{Linea}"">&nbsp;</td></tr></table></td></tr>");
        }

        private static void EscribirTitulo(StringBuilder html, string antetitulo, string titulo)
        {
            html.Append($@"
<tr><td style=""padding:24px 34px 0"">");

            if (!string.IsNullOrWhiteSpace(antetitulo))
            {
                html.Append($@"
  <div style=""font-family:{Sans};font-size:9.5px;font-weight:700;letter-spacing:.14em;text-transform:uppercase;color:{Guinda};margin:0 0 12px"">
    <span style=""color:{Dorado}"">&#9670;</span>&nbsp; {Cod(antetitulo)}
  </div>");
            }

            html.Append($@"
  <div style=""font-family:{Serif};font-size:29px;line-height:1.18;font-weight:700;color:{Tinta};margin:0"">{Cod(titulo)}</div>
</td></tr>");
        }

        private static void EscribirMetadatos(StringBuilder html, IReadOnlyList<CampoCorreo> metadatos)
        {
            if (metadatos is null || metadatos.Count == 0)
            {
                html.Append(@"
<tr><td style=""height:24px;line-height:24px;font-size:0"">&nbsp;</td></tr>");
                return;
            }

            html.Append($@"
<tr><td style=""padding:22px 34px 24px"">
  <table role=""presentation"" cellpadding=""0"" cellspacing=""0"" border=""0"">
    <tr>");

            foreach (var campo in metadatos.Take(3))
            {
                html.Append($@"
      <td valign=""top"" style=""padding-right:34px;border-left:2px solid {Dorado};padding-left:11px"">
        <div style=""font-family:{Sans};font-size:8.5px;font-weight:700;letter-spacing:.12em;text-transform:uppercase;color:{Gris};margin-bottom:5px"">{Cod(campo.Etiqueta)}</div>
        <div style=""font-family:{Mono};font-size:12.5px;color:{Tinta}"">{Cod(campo.Valor)}</div>
      </td>");
            }

            html.Append(@"
    </tr>
  </table>
</td></tr>");
        }

        private static void EscribirCuerpo(StringBuilder html, ContenidoCorreo contenido)
        {
            html.Append($@"
<tr><td style=""padding:26px 34px 0"">");

            if (!string.IsNullOrWhiteSpace(contenido.Saludo))
            {
                html.Append($@"
  <p style=""margin:0 0 16px;font-family:{Sans};font-size:14px;color:{Tinta}"">{Cod(contenido.Saludo)}:</p>");
            }

            foreach (var parrafo in contenido.Parrafos ?? Array.Empty<string>())
            {
                if (string.IsNullOrWhiteSpace(parrafo)) continue;
                html.Append($@"
  <p style=""margin:0 0 15px;font-family:{Sans};font-size:13.5px;line-height:1.65;color:{Cuerpo}"">{parrafo}</p>");
            }

            html.Append(@"
</td></tr>");
        }

        private static void EscribirPuntos(StringBuilder html, string titulo, IReadOnlyList<CampoCorreo> puntos)
        {
            if (puntos is null || puntos.Count == 0) return;

            // Filete ornamental: línea, rombo, línea. Es el mismo recurso que
            // separa bloques en los informes impresos.
            html.Append($@"
<tr><td style=""padding:12px 34px 0"">
  <table role=""presentation"" width=""100%"" cellpadding=""0"" cellspacing=""0"" border=""0"">
    <tr>
      <td style=""height:1px;line-height:1px;font-size:0;background:{Linea}"">&nbsp;</td>
      <td width=""34"" align=""center"" style=""font-family:{Sans};font-size:11px;color:{Dorado};line-height:1"">&#9670;</td>
      <td style=""height:1px;line-height:1px;font-size:0;background:{Linea}"">&nbsp;</td>
    </tr>
  </table>
</td></tr>");

            if (!string.IsNullOrWhiteSpace(titulo))
            {
                html.Append($@"
<tr><td style=""padding:26px 34px 0"">
  <div style=""font-family:{Sans};font-size:9.5px;font-weight:700;letter-spacing:.14em;text-transform:uppercase;color:{Guinda};padding-bottom:9px;border-bottom:1px solid {Guinda}"">{Cod(titulo)}</div>
</td></tr>");
            }

            html.Append($@"
<tr><td style=""padding:0 34px"">
  <table role=""presentation"" width=""100%"" cellpadding=""0"" cellspacing=""0"" border=""0"">");

            foreach (var punto in puntos)
            {
                html.Append($@"
    <tr>
      <td valign=""top"" width=""22"" style=""padding:13px 0;border-bottom:1px solid {Linea};font-family:{Sans};font-size:13px;color:{Verde}"">&#10003;</td>
      <td valign=""top"" width=""150"" style=""padding:13px 12px 13px 0;border-bottom:1px solid {Linea};font-family:{Sans};font-size:9.5px;font-weight:700;letter-spacing:.09em;text-transform:uppercase;color:{Guinda};line-height:1.5"">{Cod(punto.Etiqueta)}</td>
      <td valign=""top"" style=""padding:13px 0;border-bottom:1px solid {Linea};font-family:{Sans};font-size:12.5px;line-height:1.6;color:{Cuerpo}"">{Cod(punto.Valor)}</td>
    </tr>");
            }

            html.Append(@"
  </table>
</td></tr>");
        }

        private static void EscribirAdjunto(StringBuilder html, string nombre, string formato)
        {
            if (string.IsNullOrWhiteSpace(nombre)) return;
            var etiqueta = string.IsNullOrWhiteSpace(formato) ? "PDF" : formato.ToUpperInvariant();

            html.Append($@"
<tr><td style=""padding:24px 34px 0"">
  <table role=""presentation"" width=""100%"" cellpadding=""0"" cellspacing=""0"" border=""0"" style=""background:{Papel};border:1px solid {Linea}"">
    <tr>
      <td valign=""middle"" width=""62"" style=""padding:15px 0 15px 16px"">
        <table role=""presentation"" cellpadding=""0"" cellspacing=""0"" border=""0"" style=""background:{Guinda}"">
          <tr><td align=""center"" style=""padding:9px 11px;font-family:{Sans};font-size:9.5px;font-weight:700;letter-spacing:.06em;color:#ffffff"">{Cod(etiqueta)}</td></tr>
        </table>
      </td>
      <td valign=""middle"" style=""padding:15px 16px 15px 0"">
        <div style=""font-family:{Sans};font-size:12.5px;font-weight:700;color:{Tinta};margin-bottom:3px"">Documento adjunto</div>
        <div style=""font-family:{Mono};font-size:11px;color:{Gris};word-break:break-all"">{Cod(nombre)}</div>
      </td>
      <td valign=""middle"" align=""right"" width=""30"" style=""padding:15px 16px 15px 0;font-family:{Sans};font-size:10px;color:{Dorado}"">&#9670;</td>
    </tr>
  </table>
</td></tr>");
        }

        private static void EscribirNota(StringBuilder html, string nota)
        {
            if (string.IsNullOrWhiteSpace(nota)) return;

            html.Append($@"
<tr><td style=""padding:22px 34px 0"">
  <table role=""presentation"" width=""100%"" cellpadding=""0"" cellspacing=""0"" border=""0"" style=""background:#fdf6e9"">
    <tr><td style=""padding:14px 17px;font-family:{Sans};font-size:11.5px;line-height:1.6;color:{Cuerpo}"">
      <strong style=""color:{Guinda}"">Nota:</strong> {Cod(nota)}
    </td></tr>
  </table>
</td></tr>");
        }

        private static void EscribirFirma(StringBuilder html, string firmante, string cargo)
        {
            if (string.IsNullOrWhiteSpace(firmante)) return;

            html.Append($@"
<tr><td style=""padding:26px 34px 0;font-family:{Sans};font-size:12.5px;line-height:1.6;color:{Gris}"">
  Atentamente,<br>
  <strong style=""color:{Tinta}"">{Cod(firmante)}</strong><br>
  {Cod(string.IsNullOrWhiteSpace(cargo) ? "Secretaría de Energía · DGMESNIE" : cargo)}
</td></tr>");
        }

        private static void EscribirPie(StringBuilder html, string aviso)
        {
            html.Append($@"
<tr><td style=""padding:28px 34px 0""><table role=""presentation"" width=""100%"" cellpadding=""0"" cellspacing=""0"" border=""0""><tr><td style=""height:1px;line-height:1px;font-size:0;background:{Dorado}"">&nbsp;</td></tr></table></td></tr>
<tr><td style=""padding:16px 34px 26px"">
  <table role=""presentation"" width=""100%"" cellpadding=""0"" cellspacing=""0"" border=""0"">
    <tr>
      <td style=""font-family:{Sans};font-size:9px;font-weight:700;letter-spacing:.1em;text-transform:uppercase;color:{Tinta}"">DGMESNIE</td>
      <td align=""right"" style=""font-family:{Sans};font-size:10px;color:{Dorado}"">&#9670;</td>
    </tr>
    <tr><td colspan=""2"" style=""padding-top:9px;font-family:{Sans};font-size:10px;line-height:1.6;color:{GrisTenue}"">
      {Cod(Denominacion)}<br>
      {Cod(string.IsNullOrWhiteSpace(aviso) ? "Correo generado automáticamente por la plataforma DGMESNIE. Por favor no responda a este mensaje." : aviso)}
    </td></tr>
  </table>
</td></tr>");
        }

        private static string Cod(string valor) => WebUtility.HtmlEncode(valor ?? string.Empty);
    }

    public sealed class ContenidoCorreo
    {
        public string Adscripcion { get; set; } = string.Empty;
        public string Antetitulo { get; set; } = string.Empty;
        public string Titulo { get; set; } = string.Empty;
        public IReadOnlyList<CampoCorreo> Metadatos { get; set; } = Array.Empty<CampoCorreo>();
        public string Saludo { get; set; } = string.Empty;

        /// <summary>Párrafos ya en HTML: pueden traer &lt;strong&gt;. El texto que
        /// venga del usuario debe codificarse antes de ponerlo aquí.</summary>
        public IReadOnlyList<string> Parrafos { get; set; } = Array.Empty<string>();

        public string SeccionTitulo { get; set; } = string.Empty;
        public IReadOnlyList<CampoCorreo> Puntos { get; set; } = Array.Empty<CampoCorreo>();
        public string AdjuntoNombre { get; set; } = string.Empty;
        public string AdjuntoFormato { get; set; } = string.Empty;
        public string Nota { get; set; } = string.Empty;
        public string Firmante { get; set; } = string.Empty;
        public string FirmanteCargo { get; set; } = string.Empty;
        public string PieAviso { get; set; } = string.Empty;
    }

    public sealed class CampoCorreo
    {
        public CampoCorreo(string etiqueta, string valor)
        {
            Etiqueta = etiqueta ?? string.Empty;
            Valor = valor ?? string.Empty;
        }

        public string Etiqueta { get; }
        public string Valor { get; }
    }
}
