namespace Payment.Api.Domain.Enums;

public enum PaymentStatus
{
    Authorized = 1,
    Failed = 2,
    Voided = 3,
    Refunded = 4
}
