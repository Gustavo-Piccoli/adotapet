using AdotaPet.Application.AdoptionRequests.UseCases;
using AdotaPet.Application.Tests.Fakes;
using AdotaPet.Domain.AdoptionRequests;
using AdotaPet.Domain.Common;

namespace AdotaPet.Application.Tests.AdoptionRequests;

public class CreateAdoptionRequestUseCaseTests
{
    private readonly InMemoryPetRepository _pets = new();
    private readonly InMemoryAdoptionRequestRepository _requests = new();
    private readonly FakeUnitOfWork _uow = new();

    private CreateAdoptionRequestUseCase Sut() => new(_pets, _requests, _uow, new FakeClock());

    [Fact]
    public async Task Create_Valid_ReturnsPendingRequest()
    {
        var pet = _pets.Seed(InMemoryShelterRepository.NewShelter());
        var adopterId = Guid.NewGuid();

        var id = await Sut().ExecuteAsync(adopterId, pet.Id, "Quero muito adotar.", CancellationToken.None);

        var saved = Assert.Single(_requests.Items);
        Assert.Equal(id, saved.Id);
        Assert.Equal(AdoptionRequestStatus.Pending, saved.Status);
        Assert.Equal(adopterId, saved.AdopterId);
        Assert.Equal(1, _uow.SaveChangesCount);
    }

    [Fact]
    public async Task Create_WithPendingDuplicate_ThrowsConflict()
    {
        var pet = _pets.Seed(InMemoryShelterRepository.NewShelter());
        var adopter = InMemoryAdoptionRequestRepository.NewAdopter();
        _requests.Seed(pet, adopter);

        var ex = await Assert.ThrowsAsync<ConflictException>(() =>
            Sut().ExecuteAsync(adopter.Id, pet.Id, "Quero muito adotar.", CancellationToken.None));
        Assert.Equal("Você já tem um pedido em análise para este pet.", ex.Message);
        Assert.Equal(ConflictCodes.DuplicatePendingRequest, ex.Code);
    }

    [Fact]
    public async Task Create_AfterRejection_IsAllowed()
    {
        var pet = _pets.Seed(InMemoryShelterRepository.NewShelter());
        var adopter = InMemoryAdoptionRequestRepository.NewAdopter();
        _requests.Seed(pet, adopter).Reject(DateTimeOffset.UtcNow);

        await Sut().ExecuteAsync(adopter.Id, pet.Id, "Quero muito adotar.", CancellationToken.None);

        Assert.Equal(2, _requests.Items.Count);
    }

    [Fact]
    public async Task Create_ForAdoptedPet_ThrowsConflict()
    {
        var pet = _pets.Seed(InMemoryShelterRepository.NewShelter(), adopted: true);

        await Assert.ThrowsAsync<ConflictException>(() =>
            Sut().ExecuteAsync(Guid.NewGuid(), pet.Id, "Quero muito adotar.", CancellationToken.None));
    }

    [Fact]
    public async Task Create_ForMissingPet_ThrowsNotFound()
    {
        await Assert.ThrowsAsync<NotFoundException>(() =>
            Sut().ExecuteAsync(Guid.NewGuid(), Guid.NewGuid(), "Quero muito adotar.", CancellationToken.None));
    }
}
