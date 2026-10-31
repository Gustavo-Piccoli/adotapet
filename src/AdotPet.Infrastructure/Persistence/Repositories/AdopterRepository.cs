using AdotPet.Domain.Adopters;
using Microsoft.EntityFrameworkCore;

namespace AdotPet.Infrastructure.Persistence.Repositories;

public sealed class AdopterRepository(AppDbContext db) : IAdopterRepository
{
    public Task<Adopter?> GetByIdAsync(Guid id, CancellationToken ct) =>
        db.Adopters.FirstOrDefaultAsync(a => a.Id == id, ct);

    public Task<Adopter?> GetByUserIdAsync(string userId, CancellationToken ct) =>
        db.Adopters.FirstOrDefaultAsync(a => a.UserId == userId, ct);

    public void Add(Adopter adopter) => db.Adopters.Add(adopter);
}
