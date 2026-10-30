using System.Net;
using System.Net.Http.Headers;
using System.Net.Http.Json;
using AdotaPet.Application.Pets.Dtos;
using AdotaPet.Domain.Common;

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
        var shelter = await TestAuth.RegisterShelterAsync(factory);
        var petId = await TestAuth.CreatePetAsync(shelter, "Mel");

        var upload = await shelter.PutAsync($"/api/shelter/pets/{petId}/photo", FileContent(Png, "mel.png", "image/png"));
        Assert.Equal(HttpStatusCode.NoContent, upload.StatusCode);

        var list = await shelter.GetFromJsonAsync<PagedResult<PetSummaryDto>>("/api/pets?pageSize=48", TestAuth.Json);
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
        Assert.Contains(SizeMessage, await response.Content.ReadAsStringAsync());
    }

    [Fact]
    public async Task Upload_above_6_MB_is_rejected_with_size_message()
    {
        // Under Kestrel, [RequestSizeLimit(6 MB)] answers 413 with the same pt-BR detail. TestServer does not
        // implement IHttpMaxRequestBodySizeFeature, so the attribute cannot be enforced here and the use
        // case's own 5 MB check answers 400 instead. The 413 path needs a real Kestrel host to be exercised.
        var shelter = await TestAuth.RegisterShelterAsync(factory);
        var petId = await TestAuth.CreatePetAsync(shelter);
        var bytes = PngPaddedTo(7 * 1024 * 1024);

        var response = await shelter.PutAsync($"/api/shelter/pets/{petId}/photo", FileContent(bytes, "huge.png", "image/png"));

        Assert.Equal(HttpStatusCode.BadRequest, response.StatusCode);
        Assert.Contains(SizeMessage, await response.Content.ReadAsStringAsync());
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
}
