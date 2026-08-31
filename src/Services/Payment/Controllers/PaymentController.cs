using BuildingBlocks.Contracts.Integrations;
using BuildingBlocks.Authorization;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Payment.Api.Application.Contracts;
using Payment.Api.Application.Services;

namespace Payment.Api.Controllers;

[ApiController]
[Authorize]
[Route("api/payments")]
public sealed class PaymentController(PaymentService service) : ControllerBase
{
    [HttpPost("authorize")]
    [HasPermission(PermissionCatalog.PaymentsInitiate)]
    public async Task<ActionResult<AuthorizePaymentResponse>> Authorize(
        AuthorizePaymentRequest request,
        CancellationToken cancellationToken) =>
        Ok(await service.AuthorizeAsync(request, cancellationToken));

    [HttpGet("{paymentId:guid}")]
    [HasPermission(PermissionCatalog.PaymentsViewOwn)]
    public async Task<ActionResult<PaymentResponse>> Get(
        Guid paymentId,
        CancellationToken cancellationToken) =>
        Ok(await service.GetAsync(paymentId, cancellationToken));

    [HttpGet("booking/{bookingId:guid}")]
    [HasPermission(PermissionCatalog.PaymentsViewOwn)]
    public async Task<ActionResult<IReadOnlyList<PaymentResponse>>> GetByBooking(
        Guid bookingId,
        CancellationToken cancellationToken) =>
        Ok(await service.ListByBookingAsync(bookingId, cancellationToken));

    [HttpPost("{paymentId:guid}/void")]
    public async Task<IActionResult> Void(
        Guid paymentId,
        VoidPaymentRequest request,
        CancellationToken cancellationToken)
    {
        await service.VoidAsync(paymentId, request.Reason, cancellationToken);
        return NoContent();
    }
}
