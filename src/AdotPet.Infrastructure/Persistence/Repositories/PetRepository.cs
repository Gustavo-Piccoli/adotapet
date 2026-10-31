using AdotPet.Domain.Common;
using AdotPet.Domain.Pets;
using Microsoft.EntityFrameworkCore;

namespace AdotPet.Infrastructure.Persistence.Repositories;

public sealed class PetRepository(AppDbContext db) : IPetRepository
{
    private const string CityCollation = "Latin1_General_CI_AI";

    public Task<Pet?> GetByIdAsync(Guid id, CancellationToken ct) =>
        db.Pets.Include(p => p.Shelter).FirstOrDefaultAsync(p => p.Id == id, ct);

    public async Task<PagedResult<Pet>> SearchAvailableAsync(PetSearchCriteria criteria, CancellationToken ct)
    {
        var query = db.Pets.AsNoTracking().Include(p => p.Shelter)
            .Where(p => p.Status == PetStatus.Available);

        if (criteria.Species is { } species) query = query.Where(p => p.Species == species);
        if (criteria.Sex is { } sex) query = query.Where(p => p.Sex == sex);
        if (criteria.Size is { } size) query = query.Where(p => p.Size == size);
        if (criteria.AgeGroup is { } ageGroup) query = query.Where(p => p.AgeGroup == ageGroup);
        if (!string.IsNullOrWhiteSpace(criteria.State))
        {
            var state = criteria.State.Trim();
            query = query.Where(p => p.Shelter!.State == state);
        }
        if (!string.IsNullOrWhiteSpace(criteria.City))
        {
            var city = criteria.City.Trim();
            // Accent- and case-insensitive prefix match ("sao" finds "São Paulo"); the default collation is CI_AS.
            query = query.Where(p => EF.Functions.Collate(p.Shelter!.City, CityCollation).StartsWith(city));
        }

        var total = await query.CountAsync(ct);
        var items = await query
            .OrderByDescending(p => p.CreatedAt).ThenBy(p => p.Id)
            .Skip((criteria.Page - 1) * criteria.PageSize)
            .Take(criteria.PageSize)
            .ToListAsync(ct);

        return new PagedResult<Pet>(items, criteria.Page, criteria.PageSize, total);
    }

    public async Task<IReadOnlyList<Pet>> ListByShelterAsync(Guid shelterId, CancellationToken ct) =>
        await db.Pets.Include(p => p.Shelter)
            .Where(p => p.ShelterId == shelterId)
            .OrderByDescending(p => p.CreatedAt).ThenBy(p => p.Id)
            .ToListAsync(ct);

    public void Add(Pet pet) => db.Pets.Add(pet);
}
