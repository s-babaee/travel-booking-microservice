using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Notification.Application.Contracts;
using Notification.Application.Services;

namespace Notification.Controllers;

[ApiController]
[Authorize]
[Route("api/notifications")]
public sealed class NotificationController(
    NotificationService service) : ControllerBase
{
    [HttpGet("me")]
    public async Task<ActionResult<PagedResponse<NotificationResponse>>> ListMine(
        [FromQuery] int page = 1,
        [FromQuery] int pageSize = 50,
        CancellationToken cancellationToken = default) =>
        Ok(await service.ListMineAsync(page, pageSize, cancellationToken));

    [HttpGet("{notificationId:guid}")]
    public async Task<ActionResult<NotificationResponse>> Get(
        Guid notificationId,
        CancellationToken cancellationToken) =>
        Ok(await service.GetAsync(notificationId, cancellationToken));

    [HttpPost("{notificationId:guid}/read")]
    public async Task<ActionResult<NotificationResponse>> MarkRead(
        Guid notificationId,
        CancellationToken cancellationToken) =>
        Ok(await service.MarkReadAsync(notificationId, cancellationToken));

    [HttpPost("read-all")]
    public async Task<IActionResult> MarkAllRead(
        CancellationToken cancellationToken) =>
        Ok(new
        {
            updated = await service.MarkAllReadAsync(cancellationToken)
        });
}
