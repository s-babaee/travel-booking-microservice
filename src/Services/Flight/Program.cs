using BuildingBlocks.Authorization;
using BuildingBlocks.Messaging;
using Flight.Api.Application.Abstractions;
using Flight.Api.Application.Services;
using Flight.Api.Infrastructure.Messaging;
using Flight.Api.Infrastructure.Persistence;
using Flight.Api.Infrastructure.Web;
using MassTransit;
using Microsoft.EntityFrameworkCore;
using Microsoft.OpenApi;
using System.Text.Json.Serialization;

var builder = WebApplication.CreateBuilder(args);

// ==========================================
// 1. Configuration & Options
// ==========================================
var flightDbConnectionString = builder.Configuration.GetConnectionString("FlightDb");
if (string.IsNullOrWhiteSpace(flightDbConnectionString))
{
    throw new InvalidOperationException(
        "ConnectionStrings:FlightDb was not configured.");
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
    options.UseNpgsql(flightDbConnectionString));

builder.Services.AddScoped<IUnitOfWork>(services =>
    services.GetRequiredService<AppDbContext>());

builder.Services.AddScoped<IAirlineRepository>(services =>
    services.GetRequiredService<AppDbContext>());
builder.Services.AddScoped<IRouteRepository>(services =>
    services.GetRequiredService<AppDbContext>());
builder.Services.AddScoped<IFlightRepository>(services =>
    services.GetRequiredService<AppDbContext>());
builder.Services.AddScoped<IFlightScheduleRepository>(services =>
    services.GetRequiredService<AppDbContext>());
builder.Services.AddScoped<IFlightClassRepository>(services =>
    services.GetRequiredService<AppDbContext>());
builder.Services.AddScoped<IFlightPolicyRepository>(services =>
    services.GetRequiredService<AppDbContext>());

// ==========================================
// 3. Application Services & External Clients
// ==========================================
builder.Services.AddSingleton(TimeProvider.System);

builder.Services.AddScoped<IIntegrationEventPublisher, MassTransitEventPublisher>();

builder.Services.AddScoped<IAirlineService, AirlineService>();
builder.Services.AddScoped<IRouteService, RouteService>();
builder.Services.AddScoped<IFlightService, FlightService>();
builder.Services.AddScoped<IFlightScheduleService, FlightScheduleService>();
builder.Services.AddScoped<IFlightClassService, FlightClassService>();
builder.Services.AddScoped<IFlightPolicyService, FlightPolicyService>();

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
        Title = "Flight Catalog API",
        Version = "v1",
        Description = "Flight catalog bounded context: airlines, routes, flights, schedules, classes and policies."
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
