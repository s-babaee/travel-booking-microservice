namespace Booking.Api.Domain.Entities;

public sealed class Order
{
    private Order()
    {
    }

    private Order(
        Guid id,
        Guid bookingId,
        Guid userId,
        decimal totalAmount,
        string currency,
        DateTime createdAtUtc)
    {
        Id = id;
        BookingId = bookingId;
        UserId = userId;
        TotalAmount = totalAmount;
        Currency = currency;
        CreatedAtUtc = createdAtUtc;
    }

    public Guid Id { get; private set; }
    public Guid BookingId { get; private set; }
    public Guid UserId { get; private set; }
    public decimal TotalAmount { get; private set; }
    public string Currency { get; private set; } = null!;
    public DateTime CreatedAtUtc { get; private set; }

    public static Order Create(
        Guid id,
        Booking booking,
        DateTime nowUtc)
    {
        ArgumentNullException.ThrowIfNull(booking);
        return new Order(
            id,
            booking.Id,
            booking.UserId,
            booking.TotalAmount,
            booking.Currency,
            nowUtc);
    }
}
