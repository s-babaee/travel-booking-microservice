using Auth.Api.Domain.Common;

namespace Auth.Api.Domain.Entities;

public sealed class Permission : Entity<Guid>
{
    private Permission()
    {
    }

    private Permission(Guid id, string code, string? description, DateTime createdAtUtc)
    {
        Id = id;
        Code = code;
        Description = description;
        CreatedAtUtc = createdAtUtc;
        UpdatedAtUtc = createdAtUtc;
    }

    public string Code { get; private set; } = null!;
    public string? Description { get; private set; }
    public bool IsDeleted { get; private set; }
    public DateTime CreatedAtUtc { get; private set; }
    public DateTime UpdatedAtUtc { get; private set; }

    public static Permission Create(Guid id, string code, string? description, DateTime createdAtUtc)
    {
        if (id == Guid.Empty)
        {
            throw new ArgumentException("Permission id is required.", nameof(id));
        }

        if (string.IsNullOrWhiteSpace(code))
        {
            throw new ArgumentException("Permission code is required.", nameof(code));
        }

        return new Permission(id, code.Trim().ToLowerInvariant(), description?.Trim(), createdAtUtc);
    }

    public void Update(string code, string? description, DateTime updatedAtUtc)
    {
        if (string.IsNullOrWhiteSpace(code))
        {
            throw new ArgumentException("Permission code is required.", nameof(code));
        }

        Code = code.Trim().ToLowerInvariant();
        Description = description?.Trim();
        UpdatedAtUtc = updatedAtUtc;
    }

    public void SoftDelete(DateTime updatedAtUtc)
    {
        IsDeleted = true;
        UpdatedAtUtc = updatedAtUtc;
    }
}
