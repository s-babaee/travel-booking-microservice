using Flight.Api.Domain.Common;
using Flight.Api.Domain.Enums;

namespace Flight.Api.Domain.Entities;

public sealed class FlightClass : Entity<Guid>
{
    private FlightClass()
    {
    }

    private FlightClass(
        Guid id,
        Guid flightId,
        string code,
        string name,
        FlightClassType type,
        int capacity,
        decimal basePrice,
        string currency,
        DateTime createdAtUtc)
    {
        Id = id;
        FlightId = flightId;
        Code = code;
        Name = name;
        Type = type;
        Capacity = capacity;
        BasePrice = basePrice;
        Currency = currency;
        Status = CatalogStatus.Active;
        CreatedAtUtc = createdAtUtc;
        UpdatedAtUtc = createdAtUtc;
    }

    public Guid FlightId { get; private set; }
    public string Code { get; private set; } = null!;
    public string Name { get; private set; } = null!;
    public FlightClassType Type { get; private set; }
    public int Capacity { get; private set; }
    public decimal BasePrice { get; private set; }
    public string Currency { get; private set; } = null!;
    public CatalogStatus Status { get; private set; }
    public bool IsDeleted { get; private set; }
    public DateTime CreatedAtUtc { get; private set; }
    public DateTime UpdatedAtUtc { get; private set; }

    public static FlightClass Create(
        Guid id,
        Guid flightId,
        string code,
        string name,
        FlightClassType type,
        int capacity,
        decimal basePrice,
        string currency,
        DateTime createdAtUtc)
    {
        Validate(id, flightId, code, name, capacity, basePrice, currency);
        return new FlightClass(
            id,
            flightId,
            NormalizeCode(code),
            name.Trim(),
            type,
            capacity,
            basePrice,
            NormalizeCurrency(currency),
            createdAtUtc);
    }

    public void Update(
        string code,
        string name,
        FlightClassType type,
        int capacity,
        decimal basePrice,
        string currency,
        DateTime updatedAtUtc)
    {
        Validate(Id, FlightId, code, name, capacity, basePrice, currency);
        Code = NormalizeCode(code);
        Name = name.Trim();
        Type = type;
        Capacity = capacity;
        BasePrice = basePrice;
        Currency = NormalizeCurrency(currency);
        UpdatedAtUtc = updatedAtUtc;
    }

    public void ChangeStatus(CatalogStatus status, DateTime updatedAtUtc)
    {
        if (IsDeleted)
        {
            throw new DomainException("A deleted flight class cannot change status.");
        }

        Status = status;
        UpdatedAtUtc = updatedAtUtc;
    }

    public void SoftDelete(DateTime updatedAtUtc)
    {
        if (!IsDeleted)
        {
            IsDeleted = true;
            Status = CatalogStatus.Inactive;
            UpdatedAtUtc = updatedAtUtc;
        }
    }

    private static void Validate(
        Guid id,
        Guid flightId,
        string code,
        string name,
        int capacity,
        decimal basePrice,
        string currency)
    {
        if (id == Guid.Empty || flightId == Guid.Empty)
        {
            throw new DomainException("Flight class and flight ids are required.");
        }

        _ = NormalizeCode(code);
        if (string.IsNullOrWhiteSpace(name) || name.Trim().Length > 120)
        {
            throw new DomainException("Flight class name is required and cannot exceed 120 characters.");
        }

        if (capacity <= 0 || basePrice < 0)
        {
            throw new DomainException("Flight class capacity must be positive and price cannot be negative.");
        }

        _ = NormalizeCurrency(currency);
    }

    private static string NormalizeCode(string value)
    {
        var normalized = value?.Trim().ToUpperInvariant();
        if (normalized is null || normalized.Length is < 1 or > 10
            || normalized.Any(character => !char.IsLetterOrDigit(character)))
        {
            throw new DomainException("Flight class code must contain 1 to 10 letters or digits.");
        }

        return normalized;
    }

    private static string NormalizeCurrency(string value)
    {
        var normalized = value?.Trim().ToUpperInvariant();
        if (normalized is null || normalized.Length != 3
            || normalized.Any(character => !char.IsLetter(character)))
        {
            throw new DomainException("Currency must be a three-letter ISO code.");
        }

        return normalized;
    }
}
