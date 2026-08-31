using System.IdentityModel.Tokens.Jwt;
using System.Net;
using System.Net.Http.Headers;
using System.Net.Http.Json;
using System.Text.Json;
using System.Text.Json.Serialization;
using Auth.Api.Application.Abstractions;
using Auth.Api.Application.Contracts;
using Auth.Api.Application.Exceptions;
using Auth.Api.Domain.Entities;
using Microsoft.Extensions.Options;

namespace Auth.Api.Infrastructure.Keycloak;

public sealed class KeycloakIdentityProvider : IIdentityProvider
{
    private readonly HttpClient _httpClient;
    private readonly KeycloakOptions _options;
    private readonly SemaphoreSlim _adminTokenLock = new(1, 1);
    private string? _adminAccessToken;
    private DateTime _adminTokenExpiresAtUtc;

    public KeycloakIdentityProvider(HttpClient httpClient, IOptions<KeycloakOptions> options)
    {
        _httpClient = httpClient;
        _options = options.Value;
    }

    public async Task<ExternalUser> CreateUserAsync(
    RegisterCommand command,
    CancellationToken cancellationToken)
    {
        var username = command.Username.Trim();
        var email = command.Email.Trim().ToLowerInvariant();
        var firstName = command.FirstName?.Trim();
        var lastName = command.LastName?.Trim();

        var body = new
        {
            username,
            email,
            firstName,
            lastName,
            enabled = true,
            emailVerified = true,
            credentials = new[]
            {
            new
            {
                type = "password",
                value = command.Password,
                temporary = false
            }
        }
        };

        using var response = await SendAdminAsync(
            HttpMethod.Post,
            AdminPath("users"),
            body,
            cancellationToken);

        if (response.StatusCode != HttpStatusCode.Created)
        {
            throw await CreateExternalExceptionAsync(response);
        }

        var location = response.Headers.Location;

        if (location is null)
        {
            throw new InvalidOperationException(
                "Keycloak created the user but did not return a Location header.");
        }

        var keycloakUserIdText = location.Segments.Last().TrimEnd('/');

        if (!Guid.TryParse(keycloakUserIdText, out var keycloakUserId))
        {
            throw new InvalidOperationException(
                $"Could not parse Keycloak user ID from Location header: {location}");
        }

        return new ExternalUser(
            keycloakUserId,
            username,
            email,
            firstName,
            lastName,
            true);
    }


    public async Task<AuthTokenResponse> LoginAsync(
        LoginCommand command,
        CancellationToken cancellationToken)
    {
        var token = await RequestTokenAsync(
            new Dictionary<string, string>
            {
                ["grant_type"] = "password",
                ["client_id"] = _options.ClientId,
                ["client_secret"] = _options.ClientSecret,
                ["username"] = command.Username,
                ["password"] = command.Password,
                ["scope"] = "openid"
            },
            cancellationToken);

        return token.ToApplicationResponse();
    }

    public async Task<AuthTokenResponse> RefreshTokenAsync(
        RefreshTokenCommand command,
        CancellationToken cancellationToken)
    {
        var token = await RequestTokenAsync(
            new Dictionary<string, string>
            {
                ["grant_type"] = "refresh_token",
                ["client_id"] = _options.ClientId,
                ["client_secret"] = _options.ClientSecret,
                ["refresh_token"] = command.RefreshToken
            },
            cancellationToken);

        return token.ToApplicationResponse();
    }

    public async Task LogoutAsync(
        LogoutCommand command,
        CancellationToken cancellationToken)
    {
        using var response = await _httpClient.PostAsync(
            RealmPath("protocol/openid-connect/logout"),
            new FormUrlEncodedContent(
                new Dictionary<string, string>
                {
                    ["client_id"] = _options.ClientId,
                    ["client_secret"] = _options.ClientSecret,
                    ["refresh_token"] = command.RefreshToken
                }),
            cancellationToken);

        await EnsureSuccessAsync(response);
    }

