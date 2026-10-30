using AdotaPet.Application.Pets.UseCases;
using AdotaPet.Application.Tests.Fakes;
using AdotaPet.Domain.Common;

namespace AdotaPet.Application.Tests.Pets;

public class GetPetPhotoUseCaseTests
{
    [Fact]
    public async Task Get_ReturnsContentAndType()
    {
        var pets = new InMemoryPetRepository();
        var storage = new InMemoryPhotoStorage();
        var pet = pets.Seed(InMemoryShelterRepository.NewShelter(), photoPath: "a.webp");
        storage.Files["a.webp"] = [1, 2];

        var photo = await new GetPetPhotoUseCase(pets, storage).ExecuteAsync(pet.Id, CancellationToken.None);

        Assert.Equal("image/webp", photo.ContentType);
    }

    [Fact]
    public async Task Get_PetWithoutPhoto_ThrowsNotFound()
    {
        var pets = new InMemoryPetRepository();
        var pet = pets.Seed(InMemoryShelterRepository.NewShelter());
        await Assert.ThrowsAsync<NotFoundException>(() =>
            new GetPetPhotoUseCase(pets, new InMemoryPhotoStorage()).ExecuteAsync(pet.Id, CancellationToken.None));
    }
}
