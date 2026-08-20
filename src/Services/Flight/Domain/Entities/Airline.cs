using Flight.Api.Domain.Common;
using Flight.Api.Domain.Enums;

namespace Flight.Api.Domain.Entities;

public sealed class Airline : Entity<Guid>
{
    private Airline()
    {
    }

    private Airline(
        Guid id,
        string name,
        string iataCode,
        string icaoCode,
        string country,
        string? websiteUrl,
        DateTime createdAtUtc)
    {
        Id = id;
        Name = name;
        IataCode = iataCode;
        IcaoCode = icaoCode;
        Country = country;
        WebsiteUrl = websiteUrl;
        Status = CatalogStatus.Active;
        CreatedAtUtc = createdAtUtc;
        UpdatedAtUtc = createdAtUtc;
    }

    public string Name { get; private set; } = null!;
    public string IataCode { get; private set; } = null!;
    public string IcaoCode { get; private set; } = null!;
    public string Country { get; private set; } = null!;
    public string? WebsiteUrl { get; private set; }
    public CatalogStatus Status { get; private set; }
    public bool IsDeleted { get; private set; }
    public DateTime CreatedAtUtc { get; private set; }
    public DateTime UpdatedAtUtc { get; private set; }

    public static Airline Create(
        Guid id,
        string name,
        string iataCode,
        string icaoCode,
        string country,
        string? websiteUrl,
        DateTime createdAtUtc)
    {
        Validate(id, name, iataCode, icaoCode, country);
        return new Airline(
            id,
            name.Trim(),
            NormalizeCode(iataCode, 2),
            NormalizeCode(icaoCode, 3),
            country.Trim(),
            NormalizeOptional(websiteUrl),
            createdAtUtc);
    }

    public void Update(
        string name,
        string iataCode,
        string icaoCode,
        string country,
        string? websiteUrl,
        DateTime updatedAtUtc)
    {
        Validate(Id, name, iataCode, icaoCode, country);
        Name = name.Trim();
        IataCode = NormalizeCode(iataCode, 2);
        IcaoCode = NormalizeCode(icaoCode, 3);
        Country = country.Trim();
        WebsiteUrl = NormalizeOptional(websiteUrl);
        UpdatedAtUtc = updatedAtUtc;
    }

    public void ChangeStatus(CatalogStatus status, DateTime updatedAtUtc)
    {
        if (IsDeleted)
        {
            throw new DomainException("A deleted airline cannot change status.");
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
        Status = CatalogStatus.Inactive;
        UpdatedAtUtc = updatedAtUtc;
    }

    private static void Validate(
        Guid id,
        string name,
        string iataCode,
        string icaoCode,
        string country)
    {
        if (id == Guid.Empty)
        {
            throw new DomainException("Airline id is required.");
        }

        if (string.IsNullOrWhiteSpace(name) || name.Trim().Length > 200)
        {
            throw new DomainException("Airline name is required and cannot exceed 200 characters.");
        }

        _ = NormalizeCode(iataCode, 2);
        _ = NormalizeCode(icaoCode, 3);

        if (string.IsNullOrWhiteSpace(country) || country.Trim().Length > 120)
        {
            throw new DomainException("Airline country is required and cannot exceed 120 characters.");
        }
    }

    private static string NormalizeCode(string value, int length)
    {
        var code = value?.Trim().ToUpperInvariant();
        if (code is null || code.Length != length || code.Any(character => !char.IsLetterOrDigit(character)))
        {
            throw new DomainException($"Airline code must contain exactly {length} letters or digits.");
        }

        return code;
    }

    private static string? NormalizeOptional(string? value)
    {
        return string.IsNullOrWhiteSpace(value) ? null : value.Trim();
    }
}
