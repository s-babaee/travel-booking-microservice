using BuildingBlocks.Events;
using Hotel.Api.Application.Abstractions;
using Hotel.Api.Application.Contracts;
using Hotel.Api.Application.Exceptions;
using Hotel.Api.Application.Mapping;
using Hotel.Api.Domain.Entities;

namespace Hotel.Api.Application.Services;

public sealed class HotelPolicyService : IHotelPolicyService
{
    private readonly IHotelRepository _hotels;
    private readonly IHotelPolicyRepository _policies;
    private readonly IUnitOfWork _unitOfWork;
    private readonly IIntegrationEventPublisher _eventPublisher;
    private readonly TimeProvider _timeProvider;

    public HotelPolicyService(
        IHotelRepository hotels,
        IHotelPolicyRepository policies,
        IUnitOfWork unitOfWork,
        IIntegrationEventPublisher eventPublisher,
        TimeProvider timeProvider)
    {
        _hotels = hotels;
        _policies = policies;
        _unitOfWork = unitOfWork;
        _eventPublisher = eventPublisher;
        _timeProvider = timeProvider;
    }

    public async Task<HotelPolicyResponse> CreateAsync(
        Guid hotelId,
        CreateHotelPolicyRequest request,
        CancellationToken cancellationToken)
    {
        ArgumentNullException.ThrowIfNull(request);
        await GetHotelOrThrowAsync(hotelId, cancellationToken);

        var now = UtcNow();
        var policy = HotelPolicy.Create(
            Guid.NewGuid(),
            hotelId,
            request.PolicyType,
            request.Title,
            request.Content,
            request.Conditions,
            now);

        await _policies.AddAsync(policy, cancellationToken);
        await _unitOfWork.SaveChangesAsync(cancellationToken);

        await _eventPublisher.PublishAsync(
            new HotelPolicyCreated(
                policy.Id,
                policy.HotelId,
                policy.PolicyType,
                now),
            cancellationToken);

        return policy.ToResponse();
    }

    public async Task<IReadOnlyList<HotelPolicyResponse>> ListByHotelAsync(
        Guid hotelId,
        CancellationToken cancellationToken)
    {
        await GetHotelOrThrowAsync(hotelId, cancellationToken);
        var policies = await _policies.ListByHotelAsync(
            hotelId,
            cancellationToken);
        return policies.Select(policy => policy.ToResponse()).ToArray();
    }

    public async Task<HotelPolicyResponse> UpdateAsync(
        Guid hotelId,
        Guid policyId,
        UpdateHotelPolicyRequest request,
        CancellationToken cancellationToken)
    {
        ArgumentNullException.ThrowIfNull(request);
        await GetHotelOrThrowAsync(hotelId, cancellationToken);

        var policy = await _policies.GetByIdAsync(
            hotelId,
            policyId,
            cancellationToken);
        if (policy is null)
        {
            throw new NotFoundException("Hotel policy", policyId);
        }

        var now = UtcNow();
        policy.Update(
            request.PolicyType,
            request.Title,
            request.Content,
            request.Conditions,
            now);
        await _unitOfWork.SaveChangesAsync(cancellationToken);

        await _eventPublisher.PublishAsync(
            new HotelPolicyUpdated(
                policy.Id,
                policy.HotelId,
                policy.PolicyType,
                now),
            cancellationToken);

        return policy.ToResponse();
    }

    public async Task DeleteAsync(
        Guid hotelId,
        Guid policyId,
        CancellationToken cancellationToken)
    {
        await GetHotelOrThrowAsync(hotelId, cancellationToken);

        var policy = await _policies.GetByIdAsync(
            hotelId,
            policyId,
            cancellationToken);
        if (policy is null)
        {
            throw new NotFoundException("Hotel policy", policyId);
        }

        _policies.Remove(policy);
        await _unitOfWork.SaveChangesAsync(cancellationToken);

        await _eventPublisher.PublishAsync(
            new HotelPolicyDeleted(
                policy.Id,
                policy.HotelId,
                UtcNow()),
            cancellationToken);
    }

    private async Task GetHotelOrThrowAsync(
        Guid hotelId,
        CancellationToken cancellationToken)
    {
        var hotel = await _hotels.GetByIdAsync(hotelId, cancellationToken);
        if (hotel is null)
        {
            throw new NotFoundException("Hotel", hotelId);
        }
    }

    private DateTime UtcNow()
    {
        return _timeProvider.GetUtcNow().UtcDateTime;
    }
}
