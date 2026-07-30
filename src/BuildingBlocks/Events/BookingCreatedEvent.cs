namespace BuildingBlocks.Events
{
    public record BookingCreatedEvent(
        Guid BookingId,
        string PassengerName,
        string TripType,
        DateTime CreatedAtUtc
    );
}
