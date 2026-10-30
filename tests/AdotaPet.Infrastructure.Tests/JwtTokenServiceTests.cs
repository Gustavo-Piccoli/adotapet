using AdotaPet.Application.Common;
using AdotaPet.Infrastructure.Identity;
using Microsoft.Extensions.Options;
using Microsoft.IdentityModel.JsonWebTokens;

namespace AdotaPet.Infrastructure.Tests;

public class JwtTokenServiceTests
{
    private sealed class FixedClock(DateTimeOffset now) : IClock
    {
        public DateTimeOffset UtcNow { get; } = now;
    }

    private static readonly DateTimeOffset Now = new(2026, 1, 15, 12, 0, 0, TimeSpan.Zero);

    private static JwtTokenService CreateService() =>
        new(Options.Create(new JwtOptions { SigningKey = new string('k', 40) }), new FixedClock(Now));

    [Fact]
    public void CreateToken_ContainsExpectedClaims()
    {
        var profileId = Guid.NewGuid();

        var token = CreateService().CreateToken("user-1", "ana@example.com", "Shelter", profileId, "Ana");

        var jwt = new JsonWebTokenHandler().ReadJsonWebToken(token.AccessToken);
        Assert.Equal("user-1", jwt.GetClaim("sub").Value);
        Assert.Equal("ana@example.com", jwt.GetClaim("email").Value);
        Assert.Equal("Shelter", jwt.GetClaim("role").Value);
        Assert.Equal(profileId.ToString(), jwt.GetClaim("profile_id").Value);
        Assert.Equal("Ana", jwt.GetClaim("name").Value);
        Assert.Equal("AdotaPet", jwt.Issuer);
        Assert.Contains("AdotaPet", jwt.Audiences);
    }

    [Fact]
    public void CreateToken_ExpiresSixtyMinutesAfterNow()
    {
        var token = CreateService().CreateToken("u", "a@b.com", "Adopter", Guid.NewGuid(), "Bia");

        Assert.Equal(Now.AddMinutes(60), token.ExpiresAt);
        var jwt = new JsonWebTokenHandler().ReadJsonWebToken(token.AccessToken);
        Assert.Equal(Now.AddMinutes(60).UtcDateTime, jwt.ValidTo);
    }

    [Fact]
    public async Task CreateToken_IsValidatableWithSigningKey()
    {
        var token = CreateService().CreateToken("u", "a@b.com", "Adopter", Guid.NewGuid(), "Bia");

        var result = await new JsonWebTokenHandler().ValidateTokenAsync(token.AccessToken,
            new Microsoft.IdentityModel.Tokens.TokenValidationParameters
            {
                ValidIssuer = "AdotaPet",
                ValidAudience = "AdotaPet",
                IssuerSigningKey = new Microsoft.IdentityModel.Tokens.SymmetricSecurityKey(
                    System.Text.Encoding.UTF8.GetBytes(new string('k', 40))),
                ValidateLifetime = false,
            });

        Assert.True(result.IsValid);
    }
}
