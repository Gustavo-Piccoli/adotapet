namespace AdotPet.Domain.AdoptionRequests;

public interface IAdoptionRequestRepository
{
    Task<AdoptionRequest?> GetByIdAsync(Guid id, CancellationToken ct);
    Task<bool> HasPendingAsync(Guid adopterId, Guid petId, CancellationToken ct);
    Task<IReadOnlyList<AdoptionRequest>> ListByAdopterAsync(Guid adopterId, CancellationToken ct);
    Task<IReadOnlyList<AdoptionRequest>> ListByShelterAsync(Guid shelterId, AdoptionRequestStatus? status, CancellationToken ct);
    Task<IReadOnlyList<AdoptionRequest>> ListPendingByPetAsync(Guid petId, CancellationToken ct);
    void Add(AdoptionRequest request);
}
