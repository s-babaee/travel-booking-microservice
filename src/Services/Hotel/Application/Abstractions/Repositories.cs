using Hotel.Api.Domain.Entities;
using HotelEntity = Hotel.Api.Domain.Entities.Hotel;

namespace Hotel.Api.Application.Abstractions;

public interface IUnitOfWork
{
    Task<int> SaveChangesAsync(CancellationToken cancellationToken);
}

public interface IIntegrationEventPublisher
{
    Task PublishAsync<TEvent>(
        TEvent @event,
        CancellationToken cancellationToken)
        where TEvent : class;
}

public interface IHotelRepository
{
    Task<HotelEntity?> GetByIdAsync(Guid id, CancellationToken cancellationToken);
    Task<bool> ExistsByNameAsync(
        string name,
        Guid? excludingId,
        CancellationToken cancellationToken);
    Task AddAsync(HotelEntity hotel, CancellationToken cancellationToken);
}

public interface IRoomTypeRepository
{
    Task<RoomType?> GetByIdAsync(Guid id, CancellationToken cancellationToken);
    Task<IReadOnlyList<RoomType>> ListByHotelAsync(
        Guid hotelId,
        CancellationToken cancellationToken);
    Task<bool> ExistsByNameAsync(
        Guid hotelId,
        string name,
        Guid? excludingId,
        CancellationToken cancellationToken);
    Task AddAsync(RoomType roomType, CancellationToken cancellationToken);
}

public interface IAmenityRepository
{
    Task<Amenity?> GetByIdAsync(Guid id, CancellationToken cancellationToken);
    Task<IReadOnlyList<Amenity>> ListAsync(CancellationToken cancellationToken);
    Task<bool> ExistsByNameAsync(
        string name,
        Guid? excludingId,
        CancellationToken cancellationToken);
    Task AddAsync(Amenity amenity, CancellationToken cancellationToken);
}

public interface IHotelAmenityRepository
{
    Task<bool> ExistsAsync(
        Guid hotelId,
        Guid amenityId,
        CancellationToken cancellationToken);
    Task<IReadOnlyList<Amenity>> ListAmenitiesAsync(
        Guid hotelId,
        CancellationToken cancellationToken);
    Task AddAsync(HotelAmenity hotelAmenity, CancellationToken cancellationToken);
    Task RemoveAsync(
        Guid hotelId,
        Guid amenityId,
        CancellationToken cancellationToken);
}

public interface IRoomTypeAmenityRepository
{
    Task<bool> ExistsAsync(
        Guid roomTypeId,
        Guid amenityId,
        CancellationToken cancellationToken);
    Task<IReadOnlyList<Amenity>> ListAmenitiesAsync(
        Guid roomTypeId,
        CancellationToken cancellationToken);
    Task AddAsync(
        RoomTypeAmenity roomTypeAmenity,
        CancellationToken cancellationToken);
    Task RemoveAsync(
        Guid roomTypeId,
        Guid amenityId,
        CancellationToken cancellationToken);
}

public interface IHotelPolicyRepository
{
    Task<HotelPolicy?> GetByIdAsync(
        Guid hotelId,
        Guid policyId,
        CancellationToken cancellationToken);
    Task<IReadOnlyList<HotelPolicy>> ListByHotelAsync(
        Guid hotelId,
        CancellationToken cancellationToken);
    Task AddAsync(HotelPolicy policy, CancellationToken cancellationToken);
    void Remove(HotelPolicy policy);
}

public interface IHotelImageRepository
{
    Task<HotelImage?> GetByIdAsync(
        Guid hotelId,
        Guid imageId,
        CancellationToken cancellationToken);
    Task<IReadOnlyList<HotelImage>> ListByHotelAsync(
        Guid hotelId,
        CancellationToken cancellationToken);
    Task<IReadOnlyList<HotelImage>> ListPrimaryCandidatesAsync(
        Guid hotelId,
        CancellationToken cancellationToken);
    Task AddAsync(HotelImage image, CancellationToken cancellationToken);
    void Remove(HotelImage image);
}

public interface IRoomTypeImageRepository
{
    Task<RoomTypeImage?> GetByIdAsync(
        Guid roomTypeId,
        Guid imageId,
        CancellationToken cancellationToken);
    Task<IReadOnlyList<RoomTypeImage>> ListByRoomTypeAsync(
        Guid roomTypeId,
        CancellationToken cancellationToken);
    Task<IReadOnlyList<RoomTypeImage>> ListPrimaryCandidatesAsync(
        Guid roomTypeId,
        CancellationToken cancellationToken);
    Task AddAsync(RoomTypeImage image, CancellationToken cancellationToken);
    void Remove(RoomTypeImage image);
}
