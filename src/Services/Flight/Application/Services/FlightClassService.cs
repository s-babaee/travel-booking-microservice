using BuildingBlocks.Contracts.Events;
using Flight.Api.Application.Abstractions;
using Flight.Api.Application.Contracts;
using Flight.Api.Application.Exceptions;
using Flight.Api.Application.Mapping;
using Flight.Api.Domain.Entities;

namespace Flight.Api.Application.Services;

public sealed class FlightClassService : IFlightClassService
{
    private readonly IFlightRepository _flights;
    private readonly IFlightClassRepository _classes;
    private readonly IUnitOfWork _unitOfWork;
    private readonly IIntegrationEventPublisher _eventPublisher;
    private readonly TimeProvider _timeProvider;

    public FlightClassService(
        IFlightRepository flights,
        IFlightClassRepository classes,
        IUnitOfWork unitOfWork,
        IIntegrationEventPublisher eventPublisher,
        TimeProvider timeProvider)
    {
        _flights = flights;
        _classes = classes;
        _unitOfWork = unitOfWork;
        _eventPublisher = eventPublisher;
        _timeProvider = timeProvider;
    }

    public async Task<FlightClassResponse> CreateAsync(
        Guid flightId,
        CreateFlightClassRequest request,
        CancellationToken cancellationToken)
    {
        ArgumentNullException.ThrowIfNull(request);
        await EnsureFlightAsync(flightId, cancellationToken);
        await EnsureUniqueCodeAsync(flightId, request.Code, null, cancellationToken);
        var now = UtcNow();
        var flightClass = FlightClass.Create(
            Guid.NewGuid(),
            flightId,
            request.Code,
            request.Name,
            request.Type,
            request.Capacity,
            request.BasePrice,
            request.Currency,
            now);
        await _classes.AddAsync(flightClass, cancellationToken);
        await _unitOfWork.SaveChangesAsync(cancellationToken);
        await _eventPublisher.PublishAsync(
            new FlightClassCreated(
                flightClass.Id,
                flightClass.FlightId,
                flightClass.Code,
                flightClass.Name,
                flightClass.Capacity,
                now),
            cancellationToken);
        return flightClass.ToResponse();
    }

    public async Task<IReadOnlyList<FlightClassResponse>> ListByFlightAsync(
        Guid flightId,
        CancellationToken cancellationToken)
    {
        await EnsureFlightAsync(flightId, cancellationToken);
        return (await _classes.ListByFlightAsync(flightId, cancellationToken))
            .Select(flightClass => flightClass.ToResponse())
            .ToArray();
    }

    public async Task<FlightClassResponse> GetAsync(
        Guid classId,
        CancellationToken cancellationToken)
    {
        var flightClass = await GetOrThrowAsync(classId, cancellationToken);
        return flightClass.ToResponse();
    }

    public async Task<FlightClassResponse> UpdateAsync(
        Guid classId,
        UpdateFlightClassRequest request,
        CancellationToken cancellationToken)
    {
        ArgumentNullException.ThrowIfNull(request);
        var flightClass = await GetOrThrowAsync(classId, cancellationToken);
        await EnsureUniqueCodeAsync(
            flightClass.FlightId,
            request.Code,
            classId,
            cancellationToken);
        var now = UtcNow();
        flightClass.Update(
            request.Code,
            request.Name,
            request.Type,
            request.Capacity,
            request.BasePrice,
            request.Currency,
            now);
        await _unitOfWork.SaveChangesAsync(cancellationToken);
        await _eventPublisher.PublishAsync(
            new FlightClassUpdated(
                flightClass.Id,
                flightClass.FlightId,
                flightClass.Code,
                flightClass.Name,
                flightClass.Capacity,
                now),
            cancellationToken);
        return flightClass.ToResponse();
    }

    public async Task<FlightClassResponse> ChangeStatusAsync(
        Guid classId,
        ChangeFlightClassStatusRequest request,
        CancellationToken cancellationToken)
    {
        ArgumentNullException.ThrowIfNull(request);
        var flightClass = await GetOrThrowAsync(classId, cancellationToken);
        var now = UtcNow();
        flightClass.ChangeStatus(request.Status, now);
        await _unitOfWork.SaveChangesAsync(cancellationToken);
        await _eventPublisher.PublishAsync(
            new FlightClassStatusChanged(
                flightClass.Id,
                flightClass.FlightId,
                flightClass.Status.ToString(),
                now),
            cancellationToken);
        return flightClass.ToResponse();
    }

    public async Task DeleteAsync(Guid classId, CancellationToken cancellationToken)
    {
        var flightClass = await GetOrThrowAsync(classId, cancellationToken);
        var now = UtcNow();
        flightClass.SoftDelete(now);
        await _unitOfWork.SaveChangesAsync(cancellationToken);
        await _eventPublisher.PublishAsync(
            new FlightClassDeleted(flightClass.Id, flightClass.FlightId, now),
            cancellationToken);
    }

    private async Task<FlightClass> GetOrThrowAsync(
        Guid classId,
        CancellationToken cancellationToken)
    {
        var flightClass = await _classes.GetByIdAsync(classId, cancellationToken);
        return flightClass ?? throw new NotFoundException("Flight class", classId);
    }

    private async Task EnsureFlightAsync(Guid flightId, CancellationToken cancellationToken)
    {
        if (await _flights.GetByIdAsync(flightId, cancellationToken) is null)
        {
            throw new NotFoundException("Flight", flightId);
        }
    }

    private async Task EnsureUniqueCodeAsync(
        Guid flightId,
        string code,
        Guid? excludingId,
        CancellationToken cancellationToken)
    {
        if (await _classes.ExistsByCodeAsync(flightId, code, excludingId, cancellationToken))
        {
            throw new ConflictException(
                $"A non-deleted flight class with code '{code.Trim().ToUpperInvariant()}' already exists for this flight.");
        }
    }

    private DateTime UtcNow() => _timeProvider.GetUtcNow().UtcDateTime;
}
