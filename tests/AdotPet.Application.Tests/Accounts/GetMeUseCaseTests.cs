using AdotPet.Application.Accounts;
using AdotPet.Application.Accounts.UseCases;
using AdotPet.Application.Tests.Fakes;
using AdotPet.Domain.Common;

namespace AdotPet.Application.Tests.Accounts;

public class GetMeUseCaseTests
{
    private readonly InMemoryShelterRepository _shelters = new();
    private readonly InMemoryAdopterRepository _adopters = new();

    private GetMeUseCase Sut() => new(_shelters, _adopters);

    [Fact]
    public async Task GetMe_Shelter_ReturnsProfile()
    {
        var shelter = InMemoryShelterRepository.NewShelter();
        shelter.AssignUser("user-1");
        _shelters.Add(shelter);

        var me = await Sut().ExecuteAsync("user-1", Roles.Shelter, CancellationToken.None);

        Assert.Equal(new(Roles.Shelter, shelter.Id, shelter.Name, shelter.Email), me);
    }

    [Fact]
    public async Task GetMe_Adopter_ReturnsProfile()
    {
        var adopter = InMemoryAdoptionRequestRepository.NewAdopter();
        adopter.AssignUser("user-2");
        _adopters.Add(adopter);

        var me = await Sut().ExecuteAsync("user-2", Roles.Adopter, CancellationToken.None);

        Assert.Equal(new(Roles.Adopter, adopter.Id, adopter.Name, adopter.Email), me);
    }

    [Fact]
    public async Task GetMe_MissingProfile_ThrowsNotFound()
    {
        await Assert.ThrowsAsync<NotFoundException>(() =>
            Sut().ExecuteAsync("nobody", Roles.Adopter, CancellationToken.None));
    }
}
