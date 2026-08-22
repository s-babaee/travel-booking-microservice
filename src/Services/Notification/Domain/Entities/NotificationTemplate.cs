using Notification.Domain.Common;
using Notification.Domain.Enums;

namespace Notification.Domain.Entities;

public sealed class NotificationTemplate
{
    private NotificationTemplate()
    {
    }

    private NotificationTemplate(
        Guid id,
        string eventType,
        NotificationChannel channel,
        string subjectTemplate,
        string bodyTemplate,
        DateTime nowUtc)
    {
        Id = id;
        EventType = eventType;
        Channel = channel;
        SubjectTemplate = subjectTemplate;
        BodyTemplate = bodyTemplate;
        IsActive = true;
        CreatedAtUtc = nowUtc;
        UpdatedAtUtc = nowUtc;
    }

    public Guid Id { get; private set; }
    public string EventType { get; private set; } = null!;
    public NotificationChannel Channel { get; private set; }
    public string SubjectTemplate { get; private set; } = null!;
    public string BodyTemplate { get; private set; } = null!;
    public bool IsActive { get; private set; }
    public DateTime CreatedAtUtc { get; private set; }
    public DateTime UpdatedAtUtc { get; private set; }

    public static NotificationTemplate Create(
        Guid id,
        string eventType,
        NotificationChannel channel,
        string subjectTemplate,
        string bodyTemplate,
        DateTime nowUtc)
    {
        if (id == Guid.Empty
            || string.IsNullOrWhiteSpace(eventType)
            || string.IsNullOrWhiteSpace(subjectTemplate)
            || string.IsNullOrWhiteSpace(bodyTemplate))
        {
            throw new DomainException(
                "Template id, event type, subject and body are required.");
        }

        return new NotificationTemplate(
            id,
            eventType.Trim(),
            channel,
            subjectTemplate.Trim(),
            bodyTemplate.Trim(),
            nowUtc);
    }

    public void Update(
        string eventType,
        NotificationChannel channel,
        string subjectTemplate,
        string bodyTemplate,
        DateTime nowUtc)
    {
        if (string.IsNullOrWhiteSpace(eventType)
            || string.IsNullOrWhiteSpace(subjectTemplate)
            || string.IsNullOrWhiteSpace(bodyTemplate))
        {
            throw new DomainException(
                "Template event type, subject and body are required.");
        }

        EventType = eventType.Trim();
        Channel = channel;
        SubjectTemplate = subjectTemplate.Trim();
        BodyTemplate = bodyTemplate.Trim();
        UpdatedAtUtc = nowUtc;
    }

    public void Activate(DateTime nowUtc)
    {
        IsActive = true;
        UpdatedAtUtc = nowUtc;
    }

    public void Deactivate(DateTime nowUtc)
    {
        IsActive = false;
        UpdatedAtUtc = nowUtc;
    }
}
