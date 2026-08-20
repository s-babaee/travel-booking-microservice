using Flight.Api.Domain.Common;

namespace Flight.Api.Domain.Entities;

public sealed class FlightPolicy : Entity<Guid>
{
    private FlightPolicy()
    {
    }

    private FlightPolicy(
        Guid id,
        Guid flightId,
        string policyType,
        string title,
        string content,
        decimal? baggageAllowanceKg,
        bool refundable,
        bool changeable,
        decimal? changeFee,
        string? conditions,
        DateTime createdAtUtc)
    {
        Id = id;
        FlightId = flightId;
        PolicyType = policyType;
        Title = title;
        Content = content;
        BaggageAllowanceKg = baggageAllowanceKg;
        Refundable = refundable;
        Changeable = changeable;
        ChangeFee = changeFee;
        Conditions = conditions;
        CreatedAtUtc = createdAtUtc;
        UpdatedAtUtc = createdAtUtc;
    }

    public Guid FlightId { get; private set; }
    public string PolicyType { get; private set; } = null!;
    public string Title { get; private set; } = null!;
    public string Content { get; private set; } = null!;
    public decimal? BaggageAllowanceKg { get; private set; }
    public bool Refundable { get; private set; }
    public bool Changeable { get; private set; }
    public decimal? ChangeFee { get; private set; }
    public string? Conditions { get; private set; }
    public DateTime CreatedAtUtc { get; private set; }
    public DateTime UpdatedAtUtc { get; private set; }

    public static FlightPolicy Create(
        Guid id,
        Guid flightId,
        string policyType,
        string title,
        string content,
        decimal? baggageAllowanceKg,
        bool refundable,
        bool changeable,
        decimal? changeFee,
        string? conditions,
        DateTime createdAtUtc)
    {
        Validate(
            id,
            flightId,
            policyType,
            title,
            content,
            baggageAllowanceKg,
            changeFee);

        return new FlightPolicy(
            id,
            flightId,
            policyType.Trim(),
            title.Trim(),
            content.Trim(),
            baggageAllowanceKg,
            refundable,
            changeable,
            changeFee,
            NormalizeOptional(conditions),
            createdAtUtc);
    }

    public void Update(
        string policyType,
        string title,
        string content,
        decimal? baggageAllowanceKg,
        bool refundable,
        bool changeable,
        decimal? changeFee,
        string? conditions,
        DateTime updatedAtUtc)
    {
        Validate(
            Id,
            FlightId,
            policyType,
            title,
            content,
            baggageAllowanceKg,
            changeFee);

        PolicyType = policyType.Trim();
        Title = title.Trim();
        Content = content.Trim();
        BaggageAllowanceKg = baggageAllowanceKg;
        Refundable = refundable;
        Changeable = changeable;
        ChangeFee = changeFee;
        Conditions = NormalizeOptional(conditions);
        UpdatedAtUtc = updatedAtUtc;
    }

    private static void Validate(
        Guid id,
        Guid flightId,
        string policyType,
        string title,
        string content,
        decimal? baggageAllowanceKg,
        decimal? changeFee)
    {
        if (id == Guid.Empty || flightId == Guid.Empty)
        {
            throw new DomainException("Policy and flight ids are required.");
        }

        if (string.IsNullOrWhiteSpace(policyType) || policyType.Trim().Length > 100
            || string.IsNullOrWhiteSpace(title) || title.Trim().Length > 200
            || string.IsNullOrWhiteSpace(content))
        {
            throw new DomainException("Policy type, title and content are required.");
        }

        if (baggageAllowanceKg is < 0 || changeFee is < 0)
        {
            throw new DomainException("Policy numeric values cannot be negative.");
        }
    }

    private static string? NormalizeOptional(string? value)
    {
        return string.IsNullOrWhiteSpace(value) ? null : value.Trim();
    }
}
