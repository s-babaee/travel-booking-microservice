using System.ComponentModel.DataAnnotations;

namespace Flight.Api.Application.Contracts;

public class CreateFlightScheduleRequest
{
    public TimeSpan DepartureTime { get; init; }
    public TimeSpan ArrivalTime { get; init; }

    [Required]
    [MaxLength(50)]
    public string OperatingDays { get; init; } = null!;

    public DateOnly EffectiveFrom { get; init; }
    public DateOnly? EffectiveTo { get; init; }

    [Required]
    [MaxLength(100)]
    public string TimeZoneId { get; init; } = null!;
}

public sealed class UpdateFlightScheduleRequest : CreateFlightScheduleRequest
{
}

public sealed record FlightScheduleResponse(
    Guid Id,
    Guid FlightId,
    TimeSpan DepartureTime,
    TimeSpan ArrivalTime,
    string OperatingDays,
    DateOnly EffectiveFrom,
    DateOnly? EffectiveTo,
    string TimeZoneId,
    DateTime CreatedAtUtc,
    DateTime UpdatedAtUtc);
