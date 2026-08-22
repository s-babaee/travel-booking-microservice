using Microsoft.EntityFrameworkCore;
using Notification.Application.Abstractions;
using Notification.Application.Contracts;
using Notification.Domain.Entities;
using NotificationEntity = Notification.Domain.Entities.Notification;

namespace Notification.Infrastructure.Persistence;

public sealed class AppDbContext(DbContextOptions<AppDbContext> options)
    : DbContext(options), IUnitOfWork, INotificationRepository,
        INotificationTemplateRepository
{
    public DbSet<Notification.Domain.Entities.Notification> Notifications =>
        Set<Notification.Domain.Entities.Notification>();

    public DbSet<NotificationTemplate> NotificationTemplates =>
        Set<NotificationTemplate>();

    protected override void OnModelCreating(ModelBuilder modelBuilder)
    {
        modelBuilder.Entity<Notification.Domain.Entities.Notification>(
            entity =>
            {
                entity.ToTable("notifications");
                entity.HasKey(notification => notification.Id);
                entity.Property(notification => notification.EventType)
                    .HasMaxLength(100)
                    .IsRequired();
                entity.Property(notification => notification.Channel)
                    .HasConversion<string>()
                    .HasMaxLength(20)
                    .IsRequired();
                entity.Property(notification => notification.Subject)
                    .HasMaxLength(300)
                    .IsRequired();
                entity.Property(notification => notification.Body)
                    .HasMaxLength(5000)
                    .IsRequired();
                entity.HasIndex(notification => new
                {
                    notification.EventId,
                    notification.EventType,
                    notification.Channel
                }).IsUnique();
                entity.HasIndex(notification => new
                {
                    notification.UserId,
                    notification.CreatedAtUtc
                });
            });

        modelBuilder.Entity<NotificationTemplate>(entity =>
        {
            entity.ToTable("notification_templates");
            entity.HasKey(template => template.Id);
            entity.Property(template => template.EventType)
                .HasMaxLength(100)
                .IsRequired();
            entity.Property(template => template.Channel)
                .HasConversion<string>()
                .HasMaxLength(20)
                .IsRequired();
            entity.Property(template => template.SubjectTemplate)
                .HasMaxLength(300)
                .IsRequired();
            entity.Property(template => template.BodyTemplate)
                .HasMaxLength(5000)
                .IsRequired();
            entity.HasIndex(template => new
            {
                template.EventType,
                template.Channel
            }).IsUnique();
        });
    }

    async Task<(IReadOnlyList<NotificationEntity> Items,
        int TotalCount)> INotificationRepository.ListByUserAsync(
        Guid userId,
        int page,
        int pageSize,
        CancellationToken cancellationToken)
    {
        var query = Notifications
            .AsNoTracking()
            .Where(notification => notification.UserId == userId);
        var totalCount = await query.CountAsync(cancellationToken);
        var items = await query
            .OrderByDescending(notification => notification.CreatedAtUtc)
            .Skip((page - 1) * pageSize)
            .Take(pageSize)
            .ToArrayAsync(cancellationToken);
        return (items, totalCount);
    }

    async Task<NotificationEntity?>
        INotificationRepository.GetByIdAsync(
            Guid id,
            CancellationToken cancellationToken) =>
        await Notifications.SingleOrDefaultAsync(
            notification => notification.Id == id,
            cancellationToken);

    Task<bool> INotificationRepository.ExistsForEventAsync(
        Guid eventId,
        string eventType,
        Notification.Domain.Enums.NotificationChannel channel,
        CancellationToken cancellationToken) =>
        Notifications.AnyAsync(
            notification => notification.EventId == eventId
                && notification.EventType == eventType
                && notification.Channel == channel,
            cancellationToken);

    Task<int> INotificationRepository.MarkAllUnreadAsync(
        Guid userId,
        DateTime nowUtc,
        CancellationToken cancellationToken)
    {
        return Notifications
            .Where(notification => notification.UserId == userId
                && !notification.IsRead)
            .ExecuteUpdateAsync(
                setters => setters
                    .SetProperty(notification => notification.IsRead, true)
                    .SetProperty(notification => notification.ReadAtUtc, nowUtc),
                cancellationToken);
    }

    async Task<IReadOnlyList<NotificationTemplate>>
        INotificationTemplateRepository.ListAsync(
            CancellationToken cancellationToken) =>
        await NotificationTemplates
            .AsNoTracking()
            .OrderBy(template => template.EventType)
            .ThenBy(template => template.Channel)
            .ToArrayAsync(cancellationToken);

    async Task<IReadOnlyList<NotificationTemplate>>
        INotificationTemplateRepository.ListActiveByEventTypeAsync(
            string eventType,
            CancellationToken cancellationToken) =>
        await NotificationTemplates
            .AsNoTracking()
            .Where(template => template.EventType == eventType
                && template.IsActive)
            .OrderBy(template => template.Channel)
            .ToArrayAsync(cancellationToken);

    async Task<NotificationTemplate?>
        INotificationTemplateRepository.GetByIdAsync(
            Guid id,
            CancellationToken cancellationToken) =>
        await NotificationTemplates.SingleOrDefaultAsync(
            template => template.Id == id,
            cancellationToken);

    Task<bool> INotificationTemplateRepository.ExistsAsync(
        string eventType,
        Notification.Domain.Enums.NotificationChannel channel,
        Guid? excludingId,
        CancellationToken cancellationToken) =>
        NotificationTemplates.AnyAsync(
            template => template.EventType == eventType
                && template.Channel == channel
                && (!excludingId.HasValue || template.Id != excludingId.Value),
            cancellationToken);

    Task INotificationRepository.AddAsync(
        NotificationEntity notification,
        CancellationToken cancellationToken) =>
        Notifications.AddAsync(notification, cancellationToken).AsTask();

    Task INotificationTemplateRepository.AddAsync(
        NotificationTemplate template,
        CancellationToken cancellationToken) =>
        NotificationTemplates.AddAsync(template, cancellationToken).AsTask();

    Task<int> IUnitOfWork.SaveChangesAsync(
        CancellationToken cancellationToken) =>
        base.SaveChangesAsync(cancellationToken);
}
