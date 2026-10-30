namespace AdotaPet.Application.Accounts;

public sealed record AuthToken(string AccessToken, DateTimeOffset ExpiresAt);

public interface ITokenService
{
    AuthToken CreateToken(string userId, string email, string role, Guid profileId, string name);
}
