using Flight.Api.Application.Contracts;

namespace Flight.Api.Application.Abstractions;

public interface IFlightService
{
    Task<FlightResponse> CreateAsync(CreateFlightRequest request, CancellationToken cancellationToken);
    Task<FlightResponse> GetAsync(Guid flightId, CancellationToken cancellationToken);
    Task<FlightResponse> UpdateAsync(Guid flightId, UpdateFlightRequest request, CancellationToken cancellationToken);
    Task<FlightResponse> ChangeStatusAsync(Guid flightId, ChangeFlightStatusRequest request, CancellationToken cancellationToken);
    Task DeleteAsync(Guid flightId, CancellationToken cancellationToken);
}

public interface IRouteService
{
    Task<RouteResponse> CreateAsync(CreateRouteRequest request, CancellationToken cancellationToken);
    Task<RouteResponse> GetAsync(Guid routeId, CancellationToken cancellationToken);
    Task<IReadOnlyList<RouteResponse>> ListAsync(CancellationToken cancellationToken);
    Task<RouteResponse> UpdateAsync(Guid routeId, UpdateRouteRequest request, CancellationToken cancellationToken);
    Task DeleteAsync(Guid routeId, CancellationToken cancellationToken);
}

public interface IFlightScheduleService
{
    Task<FlightScheduleResponse> CreateAsync(Guid flightId, CreateFlightScheduleRequest request, CancellationToken cancellationToken);
    Task<IReadOnlyList<FlightScheduleResponse>> ListByFlightAsync(Guid flightId, CancellationToken cancellationToken);
    Task<FlightScheduleResponse> GetAsync(Guid scheduleId, CancellationToken cancellationToken);
    Task<FlightScheduleResponse> UpdateAsync(Guid scheduleId, UpdateFlightScheduleRequest request, CancellationToken cancellationToken);
    Task DeleteAsync(Guid scheduleId, CancellationToken cancellationToken);
}

public interface IFlightClassService
{
    Task<FlightClassResponse> CreateAsync(Guid flightId, CreateFlightClassRequest request, CancellationToken cancellationToken);
    Task<IReadOnlyList<FlightClassResponse>> ListByFlightAsync(Guid flightId, CancellationToken cancellationToken);
    Task<FlightClassResponse> GetAsync(Guid classId, CancellationToken cancellationToken);
    Task<FlightClassResponse> UpdateAsync(Guid classId, UpdateFlightClassRequest request, CancellationToken cancellationToken);
    Task<FlightClassResponse> ChangeStatusAsync(Guid classId, ChangeFlightClassStatusRequest request, CancellationToken cancellationToken);
    Task DeleteAsync(Guid classId, CancellationToken cancellationToken);
}

public interface IFlightPolicyService
{
    Task<FlightPolicyResponse> CreateAsync(Guid flightId, CreateFlightPolicyRequest request, CancellationToken cancellationToken);
    Task<IReadOnlyList<FlightPolicyResponse>> ListByFlightAsync(Guid flightId, CancellationToken cancellationToken);
    Task<FlightPolicyResponse> UpdateAsync(Guid flightId, Guid policyId, UpdateFlightPolicyRequest request, CancellationToken cancellationToken);
    Task DeleteAsync(Guid flightId, Guid policyId, CancellationToken cancellationToken);
}

public interface IAirlineService
{
    Task<AirlineResponse> CreateAsync(CreateAirlineRequest request, CancellationToken cancellationToken);
    Task<AirlineResponse> GetAsync(Guid airlineId, CancellationToken cancellationToken);
    Task<IReadOnlyList<AirlineResponse>> ListAsync(CancellationToken cancellationToken);
    Task<AirlineResponse> UpdateAsync(Guid airlineId, UpdateAirlineRequest request, CancellationToken cancellationToken);
    Task DeleteAsync(Guid airlineId, CancellationToken cancellationToken);
}
