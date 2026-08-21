using System.Security.Claims;
using Booking.Api.Application.Abstractions;
using Booking.Api.Application.Exceptions;

namespace Booking.Api.Infrastructure.Web;

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