    public async Task<TokenValidationResponse> ValidateTokenAsync(
        ValidateTokenCommand command,
        CancellationToken cancellationToken)
    {
        using var response = await _httpClient.PostAsync(
            RealmPath("protocol/openid-connect/token/introspect"),
            new FormUrlEncodedContent(
                new Dictionary<string, string>
                {
                    ["client_id"] = _options.ClientId,
                    ["client_secret"] = _options.ClientSecret,
                    ["token"] = command.AccessToken
                }),
            cancellationToken);

        await EnsureSuccessAsync(response);
        var introspection = await response.Content.ReadFromJsonAsync<IntrospectionResponse>(
            cancellationToken: cancellationToken);

        if (introspection is null || !introspection.Active)
        {
            return new TokenValidationResponse(false, null, null);
        }

        Guid? userId = Guid.TryParse(introspection.Subject, out var parsedUserId)
            ? parsedUserId
            : null;
        return new TokenValidationResponse(true, userId, introspection.Username);
    }

    public async Task<ExternalUser?> GetUserAsync(
        Guid userId,
        CancellationToken cancellationToken)
    {
        using var response = await SendAdminAsync(
            HttpMethod.Get,
            AdminPath($"users/{userId}"),
            null,
            cancellationToken);

        if (response.StatusCode == HttpStatusCode.NotFound)
        {
            return null;
        }

        await EnsureSuccessAsync(response);
        var user = await response.Content.ReadFromJsonAsync<KeycloakUser>(
            cancellationToken: cancellationToken);
        return user?.ToExternalUser();
    }

    public async Task UpdateUserAsync(
        Guid userId,
        UpdateUserCommand command,
        CancellationToken cancellationToken)
    {
        var existing = await GetUserAsync(userId, cancellationToken)
            ?? throw new ExternalServiceException("Keycloak user was not found.", 404);

        using var response = await SendAdminAsync(
            HttpMethod.Put,
            AdminPath($"users/{userId}"),
            new
            {
                id = userId.ToString(),
                username = existing.Username,
                email = command.Email.Trim().ToLowerInvariant(),
                firstName = command.FirstName?.Trim(),
                lastName = command.LastName?.Trim(),
                enabled = existing.Enabled,
                emailVerified = true
            },
            cancellationToken);
        await EnsureSuccessAsync(response);
    }

    public async Task SetUserEnabledAsync(
        Guid userId,
        bool enabled,
        CancellationToken cancellationToken)
    {
        var existing = await GetUserAsync(userId, cancellationToken)
            ?? throw new ExternalServiceException("Keycloak user was not found.", 404);

        using var response = await SendAdminAsync(
            HttpMethod.Put,
            AdminPath($"users/{userId}"),
            new
            {
                id = userId.ToString(),
                username = existing.Username,
                email = existing.Email,
                firstName = existing.FirstName,
                lastName = existing.LastName,
                enabled,
                emailVerified = true
            },
            cancellationToken);
        await EnsureSuccessAsync(response);
    }

    public async Task DeleteUserAsync(Guid userId, CancellationToken cancellationToken)
    {
        using var response = await SendAdminAsync(
            HttpMethod.Delete,
            AdminPath($"users/{userId}"),
            null,
            cancellationToken);
        await EnsureSuccessAsync(response);
    }

    public async Task ChangePasswordAsync(
        Guid userId,
        string newPassword,
        CancellationToken cancellationToken)
    {
        using var response = await SendAdminAsync(
            HttpMethod.Put,
            AdminPath($"users/{userId}/reset-password"),
            new
            {
                type = "password",
                value = newPassword,
                temporary = false
            },
            cancellationToken);
        await EnsureSuccessAsync(response);
    }

    public async Task<bool> ValidateCredentialsAsync(
        string username,
        string password,
        CancellationToken cancellationToken)
    {
        try
        {
            _ = await RequestTokenAsync(
                new Dictionary<string, string>
                {
                    ["grant_type"] = "password",
                    ["client_id"] = _options.ClientId,
                    ["client_secret"] = _options.ClientSecret,
                    ["username"] = username,
                    ["password"] = password
                },
                cancellationToken);
            return true;
        }
        catch (ExternalServiceException exception) when (exception.StatusCode == 400)
        {
            return false;
        }
    }

