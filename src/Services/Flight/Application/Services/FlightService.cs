using BuildingBlocks.Contracts.Events;
using Flight.Api.Application.Abstractions;
using Flight.Api.Application.Contracts;
using Flight.Api.Application.Exceptions;
using Flight.Api.Application.Mapping;
using Flight.Api.Domain.Entities;
using FlightEntity = Flight.Api.Domain.Entities.Flight;

namespace Flight.Api.Application.Services;

public sealed class FlightService : IFlightService
{
    private readonly IFlightRepository _flights;
    private readonly IAirlineRepository _airlines;
    private readonly IRouteRepository _routes;
    private readonly IFlightScheduleRepository _schedules;
    private readonly IFlightClassRepository _classes;
    private readonly IFlightPolicyRepository _policies;
    private readonly IUnitOfWork _unitOfWork;
    private readonly IIntegrationEventPublisher _eventPublisher;
    private readonly TimeProvider _timeProvider;

    public FlightService(
        IFlightRepository flights,
        IAirlineRepository airlines,
        IRouteRepository routes,
        IFlightScheduleRepository schedules,
        IFlightClassRepository classes,
        IFlightPolicyRepository policies,
        IUnitOfWork unitOfWork,
        IIntegrationEventPublisher eventPublisher,
        TimeProvider timeProvider)
    {
        _flights = flights;
        _airlines = airlines;
        _routes = routes;
        _schedules = schedules;
        _classes = classes;
        _policies = policies;
        _unitOfWork = unitOfWork;
        _eventPublisher = eventPublisher;
        _timeProvider = timeProvider;
    }

    public async Task<FlightResponse> CreateAsync(
        CreateFlightRequest request,
        CancellationToken cancellationToken)
    {
        ArgumentNullException.ThrowIfNull(request);
        await EnsureReferencesAsync(request.AirlineId, request.RouteId, cancellationToken);

        if (await _flights.ExistsByNumberAsync(
                request.AirlineId,
                request.FlightNumber,
                excludingId: null,
                cancellationToken))
        {
            throw new ConflictException(
                $"A non-deleted flight with number '{request.FlightNumber.Trim().ToUpperInvariant()}' already exists for this airline.");
        }

        var now = UtcNow();
        var flight = FlightEntity.Create(
            Guid.NewGuid(),
            request.AirlineId,
            request.RouteId,
            request.FlightNumber,
            request.AircraftType,
            request.Description,
            now);

        await _flights.AddAsync(flight, cancellationToken);
        await _unitOfWork.SaveChangesAsync(cancellationToken);
        await _eventPublisher.PublishAsync(
            new FlightCreated(
                flight.Id,
                flight.AirlineId,
                flight.RouteId,
                flight.FlightNumber,
                now),
            cancellationToken);

        return await BuildResponseAsync(flight, cancellationToken);
    }

    public async Task<FlightResponse> GetAsync(
        Guid flightId,
        CancellationToken cancellationToken)
    {
        var flight = await GetOrThrowAsync(flightId, cancellationToken);
        return await BuildResponseAsync(flight, cancellationToken);
    }

    public async Task<FlightResponse> UpdateAsync(
        Guid flightId,
        UpdateFlightRequest request,
        CancellationToken cancellationToken)
    {
        ArgumentNullException.ThrowIfNull(request);
        var flight = await GetOrThrowAsync(flightId, cancellationToken);
        await EnsureReferencesAsync(request.AirlineId, request.RouteId, cancellationToken);

        if (await _flights.ExistsByNumberAsync(
                request.AirlineId,
                request.FlightNumber,
                flightId,
                cancellationToken))
        {
            throw new ConflictException(
                $"A non-deleted flight with number '{request.FlightNumber.Trim().ToUpperInvariant()}' already exists for this airline.");
        }

        var now = UtcNow();
        flight.Update(
            request.AirlineId,
            request.RouteId,
            request.FlightNumber,
            request.AircraftType,
            request.Description,
            now);
        await _unitOfWork.SaveChangesAsync(cancellationToken);
        await _eventPublisher.PublishAsync(
            new FlightUpdated(
                flight.Id,
                flight.AirlineId,
                flight.RouteId,
                flight.FlightNumber,
                now),
            cancellationToken);

        return await BuildResponseAsync(flight, cancellationToken);
    }

    public async Task<FlightResponse> ChangeStatusAsync(
        Guid flightId,
        ChangeFlightStatusRequest request,
        CancellationToken cancellationToken)
    {
        ArgumentNullException.ThrowIfNull(request);
        var flight = await GetOrThrowAsync(flightId, cancellationToken);
        var now = UtcNow();
        flight.ChangeStatus(request.Status, now);
        await _unitOfWork.SaveChangesAsync(cancellationToken);
        await _eventPublisher.PublishAsync(
            new FlightStatusChanged(flight.Id, flight.Status.ToString(), now),
            cancellationToken);
        return await BuildResponseAsync(flight, cancellationToken);
    }

    public async Task DeleteAsync(Guid flightId, CancellationToken cancellationToken)
    {
        var flight = await GetOrThrowAsync(flightId, cancellationToken);
        var now = UtcNow();
        flight.SoftDelete(now);
        await _unitOfWork.SaveChangesAsync(cancellationToken);
        await _eventPublisher.PublishAsync(new FlightDeleted(flight.Id, now), cancellationToken);
    }

    private async Task<FlightEntity> GetOrThrowAsync(
        Guid flightId,
        CancellationToken cancellationToken)
    {
        var flight = await _flights.GetByIdAsync(flightId, cancellationToken);
        return flight ?? throw new NotFoundException("Flight", flightId);
    }

    private async Task EnsureReferencesAsync(
        Guid airlineId,
        Guid routeId,
        CancellationToken cancellationToken)
    {
        if (await _airlines.GetByIdAsync(airlineId, cancellationToken) is null)
        {
            throw new NotFoundException("Airline", airlineId);
        }

        if (await _routes.GetByIdAsync(routeId, cancellationToken) is null)
        {
            throw new NotFoundException("Route", routeId);
        }
    }

    private async Task<FlightResponse> BuildResponseAsync(
        FlightEntity flight,
        CancellationToken cancellationToken)
    {
        var schedules = (await _schedules.ListByFlightAsync(flight.Id, cancellationToken))
            .Select(schedule => schedule.ToResponse())
            .ToArray();
        var classes = (await _classes.ListByFlightAsync(flight.Id, cancellationToken))
            .Select(flightClass => flightClass.ToResponse())
            .ToArray();
        var policies = (await _policies.ListByFlightAsync(flight.Id, cancellationToken))
            .Select(policy => policy.ToResponse())
            .ToArray();
        return flight.ToResponse(schedules, classes, policies);
    }

    private DateTime UtcNow() => _timeProvider.GetUtcNow().UtcDateTime;
}
