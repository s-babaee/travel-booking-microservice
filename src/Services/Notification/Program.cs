using System.Security.Claims;
using BuildingBlocks.Contracts.Events;
using BuildingBlocks.Contracts.Messaging;
using BuildingBlocks.Messaging;
using MassTransit;
using Microsoft.AspNetCore.Authentication.JwtBearer;
using Microsoft.EntityFrameworkCore;
using Microsoft.IdentityModel.Tokens;
using Microsoft.OpenApi;
using Notification.Application.Abstractions;
using Notification.Application.Services;
using Notification.Infrastructure.Messaging;
using Notification.Infrastructure.Persistence;
using Notification.Infrastructure.Web;

var builder = WebApplication.CreateBuilder(args);

// -------------------- Database --------------------

var notificationDbConnectionString =
    builder.Configuration.GetConnectionString("NotificationDb");

if (string.IsNullOrWhiteSpace(notificationDbConnectionString))
{
    throw new InvalidOperationException(
        "ConnectionStrings:NotificationDb was not configured.");
}

builder.Services.AddDbContext<AppDbContext>(options =>
    options.UseNpgsql(notificationDbConnectionString));

builder.Services.AddScoped<IUnitOfWork>(services =>
    services.GetRequiredService<AppDbContext>());

builder.Services.AddScoped<INotificationRepository>(services =>
    services.GetRequiredService<AppDbContext>());

builder.Services.AddScoped<INotificationTemplateRepository>(services =>
    services.GetRequiredService<AppDbContext>());

// -------------------- Application Services --------------------

builder.Services.AddScoped<NotificationService>();

builder.Services.AddScoped<INotificationEventHandler>(services =>
    services.GetRequiredService<NotificationService>());

builder.Services.AddScoped<ICurrentUser, CurrentUser>();

builder.Services.AddSingleton(TimeProvider.System);

builder.Services.AddHttpContextAccessor();

// -------------------- Authentication / Keycloak --------------------

var keycloakBaseUrl = builder.Configuration["Keycloak:BaseUrl"]
    ?? "http://localhost:8081";

var keycloakRealm = builder.Configuration["Keycloak:Realm"]
    ?? "travel";

var keycloakAuthority =
    $"{keycloakBaseUrl.TrimEnd('/')}/realms/{keycloakRealm}";

builder.Services
    .AddAuthentication(JwtBearerDefaults.AuthenticationScheme)
    .AddJwtBearer(options =>
    {
        options.Authority = keycloakAuthority;

        // در Production باید Keycloak با HTTPS در دسترس باشد.
        options.RequireHttpsMetadata = builder.Environment.IsProduction();

        // claimها همان نام اصلی Keycloak را حفظ می‌کنند.
        options.MapInboundClaims = false;

        options.TokenValidationParameters = new TokenValidationParameters
        {
            ValidateIssuer = true,
            ValidIssuer = keycloakAuthority,

            // اگر Keycloak audience را تنظیم کردی، این مقدار را true کن.
            ValidateAudience = false,

            NameClaimType = "preferred_username",
            RoleClaimType = ClaimTypes.Role
        };
    });

builder.Services.AddAuthorization(options =>
{
    options.AddPolicy("admin", policy => policy.RequireRole("admin"));
});

// -------------------- Controllers / JSON --------------------

builder.Services.AddControllers()
    .AddJsonOptions(options =>
    {
        // Enumها در response به‌شکل string برگردند، نه عدد.
        options.JsonSerializerOptions.Converters.Add(
            new System.Text.Json.Serialization.JsonStringEnumConverter());
    });

// Swagger Controller endpointها را شناسایی می‌کند.
builder.Services.AddEndpointsApiExplorer();

// -------------------- Swagger --------------------

