using System.ComponentModel.DataAnnotations;

namespace Flight.Api.Application.Contracts;

public class CreateRouteRequest
{
    [Required]
    [StringLength(3, MinimumLength = 3)]
    public string OriginAirportCode { get; init; } = null!;

    [Required]
    [StringLength(3, MinimumLength = 3)]
    public string DestinationAirportCode { get; init; } = null!;

    [Required]
    [MaxLength(120)]
    public string OriginCity { get; init; } = null!;

    [Required]
    [MaxLength(120)]
    public string DestinationCity { get; init; } = null!;

    [Range(1, 50000)]
    public int DistanceKm { get; init; }

    [Range(1, 2000)]
    public int TypicalDurationMinutes { get; init; }
}

public sealed class UpdateRouteRequest : CreateRouteRequest
{
}

public sealed record RouteResponse(
    Guid Id,
    string OriginAirportCode,
    string DestinationAirportCode,
    string OriginCity,
    string DestinationCity,
    int DistanceKm,
    int TypicalDurationMinutes,
    DateTime CreatedAtUtc,
    DateTime UpdatedAtUtc);
