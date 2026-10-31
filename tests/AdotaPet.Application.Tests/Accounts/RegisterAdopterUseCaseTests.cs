using AdotaPet.Application.Accounts;
using AdotaPet.Application.Accounts.Dtos;
using AdotaPet.Application.Accounts.UseCases;
using AdotaPet.Application.Tests.Fakes;

namespace AdotaPet.Application.Tests.Accounts;

public class RegisterAdopterUseCaseTests
{
    private readonly FakeIdentityService _identity = new();
    private readonly InMemoryAdopterRepository _adopters = new();
    private readonly FakeUnitOfWork _uow = new();

    [Fact]
    public async Task Register_Valid_CreatesAdopterRoleUser_AndProfileIdIsNewAdopterId()
    {
        var sut = new RegisterAdopterUseCase(_identity, new FakeTokenService(), _adopters, _uow, new FakeClock());

        var result = await sut.ExecuteAsync(
            new RegisterAdopterDto("maria@example.com", "Senha@123", "Maria Silva", "(51) 99999-1111",
                "Porto Alegre", "RS"),
            CancellationToken.None);

        var user = Assert.Single(_identity.Users);
        Assert.Equal(Roles.Adopter, user.Role);
        var adopter = Assert.Single(_adopters.Items);
        Assert.Equal(user.UserId, adopter.UserId);
        Assert.Equal(Roles.Adopter, result.Role);
        Assert.Equal(adopter.Id, result.ProfileId);
        Assert.Equal("maria@example.com", result.Email);
        Assert.Equal(1, _uow.TransactionCount);
        Assert.Equal(1, _uow.SaveChangesCount);
    }

    [Fact]
    public async Task Register_InvalidInput_CreatesNoUser()
    {
        var sut = new RegisterAdopterUseCase(_identity, new FakeTokenService(), _adopters, _uow, new FakeClock());

        await Assert.ThrowsAnyAsync<Exception>(() => sut.ExecuteAsync(
            new RegisterAdopterDto("maria@example.com", "Senha@123", "Maria Silva", "(51) 99999-1111",
                "Porto Alegre", "XX"),
            CancellationToken.None));

        Assert.Empty(_identity.Users);
    }

    [Fact]
    public async Task Register_WhenTransactionIsRetried_StillSucceedsWithASingleAdopter()
    {
        _uow.SimulatedRetries = 1;
        _uow.RollbackBeforeRetry = () =>
        {
            _identity.Users.Clear();
            _adopters.Items.Clear();
        };
        var sut = new RegisterAdopterUseCase(_identity, new FakeTokenService(), _adopters, _uow, new FakeClock());

        var result = await sut.ExecuteAsync(
            new RegisterAdopterDto("maria@example.com", "Senha@123", "Maria Silva", "(51) 99999-1111",
                "Porto Alegre", "RS"),
            CancellationToken.None);

        var adopter = Assert.Single(_adopters.Items);
        Assert.Equal(Assert.Single(_identity.Users).UserId, adopter.UserId);
        Assert.Equal(adopter.Id, result.ProfileId);
    }
}