    public Task CreateRoleAsync(Role role, CancellationToken cancellationToken)
    {
        return CreateRealmRoleAsync(role.Name, role.Description, cancellationToken);
    }

    public async Task UpdateRoleAsync(
        string previousName,
        Role role,
        CancellationToken cancellationToken)
    {
        using var response = await SendAdminAsync(
            HttpMethod.Put,
            AdminPath($"roles/{Uri.EscapeDataString(previousName)}"),
            new
            {
                name = role.Name,
                description = role.Description,
                composite = false,
                clientRole = false
            },
            cancellationToken);
        await EnsureSuccessAsync(response);
    }

    public async Task DeleteRoleAsync(string roleName, CancellationToken cancellationToken)
    {
        using var response = await SendAdminAsync(
            HttpMethod.Delete,
            AdminPath($"roles/{Uri.EscapeDataString(roleName)}"),
            null,
            cancellationToken);
        await EnsureSuccessAsync(response);
    }

    public async Task<ExternalRole> GetRoleAsync(
        string roleName,
        CancellationToken cancellationToken)
    {
        using var response = await SendAdminAsync(
            HttpMethod.Get,
            AdminPath($"roles/{Uri.EscapeDataString(roleName)}"),
            null,
            cancellationToken);
        await EnsureSuccessAsync(response);
        var role = await response.Content.ReadFromJsonAsync<KeycloakRole>(
            cancellationToken: cancellationToken);
        return role is null
            ? throw new ExternalServiceException("Keycloak role was not found.", 404)
            : new ExternalRole(role.Name, role.Description, role.Id);
    }

    public async Task AssignRoleAsync(
        Guid userId,
        string roleName,
        CancellationToken cancellationToken)
    {
        var role = await GetRoleAsync(roleName, cancellationToken);
        using var response = await SendAdminAsync(
            HttpMethod.Post,
            AdminPath($"users/{userId}/role-mappings/realm"),
            new[]
            {
                new
                {
                    id = role.Id,
                    name = role.Name,
                    description = role.Description,
                    composite = false,
                    clientRole = false,
                    containerId = _options.Realm
                }
            },
            cancellationToken);
        await EnsureSuccessAsync(response);
    }

    public async Task RemoveRoleAsync(
        Guid userId,
        string roleName,
        CancellationToken cancellationToken)
    {
        var role = await GetRoleAsync(roleName, cancellationToken);
        using var response = await SendAdminAsync(
            HttpMethod.Delete,
            AdminPath($"users/{userId}/role-mappings/realm"),
            new[]
            {
                new
                {
                    id = role.Id,
                    name = role.Name,
                    description = role.Description,
                    composite = false,
                    clientRole = false,
                    containerId = _options.Realm
                }
            },
            cancellationToken);
        await EnsureSuccessAsync(response);
    }

    public Task CreatePermissionAsync(
        Permission permission,
        CancellationToken cancellationToken)
    {
        return CreateRealmRoleAsync(
            PermissionRoleName(permission.Code),
            permission.Description,
            cancellationToken);
    }

    public async Task UpdatePermissionAsync(
        string previousCode,
        Permission permission,
        CancellationToken cancellationToken)
    {
        using var response = await SendAdminAsync(
            HttpMethod.Put,
            AdminPath($"roles/{Uri.EscapeDataString(PermissionRoleName(previousCode))}"),
            new
            {
                name = PermissionRoleName(permission.Code),
                description = permission.Description,
                composite = false,
                clientRole = false
            },
            cancellationToken);
        await EnsureSuccessAsync(response);
    }

    public async Task DeletePermissionAsync(
        string code,
        CancellationToken cancellationToken)
    {
        using var response = await SendAdminAsync(
            HttpMethod.Delete,
            AdminPath($"roles/{Uri.EscapeDataString(PermissionRoleName(code))}"),
            null,
            cancellationToken);
        await EnsureSuccessAsync(response);
    }

