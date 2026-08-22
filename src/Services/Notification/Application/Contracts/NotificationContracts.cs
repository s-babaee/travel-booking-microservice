using Notification.Domain.Enums;

namespace Notification.Application.Contracts;

public sealed record NotificationResponse(
    Guid Id,
    Guid UserId,
    Guid EventId,
    string EventType,
    NotificationChannel Channel,
    string Subject,
    string Body,
    bool IsRead,
    DateTime CreatedAtUtc,
    DateTime? ReadAtUtc);

public sealed record NotificationTemplateResponse(
    Guid Id,
    string EventType,
    NotificationChannel Channel,
    string SubjectTemplate,
    string BodyTemplate,
    bool IsActive,
    DateTime CreatedAtUtc,
    DateTime UpdatedAtUtc);

public sealed record CreateNotificationTemplateRequest(
    string EventType,
    NotificationChannel Channel,
    string SubjectTemplate,
    string BodyTemplate);

public sealed record UpdateNotificationTemplateRequest(
    string EventType,
    NotificationChannel Channel,
    string SubjectTemplate,
    string BodyTemplate);

public sealed record PagedResponse<T>(
    IReadOnlyList<T> Items,
    int Page,
    int PageSize,
    int TotalCount);
