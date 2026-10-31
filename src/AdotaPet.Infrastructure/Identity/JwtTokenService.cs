using System.Text;
using AdotaPet.Application.Accounts;
using AdotaPet.Application.Common;
using Microsoft.Extensions.Options;
using Microsoft.IdentityModel.JsonWebTokens;
using Microsoft.IdentityModel.Tokens;

namespace AdotaPet.Infrastructure.Identity;

public sealed class JwtTokenService : ITokenService
{
    private const int MinimumSigningKeyLength = 32;

    private readonly JwtOptions _options;
    private readonly IClock _clock;
    private readonly SigningCredentials _credentials;

    public JwtTokenService(IOptions<JwtOptions> options, IClock clock)
    {
        _options = options.Value;
        _clock = clock;
        if (string.IsNullOrEmpty(_options.SigningKey) || _options.SigningKey.Length < MinimumSigningKeyLength)
            throw new InvalidOperationException(
                $"Jwt:SigningKey must have at least {MinimumSigningKeyLength} characters.");
        _credentials = new SigningCredentials(
            new SymmetricSecurityKey(Encoding.UTF8.GetBytes(_options.SigningKey)),
            SecurityAlgorithms.HmacSha256);
    }

    public AuthToken CreateToken(string userId, string email, string role, Guid profileId, string name)
    {
        var now = _clock.UtcNow;
        var expiresAt = now.AddMinutes(_options.ExpirationMinutes);

        var descriptor = new SecurityTokenDescriptor
        {
            Issuer = _options.Issuer,
            Audience = _options.Audience,
            IssuedAt = now.UtcDateTime,
            NotBefore = now.UtcDateTime,
            Expires = expiresAt.UtcDateTime,
            SigningCredentials = _credentials,
            Claims = new Dictionary<string, object>
            {
                ["sub"] = userId,
                ["email"] = email,
                ["role"] = role,
                ["profile_id"] = profileId.ToString(),
                ["name"] = name,
            },
        };

        return new AuthToken(new JsonWebTokenHandler().CreateToken(descriptor), expiresAt);
    }
}
