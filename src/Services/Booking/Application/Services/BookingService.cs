using Booking.Api.Application.Abstractions;
using Booking.Api.Application.Contracts;
using Booking.Api.Application.Exceptions;
using Booking.Api.Domain.Entities;
using Booking.Api.Domain.Enums;
using BuildingBlocks.Authorization;
using BuildingBlocks.Contracts.Events;
using BookingEntity = Booking.Api.Domain.Entities.Booking;

namespace Booking.Api.Application.Services;

public sealed class BookingService(
    IBookingRepository bookings,
    IOrderRepository orders,
    IUnitOfWork unitOfWork,
    IInventoryGateway inventory,
    IPaymentGateway payments,
    IBookingEventPublisher events,
    ICurrentUser currentUser,
    TimeProvider timeProvider,
    IConfiguration configuration,
    ILogger<BookingService> logger)
{
    public async Task<BookingResponse> CreateHotelAsync(
        CreateHotelBookingRequest request,
        CancellationToken cancellationToken)
    {
        ArgumentNullException.ThrowIfNull(request);
        ValidateCommon(request.TotalAmount, request.Currency, request.PaymentMethodToken);
        ValidateItems(request.Rooms);

        var userId = currentUser.GetRequiredUserId();
        var existing = await FindExistingAsync(
            userId,
            request.IdempotencyKey,
            cancellationToken);
        if (existing is not null)
        {
            return ToResponse(existing);
        }

        var nowUtc = UtcNow();
        var booking = BookingEntity.CreateHotel(
            Guid.NewGuid(),
            userId,
            request.HotelId,
            request.CheckIn,
            request.CheckOut,
            request.TotalAmount,
            NormalizeCurrency(request.Currency),
            NormalizeIdempotencyKey(request.IdempotencyKey),
            request.PassengerName,
            request.Rooms.Select(item => BookingItem.Create(
                item.RoomTypeId,
                item.Quantity,
                item.UnitAmount)),
            nowUtc);

        await bookings.AddAsync(booking, cancellationToken);
        await unitOfWork.SaveChangesAsync(cancellationToken);
        await PublishCreatedAsync(booking, cancellationToken);
        return await RunSagaAsync(
            booking,
            request.PaymentMethodToken,
            cancellationToken);
    }

    public async Task<BookingResponse> CreateFlightAsync(
        CreateFlightBookingRequest request,
        CancellationToken cancellationToken)
    {
        ArgumentNullException.ThrowIfNull(request);
        ValidateCommon(request.TotalAmount, request.Currency, request.PaymentMethodToken);
        ValidateItems(request.Classes);

        var userId = currentUser.GetRequiredUserId();
        var existing = await FindExistingAsync(
            userId,
            request.IdempotencyKey,
            cancellationToken);
        if (existing is not null)
        {
            return ToResponse(existing);
        }

        var nowUtc = UtcNow();
        var booking = BookingEntity.CreateFlight(
            Guid.NewGuid(),
            userId,
            request.FlightId,
            request.Date,
            request.TotalAmount,
            NormalizeCurrency(request.Currency),
            NormalizeIdempotencyKey(request.IdempotencyKey),
            request.PassengerName,
            request.Classes.Select(item => BookingItem.Create(
                item.FlightClassId,
                item.Quantity,
                item.UnitAmount)),
            nowUtc);

        await bookings.AddAsync(booking, cancellationToken);
        await unitOfWork.SaveChangesAsync(cancellationToken);
        await PublishCreatedAsync(booking, cancellationToken);
        return await RunSagaAsync(
            booking,
            request.PaymentMethodToken,
            cancellationToken);
    }

    public async Task<BookingResponse> GetAsync(
        Guid bookingId,
        CancellationToken cancellationToken)
    {
        var booking = await bookings.GetByIdAsync(
            bookingId,
            cancellationToken)
            ?? throw new NotFoundException("Booking", bookingId);
        EnsureOwnerOrAdmin(booking);
        return ToResponse(booking);
    }

    public async Task<PagedResponse<BookingResponse>> ListMineAsync(
        int page,
        int pageSize,
        CancellationToken cancellationToken)
    {
        ValidatePaging(ref page, ref pageSize);
        var userId = currentUser.GetRequiredUserId();
        var items = await bookings.ListByUserAsync(
            userId,
            page,
            pageSize,
            cancellationToken);
        return new PagedResponse<BookingResponse>(
            items.Select(ToResponse).ToArray(),
            page,
            pageSize,
            items.Count);
    }

    public async Task<BookingResponse> CancelAsync(
        Guid bookingId,
        CancelBookingRequest? request,
        CancellationToken cancellationToken)
    {
        var booking = await bookings.GetByIdAsync(
            bookingId,
            cancellationToken)
            ?? throw new NotFoundException("Booking", bookingId);
        EnsureOwnerOrAdmin(booking);

        if (booking.Status is BookingStatus.Cancelled or BookingStatus.Failed)
        {
            return ToResponse(booking);
        }

        var nowUtc = UtcNow();
        booking.StartCancellation(nowUtc);
        await unitOfWork.SaveChangesAsync(cancellationToken);
        await PublishStatusAsync(booking, cancellationToken);

        try
        {
            if (booking.PaymentTransactionId.HasValue)
            {
                var reason = request?.Reason ?? "Booking cancellation";
                if (booking.Status == BookingStatus.Cancelling
                    && booking.ConfirmedAtUtc.HasValue)
                {
                    await payments.RefundAsync(
                        booking.PaymentTransactionId.Value,
                        reason,
                        cancellationToken);
                }
                else
                {
                    await payments.VoidAsync(
                        booking.PaymentTransactionId.Value,
                        reason,
                        cancellationToken);
                }
            }

            if (booking.InventoryHoldId.HasValue)
            {
                await inventory.ReleaseAsync(
                    booking.InventoryHoldId.Value,
                    booking.Type,
                    cancellationToken);
            }

            booking.Cancel(nowUtc);
            await unitOfWork.SaveChangesAsync(cancellationToken);
            await PublishStatusAsync(booking, cancellationToken);
            return ToResponse(booking);
        }
        catch
        {
            logger.LogError(
                "Compensation failed while cancelling booking {BookingId}.",
                bookingId);
            throw;
        }
    }

    public async Task<PagedResponse<BookingResponse>> SearchAsync(
        BookingSearchQuery query,
        CancellationToken cancellationToken)
    {
        ValidatePaging(ref query);
        var (items, totalCount) = await bookings.SearchAsync(
            query,
            cancellationToken);
        return new PagedResponse<BookingResponse>(
            items.Select(ToResponse).ToArray(),
            query.Page,
            query.PageSize,
            totalCount);
    }

    public async Task<BookingResponse> ChangeStatusAsync(
        Guid bookingId,
        AdminStatusChangeRequest request,
        CancellationToken cancellationToken)
    {
        ArgumentNullException.ThrowIfNull(request);
        var booking = await bookings.GetByIdAsync(
            bookingId,
            cancellationToken)
            ?? throw new NotFoundException("Booking", bookingId);
        booking.AdminChangeStatus(
            request.Status,
            request.Reason,
            UtcNow());
        await unitOfWork.SaveChangesAsync(cancellationToken);
        await PublishStatusAsync(booking, cancellationToken);
        return ToResponse(booking);
    }

    public Task<BookingStatsResponse> GetStatsAsync(
        DateTime? fromUtc,
        DateTime? toUtc,
        CancellationToken cancellationToken) =>
        bookings.GetStatsAsync(fromUtc, toUtc, cancellationToken);

    public async Task<OrderResponse> GetOrderAsync(
        Guid orderId,
        CancellationToken cancellationToken)
    {
        var order = await orders.GetByIdAsync(orderId, cancellationToken)
            ?? throw new NotFoundException("Order", orderId);
        EnsureUser(order.UserId);
        var booking = await bookings.GetByIdAsync(
            order.BookingId,
            cancellationToken)
            ?? throw new NotFoundException("Booking", order.BookingId);
        return new OrderResponse(
            order.Id,
            order.BookingId,
            order.UserId,
            order.TotalAmount,
            order.Currency,
            order.CreatedAtUtc,
            ToResponse(booking));
    }

    public async Task<PagedResponse<OrderResponse>> ListOrdersAsync(
        int page,
        int pageSize,
        CancellationToken cancellationToken)
    {
        ValidatePaging(ref page, ref pageSize);
        var items = await orders.ListAsync(page, pageSize, cancellationToken);
        var results = new List<OrderResponse>(items.Count);
        foreach (var order in items)
        {
            var booking = await bookings.GetByIdAsync(
                order.BookingId,
                cancellationToken);
            if (booking is not null)
            {
                results.Add(new OrderResponse(
                    order.Id,
                    order.BookingId,
                    order.UserId,
                    order.TotalAmount,
                    order.Currency,
                    order.CreatedAtUtc,
                    ToResponse(booking)));
            }
        }

        return new PagedResponse<OrderResponse>(
            results,
            page,
            pageSize,
            results.Count);
    }

    private async Task<BookingResponse> RunSagaAsync(
        BookingEntity booking,
        string paymentMethodToken,
        CancellationToken cancellationToken)
    {
        var holdId = Guid.NewGuid();
        var expiresAtUtc = UtcNow().AddMinutes(
            configuration.GetValue("Saga:HoldDurationMinutes", 15));
        var paymentAuthorized = false;

        try
        {
            var hold = booking.Type == BookingType.Hotel
                ? await inventory.HoldHotelAsync(
                    new HoldHotelCommand(
                        holdId,
                        booking.HotelId!.Value,
                        booking.CheckIn!.Value,
                        booking.CheckOut!.Value,
                        booking.Items.Select(item => new InventoryRoomCommand(
                            item.ResourceTypeId,
                            item.Quantity)).ToArray(),
                        expiresAtUtc),
                    cancellationToken)
                : await inventory.HoldFlightAsync(
                    new HoldFlightCommand(
                        holdId,
                        booking.FlightId!.Value,
                        booking.FlightDate!.Value,
                        booking.Items.Select(item =>
                            new InventoryFlightClassCommand(
                                item.ResourceTypeId,
                                item.Quantity)).ToArray(),
                        expiresAtUtc),
                    cancellationToken);

            booking.MarkInventoryHeld(hold.HoldId, UtcNow());
            await unitOfWork.SaveChangesAsync(cancellationToken);
            await PublishStatusAsync(booking, cancellationToken);

            var payment = await payments.AuthorizeAsync(
                new PaymentAuthorizationCommand(
                    booking.Id,
                    booking.UserId,
                    booking.TotalAmount,
                    booking.Currency,
                    paymentMethodToken,
                    booking.IdempotencyKey),
                cancellationToken);
            if (!payment.Succeeded || !payment.TransactionId.HasValue)
            {
                await inventory.ReleaseAsync(
                    booking.InventoryHoldId!.Value,
                    booking.Type,
                    cancellationToken);
                booking.Fail(
                    payment.FailureReason ?? "Payment authorization failed.",
                    UtcNow());
                await unitOfWork.SaveChangesAsync(cancellationToken);
                await PublishStatusAsync(booking, cancellationToken);
                return ToResponse(booking);
            }

            booking.MarkPaymentAuthorized(
                payment.TransactionId.Value,
                UtcNow());
            paymentAuthorized = true;
            await unitOfWork.SaveChangesAsync(cancellationToken);
            await PublishStatusAsync(booking, cancellationToken);

            await inventory.ConfirmAsync(
                booking.InventoryHoldId!.Value,
                booking.Type,
                cancellationToken);

            var order = Order.Create(Guid.NewGuid(), booking, UtcNow());
            await orders.AddAsync(order, cancellationToken);
            booking.Confirm(order.Id, UtcNow());
            await unitOfWork.SaveChangesAsync(cancellationToken);
            await PublishStatusAsync(booking, cancellationToken);
            return ToResponse(booking);
        }
        catch (Exception exception)
        {
            logger.LogError(
                exception,
                "Booking saga failed for {BookingId}.",
                booking.Id);

            if (paymentAuthorized && booking.PaymentTransactionId.HasValue)
            {
                await TryCompensatePaymentAsync(
                    booking.PaymentTransactionId.Value,
                    cancellationToken);
            }

            if (booking.InventoryHoldId.HasValue
                || holdId != Guid.Empty)
            {
                await TryCompensateInventoryAsync(
                    booking.InventoryHoldId ?? holdId,
                    booking.Type,
                    cancellationToken);
            }

            if (booking.Status is not BookingStatus.Failed)
            {
                booking.Fail(
                    exception is ExternalServiceException external
                        ? external.Message
                        : "The booking saga failed.",
                    UtcNow());
                await unitOfWork.SaveChangesAsync(cancellationToken);
                await PublishStatusAsync(booking, cancellationToken);
            }

            return ToResponse(booking);
        }
    }

    private async Task TryCompensateInventoryAsync(
        Guid holdId,
        BookingType type,
        CancellationToken cancellationToken)
    {
        try
        {
            await inventory.ReleaseAsync(holdId, type, cancellationToken);
        }
        catch (Exception exception)
        {
            logger.LogError(
                exception,
                "Inventory compensation failed for hold {HoldId}.",
                holdId);
        }
    }

    private async Task TryCompensatePaymentAsync(
        Guid transactionId,
        CancellationToken cancellationToken)
    {
        try
        {
            await payments.VoidAsync(
                transactionId,
                "Booking saga compensation",
                cancellationToken);
        }
        catch (Exception exception)
        {
            logger.LogError(
                exception,
                "Payment compensation failed for transaction {TransactionId}.",
                transactionId);
        }
    }

    private async Task<BookingEntity?> FindExistingAsync(
        Guid userId,
        string? idempotencyKey,
        CancellationToken cancellationToken)
    {
        var normalized = NormalizeIdempotencyKey(idempotencyKey);
        return normalized is null
            ? null
            : await bookings.GetByUserAndIdempotencyKeyAsync(
                userId,
                normalized,
                cancellationToken);
    }

    private async Task PublishCreatedAsync(
        BookingEntity booking,
        CancellationToken cancellationToken)
    {
        await events.PublishAsync(
            new BookingCreatedEvent(
                booking.Id,
                booking.PassengerName ?? "Traveler",
                booking.Type.ToString(),
                booking.CreatedAtUtc),
            cancellationToken);
    }

    private async Task PublishStatusAsync(
        BookingEntity booking,
        CancellationToken cancellationToken)
    {
        await events.PublishAsync(
            new BookingStatusChangedEvent(
                booking.Id,
                booking.UserId,
                booking.Status.ToString(),
                UtcNow(),
                booking.FailureReason),
            cancellationToken);

        switch (booking.Status)
        {
            case BookingStatus.Confirmed:
                await events.PublishAsync(
                    new BookingConfirmedEvent(
                        booking.Id,
                        booking.UserId,
                        booking.Type.ToString(),
                        booking.TotalAmount,
                        booking.Currency,
                        UtcNow()),
                    cancellationToken);
                break;
            case BookingStatus.Failed:
                await events.PublishAsync(
                    new BookingFailedEvent(
                        booking.Id,
                        booking.UserId,
                        booking.FailureReason ?? "Booking failed.",
                        UtcNow()),
                    cancellationToken);
                break;
            case BookingStatus.Cancelling:
                await events.PublishAsync(
                    new BookingCancellationStartedEvent(
                        booking.Id,
                        booking.UserId,
                        booking.FailureReason,
                        UtcNow()),
                    cancellationToken);
                break;
            case BookingStatus.Cancelled:
                await events.PublishAsync(
                    new BookingCancelledEvent(
                        booking.Id,
                        booking.UserId,
                        UtcNow()),
                    cancellationToken);
                break;
        }
    }

    private void EnsureOwnerOrAdmin(BookingEntity booking)
    {
        if (!currentUser.HasPermission(PermissionCatalog.BookingsReadAll))
        {
            EnsureUser(booking.UserId);
        }
    }

    private void EnsureUser(Guid userId)
    {
        if (currentUser.HasPermission(PermissionCatalog.BookingsReadAll))
        {
            return;
        }

        var current = currentUser.GetRequiredUserId();
        if (current != userId)
        {
            throw new UnauthorizedException(
                "You are not allowed to access this resource.");
        }
    }

    private DateTime UtcNow() =>
        timeProvider.GetUtcNow().UtcDateTime;

    private static BookingResponse ToResponse(BookingEntity booking) =>
        new(
            booking.Id,
            booking.UserId,
            booking.Type,
            booking.Status,
            booking.TotalAmount,
            booking.Currency,
            booking.InventoryHoldId,
            booking.PaymentTransactionId,
            booking.OrderId,
            booking.HotelId,
            booking.CheckIn,
            booking.CheckOut,
            booking.FlightId,
            booking.FlightDate,
            booking.PassengerName,
            booking.FailureReason,
            booking.CreatedAtUtc,
            booking.UpdatedAtUtc,
            booking.ConfirmedAtUtc,
            booking.CancelledAtUtc,
            booking.Items.Select(item => new BookingItemResponse(
                item.ResourceTypeId,
                item.Quantity,
                item.UnitAmount)).ToArray());

    private static void ValidateCommon(
        decimal amount,
        string currency,
        string paymentMethodToken)
    {
        if (amount < 0)
        {
            throw new ValidationException("Total amount cannot be negative.");
        }

        if (string.IsNullOrWhiteSpace(currency)
            || currency.Trim().Length != 3)
        {
            throw new ValidationException(
                "Currency must be a three-letter ISO code.");
        }

        if (string.IsNullOrWhiteSpace(paymentMethodToken))
        {
            throw new ValidationException("Payment method token is required.");
        }
    }

    private static void ValidateItems<T>(IReadOnlyList<T>? items)
    {
        if (items is null || items.Count == 0)
        {
            throw new ValidationException(
                "At least one booking item is required.");
        }
    }

    private static string NormalizeCurrency(string currency) =>
        currency.Trim().ToUpperInvariant();

    private static string? NormalizeIdempotencyKey(string? key) =>
        string.IsNullOrWhiteSpace(key) ? null : key.Trim();

    private static void ValidatePaging(ref int page, ref int pageSize)
    {
        page = Math.Max(page, 1);
        pageSize = Math.Clamp(pageSize, 1, 100);
    }

    private static void ValidatePaging(ref BookingSearchQuery query)
    {
        query = query with
        {
            Page = Math.Max(query.Page, 1),
            PageSize = Math.Clamp(query.PageSize, 1, 100)
        };
    }
}
