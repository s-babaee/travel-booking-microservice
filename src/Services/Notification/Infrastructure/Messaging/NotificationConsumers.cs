using BuildingBlocks.Contracts.Events;
using MassTransit;
using Notification.Application.Abstractions;

namespace Notification.Infrastructure.Messaging;

public sealed class BookingConfirmedConsumer(
    INotificationEventHandler handler) : IConsumer<BookingConfirmedEvent>
{
    public Task Consume(ConsumeContext<BookingConfirmedEvent> context) =>
        handler.HandleAsync(
            context.Message.BookingId,
            context.Message.UserId,
            "BookingConfirmed",
            new Dictionary<string, string?>
            {
                ["BookingId"] = context.Message.BookingId.ToString(),
                ["BookingType"] = context.Message.BookingType,
                ["Amount"] = context.Message.Amount.ToString(),
                ["Currency"] = context.Message.Currency
            },
            context.CancellationToken);
}

public sealed class BookingFailedConsumer(
    INotificationEventHandler handler) : IConsumer<BookingFailedEvent>
{
    public Task Consume(ConsumeContext<BookingFailedEvent> context) =>
        handler.HandleAsync(
            context.Message.BookingId,
            context.Message.UserId,
            "BookingFailed",
            new Dictionary<string, string?>
            {
                ["BookingId"] = context.Message.BookingId.ToString(),
                ["Reason"] = context.Message.Reason
            },
            context.CancellationToken);
}

public sealed class BookingCancellationStartedConsumer(
    INotificationEventHandler handler)
    : IConsumer<BookingCancellationStartedEvent>
{
    public Task Consume(
        ConsumeContext<BookingCancellationStartedEvent> context) =>
        handler.HandleAsync(
            context.Message.BookingId,
            context.Message.UserId,
            "BookingCancellationStarted",
            new Dictionary<string, string?>
            {
                ["BookingId"] = context.Message.BookingId.ToString(),
                ["Reason"] = context.Message.Reason
            },
            context.CancellationToken);
}

public sealed class BookingCancelledConsumer(
    INotificationEventHandler handler) : IConsumer<BookingCancelledEvent>
{
    public Task Consume(ConsumeContext<BookingCancelledEvent> context) =>
        handler.HandleAsync(
            context.Message.BookingId,
            context.Message.UserId,
            "BookingCancelled",
            new Dictionary<string, string?>
            {
                ["BookingId"] = context.Message.BookingId.ToString()
            },
            context.CancellationToken);
}

public sealed class PaymentAuthorizedConsumer(
    INotificationEventHandler handler) : IConsumer<PaymentAuthorized>
{
    public Task Consume(ConsumeContext<PaymentAuthorized> context) =>
        handler.HandleAsync(
            context.Message.TransactionId,
            context.Message.UserId,
            "PaymentAuthorized",
            new Dictionary<string, string?>
            {
                ["BookingId"] = context.Message.BookingId.ToString(),
                ["PaymentId"] = context.Message.TransactionId.ToString(),
                ["Amount"] = context.Message.Amount.ToString(),
                ["Currency"] = context.Message.Currency
            },
            context.CancellationToken);
}

public sealed class PaymentFailedConsumer(
    INotificationEventHandler handler) : IConsumer<PaymentFailedEvent>
{
    public Task Consume(ConsumeContext<PaymentFailedEvent> context) =>
        handler.HandleAsync(
            context.Message.BookingId,
            context.Message.UserId,
            "PaymentFailed",
            new Dictionary<string, string?>
            {
                ["BookingId"] = context.Message.BookingId.ToString(),
                ["Reason"] = context.Message.Reason
            },
            context.CancellationToken);
}

public sealed class PaymentRefundedConsumer(
    INotificationEventHandler handler) : IConsumer<PaymentRefundedEvent>
{
    public Task Consume(ConsumeContext<PaymentRefundedEvent> context) =>
        handler.HandleAsync(
            context.Message.PaymentId,
            context.Message.UserId,
            "PaymentRefunded",
            new Dictionary<string, string?>
            {
                ["BookingId"] = context.Message.BookingId.ToString(),
                ["PaymentId"] = context.Message.PaymentId.ToString(),
                ["Amount"] = context.Message.Amount.ToString(),
                ["Currency"] = context.Message.Currency
            },
            context.CancellationToken);
}
