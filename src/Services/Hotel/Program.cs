using BuildingBlocks.Authorization;
using BuildingBlocks.Messaging;
using Hotel.Api.Application.Abstractions;
using Hotel.Api.Application.Services;
using Hotel.Api.Application.Storage;
using Hotel.Api.Infrastructure.Messaging;
using Hotel.Api.Infrastructure.Persistence;
using Hotel.Api.Infrastructure.Storage;
using Hotel.Api.Infrastructure.Web;
using MassTransit;
using Microsoft.EntityFrameworkCore;
using Microsoft.OpenApi;
using System.Text.Json.Serialization;

var builder = WebApplication.CreateBuilder(args);

// ==========================================
// 1. Configuration & Options
// ==========================================
var hotelDbConnectionString = builder.Configuration.GetConnectionString("HotelDb");
if (string.IsNullOrWhiteSpace(hotelDbConnectionString))
{
    throw new InvalidOperationException(
        "ConnectionStrings:HotelDb was not configured.");
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

// ==========================================
// 3. Application Services & External Clients
// ==========================================
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
        Title = "Hotel Catalog API",
        Version = "v1",
        Description = "Hotel catalog bounded context: hotels, room types, amenities, policies and images."
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
app.UseStaticFiles();
app.MapControllers();

app.Run();

public partial class Program
{
}
