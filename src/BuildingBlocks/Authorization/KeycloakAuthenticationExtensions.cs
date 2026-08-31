using System.Security.Claims;
using Microsoft.AspNetCore.Authentication.JwtBearer;
using Microsoft.AspNetCore.Authentication;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;
using Microsoft.IdentityModel.Tokens;

namespace BuildingBlocks.Authorization;

public static class KeycloakAuthenticationExtensions
{
    public static AuthenticationBuilder AddKeycloakJwt(
        this IServiceCollection services,
        IConfiguration configuration,
        IHostEnvironment environment)
    {
        var baseUrl = configuration["Keycloak:BaseUrl"]
            ?? "http://localhost:8081";
        var realm = configuration["Keycloak:Realm"] ?? "travel";
        var authority = $"{baseUrl.TrimEnd('/')}/realms/{realm}";

        return services
            .AddAuthentication(JwtBearerDefaults.AuthenticationScheme)
            .AddJwtBearer(options =>
            {
                options.Authority = authority;
                options.RequireHttpsMetadata = environment.IsProduction();
                options.MapInboundClaims = false;
                options.TokenValidationParameters = new TokenValidationParameters
                {
                    ValidateIssuer = true,
                    ValidIssuer = authority,
                    ValidateAudience = false,
                    NameClaimType = "preferred_username",
                    RoleClaimType = ClaimTypes.Role
                };
            });
    }
}
