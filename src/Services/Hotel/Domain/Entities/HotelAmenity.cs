namespace Hotel.Api.Domain.Entities;

public sealed class HotelAmenity
{
    private HotelAmenity()
    {
    }

    private HotelAmenity(Guid hotelId, Guid amenityId, DateTime assignedAtUtc)
    {
        HotelId = hotelId;
        AmenityId = amenityId;
        AssignedAtUtc = assignedAtUtc;
    }

    public Guid HotelId { get; private set; }
    public Guid AmenityId { get; private set; }
    public DateTime AssignedAtUtc { get; private set; }

    public static HotelAmenity Create(
        Guid hotelId,
        Guid amenityId,
        DateTime assignedAtUtc)
    {
        if (hotelId == Guid.Empty || amenityId == Guid.Empty)
        {
            throw new ArgumentException("Hotel and amenity ids are required.");
        }

        return new HotelAmenity(hotelId, amenityId, assignedAtUtc);
    }
}
