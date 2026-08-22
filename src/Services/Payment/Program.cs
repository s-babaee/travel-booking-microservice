using System.Security.Claims;
using BuildingBlocks.Messaging;
using MassTransit;
using Microsoft.AspNetCore.Authentication.JwtBearer;
using Microsoft.EntityFrameworkCore;
using Microsoft.IdentityModel.Tokens;
using Microsoft.OpenApi;
using Payment.Api.Application.Abstractions;
using Payment.Api.Application.Services;
using Payment.Api.Infrastructure.Messaging;
using Payment.Api.Infrastructure.Persistence;
using Payment.Api.Infrastructure.Web;

var builder = WebApplication.CreateBuilder(args);

// -------------------- Database --------------------

var paymentDbConnectionString =
    builder.Configuration.GetConnectionString("PaymentDb");

if (string.IsNullOrWhiteSpace(paymentDbConnectionString))
{
    throw new InvalidOperationException(
        "ConnectionStrings:PaymentDb was not configured.");
}

builder.Services.AddDbContext<AppDbContext>(options =>
    options.UseNpgsql(paymentDbConnectionString));

builder.Services.AddScoped<IUnitOfWork>(services =>
    services.GetRequiredService<AppDbContext>());

builder.Services.AddScoped<IPaymentRepository>(services =>
    services.GetRequiredService<AppDbContext>());

builder.Services.AddScoped<IRefundRepository>(services =>
    services.GetRequiredService<AppDbContext>());

// -------------------- Application Services --------------------

builder.Services.AddScoped<PaymentService>();
builder.Services.AddScoped<ICurrentUser, CurrentUser>();

builder.Services.AddScoped<IPaymentEventPublisher,
    MassTransitPaymentEventPublisher>();

builder.Services.AddSingleton<IPaymentProvider, MockPaymentProvider>();
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

        // در محیط Production بهتر است Keycloak با HTTPS باشد.
        options.RequireHttpsMetadata = builder.Environment.IsProduction();

        // claimها را بدون تبدیل پیش‌فرض دات‌نت نگه می‌دارد.
        options.MapInboundClaims = false;

        options.TokenValidationParameters = new TokenValidationParameters
        {
            ValidateIssuer = true,
            ValidIssuer = keycloakAuthority,

            // فعلاً Audience بررسی نمی‌شود.
            // اگر در Keycloak audience را تنظیم کردی، بهتر است true شود.
            ValidateAudience = false,

            NameClaimType = "preferred_username",
            RoleClaimType = ClaimTypes.Role
        };
    });

builder.Services.AddAuthorization();

// -------------------- Controllers / JSON --------------------

builder.Services.AddControllers()
    .AddJsonOptions(options =>
    {
        // enumها در خروجی JSON به شکل متن برگردند، نه عدد.
        options.JsonSerializerOptions.Converters.Add(
            new System.Text.Json.Serialization.JsonStringEnumConverter());
    });

// برای کشف Controller endpointها توسط Swagger لازم است.
builder.Services.AddEndpointsApiExplorer();

// -------------------- Swagger --------------------

builder.Services.AddSwaggerGen(options =>
{
    options.SwaggerDoc("v1", new OpenApiInfo
    {
        Title = "Payment API",
        Version = "v1",
        Description = "Payment and refund service API."
    });

    // تعریف JWT Bearer برای دکمه Authorize در Swagger
    options.AddSecurityDefinition("Bearer", new OpenApiSecurityScheme
    {
        Name = "Authorization",
        Type = SecuritySchemeType.Http,
        Scheme = "bearer",
        BearerFormat = "JWT",
        In = ParameterLocation.Header,
        Description =
            "توکن Keycloak را وارد کنید. فقط خود JWT را وارد کنید؛ " +
            "Swagger به‌صورت خودکار Bearer را اضافه می‌کند."
    });

    // اعمال JWT روی تمام endpointهای Swagger
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
    configurator.UsingRabbitMq((_, cfg) =>
    {
        cfg.Host(brokerHost, brokerVirtualHost, host =>
        {
            host.Username(brokerUsername);
            host.Password(brokerPassword);
        });

        RabbitMqTopology.ConfigureMessageTopology(cfg);
    });
});

// -------------------- Build --------------------

var app = builder.Build();

// اجرای migration و seed احتمالی دیتابیس
await DatabaseInitializer.InitializeAsync(
    app.Services,
    app.Configuration.GetValue("Database:ApplyMigrations", true));

// Middleware مدیریت exceptionهای برنامه
app.UseMiddleware<ExceptionHandlingMiddleware>();

// -------------------- Swagger UI --------------------

if (app.Environment.IsDevelopment())
{
    // فایل OpenAPI JSON:
    // /swagger/v1/swagger.json
    app.UseSwagger();

    // رابط گرافیکی Swagger:
    // /swagger
    app.UseSwaggerUI(options =>
    {
        options.SwaggerEndpoint(
            "/swagger/v1/swagger.json",
            "Payment API v1");

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
