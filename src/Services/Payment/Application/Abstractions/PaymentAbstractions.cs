using Payment.Api.Application.Contracts;
using Payment.Api.Domain.Entities;

namespace Payment.Api.Application.Abstractions;

public interface IPaymentRepository
{
    Task<PaymentTransaction?> GetByIdAsync(
        Guid id,
        CancellationToken cancellationToken);

    Task<PaymentTransaction?> GetByIdempotencyKeyAsync(
        string idempotencyKey,
        CancellationToken cancellationToken);

    Task<IReadOnlyList<PaymentTransaction>> ListByBookingAsync(
        Guid bookingId,
        CancellationToken cancellationToken);

    Task AddAsync(
        PaymentTransaction payment,
        CancellationToken cancellationToken);
}

public interface IRefundRepository
{
    Task<Refund?> GetByIdAsync(
        Guid id,
        CancellationToken cancellationToken);

    Task<Refund?> GetByPaymentIdAsync(
        Guid paymentId,
        CancellationToken cancellationToken);

    Task AddAsync(
        Refund refund,
        CancellationToken cancellationToken);
}

public interface IUnitOfWork
{
    Task<int> SaveChangesAsync(CancellationToken cancellationToken);
}

public interface IPaymentProvider
{
    Task<PaymentProviderResult> AuthorizeAsync(
        AuthorizePaymentCommand command,
        CancellationToken cancellationToken);
}

public interface IPaymentEventPublisher
{
    Task PublishAsync<TEvent>(
        TEvent @event,
        CancellationToken cancellationToken)
        where TEvent : class;
}

public interface ICurrentUser
{
    Guid GetRequiredUserId();
    bool HasPermission(string permission);
}

public sealed record PaymentProviderResult(
    bool Succeeded,
    string? FailureReason);
