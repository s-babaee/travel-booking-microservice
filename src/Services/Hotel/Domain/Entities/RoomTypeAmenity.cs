namespace Hotel.Api.Domain.Entities;

public sealed class RoomTypeAmenity
{
    private RoomTypeAmenity()
    {
    }

    private RoomTypeAmenity(Guid roomTypeId, Guid amenityId, DateTime assignedAtUtc)
    {
        RoomTypeId = roomTypeId;
        AmenityId = amenityId;
        AssignedAtUtc = assignedAtUtc;
    }

    public Guid RoomTypeId { get; private set; }
    public Guid AmenityId { get; private set; }
    public DateTime AssignedAtUtc { get; private set; }

    public static RoomTypeAmenity Create(
        Guid roomTypeId,
        Guid amenityId,
        DateTime assignedAtUtc)
    {
        if (roomTypeId == Guid.Empty || amenityId == Guid.Empty)
        {
            throw new ArgumentException("Room type and amenity ids are required.");
        }

        return new RoomTypeAmenity(roomTypeId, amenityId, assignedAtUtc);
    }
}
