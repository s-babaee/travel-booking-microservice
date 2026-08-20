using BuildingBlocks.Events;
using Hotel.Api.Application.Abstractions;
using Hotel.Api.Application.Contracts;
using Hotel.Api.Application.Exceptions;
using Hotel.Api.Application.Mapping;
using Hotel.Api.Domain.Entities;

namespace Hotel.Api.Application.Services;

public sealed class HotelImageService : IHotelImageService
{
    private readonly IHotelRepository _hotels;
    private readonly IRoomTypeRepository _roomTypes;
    private readonly IHotelImageRepository _hotelImages;
    private readonly IRoomTypeImageRepository _roomTypeImages;
    private readonly IUnitOfWork _unitOfWork;
    private readonly IIntegrationEventPublisher _eventPublisher;
    private readonly TimeProvider _timeProvider;

    public HotelImageService(
        IHotelRepository hotels,
        IRoomTypeRepository roomTypes,
        IHotelImageRepository hotelImages,
        IRoomTypeImageRepository roomTypeImages,
        IUnitOfWork unitOfWork,
        IIntegrationEventPublisher eventPublisher,
        TimeProvider timeProvider)
    {
        _hotels = hotels;
        _roomTypes = roomTypes;
        _hotelImages = hotelImages;
        _roomTypeImages = roomTypeImages;
        _unitOfWork = unitOfWork;
        _eventPublisher = eventPublisher;
        _timeProvider = timeProvider;
    }

    public async Task<HotelImageResponse> AddToHotelAsync(
        Guid hotelId,
        AddHotelImageRequest request,
        CancellationToken cancellationToken)
    {
        ArgumentNullException.ThrowIfNull(request);
        await GetHotelOrThrowAsync(hotelId, cancellationToken);

        var now = UtcNow();
        var image = HotelImage.Create(
            Guid.NewGuid(),
            hotelId,
            request.Url,
            request.AltText,
            request.DisplayOrder,
            request.IsPrimary,
            now);

        if (request.IsPrimary)
        {
            await MarkExistingHotelImagesSecondaryAsync(
                hotelId,
                cancellationToken);
        }

        await _hotelImages.AddAsync(image, cancellationToken);
        await _unitOfWork.SaveChangesAsync(cancellationToken);

        await _eventPublisher.PublishAsync(
            new HotelImageAdded(
                image.Id,
                image.HotelId,
                image.Url,
                now),
            cancellationToken);

        return image.ToResponse();
    }

    public async Task<IReadOnlyList<HotelImageResponse>> ListHotelImagesAsync(
        Guid hotelId,
        CancellationToken cancellationToken)
    {
        await GetHotelOrThrowAsync(hotelId, cancellationToken);
        var images = await _hotelImages.ListByHotelAsync(
            hotelId,
            cancellationToken);
        return images.Select(image => image.ToResponse()).ToArray();
    }

    public async Task DeleteFromHotelAsync(
        Guid hotelId,
        Guid imageId,
        CancellationToken cancellationToken)
    {
        await GetHotelOrThrowAsync(hotelId, cancellationToken);

        var image = await _hotelImages.GetByIdAsync(
            hotelId,
            imageId,
            cancellationToken);
        if (image is null)
        {
            throw new NotFoundException("Hotel image", imageId);
        }

        _hotelImages.Remove(image);
        await _unitOfWork.SaveChangesAsync(cancellationToken);

        await _eventPublisher.PublishAsync(
            new HotelImageDeleted(
                image.Id,
                image.HotelId,
                UtcNow()),
            cancellationToken);
    }

    public async Task<RoomTypeImageResponse> AddToRoomTypeAsync(
        Guid roomTypeId,
        AddRoomTypeImageRequest request,
        CancellationToken cancellationToken)
    {
        ArgumentNullException.ThrowIfNull(request);
        await GetRoomTypeOrThrowAsync(roomTypeId, cancellationToken);

        var now = UtcNow();
        var image = RoomTypeImage.Create(
            Guid.NewGuid(),
            roomTypeId,
            request.Url,
            request.AltText,
            request.DisplayOrder,
            request.IsPrimary,
            now);

        if (request.IsPrimary)
        {
            await MarkExistingRoomTypeImagesSecondaryAsync(
                roomTypeId,
                cancellationToken);
        }

        await _roomTypeImages.AddAsync(image, cancellationToken);
        await _unitOfWork.SaveChangesAsync(cancellationToken);

        await _eventPublisher.PublishAsync(
            new RoomTypeImageAdded(
                image.Id,
                image.RoomTypeId,
                image.Url,
                now),
            cancellationToken);

        return image.ToResponse();
    }

    public async Task<IReadOnlyList<RoomTypeImageResponse>> ListRoomTypeImagesAsync(
        Guid roomTypeId,
        CancellationToken cancellationToken)
    {
        await GetRoomTypeOrThrowAsync(roomTypeId, cancellationToken);
        var images = await _roomTypeImages.ListByRoomTypeAsync(
            roomTypeId,
            cancellationToken);
        return images.Select(image => image.ToResponse()).ToArray();
    }

    public async Task DeleteFromRoomTypeAsync(
        Guid roomTypeId,
        Guid imageId,
        CancellationToken cancellationToken)
    {
        await GetRoomTypeOrThrowAsync(roomTypeId, cancellationToken);

        var image = await _roomTypeImages.GetByIdAsync(
            roomTypeId,
            imageId,
            cancellationToken);
        if (image is null)
        {
            throw new NotFoundException("Room type image", imageId);
        }

        _roomTypeImages.Remove(image);
        await _unitOfWork.SaveChangesAsync(cancellationToken);

        await _eventPublisher.PublishAsync(
            new RoomTypeImageDeleted(
                image.Id,
                image.RoomTypeId,
                UtcNow()),
            cancellationToken);
    }

    private async Task MarkExistingHotelImagesSecondaryAsync(
        Guid hotelId,
        CancellationToken cancellationToken)
    {
        var currentPrimaryCandidates = await _hotelImages
            .ListPrimaryCandidatesAsync(hotelId, cancellationToken);

        foreach (var image in currentPrimaryCandidates)
        {
            image.MarkSecondary();
        }
    }

    private async Task MarkExistingRoomTypeImagesSecondaryAsync(
        Guid roomTypeId,
        CancellationToken cancellationToken)
    {
        var currentPrimaryCandidates = await _roomTypeImages
            .ListPrimaryCandidatesAsync(roomTypeId, cancellationToken);

        foreach (var image in currentPrimaryCandidates)
        {
            image.MarkSecondary();
        }
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

    private DateTime UtcNow()
    {
        return _timeProvider.GetUtcNow().UtcDateTime;
    }
}
