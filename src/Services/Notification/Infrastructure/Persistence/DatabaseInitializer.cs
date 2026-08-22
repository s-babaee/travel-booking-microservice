using Microsoft.EntityFrameworkCore;
using Notification.Domain.Entities;
using Notification.Domain.Enums;

namespace Notification.Infrastructure.Persistence;

public static class DatabaseInitializer
{
    public static async Task InitializeAsync(
        IServiceProvider services,
        IConfiguration configuration,
        CancellationToken cancellationToken = default)
    {
        await using var scope = services.CreateAsyncScope();
        var dbContext = scope.ServiceProvider.GetRequiredService<AppDbContext>();
        if (configuration.GetValue("Database:ApplyMigrations", true))
        {
            await dbContext.Database.MigrateAsync(cancellationToken);
        }

        if (!configuration.GetValue("Database:SeedTemplates", true))
        {
            return;
        }

        if (await dbContext.NotificationTemplates.AnyAsync(cancellationToken))
        {
            return;
        }

        var nowUtc = DateTime.UtcNow;
        foreach (var eventType in NotificationTemplateDefaults.EventTypes)
        {
            dbContext.NotificationTemplates.Add(
                NotificationTemplate.Create(
                    Guid.NewGuid(),
                    eventType,
                    NotificationChannel.Email,
                    NotificationTemplateDefaults.Subject(eventType),
                    NotificationTemplateDefaults.Body(eventType),
                    nowUtc));
            dbContext.NotificationTemplates.Add(
                NotificationTemplate.Create(
                    Guid.NewGuid(),
                    eventType,
                    NotificationChannel.Sms,
                    NotificationTemplateDefaults.Subject(eventType),
                    NotificationTemplateDefaults.Body(eventType),
                    nowUtc));
        }

        await dbContext.SaveChangesAsync(cancellationToken);
    }
}

internal static class NotificationTemplateDefaults
{
    public static readonly string[] EventTypes =
    [
        "BookingConfirmed",
        "BookingFailed",
        "BookingCancellationStarted",
        "BookingCancelled",
        "PaymentAuthorized",
        "PaymentFailed",
        "PaymentRefunded"
    ];

    public static string Subject(string eventType) =>
        eventType switch
        {
            "BookingConfirmed" => "Booking confirmed",
            "BookingFailed" => "Booking failed",
            "BookingCancellationStarted" => "Booking cancellation started",
            "BookingCancelled" => "Booking cancelled",
            "PaymentAuthorized" => "Payment authorized",
            "PaymentFailed" => "Payment failed",
            "PaymentRefunded" => "Payment refunded",
            _ => "Travel notification"
        };

    public static string Body(string eventType) =>
        eventType switch
        {
            "BookingConfirmed" =>
                "Your booking {{BookingId}} has been confirmed.",
            "BookingFailed" =>
                "Your booking {{BookingId}} failed. Reason: {{Reason}}",
            "BookingCancellationStarted" =>
                "Cancellation for booking {{BookingId}} has started.",
            "BookingCancelled" =>
                "Your booking {{BookingId}} has been cancelled.",
            "PaymentAuthorized" =>
                "Payment for booking {{BookingId}} was authorized.",
            "PaymentFailed" =>
                "Payment for booking {{BookingId}} failed. Reason: {{Reason}}",
            "PaymentRefunded" =>
                "Refund for booking {{BookingId}} was completed.",
            _ => "Travel notification for {{BookingId}}."
        };
}
