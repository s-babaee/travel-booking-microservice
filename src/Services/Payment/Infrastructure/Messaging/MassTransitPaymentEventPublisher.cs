using BuildingBlocks.Contracts.Events;
using BuildingBlocks.Contracts.Messaging;
using MassTransit;
using Payment.Api.Application.Abstractions;

namespace Payment.Api.Infrastructure.Messaging;

public sealed class MassTransitPaymentEventPublisher(
    IPublishEndpoint publishEndpoint) : IPaymentEventPublisher
{
    public Task PublishAsync<TEvent>(
        TEvent @event,
        CancellationToken cancellationToken)
        where TEvent : class =>
        publishEndpoint.Publish(
            @event,
            context =>
            {
                if (@event is PaymentAuthorized)
                {
                    context.SetRoutingKey(EventExchangeNames.PaymentAuthorized);
                }
                else if (@event is PaymentFailedEvent)
                {
                    context.SetRoutingKey(EventExchangeNames.PaymentFailed);
                }
                else if (@event is PaymentRefundedEvent)
                {
                    context.SetRoutingKey(EventExchangeNames.PaymentRefunded);
                }
            },
            cancellationToken);
}
