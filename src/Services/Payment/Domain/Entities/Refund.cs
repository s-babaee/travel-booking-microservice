using Payment.Api.Domain.Enums;

namespace Payment.Api.Domain.Entities;

public sealed class Refund
{
    private Refund()
    {
    }

    private Refund(
        Guid id,
        Guid paymentId,
        Guid bookingId,
        Guid userId,
        decimal amount,
        string currency,
        string? reason,
        DateTime nowUtc)
    {
        Id = id;
        PaymentId = paymentId;
        BookingId = bookingId;
        UserId = userId;
        Amount = amount;
        Currency = currency;
        Reason = reason;
        Status = RefundStatus.Completed;
        CreatedAtUtc = nowUtc;
        CompletedAtUtc = nowUtc;
    }

    public Guid Id { get; private set; }
    public Guid PaymentId { get; private set; }
    public Guid BookingId { get; private set; }
    public Guid UserId { get; private set; }
    public decimal Amount { get; private set; }
    public string Currency { get; private set; } = null!;
    public RefundStatus Status { get; private set; }
    public string? Reason { get; private set; }
    public DateTime CreatedAtUtc { get; private set; }
    public DateTime? CompletedAtUtc { get; private set; }

    public static Refund Complete(
        Guid id,
        PaymentTransaction payment,
        string? reason,
        DateTime nowUtc) =>
        new(
            id,
            payment.Id,
            payment.BookingId,
            payment.UserId,
            payment.Amount,
            payment.Currency,
            reason,
            nowUtc);
}
