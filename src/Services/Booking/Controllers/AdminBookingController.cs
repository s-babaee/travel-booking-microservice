using Booking.Api.Application.Contracts;
using Booking.Api.Application.Services;
using Booking.Api.Domain.Enums;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace Booking.Api.Controllers;

[ApiController]
[Authorize(Policy = "admin")]
[Route("api/admin/bookings")]
public sealed class AdminBookingController(BookingService service) : ControllerBase
{
    [HttpGet("search")]
    public async Task<ActionResult<PagedResponse<BookingResponse>>> Search(
        [FromQuery] Guid? userId,
        [FromQuery] BookingStatus? status,
        [FromQuery] BookingType? type,
        [FromQuery] DateTime? fromUtc,
        [FromQuery] DateTime? toUtc,
        [FromQuery] int page = 1,
        [FromQuery] int pageSize = 50,
        CancellationToken cancellationToken = default) =>
        Ok(await service.SearchAsync(
            new BookingSearchQuery(
                userId,
                status,
                type,
                fromUtc,
                toUtc,
                page,
                pageSize),
            cancellationToken));

    [HttpPatch("{bookingId:guid}/status")]
    public async Task<ActionResult<BookingResponse>> ChangeStatus(
        Guid bookingId,
        AdminStatusChangeRequest request,
        CancellationToken cancellationToken) =>
        Ok(await service.ChangeStatusAsync(
            bookingId,
            request,
            cancellationToken));

    [HttpGet("stats")]
    public async Task<ActionResult<BookingStatsResponse>> Stats(
        [FromQuery] DateTime? fromUtc,
        [FromQuery] DateTime? toUtc,
        CancellationToken cancellationToken) =>
        Ok(await service.GetStatsAsync(
            fromUtc,
            toUtc,
            cancellationToken));
}
