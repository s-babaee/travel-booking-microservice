using System.ComponentModel.DataAnnotations;
using Hotel.Api.Domain.Enums;

namespace Hotel.Api.Application.Contracts;

public class CreateAmenityRequest
{
    [Required]
    [MaxLength(150)]
    public string Name { get; init; } = null!;

    public AmenityType Type { get; init; }

    [MaxLength(500)]
    public string? Description { get; init; }
}

public sealed class UpdateAmenityRequest : CreateAmenityRequest
{
}

public sealed record AmenityResponse(
    Guid Id,
    string Name,
    AmenityType Type,
    string? Description,
    DateTime CreatedAtUtc,
    DateTime UpdatedAtUtc);
