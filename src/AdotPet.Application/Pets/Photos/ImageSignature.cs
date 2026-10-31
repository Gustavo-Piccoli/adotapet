namespace AdotPet.Application.Pets.Photos;

public static class ImageSignature
{
    public const long MaxBytes = 5 * 1024 * 1024;

    public const int HeaderLength = 12;

    private static readonly byte[] Png = [0x89, 0x50, 0x4E, 0x47, 0x0D, 0x0A, 0x1A, 0x0A];
    private static readonly byte[] Riff = "RIFF"u8.ToArray();
    private static readonly byte[] Webp = "WEBP"u8.ToArray();

    public static (string Extension, string ContentType)? Detect(ReadOnlySpan<byte> header)
    {
        if (header.Length >= 3 && header[0] == 0xFF && header[1] == 0xD8 && header[2] == 0xFF)
            return (".jpg", "image/jpeg");

        if (header.Length >= Png.Length && header[..Png.Length].SequenceEqual(Png))
            return (".png", "image/png");

        if (header.Length >= HeaderLength && header[..4].SequenceEqual(Riff) && header[8..12].SequenceEqual(Webp))
            return (".webp", "image/webp");

        return null;
    }

    public static string ContentTypeFor(string fileName) =>
        Path.GetExtension(fileName).ToLowerInvariant() switch
        {
            ".jpg" or ".jpeg" => "image/jpeg",
            ".png" => "image/png",
            ".webp" => "image/webp",
            _ => "application/octet-stream",
        };
}
