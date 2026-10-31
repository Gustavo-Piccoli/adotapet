using AdotPet.Application.AdoptionRequests.Dtos;
using AdotPet.Domain.AdoptionRequests;

namespace AdotPet.Application.AdoptionRequests.UseCases;

public interface IListShelterRequestsUseCase
{
    Task<IReadOnlyList<ShelterRequestDto>> ExecuteAsync(Guid shelterId, AdoptionRequestStatus? status, CancellationToken ct);
}

public sealed class ListShelterRequestsUseCase(IAdoptionRequestRepository requests) : IListShelterRequestsUseCase
{
    public async Task<IReadOnlyList<ShelterRequestDto>> ExecuteAsync(Guid shelterId,
        AdoptionRequestStatus? status, CancellationToken ct)
    {
        var items = await requests.ListByShelterAsync(shelterId, status, ct);
        return items.Select(r =>
        {
            var pet = r.Pet ?? throw new InvalidOperationException("Request loaded without its pet.");
            var a = r.Adopter ?? throw new InvalidOperationException("Request loaded without its adopter.");
            return new ShelterRequestDto(r.Id, pet.Id, pet.Name, a.Name, a.Email, a.Phone, a.City, a.State,
                r.Message, r.Status, r.CreatedAt, r.DecidedAt);
        }).ToList();
    }
}
