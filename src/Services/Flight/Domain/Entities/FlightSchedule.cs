using Flight.Api.Domain.Common;

namespace Flight.Api.Domain.Entities;

public sealed class FlightSchedule : Entity<Guid>
{
    private FlightSchedule()
    {
    }

    private FlightSchedule(
        Guid id,
        Guid flightId,
        TimeSpan departureTime,
        TimeSpan arrivalTime,
        string operatingDays,
        DateOnly effectiveFrom,
        DateOnly? effectiveTo,
        string timeZoneId,
        DateTime createdAtUtc)
    {
        Id = id;
        FlightId = flightId;
        DepartureTime = departureTime;
        ArrivalTime = arrivalTime;
        OperatingDays = operatingDays;
        EffectiveFrom = effectiveFrom;
        EffectiveTo = effectiveTo;
        TimeZoneId = timeZoneId;
        CreatedAtUtc = createdAtUtc;
        UpdatedAtUtc = createdAtUtc;
    }

    public Guid FlightId { get; private set; }
    public TimeSpan DepartureTime { get; private set; }
    public TimeSpan ArrivalTime { get; private set; }
    public string OperatingDays { get; private set; } = null!;
    public DateOnly EffectiveFrom { get; private set; }
    public DateOnly? EffectiveTo { get; private set; }
    public string TimeZoneId { get; private set; } = null!;
    public bool IsDeleted { get; private set; }
    public DateTime CreatedAtUtc { get; private set; }
    public DateTime UpdatedAtUtc { get; private set; }

    public static FlightSchedule Create(
        Guid id,
        Guid flightId,
        TimeSpan departureTime,
        TimeSpan arrivalTime,
        string operatingDays,
        DateOnly effectiveFrom,
        DateOnly? effectiveTo,
        string timeZoneId,
        DateTime createdAtUtc)
    {
        Validate(
            id,
            flightId,
            departureTime,
            arrivalTime,
            operatingDays,
            effectiveFrom,
            effectiveTo,
            timeZoneId);

        return new FlightSchedule(
            id,
            flightId,
            departureTime,
            arrivalTime,
            NormalizeDays(operatingDays),
            effectiveFrom,
            effectiveTo,
            timeZoneId.Trim(),
            createdAtUtc);
    }

    public void Update(
        TimeSpan departureTime,
        TimeSpan arrivalTime,
        string operatingDays,
        DateOnly effectiveFrom,
        DateOnly? effectiveTo,
        string timeZoneId,
        DateTime updatedAtUtc)
    {
        Validate(
            Id,
            FlightId,
            departureTime,
            arrivalTime,
            operatingDays,
            effectiveFrom,
            effectiveTo,
            timeZoneId);

        DepartureTime = departureTime;
        ArrivalTime = arrivalTime;
        OperatingDays = NormalizeDays(operatingDays);
        EffectiveFrom = effectiveFrom;
        EffectiveTo = effectiveTo;
        TimeZoneId = timeZoneId.Trim();
        UpdatedAtUtc = updatedAtUtc;
    }

    public void SoftDelete(DateTime updatedAtUtc)
    {
        if (!IsDeleted)
        {
            IsDeleted = true;
            UpdatedAtUtc = updatedAtUtc;
        }
    }

    private static void Validate(
        Guid id,
        Guid flightId,
        TimeSpan departureTime,
        TimeSpan arrivalTime,
        string operatingDays,
        DateOnly effectiveFrom,
        DateOnly? effectiveTo,
        string timeZoneId)
    {
        if (id == Guid.Empty || flightId == Guid.Empty)
        {
            throw new DomainException("Schedule and flight ids are required.");
        }

        if (departureTime < TimeSpan.Zero || departureTime >= TimeSpan.FromDays(1)
            || arrivalTime < TimeSpan.Zero || arrivalTime >= TimeSpan.FromDays(1))
        {
            throw new DomainException("Schedule times must be valid times of day.");
        }

        _ = NormalizeDays(operatingDays);
        if (effectiveFrom == default)
        {
            throw new DomainException("Schedule effective-from date is required.");
        }

        if (effectiveTo.HasValue && effectiveTo.Value < effectiveFrom)
        {
            throw new DomainException("Schedule effective-to date cannot be before effective-from date.");
        }

        if (string.IsNullOrWhiteSpace(timeZoneId))
        {
            throw new DomainException("Schedule time zone is required.");
        }
    }

    private static string NormalizeDays(string value)
    {
        var days = value?
            .Split(',', StringSplitOptions.RemoveEmptyEntries | StringSplitOptions.TrimEntries)
            .Select(day => day.ToUpperInvariant())
            .Distinct()
            .ToArray();

        var allowed = new[] { "MON", "TUE", "WED", "THU", "FRI", "SAT", "SUN" };
        if (days is null || days.Length == 0 || days.Any(day => !allowed.Contains(day)))
        {
            throw new DomainException("Operating days must contain one or more of MON,TUE,WED,THU,FRI,SAT,SUN.");
        }

        return string.Join(",", allowed.Where(days.Contains));
    }
}
