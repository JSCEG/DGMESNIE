using System.Text.Json;

namespace NSIE.Models;

public static class RedElectricaSubstationSources
{
    public const string DgmesnieGeoJson = "dgmesnie_geojson";
    public const string DgmesnieLineEndpoint = "dgmesnie_line_endpoint";
    public const string AtlasSen = "atlas_sen";
    public const string OpenStreetMap = "openstreetmap";
}

public static class RedElectricaSubstationReconciliationStates
{
    public const string CanonicalDgmesnie = "canonico_dgmesnie";
    public const string NameAndVoltageMatch = "coincidencia_nombre_tension";
    public const string SuggestedSpatialMatch = "coincidencia_espacial_sugerida";
    public const string NameVoltageConflict =
        "coincidencia_nombre_conflicto_tension";
    public const string AmbiguousMatch = "coincidencia_ambigua";
    public const string PendingGeoreferencing =
        "pendiente_georreferenciacion";
    public const string PendingWithCoordinates =
        "pendiente_con_coordenadas";
    public const string AutomaticGeoreference =
        "georreferenciacion_automatica";
}

public static class RedElectricaSubstationValidationStates
{
    public const string AutomaticOpenSource =
        "validada_automatica_fuente_abierta";
}

public sealed class RedElectricaSubstationInventorySummary
{
    public DateTime GeneratedUtc { get; init; }
    public long? GraphVersionId { get; init; }
    public int SourceRecords { get; init; }
    public int UniverseCandidates { get; init; }
    public int GeoreferencedUniverse { get; init; }
    public int PendingGeoreferencing { get; init; }
    public int DgmesnieRecords { get; init; }
    public int AtlasSenRecords { get; init; }
    public int OpenStreetMapRecords { get; init; }
    public int TransmissionRecords { get; init; }
    public int SubtransmissionRecords { get; init; }
    public int DistributionRecords { get; init; }
    public int ReconciledRecords { get; init; }
    public int PendingReconciliation { get; init; }
}

public sealed class RedElectricaSubstationInventoryRecord
{
    public string RecordKey { get; init; } = string.Empty;
    public string UniverseKey { get; init; } = string.Empty;
    public string SourceKey { get; init; } = string.Empty;
    public string ReferenceKey { get; init; } = string.Empty;
    public long? GraphVersionId { get; init; }
    public string CanonicalNodeKey { get; init; } = string.Empty;
    public string Name { get; init; } = string.Empty;
    public string NormalizedName { get; init; } = string.Empty;
    public double? VoltageKv { get; init; }
    public string NetworkLevel { get; init; } = "indeterminado";
    public double? Latitude { get; init; }
    public double? Longitude { get; init; }
    public string Region { get; init; } = string.Empty;
    public string Zone { get; init; } = string.Empty;
    public string TariffDivision { get; init; } = string.Empty;
    public string Operator { get; init; } = string.Empty;
    public string Source { get; init; } = string.Empty;
    public string CoordinateSourceKey { get; init; } = string.Empty;
    public string License { get; init; } = string.Empty;
    public string ReconciliationState { get; init; } = string.Empty;
    public string ValidationState { get; init; } = string.Empty;
    public JsonElement Metadata { get; init; }
    public DateTime FirstSeenUtc { get; init; }
    public DateTime LastSeenUtc { get; init; }
}

public sealed class RedElectricaSubstationPromotionSummary
{
    public long ExecutionId { get; init; }
    public DateTime PromotedUtc { get; init; }
    public int HighConfidenceCandidates { get; init; }
    public int PromotionsApplied { get; init; }
    public bool OfficialCoordinates { get; init; }
    public string ValidationState { get; init; } =
        RedElectricaSubstationValidationStates.AutomaticOpenSource;
    public RedElectricaSubstationInventorySummary Inventory { get; init; } =
        new();
}
