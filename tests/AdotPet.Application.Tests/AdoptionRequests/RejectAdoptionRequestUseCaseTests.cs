using AdotPet.Application.AdoptionRequests.UseCases;
using AdotPet.Application.Tests.Fakes;
using AdotPet.Domain.AdoptionRequests;
using AdotPet.Domain.Pets;

namespace AdotPet.Application.Tests.AdoptionRequests;

public class RejectAdoptionRequestUseCaseTests
{
    [Fact]
    public async Task Reject_Pending_SetsRejected_PetStaysAvailable()
    {
        var pets = new InMemoryPetRepository();
        var requests = new InMemoryAdoptionRequestRepository();
        var uow = new FakeUnitOfWork();
        var shelter = InMemoryShelterRepository.NewShelter();
        var pet = pets.Seed(shelter);
        var request = requests.Seed(pet, InMemoryAdoptionRequestRepository.NewAdopter());

        await new RejectAdoptionRequestUseCase(requests, uow, new FakeClock())
            .ExecuteAsync(shelter.Id, request.Id, CancellationToken.None);

        Assert.Equal(AdoptionRequestStatus.Rejected, request.Status);
        Assert.NotNull(request.DecidedAt);
        Assert.Equal(PetStatus.Available, pet.Status);
        Assert.Equal(1, uow.SaveChangesCount);
    }
}
