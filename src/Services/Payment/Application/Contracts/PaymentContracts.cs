using Payment.Api.Domain.Enums;

namespace Payment.Api.Application.Contracts;

public sealed record AuthorizePaymentCommand(
    Guid BookingId,
    Guid UserId,
    decimal Amount,
    string Currency,
    string PaymentMethodToken,
    string IdempotencyKey);

public sealed record PaymentResponse(
    Guid Id,
    Guid BookingId,
    Guid UserId,
    decimal Amount,
    string Currency,
    PaymentStatus Status,
    string? FailureReason,
    DateTime CreatedAtUtc,
    DateTime UpdatedAtUtc,
    DateTime? AuthorizedAtUtc,
    DateTime? RefundedAtUtc);

public sealed record RefundPaymentCommand(
    Guid PaymentId,
    string? Reason);

public sealed record RefundResponse(
    Guid Id,
    Guid PaymentId,
    Guid BookingId,
    Guid UserId,
    decimal Amount,
    string Currency,
    RefundStatus Status,
    string? Reason,
    DateTime CreatedAtUtc,
    DateTime? CompletedAtUtc);
