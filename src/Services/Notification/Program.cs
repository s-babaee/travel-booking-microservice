using MassTransit;
using Notification.Consumers;

var builder = Host.CreateApplicationBuilder(args);

// MassTransit + RabbitMQ + Consumer
builder.Services.AddMassTransit(x =>
{
    x.AddConsumer<BookingCreatedConsumer>();

    x.UsingRabbitMq((context, cfg) =>
    {
        cfg.Host("localhost", "/", h =>
        {
            h.Username("guest");
            h.Password("guest");
        });

        cfg.ReceiveEndpoint("notification-service", e =>
        {
            e.ConfigureConsumer<BookingCreatedConsumer>(context);
        });
    });
});

var host = builder.Build();
host.Run();
