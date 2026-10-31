using AdotPet.Application.Pets.Dtos;
using AdotPet.Application.Pets.UseCases;
using AdotPet.Application.Tests.Fakes;
using AdotPet.Domain.Common;
using AdotPet.Domain.Pets;

namespace AdotPet.Application.Tests.Pets;

public class UpdatePetUseCaseTests
{
    private static readonly PetInputDto Input =
        new("Novo Nome", Species.Cat, Sex.Female, Size.Small, AgeGroup.Puppy, false, true, "Nova descrição.");

    [Fact]
    public async Task Update_OwnPet_UpdatesFields()
    {
        var pets = new InMemoryPetRepository();
        var shelter = InMemoryShelterRepository.NewShelter();
        var pet = pets.Seed(shelter);
        var uow = new FakeUnitOfWork();

        await new UpdatePetUseCase(pets, uow, new FakeClock()).ExecuteAsync(shelter.Id, pet.Id, Input, CancellationToken.None);

        Assert.Equal("Novo Nome", pet.Name);
        Assert.Equal(Species.Cat, pet.Species);
        Assert.Equal(1, uow.SaveChangesCount);
    }

    [Fact]
    public async Task Update_PetOfOtherShelter_ThrowsNotFound()
    {
        var pets = new InMemoryPetRepository();
        var pet = pets.Seed(InMemoryShelterRepository.NewShelter());
        var sut = new UpdatePetUseCase(pets, new FakeUnitOfWork(), new FakeClock());

        var ex = await Assert.ThrowsAsync<NotFoundException>(() =>
            sut.ExecuteAsync(Guid.NewGuid(), pet.Id, Input, CancellationToken.None));
        Assert.Equal("Pet não encontrado.", ex.Message);
    }
}
