using Inventory.Api.Application.Abstractions;
using Inventory.Api.Application.Contracts;
using BuildingBlocks.Authorization;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace Inventory.Api.Controllers;

[ApiController]
[Authorize]
[Route("api/inventory/flights")]
public sealed class FlightInventoryController : ControllerBase
{
    private readonly IFlightInventoryService _service;

    public FlightInventoryController(IFlightInventoryService service)
    {
        _service = service;
    }

    [HttpPost("hold")]
    [HasPermission(PermissionCatalog.BookingsCreate)]
    [ProducesResponseType(typeof(InventoryHoldResponse), StatusCodes.Status200OK)]
    public async Task<ActionResult<InventoryHoldResponse>> Hold(
        [FromBody] FlightHoldRequest request,
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

    [HttpGet("{flightId:guid}/availability")]
    [HasPermission(PermissionCatalog.FlightsView)]
    [ProducesResponseType(
        typeof(IReadOnlyList<FlightAvailabilityResponse>),
        StatusCodes.Status200OK)]
    public async Task<ActionResult<IReadOnlyList<FlightAvailabilityResponse>>>
        GetAvailability(
            Guid flightId,
            [FromQuery] DateOnly date,
            [FromQuery] Guid? flightClassId,
            CancellationToken cancellationToken)
    {
        return Ok(await _service.GetAvailabilityAsync(
            flightId,
            date,
            flightClassId,
            cancellationToken));
    }

    [HttpPost("adjust")]
    [HasPermission(PermissionCatalog.HotelsInventoryManage)]
    [ProducesResponseType(
        typeof(IReadOnlyList<FlightAvailabilityResponse>),
        StatusCodes.Status200OK)]
    public async Task<ActionResult<IReadOnlyList<FlightAvailabilityResponse>>>
        Adjust(
            [FromBody] FlightInventoryAdjustmentRequest request,
            CancellationToken cancellationToken)
    {
        return Ok(await _service.AdjustAsync(request, cancellationToken));
    }
}
