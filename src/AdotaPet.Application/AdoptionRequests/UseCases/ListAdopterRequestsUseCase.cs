using AdotaPet.Application.AdoptionRequests.Dtos;
using AdotaPet.Application.Pets;
using AdotaPet.Domain.AdoptionRequests;

namespace AdotaPet.Application.AdoptionRequests.UseCases;

public interface IListAdopterRequestsUseCase
{
    Task<IReadOnlyList<AdopterRequestDto>> ExecuteAsync(Guid adopterId, CancellationToken ct);
}

public sealed class ListAdopterRequestsUseCase(IAdoptionRequestRepository requests) : IListAdopterRequestsUseCase
{
    public async Task<IReadOnlyList<AdopterRequestDto>> ExecuteAsync(Guid adopterId, CancellationToken ct)
    {
        var items = await requests.ListByAdopterAsync(adopterId, ct);
        return items.Select(r =>
        {
            var pet = r.Pet ?? throw new InvalidOperationException("Request loaded without its pet.");
            return new AdopterRequestDto(r.Id, pet.Id, pet.Name, PetMapping.PhotoUrl(pet),
                PetMapping.ShelterOf(pet).Name, r.Message, r.Status, r.CreatedAt, r.DecidedAt);
        }).ToList();
    }
}