builder.Services.AddSwaggerGen(options =>
{
    options.SwaggerDoc("v1", new OpenApiInfo
    {
        Title = "Notification API",
        Version = "v1",
        Description =
            "Notification service API for managing notifications and templates."
    });

    // تنظیم JWT Bearer برای دکمه Authorize در Swagger
    options.AddSecurityDefinition("Bearer", new OpenApiSecurityScheme
    {
        Name = "Authorization",
        Type = SecuritySchemeType.Http,
        Scheme = "bearer",
        BearerFormat = "JWT",
        In = ParameterLocation.Header,
        Description =
            "توکن Keycloak را وارد کنید. فقط خود JWT را وارد کنید؛ " +
            "Swagger هدر Bearer را خودکار اضافه می‌کند."
    });

    // ارسال JWT برای endpointهای محافظت‌شده
    options.AddSecurityRequirement(document =>
        new OpenApiSecurityRequirement
        {
            [new OpenApiSecuritySchemeReference("Bearer", document)] = []
        });
});

// -------------------- RabbitMQ / MassTransit --------------------

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
    // Consumerهای رویدادهای Booking
    configurator.AddConsumer<BookingConfirmedConsumer>();
    configurator.AddConsumer<BookingFailedConsumer>();
    configurator.AddConsumer<BookingCancellationStartedConsumer>();
    configurator.AddConsumer<BookingCancelledConsumer>();

    // Consumerهای رویدادهای Payment
    configurator.AddConsumer<PaymentAuthorizedConsumer>();
    configurator.AddConsumer<PaymentFailedConsumer>();
    configurator.AddConsumer<PaymentRefundedConsumer>();

    configurator.UsingRabbitMq((context, cfg) =>
    {
        cfg.Host(brokerHost, brokerVirtualHost, host =>
        {
            host.Username(brokerUsername);
            host.Password(brokerPassword);
        });

        RabbitMqTopology.ConfigureMessageTopology(cfg);

        cfg.ReceiveEndpoint(EventQueueNames.NotificationEvents, endpoint =>
        {
            // چون exchangeها را دستی bind می‌کنیم،
            // MassTransit خودش topology پیش‌فرض ایجاد نکند.
            endpoint.ConfigureConsumeTopology = false;

            // اتصال صف Notification به exchangeهای رویدادها
            endpoint.Bind(EventExchangeNames.BookingConfirmed);
            endpoint.Bind(EventExchangeNames.BookingFailed);
            endpoint.Bind(EventExchangeNames.BookingCancellationStarted);
            endpoint.Bind(EventExchangeNames.BookingCancelled);
            endpoint.Bind(EventExchangeNames.PaymentAuthorized);
            endpoint.Bind(EventExchangeNames.PaymentFailed);
            endpoint.Bind(EventExchangeNames.PaymentRefunded);

            // اتصال consumerها به همان endpoint
            endpoint.ConfigureConsumer<BookingConfirmedConsumer>(context);
            endpoint.ConfigureConsumer<BookingFailedConsumer>(context);
            endpoint.ConfigureConsumer<BookingCancellationStartedConsumer>(
                context);
            endpoint.ConfigureConsumer<BookingCancelledConsumer>(context);

            endpoint.ConfigureConsumer<PaymentAuthorizedConsumer>(context);
            endpoint.ConfigureConsumer<PaymentFailedConsumer>(context);
            endpoint.ConfigureConsumer<PaymentRefundedConsumer>(context);
        });
    });
});

// -------------------- Build --------------------

var app = builder.Build();

await DatabaseInitializer.InitializeAsync(
    app.Services,
    app.Configuration,
    app.Lifetime.ApplicationStopping);

// Middleware مدیریت Exception
app.UseMiddleware<ExceptionHandlingMiddleware>();

// -------------------- Swagger UI --------------------

if (app.Environment.IsDevelopment())
{
    // OpenAPI JSON:
    // /swagger/v1/swagger.json
    app.UseSwagger();

    // Swagger UI:
    // /swagger
    app.UseSwaggerUI(options =>
    {
        options.SwaggerEndpoint(
            "/swagger/v1/swagger.json",
            "Notification API v1");

        options.RoutePrefix = "swagger";
    });
}

// -------------------- Request Pipeline --------------------

app.UseAuthentication();
app.UseAuthorization();

app.MapControllers();

app.MapGet("/health", () =>
    Results.Ok(new { status = "ok" }));

app.Run();

public partial class Program
{
}
