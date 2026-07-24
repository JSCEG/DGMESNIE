namespace NSIE.Models;

public sealed class InegiTerritorialIndicator
{
    public string Key { get; init; } = string.Empty;
    public string Label { get; init; } = string.Empty;
    public double Value { get; init; }
    public string Unit { get; init; } = string.Empty;
    public string Period { get; init; } = string.Empty;
    public string Source { get; init; } = string.Empty;
    public string Detail { get; init; } = string.Empty;
    public int Decimals { get; init; }
}

public sealed class InegiTerritorialBreakdownItem
{
    public string Key { get; init; } = string.Empty;
    public string Label { get; init; } = string.Empty;
    public double Value { get; init; }
}

public sealed class InegiTerritorialBreakdown
{
    public string Key { get; init; } = string.Empty;
    public string Title { get; init; } = string.Empty;
    public string Subtitle { get; init; } = string.Empty;
    public string Unit { get; init; } = string.Empty;
    public string Period { get; init; } = string.Empty;
    public string Source { get; init; } = string.Empty;
    public int Decimals { get; init; }
    public IReadOnlyList<InegiTerritorialBreakdownItem> Items { get; init; } =
        Array.Empty<InegiTerritorialBreakdownItem>();
}

public sealed class InegiTerritorialResponse
{
    public string GeographicCode { get; init; } = string.Empty;
    public string Scope { get; init; } = string.Empty;
    public DateTimeOffset QueriedAt { get; init; }
    public IReadOnlyList<InegiTerritorialIndicator> Indicators { get; init; } =
        Array.Empty<InegiTerritorialIndicator>();
    public IReadOnlyList<InegiTerritorialBreakdown> Breakdowns { get; init; } =
        Array.Empty<InegiTerritorialBreakdown>();
}
