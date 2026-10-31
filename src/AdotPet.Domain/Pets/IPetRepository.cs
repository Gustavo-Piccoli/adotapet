using AdotPet.Domain.Common;

namespace AdotPet.Domain.Pets;

public interface IPetRepository
{
    Task<Pet?> GetByIdAsync(Guid id, CancellationToken ct);
    Task<PagedResult<Pet>> SearchAvailableAsync(PetSearchCriteria criteria, CancellationToken ct);
    Task<IReadOnlyList<Pet>> ListByShelterAsync(Guid shelterId, CancellationToken ct);
    void Add(Pet pet);
}
