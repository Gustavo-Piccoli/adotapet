using AdotPet.Application.AdoptionRequests.UseCases;
using AdotPet.Application.Tests.Fakes;
using AdotPet.Domain.AdoptionRequests;
using AdotPet.Domain.Common;
using AdotPet.Domain.Pets;

namespace AdotPet.Application.Tests.AdoptionRequests;

public class ApproveAdoptionRequestUseCaseTests
{
    private readonly InMemoryPetRepository _pets = new();
    private readonly InMemoryAdoptionRequestRepository _requests = new();
    private readonly FakeUnitOfWork _uow = new();

    private ApproveAdoptionRequestUseCase Sut() => new(_requests, _uow, new FakeClock());

    [Fact]
    public async Task Approve_MarksPetAdopted_AndRejectsOtherPending()
    {
        var shelter = InMemoryShelterRepository.NewShelter();
        var pet = _pets.Seed(shelter);
        var first = _requests.Seed(pet, InMemoryAdoptionRequestRepository.NewAdopter("Ana"));
        var second = _requests.Seed(pet, InMemoryAdoptionRequestRepository.NewAdopter("Bia"));
        var third = _requests.Seed(pet, InMemoryAdoptionRequestRepository.NewAdopter("Cris"));

        await Sut().ExecuteAsync(shelter.Id, second.Id, CancellationToken.None);

        Assert.Equal(AdoptionRequestStatus.Approved, second.Status);
        Assert.Equal(AdoptionRequestStatus.Rejected, first.Status);
        Assert.Equal(AdoptionRequestStatus.Rejected, third.Status);
        Assert.Equal(PetStatus.Adopted, pet.Status);
        Assert.Equal(1, _uow.TransactionCount);
        Assert.Equal(1, _uow.SaveChangesCount);
    }

    [Fact]
    public async Task Approve_RequestOfOtherShelter_ThrowsNotFound()
    {
        var pet = _pets.Seed(InMemoryShelterRepository.NewShelter());
        var request = _requests.Seed(pet, InMemoryAdoptionRequestRepository.NewAdopter());

        await Assert.ThrowsAsync<NotFoundException>(() =>
            Sut().ExecuteAsync(Guid.NewGuid(), request.Id, CancellationToken.None));
        Assert.Equal(PetStatus.Available, pet.Status);
    }

    [Fact]
    public async Task Approve_AlreadyRejected_ThrowsConflict()
    {
        var shelter = InMemoryShelterRepository.NewShelter();
        var pet = _pets.Seed(shelter);
        var request = _requests.Seed(pet, InMemoryAdoptionRequestRepository.NewAdopter());
        request.Reject(DateTimeOffset.UtcNow);

        await Assert.ThrowsAsync<ConflictException>(() =>
            Sut().ExecuteAsync(shelter.Id, request.Id, CancellationToken.None));
        Assert.Equal(PetStatus.Available, pet.Status);
    }
}
