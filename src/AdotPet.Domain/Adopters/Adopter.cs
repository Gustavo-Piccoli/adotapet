using AdotPet.Domain.Common;

namespace AdotPet.Domain.Adopters;

public sealed class Adopter
{
    private Adopter() { }

    public Guid Id { get; private set; }
    public string UserId { get; private set; } = string.Empty;
    public string Name { get; private set; } = string.Empty;
    public string Email { get; private set; } = string.Empty;
    public string Phone { get; private set; } = string.Empty;
    public string City { get; private set; } = string.Empty;
    public string State { get; private set; } = string.Empty;
    public DateTimeOffset CreatedAt { get; private set; }

    public static Adopter Create(string name, string email, string phone, string city,
        string state, DateTimeOffset now) =>
        new()
        {
            Id = Guid.NewGuid(),
            Name = Guard.Text(name, "Nome", 2, 80),
            Email = Guard.Email(email),
            Phone = Guard.Phone(phone),
            City = Guard.Text(city, "Cidade", 2, 80),
            State = Guard.State(state),
            CreatedAt = now,
        };

    public void AssignUser(string userId)
    {
        var valid = Guard.UserId(userId);
        if (UserId.Length > 0)
            throw new InvalidOperationException("O adotante já possui um usuário associado.");
        UserId = valid;
    }
}
