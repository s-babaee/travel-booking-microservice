using Inventory.Api.Domain.Common;

namespace Inventory.Api.Domain.Entities;

public sealed class FlightInventoryDay
{
    private FlightInventoryDay()
    {
    }

    private FlightInventoryDay(
        Guid flightId,
        Guid flightClassId,
        DateOnly date,
        int totalSeats,
        DateTime updatedAtUtc)
    {
        FlightId = flightId;
        FlightClassId = flightClassId;
        Date = date;
        TotalSeats = totalSeats;
        AvailableSeats = totalSeats;
        UpdatedAtUtc = updatedAtUtc;
    }

    public Guid FlightId { get; private set; }
    public Guid FlightClassId { get; private set; }
    public DateOnly Date { get; private set; }
    public int TotalSeats { get; private set; }
    public int AvailableSeats { get; private set; }
    public int HeldSeats { get; private set; }
    public int ConfirmedSeats { get; private set; }
    public DateTime UpdatedAtUtc { get; private set; }

    public static FlightInventoryDay Create(
        Guid flightId,
        Guid flightClassId,
        DateOnly date,
        int totalSeats,
        DateTime nowUtc)
    {
        if (flightId == Guid.Empty || flightClassId == Guid.Empty)
        {
            throw new DomainException("Flight and flight class ids are required.");
        }

        if (totalSeats < 0)
        {
            throw new DomainException("Flight inventory total cannot be negative.");
        }

        return new FlightInventoryDay(
            flightId,
            flightClassId,
            date,
            totalSeats,
            nowUtc);
    }

    public void AdjustTo(int totalSeats, DateTime nowUtc)
    {
        if (totalSeats < HeldSeats + ConfirmedSeats)
        {
            throw new DomainException(
                "Flight inventory total cannot be lower than held and confirmed seats.");
        }

        if (totalSeats < 0)
        {
            throw new DomainException("Flight inventory total cannot be negative.");
        }

        TotalSeats = totalSeats;
        AvailableSeats = totalSeats - HeldSeats - ConfirmedSeats;
        UpdatedAtUtc = nowUtc;
    }

    public void Hold(int quantity, DateTime nowUtc)
    {
        ValidateQuantity(quantity);
        if (AvailableSeats < quantity)
        {
            throw new DomainException(
                $"Flight inventory is not available for {quantity} seat(s) on {Date:yyyy-MM-dd}.");
        }

        AvailableSeats -= quantity;
        HeldSeats += quantity;
        UpdatedAtUtc = nowUtc;
    }

    public void ConfirmHold(int quantity, DateTime nowUtc)
    {
        ValidateQuantity(quantity);
        if (HeldSeats < quantity)
        {
            throw new DomainException("The requested held flight inventory is not available.");
        }

        HeldSeats -= quantity;
        ConfirmedSeats += quantity;
        UpdatedAtUtc = nowUtc;
    }

    public void ReleaseHold(int quantity, DateTime nowUtc)
    {
        ValidateQuantity(quantity);
        if (HeldSeats < quantity)
        {
            throw new DomainException("The requested held flight inventory is not available.");
        }

        HeldSeats -= quantity;
        AvailableSeats += quantity;
        UpdatedAtUtc = nowUtc;
    }

    private static void ValidateQuantity(int quantity)
    {
        if (quantity <= 0)
        {
            throw new DomainException("Inventory quantity must be greater than zero.");
        }
    }
}
