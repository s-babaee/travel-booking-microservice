namespace BuildingBlocks.Contracts.Integrations;

public sealed record AuthorizePaymentRequest(
    Guid BookingId,
    Guid UserId,
    decimal Amount,
    string Currency,
    string PaymentMethodToken,
    string IdempotencyKey);

public sealed record AuthorizePaymentResponse(
    bool Succeeded,
    Guid? TransactionId,
    string? FailureReason);

public sealed record VoidPaymentRequest(string Reason);

public sealed record PaymentOperationResponse(
    bool Succeeded,
    Guid TransactionId,
    string? FailureReason);

public sealed record PaymentTransactionResponse(
    Guid Id,
    Guid BookingId,
    Guid UserId,
    decimal Amount,
    string Currency,
    string Status,
    string? FailureReason,
    DateTime CreatedAtUtc,
    DateTime UpdatedAtUtc,
    DateTime? AuthorizedAtUtc,
    DateTime? RefundedAtUtc);

public sealed record RefundPaymentRequest(string? Reason);

public sealed record RefundResponse(
    Guid Id,
    Guid PaymentId,
    Guid BookingId,
    Guid UserId,
    decimal Amount,
    string Currency,
    string Status,
    string? Reason,
    DateTime CreatedAtUtc,
    DateTime? CompletedAtUtc);
