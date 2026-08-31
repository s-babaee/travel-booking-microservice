using Inventory.Api.Application.Abstractions;
using Inventory.Api.Application.Contracts;
using BuildingBlocks.Authorization;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace Inventory.Api.Controllers;

[ApiController]
[Authorize]
[Route("api/inventory/hotels")]
public sealed class HotelInventoryController : ControllerBase
{
    private readonly IHotelInventoryService _service;

    public HotelInventoryController(IHotelInventoryService service)
    {
        _service = service;
    }

    [HttpPost("hold")]
    [HasPermission(PermissionCatalog.BookingsCreate)]
    [ProducesResponseType(typeof(InventoryHoldResponse), StatusCodes.Status200OK)]
    public async Task<ActionResult<InventoryHoldResponse>> Hold(
        [FromBody] HotelHoldRequest request,
        CancellationToken cancellationToken)
    {
        return Ok(await _service.HoldAsync(request, cancellationToken));
    }



    [HttpPost("confirm")]
    [HasPermission(PermissionCatalog.BookingsCreate)]
    [ProducesResponseType(typeof(InventoryHoldResponse), StatusCodes.Status200OK)]
    public async Task<ActionResult<InventoryHoldResponse>> Confirm(
        [FromBody] ConfirmReleaseRequest request,
        CancellationToken cancellationToken)
    {
        return Ok(await _service.ConfirmAsync(request, cancellationToken));
    }



    [HttpPost("release")]
    [HasPermission(PermissionCatalog.BookingsCancelOwn)]
    [ProducesResponseType(typeof(InventoryHoldResponse), StatusCodes.Status200OK)]
    public async Task<ActionResult<InventoryHoldResponse>> Release(
        [FromBody] ConfirmReleaseRequest request,
        CancellationToken cancellationToken)
    {
        return Ok(await _service.ReleaseAsync(request, cancellationToken));
    }



    [HttpGet("{hotelId:guid}/availability")]
    [HasPermission(PermissionCatalog.HotelsInventoryManage)]
    [ProducesResponseType(
        typeof(IReadOnlyList<HotelAvailabilityResponse>),
        StatusCodes.Status200OK)]
    public async Task<ActionResult<IReadOnlyList<HotelAvailabilityResponse>>>
        GetAvailability(
            Guid hotelId,
            [FromQuery] DateOnly from,
            [FromQuery] DateOnly to,
            [FromQuery] Guid? roomTypeId,
            CancellationToken cancellationToken)
    {
        return Ok(await _service.GetAvailabilityAsync(
            hotelId,
            from,
            to,
            roomTypeId,
            cancellationToken));
    }



    [HttpPost("adjust")]
    [HasPermission(PermissionCatalog.HotelsInventoryManage)]
    [ProducesResponseType(
        typeof(IReadOnlyList<HotelAvailabilityResponse>),
        StatusCodes.Status200OK)]
    public async Task<ActionResult<IReadOnlyList<HotelAvailabilityResponse>>>
        Adjust(
            [FromBody] HotelInventoryAdjustmentRequest request,
            CancellationToken cancellationToken)
    {
        return Ok(await _service.AdjustAsync(request, cancellationToken));
    }
}
