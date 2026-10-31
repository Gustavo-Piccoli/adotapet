using AdotaPet.Domain.Adopters;

namespace AdotaPet.Application.Tests.Fakes;

public sealed class InMemoryAdopterRepository : IAdopterRepository
{
    public List<Adopter> Items { get; } = [];

    public Task<Adopter?> GetByIdAsync(Guid id, CancellationToken ct) =>
        Task.FromResult(Items.FirstOrDefault(a => a.Id == id));

    public Task<Adopter?> GetByUserIdAsync(string userId, CancellationToken ct) =>
        Task.FromResult(Items.FirstOrDefault(a => a.UserId == userId));

    public void Add(Adopter adopter) => Items.Add(adopter);
}
