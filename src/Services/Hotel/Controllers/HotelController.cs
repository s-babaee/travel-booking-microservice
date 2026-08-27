using Hotel.Api.Application.Abstractions;
using Hotel.Api.Application.Contracts;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace Hotel.Api.Controllers;

[ApiController]
[Authorize(Policy = "admin")]
[Route("api/hotels")]
public sealed class HotelController : ControllerBase
{
    private readonly IHotelService _hotelService;

    public HotelController(IHotelService hotelService)
    {
        _hotelService = hotelService;
    }

    [HttpPost]
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
    [ProducesResponseType(typeof(HotelResponse), StatusCodes.Status200OK)]
    public async Task<ActionResult<HotelResponse>> GetById(
        Guid hotelId,
        CancellationToken cancellationToken)
    {
        return Ok(await _hotelService.GetAsync(
            hotelId,
            cancellationToken));
    }

    [HttpPut("{hotelId:guid}")]
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
    [ProducesResponseType(StatusCodes.Status204NoContent)]
    public async Task<IActionResult> Delete(
        Guid hotelId,
        CancellationToken cancellationToken)
    {
        await _hotelService.DeleteAsync(hotelId, cancellationToken);
        return NoContent();
    }
}
