using System.Net.Http.Headers;
using System.Net.Http.Json;
using System.Text.Json;
using System.Text.Json.Serialization;
using AdotaPet.Application.Accounts.Dtos;
using AdotaPet.Application.Pets.Dtos;
using AdotaPet.Domain.Pets;

namespace AdotaPet.Api.IntegrationTests;

public static class TestAuth
{
    public const string Password = "Senha1234";

    public static readonly JsonSerializerOptions Json = new(JsonSerializerDefaults.Web)
    {
        Converters = { new JsonStringEnumConverter() },
    };

    private static long _counter = DateTime.UtcNow.Ticks % 100_000_000;

    public static string UniqueEmail(string prefix) => $"{prefix}-{Guid.NewGuid():N}@teste.com";

    /// <summary>Builds a CNPJ with valid check digits, unique per call.</summary>
    public static string UniqueCnpj()
    {
        var n = Interlocked.Increment(ref _counter);
        var digits = new List<int>(n.ToString("D8").Select(c => c - '0')) { 0, 0, 0, 1 };
        digits.Add(CheckDigit(digits, [5, 4, 3, 2, 9, 8, 7, 6, 5, 4, 3, 2]));
        digits.Add(CheckDigit(digits, [6, 5, 4, 3, 2, 9, 8, 7, 6, 5, 4, 3, 2]));
        return string.Concat(digits);
    }

    private static int CheckDigit(List<int> digits, int[] weights)
    {
        var sum = 0;
        for (var i = 0; i < weights.Length; i++) sum += digits[i] * weights[i];
        var remainder = sum % 11;
        return remainder < 2 ? 0 : 11 - remainder;
    }

    public static RegisterShelterDto NewShelter(string? email = null) =>
        new(email ?? UniqueEmail("ong"), Password, "ONG Patinhas", UniqueCnpj(), "Porto Alegre", "RS",
            "(51) 99999-0000", "Abrigo de testes");

    public static async Task<HttpClient> RegisterShelterAsync(ApiFactory factory)
    {
        var client = factory.CreateClient();
        var response = await client.PostAsJsonAsync("/api/auth/register/shelter", NewShelter(), Json);
        return await AuthenticateAsync(client, response);
    }

    public static async Task<HttpClient> RegisterAdopterAsync(ApiFactory factory)
    {
        var client = factory.CreateClient();
        var dto = new RegisterAdopterDto(UniqueEmail("adotante"), Password, "Maria Souza", "51988887777",
            "Porto Alegre", "RS");
        var response = await client.PostAsJsonAsync("/api/auth/register/adopter", dto, Json);
        return await AuthenticateAsync(client, response);
    }

    public static PetInputDto NewPet(string name = "Rex") =>
        new(name, Species.Dog, Sex.Male, Size.Medium, AgeGroup.Adult, true, true, "Dócil e brincalhão");

    public static async Task<Guid> CreatePetAsync(HttpClient shelter, string name = "Rex")
    {
        var response = await shelter.PostAsJsonAsync("/api/shelter/pets", NewPet(name), Json);
        response.EnsureSuccessStatusCode();
        return (await response.Content.ReadFromJsonAsync<IdBody>(Json))!.Id;
    }

    public static async Task<Guid> RequestAdoptionAsync(HttpClient adopter, Guid petId)
    {
        var response = await adopter.PostAsJsonAsync($"/api/pets/{petId}/adoption-requests",
            new { message = "Quero muito adotar!" }, Json);
        response.EnsureSuccessStatusCode();
        return (await response.Content.ReadFromJsonAsync<IdBody>(Json))!.Id;
    }

    private static async Task<HttpClient> AuthenticateAsync(HttpClient client, HttpResponseMessage response)
    {
        response.EnsureSuccessStatusCode();
        var result = (await response.Content.ReadFromJsonAsync<AuthResultDto>(Json))!;
        client.DefaultRequestHeaders.Authorization = new AuthenticationHeaderValue("Bearer", result.AccessToken);
        return client;
    }

    public sealed record IdBody(Guid Id);
}
