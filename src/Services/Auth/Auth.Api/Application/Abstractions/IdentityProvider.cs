using Auth.Api.Application.Contracts;
using Auth.Api.Domain.Entities;

namespace Auth.Api.Application.Abstractions;

public sealed record ExternalUser(
    Guid UserId,
    string Username,
    string Email,
    string? FirstName,
    string? LastName,
    bool Enabled);

public sealed record ExternalRole(string Name, string? Description, string? Id = null);

public interface IIdentityProvider
{
     Task<ExternalUser> CreateUserAsync(
    RegisterCommand command,
    CancellationToken cancellationToken);
    Task<AuthTokenResponse> LoginAsync(LoginCommand command, CancellationToken cancellationToken);
    Task<AuthTokenResponse> RefreshTokenAsync(RefreshTokenCommand command, CancellationToken cancellationToken);
    Task LogoutAsync(LogoutCommand command, CancellationToken cancellationToken);
    Task<TokenValidationResponse> ValidateTokenAsync(ValidateTokenCommand command, CancellationToken cancellationToken);
    Task<ExternalUser?> GetUserAsync(Guid userId, CancellationToken cancellationToken);
    Task UpdateUserAsync(Guid userId, UpdateUserCommand command, CancellationToken cancellationToken);
    Task SetUserEnabledAsync(Guid userId, bool enabled, CancellationToken cancellationToken);
    Task DeleteUserAsync(Guid userId, CancellationToken cancellationToken);
    Task ChangePasswordAsync(Guid userId, string newPassword, CancellationToken cancellationToken);
    Task<bool> ValidateCredentialsAsync(string username, string password, CancellationToken cancellationToken);

    Task CreateRoleAsync(Role role, CancellationToken cancellationToken);
    Task UpdateRoleAsync(string previousName, Role role, CancellationToken cancellationToken);
    Task DeleteRoleAsync(string roleName, CancellationToken cancellationToken);
    Task<ExternalRole> GetRoleAsync(string roleName, CancellationToken cancellationToken);
    Task AssignRoleAsync(Guid userId, string roleName, CancellationToken cancellationToken);
    Task RemoveRoleAsync(Guid userId, string roleName, CancellationToken cancellationToken);
    Task CreatePermissionAsync(Permission permission, CancellationToken cancellationToken);
    Task UpdatePermissionAsync(string previousCode, Permission permission, CancellationToken cancellationToken);
    Task DeletePermissionAsync(string code, CancellationToken cancellationToken);
    Task AssignPermissionToRoleAsync(string roleName, string permissionCode, CancellationToken cancellationToken);
    Task RemovePermissionFromRoleAsync(string roleName, string permissionCode, CancellationToken cancellationToken);
}
