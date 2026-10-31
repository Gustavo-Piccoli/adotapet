using AdotPet.Infrastructure.Photos;
using Microsoft.Extensions.Options;

namespace AdotPet.Infrastructure.Tests;

public sealed class LocalPhotoStorageTests : IDisposable
{
    private readonly string _root = Path.Combine(Path.GetTempPath(), "adotpet-tests-" + Guid.NewGuid().ToString("N"));
    private readonly LocalPhotoStorage _storage;

    public LocalPhotoStorageTests() =>
        _storage = new LocalPhotoStorage(Options.Create(new PhotoStorageOptions { RootPath = _root }));

    public void Dispose()
    {
        if (Directory.Exists(_root)) Directory.Delete(_root, recursive: true);
    }

    [Fact]
    public async Task Save_ThenOpen_ReturnsSameBytes()
    {
        byte[] bytes = [1, 2, 3, 4, 5];

        var name = await _storage.SaveAsync(new MemoryStream(bytes), ".jpg", CancellationToken.None);

        Assert.EndsWith(".jpg", name);
        Assert.Matches("^[0-9a-f]{32}[.]jpg$", name);
        await using var stream = await _storage.OpenReadAsync(name, CancellationToken.None);
        Assert.NotNull(stream);
        var ms = new MemoryStream();
        await stream!.CopyToAsync(ms);
        Assert.Equal(bytes, ms.ToArray());
    }

    [Theory]
    [InlineData("../x.jpg")]
    [InlineData("..\\x.jpg")]
    [InlineData("a/b.jpg")]
    [InlineData("a\\b.jpg")]
    public async Task OpenRead_WithPathTraversal_ReturnsNull(string fileName)
    {
        Assert.Null(await _storage.OpenReadAsync(fileName, CancellationToken.None));
    }

    [Fact]
    public async Task Save_ThenOpen_WithTrailingSeparatorInRoot_ReturnsSameBytes()
    {
        var storage = new LocalPhotoStorage(Options.Create(
            new PhotoStorageOptions { RootPath = _root + Path.DirectorySeparatorChar }));
        byte[] bytes = [7, 8, 9];

        var name = await storage.SaveAsync(new MemoryStream(bytes), ".webp", CancellationToken.None);

        var ms = new MemoryStream();
        await using (var stream = await storage.OpenReadAsync(name, CancellationToken.None))
        {
            Assert.NotNull(stream);
            await stream!.CopyToAsync(ms);
        }
        Assert.Equal(bytes, ms.ToArray());
        await storage.DeleteAsync(name, CancellationToken.None);
        Assert.Null(await storage.OpenReadAsync(name, CancellationToken.None));
    }

    [Fact]
    public async Task OpenRead_WithColon_ReturnsNull()
    {
        Assert.Null(await _storage.OpenReadAsync("c:x.jpg", CancellationToken.None));
    }

    [Fact]
    public async Task OpenRead_MissingFile_ReturnsNull()
    {
        Assert.Null(await _storage.OpenReadAsync("missing.jpg", CancellationToken.None));
    }

    [Fact]
    public async Task Delete_RemovesFile()
    {
        var name = await _storage.SaveAsync(new MemoryStream([9]), ".png", CancellationToken.None);

        await _storage.DeleteAsync(name, CancellationToken.None);

        Assert.Null(await _storage.OpenReadAsync(name, CancellationToken.None));
        Assert.False(File.Exists(Path.Combine(_root, name)));
    }

    [Fact]
    public async Task Delete_WithPathTraversal_DoesNotTouchOutsideFiles()
    {
        Directory.CreateDirectory(_root);
        var outside = Path.Combine(Path.GetTempPath(), "adotpet-outside-" + Guid.NewGuid().ToString("N") + ".jpg");
        await File.WriteAllBytesAsync(outside, [1]);
        try
        {
            await _storage.DeleteAsync("../" + Path.GetFileName(outside), CancellationToken.None);
            Assert.True(File.Exists(outside));
        }
        finally { File.Delete(outside); }
    }
}
