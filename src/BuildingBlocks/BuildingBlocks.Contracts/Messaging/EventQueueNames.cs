namespace BuildingBlocks.Contracts.Messaging;

public static class EventQueueNames
{
    public const string NotificationBookingCreated =
        "travel.notification.booking-created.v1";

    public const string PaymentBookingCreated =
        "travel.payment.booking-created.v1";

    public const string NotificationEvents =
        "travel.notification.events.v1";

    public const string SearchHotelCatalog =
        "travel.search.hotel-catalog.v1";
}
