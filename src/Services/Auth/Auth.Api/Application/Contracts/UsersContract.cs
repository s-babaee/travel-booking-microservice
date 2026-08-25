using Auth.Api.Domain.Enums;
using System.ComponentModel.DataAnnotations;

namespace Auth.Api.Application.Contracts
{
    public sealed record UserResponse(
    Guid UserId,
    string Username,
    string Email,
    string? FirstName,
    string? LastName,
    UserStatus Status,
    bool IsDeleted);

    public sealed record UpdateUserCommand(
        [Required, EmailAddress, StringLength(320)] string Email,
        string? FirstName,
        string? LastName);

    public sealed record ChangeUserStatusCommand(UserStatus Status);
}
