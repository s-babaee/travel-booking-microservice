using Auth.Api.Domain.Common;

namespace Auth.Api.Domain.Entities;

public sealed class Role : Entity<Guid>
{
    private Role()
    {
    }

    private Role(Guid id, string name, string? description, DateTime createdAtUtc)
    {
        Id = id;
        Name = name;
        Description = description;
        CreatedAtUtc = createdAtUtc;
        UpdatedAtUtc = createdAtUtc;
    }

    public string Name { get; private set; } = null!;
    public string? Description { get; private set; }
    public bool IsDeleted { get; private set; }
    public DateTime CreatedAtUtc { get; private set; }
    public DateTime UpdatedAtUtc { get; private set; }

    public static Role Create(Guid id, string name, string? description, DateTime createdAtUtc)
    {
        if (id == Guid.Empty)
        {
            throw new ArgumentException("Role id is required.", nameof(id));
        }

        if (string.IsNullOrWhiteSpace(name))
        {
            throw new ArgumentException("Role name is required.", nameof(name));
        }

        return new Role(id, name.Trim(), description?.Trim(), createdAtUtc);
    }

    public void Update(string name, string? description, DateTime updatedAtUtc)
    {
        if (string.IsNullOrWhiteSpace(name))
        {
            throw new ArgumentException("Role name is required.", nameof(name));
        }

        Name = name.Trim();
        Description = description?.Trim();
        UpdatedAtUtc = updatedAtUtc;
    }

    public void SoftDelete(DateTime updatedAtUtc)
    {
        IsDeleted = true;
        UpdatedAtUtc = updatedAtUtc;
    }
}
