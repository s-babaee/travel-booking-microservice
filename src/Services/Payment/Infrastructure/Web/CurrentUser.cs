using System.Security.Claims;
using Payment.Api.Application.Abstractions;
using Payment.Api.Application.Exceptions;

namespace Payment.Api.Infrastructure.Web;

public sealed class CurrentUser(
    IHttpContextAccessor httpContextAccessor) : ICurrentUser
{
    public Guid GetRequiredUserId()
    {
        var principal = httpContextAccessor.HttpContext?.User;
        var subject = principal?.FindFirstValue("sub")
            ?? principal?.FindFirstValue(ClaimTypes.NameIdentifier);
        return Guid.TryParse(subject, out var userId) && userId != Guid.Empty
            ? userId
            : throw new UnauthorizedException(
                "The access token does not contain a valid user id.");
    }

    public bool IsAdmin() =>
        httpContextAccessor.HttpContext?.User.IsInRole("admin") == true;
}
