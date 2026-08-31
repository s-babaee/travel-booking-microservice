using System.Security.Claims;
using Notification.Application.Abstractions;
using Notification.Application.Exceptions;
using BuildingBlocks.Authorization;

namespace Notification.Infrastructure.Web;

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

    public bool HasPermission(string permission) =>
        httpContextAccessor.HttpContext?.User.HasPermission(permission) == true;
}
