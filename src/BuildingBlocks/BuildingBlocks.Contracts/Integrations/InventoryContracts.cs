namespace BuildingBlocks.Contracts.Integrations;

public sealed record InventoryRoomQuantity(
    Guid RoomTypeId,
    int Quantity);

public sealed record InventoryFlightClassQuantity(
    Guid FlightClassId,
    int Quantity);

public sealed record HoldHotelInventoryRequest(
    Guid HoldId,
    Guid HotelId,
    DateOnly CheckIn,
    DateOnly CheckOut,
    IReadOnlyList<InventoryRoomQuantity> Rooms,
    DateTime? ExpiresAtUtc);

public sealed record HoldFlightInventoryRequest(
    Guid HoldId,
    Guid FlightId,
    DateOnly Date,
    IReadOnlyList<InventoryFlightClassQuantity> Classes,
    DateTime? ExpiresAtUtc);

public sealed record CompleteInventoryHoldRequest(Guid HoldId);
