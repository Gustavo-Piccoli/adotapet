using AdotaPet.Application.Common;
using AdotaPet.Application.Pets.Dtos;
using AdotaPet.Application.Pets.UseCases;
using AdotaPet.Application.Tests.Fakes;

namespace AdotaPet.Application.Tests.Pets;

public class SearchPetsUseCaseTests
{
    private readonly InMemoryPetRepository _pets = new();

    private SearchPetsUseCase Sut() => new(_pets);

    private static SearchPetsDto Query(int? page = null, int? size = null) =>
        new(null, null, null, null, null, null, page, size);

    [Fact]
    public async Task Search_ReturnsOnlyAvailable()
    {
        var shelter = InMemoryShelterRepository.NewShelter();
        _pets.Seed(shelter, "Rex");
        _pets.Seed(shelter, "Mel", adopted: true);

        var result = await Sut().ExecuteAsync(Query(), CancellationToken.None);

        Assert.Single(result.Items);
        Assert.Equal("Rex", result.Items[0].Name);
    }

    [Fact]
    public async Task Search_InvalidPaging_IsNormalized()
    {
        var result = await Sut().ExecuteAsync(Query(0, 1000), CancellationToken.None);

        Assert.Equal(1, result.Page);
        Assert.Equal(48, result.PageSize);
    }

    [Fact]
    public async Task Search_HugePage_IsCappedAndReturnsEmptyItems()
    {
        var result = await Sut().ExecuteAsync(Query(int.MaxValue), CancellationToken.None);

        Assert.Equal(Paging.MaxPage, result.Page);
        Assert.Empty(result.Items);
    }

    [Fact]
    public async Task Search_DefaultPageSizeIs12()
    {
        var result = await Sut().ExecuteAsync(Query(), CancellationToken.None);
        Assert.Equal(12, result.PageSize);
    }

    [Fact]
    public async Task Search_MapsPhotoUrlAndShelterCity()
    {
        var shelter = InMemoryShelterRepository.NewShelter("Canoas", "RS");
        var pet = _pets.Seed(shelter, photoPath: "abc.png");

        var result = await Sut().ExecuteAsync(Query(), CancellationToken.None);

        var item = Assert.Single(result.Items);
        Assert.Equal($"/api/pets/{pet.Id}/photo?v=abc", item.PhotoUrl);
        Assert.Equal("Canoas", item.City);
        Assert.Equal("RS", item.State);
        Assert.Equal("ONG Patinhas", item.ShelterName);
    }

    [Fact]
    public async Task Search_PetWithoutPhoto_HasNullPhotoUrl()
    {
        _pets.Seed(InMemoryShelterRepository.NewShelter());
        var result = await Sut().ExecuteAsync(Query(), CancellationToken.None);
        Assert.Null(result.Items[0].PhotoUrl);
    }
}
