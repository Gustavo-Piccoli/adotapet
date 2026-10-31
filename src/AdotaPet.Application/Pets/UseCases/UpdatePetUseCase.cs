using AdotaPet.Application.Common;
using AdotaPet.Application.Pets.Dtos;
using AdotaPet.Domain.Common;
using AdotaPet.Domain.Pets;

namespace AdotaPet.Application.Pets.UseCases;

public interface IUpdatePetUseCase
{
    Task ExecuteAsync(Guid shelterId, Guid petId, PetInputDto input, CancellationToken ct);
}

public sealed class UpdatePetUseCase(IPetRepository pets, IUnitOfWork unitOfWork, IClock clock) : IUpdatePetUseCase
{
    public async Task ExecuteAsync(Guid shelterId, Guid petId, PetInputDto input, CancellationToken ct)
    {
        var pet = await pets.GetByIdAsync(petId, ct);
        if (pet is null || pet.ShelterId != shelterId) throw PetMapping.PetNotFound();

        pet.Update(input.Name, input.Species, input.Sex, input.Size, input.AgeGroup,
            input.IsNeutered, input.IsVaccinated, input.Description, clock.UtcNow);
        await unitOfWork.SaveChangesAsync(ct);
    }
}
