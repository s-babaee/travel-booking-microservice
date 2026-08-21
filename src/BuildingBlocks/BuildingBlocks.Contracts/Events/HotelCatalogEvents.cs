namespace BuildingBlocks.Contracts.Events;

public sealed record HotelCreated(
    Guid HotelId,
    string Name,
    string City,
    string Country,
    int StarRating,
    DateTime OccurredAtUtc);

public sealed record HotelUpdated(
    Guid HotelId,
    string Name,
    string City,
    string Country,
    int StarRating,
    DateTime OccurredAtUtc);

public sealed record HotelStatusChanged(
    Guid HotelId,
    string Status,
    DateTime OccurredAtUtc);

public sealed record HotelDeleted(
    Guid HotelId,
    DateTime OccurredAtUtc);

public sealed record RoomTypeCreated(
    Guid RoomTypeId,
    Guid HotelId,
    string Name,
    int MaxOccupancy,
    DateTime OccurredAtUtc);

public sealed record RoomTypeUpdated(
    Guid RoomTypeId,
    Guid HotelId,
    string Name,
    int MaxOccupancy,
    DateTime OccurredAtUtc);

public sealed record RoomTypeStatusChanged(
    Guid RoomTypeId,
    Guid HotelId,
    string Status,
    DateTime OccurredAtUtc);

public sealed record RoomTypeDeleted(
    Guid RoomTypeId,
    Guid HotelId,
    DateTime OccurredAtUtc);

public sealed record AmenityCreated(
    Guid AmenityId,
    string Name,
    string Type,
    DateTime OccurredAtUtc);

public sealed record AmenityUpdated(
    Guid AmenityId,
    string Name,
    string Type,
    DateTime OccurredAtUtc);

public sealed record AmenityDeleted(
    Guid AmenityId,
    DateTime OccurredAtUtc);

public sealed record HotelAmenityAssigned(
    Guid HotelId,
    Guid AmenityId,
    DateTime OccurredAtUtc);

public sealed record HotelAmenityRemoved(
    Guid HotelId,
    Guid AmenityId,
    DateTime OccurredAtUtc);

public sealed record RoomTypeAmenityAssigned(
    Guid RoomTypeId,
    Guid AmenityId,
    DateTime OccurredAtUtc);

public sealed record RoomTypeAmenityRemoved(
    Guid RoomTypeId,
    Guid AmenityId,
    DateTime OccurredAtUtc);

public sealed record HotelPolicyCreated(
    Guid PolicyId,
    Guid HotelId,
    string PolicyType,
    DateTime OccurredAtUtc);

public sealed record HotelPolicyUpdated(
    Guid PolicyId,
    Guid HotelId,
    string PolicyType,
    DateTime OccurredAtUtc);

public sealed record HotelPolicyDeleted(
    Guid PolicyId,
    Guid HotelId,
    DateTime OccurredAtUtc);

public sealed record HotelImageAdded(
    Guid ImageId,
    Guid HotelId,
    string Url,
    DateTime OccurredAtUtc);

public sealed record HotelImageDeleted(
    Guid ImageId,
    Guid HotelId,
    DateTime OccurredAtUtc);

public sealed record RoomTypeImageAdded(
    Guid ImageId,
    Guid RoomTypeId,
    string Url,
    DateTime OccurredAtUtc);

public sealed record RoomTypeImageDeleted(
    Guid ImageId,
    Guid RoomTypeId,
    DateTime OccurredAtUtc);

public sealed record HotelAvailabilityChanged(
    Guid HotelId,
    Guid RoomTypeId,
    DateOnly Date,
    int TotalUnits,
    int AvailableUnits,
    int HeldUnits,
    int ConfirmedUnits,
    DateTime OccurredAtUtc);
