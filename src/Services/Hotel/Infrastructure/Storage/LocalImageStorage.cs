using Hotel.Api.Application.Exceptions;
using Hotel.Api.Application.Storage;
using Microsoft.AspNetCore.Http;
using Microsoft.Extensions.Options;

namespace Hotel.Api.Infrastructure.Storage;

public sealed class LocalImageStorage : IImageStorage
{
    private static readonly IReadOnlyDictionary<string, string> AllowedContentTypes =
        new Dictionary<string, string>(StringComparer.OrdinalIgnoreCase)
        {
            ["image/jpeg"] = ".jpg",
            ["image/png"] = ".png",
            ["image/webp"] = ".webp",
            ["image/gif"] = ".gif"
        };

    private readonly string _rootPath;
    private readonly string _publicBasePath;
    private readonly long _maxFileSizeBytes;

    public LocalImageStorage(
        IWebHostEnvironment environment,
        IOptions<ImageStorageOptions> options)
    {
        var storageOptions = options.Value;
        var configuredRoot = storageOptions.RootPath.Trim();
        _rootPath = Path.GetFullPath(
            Path.IsPathRooted(configuredRoot)
                ? configuredRoot
                : Path.Combine(environment.ContentRootPath, configuredRoot));
        _publicBasePath = NormalizePublicPath(storageOptions.PublicBasePath);
        _maxFileSizeBytes = storageOptions.MaxFileSizeBytes;
    }

    public async Task<StoredImage> SaveAsync(
        IFormFile file,
        string ownerType,
        Guid ownerId,
        CancellationToken cancellationToken)
    {
        await ValidateFileAsync(file, cancellationToken);

        if (ownerId == Guid.Empty)
        {
            throw new ValidationException("Image owner id is required.");
        }

        var normalizedOwnerType = NormalizeOwnerType(ownerType);
        var extension = AllowedContentTypes[file.ContentType];
        var fileName = $"{Guid.NewGuid():N}{extension}";
        var directory = Path.Combine(
            _rootPath,
            normalizedOwnerType,
            ownerId.ToString("N"));

        Directory.CreateDirectory(directory);

        var physicalPath = Path.Combine(directory, fileName);
        await using (var output = new FileStream(
            physicalPath,
            FileMode.CreateNew,
            FileAccess.Write,
            FileShare.None,
            bufferSize: 64 * 1024,
            useAsync: true))
        {
            await file.CopyToAsync(output, cancellationToken);
        }

        var publicUrl =
            $"{_publicBasePath}/{normalizedOwnerType}/{ownerId:N}/{fileName}";

        return new StoredImage(publicUrl, physicalPath);
    }

    public Task DeleteAsync(
        string publicUrl,
        CancellationToken cancellationToken)
    {
        cancellationToken.ThrowIfCancellationRequested();

        if (!TryResolvePhysicalPath(publicUrl, out var physicalPath))
        {
            return Task.CompletedTask;
        }

        if (File.Exists(physicalPath))
        {
            File.Delete(physicalPath);
        }

        return Task.CompletedTask;
    }

    private async Task ValidateFileAsync(
        IFormFile? file,
        CancellationToken cancellationToken)
    {
        if (file is null || file.Length == 0)
        {
            throw new ValidationException("An image file is required.");
        }

        if (file.Length > _maxFileSizeBytes)
        {
            throw new ValidationException(
                $"Image file cannot exceed {_maxFileSizeBytes / (1024 * 1024)} MB.");
        }

        if (!AllowedContentTypes.ContainsKey(file.ContentType))
        {
            throw new ValidationException(
                "Only JPEG, PNG, WEBP and GIF images are supported.");
        }

        if (!await HasValidImageSignatureAsync(file, cancellationToken))
        {
            throw new ValidationException(
                "The uploaded file is not a valid image.");
        }
    }

    private static async Task<bool> HasValidImageSignatureAsync(
        IFormFile file,
        CancellationToken cancellationToken)
    {
        await using var stream = file.OpenReadStream();
        var header = new byte[12];
        var bytesRead = 0;

        while (bytesRead < header.Length)
        {
            var read = await stream.ReadAsync(
                header.AsMemory(bytesRead, header.Length - bytesRead),
                cancellationToken);
            if (read == 0)
            {
                break;
            }

            bytesRead += read;
        }

        return file.ContentType switch
        {
            "image/jpeg" => bytesRead >= 3
                && header[0] == 0xFF
                && header[1] == 0xD8
                && header[2] == 0xFF,
            "image/png" => bytesRead >= 8
                && header.AsSpan(0, 8).SequenceEqual(
                    new byte[]
                    {
                        0x89, 0x50, 0x4E, 0x47,
                        0x0D, 0x0A, 0x1A, 0x0A
                    }),
            "image/gif" => bytesRead >= 6
                && (header.AsSpan(0, 6).SequenceEqual(
                        "GIF87a"u8)
                    || header.AsSpan(0, 6).SequenceEqual(
                        "GIF89a"u8)),
            "image/webp" => bytesRead >= 12
                && header.AsSpan(0, 4).SequenceEqual("RIFF"u8)
                && header.AsSpan(8, 4).SequenceEqual("WEBP"u8),
            _ => false
        };
    }

    private bool TryResolvePhysicalPath(
        string publicUrl,
        out string physicalPath)
    {
        physicalPath = string.Empty;

        if (string.IsNullOrWhiteSpace(publicUrl)
            || !publicUrl.StartsWith(
                $"{_publicBasePath}/",
                StringComparison.Ordinal))
        {
            return false;
        }

        var relativePath = publicUrl[
            (_publicBasePath.Length + 1)..]
            .Replace('/', Path.DirectorySeparatorChar);
        var candidate = Path.GetFullPath(
            Path.Combine(_rootPath, relativePath));
        var rootWithSeparator = _rootPath.EndsWith(
            Path.DirectorySeparatorChar)
            ? _rootPath
            : _rootPath + Path.DirectorySeparatorChar;

        if (!candidate.StartsWith(
                rootWithSeparator,
                StringComparison.OrdinalIgnoreCase))
        {
            return false;
        }

        physicalPath = candidate;
        return true;
    }

    private static string NormalizeOwnerType(string ownerType)
    {
        var normalized = ownerType.Trim().ToLowerInvariant();
        return normalized switch
        {
            "hotel" => "hotels",
            "room-type" => "room-types",
            _ => throw new ValidationException("Unsupported image owner type.")
        };
    }

    private static string NormalizePublicPath(string publicBasePath)
    {
        var normalized = string.IsNullOrWhiteSpace(publicBasePath)
            ? "/uploads"
            : publicBasePath.Trim();

        normalized = "/" + normalized.Trim('/');
        return normalized == "/" ? "/uploads" : normalized;
    }
}
