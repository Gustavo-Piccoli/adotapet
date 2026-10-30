using AdotaPet.Application.Pets.Dtos;
using AdotaPet.Domain.Pets;

namespace AdotaPet.Application.Pets.UseCases;

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
