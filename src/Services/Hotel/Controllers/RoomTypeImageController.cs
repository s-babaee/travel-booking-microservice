using Hotel.Api.Application.Abstractions;
using Hotel.Api.Application.Contracts;
using Microsoft.AspNetCore.Mvc;

namespace Hotel.Api.Controllers;

[ApiController]
[Route("api/room-types/{roomTypeId:guid}/images")]
public sealed class RoomTypeImageController : ControllerBase
{
    private readonly IHotelImageService _imageService;

    public RoomTypeImageController(IHotelImageService imageService)
    {
        _imageService = imageService;
    }

    [HttpPost]
    [Consumes("multipart/form-data")]
    [ProducesResponseType(typeof(RoomTypeImageResponse), StatusCodes.Status201Created)]
    public async Task<ActionResult<RoomTypeImageResponse>> Add(
        Guid roomTypeId,
        [FromForm] AddRoomTypeImageRequest request,
        CancellationToken cancellationToken)
    {
        var response = await _imageService.AddToRoomTypeAsync(
            roomTypeId,
            request,
            cancellationToken);

        return CreatedAtAction(
            nameof(List),
            new { roomTypeId },
            response);
    }

    [HttpGet]
    [ProducesResponseType(typeof(IReadOnlyList<RoomTypeImageResponse>), StatusCodes.Status200OK)]
    public async Task<ActionResult<IReadOnlyList<RoomTypeImageResponse>>> List(
        Guid roomTypeId,
        CancellationToken cancellationToken)
    {
        return Ok(await _imageService.ListRoomTypeImagesAsync(
            roomTypeId,
            cancellationToken));
    }

    [HttpDelete("{imageId:guid}")]
    [ProducesResponseType(StatusCodes.Status204NoContent)]
    public async Task<IActionResult> Delete(
        Guid roomTypeId,
        Guid imageId,
        CancellationToken cancellationToken)
    {
        await _imageService.DeleteFromRoomTypeAsync(
            roomTypeId,
            imageId,
            cancellationToken);
        return NoContent();
    }
}
