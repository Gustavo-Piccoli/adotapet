using AdotPet.Application.Accounts;
using AdotPet.Application.Accounts.Dtos;
using AdotPet.Application.Accounts.UseCases;
using AdotPet.Application.Tests.Fakes;
using AdotPet.Domain.Common;

namespace AdotPet.Application.Tests.Accounts;

public class RegisterShelterUseCaseTests
{
    private readonly FakeIdentityService _identity = new();
    private readonly InMemoryShelterRepository _shelters = new();
    private readonly FakeUnitOfWork _uow = new();

    private RegisterShelterUseCase Sut() =>
        new(_identity, new FakeTokenService(), _shelters, _uow, new FakeClock());

    private static RegisterShelterDto Dto(string cnpj = "11.222.333/0001-81") =>
        new("ong@example.com", "Senha@123", "ONG Patinhas", cnpj, "Porto Alegre", "RS",
            "(51) 99999-0000", "Resgate de animais.");

    [Fact]
    public async Task Register_Valid_CreatesUserAndShelter_InOneTransaction_ReturnsToken()
    {
        var result = await Sut().ExecuteAsync(Dto(), CancellationToken.None);

        var user = Assert.Single(_identity.Users);
        Assert.Equal(Roles.Shelter, user.Role);
        var shelter = Assert.Single(_shelters.Items);
        Assert.Equal(user.UserId, shelter.UserId);
        Assert.Equal("11222333000181", shelter.Cnpj);
        Assert.Equal(1, _uow.TransactionCount);
        Assert.Equal(1, _uow.SaveChangesCount);
        Assert.Equal(Roles.Shelter, result.Role);
        Assert.Equal(shelter.Id, result.ProfileId);
        Assert.Equal("ONG Patinhas", result.Name);
        Assert.Equal("ong@example.com", result.Email);
        Assert.Equal($"token:{user.UserId}:Shelter:{shelter.Id}", result.AccessToken);
        Assert.Equal(FakeTokenService.Expiry, result.ExpiresAt);
    }

    [Fact]
    public async Task Register_DuplicateCnpj_ThrowsConflict_AndCreatesNoUser()
    {
        _shelters.Items.Add(InMemoryShelterRepository.NewShelter());

        var ex = await Assert.ThrowsAsync<ConflictException>(() =>
            Sut().ExecuteAsync(Dto(), CancellationToken.None));

        Assert.Equal("Já existe uma ONG com este CNPJ.", ex.Message);
        Assert.Equal(ConflictCodes.CnpjTaken, ex.Code);
        Assert.Empty(_identity.Users);
        Assert.Single(_shelters.Items);
        Assert.Equal(0, _uow.SaveChangesCount);
    }

    [Fact]
    public async Task Register_InvalidCnpj_ThrowsValidation_AndCreatesNoUser()
    {
        await Assert.ThrowsAsync<DomainValidationException>(() =>
            Sut().ExecuteAsync(Dto("11.222.333/0001-82"), CancellationToken.None));

        Assert.Empty(_identity.Users);
        Assert.Empty(_shelters.Items);
        Assert.Equal(0, _uow.TransactionCount);
    }

    [Fact]
    public async Task Register_WhenTransactionIsRetried_StillSucceedsWithASingleShelter()
    {
        _uow.SimulatedRetries = 1;
        _uow.RollbackBeforeRetry = () =>
        {
            _identity.Users.Clear();
            _shelters.Items.Clear();
        };

        var result = await Sut().ExecuteAsync(Dto(), CancellationToken.None);

        var shelter = Assert.Single(_shelters.Items);
        Assert.Equal(Assert.Single(_identity.Users).UserId, shelter.UserId);
        Assert.Equal(shelter.Id, result.ProfileId);
    }
}
