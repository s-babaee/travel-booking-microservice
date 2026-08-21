using Booking.Api.Domain.Enums;

namespace Booking.Api.Application.Contracts;

public sealed record BookingRoomRequest(
    Guid RoomTypeId,
    int Quantity,
    decimal UnitAmount);

public sealed record CreateHotelBookingRequest(
    Guid HotelId,
    DateOnly CheckIn,
    DateOnly CheckOut,
    IReadOnlyList<BookingRoomRequest> Rooms,
    decimal TotalAmount,
    string Currency,
    string PaymentMethodToken,
    string? PassengerName = null,
    string? IdempotencyKey = null);

public sealed record BookingFlightClassRequest(
    Guid FlightClassId,
    int Quantity,
    decimal UnitAmount);

public sealed record CreateFlightBookingRequest(
    Guid FlightId,
    DateOnly Date,
    IReadOnlyList<BookingFlightClassRequest> Classes,
    decimal TotalAmount,
    string Currency,
    string PaymentMethodToken,
    string? PassengerName = null,
    string? IdempotencyKey = null);

public sealed record BookingItemResponse(
    Guid ResourceTypeId,
    int Quantity,
    decimal UnitAmount);

public sealed record BookingResponse(
    Guid Id,
    Guid UserId,
    BookingType Type,
    BookingStatus Status,
    decimal TotalAmount,
    string Currency,
    Guid? InventoryHoldId,
    Guid? PaymentTransactionId,
    Guid? OrderId,
    Guid? HotelId,
    DateOnly? CheckIn,
    DateOnly? CheckOut,
    Guid? FlightId,
    DateOnly? FlightDate,
    string? PassengerName,
    string? FailureReason,
    DateTime CreatedAtUtc,
    DateTime UpdatedAtUtc,
    DateTime? ConfirmedAtUtc,
    DateTime? CancelledAtUtc,
    IReadOnlyList<BookingItemResponse> Items);

public sealed record OrderResponse(
    Guid Id,
    Guid BookingId,
    Guid UserId,
    decimal TotalAmount,
    string Currency,
    DateTime CreatedAtUtc,
    BookingResponse Booking);

public sealed record CancelBookingRequest(string? Reason = null);

public sealed record AdminStatusChangeRequest(
    BookingStatus Status,
    string? Reason = null);

public sealed record BookingSearchQuery(
    Guid? UserId = null,
    BookingStatus? Status = null,
    Domain.Enums.BookingType? Type = null,
    DateTime? FromUtc = null,
    DateTime? ToUtc = null,
    int Page = 1,
    int PageSize = 50);

public sealed record PagedResponse<T>(
    IReadOnlyList<T> Items,
    int Page,
    int PageSize,
    int TotalCount);

public sealed record BookingStatsResponse(
    int Total,
    int Pending,
    int Confirmed,
    int Cancelled,
    int Failed,
    decimal ConfirmedAmount);
