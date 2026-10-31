namespace AdotPet.Domain.Common;

internal static class Guard
{
    public static string Text(string? value, string label, int min, int max)
    {
        var trimmed = (value ?? string.Empty).Trim();
        if (trimmed.Length < min || trimmed.Length > max)
            throw new DomainValidationException($"{label} deve ter entre {min} e {max} caracteres.");
        return trimmed;
    }

    public static string State(string? value)
    {
        if (!BrazilianStates.IsValid(value))
            throw new DomainValidationException("Estado (UF) inválido.");
        return value!.Trim().ToUpperInvariant();
    }

    public static string Phone(string? value)
    {
        var digits = Cnpj.Normalize(value);
        if (digits.Length is < 10 or > 11)
            throw new DomainValidationException("Telefone deve ter 10 ou 11 dígitos.");
        return digits;
    }

    public static string Email(string? value)
    {
        var trimmed = (value ?? string.Empty).Trim();
        if (trimmed.Length > 254 || !trimmed.Contains('@'))
            throw new DomainValidationException("E-mail inválido.");
        return trimmed;
    }

    public static string UserId(string? value)
    {
        if (string.IsNullOrWhiteSpace(value))
            throw new DomainValidationException("Usuário inválido.");
        return value;
    }

    public static T Defined<T>(T value, string label) where T : struct, Enum
    {
        if (!Enum.IsDefined(value))
            throw new DomainValidationException($"{label} inválido.");
        return value;
    }
}
