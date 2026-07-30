using MassTransit;

var builder = WebApplication.CreateBuilder(args);

builder.Services.AddEndpointsApiExplorer();
builder.Services.AddSwaggerGen();

// MassTransit + RabbitMQ
builder.Services.AddMassTransit(x =>
{
    x.UsingRabbitMq((context, cfg) =>
    {
        cfg.Host("localhost", "/", h =>
        {
            h.Username("guest");
            h.Password("guest");
        });
    });
});

var app = builder.Build();

app.UseSwagger();
app.UseSwaggerUI();

app.MapPost("/bookings/test-publish", async (IPublishEndpoint publishEndpoint) =>
{
    var evt = new BuildingBlocks.Events.BookingCreatedEvent(
        BookingId: Guid.NewGuid(),
        PassengerName: "Soheil",
        TripType: "Flight",
        CreatedAtUtc: DateTime.UtcNow
    );

    await publishEndpoint.Publish(evt);

    return Results.Ok(new { message = "Published BookingCreatedEvent", evt.BookingId });
});

app.Run();


