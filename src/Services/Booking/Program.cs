using MassTransit;
using BuildingBlocks.Contracts.Events;
using BuildingBlocks.Contracts.Messaging;
using BuildingBlocks.Messaging;
using Booking.Api.Infrastructure.Persistence;
using Microsoft.EntityFrameworkCore;

var builder = WebApplication.CreateBuilder(args);

var bookingDbConnectionString =
    builder.Configuration.GetConnectionString("BookingDb");

if (string.IsNullOrWhiteSpace(bookingDbConnectionString))
{
    throw new InvalidOperationException(
        "ConnectionStrings:BookingDb was not configured.");
}

builder.Services.AddDbContext<AppDbContext>(options =>
    options.UseNpgsql(bookingDbConnectionString));

builder.Services.AddEndpointsApiExplorer();
builder.Services.AddSwaggerGen();

// MassTransit + RabbitMQ
builder.Services.AddMassTransit(x =>
{
    x.UsingRabbitMq((context, cfg) =>
    {
        var broker = builder.Configuration.GetSection("MessageBroker");
        var host = broker["Host"] ?? "localhost";
        var virtualHost = broker["VirtualHost"] ?? "/";
        var username = broker["Username"];
        var password = broker["Password"];

        if (string.IsNullOrWhiteSpace(username)
            || string.IsNullOrWhiteSpace(password))
        {
            throw new InvalidOperationException(
                "MessageBroker:Username and MessageBroker:Password must be configured.");
        }

        cfg.Host(host, virtualHost, h =>
        {
            h.Username(username);
            h.Password(password);
        });

        RabbitMqTopology.ConfigureMessageTopology(cfg);
    });
});

var app = builder.Build();

await DatabaseInitializer.InitializeAsync(
    app.Services,
    app.Configuration.GetValue("Database:ApplyMigrations", true));

app.UseSwagger();
app.UseSwaggerUI();

app.MapPost("/bookings/test-publish", async (IPublishEndpoint publishEndpoint) =>
{
    var evt = new BookingCreatedEvent(
        BookingId: Guid.NewGuid(),
        PassengerName: "Soheil",
        TripType: "Flight",
        CreatedAtUtc: DateTime.UtcNow
    );

    await publishEndpoint.Publish(
        evt,
        publishContext => publishContext.SetRoutingKey(EventExchangeNames.BookingCreated));

    return Results.Ok(new { message = "Published BookingCreatedEvent", evt.BookingId });
});

app.Run();


