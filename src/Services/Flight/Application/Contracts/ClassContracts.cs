using System.ComponentModel.DataAnnotations;
using Flight.Api.Domain.Enums;

namespace Flight.Api.Application.Contracts;

public class CreateFlightClassRequest
{
    [Required]
    [MaxLength(10)]
    public string Code { get; init; } = null!;

    [Required]
    [MaxLength(120)]
    public string Name { get; init; } = null!;

    public FlightClassType Type { get; init; }

    [Range(1, 2000)]
    public int Capacity { get; init; }

    [Range(typeof(decimal), "0", "1000000000")]
    public decimal BasePrice { get; init; }

    [Required]
    [StringLength(3, MinimumLength = 3)]
    public string Currency { get; init; } = null!;
}

public sealed class UpdateFlightClassRequest : CreateFlightClassRequest
{
}

public sealed class ChangeFlightClassStatusRequest
{
    public CatalogStatus Status { get; init; }
}

public sealed record FlightClassResponse(
    Guid Id,
    Guid FlightId,
    string Code,
    string Name,
    FlightClassType Type,
    int Capacity,
    decimal BasePrice,
    string Currency,
    CatalogStatus Status,
    DateTime CreatedAtUtc,
    DateTime UpdatedAtUtc);
