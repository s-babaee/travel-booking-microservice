using System.ComponentModel.DataAnnotations;

namespace Auth.Api.Application.Contracts
{
    public sealed record ValidateTokenCommand([Required] string AccessToken);
    public sealed record TokenValidationResponse(bool IsValid, Guid? UserId, string? Username);
}
