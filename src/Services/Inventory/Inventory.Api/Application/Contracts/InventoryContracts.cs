using Inventory.Api.Domain.Enums;

namespace Inventory.Api.Application.Contracts;

public sealed record HotelRoomQuantityRequest(
    Guid RoomTypeId,
    int Quantity);

public sealed record HotelHoldRequest(
    Guid HoldId,
    Guid HotelId,
    DateOnly CheckIn,
    DateOnly CheckOut,
    IReadOnlyList<HotelRoomQuantityRequest> Rooms,
    DateTime? ExpiresAtUtc = null);

public sealed record FlightClassQuantityRequest(
    Guid FlightClassId,
    int Quantity);

public sealed record FlightHoldRequest(
    Guid HoldId,
    Guid FlightId,
    DateOnly Date,
    IReadOnlyList<FlightClassQuantityRequest> Classes,
    DateTime? ExpiresAtUtc = null);

public sealed record ConfirmReleaseRequest(Guid HoldId);

public sealed record HotelInventoryAdjustmentRequest(
    Guid HotelId,
    DateOnly From,
    DateOnly To,
    IReadOnlyList<HotelInventoryAdjustmentItem> Items);

public sealed record HotelInventoryAdjustmentItem(
    Guid RoomTypeId,
    int TotalUnits);

public sealed record FlightInventoryAdjustmentRequest(
    Guid FlightId,
    DateOnly Date,
    IReadOnlyList<FlightInventoryAdjustmentItem> Items);

public sealed record FlightInventoryAdjustmentItem(
    Guid FlightClassId,
    int TotalSeats);

public sealed record HotelAvailabilityResponse(
    Guid HotelId,
    Guid RoomTypeId,
    DateOnly Date,
    int TotalUnits,
    int AvailableUnits,
    int HeldUnits,
    int ConfirmedUnits);

public sealed record FlightAvailabilityResponse(
    Guid FlightId,
    Guid FlightClassId,
    DateOnly Date,
    int TotalSeats,
    int AvailableSeats,
    int HeldSeats,
    int ConfirmedSeats);

public sealed record InventoryHoldResponse(
    Guid HoldId,
    Guid ResourceId,
    HoldStatus Status,
    DateTime ExpiresAtUtc,
    DateTime? CompletedAtUtc,
    IReadOnlyList<InventoryHoldLineResponse> Lines);

public sealed record InventoryHoldLineResponse(
    Guid ResourceTypeId,
    DateOnly Date,
    int Quantity);
