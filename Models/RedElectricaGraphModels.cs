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
    public string Phase { get; init; } = string.Empty;
    public string Source { get; init; } = string.Empty;
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
    public int? Circuits { get; init; }
    public double? CatalogLengthKm { get; init; }
    public double GeometryLengthKm { get; init; }
    public int SegmentIndex { get; init; }
    public required JsonElement Geometry { get; init; }
    public string Source { get; init; } = string.Empty;
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
