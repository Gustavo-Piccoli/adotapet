namespace AdotPet.Domain.Common;

public static class Cnpj
{
    private static readonly int[] FirstWeights = [5, 4, 3, 2, 9, 8, 7, 6, 5, 4, 3, 2];
    private static readonly int[] SecondWeights = [6, 5, 4, 3, 2, 9, 8, 7, 6, 5, 4, 3, 2];

    public static string Normalize(string? input) =>
        new((input ?? string.Empty).Where(char.IsAsciiDigit).ToArray());

    public static bool IsValid(string digits)
    {
        if (digits is null || digits.Length != 14 || !digits.All(char.IsAsciiDigit)) return false;
        if (digits.All(c => c == digits[0])) return false;

        var first = CheckDigit(digits, FirstWeights);
        var second = CheckDigit(digits, SecondWeights);
        return digits[12] - '0' == first && digits[13] - '0' == second;
    }

    private static int CheckDigit(string digits, int[] weights)
    {
        var sum = 0;
        for (var i = 0; i < weights.Length; i++) sum += (digits[i] - '0') * weights[i];
        var remainder = sum % 11;
        return remainder < 2 ? 0 : 11 - remainder;
    }
}
