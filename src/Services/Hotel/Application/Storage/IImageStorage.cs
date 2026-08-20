using Microsoft.AspNetCore.Http;

namespace Hotel.Api.Application.Storage;

public sealed record StoredImage(
    string PublicUrl,
    string PhysicalPath);

public interface IImageStorage
{
    Task<StoredImage> SaveAsync(
        IFormFile file,
        string ownerType,
        Guid ownerId,
        CancellationToken cancellationToken);

    Task DeleteAsync(
        string publicUrl,
        CancellationToken cancellationToken);
}
