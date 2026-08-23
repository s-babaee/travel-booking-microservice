using System.Security.Claims;
using ApiGateway.Infrastructure.Security;
using Microsoft.AspNetCore.Authentication;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Authentication.JwtBearer;
using Microsoft.IdentityModel.Tokens;

var builder = WebApplication.CreateBuilder(args);

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

builder.Services.AddTransient<IClaimsTransformation,
    KeycloakClaimsTransformation>();

builder.Services.AddAuthorization(options =>
{
    options.FallbackPolicy = new AuthorizationPolicyBuilder()
        .RequireAuthenticatedUser()
        .Build();

    options.AddPolicy("gateway-public", policy =>
    {
        // Public routes are explicitly opted out of the fallback policy.
        policy.RequireAssertion(_ => true);
    });

    options.AddPolicy("gateway-admin", policy =>
    {
        policy.RequireRole("admin");
    });
});

builder.Services
    .AddReverseProxy()
    .LoadFromConfig(builder.Configuration.GetSection("ReverseProxy"));

var app = builder.Build();

app.UseAuthentication();
app.UseAuthorization();

app.MapGet("/health", () =>
    Results.Ok(new
    {
        status = "ok",
        service = "api-gateway"
    }))
    .AllowAnonymous();

app.MapReverseProxy();

app.Run();

public partial class Program
{
}
