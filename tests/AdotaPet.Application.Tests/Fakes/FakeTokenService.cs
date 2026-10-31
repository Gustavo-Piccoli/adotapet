using AdotaPet.Application.Accounts;

namespace AdotaPet.Application.Tests.Fakes;

public sealed class FakeTokenService : ITokenService
{
    public static readonly DateTimeOffset Expiry = new(2026, 1, 15, 13, 0, 0, TimeSpan.Zero);

    public AuthToken CreateToken(string userId, string email, string role, Guid profileId, string name) =>
        new($"token:{userId}:{role}:{profileId}", Expiry);
}
