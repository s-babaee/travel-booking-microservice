using Inventory.Api.Domain.Common;
using Inventory.Api.Domain.Enums;

namespace Inventory.Api.Domain.Entities;

public sealed class FlightInventoryHold
{
    private readonly List<FlightInventoryHoldLine> _lines = [];

    private FlightInventoryHold()
    {
    }

    private FlightInventoryHold(
        Guid id,
        Guid flightId,
        DateTime expiresAtUtc,
        DateTime createdAtUtc,
        IEnumerable<FlightInventoryHoldLine> lines)
    {
        Id = id;
        FlightId = flightId;
        ExpiresAtUtc = expiresAtUtc;
        CreatedAtUtc = createdAtUtc;
        Status = HoldStatus.Active;
        _lines.AddRange(lines);
    }

    public Guid Id { get; private set; }
    public Guid FlightId { get; private set; }
    public HoldStatus Status { get; private set; }
    public DateTime ExpiresAtUtc { get; private set; }
    public DateTime CreatedAtUtc { get; private set; }
    public DateTime? CompletedAtUtc { get; private set; }
    public IReadOnlyCollection<FlightInventoryHoldLine> Lines => _lines;

    public static FlightInventoryHold Create(
        Guid id,
        Guid flightId,
        DateTime expiresAtUtc,
        DateTime createdAtUtc,
        IEnumerable<FlightInventoryHoldLine> lines)
    {
        if (id == Guid.Empty || flightId == Guid.Empty)
        {
            throw new DomainException("Hold and flight ids are required.");
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

        return new FlightInventoryHold(
            id,
            flightId,
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

    public bool Matches(Guid flightId, IEnumerable<FlightInventoryHoldLine> lines)
    {
        var requested = lines
            .OrderBy(line => line.FlightClassId)
            .Select(line => (line.FlightClassId, line.Date, line.Quantity))
            .ToArray();
        var existing = Lines
            .OrderBy(line => line.FlightClassId)
            .Select(line => (line.FlightClassId, line.Date, line.Quantity))
            .ToArray();
        return FlightId == flightId && requested.SequenceEqual(existing);
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

public sealed class FlightInventoryHoldLine
{
    private FlightInventoryHoldLine()
    {
    }

    private FlightInventoryHoldLine(
        Guid flightClassId,
        DateOnly date,
        int quantity)
    {
        FlightClassId = flightClassId;
        Date = date;
        Quantity = quantity;
    }

    public Guid FlightClassId { get; private set; }
    public DateOnly Date { get; private set; }
    public int Quantity { get; private set; }

    public static FlightInventoryHoldLine Create(
        Guid flightClassId,
        DateOnly date,
        int quantity)
    {
        if (flightClassId == Guid.Empty || quantity <= 0)
        {
            throw new DomainException(
                "Flight class id and hold quantity are required.");
        }

        return new FlightInventoryHoldLine(flightClassId, date, quantity);
    }
}
