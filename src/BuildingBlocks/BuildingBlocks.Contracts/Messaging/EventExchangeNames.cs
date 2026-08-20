namespace BuildingBlocks.Contracts.Messaging;

public static class EventExchangeNames
{
    public const string BookingCreated = "travel.booking.booking-created.v1";

    public const string HotelCreated = "travel.hotel.hotel-created.v1";
    public const string HotelUpdated = "travel.hotel.hotel-updated.v1";
    public const string HotelStatusChanged = "travel.hotel.hotel-status-changed.v1";
    public const string HotelDeleted = "travel.hotel.hotel-deleted.v1";

    public const string RoomTypeCreated = "travel.hotel.room-type-created.v1";
    public const string RoomTypeUpdated = "travel.hotel.room-type-updated.v1";
    public const string RoomTypeStatusChanged = "travel.hotel.room-type-status-changed.v1";
    public const string RoomTypeDeleted = "travel.hotel.room-type-deleted.v1";

    public const string AmenityCreated = "travel.hotel.amenity-created.v1";
    public const string AmenityUpdated = "travel.hotel.amenity-updated.v1";
    public const string AmenityDeleted = "travel.hotel.amenity-deleted.v1";

    public const string HotelAmenityAssigned = "travel.hotel.hotel-amenity-assigned.v1";
    public const string HotelAmenityRemoved = "travel.hotel.hotel-amenity-removed.v1";
    public const string RoomTypeAmenityAssigned = "travel.hotel.room-type-amenity-assigned.v1";
    public const string RoomTypeAmenityRemoved = "travel.hotel.room-type-amenity-removed.v1";

    public const string HotelPolicyCreated = "travel.hotel.hotel-policy-created.v1";
    public const string HotelPolicyUpdated = "travel.hotel.hotel-policy-updated.v1";
    public const string HotelPolicyDeleted = "travel.hotel.hotel-policy-deleted.v1";

    public const string HotelImageAdded = "travel.hotel.hotel-image-added.v1";
    public const string HotelImageDeleted = "travel.hotel.hotel-image-deleted.v1";
    public const string RoomTypeImageAdded = "travel.hotel.room-type-image-added.v1";
    public const string RoomTypeImageDeleted = "travel.hotel.room-type-image-deleted.v1";
}
