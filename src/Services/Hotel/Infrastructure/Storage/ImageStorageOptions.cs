namespace Hotel.Api.Infrastructure.Storage;

public sealed class ImageStorageOptions
{
    public string RootPath { get; init; } = "wwwroot/uploads";
    public string PublicBasePath { get; init; } = "/uploads";
    public long MaxFileSizeBytes { get; init; } = 10 * 1024 * 1024;
}
