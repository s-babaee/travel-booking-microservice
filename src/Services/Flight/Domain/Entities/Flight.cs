using Flight.Api.Domain.Common;
using Flight.Api.Domain.Enums;

namespace Flight.Api.Domain.Entities;

public sealed class Flight : Entity<Guid>
{
    private Flight()
    {
    }

    private Flight(
        Guid id,
        Guid airlineId,
        Guid routeId,
        string flightNumber,
        string? aircraftType,
        string? description,
        DateTime createdAtUtc)
    {
        Id = id;
        AirlineId = airlineId;
        RouteId = routeId;
        FlightNumber = flightNumber;
        AircraftType = aircraftType;
        Description = description;
        Status = CatalogStatus.Active;
        CreatedAtUtc = createdAtUtc;
        UpdatedAtUtc = createdAtUtc;
    }

    public Guid AirlineId { get; private set; }
    public Guid RouteId { get; private set; }
    public string FlightNumber { get; private set; } = null!;
    public string? AircraftType { get; private set; }
    public string? Description { get; private set; }
    public CatalogStatus Status { get; private set; }
    public bool IsDeleted { get; private set; }
    public DateTime CreatedAtUtc { get; private set; }
    public DateTime UpdatedAtUtc { get; private set; }

    public static Flight Create(
        Guid id,
        Guid airlineId,
        Guid routeId,
        string flightNumber,
        string? aircraftType,
        string? description,
        DateTime createdAtUtc)
    {
        Validate(id, airlineId, routeId, flightNumber);
        return new Flight(
            id,
            airlineId,
            routeId,
            NormalizeFlightNumber(flightNumber),
            NormalizeOptional(aircraftType),
            NormalizeOptional(description),
            createdAtUtc);
    }

    public void Update(
        Guid airlineId,
        Guid routeId,
        string flightNumber,
        string? aircraftType,
        string? description,
        DateTime updatedAtUtc)
    {
        Validate(Id, airlineId, routeId, flightNumber);
        AirlineId = airlineId;
        RouteId = routeId;
        FlightNumber = NormalizeFlightNumber(flightNumber);
        AircraftType = NormalizeOptional(aircraftType);
        Description = NormalizeOptional(description);
        UpdatedAtUtc = updatedAtUtc;
    }

    public void ChangeStatus(CatalogStatus status, DateTime updatedAtUtc)
    {
        if (IsDeleted)
        {
            throw new DomainException("A deleted flight cannot change status.");
        }

        Status = status;
        UpdatedAtUtc = updatedAtUtc;
    }

    public void SoftDelete(DateTime updatedAtUtc)
    {
        if (!IsDeleted)
        {
            IsDeleted = true;
            Status = CatalogStatus.Inactive;
            UpdatedAtUtc = updatedAtUtc;
        }
    }

    private static void Validate(Guid id, Guid airlineId, Guid routeId, string flightNumber)
    {
        if (id == Guid.Empty || airlineId == Guid.Empty || routeId == Guid.Empty)
        {
            throw new DomainException("Flight, airline and route ids are required.");
        }

        _ = NormalizeFlightNumber(flightNumber);
    }

    private static string NormalizeFlightNumber(string value)
    {
        var number = value?.Trim().ToUpperInvariant();
        if (number is null || number.Length is < 2 or > 12 || number.Any(char.IsWhiteSpace))
        {
            throw new DomainException("Flight number must be between 2 and 12 non-whitespace characters.");
        }

        return number;
    }

    private static string? NormalizeOptional(string? value)
    {
        return string.IsNullOrWhiteSpace(value) ? null : value.Trim();
    }
}
