using Flight.Api.Application.Contracts;
using Flight.Api.Domain.Entities;
using FlightEntity = Flight.Api.Domain.Entities.Flight;
using RouteEntity = Flight.Api.Domain.Entities.Route;

namespace Flight.Api.Application.Mapping;

public static class CatalogMappings
{
    public static AirlineResponse ToResponse(this Airline airline) =>
        new(
            airline.Id,
            airline.Name,
            airline.IataCode,
            airline.IcaoCode,
            airline.Country,
            airline.WebsiteUrl,
            airline.Status,
            airline.CreatedAtUtc,
            airline.UpdatedAtUtc);

    public static RouteResponse ToResponse(this RouteEntity route) =>
        new(
            route.Id,
            route.OriginAirportCode,
            route.DestinationAirportCode,
            route.OriginCity,
            route.DestinationCity,
            route.DistanceKm,
            route.TypicalDurationMinutes,
            route.CreatedAtUtc,
            route.UpdatedAtUtc);

    public static FlightScheduleResponse ToResponse(this FlightSchedule schedule) =>
        new(
            schedule.Id,
            schedule.FlightId,
            schedule.DepartureTime,
            schedule.ArrivalTime,
            schedule.OperatingDays,
            schedule.EffectiveFrom,
            schedule.EffectiveTo,
            schedule.TimeZoneId,
            schedule.CreatedAtUtc,
            schedule.UpdatedAtUtc);

    public static FlightClassResponse ToResponse(this FlightClass flightClass) =>
        new(
            flightClass.Id,
            flightClass.FlightId,
            flightClass.Code,
            flightClass.Name,
            flightClass.Type,
            flightClass.Capacity,
            flightClass.BasePrice,
            flightClass.Currency,
            flightClass.Status,
            flightClass.CreatedAtUtc,
            flightClass.UpdatedAtUtc);

    public static FlightPolicyResponse ToResponse(this FlightPolicy policy) =>
        new(
            policy.Id,
            policy.FlightId,
            policy.PolicyType,
            policy.Title,
            policy.Content,
            policy.BaggageAllowanceKg,
            policy.Refundable,
            policy.Changeable,
            policy.ChangeFee,
            policy.Conditions,
            policy.CreatedAtUtc,
            policy.UpdatedAtUtc);

    public static FlightResponse ToResponse(
        this FlightEntity flight,
        IReadOnlyList<FlightScheduleResponse> schedules,
        IReadOnlyList<FlightClassResponse> classes,
        IReadOnlyList<FlightPolicyResponse> policies) =>
        new(
            flight.Id,
            flight.AirlineId,
            flight.RouteId,
            flight.FlightNumber,
            flight.AircraftType,
            flight.Description,
            flight.Status,
            flight.CreatedAtUtc,
            flight.UpdatedAtUtc,
            schedules,
            classes,
            policies);
}
