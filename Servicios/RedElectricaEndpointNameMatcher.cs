using System.Globalization;
using System.Text;
using System.Text.RegularExpressions;

namespace NSIE.Servicios;

internal enum RedElectricaEndpointNameMatchKind
{
    None = 0,
    ExtendedName = 1,
    SpellingVariant = 2,
    FunctionalQualifier = 3,
    Equivalent = 4,
    Exact = 5,
    LiteralExact = 6
}

internal sealed record RedElectricaEndpointNameMatch(
    bool IsMatch,
    RedElectricaEndpointNameMatchKind Kind,
    int Priority,
    string Evidence)
{
    public static RedElectricaEndpointNameMatch None { get; } =
        new(
            false,
            RedElectricaEndpointNameMatchKind.None,
            0,
            string.Empty);
}

internal static partial class RedElectricaEndpointNameMatcher
{
    private const double StrictDistanceKm = 0.25;

    private static readonly HashSet<string> EquivalenceStopWords = new(
        new[] { "DE", "DEL", "LA", "EL", "LOS", "LAS", "Y", "EN" },
        StringComparer.Ordinal);

    private static readonly HashSet<string> BankQualifiers = new(
        new[] { "BCO", "BCOS", "BANCO", "BANCOS" },
        StringComparer.Ordinal);

    public static RedElectricaEndpointNameMatch Evaluate(
        string declaredName,
        string catalogName,
        double distanceKm)
    {
        var declaredLiteral = NormalizeLiteralAlias(declaredName);
        var catalogLiteral = NormalizeLiteralAlias(catalogName);
        if (!string.IsNullOrWhiteSpace(declaredLiteral) &&
            string.Equals(
                declaredLiteral,
                catalogLiteral,
                StringComparison.Ordinal))
        {
            return new(
                true,
                RedElectricaEndpointNameMatchKind.LiteralExact,
                6,
                "coincidencia literal exacta");
        }

        var declaredAlias = NormalizeAlias(declaredLiteral);
        var catalogAlias = NormalizeAlias(catalogLiteral);
        if (string.IsNullOrWhiteSpace(declaredAlias) ||
            string.IsNullOrWhiteSpace(catalogAlias))
        {
            return RedElectricaEndpointNameMatch.None;
        }

        if (string.Equals(
                declaredAlias,
                catalogAlias,
                StringComparison.Ordinal))
        {
            return new(
                true,
                RedElectricaEndpointNameMatchKind.Exact,
                5,
                "coincidencia nominal exacta");
        }

        var declaredEquivalence = NormalizeEquivalenceKey(declaredAlias);
        var catalogEquivalence = NormalizeEquivalenceKey(catalogAlias);
        if (!string.IsNullOrWhiteSpace(declaredEquivalence) &&
            string.Equals(
                declaredEquivalence,
                catalogEquivalence,
                StringComparison.Ordinal))
        {
            return new(
                true,
                RedElectricaEndpointNameMatchKind.Equivalent,
                4,
                "equivalencia nominal controlada");
        }

        if (distanceKm > StrictDistanceKm)
        {
            return RedElectricaEndpointNameMatch.None;
        }

        var declaredTokens = declaredAlias.Split(
            ' ',
            StringSplitOptions.RemoveEmptyEntries);
        var catalogTokens = catalogAlias.Split(
            ' ',
            StringSplitOptions.RemoveEmptyEntries);
        if (IsControlledSpellingVariant(
                declaredTokens,
                catalogTokens))
        {
            return new(
                true,
                RedElectricaEndpointNameMatchKind.SpellingVariant,
                3,
                "variante ortográfica controlada a menos de 250 m");
        }

        var isCatalogExtension =
            catalogTokens.Length == declaredTokens.Length + 1 &&
            catalogAlias.StartsWith(
                $"{declaredAlias} ",
                StringComparison.Ordinal);
        if (!isCatalogExtension)
        {
            return RedElectricaEndpointNameMatch.None;
        }

        var addedToken = catalogTokens[^1];
        if (string.Equals(
                addedToken,
                "POTENCIA",
                StringComparison.Ordinal))
        {
            return new(
                true,
                RedElectricaEndpointNameMatchKind.FunctionalQualifier,
                3,
                "calificador POTENCIA corroborado a menos de 250 m");
        }

        return declaredTokens.Length >= 2 &&
               declaredAlias.Length >= 10
            ? new(
                true,
                RedElectricaEndpointNameMatchKind.ExtendedName,
                2,
                "nombre extendido único corroborado a menos de 250 m")
            : RedElectricaEndpointNameMatch.None;
    }

    private static bool IsControlledSpellingVariant(
        IReadOnlyList<string> declaredTokens,
        IReadOnlyList<string> catalogTokens)
    {
        if (declaredTokens.Count != catalogTokens.Count)
        {
            return false;
        }

        var differences = declaredTokens
            .Zip(
                catalogTokens,
                (declared, catalog) => (Declared: declared, Catalog: catalog))
            .Where(pair => !string.Equals(
                pair.Declared,
                pair.Catalog,
                StringComparison.Ordinal))
            .ToList();
        if (differences.Count != 1)
        {
            return false;
        }

        var difference = differences[0];
        return difference.Declared.Length >= 4 &&
            difference.Catalog.Length >= 4 &&
            !RomanOrNumberRegex().IsMatch(difference.Declared) &&
            !RomanOrNumberRegex().IsMatch(difference.Catalog) &&
            (LevenshteinDistance(
                 difference.Declared,
                 difference.Catalog) == 1 ||
             IsSingleAdjacentTransposition(
                 difference.Declared,
                 difference.Catalog));
    }

