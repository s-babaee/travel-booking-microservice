using BuildingBlocks.Events;
using MassTransit;

namespace Notification.Consumers;

public class BookingCreatedConsumer : IConsumer<BookingCreatedEvent>
{
    private readonly ILogger<BookingCreatedConsumer> _logger;

    public BookingCreatedConsumer(ILogger<BookingCreatedConsumer> logger)
    {
        _logger = logger;
    }

    public Task Consume(ConsumeContext<BookingCreatedEvent> context)
    {
        var msg = context.Message;

        _logger.LogInformation(
            "Received BookingCreatedEvent: BookingId={BookingId}, Passenger={Passenger}, TripType={TripType}, CreatedAtUtc={CreatedAtUtc}",
            msg.BookingId, msg.PassengerName, msg.TripType, msg.CreatedAtUtc);

        return Task.CompletedTask;
    }
}
