using Payment.Api.Domain.Common;
using Payment.Api.Domain.Enums;

namespace Payment.Api.Domain.Entities;

public sealed class PaymentTransaction
{
    private PaymentTransaction()
    {
    }

    private PaymentTransaction(
        Guid id,
        Guid bookingId,
        Guid userId,
        decimal amount,
        string currency,
        string idempotencyKey,
        DateTime nowUtc)
    {
        Id = id;
        BookingId = bookingId;
        UserId = userId;
        Amount = amount;
        Currency = currency;
        IdempotencyKey = idempotencyKey;
        Status = PaymentStatus.Authorized;
        CreatedAtUtc = nowUtc;
        UpdatedAtUtc = nowUtc;
        AuthorizedAtUtc = nowUtc;
    }

    public Guid Id { get; private set; }
    public Guid BookingId { get; private set; }
    public Guid UserId { get; private set; }
    public decimal Amount { get; private set; }
    public string Currency { get; private set; } = null!;
    public string IdempotencyKey { get; private set; } = null!;
    public PaymentStatus Status { get; private set; }
    public string? FailureReason { get; private set; }
    public DateTime CreatedAtUtc { get; private set; }
    public DateTime UpdatedAtUtc { get; private set; }
    public DateTime? AuthorizedAtUtc { get; private set; }
    public DateTime? RefundedAtUtc { get; private set; }

    public static PaymentTransaction Authorize(
        Guid id,
        Guid bookingId,
        Guid userId,
        decimal amount,
        string currency,
        string idempotencyKey,
        DateTime nowUtc)
    {
        if (id == Guid.Empty
            || bookingId == Guid.Empty
            || userId == Guid.Empty)
        {
            throw new DomainException(
                "Payment, booking and user ids are required.");
        }

        if (amount < 0)
        {
            throw new DomainException("Payment amount cannot be negative.");
        }

        if (string.IsNullOrWhiteSpace(idempotencyKey))
        {
            throw new DomainException("Payment idempotency key is required.");
        }

        return new PaymentTransaction(
            id,
            bookingId,
            userId,
            amount,
            currency,
            idempotencyKey,
            nowUtc);
    }

    public static PaymentTransaction Failed(
        Guid id,
        Guid bookingId,
        Guid userId,
        decimal amount,
        string currency,
        string idempotencyKey,
        string reason,
        DateTime nowUtc)
    {
        var payment = Authorize(
            id,
            bookingId,
            userId,
            amount,
            currency,
            idempotencyKey,
            nowUtc);
        payment.Status = PaymentStatus.Failed;
        payment.FailureReason = reason;
        payment.AuthorizedAtUtc = null;
        return payment;
    }

    public void Void(DateTime nowUtc)
    {
        if (Status is PaymentStatus.Voided or PaymentStatus.Refunded)
        {
            return;
        }

        if (Status != PaymentStatus.Authorized)
        {
            throw new DomainException(
                $"Payment cannot be voided from status '{Status}'.");
        }

        Status = PaymentStatus.Voided;
        Touch(nowUtc);
    }

    public void MarkRefunded(DateTime nowUtc)
    {
        if (Status == PaymentStatus.Refunded)
        {
            return;
        }

        if (Status != PaymentStatus.Authorized)
        {
            throw new DomainException(
                $"Payment cannot be refunded from status '{Status}'.");
        }

        Status = PaymentStatus.Refunded;
        RefundedAtUtc = nowUtc;
        Touch(nowUtc);
    }

    private void Touch(DateTime nowUtc) => UpdatedAtUtc = nowUtc;
}
