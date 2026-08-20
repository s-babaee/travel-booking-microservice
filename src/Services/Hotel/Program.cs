using System.Text.Json.Serialization;
using Hotel.Api.Application.Abstractions;
using Hotel.Api.Application.Storage;
using Hotel.Api.Application.Services;
using Hotel.Api.Infrastructure.Messaging;
using Hotel.Api.Infrastructure.Persistence;
using Hotel.Api.Infrastructure.Storage;
using Hotel.Api.Infrastructure.Web;
using MassTransit;
using Microsoft.EntityFrameworkCore;
using Microsoft.OpenApi;

var builder = WebApplication.CreateBuilder(args);

var hotelDbConnectionString = builder.Configuration.GetConnectionString("HotelDb");
if (string.IsNullOrWhiteSpace(hotelDbConnectionString))
{
    throw new InvalidOperationException(
        "ConnectionStrings:HotelDb was not configured.");
}

builder.Services.AddDbContext<AppDbContext>(options =>
    options.UseNpgsql(hotelDbConnectionString));

builder.Services.AddScoped<IUnitOfWork>(services =>
    services.GetRequiredService<AppDbContext>());
builder.Services.AddScoped<IHotelRepository>(services =>
    services.GetRequiredService<AppDbContext>());
builder.Services.AddScoped<IRoomTypeRepository>(services =>
    services.GetRequiredService<AppDbContext>());
builder.Services.AddScoped<IAmenityRepository>(services =>
    services.GetRequiredService<AppDbContext>());
builder.Services.AddScoped<IHotelAmenityRepository>(services =>
    services.GetRequiredService<AppDbContext>());
builder.Services.AddScoped<IRoomTypeAmenityRepository>(services =>
    services.GetRequiredService<AppDbContext>());
builder.Services.AddScoped<IHotelPolicyRepository>(services =>
    services.GetRequiredService<AppDbContext>());
builder.Services.AddScoped<IHotelImageRepository>(services =>
    services.GetRequiredService<AppDbContext>());
builder.Services.AddScoped<IRoomTypeImageRepository>(services =>
    services.GetRequiredService<AppDbContext>());

builder.Services.AddSingleton(TimeProvider.System);
builder.Services.Configure<ImageStorageOptions>(
    builder.Configuration.GetSection("Storage"));
builder.Services.AddSingleton<IImageStorage, LocalImageStorage>();
builder.Services.AddScoped<IIntegrationEventPublisher, MassTransitEventPublisher>();
builder.Services.AddScoped<IHotelService, HotelService>();
builder.Services.AddScoped<IRoomTypeService, RoomTypeService>();
builder.Services.AddScoped<IAmenityService, AmenityService>();
builder.Services.AddScoped<IHotelPolicyService, HotelPolicyService>();
builder.Services.AddScoped<IHotelImageService, HotelImageService>();

var brokerHost = builder.Configuration["MessageBroker:Host"] ?? "localhost";
var brokerVirtualHost = builder.Configuration["MessageBroker:VirtualHost"] ?? "/";
var brokerUsername = builder.Configuration["MessageBroker:Username"] ?? "guest";
var brokerPassword = builder.Configuration["MessageBroker:Password"] ?? "guest";

builder.Services.AddMassTransit(configurator =>
{
    configurator.UsingRabbitMq((_, cfg) =>
    {
        cfg.Host(brokerHost, brokerVirtualHost, host =>
        {
            host.Username(brokerUsername);
            host.Password(brokerPassword);
        });
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
        Title = "Hotel Catalog API",
        Version = "v1",
        Description =
            "Hotel catalog bounded context: hotels, room types, amenities, policies and images."
    });
});

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

app.UseHttpsRedirection();
app.UseStaticFiles();
app.MapControllers();
app.MapGet("/health", () => Results.Ok(new { status = "ok" }));

app.Run();

public partial class Program
{
}
