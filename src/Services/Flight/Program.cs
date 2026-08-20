using System.Text.Json.Serialization;
using BuildingBlocks.Messaging;
using Flight.Api.Application.Abstractions;
using Flight.Api.Application.Services;
using Flight.Api.Infrastructure.Messaging;
using Flight.Api.Infrastructure.Persistence;
using Flight.Api.Infrastructure.Web;
using MassTransit;
using Microsoft.EntityFrameworkCore;
using Microsoft.OpenApi;

var builder = WebApplication.CreateBuilder(args);

var flightDbConnectionString = builder.Configuration.GetConnectionString("FlightDb");
if (string.IsNullOrWhiteSpace(flightDbConnectionString))
{
    throw new InvalidOperationException(
        "ConnectionStrings:FlightDb was not configured.");
}

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
    configurator.UsingRabbitMq((context, cfg) =>
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

        cfg.Host(host, virtualHost, hostConfiguration =>
        {
            hostConfiguration.Username(username);
            hostConfiguration.Password(password);
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
        Title = "Flight Catalog API",
        Version = "v1",
        Description =
            "Flight catalog bounded context: airlines, routes, flights, schedules, classes and policies."
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

app.UseHttpsRedirection();
app.MapControllers();
app.MapGet("/health", () => Results.Ok(new { status = "ok" }));

app.Run();

public partial class Program
{
}
