using Flight.Api.Domain.Common;

namespace Flight.Api.Domain.Entities;

public sealed class Route : Entity<Guid>
{
    private Route()
    {
    }

    private Route(
        Guid id,
        string originAirportCode,
        string destinationAirportCode,
        string originCity,
        string destinationCity,
        int distanceKm,
        int typicalDurationMinutes,
        DateTime createdAtUtc)
    {
        Id = id;
        OriginAirportCode = originAirportCode;
        DestinationAirportCode = destinationAirportCode;
        OriginCity = originCity;
        DestinationCity = destinationCity;
        DistanceKm = distanceKm;
        TypicalDurationMinutes = typicalDurationMinutes;
        CreatedAtUtc = createdAtUtc;
        UpdatedAtUtc = createdAtUtc;
    }

    public string OriginAirportCode { get; private set; } = null!;
    public string DestinationAirportCode { get; private set; } = null!;
    public string OriginCity { get; private set; } = null!;
    public string DestinationCity { get; private set; } = null!;
    public int DistanceKm { get; private set; }
    public int TypicalDurationMinutes { get; private set; }
    public bool IsDeleted { get; private set; }
    public DateTime CreatedAtUtc { get; private set; }
    public DateTime UpdatedAtUtc { get; private set; }

    public static Route Create(
        Guid id,
        string originAirportCode,
        string destinationAirportCode,
        string originCity,
        string destinationCity,
        int distanceKm,
        int typicalDurationMinutes,
        DateTime createdAtUtc)
    {
        Validate(
            id,
            originAirportCode,
            destinationAirportCode,
            originCity,
            destinationCity,
            distanceKm,
            typicalDurationMinutes);

        return new Route(
            id,
            NormalizeAirportCode(originAirportCode),
            NormalizeAirportCode(destinationAirportCode),
            originCity.Trim(),
            destinationCity.Trim(),
            distanceKm,
            typicalDurationMinutes,
            createdAtUtc);
    }

    public void Update(
        string originAirportCode,
        string destinationAirportCode,
        string originCity,
        string destinationCity,
        int distanceKm,
        int typicalDurationMinutes,
        DateTime updatedAtUtc)
    {
        Validate(
            Id,
            originAirportCode,
            destinationAirportCode,
            originCity,
            destinationCity,
            distanceKm,
            typicalDurationMinutes);

        OriginAirportCode = NormalizeAirportCode(originAirportCode);
        DestinationAirportCode = NormalizeAirportCode(destinationAirportCode);
        OriginCity = originCity.Trim();
        DestinationCity = destinationCity.Trim();
        DistanceKm = distanceKm;
        TypicalDurationMinutes = typicalDurationMinutes;
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
        string originAirportCode,
        string destinationAirportCode,
        string originCity,
        string destinationCity,
        int distanceKm,
        int typicalDurationMinutes)
    {
        if (id == Guid.Empty)
        {
            throw new DomainException("Route id is required.");
        }

        var origin = NormalizeAirportCode(originAirportCode);
        var destination = NormalizeAirportCode(destinationAirportCode);
        if (origin == destination)
        {
            throw new DomainException("Route origin and destination must be different.");
        }

        if (string.IsNullOrWhiteSpace(originCity) || string.IsNullOrWhiteSpace(destinationCity))
        {
            throw new DomainException("Route cities are required.");
        }

        if (distanceKm <= 0 || typicalDurationMinutes <= 0)
        {
            throw new DomainException("Route distance and duration must be greater than zero.");
        }
    }

    private static string NormalizeAirportCode(string value)
    {
        var code = value?.Trim().ToUpperInvariant();
        if (code is null || code.Length != 3 || code.Any(character => !char.IsLetter(character)))
        {
            throw new DomainException("Airport code must contain exactly three letters.");
        }

        return code;
    }
}
