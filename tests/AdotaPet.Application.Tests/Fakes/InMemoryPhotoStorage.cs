using AdotaPet.Application.Pets.Photos;

namespace AdotaPet.Application.Tests.Fakes;

public sealed class InMemoryPhotoStorage : IPhotoStorage
{
    public Dictionary<string, byte[]> Files { get; } = [];
    public List<string> Deleted { get; } = [];

    public async Task<string> SaveAsync(Stream content, string extension, CancellationToken ct)
    {
        using var buffer = new MemoryStream();
        await content.CopyToAsync(buffer, ct);
        var name = $"{Guid.NewGuid():N}{extension}";
        Files[name] = buffer.ToArray();
        return name;
    }

    public Task<Stream?> OpenReadAsync(string fileName, CancellationToken ct) =>
        Task.FromResult<Stream?>(Files.TryGetValue(fileName, out var bytes) ? new MemoryStream(bytes) : null);

    public Task DeleteAsync(string fileName, CancellationToken ct)
    {
        Files.Remove(fileName);
        Deleted.Add(fileName);
        return Task.CompletedTask;
    }
}
