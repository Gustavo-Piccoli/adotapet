namespace AdotPet.Application.Pets.Photos;

public interface IPhotoStorage
{
    /// <summary>Stores the content and returns the generated file name.</summary>
    Task<string> SaveAsync(Stream content, string extension, CancellationToken ct);

    Task<Stream?> OpenReadAsync(string fileName, CancellationToken ct);

    Task DeleteAsync(string fileName, CancellationToken ct);
}
