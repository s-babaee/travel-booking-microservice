using Booking.Api.Application.Contracts;
using Booking.Api.Application.Services;
using BuildingBlocks.Authorization;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace Booking.Api.Controllers;

[ApiController]
[Authorize]
[Route("api/orders")]
public sealed class OrderController(BookingService service) : ControllerBase
{
    [HttpGet("{orderId:guid}")]
    [HasPermission(PermissionCatalog.PaymentsViewOwn)]
    public async Task<ActionResult<OrderResponse>> Get(
        Guid orderId,
        CancellationToken cancellationToken) =>
        Ok(await service.GetOrderAsync(orderId, cancellationToken));

    [HttpGet]
    [HasPermission(PermissionCatalog.PaymentsViewAll)]
    public async Task<ActionResult<PagedResponse<OrderResponse>>> List(
        [FromQuery] int page = 1,
        [FromQuery] int pageSize = 50,
        CancellationToken cancellationToken = default) =>
        Ok(await service.ListOrdersAsync(page, pageSize, cancellationToken));
}
