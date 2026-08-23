using System.Security.Claims;
using System.Text.Json;
using Microsoft.AspNetCore.Authentication;

namespace ApiGateway.Infrastructure.Security;

public sealed class KeycloakClaimsTransformation : IClaimsTransformation
{
    public Task<ClaimsPrincipal> TransformAsync(ClaimsPrincipal principal)
    {
        if (principal.Identity is not ClaimsIdentity identity
            || !identity.IsAuthenticated)
        {
            return Task.FromResult(principal);
        }

        var realmAccess = principal.FindFirst("realm_access")?.Value;
        if (string.IsNullOrWhiteSpace(realmAccess))
        {
            return Task.FromResult(principal);
        }

        try
        {
            using var document = JsonDocument.Parse(realmAccess);
            if (!document.RootElement.TryGetProperty("roles", out var roles)
                || roles.ValueKind != JsonValueKind.Array)
            {
                return Task.FromResult(principal);
            }

            foreach (var role in roles.EnumerateArray())
            {
                var roleName = role.GetString();
                if (!string.IsNullOrWhiteSpace(roleName)
                    && !identity.Claims.Any(
                        claim => claim.Type == ClaimTypes.Role
                            && claim.Value == roleName))
                {
                    identity.AddClaim(new Claim(ClaimTypes.Role, roleName));
                }
            }
        }
        catch (JsonException)
        {
            // A malformed optional claim must not break authentication.
        }

        return Task.FromResult(principal);
    }
}
