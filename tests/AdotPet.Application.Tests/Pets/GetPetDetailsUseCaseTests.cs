using AdotPet.Application.Pets.UseCases;
using AdotPet.Application.Tests.Fakes;
using AdotPet.Domain.Common;
using AdotPet.Domain.Pets;

namespace AdotPet.Application.Tests.Pets;

public class GetPetDetailsUseCaseTests
{
    [Fact]
    public async Task Get_Missing_ThrowsNotFound()
    {
        var sut = new GetPetDetailsUseCase(new InMemoryPetRepository());
        await Assert.ThrowsAsync<NotFoundException>(() => sut.ExecuteAsync(Guid.NewGuid(), CancellationToken.None));
    }

    [Fact]
    public async Task Get_AdoptedPet_IsStillReturned()
    {
        var pets = new InMemoryPetRepository();
        var pet = pets.Seed(InMemoryShelterRepository.NewShelter(), adopted: true);

        var dto = await new GetPetDetailsUseCase(pets).ExecuteAsync(pet.Id, CancellationToken.None);

        Assert.Equal(PetStatus.Adopted, dto.Status);
        Assert.Equal("ONG Patinhas", dto.Shelter.Name);
    }
}
