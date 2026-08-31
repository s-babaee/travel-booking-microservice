using Hotel.Api.Application.Abstractions;
using Hotel.Api.Application.Contracts;
using BuildingBlocks.Authorization;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace Hotel.Api.Controllers;

[ApiController]
[Authorize]
[Route("api/hotels")]
public sealed class HotelController : ControllerBase
{
    private readonly IHotelService _hotelService;

    public HotelController(IHotelService hotelService)
    {
        _hotelService = hotelService;
    }

    [HttpPost]
    [HasPermission(PermissionCatalog.HotelsCreate)]
    [ProducesResponseType(typeof(HotelResponse), StatusCodes.Status201Created)]
    public async Task<ActionResult<HotelResponse>> Create(
        [FromBody] CreateHotelRequest request,
        CancellationToken cancellationToken)
    {
        var response = await _hotelService.CreateAsync(
            request,
            cancellationToken);

        return CreatedAtAction(
            nameof(GetById),
            new { hotelId = response.Id },
            response);
    }



    [HttpGet("{hotelId:guid}")]
    [HasPermission(PermissionCatalog.HotelsView)]
    [ProducesResponseType(typeof(HotelResponse), StatusCodes.Status200OK)]
    public async Task<ActionResult<HotelResponse>> GetById(
        Guid hotelId,
        CancellationToken cancellationToken)
    {
        return Ok(await _hotelService.GetAsync(
            hotelId,
            cancellationToken));
    }


    [HttpGet]
    [HasPermission(PermissionCatalog.HotelsView)]
    [ProducesResponseType(typeof(IReadOnlyList<HotelResponse>), StatusCodes.Status200OK)]
    public async Task<ActionResult<IReadOnlyList<HotelResponse>>> GetAll(
    CancellationToken cancellationToken)
    {
        return Ok(await _hotelService.GetAllAsync(cancellationToken));
    }


    [HttpPut("{hotelId:guid}")]
    [HasPermission(PermissionCatalog.HotelsUpdate)]
    [ProducesResponseType(typeof(HotelResponse), StatusCodes.Status200OK)]
    public async Task<ActionResult<HotelResponse>> Update(
        Guid hotelId,
        [FromBody] UpdateHotelRequest request,
        CancellationToken cancellationToken)
    {
        return Ok(await _hotelService.UpdateAsync(
            hotelId,
            request,
            cancellationToken));
    }



    [HttpPatch("{hotelId:guid}/status")]
    [HasPermission(PermissionCatalog.HotelsUpdate)]
    [ProducesResponseType(typeof(HotelResponse), StatusCodes.Status200OK)]
    public async Task<ActionResult<HotelResponse>> ChangeStatus(
        Guid hotelId,
        [FromBody] ChangeHotelStatusRequest request,
        CancellationToken cancellationToken)
    {
        return Ok(await _hotelService.ChangeStatusAsync(
            hotelId,
            request,
            cancellationToken));
    }



    [HttpDelete("{hotelId:guid}")]
    [HasPermission(PermissionCatalog.HotelsDelete)]
    [ProducesResponseType(StatusCodes.Status204NoContent)]
    public async Task<IActionResult> Delete(
        Guid hotelId,
        CancellationToken cancellationToken)
    {
        await _hotelService.DeleteAsync(hotelId, cancellationToken);
        return NoContent();
    }
}
