using AdotaPet.Application.Pets.Dtos;
using AdotaPet.Application.Pets.UseCases;
using AdotaPet.Application.Tests.Fakes;
using AdotaPet.Domain.Pets;

namespace AdotaPet.Application.Tests.Pets;

public class CreatePetUseCaseTests
{
    [Fact]
    public async Task Create_CreatesAvailablePetForShelter()
    {
        var pets = new InMemoryPetRepository();
        var uow = new FakeUnitOfWork();
        var shelterId = Guid.NewGuid();
        var sut = new CreatePetUseCase(pets, uow, new FakeClock());

        var id = await sut.ExecuteAsync(shelterId,
            new PetInputDto("Rex", Species.Dog, Sex.Male, Size.Medium, AgeGroup.Adult, true, true, "Dócil."),
            CancellationToken.None);

        var pet = Assert.Single(pets.Items);
        Assert.Equal(id, pet.Id);
        Assert.Equal(shelterId, pet.ShelterId);
        Assert.Equal(PetStatus.Available, pet.Status);
        Assert.Equal(1, uow.SaveChangesCount);
    }
}
