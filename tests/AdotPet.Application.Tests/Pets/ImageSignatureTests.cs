using AdotPet.Application.Pets.Photos;

namespace AdotPet.Application.Tests.Pets;

public class ImageSignatureTests
{
    [Fact]
    public void Detect_Jpeg()
    {
        var result = ImageSignature.Detect([0xFF, 0xD8, 0xFF, 0xE0, 0, 0, 0, 0, 0, 0, 0, 0]);
        Assert.Equal((".jpg", "image/jpeg"), result);
    }

    [Fact]
    public void Detect_Png()
    {
        var result = ImageSignature.Detect([0x89, 0x50, 0x4E, 0x47, 0x0D, 0x0A, 0x1A, 0x0A, 0, 0, 0, 0]);
        Assert.Equal((".png", "image/png"), result);
    }

    [Fact]
    public void Detect_WebP()
    {
        byte[] header = [(byte)'R', (byte)'I', (byte)'F', (byte)'F', 1, 2, 3, 4, (byte)'W', (byte)'E', (byte)'B', (byte)'P'];
        Assert.Equal((".webp", "image/webp"), ImageSignature.Detect(header));
    }

    [Fact]
    public void Detect_Executable_ReturnsNull() =>
        Assert.Null(ImageSignature.Detect([(byte)'M', (byte)'Z', 0x90, 0, 3, 0, 0, 0, 4, 0, 0, 0]));

    [Fact]
    public void Detect_TooShort_ReturnsNull() =>
        Assert.Null(ImageSignature.Detect([0xFF, 0xD8]));
}
