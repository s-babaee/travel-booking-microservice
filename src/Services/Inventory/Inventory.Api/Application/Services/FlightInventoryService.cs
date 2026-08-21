using BuildingBlocks.Contracts.Events;
using Inventory.Api.Application.Abstractions;
using Inventory.Api.Application.Contracts;
using Inventory.Api.Application.Exceptions;
using Inventory.Api.Domain.Entities;
using Inventory.Api.Domain.Enums;

namespace Inventory.Api.Application.Services;

public sealed class FlightInventoryService : IFlightInventoryService
{
    private readonly IFlightInventoryRepository _inventory;
    private readonly IFlightInventoryHoldRepository _holds;
    private readonly IUnitOfWork _unitOfWork;
    private readonly IIntegrationEventPublisher _eventPublisher;
    private readonly TimeProvider _timeProvider;

    public FlightInventoryService(
        IFlightInventoryRepository inventory,
        IFlightInventoryHoldRepository holds,
        IUnitOfWork unitOfWork,
        IIntegrationEventPublisher eventPublisher,
        TimeProvider timeProvider)
    {
        _inventory = inventory;
        _holds = holds;
        _unitOfWork = unitOfWork;
        _eventPublisher = eventPublisher;
        _timeProvider = timeProvider;
    }

    public async Task<InventoryHoldResponse> HoldAsync(
        FlightHoldRequest request,
        CancellationToken cancellationToken)
    {
        ArgumentNullException.ThrowIfNull(request);
        InventoryServiceHelpers.ValidateId(request.HoldId, "Hold id");
        InventoryServiceHelpers.ValidateId(request.FlightId, "Flight id");
        var quantities = NormalizeQuantities(request.Classes);
        var nowUtc = UtcNow();
        var expiresAtUtc = InventoryServiceHelpers.Utc(
            request.ExpiresAtUtc,
            nowUtc);
        var lines = quantities
            .Select(item => FlightInventoryHoldLine.Create(
                item.Key,
                request.Date,
                item.Value))
            .ToArray();

        var existing = await _holds.GetByIdAsync(
            request.HoldId,
            cancellationToken);
        if (existing is not null)
        {
            if (!existing.Matches(request.FlightId, lines))
            {
                throw new ConflictException(
                    "The hold id is already associated with a different request.");
            }

            return ToResponse(existing);
        }

        await using var transaction =
            await _unitOfWork.BeginTransactionAsync(cancellationToken);
        var classIds = quantities.Keys.OrderBy(id => id).ToArray();
        var rows = await _inventory.GetForUpdateAsync(
            request.FlightId,
            classIds,
            request.Date,
            cancellationToken);
        if (rows.Count != classIds.Length)
        {
            throw new ConflictException(
                "Inventory has not been initialized for every requested flight class.");
        }

        foreach (var row in rows)
        {
            row.Hold(quantities[row.FlightClassId], nowUtc);
        }

        var hold = FlightInventoryHold.Create(
            request.HoldId,
            request.FlightId,
            expiresAtUtc,
            nowUtc,
            lines);
        await _holds.AddAsync(hold, cancellationToken);
        await _unitOfWork.SaveChangesAsync(cancellationToken);
        await transaction.CommitAsync(cancellationToken);

        await PublishAsync(rows, nowUtc, cancellationToken);
        return ToResponse(hold);
    }

    public Task<InventoryHoldResponse> ConfirmAsync(
        ConfirmReleaseRequest request,
        CancellationToken cancellationToken)
    {
        return CompleteAsync(
            request,
            confirm: true,
            cancellationToken);
    }

    public Task<InventoryHoldResponse> ReleaseAsync(
        ConfirmReleaseRequest request,
        CancellationToken cancellationToken)
    {
        return CompleteAsync(
            request,
            confirm: false,
            cancellationToken);
    }

