using BuildingBlocks.Contracts.Events;
using Flight.Api.Application.Abstractions;
using Flight.Api.Application.Contracts;
using Flight.Api.Application.Exceptions;
using Flight.Api.Application.Mapping;
using Flight.Api.Domain.Entities;

namespace Flight.Api.Application.Services;

public sealed class FlightPolicyService : IFlightPolicyService
{
    private readonly IFlightRepository _flights;
    private readonly IFlightPolicyRepository _policies;
    private readonly IUnitOfWork _unitOfWork;
    private readonly IIntegrationEventPublisher _eventPublisher;
    private readonly TimeProvider _timeProvider;

    public FlightPolicyService(
        IFlightRepository flights,
        IFlightPolicyRepository policies,
        IUnitOfWork unitOfWork,
        IIntegrationEventPublisher eventPublisher,
        TimeProvider timeProvider)
    {
        _flights = flights;
        _policies = policies;
        _unitOfWork = unitOfWork;
        _eventPublisher = eventPublisher;
        _timeProvider = timeProvider;
    }

    public async Task<FlightPolicyResponse> CreateAsync(
        Guid flightId,
        CreateFlightPolicyRequest request,
        CancellationToken cancellationToken)
    {
        ArgumentNullException.ThrowIfNull(request);
        await EnsureFlightAsync(flightId, cancellationToken);
        var now = UtcNow();
        var policy = FlightPolicy.Create(
            Guid.NewGuid(),
            flightId,
            request.PolicyType,
            request.Title,
            request.Content,
            request.BaggageAllowanceKg,
            request.Refundable,
            request.Changeable,
            request.ChangeFee,
            request.Conditions,
            now);
        await _policies.AddAsync(policy, cancellationToken);
        await _unitOfWork.SaveChangesAsync(cancellationToken);
        await _eventPublisher.PublishAsync(
            new FlightPolicyCreated(policy.Id, policy.FlightId, policy.PolicyType, now),
            cancellationToken);
        return policy.ToResponse();
    }

    public async Task<IReadOnlyList<FlightPolicyResponse>> ListByFlightAsync(
        Guid flightId,
        CancellationToken cancellationToken)
    {
        await EnsureFlightAsync(flightId, cancellationToken);
        return (await _policies.ListByFlightAsync(flightId, cancellationToken))
            .Select(policy => policy.ToResponse())
            .ToArray();
    }

    public async Task<FlightPolicyResponse> UpdateAsync(
        Guid flightId,
        Guid policyId,
        UpdateFlightPolicyRequest request,
        CancellationToken cancellationToken)
    {
        ArgumentNullException.ThrowIfNull(request);
        await EnsureFlightAsync(flightId, cancellationToken);
        var policy = await _policies.GetByIdAsync(policyId, cancellationToken);
        if (policy is null || policy.FlightId != flightId)
        {
            throw new NotFoundException("Flight policy", policyId);
        }

        var now = UtcNow();
        policy.Update(
            request.PolicyType,
            request.Title,
            request.Content,
            request.BaggageAllowanceKg,
            request.Refundable,
            request.Changeable,
            request.ChangeFee,
            request.Conditions,
            now);
        await _unitOfWork.SaveChangesAsync(cancellationToken);
        await _eventPublisher.PublishAsync(
            new FlightPolicyUpdated(policy.Id, policy.FlightId, policy.PolicyType, now),
            cancellationToken);
        return policy.ToResponse();
    }

    public async Task DeleteAsync(
        Guid flightId,
        Guid policyId,
        CancellationToken cancellationToken)
    {
        await EnsureFlightAsync(flightId, cancellationToken);
        var policy = await _policies.GetByIdAsync(policyId, cancellationToken);
        if (policy is null || policy.FlightId != flightId)
        {
            throw new NotFoundException("Flight policy", policyId);
        }

        _policies.Remove(policy);
        await _unitOfWork.SaveChangesAsync(cancellationToken);
        await _eventPublisher.PublishAsync(
            new FlightPolicyDeleted(policy.Id, policy.FlightId, UtcNow()),
            cancellationToken);
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
