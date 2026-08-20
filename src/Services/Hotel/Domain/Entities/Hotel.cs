using Hotel.Api.Domain.Common;
using Hotel.Api.Domain.Enums;

namespace Hotel.Api.Domain.Entities;

public sealed class Hotel : Entity<Guid>
{
    private Hotel()
    {
    }

    private Hotel(
        Guid id,
        string name,
        string? description,
        int starRating,
        string addressLine1,
        string? addressLine2,
        string city,
        string? stateOrProvince,
        string country,
        string? postalCode,
        string? phoneNumber,
        string? email,
        string? websiteUrl,
        decimal? latitude,
        decimal? longitude,
        DateTime createdAtUtc)
    {
        Id = id;
        Name = name;
        Description = description;
        StarRating = starRating;
        AddressLine1 = addressLine1;
        AddressLine2 = addressLine2;
        City = city;
        StateOrProvince = stateOrProvince;
        Country = country;
        PostalCode = postalCode;
        PhoneNumber = phoneNumber;
        Email = email;
        WebsiteUrl = websiteUrl;
        Latitude = latitude;
        Longitude = longitude;
        Status = HotelStatus.Active;
        CreatedAtUtc = createdAtUtc;
        UpdatedAtUtc = createdAtUtc;
    }

    public string Name { get; private set; } = null!;
    public string? Description { get; private set; }
    public int StarRating { get; private set; }
    public string AddressLine1 { get; private set; } = null!;
    public string? AddressLine2 { get; private set; }
    public string City { get; private set; } = null!;
    public string? StateOrProvince { get; private set; }
    public string Country { get; private set; } = null!;
    public string? PostalCode { get; private set; }
    public string? PhoneNumber { get; private set; }
    public string? Email { get; private set; }
    public string? WebsiteUrl { get; private set; }
    public decimal? Latitude { get; private set; }
    public decimal? Longitude { get; private set; }
    public HotelStatus Status { get; private set; }
    public bool IsDeleted { get; private set; }
    public DateTime CreatedAtUtc { get; private set; }
    public DateTime UpdatedAtUtc { get; private set; }

    public static Hotel Create(
        Guid id,
        string name,
        string? description,
        int starRating,
        string addressLine1,
        string? addressLine2,
        string city,
        string? stateOrProvince,
        string country,
        string? postalCode,
        string? phoneNumber,
        string? email,
        string? websiteUrl,
        decimal? latitude,
        decimal? longitude,
        DateTime createdAtUtc)
    {
        ValidateDetails(
            name,
            starRating,
            addressLine1,
            city,
            country,
            latitude,
            longitude);

        if (id == Guid.Empty)
        {
            throw new DomainException("Hotel id is required.");
        }

        return new Hotel(
            id,
            name.Trim(),
            NormalizeOptional(description),
            starRating,
            addressLine1.Trim(),
            NormalizeOptional(addressLine2),
            city.Trim(),
            NormalizeOptional(stateOrProvince),
            country.Trim(),
            NormalizeOptional(postalCode),
            NormalizeOptional(phoneNumber),
            NormalizeOptional(email),
            NormalizeOptional(websiteUrl),
            latitude,
            longitude,
            createdAtUtc);
    }

    public void UpdateDetails(
        string name,
        string? description,
        int starRating,
        string addressLine1,
        string? addressLine2,
        string city,
        string? stateOrProvince,
        string country,
        string? postalCode,
        string? phoneNumber,
        string? email,
        string? websiteUrl,
        decimal? latitude,
        decimal? longitude,
        DateTime updatedAtUtc)
    {
        ValidateDetails(
            name,
            starRating,
            addressLine1,
            city,
            country,
            latitude,
            longitude);

        Name = name.Trim();
        Description = NormalizeOptional(description);
        StarRating = starRating;
        AddressLine1 = addressLine1.Trim();
        AddressLine2 = NormalizeOptional(addressLine2);
        City = city.Trim();
        StateOrProvince = NormalizeOptional(stateOrProvince);
        Country = country.Trim();
        PostalCode = NormalizeOptional(postalCode);
        PhoneNumber = NormalizeOptional(phoneNumber);
        Email = NormalizeOptional(email);
        WebsiteUrl = NormalizeOptional(websiteUrl);
        Latitude = latitude;
        Longitude = longitude;
        UpdatedAtUtc = updatedAtUtc;
    }

    public void ChangeStatus(HotelStatus status, DateTime updatedAtUtc)
    {
        if (IsDeleted)
        {
            throw new DomainException("A deleted hotel cannot change status.");
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
        string name,
        int starRating,
        string addressLine1,
        string city,
        string country,
        decimal? latitude,
        decimal? longitude)
    {
        if (string.IsNullOrWhiteSpace(name))
        {
            throw new DomainException("Hotel name is required.");
        }

        if (name.Trim().Length > 200)
        {
            throw new DomainException("Hotel name cannot exceed 200 characters.");
        }

        if (starRating is < 0 or > 5)
        {
            throw new DomainException("Hotel star rating must be between 0 and 5.");
        }

        if (string.IsNullOrWhiteSpace(addressLine1))
        {
            throw new DomainException("Hotel address is required.");
        }

        if (string.IsNullOrWhiteSpace(city))
        {
            throw new DomainException("Hotel city is required.");
        }

        if (string.IsNullOrWhiteSpace(country))
        {
            throw new DomainException("Hotel country is required.");
        }

        if (latitude is < -90 or > 90)
        {
            throw new DomainException("Latitude must be between -90 and 90.");
        }

        if (longitude is < -180 or > 180)
        {
            throw new DomainException("Longitude must be between -180 and 180.");
        }
    }

    private static string? NormalizeOptional(string? value)
    {
        return string.IsNullOrWhiteSpace(value) ? null : value.Trim();
    }
}
