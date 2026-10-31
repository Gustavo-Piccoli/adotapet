using AdotaPet.Application.Pets.Photos;
using AdotaPet.Domain.Common;
using AdotaPet.Domain.Pets;

namespace AdotaPet.Application.Pets.UseCases;

public sealed record PetPhoto(Stream Content, string ContentType);

public interface IGetPetPhotoUseCase
{
    Task<PetPhoto> ExecuteAsync(Guid petId, CancellationToken ct);
}

public sealed class GetPetPhotoUseCase(IPetRepository pets, IPhotoStorage storage) : IGetPetPhotoUseCase
{
    public async Task<PetPhoto> ExecuteAsync(Guid petId, CancellationToken ct)
    {
        var pet = await pets.GetByIdAsync(petId, ct);
        if (pet?.PhotoPath is null) throw new NotFoundException("Foto não encontrada.");

        var stream = await storage.OpenReadAsync(pet.PhotoPath, ct)
            ?? throw new NotFoundException("Foto não encontrada.");
        return new PetPhoto(stream, ImageSignature.ContentTypeFor(pet.PhotoPath));
    }
}
