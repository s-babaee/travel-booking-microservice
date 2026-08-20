using BuildingBlocks.Contracts.Events;
using Flight.Api.Application.Abstractions;
using Flight.Api.Application.Contracts;
using Flight.Api.Application.Exceptions;
using Flight.Api.Application.Mapping;
using Flight.Api.Domain.Entities;

namespace Flight.Api.Application.Services;

public sealed class AirlineService : IAirlineService
{
    private readonly IAirlineRepository _airlines;
    private readonly IUnitOfWork _unitOfWork;
    private readonly IIntegrationEventPublisher _eventPublisher;
    private readonly TimeProvider _timeProvider;

    public AirlineService(
        IAirlineRepository airlines,
        IUnitOfWork unitOfWork,
        IIntegrationEventPublisher eventPublisher,
        TimeProvider timeProvider)
    {
        _airlines = airlines;
        _unitOfWork = unitOfWork;
        _eventPublisher = eventPublisher;
        _timeProvider = timeProvider;
    }

    public async Task<AirlineResponse> CreateAsync(
        CreateAirlineRequest request,
        CancellationToken cancellationToken)
    {
        ArgumentNullException.ThrowIfNull(request);
        await EnsureUniqueAsync(request, null, cancellationToken);
        var now = UtcNow();
        var airline = Airline.Create(
            Guid.NewGuid(),
            request.Name,
            request.IataCode,
            request.IcaoCode,
            request.Country,
            request.WebsiteUrl,
            now);
        await _airlines.AddAsync(airline, cancellationToken);
        await _unitOfWork.SaveChangesAsync(cancellationToken);
        await _eventPublisher.PublishAsync(
            new AirlineCreated(
                airline.Id,
                airline.Name,
                airline.IataCode,
                airline.IcaoCode,
                now),
            cancellationToken);
        return airline.ToResponse();
    }

    public async Task<AirlineResponse> GetAsync(
        Guid airlineId,
        CancellationToken cancellationToken)
    {
        var airline = await GetOrThrowAsync(airlineId, cancellationToken);
        return airline.ToResponse();
    }

    public async Task<IReadOnlyList<AirlineResponse>> ListAsync(
        CancellationToken cancellationToken)
    {
        return (await _airlines.ListAsync(cancellationToken))
            .Select(airline => airline.ToResponse())
            .ToArray();
    }

    public async Task<AirlineResponse> UpdateAsync(
        Guid airlineId,
        UpdateAirlineRequest request,
        CancellationToken cancellationToken)
    {
        ArgumentNullException.ThrowIfNull(request);
        var airline = await GetOrThrowAsync(airlineId, cancellationToken);
        await EnsureUniqueAsync(request, airlineId, cancellationToken);
        var now = UtcNow();
        airline.Update(
            request.Name,
            request.IataCode,
            request.IcaoCode,
            request.Country,
            request.WebsiteUrl,
            now);
        await _unitOfWork.SaveChangesAsync(cancellationToken);
        await _eventPublisher.PublishAsync(
            new AirlineUpdated(
                airline.Id,
                airline.Name,
                airline.IataCode,
                airline.IcaoCode,
                now),
            cancellationToken);
        return airline.ToResponse();
    }

    public async Task DeleteAsync(
        Guid airlineId,
        CancellationToken cancellationToken)
    {
        var airline = await GetOrThrowAsync(airlineId, cancellationToken);
        var now = UtcNow();
        airline.SoftDelete(now);
        await _unitOfWork.SaveChangesAsync(cancellationToken);
        await _eventPublisher.PublishAsync(new AirlineDeleted(airline.Id, now), cancellationToken);
    }

    private async Task<Airline> GetOrThrowAsync(
        Guid airlineId,
        CancellationToken cancellationToken)
    {
        var airline = await _airlines.GetByIdAsync(airlineId, cancellationToken);
        return airline ?? throw new NotFoundException("Airline", airlineId);
    }

    private async Task EnsureUniqueAsync(
        CreateAirlineRequest request,
        Guid? excludingId,
        CancellationToken cancellationToken)
    {
        if (await _airlines.ExistsByNameAsync(request.Name, excludingId, cancellationToken))
        {
            throw new ConflictException(
                $"A non-deleted airline named '{request.Name.Trim()}' already exists.");
        }

        if (await _airlines.ExistsByCodeAsync(
                request.IataCode,
                request.IcaoCode,
                excludingId,
                cancellationToken))
        {
            throw new ConflictException(
                $"An airline with IATA code '{request.IataCode.Trim().ToUpperInvariant()}' or ICAO code '{request.IcaoCode.Trim().ToUpperInvariant()}' already exists.");
        }
    }

    private DateTime UtcNow() => _timeProvider.GetUtcNow().UtcDateTime;
}
