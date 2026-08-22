using BuildingBlocks.Contracts.Integrations;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Payment.Api.Application.Contracts;
using Payment.Api.Application.Services;
using RefundApiResponse = Payment.Api.Application.Contracts.RefundResponse;

namespace Payment.Api.Controllers;

[ApiController]
[Authorize]
[Route("api/payments")]
public sealed class RefundController(PaymentService service) : ControllerBase
{
    [HttpPost("{paymentId:guid}/refund")]
    public async Task<ActionResult<RefundApiResponse>> Refund(
        Guid paymentId,
        RefundPaymentRequest request,
        CancellationToken cancellationToken) =>
        Ok(await service.RefundAsync(
            new RefundPaymentCommand(paymentId, request.Reason),
            cancellationToken));

    [HttpGet("~/api/refunds/{refundId:guid}")]
    public async Task<ActionResult<RefundApiResponse>> Get(
        Guid refundId,
        CancellationToken cancellationToken)
    {
        // The service uses the same authorization rules for refund lookup.
        return Ok(await service.GetRefundAsync(refundId, cancellationToken));
    }
}
