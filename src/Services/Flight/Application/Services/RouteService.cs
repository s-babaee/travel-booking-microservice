using BuildingBlocks.Contracts.Events;
using Flight.Api.Application.Abstractions;
using Flight.Api.Application.Contracts;
using Flight.Api.Application.Exceptions;
using Flight.Api.Application.Mapping;
using Flight.Api.Domain.Entities;
using RouteEntity = Flight.Api.Domain.Entities.Route;

namespace Flight.Api.Application.Services;

public sealed class RouteService : IRouteService
{
    private readonly IRouteRepository _routes;
    private readonly IUnitOfWork _unitOfWork;
    private readonly IIntegrationEventPublisher _eventPublisher;
    private readonly TimeProvider _timeProvider;

    public RouteService(
        IRouteRepository routes,
        IUnitOfWork unitOfWork,
        IIntegrationEventPublisher eventPublisher,
        TimeProvider timeProvider)
    {
        _routes = routes;
        _unitOfWork = unitOfWork;
        _eventPublisher = eventPublisher;
        _timeProvider = timeProvider;
    }

    public async Task<RouteResponse> CreateAsync(
        CreateRouteRequest request,
        CancellationToken cancellationToken)
    {
        ArgumentNullException.ThrowIfNull(request);
        await EnsureUniqueAsync(request.OriginAirportCode, request.DestinationAirportCode, null, cancellationToken);
        var now = UtcNow();
        var route = RouteEntity.Create(
            Guid.NewGuid(),
            request.OriginAirportCode,
            request.DestinationAirportCode,
            request.OriginCity,
            request.DestinationCity,
            request.DistanceKm,
            request.TypicalDurationMinutes,
            now);
        await _routes.AddAsync(route, cancellationToken);
        await _unitOfWork.SaveChangesAsync(cancellationToken);
        await _eventPublisher.PublishAsync(
            new RouteCreated(route.Id, route.OriginAirportCode, route.DestinationAirportCode, now),
            cancellationToken);
        return route.ToResponse();
    }

    public async Task<RouteResponse> GetAsync(
        Guid routeId,
        CancellationToken cancellationToken)
    {
        var route = await GetOrThrowAsync(routeId, cancellationToken);
        return route.ToResponse();
    }

    public async Task<IReadOnlyList<RouteResponse>> ListAsync(
        CancellationToken cancellationToken)
    {
        return (await _routes.ListAsync(cancellationToken))
            .Select(route => route.ToResponse())
            .ToArray();
    }

    public async Task<RouteResponse> UpdateAsync(
        Guid routeId,
        UpdateRouteRequest request,
        CancellationToken cancellationToken)
    {
        ArgumentNullException.ThrowIfNull(request);
        var route = await GetOrThrowAsync(routeId, cancellationToken);
        await EnsureUniqueAsync(
            request.OriginAirportCode,
            request.DestinationAirportCode,
            routeId,
            cancellationToken);
        var now = UtcNow();
        route.Update(
            request.OriginAirportCode,
            request.DestinationAirportCode,
            request.OriginCity,
            request.DestinationCity,
            request.DistanceKm,
            request.TypicalDurationMinutes,
            now);
        await _unitOfWork.SaveChangesAsync(cancellationToken);
        await _eventPublisher.PublishAsync(
            new RouteUpdated(route.Id, route.OriginAirportCode, route.DestinationAirportCode, now),
            cancellationToken);
        return route.ToResponse();
    }

    public async Task DeleteAsync(Guid routeId, CancellationToken cancellationToken)
    {
        var route = await GetOrThrowAsync(routeId, cancellationToken);
        var now = UtcNow();
        route.SoftDelete(now);
        await _unitOfWork.SaveChangesAsync(cancellationToken);
        await _eventPublisher.PublishAsync(new RouteDeleted(route.Id, now), cancellationToken);
    }

    private async Task<RouteEntity> GetOrThrowAsync(Guid routeId, CancellationToken cancellationToken)
    {
        var route = await _routes.GetByIdAsync(routeId, cancellationToken);
        return route ?? throw new NotFoundException("Route", routeId);
    }

    private async Task EnsureUniqueAsync(
        string origin,
        string destination,
        Guid? excludingId,
        CancellationToken cancellationToken)
    {
        if (await _routes.ExistsAsync(origin, destination, excludingId, cancellationToken))
        {
            throw new ConflictException(
                $"A non-deleted route from '{origin.Trim().ToUpperInvariant()}' to '{destination.Trim().ToUpperInvariant()}' already exists.");
        }
    }

    private DateTime UtcNow() => _timeProvider.GetUtcNow().UtcDateTime;
}
