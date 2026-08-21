using System.Security.Claims;
using Booking.Api.Application.Abstractions;
using Booking.Api.Application.Services;
using Booking.Api.Infrastructure.Integrations;
using Booking.Api.Infrastructure.Messaging;
using Booking.Api.Infrastructure.Persistence;
using Booking.Api.Infrastructure.Web;
using BuildingBlocks.Messaging;
using MassTransit;
using Microsoft.AspNetCore.Authentication.JwtBearer;
using Microsoft.EntityFrameworkCore;
using Microsoft.IdentityModel.Tokens;
using Microsoft.OpenApi;

var builder = WebApplication.CreateBuilder(args);

var bookingDbConnectionString =
    builder.Configuration.GetConnectionString("BookingDb");

if (string.IsNullOrWhiteSpace(bookingDbConnectionString))
{
    throw new InvalidOperationException(
        "ConnectionStrings:BookingDb was not configured.");
}

builder.Services.AddDbContext<AppDbContext>(options =>
    options.UseNpgsql(bookingDbConnectionString));

builder.Services.AddScoped<IUnitOfWork>(services =>
    services.GetRequiredService<AppDbContext>());
builder.Services.AddScoped<IBookingRepository>(services =>
    services.GetRequiredService<AppDbContext>());
builder.Services.AddScoped<IOrderRepository>(services =>
    services.GetRequiredService<AppDbContext>());
builder.Services.AddScoped<BookingService>();
builder.Services.AddScoped<ICurrentUser, CurrentUser>();
builder.Services.AddScoped<IBookingEventPublisher,
    MassTransitBookingEventPublisher>();
builder.Services.AddSingleton(TimeProvider.System);
builder.Services.AddHttpContextAccessor();

var inventoryBaseUrl = builder.Configuration["Services:InventoryBaseUrl"]
    ?? "http://localhost:5256/";
builder.Services.AddHttpClient<IInventoryGateway, InventoryHttpGateway>(
    client =>
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
    builder.Services.AddHttpClient<IPaymentGateway, PaymentHttpGateway>(
        client =>
        {
            client.BaseAddress = new Uri(paymentBaseUrl);
            client.Timeout = TimeSpan.FromSeconds(30);
        });
}

var keycloakBaseUrl = builder.Configuration["Keycloak:BaseUrl"]
    ?? "http://localhost:8081";
var keycloakRealm = builder.Configuration["Keycloak:Realm"] ?? "travel";
var keycloakAuthority =
    $"{keycloakBaseUrl.TrimEnd('/')}/realms/{keycloakRealm}";
builder.Services
    .AddAuthentication(JwtBearerDefaults.AuthenticationScheme)
    .AddJwtBearer(options =>
    {
        options.Authority = keycloakAuthority;
        options.RequireHttpsMetadata = builder.Environment.IsProduction();
        options.MapInboundClaims = false;
        options.TokenValidationParameters = new TokenValidationParameters
        {
            ValidateIssuer = true,
            ValidIssuer = keycloakAuthority,
            ValidateAudience = false,
            NameClaimType = "preferred_username",
            RoleClaimType = ClaimTypes.Role
        };
    });
builder.Services.AddAuthorization(options =>
{
    options.AddPolicy("admin", policy => policy.RequireRole("admin"));
});

builder.Services.AddEndpointsApiExplorer();
builder.Services.AddControllers()
    .AddJsonOptions(options =>
    {
        options.JsonSerializerOptions.Converters.Add(
            new System.Text.Json.Serialization.JsonStringEnumConverter());
    });
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
        Scheme = "bearer",
        BearerFormat = "JWT",
        In = ParameterLocation.Header,
        Description = "Bearer {token}"
    });
    options.AddSecurityRequirement(document =>
        new OpenApiSecurityRequirement
        {
            [new OpenApiSecuritySchemeReference("Bearer", document)] = []
        });
});

// MassTransit + RabbitMQ
builder.Services.AddMassTransit(x =>
{
    x.UsingRabbitMq((context, cfg) =>
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

        cfg.Host(host, virtualHost, h =>
        {
            h.Username(username);
            h.Password(password);
        });

        RabbitMqTopology.ConfigureMessageTopology(cfg);
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

app.UseAuthentication();
app.UseAuthorization();
app.MapControllers();
app.MapGet("/health", () => Results.Ok(new { status = "ok" }));

app.Run();


