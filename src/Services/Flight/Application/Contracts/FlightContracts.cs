using System.ComponentModel.DataAnnotations;
using Flight.Api.Domain.Enums;

namespace Flight.Api.Application.Contracts;

public class CreateFlightRequest
{
    [Required]
    public Guid AirlineId { get; init; }

    [Required]
    public Guid RouteId { get; init; }

    [Required]
    [MaxLength(12)]
    public string FlightNumber { get; init; } = null!;

    [MaxLength(120)]
    public string? AircraftType { get; init; }

    [MaxLength(1000)]
    public string? Description { get; init; }
}

public sealed class UpdateFlightRequest : CreateFlightRequest
{
}

public sealed class ChangeFlightStatusRequest
{
    public CatalogStatus Status { get; init; }
}

public sealed record FlightResponse(
    Guid Id,
    Guid AirlineId,
    Guid RouteId,
    string FlightNumber,
    string? AircraftType,
    string? Description,
    CatalogStatus Status,
    DateTime CreatedAtUtc,
    DateTime UpdatedAtUtc,
    IReadOnlyList<FlightScheduleResponse> Schedules,
    IReadOnlyList<FlightClassResponse> Classes,
    IReadOnlyList<FlightPolicyResponse> Policies);
