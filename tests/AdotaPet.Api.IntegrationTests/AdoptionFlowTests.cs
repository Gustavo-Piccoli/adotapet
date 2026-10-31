using System.Net;
using System.Net.Http.Json;
using AdotaPet.Application.AdoptionRequests.Dtos;
using AdotaPet.Application.Pets.Dtos;
using AdotaPet.Domain.AdoptionRequests;
using AdotaPet.Domain.Common;
using AdotaPet.Domain.Pets;

namespace AdotaPet.Api.IntegrationTests;

[Collection(ApiCollection.Name)]
public sealed class AdoptionFlowTests(ApiFactory factory)
{
    [Fact]
    public async Task Duplicate_adoption_request_for_the_same_pet_returns_409()
    {
        var shelter = await TestAuth.RegisterShelterAsync(factory);
        var adopter = await TestAuth.RegisterAdopterAsync(factory);
        var petId = await TestAuth.CreatePetAsync(shelter);

        var first = await adopter.PostAsJsonAsync($"/api/pets/{petId}/adoption-requests",
            new { message = "Quero adotar" }, TestAuth.Json);
        var second = await adopter.PostAsJsonAsync($"/api/pets/{petId}/adoption-requests",
            new { message = "Quero adotar de novo" }, TestAuth.Json);

        Assert.Equal(HttpStatusCode.Created, first.StatusCode);
        Assert.Equal(HttpStatusCode.Conflict, second.StatusCode);
        Assert.Equal(ConflictCodes.DuplicatePendingRequest, await TestAuth.ConflictCodeAsync(second));
    }

    [Fact]
    public async Task Request_for_an_adopted_pet_returns_409_with_pet_unavailable_code()
    {
        var shelter = await TestAuth.RegisterShelterAsync(factory);
        var adopterA = await TestAuth.RegisterAdopterAsync(factory);
        var adopterB = await TestAuth.RegisterAdopterAsync(factory);
        var petId = await TestAuth.CreatePetAsync(shelter);
        var requestA = await TestAuth.RequestAdoptionAsync(adopterA, petId);
        (await shelter.PostAsync($"/api/shelter/adoption-requests/{requestA}/approve", null)).EnsureSuccessStatusCode();

        var response = await adopterB.PostAsJsonAsync($"/api/pets/{petId}/adoption-requests",
            new { message = "Também quero adotar" }, TestAuth.Json);

        Assert.Equal(HttpStatusCode.Conflict, response.StatusCode);
        Assert.Equal(ConflictCodes.PetUnavailable, await TestAuth.ConflictCodeAsync(response));
    }

    [Fact]
    public async Task Approving_a_request_adopts_the_pet_and_rejects_the_other_pending_requests()
    {
        var city = TestAuth.UniqueCity();
        var shelter = await TestAuth.RegisterShelterAsync(factory, city);
        var adopterA = await TestAuth.RegisterAdopterAsync(factory);
        var adopterB = await TestAuth.RegisterAdopterAsync(factory);
        var petId = await TestAuth.CreatePetAsync(shelter);
        var requestA = await TestAuth.RequestAdoptionAsync(adopterA, petId);
        var requestB = await TestAuth.RequestAdoptionAsync(adopterB, petId);
        var searchUrl = $"/api/pets?state=RS&city={city}&pageSize=48";
        var before = await shelter.GetFromJsonAsync<PagedResult<PetSummaryDto>>(searchUrl, TestAuth.Json);
        Assert.Contains(before!.Items, p => p.Id == petId);

        var approve = await shelter.PostAsync($"/api/shelter/adoption-requests/{requestA}/approve", null);

        Assert.Equal(HttpStatusCode.NoContent, approve.StatusCode);

        var pet = await shelter.GetFromJsonAsync<ShelterPetDto>($"/api/shelter/pets/{petId}", TestAuth.Json);
        Assert.Equal(PetStatus.Adopted, pet!.Status);

        var requestsOfB = await adopterB.GetFromJsonAsync<List<AdopterRequestDto>>(
            "/api/adopter/adoption-requests", TestAuth.Json);
        Assert.Equal(AdoptionRequestStatus.Rejected, requestsOfB!.Single(r => r.Id == requestB).Status);

        var listed = await shelter.GetFromJsonAsync<PagedResult<PetSummaryDto>>(searchUrl, TestAuth.Json);
        Assert.DoesNotContain(listed!.Items, p => p.Id == petId);
    }

    [Fact]
    public async Task Concurrent_approvals_for_the_same_pet_yield_one_204_and_one_409()
    {
        var shelter = await TestAuth.RegisterShelterAsync(factory);
        var adopterA = await TestAuth.RegisterAdopterAsync(factory);
        var adopterB = await TestAuth.RegisterAdopterAsync(factory);
        var petId = await TestAuth.CreatePetAsync(shelter);
        var requestA = await TestAuth.RequestAdoptionAsync(adopterA, petId);
        var requestB = await TestAuth.RequestAdoptionAsync(adopterB, petId);

        var gate = new TaskCompletionSource();

        async Task<HttpStatusCode> Approve(Guid requestId)
        {
            await gate.Task;
            return (await shelter.PostAsync($"/api/shelter/adoption-requests/{requestId}/approve", null)).StatusCode;
        }

        var tasks = new[] { Task.Run(() => Approve(requestA)), Task.Run(() => Approve(requestB)) };
        gate.SetResult();
        var results = await Task.WhenAll(tasks);

        Assert.Equal(1, results.Count(s => s == HttpStatusCode.NoContent));
        Assert.Equal(1, results.Count(s => s == HttpStatusCode.Conflict));
    }
}
