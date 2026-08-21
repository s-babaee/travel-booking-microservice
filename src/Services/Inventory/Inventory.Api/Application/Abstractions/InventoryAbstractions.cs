using Inventory.Api.Application.Contracts;
using Inventory.Api.Domain.Entities;

namespace Inventory.Api.Application.Abstractions;

public interface IUnitOfWork
{
    Task<int> SaveChangesAsync(CancellationToken cancellationToken);
    Task<IUnitOfWorkTransaction> BeginTransactionAsync(
        CancellationToken cancellationToken);
}

public interface IUnitOfWorkTransaction : IAsyncDisposable
{
    Task CommitAsync(CancellationToken cancellationToken);
}

public interface IIntegrationEventPublisher
{
    Task PublishAsync<TEvent>(
        TEvent @event,
        CancellationToken cancellationToken)
        where TEvent : class;
}

public interface IHotelInventoryRepository
{
    Task<IReadOnlyList<HotelInventoryDay>> GetForUpdateAsync(
        Guid hotelId,
        IReadOnlyCollection<Guid> roomTypeIds,
        DateOnly from,
        DateOnly to,
        CancellationToken cancellationToken);

    Task<HotelInventoryDay?> GetForUpdateAsync(
        Guid hotelId,
        Guid roomTypeId,
        DateOnly date,
        CancellationToken cancellationToken);

    Task<IReadOnlyList<HotelInventoryDay>> ListAsync(
        Guid hotelId,
        DateOnly from,
        DateOnly to,
        Guid? roomTypeId,
        CancellationToken cancellationToken);

    Task AddAsync(
        HotelInventoryDay inventory,
        CancellationToken cancellationToken);

    Task EnsureExistsAsync(
        Guid hotelId,
        Guid roomTypeId,
        DateOnly date,
        DateTime nowUtc,
        CancellationToken cancellationToken);
}

public interface IFlightInventoryRepository
{
    Task<IReadOnlyList<FlightInventoryDay>> GetForUpdateAsync(
        Guid flightId,
        IReadOnlyCollection<Guid> flightClassIds,
        DateOnly date,
        CancellationToken cancellationToken);

    Task<FlightInventoryDay?> GetForUpdateAsync(
        Guid flightId,
        Guid flightClassId,
        DateOnly date,
        CancellationToken cancellationToken);

    Task<IReadOnlyList<FlightInventoryDay>> ListAsync(
        Guid flightId,
        DateOnly date,
        Guid? flightClassId,
        CancellationToken cancellationToken);

    Task AddAsync(
        FlightInventoryDay inventory,
        CancellationToken cancellationToken);

    Task EnsureExistsAsync(
        Guid flightId,
        Guid flightClassId,
        DateOnly date,
        DateTime nowUtc,
        CancellationToken cancellationToken);
}

public interface IHotelInventoryHoldRepository
{
    Task<HotelInventoryHold?> GetByIdAsync(
        Guid id,
        CancellationToken cancellationToken);

    Task AddAsync(
        HotelInventoryHold hold,
        CancellationToken cancellationToken);

    Task<IReadOnlyList<HotelInventoryHold>> ListExpiredAsync(
        DateTime nowUtc,
        CancellationToken cancellationToken);
}

public interface IFlightInventoryHoldRepository
{
    Task<FlightInventoryHold?> GetByIdAsync(
        Guid id,
        CancellationToken cancellationToken);

    Task AddAsync(
        FlightInventoryHold hold,
        CancellationToken cancellationToken);

    Task<IReadOnlyList<FlightInventoryHold>> ListExpiredAsync(
        DateTime nowUtc,
        CancellationToken cancellationToken);
}

public interface IHotelInventoryService
{
    Task<InventoryHoldResponse> HoldAsync(
        HotelHoldRequest request,
        CancellationToken cancellationToken);

    Task<InventoryHoldResponse> ConfirmAsync(
        ConfirmReleaseRequest request,
        CancellationToken cancellationToken);

    Task<InventoryHoldResponse> ReleaseAsync(
        ConfirmReleaseRequest request,
        CancellationToken cancellationToken);

    Task<IReadOnlyList<HotelAvailabilityResponse>> GetAvailabilityAsync(
        Guid hotelId,
        DateOnly from,
        DateOnly to,
        Guid? roomTypeId,
        CancellationToken cancellationToken);

    Task<IReadOnlyList<HotelAvailabilityResponse>> AdjustAsync(
        HotelInventoryAdjustmentRequest request,
        CancellationToken cancellationToken);

    Task ExpireAsync(
        DateTime nowUtc,
        CancellationToken cancellationToken);
}

public interface IFlightInventoryService
{
    Task<InventoryHoldResponse> HoldAsync(
        FlightHoldRequest request,
        CancellationToken cancellationToken);

    Task<InventoryHoldResponse> ConfirmAsync(
        ConfirmReleaseRequest request,
        CancellationToken cancellationToken);

    Task<InventoryHoldResponse> ReleaseAsync(
        ConfirmReleaseRequest request,
        CancellationToken cancellationToken);

    Task<IReadOnlyList<FlightAvailabilityResponse>> GetAvailabilityAsync(
        Guid flightId,
        DateOnly date,
        Guid? flightClassId,
        CancellationToken cancellationToken);

    Task<IReadOnlyList<FlightAvailabilityResponse>> AdjustAsync(
        FlightInventoryAdjustmentRequest request,
        CancellationToken cancellationToken);

    Task ExpireAsync(
        DateTime nowUtc,
        CancellationToken cancellationToken);
}
