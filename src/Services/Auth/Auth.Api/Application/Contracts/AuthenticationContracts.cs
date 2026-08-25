using System.ComponentModel.DataAnnotations;

namespace Auth.Api.Application.Contracts
{
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

    public sealed record RefreshTokenCommand([Required] string RefreshToken);

    public sealed record LogoutCommand([Required] string RefreshToken);

    public sealed record AuthTokenResponse(
    string AccessToken,
    string RefreshToken,
    int ExpiresIn,
    int RefreshExpiresIn,
    string TokenType,
    Guid UserId);
}
