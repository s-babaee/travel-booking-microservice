using Hotel.Api.Domain.Common;

namespace Hotel.Api.Domain.Entities;

public sealed class RoomTypeImage : Entity<Guid>
{
    private RoomTypeImage()
    {
    }

    private RoomTypeImage(
        Guid id,
        Guid roomTypeId,
        string url,
        string? altText,
        int displayOrder,
        bool isPrimary,
        DateTime createdAtUtc)
    {
        Id = id;
        RoomTypeId = roomTypeId;
        Url = url;
        AltText = altText;
        DisplayOrder = displayOrder;
        IsPrimary = isPrimary;
        CreatedAtUtc = createdAtUtc;
    }

    public Guid RoomTypeId { get; private set; }
    public string Url { get; private set; } = null!;
    public string? AltText { get; private set; }
    public int DisplayOrder { get; private set; }
    public bool IsPrimary { get; private set; }
    public DateTime CreatedAtUtc { get; private set; }

    public static RoomTypeImage Create(
        Guid id,
        Guid roomTypeId,
        string url,
        string? altText,
        int displayOrder,
        bool isPrimary,
        DateTime createdAtUtc)
    {
        if (id == Guid.Empty || roomTypeId == Guid.Empty)
        {
            throw new DomainException("Image and room type ids are required.");
        }

        if (!IsValidImageLocation(url))
        {
            throw new DomainException(
                "Image URL must be a valid HTTP/HTTPS URL or an application upload path.");
        }

        if (displayOrder < 0)
        {
            throw new DomainException("Image display order cannot be negative.");
        }

        return new RoomTypeImage(
            id,
            roomTypeId,
            url.Trim(),
            string.IsNullOrWhiteSpace(altText) ? null : altText.Trim(),
            displayOrder,
            isPrimary,
            createdAtUtc);
    }

    public void MarkPrimary()
    {
        IsPrimary = true;
    }

    public void MarkSecondary()
    {
        IsPrimary = false;
    }

    private static bool IsValidImageLocation(string value)
    {
        if (string.IsNullOrWhiteSpace(value))
        {
            return false;
        }

        var normalized = value.Trim();
        if (normalized.StartsWith("/uploads/", StringComparison.Ordinal))
        {
            return true;
        }

        return Uri.TryCreate(normalized, UriKind.Absolute, out var uri)
            && uri.Scheme is "http" or "https";
    }
}
