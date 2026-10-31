using AdotPet.Domain.Common;

namespace AdotPet.Domain.Shelters;

public sealed class Shelter
{
    private Shelter() { }

    public Guid Id { get; private set; }
    public string UserId { get; private set; } = string.Empty;
    public string Name { get; private set; } = string.Empty;
    public string Cnpj { get; private set; } = string.Empty;
    public string City { get; private set; } = string.Empty;
    public string State { get; private set; } = string.Empty;
    public string Phone { get; private set; } = string.Empty;
    public string Email { get; private set; } = string.Empty;
    public string Description { get; private set; } = string.Empty;
    public DateTimeOffset CreatedAt { get; private set; }

    public static Shelter Create(string name, string cnpj, string city, string state,
        string phone, string email, string description, DateTimeOffset now)
    {
        var normalizedCnpj = Common.Cnpj.Normalize(cnpj);
        if (!Common.Cnpj.IsValid(normalizedCnpj))
            throw new DomainValidationException("CNPJ inválido.");

        var trimmedDescription = (description ?? string.Empty).Trim();
        if (trimmedDescription.Length > 1000)
            throw new DomainValidationException("Descrição deve ter no máximo 1000 caracteres.");

        return new Shelter
        {
            Id = Guid.NewGuid(),
            Name = Guard.Text(name, "Nome", 2, 80),
            Cnpj = normalizedCnpj,
            City = Guard.Text(city, "Cidade", 2, 80),
            State = Guard.State(state),
            Phone = Guard.Phone(phone),
            Email = Guard.Email(email),
            Description = trimmedDescription,
            CreatedAt = now,
        };
    }

    public void AssignUser(string userId)
    {
        var valid = Guard.UserId(userId);
        if (UserId.Length > 0)
            throw new InvalidOperationException("A ONG já possui um usuário associado.");
        UserId = valid;
    }
}
