using Flight.Api.Domain.Entities;
using FlightEntity = Flight.Api.Domain.Entities.Flight;
using RouteEntity = Flight.Api.Domain.Entities.Route;

namespace Flight.Api.Application.Abstractions;

public interface IUnitOfWork
{
    Task<int> SaveChangesAsync(CancellationToken cancellationToken);
}

public interface IIntegrationEventPublisher
{
    Task PublishAsync<TEvent>(TEvent @event, CancellationToken cancellationToken)
        where TEvent : class;
}

public interface IAirlineRepository
{
    Task<Airline?> GetByIdAsync(Guid id, CancellationToken cancellationToken);
    Task<IReadOnlyList<Airline>> ListAsync(CancellationToken cancellationToken);
    Task<bool> ExistsByNameAsync(string name, Guid? excludingId, CancellationToken cancellationToken);
    Task<bool> ExistsByCodeAsync(string iataCode, string icaoCode, Guid? excludingId, CancellationToken cancellationToken);
    Task AddAsync(Airline airline, CancellationToken cancellationToken);
}

public interface IRouteRepository
{
    Task<RouteEntity?> GetByIdAsync(Guid id, CancellationToken cancellationToken);
    Task<IReadOnlyList<RouteEntity>> ListAsync(CancellationToken cancellationToken);
    Task<bool> ExistsAsync(string originAirportCode, string destinationAirportCode, Guid? excludingId, CancellationToken cancellationToken);
    Task AddAsync(RouteEntity route, CancellationToken cancellationToken);
}

public interface IFlightRepository
{
    Task<FlightEntity?> GetByIdAsync(Guid id, CancellationToken cancellationToken);
    Task<bool> ExistsByNumberAsync(Guid airlineId, string flightNumber, Guid? excludingId, CancellationToken cancellationToken);
    Task AddAsync(FlightEntity flight, CancellationToken cancellationToken);
}

public interface IFlightScheduleRepository
{
    Task<FlightSchedule?> GetByIdAsync(Guid id, CancellationToken cancellationToken);
    Task<IReadOnlyList<FlightSchedule>> ListByFlightAsync(Guid flightId, CancellationToken cancellationToken);
    Task AddAsync(FlightSchedule schedule, CancellationToken cancellationToken);
    void Remove(FlightSchedule schedule);
}

public interface IFlightClassRepository
{
    Task<FlightClass?> GetByIdAsync(Guid id, CancellationToken cancellationToken);
    Task<IReadOnlyList<FlightClass>> ListByFlightAsync(Guid flightId, CancellationToken cancellationToken);
    Task<bool> ExistsByCodeAsync(Guid flightId, string code, Guid? excludingId, CancellationToken cancellationToken);
    Task AddAsync(FlightClass flightClass, CancellationToken cancellationToken);
}

public interface IFlightPolicyRepository
{
    Task<FlightPolicy?> GetByIdAsync(Guid id, CancellationToken cancellationToken);
    Task<IReadOnlyList<FlightPolicy>> ListByFlightAsync(Guid flightId, CancellationToken cancellationToken);
    Task AddAsync(FlightPolicy policy, CancellationToken cancellationToken);
    void Remove(FlightPolicy policy);
}
