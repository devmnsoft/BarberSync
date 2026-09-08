using System.Text.RegularExpressions;

using System.Collections.Generic;
using System.Linq;

namespace BarberSync.Domain.ValueObjects;

public static partial class BrazilianDocument
{
    public static string Normalize(string? value) => DigitsOnly().Replace(value?.Trim() ?? string.Empty, string.Empty);

    public static bool IsValidCpf(string? value)
    {
        var digits = Normalize(value);
        if (digits.Length != 11 || digits.Distinct().Count() == 1) return false;
        return CheckDigit(digits, 9, 10) == digits[9] - '0' && CheckDigit(digits, 10, 11) == digits[10] - '0';
    }

    public static bool IsValidCnpj(string? value)
    {
        var digits = Normalize(value);
        if (digits.Length != 14 || digits.Distinct().Count() == 1) return false;
        var firstWeights = new[] { 5, 4, 3, 2, 9, 8, 7, 6, 5, 4, 3, 2 };
        var secondWeights = new[] { 6, 5, 4, 3, 2, 9, 8, 7, 6, 5, 4, 3, 2 };
        return WeightedDigit(digits, firstWeights) == digits[12] - '0' && WeightedDigit(digits, secondWeights) == digits[13] - '0';
    }

    public static bool IsValid(string? value) => Normalize(value).Length switch
    {
        11 => IsValidCpf(value),
        14 => IsValidCnpj(value),
        _ => false
    };

    public static string Mask(string? value)
    {
        var digits = Normalize(value);
        return digits.Length switch
        {
            11 => $"***.{digits[3..6]}.{digits[6..9]}-**",
            14 => $"**.{digits[2..5]}.{digits[5..8]}/****-{digits[12..14]}",
            _ => string.Empty
        };
    }

    private static int CheckDigit(string digits, int length, int initialWeight)
    {
        var sum = 0;
        for (var index = 0; index < length; index++) sum += (digits[index] - '0') * (initialWeight - index);
        var result = 11 - sum % 11;
        return result >= 10 ? 0 : result;
    }

    private static int WeightedDigit(string digits, IReadOnlyList<int> weights)
    {
        var sum = 0;
        for (var index = 0; index < weights.Count; index++) sum += (digits[index] - '0') * weights[index];
        var remainder = sum % 11;
        return remainder < 2 ? 0 : 11 - remainder;
    }

    [GeneratedRegex("[^0-9]", RegexOptions.CultureInvariant)]
    private static partial Regex DigitsOnly();
}
