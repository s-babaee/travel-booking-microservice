using Inventory.Api.Domain.Common;

namespace Inventory.Api.Domain.Entities;

public sealed class HotelInventoryDay
{
    private HotelInventoryDay()
    {
    }

    private HotelInventoryDay(
        Guid hotelId,
        Guid roomTypeId,
        DateOnly date,
        int totalUnits,
        DateTime updatedAtUtc)
    {
        HotelId = hotelId;
        RoomTypeId = roomTypeId;
        Date = date;
        TotalUnits = totalUnits;
        AvailableUnits = totalUnits;
        UpdatedAtUtc = updatedAtUtc;
    }

    public Guid HotelId { get; private set; }
    public Guid RoomTypeId { get; private set; }
    public DateOnly Date { get; private set; }
    public int TotalUnits { get; private set; }
    public int AvailableUnits { get; private set; }
    public int HeldUnits { get; private set; }
    public int ConfirmedUnits { get; private set; }
    public DateTime UpdatedAtUtc { get; private set; }

    public static HotelInventoryDay Create(
        Guid hotelId,
        Guid roomTypeId,
        DateOnly date,
        int totalUnits,
        DateTime nowUtc)
    {
        ValidateIds(hotelId, roomTypeId);
        ValidateTotal(totalUnits);
        return new HotelInventoryDay(
            hotelId,
            roomTypeId,
            date,
            totalUnits,
            nowUtc);
    }

    public void AdjustTo(int totalUnits, DateTime nowUtc)
    {
        ValidateTotal(totalUnits);
        if (totalUnits < HeldUnits + ConfirmedUnits)
        {
            throw new DomainException(
                "Inventory total cannot be lower than held and confirmed units.");
        }

        TotalUnits = totalUnits;
        AvailableUnits = totalUnits - HeldUnits - ConfirmedUnits;
        UpdatedAtUtc = nowUtc;
    }

    public void Hold(int quantity, DateTime nowUtc)
    {
        ValidateQuantity(quantity);
        if (AvailableUnits < quantity)
        {
            throw new DomainException(
                $"Hotel inventory is not available for {quantity} unit(s) on {Date:yyyy-MM-dd}.");
        }

        AvailableUnits -= quantity;
        HeldUnits += quantity;
        UpdatedAtUtc = nowUtc;
    }

    public void ConfirmHold(int quantity, DateTime nowUtc)
    {
        ValidateQuantity(quantity);
        if (HeldUnits < quantity)
        {
            throw new DomainException("The requested held hotel inventory is not available.");
        }

        HeldUnits -= quantity;
        ConfirmedUnits += quantity;
        UpdatedAtUtc = nowUtc;
    }

    public void ReleaseHold(int quantity, DateTime nowUtc)
    {
        ValidateQuantity(quantity);
        if (HeldUnits < quantity)
        {
            throw new DomainException("The requested held hotel inventory is not available.");
        }

        HeldUnits -= quantity;
        AvailableUnits += quantity;
        UpdatedAtUtc = nowUtc;
    }

    private static void ValidateIds(Guid hotelId, Guid roomTypeId)
    {
        if (hotelId == Guid.Empty || roomTypeId == Guid.Empty)
        {
            throw new DomainException("Hotel and room type ids are required.");
        }
    }

    private static void ValidateTotal(int totalUnits)
    {
        if (totalUnits < 0)
        {
            throw new DomainException("Inventory total cannot be negative.");
        }
    }

    private static void ValidateQuantity(int quantity)
    {
        if (quantity <= 0)
        {
            throw new DomainException("Inventory quantity must be greater than zero.");
        }
    }
}
