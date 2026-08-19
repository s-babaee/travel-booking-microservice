using Auth.Api.Application.Contracts;

namespace Auth.Api.Application.Abstractions;

public interface IAuthService
{
    Task<UserResponse> RegisterAsync(RegisterCommand command, CancellationToken cancellationToken);
    Task<AuthTokenResponse> LoginAsync(LoginCommand command, CancellationToken cancellationToken);
    Task<AuthTokenResponse> RefreshTokenAsync(RefreshTokenCommand command, CancellationToken cancellationToken);
    Task LogoutAsync(LogoutCommand command, CancellationToken cancellationToken);
    Task<TokenValidationResponse> ValidateTokenAsync(ValidateTokenCommand command, CancellationToken cancellationToken);
}

public interface IUserService
{
    Task<UserResponse?> GetAsync(Guid userId, CancellationToken cancellationToken);
    Task<UserResponse> UpdateAsync(Guid userId, UpdateUserCommand command, CancellationToken cancellationToken);
    Task<UserResponse> ChangeStatusAsync(Guid userId, ChangeUserStatusCommand command, CancellationToken cancellationToken);
    Task DeleteAsync(Guid userId, CancellationToken cancellationToken);
}

public interface IRoleService
{
    Task<RoleResponse> CreateAsync(CreateRoleCommand command, CancellationToken cancellationToken);
    Task<IReadOnlyList<RoleResponse>> ListAsync(CancellationToken cancellationToken);
    Task<RoleResponse> UpdateAsync(Guid roleId, UpdateRoleCommand command, CancellationToken cancellationToken);
    Task DeleteAsync(Guid roleId, CancellationToken cancellationToken);
    Task AssignToUserAsync(Guid userId, Guid roleId, CancellationToken cancellationToken);
    Task RemoveFromUserAsync(Guid userId, Guid roleId, CancellationToken cancellationToken);
    Task<IReadOnlyList<RoleResponse>> GetUserRolesAsync(Guid userId, CancellationToken cancellationToken);
}

public interface IPermissionService
{
    Task<PermissionResponse> CreateAsync(CreatePermissionCommand command, CancellationToken cancellationToken);
    Task<IReadOnlyList<PermissionResponse>> ListAsync(CancellationToken cancellationToken);
    Task<PermissionResponse> UpdateAsync(Guid permissionId, UpdatePermissionCommand command, CancellationToken cancellationToken);
    Task DeleteAsync(Guid permissionId, CancellationToken cancellationToken);
    Task AssignToRoleAsync(Guid roleId, Guid permissionId, CancellationToken cancellationToken);
    Task RemoveFromRoleAsync(Guid roleId, Guid permissionId, CancellationToken cancellationToken);
}

public interface IPasswordService
{
    Task<PasswordResetResponse> ForgotAsync(ForgotPasswordCommand command, CancellationToken cancellationToken);
    Task ResetAsync(ResetPasswordCommand command, CancellationToken cancellationToken);
    Task ChangeAsync(Guid userId, ChangePasswordCommand command, CancellationToken cancellationToken);
}
