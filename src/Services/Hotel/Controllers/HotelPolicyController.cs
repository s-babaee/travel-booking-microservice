using Hotel.Api.Application.Abstractions;
using Hotel.Api.Application.Contracts;
using Microsoft.AspNetCore.Mvc;

namespace Hotel.Api.Controllers;

[ApiController]
[Route("api/hotels/{hotelId:guid}/policies")]
public sealed class HotelPolicyController : ControllerBase
{
    private readonly IHotelPolicyService _policyService;

    public HotelPolicyController(IHotelPolicyService policyService)
    {
        _policyService = policyService;
    }

    [HttpPost]
    [ProducesResponseType(typeof(HotelPolicyResponse), StatusCodes.Status201Created)]
    public async Task<ActionResult<HotelPolicyResponse>> Create(
        Guid hotelId,
        [FromBody] CreateHotelPolicyRequest request,
        CancellationToken cancellationToken)
    {
        var response = await _policyService.CreateAsync(
            hotelId,
            request,
            cancellationToken);

        return CreatedAtAction(
            nameof(List),
            new { hotelId },
            response);
    }

    [HttpGet]
    [ProducesResponseType(typeof(IReadOnlyList<HotelPolicyResponse>), StatusCodes.Status200OK)]
    public async Task<ActionResult<IReadOnlyList<HotelPolicyResponse>>> List(
        Guid hotelId,
        CancellationToken cancellationToken)
    {
        return Ok(await _policyService.ListByHotelAsync(
            hotelId,
            cancellationToken));
    }

    [HttpPut("{policyId:guid}")]
    [ProducesResponseType(typeof(HotelPolicyResponse), StatusCodes.Status200OK)]
    public async Task<ActionResult<HotelPolicyResponse>> Update(
        Guid hotelId,
        Guid policyId,
        [FromBody] UpdateHotelPolicyRequest request,
        CancellationToken cancellationToken)
    {
        return Ok(await _policyService.UpdateAsync(
            hotelId,
            policyId,
            request,
            cancellationToken));
    }

    [HttpDelete("{policyId:guid}")]
    [ProducesResponseType(StatusCodes.Status204NoContent)]
    public async Task<IActionResult> Delete(
        Guid hotelId,
        Guid policyId,
        CancellationToken cancellationToken)
    {
        await _policyService.DeleteAsync(
            hotelId,
            policyId,
            cancellationToken);
        return NoContent();
    }
}
