using System.Net.Http.Json;
using System.Net.Http.Headers;
using Booking.Api.Application.Abstractions;
using Booking.Api.Application.Exceptions;
using BuildingBlocks.Contracts.Integrations;

namespace Booking.Api.Infrastructure.Integrations;

public sealed class PaymentHttpGateway(
    HttpClient httpClient,
    ILogger<PaymentHttpGateway> logger,
    IHttpContextAccessor httpContextAccessor) : IPaymentGateway
{
    public async Task<PaymentAuthorizationResult> AuthorizeAsync(
        PaymentAuthorizationCommand command,
        CancellationToken cancellationToken)
    {
        try
        {
            using var response = await SendAsync(
                HttpMethod.Post,
                "api/payments/authorize",
                new AuthorizePaymentRequest(
                    command.BookingId,
                    command.UserId,
                    command.Amount,
                    command.Currency,
                    command.PaymentMethodToken,
                    command.IdempotencyKey
                        ?? command.BookingId.ToString("N")),
                cancellationToken);

            var result = await response.Content.ReadFromJsonAsync<
                AuthorizePaymentResponse>(cancellationToken: cancellationToken);
            if (result is not null)
            {
                return new PaymentAuthorizationResult(
                    result.Succeeded,
                    result.TransactionId,
                    result.FailureReason);
            }

            var detail = await response.Content.ReadAsStringAsync(
                cancellationToken);
            throw new ExternalServiceException(
                "Payment Service",
                $"authorization returned {(int)response.StatusCode}: {detail}");
        }
        catch (ExternalServiceException)
        {
            throw;
        }
        catch (Exception exception) when (
            exception is HttpRequestException or TaskCanceledException)
        {
            logger.LogWarning(
                exception,
                "Payment authorization failed for booking {BookingId}.",
                command.BookingId);
            throw new ExternalServiceException(
                "Payment Service",
                "the service could not be reached.");
        }
    }

    public async Task VoidAsync(
        Guid transactionId,
        string reason,
        CancellationToken cancellationToken)
    {
        try
        {
            using var response = await SendAsync(
                HttpMethod.Post,
                $"api/payments/{transactionId}/void",
                new VoidPaymentRequest(reason),
                cancellationToken);
            if (!response.IsSuccessStatusCode)
            {
                var detail = await response.Content.ReadAsStringAsync(
                    cancellationToken);
                throw new ExternalServiceException(
                    "Payment Service",
                    $"void failed with {(int)response.StatusCode}: {detail}");
            }
        }
        catch (ExternalServiceException)
        {
            throw;
        }
        catch (Exception exception) when (
            exception is HttpRequestException or TaskCanceledException)
        {
            logger.LogWarning(
                exception,
                "Payment void failed for transaction {TransactionId}.",
                transactionId);
            throw new ExternalServiceException(
                "Payment Service",
                "the service could not be reached while compensating.");
        }
    }

    public async Task RefundAsync(
        Guid transactionId,
        string reason,
        CancellationToken cancellationToken)
    {
        try
        {
            using var response = await SendAsync(
                HttpMethod.Post,
                $"api/payments/{transactionId}/refund",
                new RefundPaymentRequest(reason),
                cancellationToken);
            if (!response.IsSuccessStatusCode)
            {
                var detail = await response.Content.ReadAsStringAsync(
                    cancellationToken);
                throw new ExternalServiceException(
                    "Payment Service",
                    $"refund failed with {(int)response.StatusCode}: {detail}");
            }
        }
        catch (ExternalServiceException)
        {
            throw;
        }
        catch (Exception exception) when (
            exception is HttpRequestException or TaskCanceledException)
        {
            logger.LogWarning(
                exception,
                "Payment refund failed for transaction {TransactionId}.",
                transactionId);
            throw new ExternalServiceException(
                "Payment Service",
                "the service could not be reached while refunding.");
        }
    }

    private async Task<HttpResponseMessage> SendAsync<TBody>(
        HttpMethod method,
        string path,
        TBody body,
        CancellationToken cancellationToken)
    {
        using var request = new HttpRequestMessage(method, path)
        {
            Content = JsonContent.Create(body)
        };
        var accessToken = httpContextAccessor.HttpContext?
            .Request.Headers.Authorization
            .ToString();
        if (!string.IsNullOrWhiteSpace(accessToken)
            && AuthenticationHeaderValue.TryParse(
                accessToken,
                out var authorization))
        {
            request.Headers.Authorization = authorization;
        }

        return await httpClient.SendAsync(request, cancellationToken);
    }
}

public sealed class MockPaymentGateway : IPaymentGateway
{
    public Task<PaymentAuthorizationResult> AuthorizeAsync(
        PaymentAuthorizationCommand command,
        CancellationToken cancellationToken)
    {
        if (command.PaymentMethodToken.Equals(
                "declined",
                StringComparison.OrdinalIgnoreCase))
        {
            return Task.FromResult(new PaymentAuthorizationResult(
                false,
                null,
                "Payment was declined by the mock provider."));
        }

        return Task.FromResult(new PaymentAuthorizationResult(
            true,
            Guid.NewGuid(),
            null));
    }

    public Task VoidAsync(
        Guid transactionId,
        string reason,
        CancellationToken cancellationToken) =>
        Task.CompletedTask;

    public Task RefundAsync(
        Guid transactionId,
        string reason,
        CancellationToken cancellationToken) =>
        Task.CompletedTask;
}
