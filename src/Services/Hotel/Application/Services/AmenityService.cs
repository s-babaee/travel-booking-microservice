using BuildingBlocks.Contracts.Events;
using Hotel.Api.Application.Abstractions;
using Hotel.Api.Application.Contracts;
using Hotel.Api.Application.Exceptions;
using Hotel.Api.Application.Mapping;
using Hotel.Api.Domain.Entities;

namespace Hotel.Api.Application.Services;

public sealed class AmenityService : IAmenityService
{
    private readonly IHotelRepository _hotels;
    private readonly IRoomTypeRepository _roomTypes;
    private readonly IAmenityRepository _amenities;
    private readonly IHotelAmenityRepository _hotelAmenities;
    private readonly IRoomTypeAmenityRepository _roomTypeAmenities;
    private readonly IUnitOfWork _unitOfWork;
    private readonly IIntegrationEventPublisher _eventPublisher;
    private readonly TimeProvider _timeProvider;

    public AmenityService(
        IHotelRepository hotels,
        IRoomTypeRepository roomTypes,
        IAmenityRepository amenities,
        IHotelAmenityRepository hotelAmenities,
        IRoomTypeAmenityRepository roomTypeAmenities,
        IUnitOfWork unitOfWork,
        IIntegrationEventPublisher eventPublisher,
        TimeProvider timeProvider)
    {
        _hotels = hotels;
        _roomTypes = roomTypes;
        _amenities = amenities;
        _hotelAmenities = hotelAmenities;
        _roomTypeAmenities = roomTypeAmenities;
        _unitOfWork = unitOfWork;
        _eventPublisher = eventPublisher;
        _timeProvider = timeProvider;
    }

    public async Task<AmenityResponse> CreateAsync(
        CreateAmenityRequest request,
        CancellationToken cancellationToken)
    {
        ArgumentNullException.ThrowIfNull(request);

        if (await _amenities.ExistsByNameAsync(
                request.Name,
                excludingId: null,
                cancellationToken))
        {
            throw new ConflictException(
                $"A non-deleted amenity named '{request.Name.Trim()}' already exists.");
        }

        var now = UtcNow();
        var amenity = Amenity.Create(
            Guid.NewGuid(),
            request.Name,
            request.Type,
            request.Description,
            now);

        await _amenities.AddAsync(amenity, cancellationToken);
        await _unitOfWork.SaveChangesAsync(cancellationToken);

        await _eventPublisher.PublishAsync(
            new AmenityCreated(
                amenity.Id,
                amenity.Name,
                amenity.Type.ToString(),
                now),
            cancellationToken);

        return amenity.ToResponse();
    }

    public async Task<IReadOnlyList<AmenityResponse>> ListAsync(
        CancellationToken cancellationToken)
    {
        var amenities = await _amenities.ListAsync(cancellationToken);
        return amenities.Select(amenity => amenity.ToResponse()).ToArray();
    }

    public async Task<AmenityResponse> UpdateAsync(
        Guid amenityId,
        UpdateAmenityRequest request,
        CancellationToken cancellationToken)
    {
        ArgumentNullException.ThrowIfNull(request);

        var amenity = await GetAmenityOrThrowAsync(
            amenityId,
            cancellationToken);

        if (await _amenities.ExistsByNameAsync(
                request.Name,
                amenityId,
                cancellationToken))
        {
            throw new ConflictException(
                $"A non-deleted amenity named '{request.Name.Trim()}' already exists.");
        }

        var now = UtcNow();
        amenity.Update(
            request.Name,
            request.Type,
            request.Description,
            now);

        await _unitOfWork.SaveChangesAsync(cancellationToken);

        await _eventPublisher.PublishAsync(
            new AmenityUpdated(
                amenity.Id,
                amenity.Name,
                amenity.Type.ToString(),
                now),
            cancellationToken);

        return amenity.ToResponse();
    }

    public async Task DeleteAsync(
        Guid amenityId,
        CancellationToken cancellationToken)
    {
        var amenity = await GetAmenityOrThrowAsync(
            amenityId,
            cancellationToken);
        var now = UtcNow();
        amenity.SoftDelete(now);

        await _unitOfWork.SaveChangesAsync(cancellationToken);

        await _eventPublisher.PublishAsync(
            new AmenityDeleted(amenity.Id, now),
            cancellationToken);
    }