    public async Task AssignPermissionToRoleAsync(
        string roleName,
        string permissionCode,
        CancellationToken cancellationToken)
    {
        var role = await GetRoleAsync(roleName, cancellationToken);
        var permission = await GetRoleAsync(
            PermissionRoleName(permissionCode),
            cancellationToken);

        using var response = await SendAdminAsync(
            HttpMethod.Post,
            AdminPath($"roles/{Uri.EscapeDataString(role.Name)}/composites"),
            new[]
            {
                new
                {
                    id = permission.Id,
                    name = permission.Name,
                    description = permission.Description,
                    composite = false,
                    clientRole = false,
                    containerId = _options.Realm
                }
            },
            cancellationToken);
        await EnsureSuccessAsync(response);
    }

    public async Task RemovePermissionFromRoleAsync(
        string roleName,
        string permissionCode,
        CancellationToken cancellationToken)
    {
        var role = await GetRoleAsync(roleName, cancellationToken);
        var permission = await GetRoleAsync(
            PermissionRoleName(permissionCode),
            cancellationToken);

        using var response = await SendAdminAsync(
            HttpMethod.Delete,
            AdminPath($"roles/{Uri.EscapeDataString(role.Name)}/composites"),
            new[]
            {
                new
                {
                    id = permission.Id,
                    name = permission.Name,
                    description = permission.Description,
                    composite = false,
                    clientRole = false,
                    containerId = _options.Realm
                }
            },
            cancellationToken);
        await EnsureSuccessAsync(response);
    }

    private async Task<KeycloakTokenResponse> RequestTokenAsync(
        IDictionary<string, string> formValues,
        CancellationToken cancellationToken)
    {
        using var response = await _httpClient.PostAsync(
            RealmPath("protocol/openid-connect/token"),
            new FormUrlEncodedContent(formValues),
            cancellationToken);
        await EnsureSuccessAsync(response);
        var token = await response.Content.ReadFromJsonAsync<KeycloakTokenResponse>(
            cancellationToken: cancellationToken);
        return token ?? throw new ExternalServiceException("Keycloak returned an empty token response.", 502);
    }

    private async Task<HttpResponseMessage> SendAdminAsync(
        HttpMethod method,
        string path,
        object? body,
        CancellationToken cancellationToken)
    {
        var token = await GetAdminTokenAsync(cancellationToken);
        using var request = new HttpRequestMessage(method, path);
        request.Headers.Authorization = new AuthenticationHeaderValue("Bearer", token);
        if (body is not null)
        {
            request.Content = JsonContent.Create(body);
        }

        return await _httpClient.SendAsync(
            request,
            HttpCompletionOption.ResponseHeadersRead,
            cancellationToken);
    }

    private async Task<string> GetAdminTokenAsync(CancellationToken cancellationToken)
    {
        if (!string.IsNullOrWhiteSpace(_adminAccessToken)
            && _adminTokenExpiresAtUtc > DateTime.UtcNow.AddSeconds(30))
        {
            return _adminAccessToken;
        }

        await _adminTokenLock.WaitAsync(cancellationToken);
        try
        {
            if (!string.IsNullOrWhiteSpace(_adminAccessToken)
                && _adminTokenExpiresAtUtc > DateTime.UtcNow.AddSeconds(30))
            {
                return _adminAccessToken;
            }

            using var response = await _httpClient.PostAsync(
                $"realms/{Uri.EscapeDataString(_options.AdminRealm)}/protocol/openid-connect/token",
                new FormUrlEncodedContent(
                    new Dictionary<string, string>
                    {
                        ["grant_type"] = "password",
                        ["client_id"] = "admin-cli",
                        ["username"] = _options.AdminUsername,
                        ["password"] = _options.AdminPassword
                    }),
                cancellationToken);
            await EnsureSuccessAsync(response);

            var token = await response.Content.ReadFromJsonAsync<KeycloakTokenResponse>(
                cancellationToken: cancellationToken)
                ?? throw new ExternalServiceException("Keycloak admin token response was empty.", 502);
            _adminAccessToken = token.AccessToken;
            _adminTokenExpiresAtUtc = DateTime.UtcNow.AddSeconds(token.ExpiresIn);
            return _adminAccessToken;
        }
        finally
        {
            _adminTokenLock.Release();
        }
    }

