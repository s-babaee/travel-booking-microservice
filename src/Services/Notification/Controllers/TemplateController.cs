using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Notification.Application.Contracts;
using Notification.Application.Services;
using BuildingBlocks.Authorization;

namespace Notification.Controllers;

[ApiController]
[HasPermission(PermissionCatalog.NotificationsManage)]
[Route("api/notification-templates")]
public sealed class TemplateController(
    NotificationService service) : ControllerBase
{
    [HttpGet]
    public async Task<ActionResult<IReadOnlyList<NotificationTemplateResponse>>>
        List(CancellationToken cancellationToken) =>
        Ok(await service.ListTemplatesAsync(cancellationToken));

    [HttpPost]
    public async Task<ActionResult<NotificationTemplateResponse>> Create(
        CreateNotificationTemplateRequest request,
        CancellationToken cancellationToken) =>
        Ok(await service.CreateTemplateAsync(request, cancellationToken));

    [HttpPut("{templateId:guid}")]
    public async Task<ActionResult<NotificationTemplateResponse>> Update(
        Guid templateId,
        UpdateNotificationTemplateRequest request,
        CancellationToken cancellationToken) =>
        Ok(await service.UpdateTemplateAsync(
            templateId,
            request,
            cancellationToken));

    [HttpPost("{templateId:guid}/activate")]
    public async Task<ActionResult<NotificationTemplateResponse>> Activate(
        Guid templateId,
        CancellationToken cancellationToken) =>
        Ok(await service.ActivateTemplateAsync(
            templateId,
            cancellationToken));

    [HttpPost("{templateId:guid}/deactivate")]
    public async Task<ActionResult<NotificationTemplateResponse>> Deactivate(
        Guid templateId,
        CancellationToken cancellationToken) =>
        Ok(await service.DeactivateTemplateAsync(
            templateId,
            cancellationToken));
}
