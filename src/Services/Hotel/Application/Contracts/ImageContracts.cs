using System.ComponentModel.DataAnnotations;

namespace Hotel.Api.Application.Contracts;

public sealed class AddHotelImageRequest
{
    [Required]
    [Url]
    [MaxLength(1000)]
    public string Url { get; init; } = null!;

    [MaxLength(300)]
    public string? AltText { get; init; }

    [Range(0, int.MaxValue)]
    public int DisplayOrder { get; init; }

    public bool IsPrimary { get; init; }
}

public sealed class AddRoomTypeImageRequest
{
    [Required]
    [Url]
    [MaxLength(1000)]
    public string Url { get; init; } = null!;

    [MaxLength(300)]
    public string? AltText { get; init; }

    [Range(0, int.MaxValue)]
    public int DisplayOrder { get; init; }

    public bool IsPrimary { get; init; }
}

public sealed record HotelImageResponse(
    Guid Id,
    Guid HotelId,
    string Url,
    string? AltText,
    int DisplayOrder,
    bool IsPrimary,
    DateTime CreatedAtUtc);

public sealed record RoomTypeImageResponse(
    Guid Id,
    Guid RoomTypeId,
    string Url,
    string? AltText,
    int DisplayOrder,
    bool IsPrimary,
    DateTime CreatedAtUtc);
