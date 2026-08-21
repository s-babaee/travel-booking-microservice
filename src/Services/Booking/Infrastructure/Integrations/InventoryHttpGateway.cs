using System.Net.Http.Json;
using Booking.Api.Application.Abstractions;
using Booking.Api.Application.Exceptions;
using BuildingBlocks.Contracts.Integrations;

namespace Booking.Api.Infrastructure.Integrations;

public sealed class InventoryHttpGateway(
    HttpClient httpClient,
    ILogger<InventoryHttpGateway> logger) : IInventoryGateway
{
    public Task<InventoryHoldResult> HoldHotelAsync(
        HoldHotelCommand command,
        CancellationToken cancellationToken) =>
        SendHoldAsync(
            "api/inventory/hotels/hold",
            new HoldHotelInventoryRequest(
                command.HoldId,
                command.HotelId,
                command.CheckIn,
                command.CheckOut,
                command.Rooms
                    .Select(room => new InventoryRoomQuantity(
                        room.RoomTypeId,
                        room.Quantity))
                    .ToArray(),
                command.ExpiresAtUtc),
            cancellationToken);

    public Task<InventoryHoldResult> HoldFlightAsync(
        HoldFlightCommand command,
        CancellationToken cancellationToken) =>
        SendHoldAsync(
            "api/inventory/flights/hold",
            new HoldFlightInventoryRequest(
                command.HoldId,
                command.FlightId,
                command.Date,
                command.Classes
                    .Select(item => new InventoryFlightClassQuantity(
                        item.FlightClassId,
                        item.Quantity))
                    .ToArray(),
                command.ExpiresAtUtc),
            cancellationToken);

    public Task ConfirmAsync(
        Guid holdId,
        Domain.Enums.BookingType type,
        CancellationToken cancellationToken) =>
        SendCompleteAsync(type, "confirm", holdId, cancellationToken);

    public Task ReleaseAsync(
        Guid holdId,
        Domain.Enums.BookingType type,
        CancellationToken cancellationToken) =>
        SendCompleteAsync(type, "release", holdId, cancellationToken);

    private async Task<InventoryHoldResult> SendHoldAsync<TRequest>(
        string path,
        TRequest request,
        CancellationToken cancellationToken)
    {
        try
        {
            using var response = await httpClient.PostAsJsonAsync(
                path,
                request,
                cancellationToken);
            if (!response.IsSuccessStatusCode)
            {
                var detail = await response.Content.ReadAsStringAsync(
                    cancellationToken);
                throw new ExternalServiceException(
                    "Inventory Service",
                    $"hold failed with {(int)response.StatusCode}: {detail}");
            }

            var result = await response.Content.ReadFromJsonAsync<
                InventoryHoldResponse>(cancellationToken: cancellationToken)
                ?? throw new ExternalServiceException(
                    "Inventory Service",
                    "hold returned an empty response.");
            return new InventoryHoldResult(
                result.HoldId,
                result.Status,
                result.ExpiresAtUtc,
                result.CompletedAtUtc);
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
                "Inventory hold request failed.");
            throw new ExternalServiceException(
                "Inventory Service",
                "the service could not be reached.");
        }
    }

    private async Task SendCompleteAsync(
        Domain.Enums.BookingType type,
        string operation,
        Guid holdId,
        CancellationToken cancellationToken)
    {
        var path = type == Domain.Enums.BookingType.Hotel
            ? $"api/inventory/hotels/{operation}"
            : $"api/inventory/flights/{operation}";

        try
        {
            using var response = await httpClient.PostAsJsonAsync(
                path,
                new CompleteInventoryHoldRequest(holdId),
                cancellationToken);
            if (!response.IsSuccessStatusCode)
            {
                var detail = await response.Content.ReadAsStringAsync(
                    cancellationToken);
                throw new ExternalServiceException(
                    "Inventory Service",
                    $"{operation} failed with {(int)response.StatusCode}: {detail}");
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
                "Inventory {Operation} request failed for hold {HoldId}.",
                operation,
                holdId);
            throw new ExternalServiceException(
                "Inventory Service",
                "the service could not be reached.");
        }
    }

    private sealed record InventoryHoldResponse(
        Guid HoldId,
        Guid ResourceId,
        string Status,
        DateTime ExpiresAtUtc,
        DateTime? CompletedAtUtc,
        IReadOnlyList<InventoryHoldLineResponse> Lines);

    private sealed record InventoryHoldLineResponse(
        Guid ResourceTypeId,
        DateOnly Date,
        int Quantity);
}
