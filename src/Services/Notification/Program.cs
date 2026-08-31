using System.Text.Json.Serialization;
using BuildingBlocks.Authorization;
using BuildingBlocks.Contracts.Events;
using BuildingBlocks.Contracts.Messaging;
using BuildingBlocks.Messaging;
using MassTransit;
using Microsoft.EntityFrameworkCore;
using Microsoft.OpenApi;
using Notification.Application.Abstractions;
using Notification.Application.Services;
using Notification.Infrastructure.Messaging;
using Notification.Infrastructure.Persistence;
using Notification.Infrastructure.Web;

var builder = WebApplication.CreateBuilder(args);

// ==========================================
// 1. Configuration & Options
// ==========================================
var notificationDbConnectionString =
    builder.Configuration.GetConnectionString("NotificationDb");

if (string.IsNullOrWhiteSpace(notificationDbConnectionString))
{
    throw new InvalidOperationException(
        "ConnectionStrings:NotificationDb was not configured.");
}

var brokerHost =
    builder.Configuration["MessageBroker:Host"] ?? "localhost";

var brokerPort =
    builder.Configuration.GetValue<ushort>("MessageBroker:Port", 5672);

var brokerVirtualHost =
    builder.Configuration["MessageBroker:VirtualHost"] ?? "/";

var brokerUsername =
    builder.Configuration["MessageBroker:Username"];

var brokerPassword =
    builder.Configuration["MessageBroker:Password"];

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
    options.UseNpgsql(notificationDbConnectionString));

builder.Services.AddScoped<IUnitOfWork>(services =>
    services.GetRequiredService<AppDbContext>());

builder.Services.AddScoped<INotificationRepository>(services =>
    services.GetRequiredService<AppDbContext>());

builder.Services.AddScoped<INotificationTemplateRepository>(services =>
    services.GetRequiredService<AppDbContext>());

// ==========================================
// 3. Application Services & External Clients
// ==========================================
builder.Services.AddScoped<NotificationService>();

builder.Services.AddScoped<INotificationEventHandler>(services =>
    services.GetRequiredService<NotificationService>());

builder.Services.AddScoped<ICurrentUser, CurrentUser>();

builder.Services.AddSingleton(TimeProvider.System);

builder.Services.AddHttpContextAccessor();

builder.Services.AddMassTransit(configurator =>
{
    // Booking Event Consumers
    configurator.AddConsumer<BookingConfirmedConsumer>();
    configurator.AddConsumer<BookingFailedConsumer>();
    configurator.AddConsumer<BookingCancellationStartedConsumer>();
    configurator.AddConsumer<BookingCancelledConsumer>();

    // Payment Event Consumers
    configurator.AddConsumer<PaymentAuthorizedConsumer>();
    configurator.AddConsumer<PaymentFailedConsumer>();
    configurator.AddConsumer<PaymentRefundedConsumer>();

    configurator.UsingRabbitMq((context, cfg) =>
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

        cfg.ReceiveEndpoint(EventQueueNames.NotificationEvents, endpoint =>
        {
            endpoint.ConfigureConsumeTopology = false;

            // Bind Booking event exchanges
            endpoint.Bind(EventExchangeNames.BookingConfirmed);
            endpoint.Bind(EventExchangeNames.BookingFailed);
            endpoint.Bind(EventExchangeNames.BookingCancellationStarted);
            endpoint.Bind(EventExchangeNames.BookingCancelled);

            // Bind Payment event exchanges
            endpoint.Bind(EventExchangeNames.PaymentAuthorized);
            endpoint.Bind(EventExchangeNames.PaymentFailed);
            endpoint.Bind(EventExchangeNames.PaymentRefunded);

            // Configure Consumers
            endpoint.ConfigureConsumer<BookingConfirmedConsumer>(context);
            endpoint.ConfigureConsumer<BookingFailedConsumer>(context);
            endpoint.ConfigureConsumer<BookingCancellationStartedConsumer>(context);
            endpoint.ConfigureConsumer<BookingCancelledConsumer>(context);

            endpoint.ConfigureConsumer<PaymentAuthorizedConsumer>(context);
            endpoint.ConfigureConsumer<PaymentFailedConsumer>(context);
            endpoint.ConfigureConsumer<PaymentRefundedConsumer>(context);
        });
    });
});

// ==========================================
// 4. Authentication & Authorization
// ==========================================
builder.Services.AddKeycloakJwt(
    builder.Configuration,
    builder.Environment);

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
        Title = "Notification API",
        Version = "v1",
        Description =
            "Notification service API for managing notifications and templates."
    });

    options.AddSecurityDefinition("Bearer", new OpenApiSecurityScheme
    {
        Name = "Authorization",
        Type = SecuritySchemeType.Http,
        Scheme = "bearer",
        BearerFormat = "JWT",
        In = ParameterLocation.Header,
        Description =
            "Enter the Keycloak access token. Swagger automatically adds the Bearer prefix."
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

await DatabaseInitializer.InitializeAsync(
    app.Services,
    app.Configuration,
    app.Lifetime.ApplicationStopping);

app.UseMiddleware<ExceptionHandlingMiddleware>();

if (app.Environment.IsDevelopment())
{
    app.UseSwagger();

    app.UseSwaggerUI(options =>
    {
        options.SwaggerEndpoint(
            "/swagger/v1/swagger.json",
            "Notification API v1");

        options.RoutePrefix = "swagger";
    });
}

app.UseAuthentication();
app.UseAuthorization();

app.MapControllers();

app.Run();

public partial class Program
{
}
