using Inventory.Api.Domain.Common;
using Inventory.Api.Domain.Enums;

namespace Inventory.Api.Domain.Entities;

public sealed class HotelInventoryHold
{
    private readonly List<HotelInventoryHoldLine> _lines = [];

    private HotelInventoryHold()
    {
    }

    private HotelInventoryHold(
        Guid id,
        Guid hotelId,
        DateTime expiresAtUtc,
        DateTime createdAtUtc,
        IEnumerable<HotelInventoryHoldLine> lines)
    {
        Id = id;
        HotelId = hotelId;
        ExpiresAtUtc = expiresAtUtc;
        CreatedAtUtc = createdAtUtc;
        Status = HoldStatus.Active;
        _lines.AddRange(lines);
    }

    public Guid Id { get; private set; }
    public Guid HotelId { get; private set; }
    public HoldStatus Status { get; private set; }
    public DateTime ExpiresAtUtc { get; private set; }
    public DateTime CreatedAtUtc { get; private set; }
    public DateTime? CompletedAtUtc { get; private set; }
    public IReadOnlyCollection<HotelInventoryHoldLine> Lines => _lines;

    public static HotelInventoryHold Create(
        Guid id,
        Guid hotelId,
        DateTime expiresAtUtc,
        DateTime createdAtUtc,
        IEnumerable<HotelInventoryHoldLine> lines)
    {
        if (id == Guid.Empty || hotelId == Guid.Empty)
        {
            throw new DomainException("Hold and hotel ids are required.");
        }

        var materializedLines = lines?.ToArray()
            ?? throw new ArgumentNullException(nameof(lines));
        if (materializedLines.Length == 0)
        {
            throw new DomainException("A hold must contain at least one inventory line.");
        }

        if (expiresAtUtc <= createdAtUtc)
        {
            throw new DomainException("Hold expiration must be in the future.");
        }

        return new HotelInventoryHold(
            id,
            hotelId,
            expiresAtUtc,
            createdAtUtc,
            materializedLines);
    }

    public void Confirm(DateTime nowUtc)
    {
        EnsureActive();
        Status = HoldStatus.Confirmed;
        CompletedAtUtc = nowUtc;
    }

    public void Release(DateTime nowUtc)
    {
        EnsureActive();
        Status = HoldStatus.Released;
        CompletedAtUtc = nowUtc;
    }

    public void Expire(DateTime nowUtc)
    {
        EnsureActive();
        Status = HoldStatus.Expired;
        CompletedAtUtc = nowUtc;
    }

    public bool Matches(Guid hotelId, IEnumerable<HotelInventoryHoldLine> lines)
    {
        var requested = lines
            .OrderBy(line => line.RoomTypeId)
            .ThenBy(line => line.Date)
            .Select(line => (line.RoomTypeId, line.Date, line.Quantity))
            .ToArray();
        var existing = Lines
            .OrderBy(line => line.RoomTypeId)
            .ThenBy(line => line.Date)
            .Select(line => (line.RoomTypeId, line.Date, line.Quantity))
            .ToArray();
        return HotelId == hotelId && requested.SequenceEqual(existing);
    }

    private void EnsureActive()
    {
        if (Status != HoldStatus.Active)
        {
            throw new DomainException(
                $"The inventory hold is already {Status.ToString().ToLowerInvariant()}.");
        }
    }
}

public sealed class HotelInventoryHoldLine
{
    private HotelInventoryHoldLine()
    {
    }

    private HotelInventoryHoldLine(
        Guid roomTypeId,
        DateOnly date,
        int quantity)
    {
        RoomTypeId = roomTypeId;
        Date = date;
        Quantity = quantity;
    }

    public Guid RoomTypeId { get; private set; }
    public DateOnly Date { get; private set; }
    public int Quantity { get; private set; }

    public static HotelInventoryHoldLine Create(
        Guid roomTypeId,
        DateOnly date,
        int quantity)
    {
        if (roomTypeId == Guid.Empty || quantity <= 0)
        {
            throw new DomainException(
                "Room type id and hold quantity are required.");
        }

        return new HotelInventoryHoldLine(roomTypeId, date, quantity);
    }
}
