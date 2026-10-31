using AdotPet.Application.Pets.Dtos;
using AdotPet.Domain.Pets;

namespace AdotPet.Application.Pets.UseCases;

public interface IGetShelterPetUseCase
{
    Task<ShelterPetDto> ExecuteAsync(Guid shelterId, Guid petId, CancellationToken ct);
}

public sealed class GetShelterPetUseCase(IPetRepository pets) : IGetShelterPetUseCase
{
    public async Task<ShelterPetDto> ExecuteAsync(Guid shelterId, Guid petId, CancellationToken ct)
    {
        var pet = await pets.GetByIdAsync(petId, ct);
        if (pet is null || pet.ShelterId != shelterId) throw PetMapping.PetNotFound();
        return PetMapping.ToShelterPet(pet);
    }
}
