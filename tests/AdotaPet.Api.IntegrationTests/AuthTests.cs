using System.Net;
using System.Net.Http.Json;
using AdotaPet.Application.Accounts.Dtos;
using AdotaPet.Domain.Common;

namespace AdotaPet.Api.IntegrationTests;

[Collection(ApiCollection.Name)]
public sealed class AuthTests(ApiFactory factory)
{
    [Fact]
    public async Task Register_shelter_returns_201_with_shelter_role()
    {
        var client = factory.CreateClient();

        var response = await client.PostAsJsonAsync("/api/auth/register/shelter", TestAuth.NewShelter(), TestAuth.Json);

        Assert.Equal(HttpStatusCode.Created, response.StatusCode);
        var body = await response.Content.ReadFromJsonAsync<AuthResultDto>(TestAuth.Json);
        Assert.Equal("/api/me", response.Headers.Location?.OriginalString);
        Assert.Equal("Shelter", body!.Role);
        Assert.False(string.IsNullOrEmpty(body.AccessToken));
    }

    [Fact]
    public async Task Login_with_wrong_password_returns_401()
    {
        var client = factory.CreateClient();
        var dto = TestAuth.NewShelter();
        (await client.PostAsJsonAsync("/api/auth/register/shelter", dto, TestAuth.Json)).EnsureSuccessStatusCode();

        var response = await client.PostAsJsonAsync("/api/auth/login",
            new LoginDto(dto.Email, "SenhaErrada99"), TestAuth.Json);

        Assert.Equal(HttpStatusCode.Unauthorized, response.StatusCode);
    }

    [Fact]
    public async Task Register_with_duplicate_email_returns_409()
    {
        var client = factory.CreateClient();
        var email = TestAuth.UniqueEmail("dup");
        var first = await client.PostAsJsonAsync("/api/auth/register/shelter", TestAuth.NewShelter(email), TestAuth.Json);
        Assert.Equal(HttpStatusCode.Created, first.StatusCode);

        var second = await client.PostAsJsonAsync("/api/auth/register/shelter", TestAuth.NewShelter(email), TestAuth.Json);

        Assert.Equal(HttpStatusCode.Conflict, second.StatusCode);
        Assert.Equal(ConflictCodes.EmailTaken, await TestAuth.ConflictCodeAsync(second));
    }
}