    private async Task CreateRealmRoleAsync(
        string name,
        string? description,
        CancellationToken cancellationToken)
    {
        using var response = await SendAdminAsync(
            HttpMethod.Post,
            AdminPath("roles"),
            new
            {
                name,
                description,
                composite = false,
                clientRole = false
            },
            cancellationToken);
        if (response.StatusCode == HttpStatusCode.Conflict)
        {
            return;
        }

        await EnsureSuccessAsync(response);
    }

    private string RealmPath(string suffix)
    {
        return $"realms/{Uri.EscapeDataString(_options.Realm)}/{suffix}";
    }

    private string AdminPath(string suffix)
    {
        return $"admin/realms/{Uri.EscapeDataString(_options.Realm)}/{suffix}";
    }

    private static string PermissionRoleName(string code) =>
        $"permission:{code.Trim().ToLowerInvariant()}";

    private static async Task EnsureSuccessAsync(HttpResponseMessage response)
    {
        if (response.IsSuccessStatusCode)
        {
            return;
        }

        throw await CreateExternalExceptionAsync(response);
    }

    private static async Task<ExternalServiceException> CreateExternalExceptionAsync(
        HttpResponseMessage response)
    {
        var detail = await response.Content.ReadAsStringAsync();
        if (string.IsNullOrWhiteSpace(detail))
        {
            detail = response.ReasonPhrase ?? "Keycloak request failed.";
        }

        return new ExternalServiceException(
            $"Keycloak request failed ({response.StatusCode}): {detail}",
            (int)response.StatusCode);
    }

    private sealed record KeycloakTokenResponse(
        [property: JsonPropertyName("access_token")] string AccessToken,
        [property: JsonPropertyName("refresh_token")] string RefreshToken,
        [property: JsonPropertyName("expires_in")] int ExpiresIn,
        [property: JsonPropertyName("refresh_expires_in")] int RefreshExpiresIn,
        [property: JsonPropertyName("token_type")] string TokenType)
    {
        public AuthTokenResponse ToApplicationResponse()
        {
            var jwt = new JwtSecurityTokenHandler().ReadJwtToken(AccessToken);
            var userId = Guid.TryParse(jwt.Subject, out var parsedUserId)
                ? parsedUserId
                : Guid.Empty;
            if (userId == Guid.Empty)
            {
                throw new ExternalServiceException("Keycloak token did not contain a valid user id.", 502);
            }

            return new AuthTokenResponse(
                AccessToken,
                RefreshToken,
                ExpiresIn,
                RefreshExpiresIn,
                TokenType,
                userId);
        }
    }

    private sealed record IntrospectionResponse(
        [property: JsonPropertyName("active")] bool Active,
        [property: JsonPropertyName("sub")] string? Subject,
        [property: JsonPropertyName("username")] string? Username);

    private sealed record KeycloakUser(
        [property: JsonPropertyName("id")] string Id,
        [property: JsonPropertyName("username")] string Username,
        [property: JsonPropertyName("email")] string? Email,
        [property: JsonPropertyName("firstName")] string? FirstName,
        [property: JsonPropertyName("lastName")] string? LastName,
        [property: JsonPropertyName("enabled")] bool Enabled)
    {
        public ExternalUser? ToExternalUser()
        {
            return Guid.TryParse(Id, out var userId)
                ? new ExternalUser(
                    userId,
                    Username,
                    Email ?? string.Empty,
                    FirstName,
                    LastName,
                    Enabled)
                : null;
        }
    }

    private sealed record KeycloakRole(
        [property: JsonPropertyName("id")] string? Id,
        [property: JsonPropertyName("name")] string Name,
        [property: JsonPropertyName("description")] string? Description);
}
