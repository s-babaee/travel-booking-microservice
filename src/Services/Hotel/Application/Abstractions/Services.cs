using Hotel.Api.Application.Contracts;

namespace Hotel.Api.Application.Abstractions;

public interface IHotelService
{
    Task<HotelResponse> CreateAsync(
        CreateHotelRequest request,
        CancellationToken cancellationToken);
    Task<HotelResponse> GetAsync(
        Guid hotelId,
        CancellationToken cancellationToken);

    Task<IReadOnlyList<HotelResponse>> GetAllAsync(CancellationToken cancellationToken);

    Task<HotelResponse> UpdateAsync(
        Guid hotelId,
        UpdateHotelRequest request,
        CancellationToken cancellationToken);
    Task<HotelResponse> ChangeStatusAsync(
        Guid hotelId,
        ChangeHotelStatusRequest request,
        CancellationToken cancellationToken);
    Task DeleteAsync(Guid hotelId, CancellationToken cancellationToken);
}

public interface IRoomTypeService
{
    Task<RoomTypeResponse> CreateAsync(
        Guid hotelId,
        CreateRoomTypeRequest request,
        CancellationToken cancellationToken);
    Task<IReadOnlyList<RoomTypeResponse>> ListByHotelAsync(
        Guid hotelId,
        CancellationToken cancellationToken);
    Task<RoomTypeResponse> GetAsync(
        Guid roomTypeId,
        CancellationToken cancellationToken);
    Task<RoomTypeResponse> UpdateAsync(
        Guid roomTypeId,
        UpdateRoomTypeRequest request,
        CancellationToken cancellationToken);
    Task<RoomTypeResponse> ChangeStatusAsync(
        Guid roomTypeId,
        ChangeRoomTypeStatusRequest request,
        CancellationToken cancellationToken);
    Task DeleteAsync(Guid roomTypeId, CancellationToken cancellationToken);
}

public interface IAmenityService
{
    Task<AmenityResponse> CreateAsync(
        CreateAmenityRequest request,
        CancellationToken cancellationToken);
    Task<IReadOnlyList<AmenityResponse>> ListAsync(
        CancellationToken cancellationToken);
    Task<AmenityResponse> UpdateAsync(
        Guid amenityId,
        UpdateAmenityRequest request,
        CancellationToken cancellationToken);
    Task DeleteAsync(Guid amenityId, CancellationToken cancellationToken);
    Task AssignToHotelAsync(
        Guid hotelId,
        Guid amenityId,
        CancellationToken cancellationToken);
    Task RemoveFromHotelAsync(
        Guid hotelId,
        Guid amenityId,
        CancellationToken cancellationToken);
    Task AssignToRoomTypeAsync(
        Guid roomTypeId,
        Guid amenityId,
        CancellationToken cancellationToken);
    Task RemoveFromRoomTypeAsync(
        Guid roomTypeId,
        Guid amenityId,
        CancellationToken cancellationToken);
}

public interface IHotelPolicyService
{
    Task<HotelPolicyResponse> CreateAsync(
        Guid hotelId,
        CreateHotelPolicyRequest request,
        CancellationToken cancellationToken);
    Task<IReadOnlyList<HotelPolicyResponse>> ListByHotelAsync(
        Guid hotelId,
        CancellationToken cancellationToken);
    Task<HotelPolicyResponse> UpdateAsync(
        Guid hotelId,
        Guid policyId,
        UpdateHotelPolicyRequest request,
        CancellationToken cancellationToken);
    Task DeleteAsync(
        Guid hotelId,
        Guid policyId,
        CancellationToken cancellationToken);
}

public interface IHotelImageService
{
    Task<HotelImageResponse> AddToHotelAsync(
        Guid hotelId,
        AddHotelImageRequest request,
        CancellationToken cancellationToken);
    Task<IReadOnlyList<HotelImageResponse>> ListHotelImagesAsync(
        Guid hotelId,
        CancellationToken cancellationToken);
    Task DeleteFromHotelAsync(
        Guid hotelId,
        Guid imageId,
        CancellationToken cancellationToken);
    Task<RoomTypeImageResponse> AddToRoomTypeAsync(
        Guid roomTypeId,
        AddRoomTypeImageRequest request,
        CancellationToken cancellationToken);
    Task<IReadOnlyList<RoomTypeImageResponse>> ListRoomTypeImagesAsync(
        Guid roomTypeId,
        CancellationToken cancellationToken);
    Task DeleteFromRoomTypeAsync(
        Guid roomTypeId,
        Guid imageId,
        CancellationToken cancellationToken);
}
