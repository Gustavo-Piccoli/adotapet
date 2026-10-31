using AdotPet.Domain.Shelters;

namespace AdotPet.Application.Tests.Fakes;

public sealed class InMemoryShelterRepository : IShelterRepository
{
    public List<Shelter> Items { get; } = [];

    public Task<Shelter?> GetByIdAsync(Guid id, CancellationToken ct) =>
        Task.FromResult(Items.FirstOrDefault(s => s.Id == id));

    public Task<Shelter?> GetByUserIdAsync(string userId, CancellationToken ct) =>
        Task.FromResult(Items.FirstOrDefault(s => s.UserId == userId));

    public Task<bool> CnpjExistsAsync(string cnpj, CancellationToken ct) =>
        Task.FromResult(Items.Any(s => s.Cnpj == cnpj));

    public void Add(Shelter shelter) => Items.Add(shelter);

    public static Shelter NewShelter(string city = "Porto Alegre", string state = "RS") =>
        Shelter.Create("ONG Patinhas", "11.222.333/0001-81", city, state,
            "(51) 99999-0000", "ong@example.com", "Resgate de animais.", DateTimeOffset.UtcNow);
}
