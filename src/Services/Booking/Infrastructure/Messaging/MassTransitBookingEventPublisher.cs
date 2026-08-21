using Booking.Api.Application.Abstractions;
using BuildingBlocks.Contracts.Events;
using BuildingBlocks.Contracts.Messaging;
using MassTransit;

namespace Booking.Api.Infrastructure.Messaging;

public sealed class MassTransitBookingEventPublisher(
    IPublishEndpoint publishEndpoint) : IBookingEventPublisher
{
    public Task PublishAsync<TEvent>(
        TEvent @event,
        CancellationToken cancellationToken)
        where TEvent : class =>
        publishEndpoint.Publish(
            @event,
            context =>
            {
                if (@event is BookingCreatedEvent)
                {
                    context.SetRoutingKey(EventExchangeNames.BookingCreated);
                }
                else if (@event is BookingStatusChangedEvent)
                {
                    context.SetRoutingKey(EventExchangeNames.BookingStatusChanged);
                }
            },
            cancellationToken);
}
