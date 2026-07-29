using System.Text.Json;
using System.Text.Json.Serialization;

namespace NSIE.Models;

public static class AtlasSenDatasetKeys
{
    public const string Atlas = "atlas";
    public const string OsmSubstations = "osm_substations";
    public const string TariffUsers = "tariff_users";
    public const string TariffEnergy = "tariff_energy";
    public const string Mda = "mda";
}

public sealed class AtlasSenStatus
{
    public DateTime CheckedUtc { get; init; }
    public bool UsedLocalFallback { get; init; }
    public bool HasPendingStructuralChanges { get; init; }
    public int TransmissionSubstations { get; init; }
    public int DistributionSubstations { get; init; }
    public int TransmissionLineFeatures { get; init; }
    public int TariffDivisions { get; init; }
    public int MdaZones { get; init; }
    public DateTime? MdaUpdatedUtc { get; init; }
    public DateOnly? MdaOperatingDate { get; init; }
    public IReadOnlyList<AtlasSenDatasetState> Datasets { get; init; } =
        Array.Empty<AtlasSenDatasetState>();
}

public sealed class AtlasSenDatasetState
{
    public string Key { get; init; } = string.Empty;
    public string Url { get; init; } = string.Empty;
    public string Branch { get; init; } = string.Empty;
    public string Sha256 { get; init; } = string.Empty;
    public string ETag { get; init; } = string.Empty;
    public DateTime CheckedUtc { get; init; }
    public DateTime LastChangedUtc { get; init; }
    public DateTime? SourceUpdatedUtc { get; init; }
    public int Records { get; init; }
    public string ChangeState { get; init; } = "sin_cambio";
    public bool PendingReview { get; init; }
    public bool AutoAccepted { get; init; }
    public bool FromLocalFallback { get; init; }
    public string Source { get; init; } = string.Empty;
    public string License { get; init; } = string.Empty;
}

public sealed class AtlasSenSubstationReference
{
    public string ReferenceId { get; init; } = string.Empty;
    public string Name { get; init; } = string.Empty;
    public string NormalizedName { get; init; } = string.Empty;
    public string NetworkLevel { get; init; } = "indeterminado";
    public double? VoltageKv { get; init; }
    public string VoltageRaw { get; init; } = string.Empty;
    public double? TransformerMva { get; init; }
    public double? Saturation { get; init; }
    public string Region { get; init; } = string.Empty;
    public string Zone { get; init; } = string.Empty;
    public string TariffDivision { get; init; } = string.Empty;
    public string Operator { get; init; } = string.Empty;
    public double? Latitude { get; init; }
    public double? Longitude { get; init; }
    public string SourceDataset { get; init; } = string.Empty;
    public string Source { get; init; } = string.Empty;
    public string License { get; init; } = string.Empty;
    public string ValidationState { get; init; } =
        "referencia_secundaria_pendiente_validacion";
}

public sealed class AtlasSenSubstationAudit
{
    public string State { get; init; } = "sin_coincidencia";
    public string DeclaredName { get; init; } = string.Empty;
    public double? DeclaredVoltageKv { get; init; }
    public bool ExactName { get; init; }
    public bool? VoltageCompatible { get; init; }
    public bool HasTransmissionReference { get; init; }
    public bool HasDistributionReference { get; init; }
    public bool RequiresHumanValidation { get; init; } = true;
    public string Rule { get; init; } = "ATLAS-SEN-AUDIT-v1";
    public IReadOnlyList<string> Findings { get; init; } =
        Array.Empty<string>();
    public IReadOnlyList<AtlasSenSubstationReference> Matches { get; init; } =
        Array.Empty<AtlasSenSubstationReference>();
}

public sealed class AtlasSenTariffOverview
{
    public IReadOnlyList<string> Divisions { get; init; } =
        Array.Empty<string>();
    public IReadOnlyList<string> UsersYears { get; init; } =
        Array.Empty<string>();
    public IReadOnlyList<string> EnergyYears { get; init; } =
        Array.Empty<string>();
    public string GeometrySource { get; init; } =
        "DOF municipio-división + geometría INEGI";
    public string SeriesSource { get; init; } =
        "CNE · memorias de cálculo del Suministro Básico";
    public string ValidationState { get; init; } =
        "referencia_secundaria_pendiente_comparacion";
}

public sealed class AtlasSenGeoJson
{
    public string Type { get; init; } = "FeatureCollection";
    public required AtlasSenGeoJsonMeta Meta { get; init; }
    public required IReadOnlyList<AtlasSenGeoJsonFeature> Features { get; init; }
}

public sealed class AtlasSenGeoJsonMeta
{
    public string Layer { get; init; } = string.Empty;
    public DateTime GeneratedUtc { get; init; }
    public int Features { get; init; }
    public string Source { get; init; } = string.Empty;
    public string License { get; init; } = string.Empty;
    public string ValidationState { get; init; } = string.Empty;
}

public sealed class AtlasSenGeoJsonFeature
{
    public string Type { get; init; } = "Feature";
    public required JsonElement Geometry { get; init; }
    public required IReadOnlyDictionary<string, object?> Properties { get; init; }
}

public sealed class AtlasSenMdaSnapshot
{
    [JsonPropertyName("updatedAt")]
    public DateTime UpdatedAt { get; init; }

    [JsonPropertyName("operatingDate")]
    public DateOnly OperatingDate { get; init; }

    [JsonPropertyName("source")]
    public string Source { get; init; } = "CENACE";

    [JsonPropertyName("market")]
    public string Market { get; init; } = "MDA";

    [JsonPropertyName("count")]
    public int Count { get; init; }

    [JsonPropertyName("zonas")]
    public IReadOnlyDictionary<string, AtlasSenMdaZone> Zones { get; init; } =
        new Dictionary<string, AtlasSenMdaZone>(StringComparer.OrdinalIgnoreCase);
}

public sealed class AtlasSenMdaZone
{
    [JsonPropertyName("system")]
    public string System { get; init; } = string.Empty;

    [JsonPropertyName("avgPND")]
    public double? AveragePnd { get; init; }

    [JsonPropertyName("hourly")]
    public IReadOnlyList<AtlasSenMdaHour> Hourly { get; init; } =
        Array.Empty<AtlasSenMdaHour>();
}

public sealed class AtlasSenMdaHour
{
    [JsonPropertyName("hora")]
    public int Hour { get; init; }

    [JsonPropertyName("pnd")]
    public double? Pnd { get; init; }

    [JsonPropertyName("energia")]
    public double? Energy { get; init; }

    [JsonPropertyName("perdidas")]
    public double? Losses { get; init; }

    [JsonPropertyName("congestion")]
    public double? Congestion { get; init; }
}

public sealed class AtlasSenMdaResponse
{
    public DateTime UpdatedAt { get; init; }
    public DateOnly OperatingDate { get; init; }
    public string Source { get; init; } = "CENACE";
    public string Market { get; init; } = "MDA";
    public int Count { get; init; }
    public bool IncludesHourlyDetail { get; init; }
    public IReadOnlyDictionary<string, AtlasSenMdaZone> Zones { get; init; } =
        new Dictionary<string, AtlasSenMdaZone>(StringComparer.OrdinalIgnoreCase);
}