    public async Task<IReadOnlyList<FlightAvailabilityResponse>> GetAvailabilityAsync(
        Guid flightId,
        DateOnly date,
        Guid? flightClassId,
        CancellationToken cancellationToken)
    {
        InventoryServiceHelpers.ValidateId(flightId, "Flight id");
        var rows = await _inventory.ListAsync(
            flightId,
            date,
            flightClassId,
            cancellationToken);
        return rows.Select(ToResponse).ToArray();
    }

    public async Task<IReadOnlyList<FlightAvailabilityResponse>> AdjustAsync(
        FlightInventoryAdjustmentRequest request,
        CancellationToken cancellationToken)
    {
        ArgumentNullException.ThrowIfNull(request);
        InventoryServiceHelpers.ValidateId(request.FlightId, "Flight id");
        var adjustments = NormalizeAdjustments(request.Items);
        var nowUtc = UtcNow();

        await using var transaction =
            await _unitOfWork.BeginTransactionAsync(cancellationToken);
        var changed = new List<FlightInventoryDay>(adjustments.Count);
        foreach (var adjustment in adjustments)
        {
            await _inventory.EnsureExistsAsync(
                request.FlightId,
                adjustment.Key,
                request.Date,
                nowUtc,
                cancellationToken);

            var row = await _inventory.GetForUpdateAsync(
                request.FlightId,
                adjustment.Key,
                request.Date,
                cancellationToken)
                ?? throw new ConflictException(
                    "The inventory row could not be created.");
            row.AdjustTo(adjustment.Value, nowUtc);
            changed.Add(row);
        }

        await _unitOfWork.SaveChangesAsync(cancellationToken);
        await transaction.CommitAsync(cancellationToken);
        await PublishAsync(changed, nowUtc, cancellationToken);
        return changed.Select(ToResponse).ToArray();
    }

    public async Task ExpireAsync(
        DateTime nowUtc,
        CancellationToken cancellationToken)
    {
        var expired = await _holds.ListExpiredAsync(
            nowUtc,
            cancellationToken);
        foreach (var hold in expired)
        {
            await using var transaction =
                await _unitOfWork.BeginTransactionAsync(cancellationToken);
            var current = await _holds.GetByIdAsync(
                hold.Id,
                cancellationToken);
            if (current is null
                || current.Status != HoldStatus.Active)
            {
                continue;
            }

            var classIds = current.Lines
                .Select(line => line.FlightClassId)
                .Distinct()
                .OrderBy(id => id)
                .ToArray();
            var rows = await _inventory.GetForUpdateAsync(
                current.FlightId,
                classIds,
                current.Lines.First().Date,
                cancellationToken);

            foreach (var line in current.Lines)
            {
                var row = rows.SingleOrDefault(candidate =>
                    candidate.FlightClassId == line.FlightClassId);
                row?.ReleaseHold(line.Quantity, nowUtc);
            }

            current.Expire(nowUtc);
            await _unitOfWork.SaveChangesAsync(cancellationToken);
            await transaction.CommitAsync(cancellationToken);
            await PublishAsync(rows, nowUtc, cancellationToken);
        }
    }

    private async Task<InventoryHoldResponse> CompleteAsync(
        ConfirmReleaseRequest request,
        bool confirm,
        CancellationToken cancellationToken)
    {
        ArgumentNullException.ThrowIfNull(request);
        InventoryServiceHelpers.ValidateId(request.HoldId, "Hold id");
        var hold = await _holds.GetByIdAsync(
            request.HoldId,
            cancellationToken)
            ?? throw new NotFoundException("Flight inventory hold", request.HoldId);

        if (hold.Status != HoldStatus.Active)
        {
            return ToResponse(hold);
        }

        var nowUtc = UtcNow();
        if (hold.ExpiresAtUtc <= nowUtc)
        {
            throw new ConflictException(
                "The inventory hold has expired and must be released.");
        }

        await using var transaction =
            await _unitOfWork.BeginTransactionAsync(cancellationToken);
        var classIds = hold.Lines
            .Select(line => line.FlightClassId)
            .Distinct()
            .OrderBy(id => id)
            .ToArray();
        var date = hold.Lines.First().Date;
        var rows = await _inventory.GetForUpdateAsync(
            hold.FlightId,
            classIds,
            date,
            cancellationToken);

        foreach (var line in hold.Lines)
        {
            var row = rows.SingleOrDefault(candidate =>
                candidate.FlightClassId == line.FlightClassId)
                ?? throw new ConflictException(
                    "Inventory for the hold is no longer available.");

            if (confirm)
            {
                row.ConfirmHold(line.Quantity, nowUtc);
            }
            else
            {
                row.ReleaseHold(line.Quantity, nowUtc);
            }
        }

        if (confirm)
        {
            hold.Confirm(nowUtc);
        }
        else
        {
            hold.Release(nowUtc);
        }

        await _unitOfWork.SaveChangesAsync(cancellationToken);
        await transaction.CommitAsync(cancellationToken);
        await PublishAsync(rows, nowUtc, cancellationToken);
        return ToResponse(hold);
    }

