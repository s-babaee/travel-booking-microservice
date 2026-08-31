using Booking.Api.Application.Contracts;
using Booking.Api.Application.Services;
using BuildingBlocks.Authorization;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace Booking.Api.Controllers;

[ApiController]
[Authorize]
[Route("api/bookings")]
public sealed class BookingController(BookingService service) : ControllerBase
{
    [HttpPost("hotels")]
    [HasPermission(PermissionCatalog.BookingsCreate)]
    [ProducesResponseType(typeof(BookingResponse), StatusCodes.Status201Created)]
    public async Task<ActionResult<BookingResponse>> CreateHotel(
        CreateHotelBookingRequest request,
        CancellationToken cancellationToken)
    {
        var result = await service.CreateHotelAsync(request, cancellationToken);
        return CreatedAtAction(
            nameof(Get),
            new { bookingId = result.Id },
            result);
    }



    [HttpPost("flights")]
    [HasPermission(PermissionCatalog.BookingsCreate)]
    [ProducesResponseType(typeof(BookingResponse), StatusCodes.Status201Created)]
    public async Task<ActionResult<BookingResponse>> CreateFlight(
        CreateFlightBookingRequest request,
        CancellationToken cancellationToken)
    {
        var result = await service.CreateFlightAsync(request, cancellationToken);
        return CreatedAtAction(
            nameof(Get),
            new { bookingId = result.Id },
            result);
    }



    [HttpGet("{bookingId:guid}")]
    [HasPermission(PermissionCatalog.BookingsReadOwn)]
    public async Task<ActionResult<BookingResponse>> Get(
        Guid bookingId,
        CancellationToken cancellationToken) =>
        Ok(await service.GetAsync(bookingId, cancellationToken));



    [HttpGet("user/me")]
    [HasPermission(PermissionCatalog.BookingsReadOwn)]
    public async Task<ActionResult<PagedResponse<BookingResponse>>> ListMine(
        [FromQuery] int page = 1,
        [FromQuery] int pageSize = 50,
        CancellationToken cancellationToken = default) =>
        Ok(await service.ListMineAsync(page, pageSize, cancellationToken));



    [HttpPost("{bookingId:guid}/cancel")]
    [HasPermission(PermissionCatalog.BookingsCancelOwn)]
    public async Task<ActionResult<BookingResponse>> Cancel(
        Guid bookingId,
        CancelBookingRequest? request,
        CancellationToken cancellationToken) =>
        Ok(await service.CancelAsync(bookingId, request, cancellationToken));
}
