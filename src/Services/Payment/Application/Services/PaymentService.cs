using BookingPaymentRequest = BuildingBlocks.Contracts.Integrations.AuthorizePaymentRequest;
using BuildingBlocks.Contracts.Events;
using BookingPaymentResponse = BuildingBlocks.Contracts.Integrations.AuthorizePaymentResponse;
using Payment.Api.Application.Abstractions;
using Payment.Api.Application.Contracts;
using Payment.Api.Application.Exceptions;
using BuildingBlocks.Authorization;
using Payment.Api.Domain.Entities;
using Payment.Api.Domain.Enums;

namespace Payment.Api.Application.Services;

public sealed class PaymentService(
    IPaymentRepository payments,
    IRefundRepository refunds,
    IUnitOfWork unitOfWork,
    IPaymentProvider provider,
    IPaymentEventPublisher events,
    ICurrentUser currentUser,
    TimeProvider timeProvider,
    ILogger<PaymentService> logger)
{
    public async Task<BookingPaymentResponse> AuthorizeAsync(
        BookingPaymentRequest request,
        CancellationToken cancellationToken)
    {
        ArgumentNullException.ThrowIfNull(request);
        Validate(request);
        EnsureUser(request.UserId);

        var existing = await payments.GetByIdempotencyKeyAsync(
            request.IdempotencyKey,
            cancellationToken);
        if (existing is not null)
        {
            EnsureSameRequest(existing, request);
            return ToIntegrationResponse(existing);
        }

        var command = new AuthorizePaymentCommand(
            request.BookingId,
            request.UserId,
            request.Amount,
            NormalizeCurrency(request.Currency),
            request.PaymentMethodToken,
            request.IdempotencyKey);
        var providerResult = await provider.AuthorizeAsync(
            command,
            cancellationToken);
        var nowUtc = UtcNow();
        var payment = providerResult.Succeeded
            ? PaymentTransaction.Authorize(
                Guid.NewGuid(),
                request.BookingId,
                request.UserId,
                request.Amount,
                NormalizeCurrency(request.Currency),
                request.IdempotencyKey,
                nowUtc)
            : PaymentTransaction.Failed(
                Guid.NewGuid(),
                request.BookingId,
                request.UserId,
                request.Amount,
                NormalizeCurrency(request.Currency),
                request.IdempotencyKey,
                providerResult.FailureReason ?? "Payment failed.",
                nowUtc);

        try
        {
            await payments.AddAsync(payment, cancellationToken);
            await unitOfWork.SaveChangesAsync(cancellationToken);
        }
        catch (Microsoft.EntityFrameworkCore.DbUpdateException)
        {
            var concurrent = await payments.GetByIdempotencyKeyAsync(
                request.IdempotencyKey,
                cancellationToken);
            if (concurrent is not null)
            {
                return ToIntegrationResponse(concurrent);
            }

            throw;
        }

        if (providerResult.Succeeded)
        {
            await events.PublishAsync(
                new PaymentAuthorized(
                    payment.BookingId,
                    payment.UserId,
                    payment.Id,
                    payment.Amount,
                    payment.Currency,
                    nowUtc),
                cancellationToken);
        }
        else
        {
            await events.PublishAsync(
                new PaymentFailedEvent(
                    payment.BookingId,
                    payment.UserId,
                    payment.FailureReason,
                    nowUtc),
                cancellationToken);
        }

        return ToIntegrationResponse(payment);
    }

    public async Task<PaymentResponse> GetAsync(
        Guid paymentId,
        CancellationToken cancellationToken)
    {
        var payment = await payments.GetByIdAsync(
            paymentId,
            cancellationToken)
            ?? throw new NotFoundException("Payment", paymentId);
        EnsureOwnerOrAdmin(payment.UserId);
        return ToResponse(payment);
    }

    public async Task<IReadOnlyList<PaymentResponse>> ListByBookingAsync(
        Guid bookingId,
        CancellationToken cancellationToken)
    {
        var results = await payments.ListByBookingAsync(
            bookingId,
            cancellationToken);
        if (results.Count > 0)
        {
            EnsureOwnerOrAdmin(results[0].UserId);
        }

        return results.Select(ToResponse).ToArray();
    }

    public async Task<RefundResponse> RefundAsync(
        RefundPaymentCommand command,
        CancellationToken cancellationToken)
    {
        var payment = await payments.GetByIdAsync(
            command.PaymentId,
            cancellationToken)
            ?? throw new NotFoundException("Payment", command.PaymentId);
        EnsureOwnerOrAdmin(payment.UserId);

        var existing = await refunds.GetByPaymentIdAsync(
            payment.Id,
            cancellationToken);
        if (existing is not null)
        {
            return ToResponse(existing);
        }

        if (payment.Status != PaymentStatus.Authorized)
        {
            throw new ConflictException(
                $"Payment cannot be refunded from status '{payment.Status}'.");
        }

        var nowUtc = UtcNow();
        var refund = Refund.Complete(
            Guid.NewGuid(),
            payment,
            command.Reason,
            nowUtc);
        payment.MarkRefunded(nowUtc);
        await refunds.AddAsync(refund, cancellationToken);
        await unitOfWork.SaveChangesAsync(cancellationToken);

        await events.PublishAsync(
            new PaymentRefundedEvent(
                payment.Id,
                payment.BookingId,
                payment.UserId,
                payment.Amount,
                payment.Currency,
                nowUtc),
            cancellationToken);

        return ToResponse(refund);
    }

    public async Task<RefundResponse> GetRefundAsync(
        Guid refundId,
        CancellationToken cancellationToken)
    {
        var refund = await refunds.GetByIdAsync(
            refundId,
            cancellationToken)
            ?? throw new NotFoundException("Refund", refundId);
        EnsureOwnerOrAdmin(refund.UserId);
        return ToResponse(refund);
    }

    public async Task VoidAsync(
        Guid paymentId,
        string? reason,
        CancellationToken cancellationToken)
    {
        var payment = await payments.GetByIdAsync(
            paymentId,
            cancellationToken)
            ?? throw new NotFoundException("Payment", paymentId);
        EnsureOwnerOrAdmin(payment.UserId);
        payment.Void(UtcNow());
        await unitOfWork.SaveChangesAsync(cancellationToken);
        logger.LogInformation(
            "Mock payment {PaymentId} voided. Reason: {Reason}",
            paymentId,
            reason);
    }

    private void EnsureOwnerOrAdmin(Guid userId)
    {
        if (currentUser.HasPermission(PermissionCatalog.PaymentsViewAll))
        {
            return;
        }

        EnsureUser(userId);
    }

    private void EnsureUser(Guid userId)
    {
        if (currentUser.GetRequiredUserId() != userId)
        {
            throw new UnauthorizedException(
                "You are not allowed to access this payment.");
        }
    }

    private static void Validate(BookingPaymentRequest request)
    {
        if (request.BookingId == Guid.Empty
            || request.UserId == Guid.Empty)
        {
            throw new ValidationException(
                "Booking and user ids are required.");
        }

        if (request.Amount < 0)
        {
            throw new ValidationException(
                "Payment amount cannot be negative.");
        }

        if (string.IsNullOrWhiteSpace(request.Currency)
            || request.Currency.Trim().Length != 3)
        {
            throw new ValidationException(
                "Currency must be a three-letter ISO code.");
        }

        if (string.IsNullOrWhiteSpace(request.PaymentMethodToken))
        {
            throw new ValidationException(
                "Payment method token is required.");
        }

        if (string.IsNullOrWhiteSpace(request.IdempotencyKey))
        {
            throw new ValidationException(
                "IdempotencyKey is required.");
        }
    }

    private static void EnsureSameRequest(
        PaymentTransaction payment,
        BookingPaymentRequest request)
    {
        if (payment.BookingId != request.BookingId
            || payment.UserId != request.UserId
            || payment.Amount != request.Amount
            || !payment.Currency.Equals(
                NormalizeCurrency(request.Currency),
                StringComparison.OrdinalIgnoreCase))
        {
            throw new ConflictException(
                "The idempotency key is already used by another payment request.");
        }
    }

    private static string NormalizeCurrency(string currency) =>
        currency.Trim().ToUpperInvariant();

    private DateTime UtcNow() =>
        timeProvider.GetUtcNow().UtcDateTime;

    private static BookingPaymentResponse ToIntegrationResponse(
        PaymentTransaction payment) =>
        new(
            payment.Status == PaymentStatus.Authorized,
            payment.Status == PaymentStatus.Authorized
                ? payment.Id
                : null,
            payment.FailureReason);

    private static PaymentResponse ToResponse(
        PaymentTransaction payment) =>
        new(
            payment.Id,
            payment.BookingId,
            payment.UserId,
            payment.Amount,
            payment.Currency,
            payment.Status,
            payment.FailureReason,
            payment.CreatedAtUtc,
            payment.UpdatedAtUtc,
            payment.AuthorizedAtUtc,
            payment.RefundedAtUtc);

    private static RefundResponse ToResponse(Refund refund) =>
        new(
            refund.Id,
            refund.PaymentId,
            refund.BookingId,
            refund.UserId,
            refund.Amount,
            refund.Currency,
            refund.Status,
            refund.Reason,
            refund.CreatedAtUtc,
            refund.CompletedAtUtc);
}
