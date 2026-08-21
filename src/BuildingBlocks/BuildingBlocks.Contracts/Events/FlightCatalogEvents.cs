namespace BuildingBlocks.Contracts.Events;

public sealed record FlightCreated(
    Guid FlightId,
    Guid AirlineId,
    Guid RouteId,
    string FlightNumber,
    DateTime OccurredAtUtc);

public sealed record FlightUpdated(
    Guid FlightId,
    Guid AirlineId,
    Guid RouteId,
    string FlightNumber,
    DateTime OccurredAtUtc);

public sealed record FlightStatusChanged(
    Guid FlightId,
    string Status,
    DateTime OccurredAtUtc);

public sealed record FlightDeleted(Guid FlightId, DateTime OccurredAtUtc);

public sealed record RouteCreated(
    Guid RouteId,
    string OriginAirportCode,
    string DestinationAirportCode,
    DateTime OccurredAtUtc);

public sealed record RouteUpdated(
    Guid RouteId,
    string OriginAirportCode,
    string DestinationAirportCode,
    DateTime OccurredAtUtc);

public sealed record RouteDeleted(Guid RouteId, DateTime OccurredAtUtc);

public sealed record FlightScheduleCreated(
    Guid ScheduleId,
    Guid FlightId,
    TimeSpan DepartureTime,
    TimeSpan ArrivalTime,
    DateTime OccurredAtUtc);

public sealed record FlightScheduleUpdated(
    Guid ScheduleId,
    Guid FlightId,
    TimeSpan DepartureTime,
    TimeSpan ArrivalTime,
    DateTime OccurredAtUtc);

public sealed record FlightScheduleDeleted(
    Guid ScheduleId,
    Guid FlightId,
    DateTime OccurredAtUtc);

public sealed record FlightClassCreated(
    Guid ClassId,
    Guid FlightId,
    string Code,
    string Name,
    int Capacity,
    DateTime OccurredAtUtc);

public sealed record FlightClassUpdated(
    Guid ClassId,
    Guid FlightId,
    string Code,
    string Name,
    int Capacity,
    DateTime OccurredAtUtc);

public sealed record FlightClassStatusChanged(
    Guid ClassId,
    Guid FlightId,
    string Status,
    DateTime OccurredAtUtc);

public sealed record FlightClassDeleted(
    Guid ClassId,
    Guid FlightId,
    DateTime OccurredAtUtc);

public sealed record FlightPolicyCreated(
    Guid PolicyId,
    Guid FlightId,
    string PolicyType,
    DateTime OccurredAtUtc);

public sealed record FlightPolicyUpdated(
    Guid PolicyId,
    Guid FlightId,
    string PolicyType,
    DateTime OccurredAtUtc);

public sealed record FlightPolicyDeleted(
    Guid PolicyId,
    Guid FlightId,
    DateTime OccurredAtUtc);

public sealed record AirlineCreated(
    Guid AirlineId,
    string Name,
    string IataCode,
    string IcaoCode,
    DateTime OccurredAtUtc);

public sealed record AirlineUpdated(
    Guid AirlineId,
    string Name,
    string IataCode,
    string IcaoCode,
    DateTime OccurredAtUtc);

public sealed record AirlineDeleted(Guid AirlineId, DateTime OccurredAtUtc);

public sealed record FlightAvailabilityChanged(
    Guid FlightId,
    Guid FlightClassId,
    DateOnly Date,
    int TotalSeats,
    int AvailableSeats,
    int HeldSeats,
    int ConfirmedSeats,
    DateTime OccurredAtUtc);
