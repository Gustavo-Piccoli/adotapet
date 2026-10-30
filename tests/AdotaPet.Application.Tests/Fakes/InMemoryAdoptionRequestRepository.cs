using System.Reflection;
using AdotaPet.Domain.Adopters;
using AdotaPet.Domain.AdoptionRequests;
using AdotaPet.Domain.Pets;

namespace AdotaPet.Application.Tests.Fakes;

public sealed class InMemoryAdoptionRequestRepository : IAdoptionRequestRepository
{
    public List<AdoptionRequest> Items { get; } = [];

    public Task<AdoptionRequest?> GetByIdAsync(Guid id, CancellationToken ct) =>
        Task.FromResult(Items.FirstOrDefault(r => r.Id == id));

    public Task<bool> HasPendingAsync(Guid adopterId, Guid petId, CancellationToken ct) =>
        Task.FromResult(Items.Any(r => r.AdopterId == adopterId && r.PetId == petId
            && r.Status == AdoptionRequestStatus.Pending));

    public Task<IReadOnlyList<AdoptionRequest>> ListByAdopterAsync(Guid adopterId, CancellationToken ct) =>
        Task.FromResult<IReadOnlyList<AdoptionRequest>>(Items.Where(r => r.AdopterId == adopterId).ToList());

    public Task<IReadOnlyList<AdoptionRequest>> ListByShelterAsync(Guid shelterId,
        AdoptionRequestStatus? status, CancellationToken ct) =>
        Task.FromResult<IReadOnlyList<AdoptionRequest>>(Items
            .Where(r => r.Pet!.ShelterId == shelterId && (status is null || r.Status == status)).ToList());

    public Task<IReadOnlyList<AdoptionRequest>> ListPendingByPetAsync(Guid petId, CancellationToken ct) =>
        Task.FromResult<IReadOnlyList<AdoptionRequest>>(Items
            .Where(r => r.PetId == petId && r.Status == AdoptionRequestStatus.Pending).ToList());

    public void Add(AdoptionRequest request) => Items.Add(request);

    public static Adopter NewAdopter(string name = "Maria Silva") =>
        Adopter.Create(name, "maria@example.com", "(51) 99999-1111", "Porto Alegre", "RS", DateTimeOffset.UtcNow);

    /// <summary>Creates a pending request with the Adopter navigation attached (as an EF Include would) and stores it.</summary>
    public AdoptionRequest Seed(Pet pet, Adopter adopter, string message = "Quero muito adotar este pet.")
    {
        var request = AdoptionRequest.Create(pet, adopter.Id, message, DateTimeOffset.UtcNow);
        typeof(AdoptionRequest).GetProperty(nameof(AdoptionRequest.Adopter), BindingFlags.Public | BindingFlags.Instance)!
            .SetValue(request, adopter);
        Items.Add(request);
        return request;
    }
}
