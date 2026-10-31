using AdotPet.Domain.Shelters;
using Microsoft.EntityFrameworkCore;

namespace AdotPet.Infrastructure.Persistence.Repositories;

public sealed class ShelterRepository(AppDbContext db) : IShelterRepository
{
    public Task<Shelter?> GetByIdAsync(Guid id, CancellationToken ct) =>
        db.Shelters.FirstOrDefaultAsync(s => s.Id == id, ct);

    public Task<Shelter?> GetByUserIdAsync(string userId, CancellationToken ct) =>
        db.Shelters.FirstOrDefaultAsync(s => s.UserId == userId, ct);

    public Task<bool> CnpjExistsAsync(string cnpj, CancellationToken ct) =>
        db.Shelters.AnyAsync(s => s.Cnpj == cnpj, ct);

    public void Add(Shelter shelter) => db.Shelters.Add(shelter);
}
