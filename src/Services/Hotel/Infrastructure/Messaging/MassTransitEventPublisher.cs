using Hotel.Api.Application.Abstractions;
using MassTransit;

namespace Hotel.Api.Infrastructure.Messaging;

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
        where TEvent : class
    {
        return _publishEndpoint.Publish(@event, cancellationToken);
    }
}
