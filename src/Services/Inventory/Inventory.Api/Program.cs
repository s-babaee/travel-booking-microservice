using System.Text.Json.Serialization;
using BuildingBlocks.Messaging;
using Inventory.Api.Application.Abstractions;
using Inventory.Api.Application.Services;
using Inventory.Api.Infrastructure.Messaging;
using Inventory.Api.Infrastructure.Background;
using Inventory.Api.Infrastructure.Persistence;
using Inventory.Api.Infrastructure.Web;
using MassTransit;
using Microsoft.EntityFrameworkCore;
using Microsoft.OpenApi;

var builder = WebApplication.CreateBuilder(args);

var connectionString =
    builder.Configuration.GetConnectionString("InventoryDb");
if (string.IsNullOrWhiteSpace(connectionString))
{
    throw new InvalidOperationException(
        "ConnectionStrings:InventoryDb was not configured.");
}

builder.Services.AddDbContext<AppDbContext>(options =>
    options.UseNpgsql(connectionString));
builder.Services.AddScoped<IUnitOfWork>(services =>
    services.GetRequiredService<AppDbContext>());
builder.Services.AddScoped<IHotelInventoryRepository>(services =>
    services.GetRequiredService<AppDbContext>());
builder.Services.AddScoped<IFlightInventoryRepository>(services =>
    services.GetRequiredService<AppDbContext>());
builder.Services.AddScoped<IHotelInventoryHoldRepository>(services =>
    services.GetRequiredService<AppDbContext>());
builder.Services.AddScoped<IFlightInventoryHoldRepository>(services =>
    services.GetRequiredService<AppDbContext>());
builder.Services.AddScoped<IIntegrationEventPublisher,
    MassTransitEventPublisher>();
builder.Services.AddScoped<IHotelInventoryService, HotelInventoryService>();
builder.Services.AddScoped<IFlightInventoryService, FlightInventoryService>();
builder.Services.AddSingleton(TimeProvider.System);
builder.Services.AddHostedService<InventoryExpirationWorker>();

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

builder.Services.AddMassTransit(configurator =>
{
    configurator.UsingRabbitMq((_, cfg) =>
    {
        cfg.Host(brokerHost, brokerVirtualHost, host =>
        {
            host.Username(brokerUsername);
            host.Password(brokerPassword);
        });
        RabbitMqTopology.ConfigureMessageTopology(cfg);
    });
});

builder.Services
    .AddControllers()
    .AddJsonOptions(options =>
    {
        options.JsonSerializerOptions.Converters.Add(
            new JsonStringEnumConverter());
    });
builder.Services.AddEndpointsApiExplorer();
builder.Services.AddSwaggerGen(options =>
{
    options.SwaggerDoc("v1", new OpenApiInfo
    {
        Title = "Inventory API",
        Version = "v1",
        Description =
            "Atomic hotel and flight inventory holds, confirmations, releases and availability."
    });
});

var app = builder.Build();
await DatabaseInitializer.InitializeAsync(
    app.Services,
    app.Configuration.GetValue("Database:ApplyMigrations", true));

app.UseMiddleware<ExceptionHandlingMiddleware>();
if (app.Environment.IsDevelopment())
{
    app.UseSwagger();
    app.UseSwaggerUI();
}

app.MapControllers();
app.MapGet("/health", () => Results.Ok(new { status = "ok" }));
app.Run();

public partial class Program
{
}
