using NSIE.Models;
using System.Globalization;
using System.Text;

namespace NSIE.Servicios;

public interface IPoliticaAccesoPermisosEnergeticos
{
    PermisoEnergeticoContextoAcceso Resolver(PerfilUsuario? perfil);
}

/// <summary>
/// Resuelve el nivel de detalle por afiliación institucional, no por el nombre
/// de una persona. Las cuentas externas conservan el filtro Rol/Mercado de
/// dbo.CamposMapas; las cuentas de trabajo DGMESNIE/SENER ven el inventario
/// institucional completo.
/// </summary>
public sealed class PoliticaAccesoPermisosEnergeticos : IPoliticaAccesoPermisosEnergeticos
{
    private static readonly string[] MarcadoresExternosPredeterminados =
    {
        "cenace",
        "cfe",
        "cenagas",
        "comision reguladora de energia",
        "externo"
    };

    private readonly HashSet<string> _dominiosInstitucionales;
    private readonly string[] _marcadoresExternos;

    public PoliticaAccesoPermisosEnergeticos(IConfiguration configuration)
    {
        var dominios = configuration
            .GetSection("PermisosEnergeticos:DominiosInstitucionales")
            .Get<string[]>();
        _dominiosInstitucionales = new HashSet<string>(
            dominios is { Length: > 0 } ? dominios : new[] { "energia.gob.mx" },
            StringComparer.OrdinalIgnoreCase);

        var externos = configuration
            .GetSection("PermisosEnergeticos:MarcadoresExternos")
            .Get<string[]>();
        _marcadoresExternos = (externos is { Length: > 0 }
                ? externos
                : MarcadoresExternosPredeterminados)
            .Select(Normalizar)
            .Where(value => value.Length > 0)
            .ToArray();
    }

    public PermisoEnergeticoContextoAcceso Resolver(PerfilUsuario? perfil)
    {
        if (perfil is null)
        {
            return PermisoEnergeticoContextoAcceso.Publico;
        }

        var correo = (perfil.Correo ?? string.Empty).Trim();
        var dominio = correo.Contains('@')
            ? correo[(correo.LastIndexOf('@') + 1)..]
            : string.Empty;
        var afiliacion = Normalizar(string.Join(
            " ",
            dominio,
            perfil.Unidad_de_Adscripcion,
            perfil.Cargo));
        var esExternoExplicito = _marcadoresExternos.Any(afiliacion.Contains);
        var esDominioInstitucional = _dominiosInstitucionales.Contains(dominio);
        var esUnidadInstitucional =
            afiliacion.Contains("dgmesnie", StringComparison.Ordinal) ||
            afiliacion.Contains("direccion general de metodologias y estadisticas del sistema nacional de informacion energetica", StringComparison.Ordinal) ||
            afiliacion.Contains("secretaria de energia", StringComparison.Ordinal) ||
            afiliacion.Contains("sener", StringComparison.Ordinal);

        // En el padrón vigente las cuentas autenticadas son de trabajo salvo las
        // afiliaciones externas expresamente identificadas. La configuración
        // permite ampliar dominios o marcadores sin tocar código.
        var esAutenticado =
            !string.IsNullOrWhiteSpace(perfil.IdUsuario) ||
            !string.IsNullOrWhiteSpace(correo);
        var esInstitucional =
            !esExternoExplicito &&
            (esDominioInstitucional || esUnidadInstitucional || esAutenticado);

        return new PermisoEnergeticoContextoAcceso(
            esInstitucional,
            ParseId(perfil.Rol_ID, perfil.Rol),
            ParseId(perfil.Mercado_ID),
            esInstitucional
                ? "Detalle institucional DGMESNIE"
                : "Consulta externa por perfil");
    }

    private static int ParseId(params string?[] candidates)
    {
        foreach (var candidate in candidates)
        {
            if (int.TryParse(candidate, out var value) && value >= 0)
            {
                return value;
            }
        }

        return 0;
    }

    private static string Normalizar(string? value)
    {
        var source = (value ?? string.Empty).Normalize(NormalizationForm.FormD);
        var buffer = new StringBuilder(source.Length);
        foreach (var character in source)
        {
            if (CharUnicodeInfo.GetUnicodeCategory(character) != UnicodeCategory.NonSpacingMark)
            {
                buffer.Append(char.ToLowerInvariant(character));
            }
        }

        return buffer.ToString().Normalize(NormalizationForm.FormC);
    }
}
