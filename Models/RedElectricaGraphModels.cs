using System.Text.Json;

namespace NSIE.Models;

public sealed class RedElectricaGraphSummary
{
    public string Version { get; init; } = string.Empty;
    public string VersionReglas { get; init; } = "RED-GRAFO-v1.0";
    public DateTime GeneradoUtc { get; init; }
    public long DuracionConstruccionMs { get; init; }
    public int Subestaciones { get; init; }
    public int NodosVirtuales { get; init; }
    public int NodosTotales { get; init; }
    public int LineasCatalogo { get; init; }
    public int Aristas { get; init; }
    public int AristasConectadas { get; init; }
    public int AristasParciales { get; init; }
    public int AristasSinResolver { get; init; }
    public int Componentes { get; init; }
    public int NodosAislados { get; init; }
    public int RevisionesPendientes { get; init; }
    public int SubestacionesTransmision { get; set; }
    public int SubestacionesSubtransmision { get; set; }
    public int SubestacionesDistribucion { get; set; }
    public int LineasTransmision { get; set; }
    public int LineasSubtransmision { get; set; }
    public int LineasDistribucion { get; set; }
    public string FuenteSubestaciones { get; init; } = string.Empty;
    public string FuenteLineas { get; init; } = string.Empty;
    public string HashSubestaciones { get; init; } = string.Empty;
    public string HashLineas { get; init; } = string.Empty;
    public bool Persistida { get; set; }
    public long? VersionIdBaseDatos { get; set; }
}

public sealed class RedElectricaGraphSnapshot
{
    public required RedElectricaGraphSummary Summary { get; init; }
    public required IReadOnlyDictionary<string, RedElectricaGraphNode> Nodes { get; init; }
    public required IReadOnlyDictionary<string, RedElectricaGraphEdge> Edges { get; init; }
    public required IReadOnlyDictionary<string, IReadOnlyList<RedElectricaGraphAdjacency>> Adjacency { get; init; }
    public required IReadOnlyList<RedElectricaGraphReview> Reviews { get; init; }
}

public sealed class RedElectricaGraphSimulation
{
    public required RedElectricaGraphSummary BasePersistida { get; init; }
    public required RedElectricaGraphSummary Candidata { get; init; }
    public bool MismasFuentes { get; init; }
    public int AristasComparables { get; init; }
    public int AristasNuevas { get; init; }
    public int AristasRetiradas { get; init; }
    public int AristasConCambio { get; init; }
    public int PromovidasAConectada { get; init; }
    public int PromovidasAParcial { get; init; }
    public int Regresiones { get; init; }
    public int RevisionesReducidas { get; init; }
    public required IReadOnlyList<RedElectricaGraphSimulationChange> Cambios
    {
        get;
        init;
    }
    public required IReadOnlyList<RedElectricaGraphSimulationChange>
        RegresionesDetalle
    {
        get;
        init;
    }
    public required IReadOnlyList<RedElectricaGraphReview>
        RevisionesRegresion
    {
        get;
        init;
    }
}

public sealed class RedElectricaGraphSimulationChange
{
    public string EdgeId { get; init; } = string.Empty;
    public string NombreLinea { get; init; } = string.Empty;
    public string EstadoAnterior { get; init; } = string.Empty;
    public string EstadoCandidato { get; init; } = string.Empty;
    public string ExtremoOrigenNominal { get; init; } = string.Empty;
    public string ExtremoDestinoNominal { get; init; } = string.Empty;
    public string OrigenAnterior { get; init; } = string.Empty;
    public string OrigenCandidato { get; init; } = string.Empty;
    public string DestinoAnterior { get; init; } = string.Empty;
    public string DestinoCandidato { get; init; } = string.Empty;
    public int ConfianzaOrigenAnterior { get; init; }
    public int ConfianzaOrigenCandidata { get; init; }
    public int ConfianzaDestinoAnterior { get; init; }
    public int ConfianzaDestinoCandidata { get; init; }
    public string ResolucionOrigenCandidata { get; init; } = string.Empty;
    public string ResolucionDestinoCandidata { get; init; } = string.Empty;
}

public sealed class RedElectricaGraphNode
{
    public string NodeId { get; init; } = string.Empty;
    public string CatalogElementKey { get; init; } = string.Empty;
    public string Type { get; init; } = "subestacion";
    public string Name { get; init; } = string.Empty;
    public string NormalizedName { get; init; } = string.Empty;
    public double Latitude { get; init; }
    public double Longitude { get; init; }
    public double? VoltageKv { get; init; }
    public string NetworkLevel { get; set; } = "indeterminado";
    public string Phase { get; init; } = string.Empty;
    public string Source { get; init; } = string.Empty;
    public string SourceKind { get; init; } = "catalogo_dgmesnie";
    public string ValidationState { get; init; } = "catalogado";
    public bool IsVirtual { get; init; }
    public string VirtualReason { get; init; } = string.Empty;
    public int Degree { get; set; }
    public string ComponentId { get; set; } = string.Empty;
}

