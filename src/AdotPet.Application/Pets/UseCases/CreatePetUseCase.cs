using AdotPet.Application.Common;
using AdotPet.Application.Pets.Dtos;
using AdotPet.Domain.Common;
using AdotPet.Domain.Pets;

namespace AdotPet.Application.Pets.UseCases;

public interface ICreatePetUseCase
{
    Task<Guid> ExecuteAsync(Guid shelterId, PetInputDto input, CancellationToken ct);
}

public sealed class CreatePetUseCase(IPetRepository pets, IUnitOfWork unitOfWork, IClock clock) : ICreatePetUseCase
{
    public async Task<Guid> ExecuteAsync(Guid shelterId, PetInputDto input, CancellationToken ct)
    {
        var pet = Pet.Create(shelterId, input.Name, input.Species, input.Sex, input.Size, input.AgeGroup,
            input.IsNeutered, input.IsVaccinated, input.Description, clock.UtcNow);
        pets.Add(pet);
        await unitOfWork.SaveChangesAsync(ct);
        return pet.Id;
    }
}
