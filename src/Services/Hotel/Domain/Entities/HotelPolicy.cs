using Hotel.Api.Domain.Common;

namespace Hotel.Api.Domain.Entities;

public sealed class HotelPolicy : Entity<Guid>
{
    private HotelPolicy()
    {
    }

    private HotelPolicy(
        Guid id,
        Guid hotelId,
        string policyType,
        string title,
        string content,
        string? conditions,
        DateTime createdAtUtc)
    {
        Id = id;
        HotelId = hotelId;
        PolicyType = policyType;
        Title = title;
        Content = content;
        Conditions = conditions;
        CreatedAtUtc = createdAtUtc;
        UpdatedAtUtc = createdAtUtc;
    }

    public Guid HotelId { get; private set; }
    public string PolicyType { get; private set; } = null!;
    public string Title { get; private set; } = null!;
    public string Content { get; private set; } = null!;
    public string? Conditions { get; private set; }
    public DateTime CreatedAtUtc { get; private set; }
    public DateTime UpdatedAtUtc { get; private set; }

    public static HotelPolicy Create(
        Guid id,
        Guid hotelId,
        string policyType,
        string title,
        string content,
        string? conditions,
        DateTime createdAtUtc)
    {
        Validate(id, hotelId, policyType, title, content);

        return new HotelPolicy(
            id,
            hotelId,
            policyType.Trim(),
            title.Trim(),
            content.Trim(),
            NormalizeOptional(conditions),
            createdAtUtc);
    }

    public void Update(
        string policyType,
        string title,
        string content,
        string? conditions,
        DateTime updatedAtUtc)
    {
        Validate(Id, HotelId, policyType, title, content);

        PolicyType = policyType.Trim();
        Title = title.Trim();
        Content = content.Trim();
        Conditions = NormalizeOptional(conditions);
        UpdatedAtUtc = updatedAtUtc;
    }

    private static void Validate(
        Guid id,
        Guid hotelId,
        string policyType,
        string title,
        string content)
    {
        if (id == Guid.Empty || hotelId == Guid.Empty)
        {
            throw new DomainException("Policy and hotel ids are required.");
        }

        if (string.IsNullOrWhiteSpace(policyType))
        {
            throw new DomainException("Policy type is required.");
        }

        if (string.IsNullOrWhiteSpace(title))
        {
            throw new DomainException("Policy title is required.");
        }

        if (string.IsNullOrWhiteSpace(content))
        {
            throw new DomainException("Policy content is required.");
        }
    }

    private static string? NormalizeOptional(string? value)
    {
        return string.IsNullOrWhiteSpace(value) ? null : value.Trim();
    }
}
