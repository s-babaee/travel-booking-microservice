using Hotel.Api.Application.Abstractions;
using Hotel.Api.Application.Contracts;
using BuildingBlocks.Authorization;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace Hotel.Api.Controllers;

[ApiController]
[Authorize]
[Route("api/hotels/{hotelId:guid}/policies")]
public sealed class HotelPolicyController : ControllerBase
{
    private readonly IHotelPolicyService _policyService;

    public HotelPolicyController(IHotelPolicyService policyService)
    {
        _policyService = policyService;
    }

    [HttpPost]
    [HasPermission(PermissionCatalog.HotelsCreate)]
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
    [HasPermission(PermissionCatalog.HotelsView)]
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
    [HasPermission(PermissionCatalog.HotelsUpdate)]
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
    [HasPermission(PermissionCatalog.HotelsDelete)]
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