    private static bool IsSingleAdjacentTransposition(
        string left,
        string right)
    {
        if (left.Length != right.Length)
        {
            return false;
        }

        var differences = Enumerable.Range(0, left.Length)
            .Where(index => left[index] != right[index])
            .ToList();
        return differences.Count == 2 &&
            differences[1] == differences[0] + 1 &&
            left[differences[0]] == right[differences[1]] &&
            left[differences[1]] == right[differences[0]];
    }

    public static string NormalizeAlias(string value)
    {
        var normalized = NormalizeLiteralAlias(value);
        normalized = PotAbbreviationRegex().Replace(
            normalized,
            "POTENCIA");
        var tokens = normalized
            .Split(' ', StringSplitOptions.RemoveEmptyEntries)
            .ToArray();
        if (tokens.Length > 0)
        {
            tokens[^1] = NormalizeTrailingOrdinal(tokens[^1]);
        }
        return string.Join(' ', tokens).Trim();
    }

    private static string NormalizeTrailingOrdinal(string token) =>
        token switch
        {
            "UNO" or "I" => "1",
            "DOS" or "II" => "2",
            "TRES" or "III" => "3",
            "CUATRO" or "IV" => "4",
            "CINCO" or "V" => "5",
            _ => token
        };

    private static string NormalizeLiteralAlias(string value)
    {
        var normalized = NormalizeText(value);
        normalized = SubstationPrefixRegex().Replace(
            normalized,
            string.Empty);
        normalized = SetOrCfePrefixRegex().Replace(
            normalized,
            string.Empty);
        normalized = CfeSuffixRegex().Replace(
            normalized,
            string.Empty);
        normalized = CircuitSuffixRegex().Replace(
            normalized,
            string.Empty);
        return normalized.Trim();
    }

    private static string NormalizeEquivalenceKey(string value)
    {
        var tokens = NormalizeText(value)
            .Split(' ', StringSplitOptions.RemoveEmptyEntries)
            .Where(token => !EquivalenceStopWords.Contains(token))
            .ToList();
        var hasBankQualifier = tokens.Any(BankQualifiers.Contains);
        tokens = tokens
            .Where(token =>
                !BankQualifiers.Contains(token) &&
                !string.Equals(token, "PRESA", StringComparison.Ordinal) &&
                (!hasBankQualifier ||
                 !RomanOrNumberRegex().IsMatch(token)))
            .OrderBy(token => token, StringComparer.Ordinal)
            .ToList();
        return string.Join("|", tokens);
    }

    private static string NormalizeText(string value)
    {
        if (string.IsNullOrWhiteSpace(value))
        {
            return string.Empty;
        }

        var decomposed = value.Normalize(NormalizationForm.FormD);
        var builder = new StringBuilder(decomposed.Length);
        foreach (var character in decomposed)
        {
            if (CharUnicodeInfo.GetUnicodeCategory(character) !=
                UnicodeCategory.NonSpacingMark)
            {
                builder.Append(character);
            }
        }

        var normalized = builder
            .ToString()
            .Normalize(NormalizationForm.FormC)
            .ToUpperInvariant();
        normalized = NonAlphanumericRegex().Replace(normalized, " ");
        return WhitespaceRegex().Replace(normalized, " ").Trim();
    }

    private static int LevenshteinDistance(string left, string right)
    {
        var previous = new int[right.Length + 1];
        var current = new int[right.Length + 1];
        for (var column = 0; column <= right.Length; column++)
        {
            previous[column] = column;
        }

        for (var row = 1; row <= left.Length; row++)
        {
            current[0] = row;
            for (var column = 1; column <= right.Length; column++)
            {
                var substitutionCost =
                    left[row - 1] == right[column - 1] ? 0 : 1;
                current[column] = Math.Min(
                    Math.Min(
                        current[column - 1] + 1,
                        previous[column] + 1),
                    previous[column - 1] + substitutionCost);
            }
            (previous, current) = (current, previous);
        }
        return previous[right.Length];
    }

    [GeneratedRegex(
        @"^(?:CFE\s+)?(?:S\s*E|SUBESTACION(?:\s+ELECTRICA)?)\s+",
        RegexOptions.CultureInvariant)]
    private static partial Regex SubstationPrefixRegex();

    [GeneratedRegex(
        @"^(?:SET|CFE)\s+",
        RegexOptions.CultureInvariant)]
    private static partial Regex SetOrCfePrefixRegex();

    [GeneratedRegex(
        @"\s+DE\s+LA\s+CFE$",
        RegexOptions.CultureInvariant)]
    private static partial Regex CfeSuffixRegex();

    [GeneratedRegex(
        @"\s+\d{2}[A-Z]{3}\s+\d{2,3}$",
        RegexOptions.CultureInvariant)]
    private static partial Regex CircuitSuffixRegex();

    [GeneratedRegex(
        @"\bPOT\b",
        RegexOptions.CultureInvariant)]
    private static partial Regex PotAbbreviationRegex();

    [GeneratedRegex(
        @"^(?:\d+|[IVXLCDM]+)$",
        RegexOptions.CultureInvariant)]
    private static partial Regex RomanOrNumberRegex();

    [GeneratedRegex(
        @"[^A-Z0-9]+",
        RegexOptions.CultureInvariant)]
    private static partial Regex NonAlphanumericRegex();

    [GeneratedRegex(
        @"\s+",
        RegexOptions.CultureInvariant)]
    private static partial Regex WhitespaceRegex();
}
