using Flight.Api.Application.Abstractions;
using MassTransit;

namespace Flight.Api.Infrastructure.Messaging;

public sealed class MassTransitEventPublisher : IIntegrationEventPublisher
{
    private readonly IPublishEndpoint _publishEndpoint;

    public MassTransitEventPublisher(IPublishEndpoint publishEndpoint)
    {
        _publishEndpoint = publishEndpoint;
    }

    public Task PublishAsync<TEvent>(
        TEvent @event,
        CancellationToken cancellationToken)
        where TEvent : class =>
        _publishEndpoint.Publish(@event, cancellationToken);
}
