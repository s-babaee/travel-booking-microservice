namespace BuildingBlocks.Contracts.Events;

public sealed record BookingConfirmedEvent(
    Guid BookingId,
    Guid UserId,
    string BookingType,
    decimal Amount,
    string Currency,
    DateTime OccurredAtUtc);

public sealed record BookingFailedEvent(
    Guid BookingId,
    Guid UserId,
    string Reason,
    DateTime OccurredAtUtc);

public sealed record BookingCancellationStartedEvent(
    Guid BookingId,
    Guid UserId,
    string? Reason,
    DateTime OccurredAtUtc);

public sealed record BookingCancelledEvent(
    Guid BookingId,
    Guid UserId,
    DateTime OccurredAtUtc);
