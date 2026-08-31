using Booking.Api.Application.Abstractions;
using Booking.Api.Application.Services;
using Booking.Api.Infrastructure.Integrations;
using Booking.Api.Infrastructure.Messaging;
using Booking.Api.Infrastructure.Persistence;
using Booking.Api.Infrastructure.Web;
using BuildingBlocks.Authorization;
using BuildingBlocks.Messaging;
using MassTransit;
using Microsoft.EntityFrameworkCore;
using Microsoft.OpenApi;
using System.Text.Json.Serialization;

var builder = WebApplication.CreateBuilder(args);

// ==========================================
// 1. Configuration & Options
// ==========================================
var bookingDbConnectionString = builder.Configuration.GetConnectionString("BookingDb");
if (string.IsNullOrWhiteSpace(bookingDbConnectionString))
{
    throw new InvalidOperationException(
        "ConnectionStrings:BookingDb was not configured.");
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
    options.UseNpgsql(bookingDbConnectionString));

builder.Services.AddScoped<IUnitOfWork>(services =>
    services.GetRequiredService<AppDbContext>());

builder.Services.AddScoped<IBookingRepository>(services =>
    services.GetRequiredService<AppDbContext>());
builder.Services.AddScoped<IOrderRepository>(services =>
    services.GetRequiredService<AppDbContext>());

// ==========================================
// 3. Application Services & External Clients
// ==========================================
builder.Services.AddSingleton(TimeProvider.System);
builder.Services.AddHttpContextAccessor();

builder.Services.AddScoped<BookingService>();
builder.Services.AddScoped<ICurrentUser, CurrentUser>();
builder.Services.AddScoped<IBookingEventPublisher, MassTransitBookingEventPublisher>();

var inventoryBaseUrl = builder.Configuration["Services:InventoryBaseUrl"]
    ?? "http://localhost:5256/";
builder.Services.AddHttpClient<IInventoryGateway, InventoryHttpGateway>(client =>
{
    client.BaseAddress = new Uri(inventoryBaseUrl);
    client.Timeout = TimeSpan.FromSeconds(30);
});

var paymentMode = builder.Configuration["Payment:Mode"] ?? "Http";
if (paymentMode.Equals("Mock", StringComparison.OrdinalIgnoreCase))
{
    builder.Services.AddSingleton<IPaymentGateway, MockPaymentGateway>();
}
else
{
    var paymentBaseUrl = builder.Configuration["Services:PaymentBaseUrl"]
        ?? "http://localhost:5209/";
    builder.Services.AddHttpClient<IPaymentGateway, PaymentHttpGateway>(client =>
    {
        client.BaseAddress = new Uri(paymentBaseUrl);
        client.Timeout = TimeSpan.FromSeconds(30);
    });
}

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
        Title = "Booking API",
        Version = "v1",
        Description = "DDD booking saga orchestrator."
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

app.UseHttpsRedirection();
app.MapControllers();

app.Run();

public partial class Program
{
}
