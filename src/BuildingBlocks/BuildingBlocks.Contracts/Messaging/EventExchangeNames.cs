namespace BuildingBlocks.Contracts.Messaging;

public static class EventExchangeNames
{
    public const string BookingCreated = "travel.booking.booking-created.v1";
    public const string BookingStatusChanged = "travel.booking.status-changed.v1";
    public const string PaymentAuthorizationRequested =
        "travel.payment.authorization-requested.v1";
    public const string PaymentAuthorized = "travel.payment.authorized.v1";
    public const string PaymentAuthorizationFailed =
        "travel.payment.authorization-failed.v1";
    public const string PaymentFailed = "travel.payment.failed.v1";
    public const string PaymentRefunded = "travel.payment.refunded.v1";
    public const string BookingConfirmed = "travel.booking.confirmed.v1";
    public const string BookingFailed = "travel.booking.failed.v1";
    public const string BookingCancellationStarted =
        "travel.booking.cancellation-started.v1";
    public const string BookingCancelled = "travel.booking.cancelled.v1";

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
    public const string HotelAvailabilityChanged = "travel.inventory.hotel-availability-changed.v1";

    public const string FlightCreated = "travel.flight.flight-created.v1";
    public const string FlightUpdated = "travel.flight.flight-updated.v1";
    public const string FlightStatusChanged = "travel.flight.flight-status-changed.v1";
    public const string FlightDeleted = "travel.flight.flight-deleted.v1";
    public const string RouteCreated = "travel.flight.route-created.v1";
    public const string RouteUpdated = "travel.flight.route-updated.v1";
    public const string RouteDeleted = "travel.flight.route-deleted.v1";
    public const string FlightScheduleCreated = "travel.flight.flight-schedule-created.v1";
    public const string FlightScheduleUpdated = "travel.flight.flight-schedule-updated.v1";
    public const string FlightScheduleDeleted = "travel.flight.flight-schedule-deleted.v1";
    public const string FlightClassCreated = "travel.flight.flight-class-created.v1";
    public const string FlightClassUpdated = "travel.flight.flight-class-updated.v1";
    public const string FlightClassStatusChanged = "travel.flight.flight-class-status-changed.v1";
    public const string FlightClassDeleted = "travel.flight.flight-class-deleted.v1";
    public const string FlightPolicyCreated = "travel.flight.flight-policy-created.v1";
    public const string FlightPolicyUpdated = "travel.flight.flight-policy-updated.v1";
    public const string FlightPolicyDeleted = "travel.flight.flight-policy-deleted.v1";
    public const string AirlineCreated = "travel.flight.airline-created.v1";
    public const string AirlineUpdated = "travel.flight.airline-updated.v1";
    public const string AirlineDeleted = "travel.flight.airline-deleted.v1";
    public const string FlightAvailabilityChanged = "travel.inventory.flight-availability-changed.v1";
}
