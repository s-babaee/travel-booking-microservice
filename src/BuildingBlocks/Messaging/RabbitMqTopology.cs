using BuildingBlocks.Contracts.Events;
using BuildingBlocks.Contracts.Messaging;
using MassTransit;

namespace BuildingBlocks.Messaging;

public static class RabbitMqTopology
{
    public static void ConfigureMessageTopology(
        IRabbitMqBusFactoryConfigurator configurator)
    {
        Configure<BookingCreatedEvent>(
            configurator,
            EventExchangeNames.BookingCreated);
        Configure<BookingStatusChangedEvent>(
            configurator,
            EventExchangeNames.BookingStatusChanged);
        Configure<PaymentAuthorizationRequested>(
            configurator,
            EventExchangeNames.PaymentAuthorizationRequested);
        Configure<PaymentAuthorized>(
            configurator,
            EventExchangeNames.PaymentAuthorized);
        Configure<PaymentAuthorizationFailed>(
            configurator,
            EventExchangeNames.PaymentAuthorizationFailed);

        Configure<HotelCreated>(configurator, EventExchangeNames.HotelCreated);
        Configure<HotelUpdated>(configurator, EventExchangeNames.HotelUpdated);
        Configure<HotelStatusChanged>(
            configurator,
            EventExchangeNames.HotelStatusChanged);
        Configure<HotelDeleted>(configurator, EventExchangeNames.HotelDeleted);

        Configure<RoomTypeCreated>(
            configurator,
            EventExchangeNames.RoomTypeCreated);
        Configure<RoomTypeUpdated>(
            configurator,
            EventExchangeNames.RoomTypeUpdated);
        Configure<RoomTypeStatusChanged>(
            configurator,
            EventExchangeNames.RoomTypeStatusChanged);
        Configure<RoomTypeDeleted>(
            configurator,
            EventExchangeNames.RoomTypeDeleted);

        Configure<AmenityCreated>(
            configurator,
            EventExchangeNames.AmenityCreated);
        Configure<AmenityUpdated>(
            configurator,
            EventExchangeNames.AmenityUpdated);
        Configure<AmenityDeleted>(
            configurator,
            EventExchangeNames.AmenityDeleted);

        Configure<HotelAmenityAssigned>(
            configurator,
            EventExchangeNames.HotelAmenityAssigned);
        Configure<HotelAmenityRemoved>(
            configurator,
            EventExchangeNames.HotelAmenityRemoved);
        Configure<RoomTypeAmenityAssigned>(
            configurator,
            EventExchangeNames.RoomTypeAmenityAssigned);
        Configure<RoomTypeAmenityRemoved>(
            configurator,
            EventExchangeNames.RoomTypeAmenityRemoved);

        Configure<HotelPolicyCreated>(
            configurator,
            EventExchangeNames.HotelPolicyCreated);
        Configure<HotelPolicyUpdated>(
            configurator,
            EventExchangeNames.HotelPolicyUpdated);
        Configure<HotelPolicyDeleted>(
            configurator,
            EventExchangeNames.HotelPolicyDeleted);

        Configure<HotelImageAdded>(
            configurator,
            EventExchangeNames.HotelImageAdded);
        Configure<HotelImageDeleted>(
            configurator,
            EventExchangeNames.HotelImageDeleted);
        Configure<RoomTypeImageAdded>(
            configurator,
            EventExchangeNames.RoomTypeImageAdded);
        Configure<RoomTypeImageDeleted>(
            configurator,
            EventExchangeNames.RoomTypeImageDeleted);
        Configure<HotelAvailabilityChanged>(
            configurator,
            EventExchangeNames.HotelAvailabilityChanged);

        Configure<FlightCreated>(configurator, EventExchangeNames.FlightCreated);
        Configure<FlightUpdated>(configurator, EventExchangeNames.FlightUpdated);
        Configure<FlightStatusChanged>(configurator, EventExchangeNames.FlightStatusChanged);
        Configure<FlightDeleted>(configurator, EventExchangeNames.FlightDeleted);
        Configure<RouteCreated>(configurator, EventExchangeNames.RouteCreated);
        Configure<RouteUpdated>(configurator, EventExchangeNames.RouteUpdated);
        Configure<RouteDeleted>(configurator, EventExchangeNames.RouteDeleted);
        Configure<FlightScheduleCreated>(configurator, EventExchangeNames.FlightScheduleCreated);
        Configure<FlightScheduleUpdated>(configurator, EventExchangeNames.FlightScheduleUpdated);
        Configure<FlightScheduleDeleted>(configurator, EventExchangeNames.FlightScheduleDeleted);
        Configure<FlightClassCreated>(configurator, EventExchangeNames.FlightClassCreated);
        Configure<FlightClassUpdated>(configurator, EventExchangeNames.FlightClassUpdated);
        Configure<FlightClassStatusChanged>(configurator, EventExchangeNames.FlightClassStatusChanged);
        Configure<FlightClassDeleted>(configurator, EventExchangeNames.FlightClassDeleted);
        Configure<FlightPolicyCreated>(configurator, EventExchangeNames.FlightPolicyCreated);
        Configure<FlightPolicyUpdated>(configurator, EventExchangeNames.FlightPolicyUpdated);
        Configure<FlightPolicyDeleted>(configurator, EventExchangeNames.FlightPolicyDeleted);
        Configure<AirlineCreated>(configurator, EventExchangeNames.AirlineCreated);
        Configure<AirlineUpdated>(configurator, EventExchangeNames.AirlineUpdated);
        Configure<AirlineDeleted>(configurator, EventExchangeNames.AirlineDeleted);
        Configure<FlightAvailabilityChanged>(
            configurator,
            EventExchangeNames.FlightAvailabilityChanged);
    }

    private static void Configure<TEvent>(
        IRabbitMqBusFactoryConfigurator configurator,
        string exchangeName)
        where TEvent : class
    {
        configurator.Message<TEvent>(message =>
            message.SetEntityName(exchangeName));

        configurator.Publish<TEvent>(publish =>
            publish.ExchangeType = "fanout");
    }
}
