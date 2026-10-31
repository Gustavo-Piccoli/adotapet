namespace AdotaPet.Infrastructure.Photos;

public sealed class PhotoStorageOptions
{
    public const string SectionName = "Storage";

    public string RootPath { get; set; } = "./data/photos";
}
