using Booking.Api.Application.Contracts;
using Booking.Api.Domain.Entities;
using Booking.Api.Domain.Enums;
using BookingEntity = Booking.Api.Domain.Entities.Booking;

namespace Booking.Api.Application.Abstractions;

public interface IBookingRepository
{
    Task<BookingEntity?> GetByIdAsync(
        Guid id,
        CancellationToken cancellationToken);

    Task<BookingEntity?> GetByUserAndIdempotencyKeyAsync(
        Guid userId,
        string idempotencyKey,
        CancellationToken cancellationToken);

    Task<IReadOnlyList<BookingEntity>> ListByUserAsync(
        Guid userId,
        int page,
        int pageSize,
        CancellationToken cancellationToken);

    Task<(IReadOnlyList<BookingEntity> Items, int TotalCount)> SearchAsync(
        BookingSearchQuery query,
        CancellationToken cancellationToken);

    Task<BookingStatsResponse> GetStatsAsync(
        DateTime? fromUtc,
        DateTime? toUtc,
        CancellationToken cancellationToken);

    Task AddAsync(BookingEntity booking, CancellationToken cancellationToken);
}

public interface IOrderRepository
{
    Task<Order?> GetByIdAsync(Guid id, CancellationToken cancellationToken);
    Task<IReadOnlyList<Order>> ListAsync(
        int page,
        int pageSize,
        CancellationToken cancellationToken);
    Task AddAsync(Order order, CancellationToken cancellationToken);
}

public interface IUnitOfWork
{
    Task<int> SaveChangesAsync(CancellationToken cancellationToken);
}

public interface IInventoryGateway
{
    Task<InventoryHoldResult> HoldHotelAsync(
        HoldHotelCommand command,
        CancellationToken cancellationToken);

    Task<InventoryHoldResult> HoldFlightAsync(
        HoldFlightCommand command,
        CancellationToken cancellationToken);

    Task ConfirmAsync(Guid holdId, BookingType type, CancellationToken cancellationToken);
    Task ReleaseAsync(Guid holdId, BookingType type, CancellationToken cancellationToken);
}

public interface IPaymentGateway
{
    Task<PaymentAuthorizationResult> AuthorizeAsync(
        PaymentAuthorizationCommand command,
        CancellationToken cancellationToken);

    Task VoidAsync(
        Guid transactionId,
        string reason,
        CancellationToken cancellationToken);
}

public interface ICurrentUser
{
    Guid GetRequiredUserId();
    bool IsAdmin();
}

public interface IBookingEventPublisher
{
    Task PublishAsync<TEvent>(
        TEvent @event,
        CancellationToken cancellationToken)
        where TEvent : class;
}

public sealed record HoldHotelCommand(
    Guid HoldId,
    Guid HotelId,
    DateOnly CheckIn,
    DateOnly CheckOut,
    IReadOnlyList<InventoryRoomCommand> Rooms,
    DateTime ExpiresAtUtc);

public sealed record InventoryRoomCommand(Guid RoomTypeId, int Quantity);

public sealed record HoldFlightCommand(
    Guid HoldId,
    Guid FlightId,
    DateOnly Date,
    IReadOnlyList<InventoryFlightClassCommand> Classes,
    DateTime ExpiresAtUtc);

public sealed record InventoryFlightClassCommand(Guid FlightClassId, int Quantity);

public sealed record InventoryHoldResult(
    Guid HoldId,
    string Status,
    DateTime ExpiresAtUtc,
    DateTime? CompletedAtUtc);

public sealed record PaymentAuthorizationCommand(
    Guid BookingId,
    Guid UserId,
    decimal Amount,
    string Currency,
    string PaymentMethodToken);

public sealed record PaymentAuthorizationResult(
    bool Succeeded,
    Guid? TransactionId,
    string? FailureReason);
