using Payment.Api.Application.Abstractions;
using Payment.Api.Application.Contracts;

namespace Payment.Api.Application.Services;

public sealed class MockPaymentProvider : IPaymentProvider
{
    public Task<PaymentProviderResult> AuthorizeAsync(
        AuthorizePaymentCommand command,
        CancellationToken cancellationToken)
    {
        var token = command.PaymentMethodToken.Trim();
        if (token.Equals("declined", StringComparison.OrdinalIgnoreCase)
            || token.Equals("fail", StringComparison.OrdinalIgnoreCase))
        {
            return Task.FromResult(new PaymentProviderResult(
                false,
                "Payment was declined by the mock provider."));
        }

        return Task.FromResult(new PaymentProviderResult(true, null));
    }
}
