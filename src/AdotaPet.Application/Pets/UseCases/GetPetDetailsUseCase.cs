using AdotaPet.Application.Pets.Dtos;
using AdotaPet.Domain.Pets;

namespace AdotaPet.Application.Pets.UseCases;

public interface IGetPetDetailsUseCase
{
    Task<PetDetailsDto> ExecuteAsync(Guid petId, CancellationToken ct);
}

public sealed class GetPetDetailsUseCase(IPetRepository pets) : IGetPetDetailsUseCase
{
    public async Task<PetDetailsDto> ExecuteAsync(Guid petId, CancellationToken ct)
    {
        var pet = await pets.GetByIdAsync(petId, ct) ?? throw PetMapping.PetNotFound();
        return PetMapping.ToDetails(pet);
    }
}
