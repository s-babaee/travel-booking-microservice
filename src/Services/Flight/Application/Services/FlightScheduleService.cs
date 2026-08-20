using BuildingBlocks.Contracts.Events;
using Flight.Api.Application.Abstractions;
using Flight.Api.Application.Contracts;
using Flight.Api.Application.Exceptions;
using Flight.Api.Application.Mapping;
using Flight.Api.Domain.Entities;

namespace Flight.Api.Application.Services;

public sealed class FlightScheduleService : IFlightScheduleService
{
    private readonly IFlightRepository _flights;
    private readonly IFlightScheduleRepository _schedules;
    private readonly IUnitOfWork _unitOfWork;
    private readonly IIntegrationEventPublisher _eventPublisher;
    private readonly TimeProvider _timeProvider;

    public FlightScheduleService(
        IFlightRepository flights,
        IFlightScheduleRepository schedules,
        IUnitOfWork unitOfWork,
        IIntegrationEventPublisher eventPublisher,
        TimeProvider timeProvider)
    {
        _flights = flights;
        _schedules = schedules;
        _unitOfWork = unitOfWork;
        _eventPublisher = eventPublisher;
        _timeProvider = timeProvider;
    }

    public async Task<FlightScheduleResponse> CreateAsync(
        Guid flightId,
        CreateFlightScheduleRequest request,
        CancellationToken cancellationToken)
    {
        ArgumentNullException.ThrowIfNull(request);
        await EnsureFlightAsync(flightId, cancellationToken);
        var now = UtcNow();
        var schedule = FlightSchedule.Create(
            Guid.NewGuid(),
            flightId,
            request.DepartureTime,
            request.ArrivalTime,
            request.OperatingDays,
            request.EffectiveFrom,
            request.EffectiveTo,
            request.TimeZoneId,
            now);
        await _schedules.AddAsync(schedule, cancellationToken);
        await _unitOfWork.SaveChangesAsync(cancellationToken);
        await _eventPublisher.PublishAsync(
            new FlightScheduleCreated(
                schedule.Id,
                schedule.FlightId,
                schedule.DepartureTime,
                schedule.ArrivalTime,
                now),
            cancellationToken);
        return schedule.ToResponse();
    }

    public async Task<IReadOnlyList<FlightScheduleResponse>> ListByFlightAsync(
        Guid flightId,
        CancellationToken cancellationToken)
    {
        await EnsureFlightAsync(flightId, cancellationToken);
        return (await _schedules.ListByFlightAsync(flightId, cancellationToken))
            .Select(schedule => schedule.ToResponse())
            .ToArray();
    }

    public async Task<FlightScheduleResponse> GetAsync(
        Guid scheduleId,
        CancellationToken cancellationToken)
    {
        var schedule = await GetOrThrowAsync(scheduleId, cancellationToken);
        return schedule.ToResponse();
    }

    public async Task<FlightScheduleResponse> UpdateAsync(
        Guid scheduleId,
        UpdateFlightScheduleRequest request,
        CancellationToken cancellationToken)
    {
        ArgumentNullException.ThrowIfNull(request);
        var schedule = await GetOrThrowAsync(scheduleId, cancellationToken);
        var now = UtcNow();
        schedule.Update(
            request.DepartureTime,
            request.ArrivalTime,
            request.OperatingDays,
            request.EffectiveFrom,
            request.EffectiveTo,
            request.TimeZoneId,
            now);
        await _unitOfWork.SaveChangesAsync(cancellationToken);
        await _eventPublisher.PublishAsync(
            new FlightScheduleUpdated(
                schedule.Id,
                schedule.FlightId,
                schedule.DepartureTime,
                schedule.ArrivalTime,
                now),
            cancellationToken);
        return schedule.ToResponse();
    }

    public async Task DeleteAsync(Guid scheduleId, CancellationToken cancellationToken)
    {
        var schedule = await GetOrThrowAsync(scheduleId, cancellationToken);
        var now = UtcNow();
        schedule.SoftDelete(now);
        await _unitOfWork.SaveChangesAsync(cancellationToken);
        await _eventPublisher.PublishAsync(
            new FlightScheduleDeleted(schedule.Id, schedule.FlightId, now),
            cancellationToken);
    }

    private async Task<FlightSchedule> GetOrThrowAsync(
        Guid scheduleId,
        CancellationToken cancellationToken)
    {
        var schedule = await _schedules.GetByIdAsync(scheduleId, cancellationToken);
        return schedule ?? throw new NotFoundException("Flight schedule", scheduleId);
    }

    private async Task EnsureFlightAsync(Guid flightId, CancellationToken cancellationToken)
    {
        if (await _flights.GetByIdAsync(flightId, cancellationToken) is null)
        {
            throw new NotFoundException("Flight", flightId);
        }
    }

    private DateTime UtcNow() => _timeProvider.GetUtcNow().UtcDateTime;
}
