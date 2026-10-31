using AdotPet.Application.Accounts;
using AdotPet.Domain.Common;

namespace AdotPet.Application.Tests.Fakes;

public sealed class FakeIdentityService : IIdentityService
{
    public sealed record FakeUser(string UserId, string Email, string Password, string Role);

    public List<FakeUser> Users { get; } = [];

    public Task<string> CreateUserAsync(string email, string password, string role, CancellationToken ct)
    {
        if (Users.Any(u => string.Equals(u.Email, email, StringComparison.OrdinalIgnoreCase)))
            throw new ConflictException("Já existe uma conta com este e-mail.", ConflictCodes.EmailTaken);

        var user = new FakeUser(Guid.NewGuid().ToString(), email, password, role);
        Users.Add(user);
        return Task.FromResult(user.UserId);
    }

    public Task<IdentityUserInfo?> ValidateCredentialsAsync(string email, string password, CancellationToken ct)
    {
        var user = Users.FirstOrDefault(u =>
            string.Equals(u.Email, email, StringComparison.OrdinalIgnoreCase) && u.Password == password);
        return Task.FromResult(user is null ? null : new IdentityUserInfo(user.UserId, user.Email, user.Role));
    }
}
