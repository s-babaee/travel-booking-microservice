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