    public async Task AssignToHotelAsync(
        Guid hotelId,
        Guid amenityId,
        CancellationToken cancellationToken)
    {
        await GetHotelOrThrowAsync(hotelId, cancellationToken);
        await GetAmenityOrThrowAsync(amenityId, cancellationToken);

        if (await _hotelAmenities.ExistsAsync(
                hotelId,
                amenityId,
                cancellationToken))
        {
            throw new ConflictException(
                "The amenity is already assigned to this hotel.");
        }

        var now = UtcNow();
        await _hotelAmenities.AddAsync(
            HotelAmenity.Create(hotelId, amenityId, now),
            cancellationToken);
        await _unitOfWork.SaveChangesAsync(cancellationToken);

        await _eventPublisher.PublishAsync(
            new HotelAmenityAssigned(hotelId, amenityId, now),
            cancellationToken);
    }

    public async Task RemoveFromHotelAsync(
        Guid hotelId,
        Guid amenityId,
        CancellationToken cancellationToken)
    {
        await GetHotelOrThrowAsync(hotelId, cancellationToken);
        await GetAmenityOrThrowAsync(amenityId, cancellationToken);
        await EnsureHotelAssignmentExistsAsync(
            hotelId,
            amenityId,
            cancellationToken);

        await _hotelAmenities.RemoveAsync(
            hotelId,
            amenityId,
            cancellationToken);
        await _unitOfWork.SaveChangesAsync(cancellationToken);

        await _eventPublisher.PublishAsync(
            new HotelAmenityRemoved(
                hotelId,
                amenityId,
                UtcNow()),
            cancellationToken);
    }

    public async Task AssignToRoomTypeAsync(
        Guid roomTypeId,
        Guid amenityId,
        CancellationToken cancellationToken)
    {
        await GetRoomTypeOrThrowAsync(roomTypeId, cancellationToken);
        await GetAmenityOrThrowAsync(amenityId, cancellationToken);

        if (await _roomTypeAmenities.ExistsAsync(
                roomTypeId,
                amenityId,
                cancellationToken))
        {
            throw new ConflictException(
                "The amenity is already assigned to this room type.");
        }

        var now = UtcNow();
        await _roomTypeAmenities.AddAsync(
            RoomTypeAmenity.Create(roomTypeId, amenityId, now),
            cancellationToken);
        await _unitOfWork.SaveChangesAsync(cancellationToken);

        await _eventPublisher.PublishAsync(
            new RoomTypeAmenityAssigned(roomTypeId, amenityId, now),
            cancellationToken);
    }

    public async Task RemoveFromRoomTypeAsync(
        Guid roomTypeId,
        Guid amenityId,
        CancellationToken cancellationToken)
    {
        await GetRoomTypeOrThrowAsync(roomTypeId, cancellationToken);
        await GetAmenityOrThrowAsync(amenityId, cancellationToken);

        if (!await _roomTypeAmenities.ExistsAsync(
                roomTypeId,
                amenityId,
                cancellationToken))
        {
            throw new NotFoundException(
                "Room type amenity assignment",
                amenityId);
        }

        await _roomTypeAmenities.RemoveAsync(
            roomTypeId,
            amenityId,
            cancellationToken);
        await _unitOfWork.SaveChangesAsync(cancellationToken);

        await _eventPublisher.PublishAsync(
            new RoomTypeAmenityRemoved(
                roomTypeId,
                amenityId,
                UtcNow()),
            cancellationToken);
    }

    private async Task<Amenity> GetAmenityOrThrowAsync(
        Guid amenityId,
        CancellationToken cancellationToken)
    {
        var amenity = await _amenities.GetByIdAsync(
            amenityId,
            cancellationToken);
        return amenity ?? throw new NotFoundException("Amenity", amenityId);
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

    private async Task GetRoomTypeOrThrowAsync(
        Guid roomTypeId,
        CancellationToken cancellationToken)
    {
        var roomType = await _roomTypes.GetByIdAsync(
            roomTypeId,
            cancellationToken);
        if (roomType is null)
        {
            throw new NotFoundException("Room type", roomTypeId);
        }
    }

    private async Task EnsureHotelAssignmentExistsAsync(
        Guid hotelId,
        Guid amenityId,
        CancellationToken cancellationToken)
    {
        if (!await _hotelAmenities.ExistsAsync(
                hotelId,
                amenityId,
                cancellationToken))
        {
            throw new NotFoundException(
                "Hotel amenity assignment",
                amenityId);
        }
    }

    private DateTime UtcNow()
    {
        return _timeProvider.GetUtcNow().UtcDateTime;
    }
}
