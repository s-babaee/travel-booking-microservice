using BuildingBlocks.Contracts.Events;
using Hotel.Api.Application.Abstractions;
using Hotel.Api.Application.Contracts;
using Hotel.Api.Application.Exceptions;
using Hotel.Api.Application.Mapping;
using Hotel.Api.Domain.Entities;
using HotelEntity = Hotel.Api.Domain.Entities.Hotel;

namespace Hotel.Api.Application.Services;

public sealed class HotelService : IHotelService
{
    private readonly IHotelRepository _hotels;
    private readonly IRoomTypeRepository _roomTypes;
    private readonly IRoomTypeAmenityRepository _roomTypeAmenities;
    private readonly IRoomTypeImageRepository _roomTypeImages;
    private readonly IHotelAmenityRepository _hotelAmenities;
    private readonly IHotelPolicyRepository _policies;
    private readonly IHotelImageRepository _images;
    private readonly IUnitOfWork _unitOfWork;
    private readonly IIntegrationEventPublisher _eventPublisher;
    private readonly TimeProvider _timeProvider;

    public HotelService(
        IHotelRepository hotels,
        IRoomTypeRepository roomTypes,
        IRoomTypeAmenityRepository roomTypeAmenities,
        IRoomTypeImageRepository roomTypeImages,
        IHotelAmenityRepository hotelAmenities,
        IHotelPolicyRepository policies,
        IHotelImageRepository images,
        IUnitOfWork unitOfWork,
        IIntegrationEventPublisher eventPublisher,
        TimeProvider timeProvider)
    {
        _hotels = hotels;
        _roomTypes = roomTypes;
        _roomTypeAmenities = roomTypeAmenities;
        _roomTypeImages = roomTypeImages;
        _hotelAmenities = hotelAmenities;
        _policies = policies;
        _images = images;
        _unitOfWork = unitOfWork;
        _eventPublisher = eventPublisher;
        _timeProvider = timeProvider;
    }

    public async Task<HotelResponse> CreateAsync(
        CreateHotelRequest request,
        CancellationToken cancellationToken)
    {
        ArgumentNullException.ThrowIfNull(request);

        if (await _hotels.ExistsByNameAsync(
                request.Name,
                excludingId: null,
                cancellationToken))
        {
            throw new ConflictException(
                $"A non-deleted hotel named '{request.Name.Trim()}' already exists.");
        }

        var now = UtcNow();
        var hotel = HotelEntity.Create(
            Guid.NewGuid(),
            request.Name,
            request.Description,
            request.StarRating,
            request.AddressLine1,
            request.AddressLine2,
            request.City,
            request.StateOrProvince,
            request.Country,
            request.PostalCode,
            request.PhoneNumber,
            request.Email,
            request.WebsiteUrl,
            request.Latitude,
            request.Longitude,
            now);

        await _hotels.AddAsync(hotel, cancellationToken);
        await _unitOfWork.SaveChangesAsync(cancellationToken);

        await _eventPublisher.PublishAsync(
            new HotelCreated(
                hotel.Id,
                hotel.Name,
                hotel.City,
                hotel.Country,
                hotel.StarRating,
                now),
            cancellationToken);

        return await BuildResponseAsync(hotel, cancellationToken);
    }

    public async Task<HotelResponse> GetAsync(
        Guid hotelId,
        CancellationToken cancellationToken)
    {
        var hotel = await GetHotelOrThrowAsync(hotelId, cancellationToken);
        return await BuildResponseAsync(hotel, cancellationToken);
    }

    public async Task<HotelResponse> UpdateAsync(
        Guid hotelId,
        UpdateHotelRequest request,
        CancellationToken cancellationToken)
    {
        ArgumentNullException.ThrowIfNull(request);

        var hotel = await GetHotelOrThrowAsync(hotelId, cancellationToken);
        if (await _hotels.ExistsByNameAsync(
                request.Name,
                hotelId,
                cancellationToken))
        {
            throw new ConflictException(
                $"A non-deleted hotel named '{request.Name.Trim()}' already exists.");
        }

        var now = UtcNow();
        hotel.UpdateDetails(
            request.Name,
            request.Description,
            request.StarRating,
            request.AddressLine1,
            request.AddressLine2,
            request.City,
            request.StateOrProvince,
            request.Country,
            request.PostalCode,
            request.PhoneNumber,
            request.Email,
            request.WebsiteUrl,
            request.Latitude,
            request.Longitude,
            now);

        await _unitOfWork.SaveChangesAsync(cancellationToken);

        await _eventPublisher.PublishAsync(
            new HotelUpdated(
                hotel.Id,
                hotel.Name,
                hotel.City,
                hotel.Country,
                hotel.StarRating,
                now),
            cancellationToken);

        return await BuildResponseAsync(hotel, cancellationToken);
    }

    public async Task<HotelResponse> ChangeStatusAsync(
        Guid hotelId,
        ChangeHotelStatusRequest request,
        CancellationToken cancellationToken)
    {
        ArgumentNullException.ThrowIfNull(request);

        var hotel = await GetHotelOrThrowAsync(hotelId, cancellationToken);
        var now = UtcNow();
        hotel.ChangeStatus(request.Status, now);

        await _unitOfWork.SaveChangesAsync(cancellationToken);

        await _eventPublisher.PublishAsync(
            new HotelStatusChanged(
                hotel.Id,
                hotel.Status.ToString(),
                now),
            cancellationToken);

        return await BuildResponseAsync(hotel, cancellationToken);
    }

    public async Task DeleteAsync(
        Guid hotelId,
        CancellationToken cancellationToken)
    {
        var hotel = await GetHotelOrThrowAsync(hotelId, cancellationToken);
        var now = UtcNow();
        hotel.SoftDelete(now);

        await _unitOfWork.SaveChangesAsync(cancellationToken);

        await _eventPublisher.PublishAsync(
            new HotelDeleted(hotel.Id, now),
            cancellationToken);
    }

    private async Task<HotelEntity> GetHotelOrThrowAsync(
        Guid hotelId,
        CancellationToken cancellationToken)
    {
        var hotel = await _hotels.GetByIdAsync(hotelId, cancellationToken);
        return hotel ?? throw new NotFoundException("Hotel", hotelId);
    }

    private async Task<HotelResponse> BuildResponseAsync(
        HotelEntity hotel,
        CancellationToken cancellationToken)
    {
        var roomTypes = await _roomTypes.ListByHotelAsync(
            hotel.Id,
            cancellationToken);
        var roomTypeResponses = new List<RoomTypeResponse>(roomTypes.Count);

        foreach (var roomType in roomTypes)
        {
            var amenities = await _roomTypeAmenities.ListAmenitiesAsync(
                roomType.Id,
                cancellationToken);
            var images = await _roomTypeImages.ListByRoomTypeAsync(
                roomType.Id,
                cancellationToken);

            roomTypeResponses.Add(
                roomType.ToResponse(amenities, images));
        }

        var hotelAmenities = await _hotelAmenities.ListAmenitiesAsync(
            hotel.Id,
            cancellationToken);
        var policies = await _policies.ListByHotelAsync(
            hotel.Id,
            cancellationToken);
        var imagesForHotel = await _images.ListByHotelAsync(
            hotel.Id,
            cancellationToken);

        return hotel.ToResponse(
            roomTypeResponses,
            hotelAmenities,
            policies,
            imagesForHotel);
    }

    private DateTime UtcNow()
    {
        return _timeProvider.GetUtcNow().UtcDateTime;
    }
}
