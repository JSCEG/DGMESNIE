using Microsoft.Extensions.Options;
using NSIE.Models;
using System.Globalization;
using System.Text;
using System.Text.Json;
using System.Text.RegularExpressions;

namespace NSIE.Servicios;

public interface ICfeRgdSubstationAuditService
{
    CfeRgdSubstationAudit Auditar(
        string nombreDeclarado,
        double? tensionKv,
        string gcr,
        string entidad,
        string municipio);
}

public sealed class CfeRgdSubstationAuditService :
    ICfeRgdSubstationAuditService
{
    private const string AuditRule = "CFE-RGD-AUDIT-v1";
    private readonly CfeRgdCatalog _catalog;
    private readonly ILogger<CfeRgdSubstationAuditService> _logger;

    public CfeRgdSubstationAuditService(
        IOptions<PamConvocatoriaEvidenceOptions> options,
        IWebHostEnvironment environment,
        ILogger<CfeRgdSubstationAuditService> logger)
    {
        _logger = logger;
        var configuredPath = string.IsNullOrWhiteSpace(
            options.Value.CfeRgdCatalogPath)
                ? "wwwroot/data/cfe_rgd_subestaciones_2026.json"
                : options.Value.CfeRgdCatalogPath.Trim();
        var path = Path.IsPathRooted(configuredPath)
            ? configuredPath
            : Path.GetFullPath(
                Path.Combine(environment.ContentRootPath, configuredPath));
        _catalog = LoadCatalog(path);
        _logger.LogInformation(
            "Catálogo CFE RGD cargado: {Records} bancos, fuente {Source}, SHA-256 {Sha256}.",
            _catalog.Records.Count,
            _catalog.SourceFile,
            _catalog.SourceSha256);
    }

    public CfeRgdSubstationAudit Auditar(
        string nombreDeclarado,
        double? tensionKv,
        string gcr,
        string entidad,
        string municipio)
    {
        var normalizedName = NormalizeName(nombreDeclarado);
        if (string.IsNullOrWhiteSpace(normalizedName))
        {
            return EmptyAudit("La referencia no contiene un nombre auditable.");
        }

        var exactMatches = _catalog.Records
            .Where(record =>
                string.Equals(
                    NormalizeName(record.Substation),
                    normalizedName,
                    StringComparison.Ordinal))
            .ToList();
        var controlledAlias = false;
        var nameMatches = exactMatches;
        if (nameMatches.Count == 0)
        {
            var alias = NormalizeControlledAlias(nombreDeclarado);
            if (!string.IsNullOrWhiteSpace(alias))
            {
                nameMatches = _catalog.Records
                    .Where(record =>
                        string.Equals(
                            NormalizeControlledAlias(record.Substation),
                            alias,
                            StringComparison.Ordinal))
                    .ToList();
                controlledAlias = nameMatches.Count > 0;
            }
        }

        if (nameMatches.Count == 0)
        {
            return EmptyAudit(
                "No existe una coincidencia nominal exacta ni un alias controlado en CFE RGD.");
        }

        var territoryMatches = nameMatches
            .Where(record => IsTerritoryCompatible(
                record.Division,
                gcr,
                entidad))
            .ToList();
        if (territoryMatches.Count == 0)
        {
            return CreateAudit(
                CfeRgdAuditStates.TerritoryMismatch,
                exactMatches.Count > 0,
                controlledAlias,
                territoryCompatible: false,
                tensionCompatible: false,
                nameMatches,
                tensionKv,
                new[]
                {
                    "El nombre aparece en CFE RGD, pero ninguna División es compatible con la entidad o GCR declarada.",
                    FormatTerritory(entidad, municipio, gcr)
                });
        }

        var distinctTerritories = territoryMatches
            .Select(record =>
                $"{Normalize(record.Division)}|{Normalize(record.Zone)}|" +
                NormalizeName(record.Substation))
            .Distinct(StringComparer.Ordinal)
            .Count();
        var voltageMatches = territoryMatches
            .Where(record => IsVoltageCompatible(
                tensionKv,
                record.AtKv,
                record.MtKv))
            .ToList();
        var voltageCompatible =
            !tensionKv.HasValue || voltageMatches.Count > 0;
        var backed =
            exactMatches.Count > 0 &&
            distinctTerritories == 1 &&
            voltageCompatible;
        var findings = new List<string>
        {
            exactMatches.Count > 0
                ? "Nombre exacto después de normalizar prefijos y numerales."
                : "Coincidencia obtenida sólo mediante alias controlado; requiere revisión nominal.",
            distinctTerritories == 1
                ? "La entidad o GCR reduce la coincidencia a un solo territorio CFE."
                : $"Persisten {distinctTerritories} territorios CFE compatibles.",
            !tensionKv.HasValue
                ? "La fuente de convocatoria no declara tensión; CFE aporta los niveles AT/MT."
                : voltageCompatible
                    ? $"La tensión declarada {tensionKv:0.##} kV aparece como AT o MT."
                    : $"La tensión declarada {tensionKv:0.##} kV no aparece como AT ni MT."
        };
        findings.Add(FormatTerritory(entidad, municipio, gcr));

        return CreateAudit(
            backed
                ? CfeRgdAuditStates.Backed
                : CfeRgdAuditStates.Review,
            exactMatches.Count > 0,
            controlledAlias,
            territoryCompatible: true,
            voltageCompatible,
            territoryMatches,
            tensionKv,
            findings);
    }

    private CfeRgdSubstationAudit EmptyAudit(string finding) =>
        new()
        {
            Estado = CfeRgdAuditStates.NoMatch,
            Regla = AuditRule,
            Fuente = _catalog.SourceFile,
            Sha256Fuente = _catalog.SourceSha256,
            AnioPublicacion = _catalog.PublicationYear,
            AnioHorizonte = _catalog.HorizonYear,
            Hallazgos = new[] { finding }
        };

    private CfeRgdSubstationAudit CreateAudit(
        string state,
        bool exactName,
        bool controlledAlias,
        bool territoryCompatible,
        bool tensionCompatible,
        IReadOnlyCollection<CfeRgdRecord> matches,
        double? declaredVoltageKv,
        IReadOnlyList<string> findings) =>
        new()
        {
            Estado = state,
            NombreExacto = exactName,
            AliasControlado = controlledAlias,
            TerritorioCompatible = territoryCompatible,
            TensionCompatible = tensionCompatible,
            RequiereValidacionHumana = true,
            Regla = AuditRule,
            Fuente = _catalog.SourceFile,
            Sha256Fuente = _catalog.SourceSha256,
            AnioPublicacion = _catalog.PublicationYear,
            AnioHorizonte = _catalog.HorizonYear,
            Hallazgos = findings,
            Coincidencias = matches
                .OrderBy(record => record.Division, StringComparer.Ordinal)
                .ThenBy(record => record.Zone, StringComparer.Ordinal)
                .ThenBy(record => record.Substation, StringComparer.Ordinal)
                .ThenBy(record => record.Bank, StringComparer.Ordinal)
                .Take(20)
                .Select(record => new CfeRgdSubstationMatch
                {
                    Division = record.Division,
                    Zona = record.Zone,
                    Subestacion = record.Substation,
                    Banco = record.Bank,
                    TensionAtKv = record.AtKv,
                    TensionMtKv = record.MtKv,
                    CapacidadMva = record.CapacityMva,
                    PaginaFuente = record.SourcePage,
                    TensionCompatible = IsVoltageCompatible(
                        declaredVoltageKv,
                        record.AtKv,
                        record.MtKv)
                })
                .ToList()
        };

    private static bool IsVoltageCompatible(
        double? declaredVoltageKv,
        double atKv,
        double mtKv) =>
        !declaredVoltageKv.HasValue ||
        Math.Abs(declaredVoltageKv.Value - atKv) <= 0.11 ||
        Math.Abs(declaredVoltageKv.Value - mtKv) <= 0.11;

    private static bool IsTerritoryCompatible(
        string division,
        string gcr,
        string entity)
    {
        var normalizedDivision = Normalize(division);
        var normalizedEntity = Normalize(entity);
        if (TerritoryByEntity.TryGetValue(
                normalizedEntity,
                out var allowedDivisions))
        {
            return allowedDivisions.Contains(normalizedDivision);
        }

        var normalizedGcr = Normalize(gcr);
        return TerritoryByGcr.TryGetValue(
                normalizedGcr,
                out allowedDivisions) &&
            allowedDivisions.Contains(normalizedDivision);
    }

    private static string FormatTerritory(
        string entity,
        string municipality,
        string gcr) =>
        $"Territorio declarado: {FirstNonEmpty(entity, "sin entidad")}" +
        (string.IsNullOrWhiteSpace(municipality)
            ? string.Empty
            : $" / {municipality.Trim()}") +
        $" · GCR {FirstNonEmpty(gcr, "sin dato")}.";

    private static string NormalizeControlledAlias(string value)
    {
        var normalized = NormalizeName(value);
        normalized = Regex.Replace(
            normalized,
            @"\s+(?:DE\s+)?POTENCIA$",
            string.Empty,
            RegexOptions.CultureInvariant);
        return normalized.Trim();
    }

    private static string NormalizeName(string value)
    {
        var normalized = Normalize(value);
        normalized = Regex.Replace(
            normalized,
            @"^(?:CFE\s+)?(?:S\s*E|SUBESTACION(?:\s+ELECTRICA)?)\s+",
            string.Empty,
            RegexOptions.CultureInvariant);
        normalized = Regex.Replace(
            normalized,
            @"(?:\s+AT)?\s+\d+(?:\.\d+)?\s*KV\b",
            string.Empty,
            RegexOptions.CultureInvariant);
        var tokens = normalized
            .Split(' ', StringSplitOptions.RemoveEmptyEntries)
            .Select(token => token switch
            {
                "I" or "UNO" => "1",
                "II" or "DOS" => "2",
                "III" or "TRES" => "3",
                _ => token
            });
        return string.Join(' ', tokens).Trim();
    }

    private static string Normalize(string? value)
    {
        if (string.IsNullOrWhiteSpace(value))
        {
            return string.Empty;
        }

        var builder = new StringBuilder(value.Length);
        foreach (var character in value.Normalize(NormalizationForm.FormD))
        {
            if (CharUnicodeInfo.GetUnicodeCategory(character) ==
                UnicodeCategory.NonSpacingMark)
            {
                continue;
            }
            builder.Append(char.IsLetterOrDigit(character)
                ? char.ToUpperInvariant(character)
                : ' ');
        }
        return Regex.Replace(
                builder.ToString(),
                @"\s+",
                " ",
                RegexOptions.CultureInvariant)
            .Trim();
    }

    private static string FirstNonEmpty(string value, string fallback) =>
        string.IsNullOrWhiteSpace(value) ? fallback : value.Trim();

    private static CfeRgdCatalog LoadCatalog(string path)
    {
        if (!File.Exists(path))
        {
            throw new InvalidDataException(
                $"No existe el catálogo derivado de CFE RGD: {path}");
        }

        using var stream = File.OpenRead(path);
        return JsonSerializer.Deserialize<CfeRgdCatalog>(
                   stream,
                   new JsonSerializerOptions
                   {
                       PropertyNameCaseInsensitive = true
                   }) ??
               throw new InvalidDataException(
                   $"El catálogo derivado de CFE RGD no es válido: {path}");
    }

    private static readonly IReadOnlyDictionary<string, IReadOnlySet<string>>
        TerritoryByGcr =
            new Dictionary<string, IReadOnlySet<string>>(StringComparer.Ordinal)
            {
                ["BAJA CALIFORNIA"] = Set("BAJA CALIFORNIA"),
                ["NOROESTE"] = Set("NOROESTE"),
                ["NORTE"] = Set("NORTE"),
                ["NORESTE"] = Set("GOLFO NORTE", "GOLFO CENTRO"),
                ["OCCIDENTE"] = Set(
                    "BAJIO",
                    "JALISCO",
                    "CENTRO OCCIDENTE"),
                ["CENTRAL"] = Set(
                    "CENTRO SUR",
                    "CENTRO ORIENTE",
                    "VALLE DE MEXICO NORTE",
                    "VALLE DE MEXICO CENTRO",
                    "VALLE DE MEXICO SUR"),
                ["ORIENTAL"] = Set(
                    "ORIENTE",
                    "SURESTE",
                    "PENINSULAR",
                    "GOLFO CENTRO")
            };

    private static readonly IReadOnlyDictionary<string, IReadOnlySet<string>>
        TerritoryByEntity =
            new Dictionary<string, IReadOnlySet<string>>(StringComparer.Ordinal)
            {
                ["BAJA CALIFORNIA"] = Set("BAJA CALIFORNIA"),
                ["BAJA CALIFORNIA SUR"] = Set("BAJA CALIFORNIA"),
                ["SONORA"] = Set("NOROESTE", "BAJA CALIFORNIA"),
                ["SINALOA"] = Set("NOROESTE"),
                ["CHIHUAHUA"] = Set("NORTE"),
                ["DURANGO"] = Set("NORTE"),
                ["COAHUILA"] = Set("NORTE", "GOLFO NORTE"),
                ["COAHUILA DE ZARAGOZA"] = Set("NORTE", "GOLFO NORTE"),
                ["NUEVO LEON"] = Set("GOLFO NORTE"),
                ["TAMAULIPAS"] = Set("GOLFO NORTE", "GOLFO CENTRO"),
                ["SAN LUIS POTOSI"] = Set(
                    "GOLFO NORTE",
                    "GOLFO CENTRO",
                    "BAJIO"),
                ["ZACATECAS"] = Set("BAJIO"),
                ["AGUASCALIENTES"] = Set("BAJIO"),
                ["GUANAJUATO"] = Set("BAJIO"),
                ["QUERETARO"] = Set("BAJIO"),
                ["NAYARIT"] = Set("JALISCO", "CENTRO OCCIDENTE"),
                ["JALISCO"] = Set("JALISCO"),
                ["COLIMA"] = Set("JALISCO"),
                ["MICHOACAN"] = Set("CENTRO OCCIDENTE"),
                ["MICHOACAN DE OCAMPO"] = Set("CENTRO OCCIDENTE"),
                ["GUERRERO"] = Set("CENTRO SUR"),
                ["MORELOS"] = Set("CENTRO SUR"),
                ["PUEBLA"] = Set("CENTRO ORIENTE", "ORIENTE"),
                ["TLAXCALA"] = Set("CENTRO ORIENTE", "ORIENTE"),
                ["VERACRUZ"] = Set("GOLFO CENTRO", "ORIENTE"),
                ["VERACRUZ DE IGNACIO DE LA LLAVE"] = Set(
                    "GOLFO CENTRO",
                    "ORIENTE"),
                ["OAXACA"] = Set("SURESTE"),
                ["CHIAPAS"] = Set("SURESTE"),
                ["TABASCO"] = Set("SURESTE"),
                ["CAMPECHE"] = Set("PENINSULAR"),
                ["YUCATAN"] = Set("PENINSULAR"),
                ["QUINTANA ROO"] = Set("PENINSULAR"),
                ["HIDALGO"] = Set(
                    "CENTRO ORIENTE",
                    "VALLE DE MEXICO NORTE"),
                ["MEXICO"] = Set(
                    "CENTRO SUR",
                    "CENTRO ORIENTE",
                    "VALLE DE MEXICO NORTE",
                    "VALLE DE MEXICO CENTRO",
                    "VALLE DE MEXICO SUR"),
                ["CIUDAD DE MEXICO"] = Set(
                    "VALLE DE MEXICO NORTE",
                    "VALLE DE MEXICO CENTRO",
                    "VALLE DE MEXICO SUR")
            };

    private static IReadOnlySet<string> Set(params string[] values) =>
        values
            .Select(Normalize)
            .ToHashSet(StringComparer.Ordinal);

    private sealed class CfeRgdCatalog
    {
        public string SourceFile { get; init; } = string.Empty;
        public string SourceSha256 { get; init; } = string.Empty;
        public int PublicationYear { get; init; }
        public int HorizonYear { get; init; }
        public IReadOnlyList<CfeRgdRecord> Records { get; init; } =
            Array.Empty<CfeRgdRecord>();
    }

    private sealed class CfeRgdRecord
    {
        public string Division { get; init; } = string.Empty;
        public string Zone { get; init; } = string.Empty;
        public string Substation { get; init; } = string.Empty;
        public string Bank { get; init; } = string.Empty;
        public double AtKv { get; init; }
        public double MtKv { get; init; }
        public double CapacityMva { get; init; }
        public int SourcePage { get; init; }
    }
}
