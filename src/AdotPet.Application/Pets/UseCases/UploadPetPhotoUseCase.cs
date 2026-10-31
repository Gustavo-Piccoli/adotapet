using AdotPet.Application.Common;
using AdotPet.Application.Pets.Photos;
using AdotPet.Domain.Common;
using AdotPet.Domain.Pets;

namespace AdotPet.Application.Pets.UseCases;

public interface IUploadPetPhotoUseCase
{
    Task ExecuteAsync(Guid shelterId, Guid petId, Stream content, long length, CancellationToken ct);
}

public sealed class UploadPetPhotoUseCase(
    IPetRepository pets, IPhotoStorage storage, IUnitOfWork unitOfWork, IClock clock) : IUploadPetPhotoUseCase
{
    public async Task ExecuteAsync(Guid shelterId, Guid petId, Stream content, long length, CancellationToken ct)
    {
        var pet = await pets.GetByIdAsync(petId, ct);
        if (pet is null || pet.ShelterId != shelterId) throw PetMapping.PetNotFound();

        if (length <= 0 || length > ImageSignature.MaxBytes)
            throw new DomainValidationException("A foto deve ter no máximo 5 MB.");

        // The header must be re-readable by the storage, so make sure the stream can rewind.
        Stream source = content;
        if (!content.CanSeek)
        {
            var buffer = new MemoryStream();
            await content.CopyToAsync(buffer, ct);
            buffer.Position = 0;
            source = buffer;
        }

        var header = new byte[ImageSignature.HeaderLength];
        var read = await source.ReadAtLeastAsync(header, header.Length, throwOnEndOfStream: false, ct);
        var detected = ImageSignature.Detect(header.AsSpan(0, read))
            ?? throw new DomainValidationException("Envie uma imagem JPEG, PNG ou WebP.");
        source.Position = 0;

        var previous = pet.PhotoPath;
        var fileName = await storage.SaveAsync(source, detected.Extension, ct);
        try
        {
            pet.SetPhoto(fileName, clock.UtcNow);
            await unitOfWork.SaveChangesAsync(ct);
        }
        catch
        {
            await TryDeleteAsync(fileName);
            throw;
        }

        if (previous is not null) await TryDeleteAsync(previous);
    }

    private async Task TryDeleteAsync(string fileName)
    {
        try
        {
            await storage.DeleteAsync(fileName, CancellationToken.None);
        }
        catch
        {
            // Best effort: an orphaned file must not fail the request.
        }
    }
}
