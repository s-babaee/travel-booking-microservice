using System.Text.Json.Serialization;
using BuildingBlocks.Authorization;
using BuildingBlocks.Messaging;
using MassTransit;
using Microsoft.EntityFrameworkCore;
using Microsoft.OpenApi;
using Payment.Api.Application.Abstractions;
using Payment.Api.Application.Services;
using Payment.Api.Infrastructure.Messaging;
using Payment.Api.Infrastructure.Persistence;
using Payment.Api.Infrastructure.Web;

var builder = WebApplication.CreateBuilder(args);

// ==========================================
// 1. Configuration & Options
// ==========================================
var paymentDbConnectionString =
    builder.Configuration.GetConnectionString("PaymentDb");

if (string.IsNullOrWhiteSpace(paymentDbConnectionString))
{
    throw new InvalidOperationException(
        "ConnectionStrings:PaymentDb was not configured.");
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
    options.UseNpgsql(paymentDbConnectionString));

builder.Services.AddScoped<IUnitOfWork>(services =>
    services.GetRequiredService<AppDbContext>());

builder.Services.AddScoped<IPaymentRepository>(services =>
    services.GetRequiredService<AppDbContext>());

builder.Services.AddScoped<IRefundRepository>(services =>
    services.GetRequiredService<AppDbContext>());

// ==========================================
// 3. Application Services & External Clients
// ==========================================
builder.Services.AddScoped<PaymentService>();
builder.Services.AddScoped<ICurrentUser, CurrentUser>();

builder.Services.AddScoped<
    IPaymentEventPublisher,
    MassTransitPaymentEventPublisher>();

builder.Services.AddSingleton<IPaymentProvider, MockPaymentProvider>();
builder.Services.AddSingleton(TimeProvider.System);

builder.Services.AddHttpContextAccessor();

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
        Title = "Payment API",
        Version = "v1",
        Description = "Payment and refund service API."
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

    app.UseSwaggerUI(options =>
    {
        options.SwaggerEndpoint(
            "/swagger/v1/swagger.json",
            "Payment API v1");

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
