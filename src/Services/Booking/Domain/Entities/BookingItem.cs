namespace Booking.Api.Domain.Entities;

public sealed class BookingItem
{
    private BookingItem()
    {
    }

    private BookingItem(
        Guid resourceTypeId,
        int quantity,
        decimal unitAmount)
    {
        ResourceTypeId = resourceTypeId;
        Quantity = quantity;
        UnitAmount = unitAmount;
    }

    public Guid BookingId { get; private set; }
    public Guid ResourceTypeId { get; private set; }
    public int Quantity { get; private set; }
    public decimal UnitAmount { get; private set; }

    public static BookingItem Create(
        Guid resourceTypeId,
        int quantity,
        decimal unitAmount)
    {
        if (resourceTypeId == Guid.Empty)
        {
            throw new ArgumentException(
                "Resource type id is required.",
                nameof(resourceTypeId));
        }

        if (quantity <= 0)
        {
            throw new ArgumentException(
                "Quantity must be greater than zero.",
                nameof(quantity));
        }

        if (unitAmount < 0)
        {
            throw new ArgumentException(
                "Unit amount cannot be negative.",
                nameof(unitAmount));
        }

        return new BookingItem(resourceTypeId, quantity, unitAmount);
    }
}
