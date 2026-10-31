using AdotPet.Domain.AdoptionRequests;
using Microsoft.EntityFrameworkCore;

namespace AdotPet.Infrastructure.Persistence.Repositories;

public sealed class AdoptionRequestRepository(AppDbContext db) : IAdoptionRequestRepository
{
    private IQueryable<AdoptionRequest> WithRelations() =>
        db.AdoptionRequests
            .Include(r => r.Pet).ThenInclude(p => p!.Shelter)
            .Include(r => r.Adopter);

    public Task<AdoptionRequest?> GetByIdAsync(Guid id, CancellationToken ct) =>
        WithRelations().FirstOrDefaultAsync(r => r.Id == id, ct);

    public Task<bool> HasPendingAsync(Guid adopterId, Guid petId, CancellationToken ct) =>
        db.AdoptionRequests.AnyAsync(r =>
            r.AdopterId == adopterId && r.PetId == petId && r.Status == AdoptionRequestStatus.Pending, ct);

    public async Task<IReadOnlyList<AdoptionRequest>> ListByAdopterAsync(Guid adopterId, CancellationToken ct) =>
        await WithRelations()
            .Where(r => r.AdopterId == adopterId)
            .OrderByDescending(r => r.CreatedAt).ThenBy(r => r.Id)
            .ToListAsync(ct);

    public async Task<IReadOnlyList<AdoptionRequest>> ListByShelterAsync(
        Guid shelterId, AdoptionRequestStatus? status, CancellationToken ct)
    {
        var query = WithRelations().Where(r => r.Pet!.ShelterId == shelterId);
        if (status is { } s) query = query.Where(r => r.Status == s);
        return await query.OrderByDescending(r => r.CreatedAt).ThenBy(r => r.Id).ToListAsync(ct);
    }

    public async Task<IReadOnlyList<AdoptionRequest>> ListPendingByPetAsync(Guid petId, CancellationToken ct) =>
        await WithRelations()
            .Where(r => r.PetId == petId && r.Status == AdoptionRequestStatus.Pending)
            .OrderBy(r => r.CreatedAt).ThenBy(r => r.Id)
            .ToListAsync(ct);

    public void Add(AdoptionRequest request) => db.AdoptionRequests.Add(request);
}
