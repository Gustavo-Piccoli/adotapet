using AdotPet.Application.Pets.Photos;
using Microsoft.Extensions.Options;

namespace AdotPet.Infrastructure.Photos;

public sealed class LocalPhotoStorage : IPhotoStorage
{
    private static readonly char[] Separators = ['/', (char)92];

    private readonly string _root;

    public LocalPhotoStorage(IOptions<PhotoStorageOptions> options)
    {
        _root = Path.TrimEndingDirectorySeparator(Path.GetFullPath(options.Value.RootPath));
    }

    public async Task<string> SaveAsync(Stream content, string extension, CancellationToken ct)
    {
        Directory.CreateDirectory(_root);
        var safeExtension = Path.GetExtension(extension.StartsWith('.') ? extension : "." + extension);
        var fileName = $"{Guid.NewGuid():N}{safeExtension}";

        await using var file = new FileStream(Path.Combine(_root, fileName), FileMode.CreateNew,
            FileAccess.Write, FileShare.None, 4096, useAsync: true);
        await content.CopyToAsync(file, ct);
        return fileName;
    }

    public Task<Stream?> OpenReadAsync(string fileName, CancellationToken ct)
    {
        var path = Resolve(fileName);
        if (path is null || !File.Exists(path))
            return Task.FromResult<Stream?>(null);

        Stream stream = new FileStream(path, FileMode.Open, FileAccess.Read, FileShare.Read, 4096, useAsync: true);
        return Task.FromResult<Stream?>(stream);
    }

    public Task DeleteAsync(string fileName, CancellationToken ct)
    {
        var path = Resolve(fileName);
        if (path is not null && File.Exists(path))
            File.Delete(path);
        return Task.CompletedTask;
    }

    /// <summary>Returns the full path for a plain file name, or null when the name could escape the root.</summary>
    private string? Resolve(string fileName)
    {
        if (string.IsNullOrWhiteSpace(fileName)
            || fileName.Contains("..", StringComparison.Ordinal)
            || fileName.IndexOfAny(Separators) >= 0
            || fileName.Contains(':')
            || fileName.IndexOfAny(Path.GetInvalidFileNameChars()) >= 0)
            return null;

        var full = Path.GetFullPath(Path.Combine(_root, fileName));
        return Path.GetDirectoryName(full) == _root ? full : null;
    }
}
