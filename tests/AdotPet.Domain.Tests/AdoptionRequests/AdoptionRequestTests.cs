using AdotPet.Domain.AdoptionRequests;
using AdotPet.Domain.Common;
using AdotPet.Domain.Pets;

namespace AdotPet.Domain.Tests.AdoptionRequests;

public class AdoptionRequestTests
{
    private static readonly DateTimeOffset Now = new(2026, 10, 30, 12, 0, 0, TimeSpan.Zero);
    private const string Message = "Tenho quintal e muito carinho para dar.";

    private static Pet NewPet() =>
        Pet.Create(Guid.NewGuid(), "Rex", Species.Dog, Sex.Male, Size.Medium, AgeGroup.Adult,
            true, true, "Cão muito dócil", Now);

    private static AdoptionRequest NewRequest() => AdoptionRequest.Create(NewPet(), Guid.NewGuid(), Message, Now);

    [Fact]
    public void Create_ForAvailablePet_IsPending()
    {
        var pet = NewPet();
        var adopterId = Guid.NewGuid();

        var request = AdoptionRequest.Create(pet, adopterId, $"  {Message}  ", Now);

        Assert.Equal(AdoptionRequestStatus.Pending, request.Status);
        Assert.Equal(pet.Id, request.PetId);
        Assert.Equal(adopterId, request.AdopterId);
        Assert.Equal(Message, request.Message);
        Assert.Equal(Now, request.CreatedAt);
        Assert.Null(request.DecidedAt);
    }

    [Fact]
    public void Create_ForAdoptedPet_ThrowsConflict()
    {
        var pet = NewPet();
        pet.MarkAdopted(Now);

        var ex = Assert.Throws<ConflictException>(() => AdoptionRequest.Create(pet, Guid.NewGuid(), Message, Now));
        Assert.Equal("Este pet não está mais disponível para adoção.", ex.Message);
        Assert.Equal(ConflictCodes.PetUnavailable, ex.Code);
    }

    [Fact]
    public void Create_WithShortMessage_Throws() =>
        Assert.Throws<DomainValidationException>(() => AdoptionRequest.Create(NewPet(), Guid.NewGuid(), "oi", Now));

    [Fact]
    public void Approve_Pending_SetsApprovedAndDecidedAt()
    {
        var request = NewRequest();
        var later = Now.AddHours(2);

        request.Approve(later);

        Assert.Equal(AdoptionRequestStatus.Approved, request.Status);
        Assert.Equal(later, request.DecidedAt);
    }

    [Fact]
    public void Reject_Pending_SetsRejected()
    {
        var request = NewRequest();

        request.Reject(Now.AddHours(1));

        Assert.Equal(AdoptionRequestStatus.Rejected, request.Status);
        Assert.NotNull(request.DecidedAt);
    }

    [Fact]
    public void Approve_WhenRejected_ThrowsConflict()
    {
        var request = NewRequest();
        request.Reject(Now);

        var ex = Assert.Throws<ConflictException>(() => request.Approve(Now));
        Assert.Equal(ConflictCodes.RequestAlreadyDecided, ex.Code);
    }
}
