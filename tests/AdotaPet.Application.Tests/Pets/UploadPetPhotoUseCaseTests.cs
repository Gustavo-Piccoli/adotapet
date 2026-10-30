using AdotaPet.Application.Pets.Photos;
using AdotaPet.Application.Pets.UseCases;
using AdotaPet.Application.Tests.Fakes;
using AdotaPet.Domain.Common;
using AdotaPet.Domain.Shelters;

namespace AdotaPet.Application.Tests.Pets;

public class UploadPetPhotoUseCaseTests
{
    private readonly InMemoryPetRepository _pets = new();
    private readonly InMemoryPhotoStorage _storage = new();
    private readonly FakeUnitOfWork _uow = new();
    private readonly Shelter _shelter = InMemoryShelterRepository.NewShelter();

    private UploadPetPhotoUseCase Sut() => new(_pets, _storage, _uow, new FakeClock());

    private static byte[] Png() =>
        [0x89, 0x50, 0x4E, 0x47, 0x0D, 0x0A, 0x1A, 0x0A, 1, 2, 3, 4, 5, 6, 7, 8];

    private static Task Upload(UploadPetPhotoUseCase sut, Guid shelterId, Guid petId, byte[] bytes, long? length = null) =>
        sut.ExecuteAsync(shelterId, petId, new MemoryStream(bytes), length ?? bytes.Length, CancellationToken.None);

    [Fact]
    public async Task Upload_NonImage_ThrowsValidation_AndStoresNothing()
    {
        var pet = _pets.Seed(_shelter);
        byte[] exe = [(byte)'M', (byte)'Z', 0x90, 0, 3, 0, 0, 0, 4, 0, 0, 0, 0xFF, 0xFF];

        var ex = await Assert.ThrowsAsync<DomainValidationException>(() => Upload(Sut(), _shelter.Id, pet.Id, exe));

        Assert.Equal("Envie uma imagem JPEG, PNG ou WebP.", ex.Message);
        Assert.Empty(_storage.Files);
        Assert.Null(pet.PhotoPath);
    }

    [Fact]
    public async Task Upload_TooLarge_ThrowsValidation()
    {
        var pet = _pets.Seed(_shelter);

        var ex = await Assert.ThrowsAsync<DomainValidationException>(() =>
            Upload(Sut(), _shelter.Id, pet.Id, Png(), ImageSignature.MaxBytes + 1));

        Assert.Equal("A foto deve ter no máximo 5 MB.", ex.Message);
        Assert.Empty(_storage.Files);
    }

    [Fact]
    public async Task Upload_Empty_ThrowsValidation()
    {
        var pet = _pets.Seed(_shelter);
        await Assert.ThrowsAsync<DomainValidationException>(() => Upload(Sut(), _shelter.Id, pet.Id, []));
    }

    [Fact]
    public async Task Upload_Png_SetsPhotoAndDeletesPrevious()
    {
        var pet = _pets.Seed(_shelter, photoPath: "old.jpg");
        _storage.Files["old.jpg"] = [1];

        await Upload(Sut(), _shelter.Id, pet.Id, Png());

        Assert.EndsWith(".png", pet.PhotoPath);
        Assert.Contains(pet.PhotoPath!, _storage.Files.Keys);
        Assert.Equal(Png(), _storage.Files[pet.PhotoPath!]);
        Assert.Equal(["old.jpg"], _storage.Deleted);
        Assert.Equal(1, _uow.SaveChangesCount);
    }

    [Fact]
    public async Task Upload_PetOfOtherShelter_ThrowsNotFound()
    {
        var pet = _pets.Seed(_shelter);
        await Assert.ThrowsAsync<NotFoundException>(() => Upload(Sut(), Guid.NewGuid(), pet.Id, Png()));
        Assert.Empty(_storage.Files);
    }
}
