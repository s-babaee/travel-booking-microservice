using System.ComponentModel.DataAnnotations;
using Hotel.Api.Domain.Enums;

namespace Hotel.Api.Application.Contracts;

public class CreateRoomTypeRequest
{
    [Required]
    [MaxLength(200)]
    public string Name { get; init; } = null!;

    [MaxLength(2000)]
    public string? Description { get; init; }

    [Range(1, 100)]
    public int MaxOccupancy { get; init; }

    [MaxLength(120)]
    public string? BedType { get; init; }

    [Range(typeof(decimal), "0.01", "10000")]
    public decimal? SizeInSquareMeters { get; init; }

    [MaxLength(120)]
    public string? View { get; init; }
}

public sealed class UpdateRoomTypeRequest : CreateRoomTypeRequest
{
}

public sealed class ChangeRoomTypeStatusRequest
{
    public HotelStatus Status { get; init; }
}

public sealed record RoomTypeResponse(
    Guid Id,
    Guid HotelId,
    string Name,
    string? Description,
    int MaxOccupancy,
    string? BedType,
    decimal? SizeInSquareMeters,
    string? View,
    HotelStatus Status,
    DateTime CreatedAtUtc,
    DateTime UpdatedAtUtc,
    IReadOnlyList<AmenityResponse> Amenities,
    IReadOnlyList<RoomTypeImageResponse> Images);
