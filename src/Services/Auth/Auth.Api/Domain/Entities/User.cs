using Auth.Api.Domain.Common;
using Auth.Api.Domain.Enums;

namespace Auth.Api.Domain.Entities;

public sealed class User : Entity<Guid>
{
    private User()
    {
    }

    private User(
        Guid id,
        string username,
        string email,
        string? firstName,
        string? lastName,
        DateTime createdAtUtc)
    {
        Id = id;
        Username = username;
        Email = email;
        FirstName = firstName;
        LastName = lastName;
        Status = UserStatus.Active;
        CreatedAtUtc = createdAtUtc;
        UpdatedAtUtc = createdAtUtc;
    }

    public string Username { get; private set; } = null!;
    public string Email { get; private set; } = null!;
    public string? FirstName { get; private set; }
    public string? LastName { get; private set; }
    public UserStatus Status { get; private set; }
    public bool IsDeleted { get; private set; }
    public DateTime CreatedAtUtc { get; private set; }
    public DateTime UpdatedAtUtc { get; private set; }

    public static User Create(
        Guid id,
        string username,
        string email,
        string? firstName,
        string? lastName,
        DateTime createdAtUtc)
    {
        if (id == Guid.Empty)
        {
            throw new ArgumentException("User id is required.", nameof(id));
        }

        if (string.IsNullOrWhiteSpace(username))
        {
            throw new ArgumentException("Username is required.", nameof(username));
        }

        if (string.IsNullOrWhiteSpace(email))
        {
            throw new ArgumentException("Email is required.", nameof(email));
        }

        return new User(id, username.Trim(), email.Trim().ToLowerInvariant(), firstName?.Trim(), lastName?.Trim(), createdAtUtc);
    }

    public void UpdateProfile(string email, string? firstName, string? lastName, DateTime updatedAtUtc)
    {
        if (string.IsNullOrWhiteSpace(email))
        {
            throw new ArgumentException("Email is required.", nameof(email));
        }

        Email = email.Trim().ToLowerInvariant();
        FirstName = firstName?.Trim();
        LastName = lastName?.Trim();
        UpdatedAtUtc = updatedAtUtc;
    }

    public void SetStatus(UserStatus status, DateTime updatedAtUtc)
    {
        Status = status;
        UpdatedAtUtc = updatedAtUtc;
    }

    public void SoftDelete(DateTime updatedAtUtc)
    {
        IsDeleted = true;
        Status = UserStatus.Inactive;
        UpdatedAtUtc = updatedAtUtc;
    }
}
