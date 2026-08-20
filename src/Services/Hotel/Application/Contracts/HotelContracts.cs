using System.ComponentModel.DataAnnotations;
using Hotel.Api.Domain.Enums;

namespace Hotel.Api.Application.Contracts;

public class CreateHotelRequest
{
    [Required]
    [MaxLength(200)]
    public string Name { get; init; } = null!;

    [MaxLength(2000)]
    public string? Description { get; init; }

    [Range(0, 5)]
    public int StarRating { get; init; }

    [Required]
    [MaxLength(300)]
    public string AddressLine1 { get; init; } = null!;

    [MaxLength(300)]
    public string? AddressLine2 { get; init; }

    [Required]
    [MaxLength(120)]
    public string City { get; init; } = null!;

    [MaxLength(120)]
    public string? StateOrProvince { get; init; }

    [Required]
    [MaxLength(120)]
    public string Country { get; init; } = null!;

    [MaxLength(30)]
    public string? PostalCode { get; init; }

    [MaxLength(50)]
    public string? PhoneNumber { get; init; }

    [EmailAddress]
    [MaxLength(320)]
    public string? Email { get; init; }

    [Url]
    [MaxLength(500)]
    public string? WebsiteUrl { get; init; }

    [Range(-90, 90)]
    public decimal? Latitude { get; init; }

    [Range(-180, 180)]
    public decimal? Longitude { get; init; }
}

public sealed class UpdateHotelRequest : CreateHotelRequest
{
}

public sealed class ChangeHotelStatusRequest
{
    public HotelStatus Status { get; init; }
}

public sealed record HotelResponse(
    Guid Id,
    string Name,
    string? Description,
    int StarRating,
    string AddressLine1,
    string? AddressLine2,
    string City,
    string? StateOrProvince,
    string Country,
    string? PostalCode,
    string? PhoneNumber,
    string? Email,
    string? WebsiteUrl,
    decimal? Latitude,
    decimal? Longitude,
    HotelStatus Status,
    DateTime CreatedAtUtc,
    DateTime UpdatedAtUtc,
    IReadOnlyList<RoomTypeResponse> RoomTypes,
    IReadOnlyList<AmenityResponse> Amenities,
    IReadOnlyList<HotelPolicyResponse> Policies,
    IReadOnlyList<HotelImageResponse> Images);
