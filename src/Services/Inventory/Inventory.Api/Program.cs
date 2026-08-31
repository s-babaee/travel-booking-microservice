using System.Text.Json.Serialization;
using BuildingBlocks.Authorization;
using BuildingBlocks.Messaging;
using Inventory.Api.Application.Abstractions;
using Inventory.Api.Application.Services;
using Inventory.Api.Infrastructure.Background;
using Inventory.Api.Infrastructure.Messaging;
using Inventory.Api.Infrastructure.Persistence;
using Inventory.Api.Infrastructure.Web;
using MassTransit;
using Microsoft.EntityFrameworkCore;
using Microsoft.OpenApi;

var builder = WebApplication.CreateBuilder(args);

// ==========================================
// 1. Configuration & Options
// ==========================================
var inventoryDbConnectionString = builder.Configuration.GetConnectionString("InventoryDb");
if (string.IsNullOrWhiteSpace(inventoryDbConnectionString))
{
    throw new InvalidOperationException(
        "ConnectionStrings:InventoryDb was not configured.");
}

var brokerHost = builder.Configuration["MessageBroker:Host"] ?? "localhost";
var brokerPort = builder.Configuration.GetValue<ushort>("MessageBroker:Port", 5672);
var brokerVirtualHost = builder.Configuration["MessageBroker:VirtualHost"] ?? "/";
var brokerUsername = builder.Configuration["MessageBroker:Username"];
var brokerPassword = builder.Configuration["MessageBroker:Password"];

if (string.IsNullOrWhiteSpace(brokerUsername)
    || string.IsNullOrWhiteSpace(brokerPassword))
{
    throw new InvalidOperationException(
        "MessageBroker:Username and MessageBroker:Password must be configured.");
}

// ==========================================
// 2. Database & Repositories (Persistence)
// ==========================================
builder.Services.AddDbContext<AppDbContext>(options =>
    options.UseNpgsql(inventoryDbConnectionString));

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

// ==========================================
// 3. Application Services & External Clients
// ==========================================
builder.Services.AddSingleton(TimeProvider.System);

builder.Services.AddScoped<IIntegrationEventPublisher, MassTransitEventPublisher>();
builder.Services.AddScoped<IHotelInventoryService, HotelInventoryService>();
builder.Services.AddScoped<IFlightInventoryService, FlightInventoryService>();

builder.Services.AddHostedService<InventoryExpirationWorker>();

builder.Services.AddMassTransit(configurator =>
{
    configurator.UsingRabbitMq((_, cfg) =>
    {
        cfg.Host(
            brokerHost,
            brokerPort,
            brokerVirtualHost,
            host =>
            {
                host.Username(brokerUsername);
                host.Password(brokerPassword);
            });

        RabbitMqTopology.ConfigureMessageTopology(cfg);
    });
});

// ==========================================
// 4. Authentication & Authorization
// ==========================================
builder.Services.AddKeycloakJwt(builder.Configuration, builder.Environment);
builder.Services.AddPermissionAuthorization();

// ==========================================
// 5. Web & Swagger Setup
// ==========================================
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
        Description = "Atomic hotel and flight inventory holds, confirmations, releases and availability."
    });

    options.AddSecurityDefinition("Bearer", new OpenApiSecurityScheme
    {
        Name = "Authorization",
        Type = SecuritySchemeType.Http,
        Scheme = "bearer"
    });

    options.AddSecurityRequirement(document =>
        new OpenApiSecurityRequirement
        {
            [new OpenApiSecuritySchemeReference("Bearer", document)] = []
        });
});

// ==========================================
// 6. Application Pipeline (Middleware)
// ==========================================
var app = builder.Build();

var applyMigrations = app.Configuration.GetValue(
    "Database:ApplyMigrations",
    defaultValue: true);

await DatabaseInitializer.InitializeAsync(
    app.Services,
    applyMigrations);

app.UseMiddleware<ExceptionHandlingMiddleware>();

if (app.Environment.IsDevelopment())
{
    app.UseSwagger();
    app.UseSwaggerUI();
}

app.UseAuthentication();
app.UseAuthorization();

app.MapControllers();

app.Run();

public partial class Program
{
}
