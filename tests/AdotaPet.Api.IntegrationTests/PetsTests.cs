using System.Net;
using System.Net.Http.Headers;
using System.Net.Http.Json;
using AdotaPet.Application.Accounts;
using AdotaPet.Application.Pets.Dtos;
using AdotaPet.Domain.Common;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.DependencyInjection;

namespace AdotaPet.Api.IntegrationTests;

[Collection(ApiCollection.Name)]
public sealed class PetsTests(ApiFactory factory)
{
    private const string SizeMessage = "A foto deve ter no máximo 5 MB.";

    // 1x1 PNG
    private static readonly byte[] Png = Convert.FromBase64String(
        "iVBORw0KGgoAAAANSUhEUgAAAAEAAAABCAYAAAAfFcSJAAAADUlEQVR42mP8z8BQDwAEhQGAhKmMIQAAAABJRU5ErkJggg==");

    private static MultipartFormDataContent FileContent(byte[] bytes, string fileName, string contentType)
    {
        var file = new ByteArrayContent(bytes);
        file.Headers.ContentType = new MediaTypeHeaderValue(contentType);
        return new MultipartFormDataContent { { file, "file", fileName } };
    }

    private static byte[] PngPaddedTo(int length)
    {
        var bytes = new byte[length];
        Png.CopyTo(bytes, 0);
        return bytes;
    }

    [Fact]
    public async Task Shelter_creates_pet_and_uploads_photo_then_pet_is_listed_with_photo_url()
    {
        var city = TestAuth.UniqueCity();
        var shelter = await TestAuth.RegisterShelterAsync(factory, city);
        var petId = await TestAuth.CreatePetAsync(shelter, "Mel");

        var upload = await shelter.PutAsync($"/api/shelter/pets/{petId}/photo", FileContent(Png, "mel.png", "image/png"));
        Assert.Equal(HttpStatusCode.NoContent, upload.StatusCode);

        var list = await shelter.GetFromJsonAsync<PagedResult<PetSummaryDto>>($"/api/pets?state=RS&city={city}&pageSize=48", TestAuth.Json);
        var pet = Assert.Single(list!.Items, p => p.Id == petId);
        Assert.False(string.IsNullOrEmpty(pet.PhotoUrl));
    }

    [Fact]
    public async Task Upload_of_non_image_named_jpg_returns_400()
    {
        var shelter = await TestAuth.RegisterShelterAsync(factory);
        var petId = await TestAuth.CreatePetAsync(shelter);
        byte[] notAnImage = [(byte)'M', (byte)'Z', 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0];

        var response = await shelter.PutAsync($"/api/shelter/pets/{petId}/photo",
            FileContent(notAnImage, "x.jpg", "image/jpeg"));

        Assert.Equal(HttpStatusCode.BadRequest, response.StatusCode);
    }

    [Fact]
    public async Task Upload_between_5_and_6_MB_returns_400_with_size_message()
    {
        var shelter = await TestAuth.RegisterShelterAsync(factory);
        var petId = await TestAuth.CreatePetAsync(shelter);
        var bytes = PngPaddedTo(5 * 1024 * 1024 + 512 * 1024);

        var response = await shelter.PutAsync($"/api/shelter/pets/{petId}/photo", FileContent(bytes, "big.png", "image/png"));

        Assert.Equal(HttpStatusCode.BadRequest, response.StatusCode);
        var problem = await response.Content.ReadFromJsonAsync<ProblemDetails>();
        Assert.Equal(SizeMessage, problem!.Detail);
    }

