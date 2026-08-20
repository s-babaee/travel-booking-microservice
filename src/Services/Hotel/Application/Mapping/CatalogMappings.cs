using Hotel.Api.Application.Contracts;
using Hotel.Api.Domain.Entities;
using HotelEntity = Hotel.Api.Domain.Entities.Hotel;

namespace Hotel.Api.Application.Mapping;

public static class CatalogMappings
{
    public static AmenityResponse ToResponse(this Amenity amenity)
    {
        return new AmenityResponse(
            amenity.Id,
            amenity.Name,
            amenity.Type,
            amenity.Description,
            amenity.CreatedAtUtc,
            amenity.UpdatedAtUtc);
    }

    public static HotelPolicyResponse ToResponse(this HotelPolicy policy)
    {
        return new HotelPolicyResponse(
            policy.Id,
            policy.HotelId,
            policy.PolicyType,
            policy.Title,
            policy.Content,
            policy.Conditions,
            policy.CreatedAtUtc,
            policy.UpdatedAtUtc);
    }

    public static HotelImageResponse ToResponse(this HotelImage image)
    {
        return new HotelImageResponse(
            image.Id,
            image.HotelId,
            image.Url,
            image.AltText,
            image.DisplayOrder,
            image.IsPrimary,
            image.CreatedAtUtc);
    }

    public static RoomTypeImageResponse ToResponse(this RoomTypeImage image)
    {
        return new RoomTypeImageResponse(
            image.Id,
            image.RoomTypeId,
            image.Url,
            image.AltText,
            image.DisplayOrder,
            image.IsPrimary,
            image.CreatedAtUtc);
    }

    public static RoomTypeResponse ToResponse(
        this RoomType roomType,
        IReadOnlyList<Amenity> amenities,
        IReadOnlyList<RoomTypeImage> images)
    {
        return new RoomTypeResponse(
            roomType.Id,
            roomType.HotelId,
            roomType.Name,
            roomType.Description,
            roomType.MaxOccupancy,
            roomType.BedType,
            roomType.SizeInSquareMeters,
            roomType.View,
            roomType.Status,
            roomType.CreatedAtUtc,
            roomType.UpdatedAtUtc,
            amenities.Select(amenity => amenity.ToResponse()).ToArray(),
            images.Select(image => image.ToResponse()).ToArray());
    }

    public static HotelResponse ToResponse(
        this HotelEntity hotel,
        IReadOnlyList<RoomTypeResponse> roomTypes,
        IReadOnlyList<Amenity> amenities,
        IReadOnlyList<HotelPolicy> policies,
        IReadOnlyList<HotelImage> images)
    {
        return new HotelResponse(
            hotel.Id,
            hotel.Name,
            hotel.Description,
            hotel.StarRating,
            hotel.AddressLine1,
            hotel.AddressLine2,
            hotel.City,
            hotel.StateOrProvince,
            hotel.Country,
            hotel.PostalCode,
            hotel.PhoneNumber,
            hotel.Email,
            hotel.WebsiteUrl,
            hotel.Latitude,
            hotel.Longitude,
            hotel.Status,
            hotel.CreatedAtUtc,
            hotel.UpdatedAtUtc,
            roomTypes,
            amenities.Select(amenity => amenity.ToResponse()).ToArray(),
            policies.Select(policy => policy.ToResponse()).ToArray(),
            images.Select(image => image.ToResponse()).ToArray());
    }
}
