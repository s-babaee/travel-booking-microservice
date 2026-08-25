using System.ComponentModel.DataAnnotations;

namespace Auth.Api.Application.Contracts
{
    public sealed record ForgotPasswordCommand(
    [Required, EmailAddress, StringLength(320)] string Email);

    public sealed record ResetPasswordCommand(
        [Required] string Token,
        [Required, MinLength(8)] string NewPassword);

    public sealed record ChangePasswordCommand(
        [Required] string CurrentPassword,
        [Required, MinLength(8)] string NewPassword);

    public sealed record PasswordResetResponse(string Message, string? Token);
}
