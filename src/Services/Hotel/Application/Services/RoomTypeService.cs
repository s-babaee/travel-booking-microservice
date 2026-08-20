using BuildingBlocks.Events;
using Hotel.Api.Application.Abstractions;
using Hotel.Api.Application.Contracts;
using Hotel.Api.Application.Exceptions;
using Hotel.Api.Application.Mapping;
using Hotel.Api.Domain.Entities;

namespace Hotel.Api.Application.Services;

public sealed class RoomTypeService : IRoomTypeService
{
    private readonly IHotelRepository _hotels;
    private readonly IRoomTypeRepository _roomTypes;
    private readonly IRoomTypeAmenityRepository _roomTypeAmenities;
    private readonly IRoomTypeImageRepository _images;
    private readonly IUnitOfWork _unitOfWork;
    private readonly IIntegrationEventPublisher _eventPublisher;
    private readonly TimeProvider _timeProvider;

    public RoomTypeService(
        IHotelRepository hotels,
        IRoomTypeRepository roomTypes,
        IRoomTypeAmenityRepository roomTypeAmenities,
        IRoomTypeImageRepository images,
        IUnitOfWork unitOfWork,
        IIntegrationEventPublisher eventPublisher,
        TimeProvider timeProvider)
    {
        _hotels = hotels;
        _roomTypes = roomTypes;
        _roomTypeAmenities = roomTypeAmenities;
        _images = images;
        _unitOfWork = unitOfWork;
        _eventPublisher = eventPublisher;
        _timeProvider = timeProvider;
    }

    public async Task<RoomTypeResponse> CreateAsync(
        Guid hotelId,
        CreateRoomTypeRequest request,
        CancellationToken cancellationToken)
    {
        ArgumentNullException.ThrowIfNull(request);
        await GetHotelOrThrowAsync(hotelId, cancellationToken);

        if (await _roomTypes.ExistsByNameAsync(
                hotelId,
                request.Name,
                excludingId: null,
                cancellationToken))
        {
            throw new ConflictException(
                $"A non-deleted room type named '{request.Name.Trim()}' already exists for this hotel.");
        }

        var now = UtcNow();
        var roomType = RoomType.Create(
            Guid.NewGuid(),
            hotelId,
            request.Name,
            request.Description,
            request.MaxOccupancy,
            request.BedType,
            request.SizeInSquareMeters,
            request.View,
            now);

        await _roomTypes.AddAsync(roomType, cancellationToken);
        await _unitOfWork.SaveChangesAsync(cancellationToken);

        await _eventPublisher.PublishAsync(
            new RoomTypeCreated(
                roomType.Id,
                roomType.HotelId,
                roomType.Name,
                roomType.MaxOccupancy,
                now),
            cancellationToken);

        return await BuildResponseAsync(roomType, cancellationToken);
    }

    public async Task<IReadOnlyList<RoomTypeResponse>> ListByHotelAsync(
        Guid hotelId,
        CancellationToken cancellationToken)
    {
        await GetHotelOrThrowAsync(hotelId, cancellationToken);
        var roomTypes = await _roomTypes.ListByHotelAsync(
            hotelId,
            cancellationToken);

        var responses = new List<RoomTypeResponse>(roomTypes.Count);
        foreach (var roomType in roomTypes)
        {
            responses.Add(await BuildResponseAsync(roomType, cancellationToken));
        }

        return responses;
    }

    public async Task<RoomTypeResponse> GetAsync(
        Guid roomTypeId,
        CancellationToken cancellationToken)
    {
        var roomType = await GetRoomTypeOrThrowAsync(
            roomTypeId,
            cancellationToken);
        return await BuildResponseAsync(roomType, cancellationToken);
    }

    public async Task<RoomTypeResponse> UpdateAsync(
        Guid roomTypeId,
        UpdateRoomTypeRequest request,
        CancellationToken cancellationToken)
    {
        ArgumentNullException.ThrowIfNull(request);

        var roomType = await GetRoomTypeOrThrowAsync(
            roomTypeId,
            cancellationToken);

        if (await _roomTypes.ExistsByNameAsync(
                roomType.HotelId,
                request.Name,
                roomTypeId,
                cancellationToken))
        {
            throw new ConflictException(
                $"A non-deleted room type named '{request.Name.Trim()}' already exists for this hotel.");
        }

        var now = UtcNow();
        roomType.UpdateDetails(
            request.Name,
            request.Description,
            request.MaxOccupancy,
            request.BedType,
            request.SizeInSquareMeters,
            request.View,
            now);

        await _unitOfWork.SaveChangesAsync(cancellationToken);

        await _eventPublisher.PublishAsync(
            new RoomTypeUpdated(
                roomType.Id,
                roomType.HotelId,
                roomType.Name,
                roomType.MaxOccupancy,
                now),
            cancellationToken);

        return await BuildResponseAsync(roomType, cancellationToken);
    }

    public async Task<RoomTypeResponse> ChangeStatusAsync(
        Guid roomTypeId,
        ChangeRoomTypeStatusRequest request,
        CancellationToken cancellationToken)
    {
        ArgumentNullException.ThrowIfNull(request);

        var roomType = await GetRoomTypeOrThrowAsync(
            roomTypeId,
            cancellationToken);
        var now = UtcNow();
        roomType.ChangeStatus(request.Status, now);

        await _unitOfWork.SaveChangesAsync(cancellationToken);

        await _eventPublisher.PublishAsync(
            new RoomTypeStatusChanged(
                roomType.Id,
                roomType.HotelId,
                roomType.Status.ToString(),
                now),
            cancellationToken);

        return await BuildResponseAsync(roomType, cancellationToken);
    }

    public async Task DeleteAsync(
        Guid roomTypeId,
        CancellationToken cancellationToken)
    {
        var roomType = await GetRoomTypeOrThrowAsync(
            roomTypeId,
            cancellationToken);
        var now = UtcNow();
        roomType.SoftDelete(now);

        await _unitOfWork.SaveChangesAsync(cancellationToken);

        await _eventPublisher.PublishAsync(
            new RoomTypeDeleted(
                roomType.Id,
                roomType.HotelId,
                now),
            cancellationToken);
    }

    private async Task<Hotel.Api.Domain.Entities.Hotel> GetHotelOrThrowAsync(
        Guid hotelId,
        CancellationToken cancellationToken)
    {
        var hotel = await _hotels.GetByIdAsync(hotelId, cancellationToken);
        return hotel ?? throw new NotFoundException("Hotel", hotelId);
    }

    private async Task<RoomType> GetRoomTypeOrThrowAsync(
        Guid roomTypeId,
        CancellationToken cancellationToken)
    {
        var roomType = await _roomTypes.GetByIdAsync(
            roomTypeId,
            cancellationToken);
        return roomType ?? throw new NotFoundException("Room type", roomTypeId);
    }

    private async Task<RoomTypeResponse> BuildResponseAsync(
        RoomType roomType,
        CancellationToken cancellationToken)
    {
        var amenities = await _roomTypeAmenities.ListAmenitiesAsync(
            roomType.Id,
            cancellationToken);
        var images = await _images.ListByRoomTypeAsync(
            roomType.Id,
            cancellationToken);

        return roomType.ToResponse(amenities, images);
    }

    private DateTime UtcNow()
    {
        return _timeProvider.GetUtcNow().UtcDateTime;
    }
}
