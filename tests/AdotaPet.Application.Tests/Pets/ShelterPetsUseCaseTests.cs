using AdotaPet.Application.Pets.UseCases;
using AdotaPet.Application.Tests.Fakes;
using AdotaPet.Domain.Common;

namespace AdotaPet.Application.Tests.Pets;

public class ShelterPetsUseCaseTests
{
    [Fact]
    public async Task List_ReturnsOnlyOwnPets_IncludingAdopted()
    {
        var pets = new InMemoryPetRepository();
        var mine = InMemoryShelterRepository.NewShelter();
        pets.Seed(mine, "Ana");
        pets.Seed(mine, "Bia", adopted: true);
        pets.Seed(InMemoryShelterRepository.NewShelter(), "Cid");

        var list = await new ListShelterPetsUseCase(pets).ExecuteAsync(mine.Id, CancellationToken.None);

        Assert.Equal(2, list.Count);
    }

    [Fact]
    public async Task Get_PetOfOtherShelter_ThrowsNotFound()
    {
        var pets = new InMemoryPetRepository();
        var pet = pets.Seed(InMemoryShelterRepository.NewShelter());
        await Assert.ThrowsAsync<NotFoundException>(() =>
            new GetShelterPetUseCase(pets).ExecuteAsync(Guid.NewGuid(), pet.Id, CancellationToken.None));
    }
}
