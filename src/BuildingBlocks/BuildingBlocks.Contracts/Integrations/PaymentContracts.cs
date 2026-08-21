namespace BuildingBlocks.Contracts.Integrations;

public sealed record AuthorizePaymentRequest(
    Guid BookingId,
    Guid UserId,
    decimal Amount,
    string Currency,
    string PaymentMethodToken);

public sealed record AuthorizePaymentResponse(
    bool Succeeded,
    Guid? TransactionId,
    string? FailureReason);

public sealed record VoidPaymentRequest(string Reason);

public sealed record PaymentOperationResponse(
    bool Succeeded,
    Guid TransactionId,
    string? FailureReason);
