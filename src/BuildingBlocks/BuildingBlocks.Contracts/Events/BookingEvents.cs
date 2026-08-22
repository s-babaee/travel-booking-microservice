namespace BuildingBlocks.Contracts.Events;

public sealed record BookingStatusChangedEvent(
    Guid BookingId,
    Guid UserId,
    string Status,
    DateTime OccurredAtUtc,
    string? FailureReason = null);

public sealed record PaymentAuthorizationRequested(
    Guid BookingId,
    Guid UserId,
    decimal Amount,
    string Currency,
    string PaymentMethodToken,
    DateTime OccurredAtUtc);

public sealed record PaymentAuthorized(
    Guid BookingId,
    Guid UserId,
    Guid TransactionId,
    decimal Amount,
    string Currency,
    DateTime OccurredAtUtc);

public sealed record PaymentAuthorizationFailed(
    Guid BookingId,
    string Reason,
    DateTime OccurredAtUtc);