public sealed class RedElectricaGraphEdge
{
    public string EdgeId { get; init; } = string.Empty;
    public string CatalogElementKey { get; init; } = string.Empty;
    public string Name { get; init; } = string.Empty;
    public string NormalizedName { get; init; } = string.Empty;
    public string FromNodeId { get; init; } = string.Empty;
    public string ToNodeId { get; init; } = string.Empty;
    public string NominalEndpointA { get; init; } = string.Empty;
    public string NominalEndpointB { get; init; } = string.Empty;
    public int FromConfidence { get; init; }
    public int ToConfidence { get; init; }
    public string FromResolution { get; init; } = string.Empty;
    public string ToResolution { get; init; } = string.Empty;
    public string ConnectionState { get; init; } = string.Empty;
    public double? VoltageKv { get; init; }
    public string NetworkLevel { get; set; } = "indeterminado";
    public int? Circuits { get; init; }
    public double? CatalogLengthKm { get; init; }
    public double GeometryLengthKm { get; init; }
    public int SegmentIndex { get; init; }
    public required JsonElement Geometry { get; init; }
    public string Source { get; init; } = string.Empty;
    public string SourceKind { get; init; } = "catalogo_dgmesnie";
    public string ValidationState { get; init; } = "topologia_derivada";
}

public sealed class RedElectricaGraphAdjacency
{
    public string EdgeId { get; init; } = string.Empty;
    public string NeighborNodeId { get; init; } = string.Empty;
    public double WeightKm { get; init; }
}

public sealed class RedElectricaGraphCandidate
{
    public string NodeId { get; init; } = string.Empty;
    public string Name { get; init; } = string.Empty;
    public string NameMatchKind { get; init; } = string.Empty;
    public int Score { get; init; }
    public double DistanceKm { get; init; }
    public double? VoltageKv { get; init; }
    public IReadOnlyList<string> Evidence { get; init; } = Array.Empty<string>();
}

public sealed class RedElectricaGraphReview
{
    public string ReviewId { get; init; } = string.Empty;
    public string EdgeId { get; init; } = string.Empty;
    public string LineName { get; init; } = string.Empty;
    public string EndpointSide { get; init; } = string.Empty;
    public string NominalEndpoint { get; init; } = string.Empty;
    public double Latitude { get; init; }
    public double Longitude { get; init; }
    public string Reason { get; init; } = string.Empty;
    public IReadOnlyList<RedElectricaGraphCandidate> Candidates { get; init; } =
        Array.Empty<RedElectricaGraphCandidate>();
}

public sealed class RedElectricaGraphNeighborResult
{
    public required RedElectricaGraphNode Origin { get; init; }
    public int Depth { get; init; }
    public required IReadOnlyList<RedElectricaGraphNode> Nodes { get; init; }
    public required IReadOnlyList<RedElectricaGraphEdge> Edges { get; init; }
}

public sealed class RedElectricaGraphRouteResult
{
    public string OriginNodeId { get; init; } = string.Empty;
    public string DestinationNodeId { get; init; } = string.Empty;
    public bool Found { get; init; }
    public double TotalDistanceKm { get; init; }
    public required IReadOnlyList<RedElectricaGraphNode> Nodes { get; init; }
    public required IReadOnlyList<RedElectricaGraphEdge> Edges { get; init; }
}

public sealed class RedElectricaPamSubgraphResult
{
    public long ProjectId { get; init; }
    public string ProjectKey { get; init; } = string.Empty;
    public string ProjectName { get; init; } = string.Empty;
    public int Depth { get; init; }
    public int HighConfidenceAssociations { get; init; }
    public int ReviewAssociations { get; init; }
    public required IReadOnlyList<string> SeedElementKeys { get; init; }
    public required RedElectricaGeoJson GeoJson { get; init; }
}

public sealed class RedElectricaGeoJson
{
    public string Type { get; init; } = "FeatureCollection";
    public required RedElectricaGeoJsonMeta Meta { get; init; }
    public required IReadOnlyList<RedElectricaGeoJsonFeature> Features { get; init; }
}

public sealed class RedElectricaGeoJsonMeta
{
    public string Version { get; init; } = string.Empty;
    public string Layer { get; init; } = string.Empty;
    public DateTime GeneratedUtc { get; init; }
    public int Nodes { get; init; }
    public int Edges { get; init; }
    public int Reviews { get; init; }
    public string Source { get; init; } = string.Empty;
}

public sealed class RedElectricaGeoJsonFeature
{
    public string Type { get; init; } = "Feature";
    public required JsonElement Geometry { get; init; }
    public required IReadOnlyDictionary<string, object?> Properties { get; init; }
}
