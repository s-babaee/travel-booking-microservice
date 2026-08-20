using Hotel.Api.Domain.Common;

namespace Hotel.Api.Domain.Entities;

public sealed class HotelImage : Entity<Guid>
{
    private HotelImage()
    {
    }

    private HotelImage(
        Guid id,
        Guid hotelId,
        string url,
        string? altText,
        int displayOrder,
        bool isPrimary,
        DateTime createdAtUtc)
    {
        Id = id;
        HotelId = hotelId;
        Url = url;
        AltText = altText;
        DisplayOrder = displayOrder;
        IsPrimary = isPrimary;
        CreatedAtUtc = createdAtUtc;
    }

    public Guid HotelId { get; private set; }
    public string Url { get; private set; } = null!;
    public string? AltText { get; private set; }
    public int DisplayOrder { get; private set; }
    public bool IsPrimary { get; private set; }
    public DateTime CreatedAtUtc { get; private set; }

    public static HotelImage Create(
        Guid id,
        Guid hotelId,
        string url,
        string? altText,
        int displayOrder,
        bool isPrimary,
        DateTime createdAtUtc)
    {
        Validate(id, hotelId, url, displayOrder);

        return new HotelImage(
            id,
            hotelId,
            url.Trim(),
            NormalizeOptional(altText),
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

    private static void Validate(
        Guid id,
        Guid hotelId,
        string url,
        int displayOrder)
    {
        if (id == Guid.Empty || hotelId == Guid.Empty)
        {
            throw new DomainException("Image and hotel ids are required.");
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
    }

    private static string? NormalizeOptional(string? value)
    {
        return string.IsNullOrWhiteSpace(value) ? null : value.Trim();
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