    private async Task PublishAsync(
        IEnumerable<FlightInventoryDay> rows,
        DateTime occurredAtUtc,
        CancellationToken cancellationToken)
    {
        foreach (var row in rows
            .GroupBy(row => (row.FlightId, row.FlightClassId, row.Date))
            .Select(group => group.First()))
        {
            await _eventPublisher.PublishAsync(
                new FlightAvailabilityChanged(
                    row.FlightId,
                    row.FlightClassId,
                    row.Date,
                    row.TotalSeats,
                    row.AvailableSeats,
                    row.HeldSeats,
                    row.ConfirmedSeats,
                    occurredAtUtc),
                cancellationToken);
        }
    }

    private static Dictionary<Guid, int> NormalizeQuantities(
        IReadOnlyList<FlightClassQuantityRequest>? items)
    {
        if (items is null || items.Count == 0)
        {
            throw new ValidationException(
                "At least one flight class is required.");
        }

        var result = new Dictionary<Guid, int>();
        foreach (var item in items)
        {
            InventoryServiceHelpers.ValidateId(
                item.FlightClassId,
                "Flight class id");
            if (item.Quantity <= 0)
            {
                throw new ValidationException(
                    "Seat quantity must be greater than zero.");
            }

            result[item.FlightClassId] =
                result.GetValueOrDefault(item.FlightClassId) + item.Quantity;
        }

        return result;
    }

    private static Dictionary<Guid, int> NormalizeAdjustments(
        IReadOnlyList<FlightInventoryAdjustmentItem>? items)
    {
        if (items is null || items.Count == 0)
        {
            throw new ValidationException(
                "At least one flight class adjustment is required.");
        }

        var result = new Dictionary<Guid, int>();
        foreach (var item in items)
        {
            InventoryServiceHelpers.ValidateId(
                item.FlightClassId,
                "Flight class id");
            if (item.TotalSeats < 0)
            {
                throw new ValidationException(
                    "Total flight inventory cannot be negative.");
            }

            if (!result.TryAdd(item.FlightClassId, item.TotalSeats))
            {
                throw new ValidationException(
                    "A flight class can appear only once in an adjustment.");
            }
        }

        return result;
    }

    private static InventoryHoldResponse ToResponse(FlightInventoryHold hold)
    {
        return new InventoryHoldResponse(
            hold.Id,
            hold.FlightId,
            hold.Status,
            hold.ExpiresAtUtc,
            hold.CompletedAtUtc,
            hold.Lines
                .OrderBy(line => line.FlightClassId)
                .Select(line => new InventoryHoldLineResponse(
                    line.FlightClassId,
                    line.Date,
                    line.Quantity))
                .ToArray());
    }

    private static FlightAvailabilityResponse ToResponse(
        FlightInventoryDay row)
    {
        return new FlightAvailabilityResponse(
            row.FlightId,
            row.FlightClassId,
            row.Date,
            row.TotalSeats,
            row.AvailableSeats,
            row.HeldSeats,
            row.ConfirmedSeats);
    }

    private DateTime UtcNow() =>
        _timeProvider.GetUtcNow().UtcDateTime;
}
