using System.ComponentModel.DataAnnotations;

namespace Flight.Api.Application.Contracts;

public class CreateFlightPolicyRequest
{
    [Required]
    [MaxLength(100)]
    public string PolicyType { get; init; } = null!;

    [Required]
    [MaxLength(200)]
    public string Title { get; init; } = null!;

    [Required]
    [MaxLength(10000)]
    public string Content { get; init; } = null!;

    [Range(typeof(decimal), "0", "1000")]
    public decimal? BaggageAllowanceKg { get; init; }

    public bool Refundable { get; init; }
    public bool Changeable { get; init; }

    [Range(typeof(decimal), "0", "1000000000")]
    public decimal? ChangeFee { get; init; }

    [MaxLength(5000)]
    public string? Conditions { get; init; }
}

public sealed class UpdateFlightPolicyRequest : CreateFlightPolicyRequest
{
}

public sealed record FlightPolicyResponse(
    Guid Id,
    Guid FlightId,
    string PolicyType,
    string Title,
    string Content,
    decimal? BaggageAllowanceKg,
    bool Refundable,
    bool Changeable,
    decimal? ChangeFee,
    string? Conditions,
    DateTime CreatedAtUtc,
    DateTime UpdatedAtUtc);
