using System.ComponentModel.DataAnnotations;
using Flight.Api.Domain.Enums;

namespace Flight.Api.Application.Contracts;

public class CreateAirlineRequest
{
    [Required]
    [MaxLength(200)]
    public string Name { get; init; } = null!;

    [Required]
    [StringLength(2, MinimumLength = 2)]
    public string IataCode { get; init; } = null!;

    [Required]
    [StringLength(3, MinimumLength = 3)]
    public string IcaoCode { get; init; } = null!;

    [Required]
    [MaxLength(120)]
    public string Country { get; init; } = null!;

    [Url]
    [MaxLength(500)]
    public string? WebsiteUrl { get; init; }
}

public sealed class UpdateAirlineRequest : CreateAirlineRequest
{
}

public sealed record AirlineResponse(
    Guid Id,
    string Name,
    string IataCode,
    string IcaoCode,
    string Country,
    string? WebsiteUrl,
    CatalogStatus Status,
    DateTime CreatedAtUtc,
    DateTime UpdatedAtUtc);
