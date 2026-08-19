using Auth.Api.Domain.Common;

namespace Auth.Api.Domain.Entities;

public sealed class PasswordResetToken : Entity<Guid>
{
    private PasswordResetToken()
    {
    }

    private PasswordResetToken(
        Guid id,
        Guid userId,
        string tokenHash,
        DateTime expiresAtUtc,
        DateTime createdAtUtc)
    {
        Id = id;
        UserId = userId;
        TokenHash = tokenHash;
        ExpiresAtUtc = expiresAtUtc;
        CreatedAtUtc = createdAtUtc;
    }

    public Guid UserId { get; private set; }
    public string TokenHash { get; private set; } = null!;
    public DateTime ExpiresAtUtc { get; private set; }
    public DateTime CreatedAtUtc { get; private set; }
    public DateTime? UsedAtUtc { get; private set; }

    public static PasswordResetToken Create(
        Guid id,
        Guid userId,
        string tokenHash,
        DateTime expiresAtUtc,
        DateTime createdAtUtc)
    {
        return new PasswordResetToken(id, userId, tokenHash, expiresAtUtc, createdAtUtc);
    }

    public bool IsValid(DateTime nowUtc) => UsedAtUtc is null && ExpiresAtUtc > nowUtc;

    public void MarkUsed(DateTime usedAtUtc)
    {
        UsedAtUtc = usedAtUtc;
    }
}
