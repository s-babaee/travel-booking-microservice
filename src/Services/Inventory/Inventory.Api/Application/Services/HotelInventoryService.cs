using BuildingBlocks.Contracts.Events;
using Inventory.Api.Application.Abstractions;
using Inventory.Api.Application.Contracts;
using Inventory.Api.Application.Exceptions;
using Inventory.Api.Domain.Entities;

namespace Inventory.Api.Application.Services;

public sealed class HotelInventoryService : IHotelInventoryService
{
    private readonly IHotelInventoryRepository _inventory;
    private readonly IHotelInventoryHoldRepository _holds;
    private readonly IUnitOfWork _unitOfWork;
    private readonly IIntegrationEventPublisher _eventPublisher;
    private readonly TimeProvider _timeProvider;

    public HotelInventoryService(
        IHotelInventoryRepository inventory,
        IHotelInventoryHoldRepository holds,
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
        HotelHoldRequest request,
        CancellationToken cancellationToken)
    {
        ArgumentNullException.ThrowIfNull(request);
        InventoryServiceHelpers.ValidateId(request.HoldId, "Hold id");
        InventoryServiceHelpers.ValidateId(request.HotelId, "Hotel id");
        var dates = InventoryServiceHelpers.Dates(
            request.CheckIn,
            request.CheckOut);
        var quantities = NormalizeQuantities(request.Rooms);
        var nowUtc = UtcNow();
        var expiresAtUtc = InventoryServiceHelpers.Utc(
            request.ExpiresAtUtc,
            nowUtc);

        var lines = quantities
            .SelectMany(item => dates.Select(date =>
                HotelInventoryHoldLine.Create(
                    item.Key,
                    date,
                    item.Value)))
            .ToArray();

        var existing = await _holds.GetByIdAsync(
            request.HoldId,
            cancellationToken);
        if (existing is not null)
        {
            if (!existing.Matches(request.HotelId, lines))
            {
                throw new ConflictException(
                    "The hold id is already associated with a different request.");
            }

            return ToResponse(existing);
        }

        await using var transaction =
            await _unitOfWork.BeginTransactionAsync(cancellationToken);
        var roomTypeIds = quantities.Keys.OrderBy(id => id).ToArray();
        var rows = await _inventory.GetForUpdateAsync(
            request.HotelId,
            roomTypeIds,
            request.CheckIn,
            request.CheckOut,
            cancellationToken);
        EnsureAllRowsExist(rows, roomTypeIds, dates);

        foreach (var row in rows)
        {
            row.Hold(
                quantities[(row.RoomTypeId)],
                nowUtc);
        }

        var hold = HotelInventoryHold.Create(
            request.HoldId,
            request.HotelId,
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

    public async Task<IReadOnlyList<HotelAvailabilityResponse>> GetAvailabilityAsync(
        Guid hotelId,
        DateOnly from,
        DateOnly to,
        Guid? roomTypeId,
        CancellationToken cancellationToken)
    {
        InventoryServiceHelpers.ValidateId(hotelId, "Hotel id");
        _ = InventoryServiceHelpers.Dates(from, to);
        var rows = await _inventory.ListAsync(
            hotelId,
            from,
            to,
            roomTypeId,
            cancellationToken);
        return rows.Select(ToResponse).ToArray();
    }

    public async Task<IReadOnlyList<HotelAvailabilityResponse>> AdjustAsync(
        HotelInventoryAdjustmentRequest request,
        CancellationToken cancellationToken)
    {
        ArgumentNullException.ThrowIfNull(request);
        InventoryServiceHelpers.ValidateId(request.HotelId, "Hotel id");
        var dates = InventoryServiceHelpers.Dates(request.From, request.To);
        var adjustments = NormalizeAdjustments(request.Items);
        var nowUtc = UtcNow();

        await using var transaction =
            await _unitOfWork.BeginTransactionAsync(cancellationToken);
        var changed = new List<HotelInventoryDay>(
            dates.Count * adjustments.Count);
        foreach (var date in dates)
        {
            foreach (var adjustment in adjustments)
            {
                await _inventory.EnsureExistsAsync(
                    request.HotelId,
                    adjustment.Key,
                    date,
                    nowUtc,
                    cancellationToken);

                var row = await _inventory.GetForUpdateAsync(
                    request.HotelId,
                    adjustment.Key,
                    date,
                    cancellationToken)
                    ?? throw new ConflictException(
                        "The inventory row could not be created.");
                row.AdjustTo(adjustment.Value, nowUtc);
                changed.Add(row);
            }
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
                || current.Status != Domain.Enums.HoldStatus.Active)
            {
                continue;
            }

            var roomTypeIds = current.Lines
                .Select(line => line.RoomTypeId)
                .Distinct()
                .OrderBy(id => id)
                .ToArray();
            var rows = await _inventory.GetForUpdateAsync(
                current.HotelId,
                roomTypeIds,
                current.Lines.Min(line => line.Date),
                current.Lines.Max(line => line.Date).AddDays(1),
                cancellationToken);

            foreach (var line in current.Lines)
            {
                var row = rows.SingleOrDefault(candidate =>
                    candidate.RoomTypeId == line.RoomTypeId
                    && candidate.Date == line.Date);
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
            ?? throw new NotFoundException("Hotel inventory hold", request.HoldId);

        if (hold.Status != Domain.Enums.HoldStatus.Active)
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
        var grouped = hold.Lines
            .GroupBy(line => line.RoomTypeId)
            .Select(group => group.Key)
            .OrderBy(id => id)
            .ToArray();
        var rows = await _inventory.GetForUpdateAsync(
            hold.HotelId,
            grouped,
            hold.Lines.Min(line => line.Date),
            hold.Lines.Max(line => line.Date).AddDays(1),
            cancellationToken);

        foreach (var line in hold.Lines)
        {
            var row = rows.SingleOrDefault(candidate =>
                candidate.RoomTypeId == line.RoomTypeId
                && candidate.Date == line.Date)
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
        IEnumerable<HotelInventoryDay> rows,
        DateTime occurredAtUtc,
        CancellationToken cancellationToken)
    {
        foreach (var row in rows
            .GroupBy(row => (row.HotelId, row.RoomTypeId, row.Date))
            .Select(group => group.First()))
        {
            await _eventPublisher.PublishAsync(
                new HotelAvailabilityChanged(
                    row.HotelId,
                    row.RoomTypeId,
                    row.Date,
                    row.TotalUnits,
                    row.AvailableUnits,
                    row.HeldUnits,
                    row.ConfirmedUnits,
                    occurredAtUtc),
                cancellationToken);
        }
    }

    private static Dictionary<Guid, int> NormalizeQuantities(
        IReadOnlyList<HotelRoomQuantityRequest>? items)
    {
        if (items is null || items.Count == 0)
        {
            throw new ValidationException(
                "At least one room type is required.");
        }

        var result = new Dictionary<Guid, int>();
        foreach (var item in items)
        {
            InventoryServiceHelpers.ValidateId(item.RoomTypeId, "Room type id");
            if (item.Quantity <= 0)
            {
                throw new ValidationException(
                    "Room quantity must be greater than zero.");
            }

            result[item.RoomTypeId] =
                result.GetValueOrDefault(item.RoomTypeId) + item.Quantity;
        }

        return result;
    }

    private static Dictionary<Guid, int> NormalizeAdjustments(
        IReadOnlyList<HotelInventoryAdjustmentItem>? items)
    {
        if (items is null || items.Count == 0)
        {
            throw new ValidationException(
                "At least one room type adjustment is required.");
        }

        var result = new Dictionary<Guid, int>();
        foreach (var item in items)
        {
            InventoryServiceHelpers.ValidateId(item.RoomTypeId, "Room type id");
            if (item.TotalUnits < 0)
            {
                throw new ValidationException(
                    "Total room inventory cannot be negative.");
            }

            if (!result.TryAdd(item.RoomTypeId, item.TotalUnits))
            {
                throw new ValidationException(
                    "A room type can appear only once in an adjustment.");
            }
        }

        return result;
    }

    private static void EnsureAllRowsExist(
        IReadOnlyList<HotelInventoryDay> rows,
        IReadOnlyCollection<Guid> roomTypeIds,
        IReadOnlyCollection<DateOnly> dates)
    {
        var expected = roomTypeIds.Count * dates.Count;
        if (rows.Count != expected)
        {
            throw new ConflictException(
                "Inventory has not been initialized for every requested room type and date.");
        }
    }

    private static InventoryHoldResponse ToResponse(HotelInventoryHold hold)
    {
        return new InventoryHoldResponse(
            hold.Id,
            hold.HotelId,
            hold.Status,
            hold.ExpiresAtUtc,
            hold.CompletedAtUtc,
            hold.Lines
                .OrderBy(line => line.Date)
                .ThenBy(line => line.RoomTypeId)
                .Select(line => new InventoryHoldLineResponse(
                    line.RoomTypeId,
                    line.Date,
                    line.Quantity))
                .ToArray());
    }

    private static HotelAvailabilityResponse ToResponse(
        HotelInventoryDay row)
    {
        return new HotelAvailabilityResponse(
            row.HotelId,
            row.RoomTypeId,
            row.Date,
            row.TotalUnits,
            row.AvailableUnits,
            row.HeldUnits,
            row.ConfirmedUnits);
    }

    private DateTime UtcNow() =>
        _timeProvider.GetUtcNow().UtcDateTime;
}
