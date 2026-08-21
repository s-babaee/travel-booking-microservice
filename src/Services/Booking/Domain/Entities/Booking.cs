using Booking.Api.Domain.Common;
using Booking.Api.Domain.Enums;

namespace Booking.Api.Domain.Entities;

public sealed class Booking
{
    private readonly List<BookingItem> _items = [];

    private Booking()
    {
    }

    private Booking(
        Guid id,
        Guid userId,
        BookingType type,
        decimal totalAmount,
        string currency,
        string? idempotencyKey,
        DateTime nowUtc)
    {
        Id = id;
        UserId = userId;
        Type = type;
        TotalAmount = totalAmount;
        Currency = currency;
        IdempotencyKey = idempotencyKey;
        Status = BookingStatus.PendingInventory;
        CreatedAtUtc = nowUtc;
        UpdatedAtUtc = nowUtc;
    }

    public Guid Id { get; private set; }
    public Guid UserId { get; private set; }
    public BookingType Type { get; private set; }
    public BookingStatus Status { get; private set; }
    public decimal TotalAmount { get; private set; }
    public string Currency { get; private set; } = null!;
    public string? IdempotencyKey { get; private set; }
    public Guid? InventoryHoldId { get; private set; }
    public Guid? PaymentTransactionId { get; private set; }
    public Guid? OrderId { get; private set; }
    public Guid? HotelId { get; private set; }
    public DateOnly? CheckIn { get; private set; }
    public DateOnly? CheckOut { get; private set; }
    public Guid? FlightId { get; private set; }
    public DateOnly? FlightDate { get; private set; }
    public string? PassengerName { get; private set; }
    public string? FailureReason { get; private set; }
    public DateTime CreatedAtUtc { get; private set; }
    public DateTime UpdatedAtUtc { get; private set; }
    public DateTime? ConfirmedAtUtc { get; private set; }
    public DateTime? CancelledAtUtc { get; private set; }
    public IReadOnlyCollection<BookingItem> Items => _items;

    public static Booking CreateHotel(
        Guid id,
        Guid userId,
        Guid hotelId,
        DateOnly checkIn,
        DateOnly checkOut,
        decimal totalAmount,
        string currency,
        string? idempotencyKey,
        string? passengerName,
        IEnumerable<BookingItem> items,
        DateTime nowUtc)
    {
        if (hotelId == Guid.Empty || userId == Guid.Empty)
        {
            throw new DomainException("User and hotel ids are required.");
        }

        if (checkOut <= checkIn)
        {
            throw new DomainException("Check-out must be after check-in.");
        }

        var booking = new Booking(
            id,
            userId,
            BookingType.Hotel,
            totalAmount,
            currency,
            idempotencyKey,
            nowUtc)
        {
            HotelId = hotelId,
            CheckIn = checkIn,
            CheckOut = checkOut,
            PassengerName = passengerName
        };
        booking.AddItems(items);
        return booking;
    }

    public static Booking CreateFlight(
        Guid id,
        Guid userId,
        Guid flightId,
        DateOnly date,
        decimal totalAmount,
        string currency,
        string? idempotencyKey,
        string? passengerName,
        IEnumerable<BookingItem> items,
        DateTime nowUtc)
    {
        if (flightId == Guid.Empty || userId == Guid.Empty)
        {
            throw new DomainException("User and flight ids are required.");
        }

        var booking = new Booking(
            id,
            userId,
            BookingType.Flight,
            totalAmount,
            currency,
            idempotencyKey,
            nowUtc)
        {
            FlightId = flightId,
            FlightDate = date,
            PassengerName = passengerName
        };
        booking.AddItems(items);
        return booking;
    }

    public void MarkInventoryHeld(Guid holdId, DateTime nowUtc)
    {
        EnsureStatus(BookingStatus.PendingInventory);
        if (holdId == Guid.Empty)
        {
            throw new DomainException("Inventory hold id is required.");
        }

        InventoryHoldId = holdId;
        Status = BookingStatus.PendingPayment;
        Touch(nowUtc);
    }

    public void MarkPaymentAuthorized(Guid transactionId, DateTime nowUtc)
    {
        EnsureStatus(BookingStatus.PendingPayment);
        if (transactionId == Guid.Empty)
        {
            throw new DomainException("Payment transaction id is required.");
        }

        PaymentTransactionId = transactionId;
        Status = BookingStatus.ConfirmingInventory;
        Touch(nowUtc);
    }

    public void Confirm(Guid orderId, DateTime nowUtc)
    {
        EnsureStatus(BookingStatus.ConfirmingInventory);
        if (orderId == Guid.Empty)
        {
            throw new DomainException("Order id is required.");
        }

        OrderId = orderId;
        Status = BookingStatus.Confirmed;
        ConfirmedAtUtc = nowUtc;
        Touch(nowUtc);
    }

    public void StartCancellation(DateTime nowUtc)
    {
        if (Status is not (BookingStatus.PendingInventory
            or BookingStatus.PendingPayment
            or BookingStatus.ConfirmingInventory
            or BookingStatus.Confirmed))
        {
            throw new DomainException(
                $"Booking cannot be cancelled from status '{Status}'.");
        }

        Status = BookingStatus.Cancelling;
        Touch(nowUtc);
    }

    public void Cancel(DateTime nowUtc)
    {
        if (Status != BookingStatus.Cancelling)
        {
            throw new DomainException(
                $"Booking cannot be completed as cancelled from status '{Status}'.");
        }

        Status = BookingStatus.Cancelled;
        CancelledAtUtc = nowUtc;
        Touch(nowUtc);
    }

    public void Fail(string reason, DateTime nowUtc)
    {
        if (Status is BookingStatus.Confirmed or BookingStatus.Cancelled)
        {
            throw new DomainException(
                $"Booking cannot be failed from status '{Status}'.");
        }

        Status = BookingStatus.Failed;
        FailureReason = string.IsNullOrWhiteSpace(reason)
            ? "The booking saga failed."
            : reason;
        Touch(nowUtc);
    }

    public void AdminChangeStatus(BookingStatus status, string? reason, DateTime nowUtc)
    {
        if (status == BookingStatus.Confirmed && OrderId is null)
        {
            throw new DomainException(
                "A booking can only be marked confirmed when it has an order.");
        }

        Status = status;
        FailureReason = reason;
        if (status == BookingStatus.Cancelled)
        {
            CancelledAtUtc ??= nowUtc;
        }

        Touch(nowUtc);
    }

    private void AddItems(IEnumerable<BookingItem> items)
    {
        var normalized = items?.ToArray()
            ?? throw new DomainException("At least one booking item is required.");
        if (normalized.Length == 0)
        {
            throw new DomainException("At least one booking item is required.");
        }

        _items.AddRange(normalized);
    }

    private void EnsureStatus(BookingStatus expected)
    {
        if (Status != expected)
        {
            throw new DomainException(
                $"Booking transition requires '{expected}', current status is '{Status}'.");
        }
    }

    private void Touch(DateTime nowUtc) => UpdatedAtUtc = nowUtc;
}
