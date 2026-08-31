using System.Security.Claims;
using System.Text.Json;
using Microsoft.AspNetCore.Authentication;
using Microsoft.AspNetCore.Authorization;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Options;

namespace BuildingBlocks.Authorization;

public sealed class HasPermissionAttribute : AuthorizeAttribute
{
    public HasPermissionAttribute(string permission)
    {
        Policy = $"{PermissionCatalog.PolicyPrefix}{PermissionCatalog.Normalize(permission)}";
    }
}

public sealed class HasAnyPermissionAttribute : AuthorizeAttribute
{
    public HasAnyPermissionAttribute(params string[] permissions)
    {
        Policy = $"{PermissionCatalog.PolicyPrefix}any:{string.Join(
            "|",
            permissions.Select(PermissionCatalog.Normalize))}";
    }
}

public sealed class PermissionRequirement(string permission) : IAuthorizationRequirement
{
    public string Permission { get; } = PermissionCatalog.Normalize(permission);
}

public sealed class AnyPermissionRequirement(
    IEnumerable<string> permissions) : IAuthorizationRequirement
{
    public IReadOnlyList<string> Permissions { get; } =
        permissions.Select(PermissionCatalog.Normalize).ToArray();
}

public sealed class PermissionAuthorizationHandler
    : IAuthorizationHandler
{
    public Task HandleAsync(AuthorizationHandlerContext context)
    {
        foreach (var requirement in context.PendingRequirements.ToArray())
        {
            IEnumerable<string> required = requirement switch
            {
                PermissionRequirement single => [single.Permission],
                AnyPermissionRequirement any => any.Permissions,
                _ => []
            };

            var granted = context.User.Claims
                .Where(claim => claim.Type == PermissionCatalog.ClaimType)
                .SelectMany(claim => ParseClaim(claim.Value))
                .Select(PermissionCatalog.Normalize)
                .ToHashSet(StringComparer.OrdinalIgnoreCase);

            if (required.Any(granted.Contains))
            {
                context.Succeed(requirement);
            }
        }

        return Task.CompletedTask;
    }

    private static IEnumerable<string> ParseClaim(string value)
    {
        if (value.StartsWith("[", StringComparison.Ordinal))
        {
            try
            {
                return JsonSerializer.Deserialize<string[]>(value) ?? [];
            }
            catch (JsonException)
            {
                return [];
            }
        }

        return value.Split(
            [',', ' '],
            StringSplitOptions.RemoveEmptyEntries | StringSplitOptions.TrimEntries);
    }
}

public sealed class PermissionPolicyProvider : IAuthorizationPolicyProvider
{
    private readonly DefaultAuthorizationPolicyProvider _fallback;

    public PermissionPolicyProvider(IOptions<AuthorizationOptions> options)
    {
        _fallback = new DefaultAuthorizationPolicyProvider(options);
    }

    public Task<AuthorizationPolicy> GetDefaultPolicyAsync() =>
        _fallback.GetDefaultPolicyAsync();

    public Task<AuthorizationPolicy?> GetFallbackPolicyAsync() =>
        _fallback.GetFallbackPolicyAsync();

    public Task<AuthorizationPolicy?> GetPolicyAsync(string policyName)
    {
        if (!policyName.StartsWith(
                PermissionCatalog.PolicyPrefix,
                StringComparison.OrdinalIgnoreCase))
        {
            return _fallback.GetPolicyAsync(policyName);
        }

        var value = policyName[PermissionCatalog.PolicyPrefix.Length..];
        IAuthorizationRequirement requirement = value.StartsWith(
            "any:",
            StringComparison.OrdinalIgnoreCase)
            ? new AnyPermissionRequirement(value[4..].Split('|'))
            : new PermissionRequirement(value);
        var policy = new AuthorizationPolicyBuilder()
            .RequireAuthenticatedUser()
            .AddRequirements(requirement)
            .Build();
        return Task.FromResult<AuthorizationPolicy?>(policy);
    }
}

public sealed class PermissionClaimsTransformation : IClaimsTransformation
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
            if (document.RootElement.TryGetProperty("roles", out var roles)
                && roles.ValueKind == JsonValueKind.Array)
            {
                foreach (var role in roles.EnumerateArray())
                {
                    var roleName = role.GetString();
                    if (string.IsNullOrWhiteSpace(roleName)
                        || !roleName.StartsWith(
                            PermissionCatalog.PolicyPrefix,
                            StringComparison.OrdinalIgnoreCase))
                    {
                        continue;
                    }

                    var permission = PermissionCatalog.Normalize(
                        roleName[PermissionCatalog.PolicyPrefix.Length..]);
                    if (PermissionCatalog.All.Contains(permission)
                        && !identity.Claims.Any(claim =>
                            claim.Type == PermissionCatalog.ClaimType
                            && string.Equals(
                                PermissionCatalog.Normalize(claim.Value),
                                permission,
                                StringComparison.OrdinalIgnoreCase)))
                    {
                        identity.AddClaim(new Claim(
                            PermissionCatalog.ClaimType,
                            permission));
                    }
                }
            }
        }
        catch (JsonException)
        {
            // A malformed optional Keycloak claim must not break authentication.
        }

        return Task.FromResult(principal);
    }
}

public static class PermissionAuthorizationExtensions
{
    public static bool HasPermission(
        this ClaimsPrincipal principal,
        string permission)
    {
        var required = PermissionCatalog.Normalize(permission);
        return principal.Claims
            .Where(claim => claim.Type == PermissionCatalog.ClaimType)
            .SelectMany(claim => claim.Value.Split(
                [',', ' ', '[', ']', '"'],
                StringSplitOptions.RemoveEmptyEntries | StringSplitOptions.TrimEntries))
            .Select(PermissionCatalog.Normalize)
            .Contains(required, StringComparer.OrdinalIgnoreCase);
    }

    public static IServiceCollection AddPermissionAuthorization(
        this IServiceCollection services,
        Action<AuthorizationOptions>? configure = null)
    {
        services.AddTransient<IClaimsTransformation, PermissionClaimsTransformation>();
        services.AddSingleton<IAuthorizationHandler, PermissionAuthorizationHandler>();
        services.AddSingleton<IAuthorizationPolicyProvider, PermissionPolicyProvider>();
        services.AddAuthorization(configure ?? (_ => { }));
        return services;
    }
}
