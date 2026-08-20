using Hotel.Api.Domain.Common;
using Hotel.Api.Domain.Enums;

namespace Hotel.Api.Domain.Entities;

public sealed class Amenity : Entity<Guid>
{
    private Amenity()
    {
    }

    private Amenity(
        Guid id,
        string name,
        AmenityType type,
        string? description,
        DateTime createdAtUtc)
    {
        Id = id;
        Name = name;
        Type = type;
        Description = description;
        CreatedAtUtc = createdAtUtc;
        UpdatedAtUtc = createdAtUtc;
    }

    public string Name { get; private set; } = null!;
    public AmenityType Type { get; private set; }
    public string? Description { get; private set; }
    public bool IsDeleted { get; private set; }
    public DateTime CreatedAtUtc { get; private set; }
    public DateTime UpdatedAtUtc { get; private set; }

    public static Amenity Create(
        Guid id,
        string name,
        AmenityType type,
        string? description,
        DateTime createdAtUtc)
    {
        if (id == Guid.Empty)
        {
            throw new DomainException("Amenity id is required.");
        }

        ValidateName(name);

        return new Amenity(
            id,
            name.Trim(),
            type,
            NormalizeOptional(description),
            createdAtUtc);
    }

    public void Update(
        string name,
        AmenityType type,
        string? description,
        DateTime updatedAtUtc)
    {
        ValidateName(name);

        Name = name.Trim();
        Type = type;
        Description = NormalizeOptional(description);
        UpdatedAtUtc = updatedAtUtc;
    }

    public void SoftDelete(DateTime updatedAtUtc)
    {
        if (IsDeleted)
        {
            return;
        }

        IsDeleted = true;
        UpdatedAtUtc = updatedAtUtc;
    }

    private static void ValidateName(string name)
    {
        if (string.IsNullOrWhiteSpace(name))
        {
            throw new DomainException("Amenity name is required.");
        }

        if (name.Trim().Length > 150)
        {
            throw new DomainException("Amenity name cannot exceed 150 characters.");
        }
    }

    private static string? NormalizeOptional(string? value)
    {
        return string.IsNullOrWhiteSpace(value) ? null : value.Trim();
    }
}
