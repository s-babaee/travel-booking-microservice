using Notification.Domain.Common;
using Notification.Domain.Enums;

namespace Notification.Domain.Entities;

public sealed class Notification
{
    private Notification()
    {
    }

    private Notification(
        Guid id,
        Guid userId,
        Guid eventId,
        string eventType,
        NotificationChannel channel,
        string subject,
        string body,
        DateTime nowUtc)
    {
        Id = id;
        UserId = userId;
        EventId = eventId;
        EventType = eventType;
        Channel = channel;
        Subject = subject;
        Body = body;
        CreatedAtUtc = nowUtc;
    }

    public Guid Id { get; private set; }
    public Guid UserId { get; private set; }
    public Guid EventId { get; private set; }
    public string EventType { get; private set; } = null!;
    public NotificationChannel Channel { get; private set; }
    public string Subject { get; private set; } = null!;
    public string Body { get; private set; } = null!;
    public bool IsRead { get; private set; }
    public DateTime CreatedAtUtc { get; private set; }
    public DateTime? ReadAtUtc { get; private set; }

    public static Notification Create(
        Guid id,
        Guid userId,
        Guid eventId,
        string eventType,
        NotificationChannel channel,
        string subject,
        string body,
        DateTime nowUtc)
    {
        if (id == Guid.Empty || userId == Guid.Empty || eventId == Guid.Empty)
        {
            throw new DomainException(
                "Notification, user and event ids are required.");
        }

        if (string.IsNullOrWhiteSpace(eventType)
            || string.IsNullOrWhiteSpace(subject)
            || string.IsNullOrWhiteSpace(body))
        {
            throw new DomainException(
                "Notification event type, subject and body are required.");
        }

        return new Notification(
            id,
            userId,
            eventId,
            eventType,
            channel,
            subject,
            body,
            nowUtc);
    }

    public void MarkRead(DateTime nowUtc)
    {
        IsRead = true;
        ReadAtUtc ??= nowUtc;
    }
}