    [Fact]
    public async Task Upload_above_6_MB_is_rejected_with_size_message()
    {
        // Under Kestrel, [RequestSizeLimit(6 MB)] makes reading the form throw BadHttpRequestException(413),
        // which DomainExceptionHandler turns into a 413 with the same pt-BR detail. TestServer has no
        // IHttpMaxRequestBodySizeFeature, so the limit is never enforced here and the use case's own 5 MB
        // check answers 400 instead: this test cannot observe the 413 (it needs a real Kestrel host).
        var shelter = await TestAuth.RegisterShelterAsync(factory);
        var petId = await TestAuth.CreatePetAsync(shelter);
        var bytes = PngPaddedTo(7 * 1024 * 1024);

        var response = await shelter.PutAsync($"/api/shelter/pets/{petId}/photo", FileContent(bytes, "huge.png", "image/png"));

        Assert.Equal(HttpStatusCode.BadRequest, response.StatusCode);
        var problem = await response.Content.ReadFromJsonAsync<ProblemDetails>();
        Assert.Equal(SizeMessage, problem!.Detail);
    }

    [Fact]
    public async Task Shelter_cannot_update_pet_of_another_shelter()
    {
        var shelterA = await TestAuth.RegisterShelterAsync(factory);
        var shelterB = await TestAuth.RegisterShelterAsync(factory);
        var petOfA = await TestAuth.CreatePetAsync(shelterA);

        var response = await shelterB.PutAsJsonAsync($"/api/shelter/pets/{petOfA}", TestAuth.NewPet("Roubado"), TestAuth.Json);

        Assert.Equal(HttpStatusCode.NotFound, response.StatusCode);
    }

    [Fact]
    public async Task Adopter_cannot_list_shelter_pets()
    {
        var adopter = await TestAuth.RegisterAdopterAsync(factory);

        var response = await adopter.GetAsync("/api/shelter/pets");

        Assert.Equal(HttpStatusCode.Forbidden, response.StatusCode);
    }

    [Fact]
    public async Task City_filter_ignores_case_and_accents_and_matches_prefix()
    {
        var suffix = Guid.NewGuid().ToString("N")[..8];
        var shelter = await TestAuth.RegisterShelterAsync(factory, $"São José {suffix}");
        var petId = await TestAuth.CreatePetAsync(shelter);

        var list = await shelter.GetFromJsonAsync<PagedResult<PetSummaryDto>>(
            $"/api/pets?state=RS&city=sao%20jose%20{suffix[..4]}&pageSize=48", TestAuth.Json);

        Assert.Contains(list!.Items, p => p.Id == petId);
    }

    [Fact]
    public async Task Invalid_enum_value_returns_pt_BR_validation_problem()
    {
        var shelter = await TestAuth.RegisterShelterAsync(factory);

        var response = await shelter.PostAsJsonAsync("/api/shelter/pets",
            new { name = "Rex", species = "Dragon", sex = "Male", size = "Medium", ageGroup = "Adult",
                  isNeutered = true, isVaccinated = true, description = "Dócil" }, TestAuth.Json);

        Assert.Equal(HttpStatusCode.BadRequest, response.StatusCode);
        var problem = await response.Content.ReadFromJsonAsync<ValidationProblemDetails>();
        Assert.Equal("Um ou mais campos são inválidos.", problem!.Title);
        Assert.Contains(problem.Errors.Values.SelectMany(e => e), m => m.StartsWith("Valor inválido para o campo"));
    }

    [Fact]
    public async Task Token_of_a_profile_that_no_longer_exists_returns_401()
    {
        // Same shape as a token issued before a database reset: valid signature, unknown profile_id.
        var token = factory.Services.GetRequiredService<ITokenService>().CreateToken(
            Guid.NewGuid().ToString(), "fantasma@teste.com", Roles.Shelter, Guid.NewGuid(), "Fantasma").AccessToken;
        var client = factory.CreateClient();
        client.DefaultRequestHeaders.Authorization = new AuthenticationHeaderValue("Bearer", token);

        var response = await client.PostAsJsonAsync("/api/shelter/pets", TestAuth.NewPet(), TestAuth.Json);

        Assert.Equal(HttpStatusCode.Unauthorized, response.StatusCode);
    }
}
