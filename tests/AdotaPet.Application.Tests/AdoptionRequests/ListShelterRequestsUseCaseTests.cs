using AdotaPet.Application.AdoptionRequests.UseCases;
using AdotaPet.Application.Tests.Fakes;
using AdotaPet.Domain.AdoptionRequests;

namespace AdotaPet.Application.Tests.AdoptionRequests;

public class ListShelterRequestsUseCaseTests
{
    [Fact]
    public async Task List_FiltersByStatus_AndOnlyOwnShelter()
    {
        var pets = new InMemoryPetRepository();
        var requests = new InMemoryAdoptionRequestRepository();
        var shelter = InMemoryShelterRepository.NewShelter();
        var pet = pets.Seed(shelter);
        var otherPet = pets.Seed(InMemoryShelterRepository.NewShelter());
        var pending = requests.Seed(pet, InMemoryAdoptionRequestRepository.NewAdopter("Ana"));
        var rejected = requests.Seed(pet, InMemoryAdoptionRequestRepository.NewAdopter("Bia"));
        rejected.Reject(DateTimeOffset.UtcNow);
        requests.Seed(otherPet, InMemoryAdoptionRequestRepository.NewAdopter("Cris"));
        var sut = new ListShelterRequestsUseCase(requests);

        var all = await sut.ExecuteAsync(shelter.Id, null, CancellationToken.None);
        var onlyPending = await sut.ExecuteAsync(shelter.Id, AdoptionRequestStatus.Pending, CancellationToken.None);

        Assert.Equal(2, all.Count);
        var dto = Assert.Single(onlyPending);
        Assert.Equal(pending.Id, dto.Id);
        Assert.Equal("Ana", dto.AdopterName);
        Assert.Equal("maria@example.com", dto.AdopterEmail);
        Assert.Equal(pet.Name, dto.PetName);
    }
}
