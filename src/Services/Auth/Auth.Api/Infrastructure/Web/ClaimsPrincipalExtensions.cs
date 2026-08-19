using System.Security.Claims;
using Auth.Api.Application.Exceptions;

namespace Auth.Api.Infrastructure.Web;

public static class ClaimsPrincipalExtensions
{
    public static Guid GetRequiredUserId(this ClaimsPrincipal principal)
    {
        var subject = principal.FindFirstValue("sub");
        return Guid.TryParse(subject, out var userId) && userId != Guid.Empty
            ? userId
            : throw new UnauthorizedException("The access token does not contain a valid user id.");
    }
}
