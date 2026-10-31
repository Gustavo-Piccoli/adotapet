using AdotaPet.Application.Accounts;
using AdotaPet.Application.Accounts.Dtos;
using AdotaPet.Application.Accounts.UseCases;
using AdotaPet.Application.Tests.Fakes;

namespace AdotaPet.Application.Tests.Accounts;

public class LoginUseCaseTests
{
    private readonly FakeIdentityService _identity = new();
    private readonly InMemoryShelterRepository _shelters = new();
    private readonly InMemoryAdopterRepository _adopters = new();

    private LoginUseCase Sut() => new(_identity, new FakeTokenService(), _shelters, _adopters);

    private async Task<Domain.Shelters.Shelter> SeedShelterAsync()
    {
        var userId = await _identity.CreateUserAsync("ong@example.com", "Senha@123", Roles.Shelter, CancellationToken.None);
        var shelter = InMemoryShelterRepository.NewShelter();
        shelter.AssignUser(userId);
        _shelters.Add(shelter);
        return shelter;
    }

    [Fact]
    public async Task Login_ValidShelter_ReturnsTokenWithShelterRole()
    {
        var shelter = await SeedShelterAsync();

        var result = await Sut().ExecuteAsync(new LoginDto("ong@example.com", "Senha@123"), CancellationToken.None);

        Assert.Equal(Roles.Shelter, result.Role);
        Assert.Equal(shelter.Id, result.ProfileId);
        Assert.Equal(shelter.Name, result.Name);
        Assert.Equal(shelter.Email, result.Email);
        Assert.Equal($"token:{shelter.UserId}:Shelter:{shelter.Id}", result.AccessToken);
    }

    [Fact]
    public async Task Login_ValidAdopter_ReturnsTokenWithAdopterRole()
    {
        var userId = await _identity.CreateUserAsync("maria@example.com", "Senha@123", Roles.Adopter, CancellationToken.None);
        var adopter = InMemoryAdoptionRequestRepository.NewAdopter();
        adopter.AssignUser(userId);
        _adopters.Add(adopter);

        var result = await Sut().ExecuteAsync(new LoginDto("maria@example.com", "Senha@123"), CancellationToken.None);

        Assert.Equal(Roles.Adopter, result.Role);
        Assert.Equal(adopter.Id, result.ProfileId);
    }

    [Fact]
    public async Task Login_WrongPassword_ThrowsInvalidCredentials()
    {
        await SeedShelterAsync();

        var ex = await Assert.ThrowsAsync<InvalidCredentialsException>(() =>
            Sut().ExecuteAsync(new LoginDto("ong@example.com", "errada"), CancellationToken.None));
        Assert.Equal("E-mail ou senha inválidos.", ex.Message);
    }
}
