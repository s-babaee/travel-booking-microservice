namespace BuildingBlocks.Contracts.Events;

public sealed record PaymentFailedEvent(
    Guid BookingId,
    Guid UserId,
    string? Reason,
    DateTime OccurredAtUtc);

public sealed record PaymentRefundedEvent(
    Guid PaymentId,
    Guid BookingId,
    Guid UserId,
    decimal Amount,
    string Currency,
    DateTime OccurredAtUtc);
