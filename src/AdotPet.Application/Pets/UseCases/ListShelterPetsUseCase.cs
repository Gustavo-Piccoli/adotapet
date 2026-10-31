using AdotPet.Application.Pets.Dtos;
using AdotPet.Domain.Pets;

namespace AdotPet.Application.Pets.UseCases;

public interface IListShelterPetsUseCase
{
    Task<IReadOnlyList<ShelterPetDto>> ExecuteAsync(Guid shelterId, CancellationToken ct);
}

public sealed class ListShelterPetsUseCase(IPetRepository pets) : IListShelterPetsUseCase
{
    public async Task<IReadOnlyList<ShelterPetDto>> ExecuteAsync(Guid shelterId, CancellationToken ct)
    {
        var list = await pets.ListByShelterAsync(shelterId, ct);
        return list.Select(PetMapping.ToShelterPet).ToList();
    }
}
