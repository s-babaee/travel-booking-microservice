using System.ComponentModel.DataAnnotations;
using Auth.Api.Domain.Enums;

namespace Auth.Api.Application.Contracts;

public sealed record RegisterCommand(
    [Required, StringLength(100, MinimumLength = 3)]
    string Username,
    [Required, EmailAddress, StringLength(320)]
    string Email,
    [Required, MinLength(8)]
    string Password,
    string? FirstName,
    string? LastName);

public sealed record LoginCommand(
    [Required] string Username,
    [Required] string Password);

public sealed record RefreshTokenCommand([property: Required] string RefreshToken);

public sealed record LogoutCommand([property: Required] string RefreshToken);

public sealed record ValidateTokenCommand([property: Required] string AccessToken);

public sealed record AuthTokenResponse(
    string AccessToken,
    string RefreshToken,
    int ExpiresIn,
    int RefreshExpiresIn,
    string TokenType,
    Guid UserId);

public sealed record TokenValidationResponse(bool IsValid, Guid? UserId, string? Username);

public sealed record UserResponse(
    Guid UserId,
    string Username,
    string Email,
    string? FirstName,
    string? LastName,
    UserStatus Status,
    bool IsDeleted);

public sealed record UpdateUserCommand(
    [property: Required, EmailAddress, StringLength(320)] string Email,
    string? FirstName,
    string? LastName);

public sealed record ChangeUserStatusCommand(UserStatus Status);

public sealed record CreateRoleCommand(
    [property: Required, StringLength(100, MinimumLength = 2)] string Name,
    string? Description);

public sealed record UpdateRoleCommand(
    [property: Required, StringLength(100, MinimumLength = 2)] string Name,
    string? Description);

public sealed record RoleResponse(Guid RoleId, string Name, string? Description);

public sealed record CreatePermissionCommand(
    [property: Required, StringLength(150, MinimumLength = 2)] string Code,
    string? Description);

public sealed record UpdatePermissionCommand(
    [property: Required, StringLength(150, MinimumLength = 2)] string Code,
    string? Description);

public sealed record PermissionResponse(Guid PermissionId, string Code, string? Description);

public sealed record ForgotPasswordCommand(
    [property: Required, EmailAddress, StringLength(320)] string Email);

public sealed record ResetPasswordCommand(
    [property: Required] string Token,
    [property: Required, MinLength(8)] string NewPassword);

public sealed record ChangePasswordCommand(
    [property: Required] string CurrentPassword,
    [property: Required, MinLength(8)] string NewPassword);

public sealed record PasswordResetResponse(string Message, string? Token);
