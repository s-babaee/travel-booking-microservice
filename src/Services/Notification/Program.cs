using MassTransit;
using Notification.Consumers;
using BuildingBlocks.Contracts.Events;
using BuildingBlocks.Contracts.Messaging;
using BuildingBlocks.Messaging;

var builder = Host.CreateApplicationBuilder(args);

var broker = builder.Configuration.GetSection("MessageBroker");
var brokerHost = broker["Host"] ?? "localhost";
var brokerVirtualHost = broker["VirtualHost"] ?? "/";
var brokerUsername = broker["Username"];
var brokerPassword = broker["Password"];

if (string.IsNullOrWhiteSpace(brokerUsername)
    || string.IsNullOrWhiteSpace(brokerPassword))
{
    throw new InvalidOperationException(
        "MessageBroker:Username and MessageBroker:Password must be configured.");
}

builder.Services.AddMassTransit(x =>
{
    x.AddConsumer<BookingCreatedConsumer>();

    x.UsingRabbitMq((context, cfg) =>
    {
        cfg.Host(brokerHost, brokerVirtualHost, h =>
        {
            h.Username(brokerUsername);
            h.Password(brokerPassword);
        });

        RabbitMqTopology.ConfigureMessageTopology(cfg);

        cfg.ReceiveEndpoint(EventQueueNames.NotificationBookingCreated, e =>
        {
            e.ConfigureConsumeTopology = false;
            e.Bind(EventExchangeNames.BookingCreated);
            e.ConfigureConsumer<BookingCreatedConsumer>(context);
        });
    });
});

var host = builder.Build();
host.Run();
