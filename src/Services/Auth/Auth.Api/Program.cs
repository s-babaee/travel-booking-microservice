using Auth.Api.Application.Abstractions;
using Auth.Api.Application.Services;
using Auth.Api.Infrastructure.Keycloak;
using Auth.Api.Infrastructure.Persistence;
using Auth.Api.Infrastructure.Security;
using Auth.Api.Infrastructure.Web;
using Microsoft.AspNetCore.Authentication.JwtBearer;
using Microsoft.EntityFrameworkCore;
using Microsoft.IdentityModel.Tokens;
using Microsoft.OpenApi;
using Npgsql;
using System.Security.Claims;

var builder = WebApplication.CreateBuilder(args);

var authDbConnectionString =
    builder.Configuration.GetConnectionString("AuthDb");

if (string.IsNullOrWhiteSpace(authDbConnectionString))
{
    throw new InvalidOperationException(
        "ConnectionStrings:AuthDb پیدا نشد.");
}

var csb = new NpgsqlConnectionStringBuilder(authDbConnectionString);

Console.WriteLine(
    $"DB => Host={csb.Host}; Port={csb.Port}; " +
    $"Database={csb.Database}; Username={csb.Username}; " +
    $"PasswordLength={csb.Password?.Length ?? 0}");

try
{
    await using var connection =
        new NpgsqlConnection(authDbConnectionString);

    await connection.OpenAsync();

    await using var command = new NpgsqlCommand(
        "SELECT current_user, current_database(), inet_server_port();",
        connection);

    await using var reader = await command.ExecuteReaderAsync();
    await reader.ReadAsync();

    Console.WriteLine(
        $"✅ PostgreSQL connected => " +
        $"User={reader.GetString(0)}, " +
        $"Database={reader.GetString(1)}, " +
        $"ServerPort={reader.GetInt32(2)}");
}
catch (Exception exception)
{
    Console.WriteLine(
        $"❌ Direct Npgsql failed => {exception.Message}");

    throw;
}

builder.Services.Configure<KeycloakOptions>(
    builder.Configuration.GetSection("Keycloak"));
builder.Services.Configure<AuthOptions>(
    builder.Configuration.GetSection("Auth"));

var keycloakOptions = builder.Configuration
    .GetSection("Keycloak")
    .Get<KeycloakOptions>() ?? new KeycloakOptions();

if (string.IsNullOrWhiteSpace(keycloakOptions.ClientSecret)
    || string.IsNullOrWhiteSpace(keycloakOptions.AdminPassword))
{
    throw new InvalidOperationException(
        "Keycloak:ClientSecret and Keycloak:AdminPassword must be configured.");
}

var keycloakAuthority =
    $"{keycloakOptions.BaseUrl.TrimEnd('/')}/realms/{keycloakOptions.Realm}";

builder.Services.AddDbContext<AppDbContext>(options =>
    options.UseNpgsql(builder.Configuration.GetConnectionString("AuthDb")));

builder.Services.AddScoped<IUnitOfWork>(services =>
    services.GetRequiredService<AppDbContext>());
builder.Services.AddScoped<IUserRepository>(services =>
    services.GetRequiredService<AppDbContext>());
builder.Services.AddScoped<IRoleRepository>(services =>
    services.GetRequiredService<AppDbContext>());
builder.Services.AddScoped<IPermissionRepository>(services =>
    services.GetRequiredService<AppDbContext>());
builder.Services.AddScoped<IUserRoleRepository>(services =>
    services.GetRequiredService<AppDbContext>());
builder.Services.AddScoped<IRolePermissionRepository>(services =>
    services.GetRequiredService<AppDbContext>());
builder.Services.AddScoped<IPasswordResetTokenRepository>(services =>
    services.GetRequiredService<AppDbContext>());

builder.Services.AddHttpClient<IIdentityProvider, KeycloakIdentityProvider>(
    (services, client) =>
    {
        var options = services.GetRequiredService<
            Microsoft.Extensions.Options.IOptions<KeycloakOptions>>().Value;

        client.BaseAddress = new Uri($"{options.BaseUrl.TrimEnd('/')}/");
        client.Timeout = TimeSpan.FromSeconds(30);
    });

builder.Services.AddSingleton(TimeProvider.System);
builder.Services.AddScoped<IAuthService, AuthService>();
builder.Services.AddScoped<IUserService, UserService>();
builder.Services.AddScoped<IRoleService, RoleService>();
builder.Services.AddScoped<IPermissionService, PermissionService>();
builder.Services.AddScoped<IPasswordService, PasswordService>();

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

builder.Services.AddTransient<
    Microsoft.AspNetCore.Authentication.IClaimsTransformation,
    KeycloakClaimsTransformation>();

builder.Services.AddAuthorization(options =>
{
    options.AddPolicy("admin", policy => policy.RequireRole("admin"));
});

builder.Services.AddControllers();

builder.Services.AddEndpointsApiExplorer();

builder.Services.AddSwaggerGen(options =>
{
    options.SwaggerDoc("v1", new OpenApiInfo
    {
        Title = "Auth API",
        Version = "v1",
        Description = "Authentication and Authorization service API."
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

var app = builder.Build();

await DatabaseInitializer.InitializeAsync(app.Services);

app.UseMiddleware<ApiExceptionMiddleware>();

if (app.Environment.IsDevelopment())
{
    app.UseSwagger();

    app.UseSwaggerUI(options =>
    {
        options.SwaggerEndpoint("/swagger/v1/swagger.json", "Auth API v1");
        options.RoutePrefix = "swagger";
    });
}

app.UseAuthentication();
app.UseAuthorization();

app.MapControllers();
app.MapGet("/health", () => Results.Ok(new { status = "ok" }));

app.Run();

public partial class Program
{
}
