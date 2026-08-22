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
                else if (@event is BookingConfirmedEvent)
                {
                    context.SetRoutingKey(EventExchangeNames.BookingConfirmed);
                }
                else if (@event is BookingFailedEvent)
                {
                    context.SetRoutingKey(EventExchangeNames.BookingFailed);
                }
                else if (@event is BookingCancellationStartedEvent)
                {
                    context.SetRoutingKey(
                        EventExchangeNames.BookingCancellationStarted);
                }
                else if (@event is BookingCancelledEvent)
                {
                    context.SetRoutingKey(EventExchangeNames.BookingCancelled);
                }
            },
            cancellationToken);
}
