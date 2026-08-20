using System.ComponentModel.DataAnnotations;

namespace Hotel.Api.Application.Contracts;

public class CreateHotelPolicyRequest
{
    [Required]
    [MaxLength(100)]
    public string PolicyType { get; init; } = null!;

    [Required]
    [MaxLength(200)]
    public string Title { get; init; } = null!;

    [Required]
    [MaxLength(5000)]
    public string Content { get; init; } = null!;

    [MaxLength(5000)]
    public string? Conditions { get; init; }
}

public sealed class UpdateHotelPolicyRequest : CreateHotelPolicyRequest
{
}

public sealed record HotelPolicyResponse(
    Guid Id,
    Guid HotelId,
    string PolicyType,
    string Title,
    string Content,
    string? Conditions,
    DateTime CreatedAtUtc,
    DateTime UpdatedAtUtc);
