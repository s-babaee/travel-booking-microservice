using Hotel.Api.Domain.Common;
using Hotel.Api.Domain.Enums;

namespace Hotel.Api.Domain.Entities;

public sealed class RoomType : Entity<Guid>
{
    private RoomType()
    {
    }

    private RoomType(
        Guid id,
        Guid hotelId,
        string name,
        string? description,
        int maxOccupancy,
        string? bedType,
        decimal? sizeInSquareMeters,
        string? view,
        DateTime createdAtUtc)
    {
        Id = id;
        HotelId = hotelId;
        Name = name;
        Description = description;
        MaxOccupancy = maxOccupancy;
        BedType = bedType;
        SizeInSquareMeters = sizeInSquareMeters;
        View = view;
        Status = HotelStatus.Active;
        CreatedAtUtc = createdAtUtc;
        UpdatedAtUtc = createdAtUtc;
    }

    public Guid HotelId { get; private set; }
    public string Name { get; private set; } = null!;
    public string? Description { get; private set; }
    public int MaxOccupancy { get; private set; }
    public string? BedType { get; private set; }
    public decimal? SizeInSquareMeters { get; private set; }
    public string? View { get; private set; }
    public HotelStatus Status { get; private set; }
    public bool IsDeleted { get; private set; }
    public DateTime CreatedAtUtc { get; private set; }
    public DateTime UpdatedAtUtc { get; private set; }

    public static RoomType Create(
        Guid id,
        Guid hotelId,
        string name,
        string? description,
        int maxOccupancy,
        string? bedType,
        decimal? sizeInSquareMeters,
        string? view,
        DateTime createdAtUtc)
    {
        ValidateDetails(id, hotelId, name, maxOccupancy, sizeInSquareMeters);

        return new RoomType(
            id,
            hotelId,
            name.Trim(),
            NormalizeOptional(description),
            maxOccupancy,
            NormalizeOptional(bedType),
            sizeInSquareMeters,
            NormalizeOptional(view),
            createdAtUtc);
    }

    public void UpdateDetails(
        string name,
        string? description,
        int maxOccupancy,
        string? bedType,
        decimal? sizeInSquareMeters,
        string? view,
        DateTime updatedAtUtc)
    {
        ValidateDetails(Id, HotelId, name, maxOccupancy, sizeInSquareMeters);

        Name = name.Trim();
        Description = NormalizeOptional(description);
        MaxOccupancy = maxOccupancy;
        BedType = NormalizeOptional(bedType);
        SizeInSquareMeters = sizeInSquareMeters;
        View = NormalizeOptional(view);
        UpdatedAtUtc = updatedAtUtc;
    }

    public void ChangeStatus(HotelStatus status, DateTime updatedAtUtc)
    {
        if (IsDeleted)
        {
            throw new DomainException("A deleted room type cannot change status.");
        }

        Status = status;
        UpdatedAtUtc = updatedAtUtc;
    }

    public void SoftDelete(DateTime updatedAtUtc)
    {
        if (IsDeleted)
        {
            return;
        }

        IsDeleted = true;
        Status = HotelStatus.Inactive;
        UpdatedAtUtc = updatedAtUtc;
    }

    private static void ValidateDetails(
        Guid id,
        Guid hotelId,
        string name,
        int maxOccupancy,
        decimal? sizeInSquareMeters)
    {
        if (id == Guid.Empty)
        {
            throw new DomainException("Room type id is required.");
        }

        if (hotelId == Guid.Empty)
        {
            throw new DomainException("Hotel id is required.");
        }

        if (string.IsNullOrWhiteSpace(name))
        {
            throw new DomainException("Room type name is required.");
        }

        if (name.Trim().Length > 200)
        {
            throw new DomainException("Room type name cannot exceed 200 characters.");
        }

        if (maxOccupancy <= 0)
        {
            throw new DomainException("Room type max occupancy must be greater than zero.");
        }

        if (sizeInSquareMeters is <= 0)
        {
            throw new DomainException("Room type size must be greater than zero.");
        }
    }

    private static string? NormalizeOptional(string? value)
    {
        return string.IsNullOrWhiteSpace(value) ? null : value.Trim();
    }
}
