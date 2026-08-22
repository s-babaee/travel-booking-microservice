using System.Globalization;
using Microsoft.EntityFrameworkCore;
using Notification.Application.Abstractions;
using Notification.Application.Contracts;
using Notification.Application.Exceptions;
using Notification.Domain.Entities;
using Notification.Domain.Enums;
using Notification.Infrastructure.Persistence;
using NotificationEntity = Notification.Domain.Entities.Notification;

namespace Notification.Application.Services;

public sealed class NotificationService(
    INotificationRepository notifications,
    INotificationTemplateRepository templates,
    IUnitOfWork unitOfWork,
    ICurrentUser currentUser,
    TimeProvider timeProvider,
    ILogger<NotificationService> logger)
    : INotificationEventHandler
{
    public async Task<PagedResponse<NotificationResponse>> ListMineAsync(
        int page,
        int pageSize,
        CancellationToken cancellationToken)
    {
        ValidatePaging(ref page, ref pageSize);
        var userId = currentUser.GetRequiredUserId();
        var (items, totalCount) = await notifications.ListByUserAsync(
            userId,
            page,
            pageSize,
            cancellationToken);
        return new PagedResponse<NotificationResponse>(
            items.Select(ToResponse).ToArray(),
            page,
            pageSize,
            totalCount);
    }

    public async Task<NotificationResponse> GetAsync(
        Guid notificationId,
        CancellationToken cancellationToken)
    {
        var notification = await notifications.GetByIdAsync(
            notificationId,
            cancellationToken)
            ?? throw new NotFoundException(
                "Notification",
                notificationId);
        EnsureOwner(notification.UserId);
        return ToResponse(notification);
    }

    public async Task<NotificationResponse> MarkReadAsync(
        Guid notificationId,
        CancellationToken cancellationToken)
    {
        var notification = await notifications.GetByIdAsync(
            notificationId,
            cancellationToken)
            ?? throw new NotFoundException(
                "Notification",
                notificationId);
        EnsureOwner(notification.UserId);
        notification.MarkRead(UtcNow());
        await unitOfWork.SaveChangesAsync(cancellationToken);
        return ToResponse(notification);
    }

    public async Task<int> MarkAllReadAsync(
        CancellationToken cancellationToken)
    {
        var userId = currentUser.GetRequiredUserId();
        return await notifications.MarkAllUnreadAsync(
            userId,
            UtcNow(),
            cancellationToken);
    }

    public async Task<IReadOnlyList<NotificationTemplateResponse>>
        ListTemplatesAsync(CancellationToken cancellationToken)
    {
        var items = await templates.ListAsync(cancellationToken);
        return items.Select(ToResponse).ToArray();
    }

    public async Task<NotificationTemplateResponse> CreateTemplateAsync(
        CreateNotificationTemplateRequest request,
        CancellationToken cancellationToken)
    {
        ArgumentNullException.ThrowIfNull(request);
        var exists = await templates.ExistsAsync(
            request.EventType,
            request.Channel,
            null,
            cancellationToken);
        if (exists)
        {
            throw new ConflictException(
                "A template already exists for this event and channel.");
        }

        var template = NotificationTemplate.Create(
            Guid.NewGuid(),
            request.EventType,
            request.Channel,
            request.SubjectTemplate,
            request.BodyTemplate,
            UtcNow());
        await templates.AddAsync(template, cancellationToken);
        await unitOfWork.SaveChangesAsync(cancellationToken);
        return ToResponse(template);
    }

    public async Task<NotificationTemplateResponse> UpdateTemplateAsync(
        Guid templateId,
        UpdateNotificationTemplateRequest request,
        CancellationToken cancellationToken)
    {
        ArgumentNullException.ThrowIfNull(request);
        var template = await templates.GetByIdAsync(
            templateId,
            cancellationToken)
            ?? throw new NotFoundException("Notification template", templateId);
        if (await templates.ExistsAsync(
                request.EventType,
                request.Channel,
                templateId,
                cancellationToken))
        {
            throw new ConflictException(
                "A template already exists for this event and channel.");
        }

        template.Update(
            request.EventType,
            request.Channel,
            request.SubjectTemplate,
            request.BodyTemplate,
            UtcNow());
        await unitOfWork.SaveChangesAsync(cancellationToken);
        return ToResponse(template);
    }

    public Task<NotificationTemplateResponse> ActivateTemplateAsync(
        Guid templateId,
        CancellationToken cancellationToken) =>
        ChangeTemplateStateAsync(templateId, true, cancellationToken);

    public Task<NotificationTemplateResponse> DeactivateTemplateAsync(
        Guid templateId,
        CancellationToken cancellationToken) =>
        ChangeTemplateStateAsync(templateId, false, cancellationToken);

    public async Task HandleAsync(
        Guid eventId,
        Guid userId,
        string eventType,
        IReadOnlyDictionary<string, string?> values,
        CancellationToken cancellationToken)
    {
        if (eventId == Guid.Empty || userId == Guid.Empty)
        {
            throw new ValidationException(
                "Notification event and user ids are required.");
        }

        var activeTemplates = await templates.ListActiveByEventTypeAsync(
            eventType,
            cancellationToken);
        if (activeTemplates.Count == 0)
        {
            activeTemplates =
            [
                NotificationTemplate.Create(
                    Guid.NewGuid(),
                    eventType,
                    NotificationChannel.Email,
                    NotificationTemplateDefaults.Subject(eventType),
                    NotificationTemplateDefaults.Body(eventType),
                    UtcNow()),
                NotificationTemplate.Create(
                    Guid.NewGuid(),
                    eventType,
                    NotificationChannel.Sms,
                    NotificationTemplateDefaults.Subject(eventType),
                    NotificationTemplateDefaults.Body(eventType),
                    UtcNow())
            ];
        }

        foreach (var template in activeTemplates)
        {
            if (await notifications.ExistsForEventAsync(
                    eventId,
                    eventType,
                    template.Channel,
                    cancellationToken))
            {
                continue;
            }

            var notification = NotificationEntity.Create(
                Guid.NewGuid(),
                userId,
                eventId,
                eventType,
                template.Channel,
                Render(template.SubjectTemplate, values),
                Render(template.BodyTemplate, values),
                UtcNow());
            await notifications.AddAsync(notification, cancellationToken);
            logger.LogInformation(
                "Mock {Channel} notification created for {EventType}, event {EventId}, user {UserId}. Subject: {Subject}",
                template.Channel,
                eventType,
                eventId,
                userId,
                notification.Subject);
        }

        try
        {
            await unitOfWork.SaveChangesAsync(cancellationToken);
        }
        catch (DbUpdateException exception)
        {
            logger.LogWarning(
                exception,
                "Notification event {EventId} was already processed concurrently.",
                eventId);
        }
    }

    private async Task<NotificationTemplateResponse>
        ChangeTemplateStateAsync(
            Guid templateId,
            bool activate,
            CancellationToken cancellationToken)
    {
        var template = await templates.GetByIdAsync(
            templateId,
            cancellationToken)
            ?? throw new NotFoundException("Notification template", templateId);
        if (activate)
        {
            template.Activate(UtcNow());
        }
        else
        {
            template.Deactivate(UtcNow());
        }

        await unitOfWork.SaveChangesAsync(cancellationToken);
        return ToResponse(template);
    }

    private void EnsureOwner(Guid userId)
    {
        if (currentUser.IsAdmin())
        {
            return;
        }

        if (currentUser.GetRequiredUserId() != userId)
        {
            throw new UnauthorizedException(
                "You are not allowed to access this notification.");
        }
    }

    private DateTime UtcNow() =>
        timeProvider.GetUtcNow().UtcDateTime;

    private static string Render(
        string template,
        IReadOnlyDictionary<string, string?> values)
    {
        var result = template;
        foreach (var pair in values)
        {
            result = result.Replace(
                "{{" + pair.Key + "}}",
                pair.Value ?? string.Empty,
                StringComparison.OrdinalIgnoreCase);
        }

        return result;
    }

    private static NotificationResponse ToResponse(
        NotificationEntity notification) =>
        new(
            notification.Id,
            notification.UserId,
            notification.EventId,
            notification.EventType,
            notification.Channel,
            notification.Subject,
            notification.Body,
            notification.IsRead,
            notification.CreatedAtUtc,
            notification.ReadAtUtc);

    private static NotificationTemplateResponse ToResponse(
        NotificationTemplate template) =>
        new(
            template.Id,
            template.EventType,
            template.Channel,
            template.SubjectTemplate,
            template.BodyTemplate,
            template.IsActive,
            template.CreatedAtUtc,
            template.UpdatedAtUtc);

    private static void ValidatePaging(ref int page, ref int pageSize)
    {
        page = Math.Max(page, 1);
        pageSize = Math.Clamp(pageSize, 1, 100);
    }
}
