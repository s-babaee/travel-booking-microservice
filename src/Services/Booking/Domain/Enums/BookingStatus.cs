namespace Booking.Api.Domain.Enums;

public enum BookingStatus
{
    PendingInventory = 1,
    PendingPayment = 2,
    ConfirmingInventory = 3,
    Confirmed = 4,
    Cancelling = 5,
    Cancelled = 6,
    Failed = 7
}
