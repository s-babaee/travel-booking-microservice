using Hotel.Api.Application.Abstractions;
using Hotel.Api.Application.Contracts;
using Microsoft.AspNetCore.Mvc;

namespace Hotel.Api.Controllers;

[ApiController]
[Route("api")]
public sealed class RoomTypeController : ControllerBase
{
    private readonly IRoomTypeService _roomTypeService;

    public RoomTypeController(IRoomTypeService roomTypeService)
    {
        _roomTypeService = roomTypeService;
    }

    [HttpPost("hotels/{hotelId:guid}/room-types")]
    [ProducesResponseType(typeof(RoomTypeResponse), StatusCodes.Status201Created)]
    public async Task<ActionResult<RoomTypeResponse>> Create(
        Guid hotelId,
        [FromBody] CreateRoomTypeRequest request,
        CancellationToken cancellationToken)
    {
        var response = await _roomTypeService.CreateAsync(
            hotelId,
            request,
            cancellationToken);

        return CreatedAtAction(
            nameof(GetById),
            new { roomTypeId = response.Id },
            response);
    }

    [HttpGet("hotels/{hotelId:guid}/room-types")]
    [ProducesResponseType(typeof(IReadOnlyList<RoomTypeResponse>), StatusCodes.Status200OK)]
    public async Task<ActionResult<IReadOnlyList<RoomTypeResponse>>> ListByHotel(
        Guid hotelId,
        CancellationToken cancellationToken)
    {
        return Ok(await _roomTypeService.ListByHotelAsync(
            hotelId,
            cancellationToken));
    }

    [HttpGet("room-types/{roomTypeId:guid}")]
    [ProducesResponseType(typeof(RoomTypeResponse), StatusCodes.Status200OK)]
    public async Task<ActionResult<RoomTypeResponse>> GetById(
        Guid roomTypeId,
        CancellationToken cancellationToken)
    {
        return Ok(await _roomTypeService.GetAsync(
            roomTypeId,
            cancellationToken));
    }

    [HttpPut("room-types/{roomTypeId:guid}")]
    [ProducesResponseType(typeof(RoomTypeResponse), StatusCodes.Status200OK)]
    public async Task<ActionResult<RoomTypeResponse>> Update(
        Guid roomTypeId,
        [FromBody] UpdateRoomTypeRequest request,
        CancellationToken cancellationToken)
    {
        return Ok(await _roomTypeService.UpdateAsync(
            roomTypeId,
            request,
            cancellationToken));
    }

    [HttpPatch("room-types/{roomTypeId:guid}/status")]
    [ProducesResponseType(typeof(RoomTypeResponse), StatusCodes.Status200OK)]
    public async Task<ActionResult<RoomTypeResponse>> ChangeStatus(
        Guid roomTypeId,
        [FromBody] ChangeRoomTypeStatusRequest request,
        CancellationToken cancellationToken)
    {
        return Ok(await _roomTypeService.ChangeStatusAsync(
            roomTypeId,
            request,
            cancellationToken));
    }

    [HttpDelete("room-types/{roomTypeId:guid}")]
    [ProducesResponseType(StatusCodes.Status204NoContent)]
    public async Task<IActionResult> Delete(
        Guid roomTypeId,
        CancellationToken cancellationToken)
    {
        await _roomTypeService.DeleteAsync(roomTypeId, cancellationToken);
        return NoContent();
    }
}
