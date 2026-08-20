namespace BuildingBlocks.Contracts.Events;

public sealed record BookingCreatedEvent(
    Guid BookingId,
    string PassengerName,
    string TripType,
    DateTime CreatedAtUtc);
