namespace NSIE.Servicios;

public static class RedElectricaNetworkLevels
{
    public const string Transmission = "transmision";
    public const string Subtransmission = "subtransmision";
    public const string Distribution = "distribucion";
    public const string Undetermined = "indeterminado";
}

public static class RedElectricaNetworkClassifier
{
    public static string Classify(
        double? voltageKv,
        string sourceKind = "",
        bool isVirtual = false)
    {
        if (isVirtual && !voltageKv.HasValue)
        {
            return RedElectricaNetworkLevels.Undetermined;
        }

        var source = (sourceKind ?? string.Empty).Trim().ToLowerInvariant();
        if (source is "atlas_transmission" or "cenace_transmission")
        {
            return RedElectricaNetworkLevels.Transmission;
        }

        if (source is "osm_distribution" or "cfe_rgd")
        {
            return voltageKv >= 69
                ? RedElectricaNetworkLevels.Subtransmission
                : RedElectricaNetworkLevels.Distribution;
        }

        if (!voltageKv.HasValue || voltageKv <= 0)
        {
            return RedElectricaNetworkLevels.Undetermined;
        }

        if (voltageKv >= 230)
        {
            return RedElectricaNetworkLevels.Transmission;
        }

        return voltageKv >= 69
            ? RedElectricaNetworkLevels.Subtransmission
            : RedElectricaNetworkLevels.Distribution;
    }

    public static string Label(string networkLevel) =>
        networkLevel switch
        {
            RedElectricaNetworkLevels.Transmission => "Transmisión",
            RedElectricaNetworkLevels.Subtransmission => "Subtransmisión",
            RedElectricaNetworkLevels.Distribution => "Distribución",
            _ => "Nivel por determinar"
        };
}
