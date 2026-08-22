using Notification.Application.Contracts;
using Notification.Domain.Entities;
using NotificationEntity = Notification.Domain.Entities.Notification;

namespace Notification.Application.Abstractions;

public interface INotificationRepository
{
    Task<(IReadOnlyList<NotificationEntity> Items, int TotalCount)> ListByUserAsync(
        Guid userId,
        int page,
        int pageSize,
        CancellationToken cancellationToken);

    Task<NotificationEntity?> GetByIdAsync(
        Guid id,
        CancellationToken cancellationToken);

    Task<bool> ExistsForEventAsync(
        Guid eventId,
        string eventType,
        Notification.Domain.Enums.NotificationChannel channel,
        CancellationToken cancellationToken);

    Task AddAsync(
        NotificationEntity notification,
        CancellationToken cancellationToken);

    Task<int> MarkAllUnreadAsync(
        Guid userId,
        DateTime nowUtc,
        CancellationToken cancellationToken);
}

public interface INotificationTemplateRepository
{
    Task<IReadOnlyList<NotificationTemplate>> ListAsync(
        CancellationToken cancellationToken);

    Task<IReadOnlyList<NotificationTemplate>> ListActiveByEventTypeAsync(
        string eventType,
        CancellationToken cancellationToken);

    Task<NotificationTemplate?> GetByIdAsync(
        Guid id,
        CancellationToken cancellationToken);

    Task<bool> ExistsAsync(
        string eventType,
        Notification.Domain.Enums.NotificationChannel channel,
        Guid? excludingId,
        CancellationToken cancellationToken);

    Task AddAsync(
        NotificationTemplate template,
        CancellationToken cancellationToken);
}

public interface IUnitOfWork
{
    Task<int> SaveChangesAsync(CancellationToken cancellationToken);
}

public interface ICurrentUser
{
    Guid GetRequiredUserId();
    bool IsAdmin();
}

public interface INotificationEventHandler
{
    Task HandleAsync(
        Guid eventId,
        Guid userId,
        string eventType,
        IReadOnlyDictionary<string, string?> values,
        CancellationToken cancellationToken);
}
