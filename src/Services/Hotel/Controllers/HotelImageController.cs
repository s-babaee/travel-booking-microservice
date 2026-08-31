using Hotel.Api.Application.Abstractions;
using Hotel.Api.Application.Contracts;
using BuildingBlocks.Authorization;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace Hotel.Api.Controllers;

[ApiController]
[Authorize]
[Route("api/hotels/{hotelId:guid}/images")]
public sealed class HotelImageController : ControllerBase
{
    private readonly IHotelImageService _imageService;

    public HotelImageController(IHotelImageService imageService)
    {
        _imageService = imageService;
    }

    [HttpPost]
    [HasPermission(PermissionCatalog.HotelsUpdate)]
    [Consumes("multipart/form-data")]
    [ProducesResponseType(typeof(HotelImageResponse), StatusCodes.Status201Created)]
    public async Task<ActionResult<HotelImageResponse>> Add(
        Guid hotelId,
        [FromForm] AddHotelImageRequest request,
        CancellationToken cancellationToken)
    {
        var response = await _imageService.AddToHotelAsync(
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
    [ProducesResponseType(typeof(IReadOnlyList<HotelImageResponse>), StatusCodes.Status200OK)]
    public async Task<ActionResult<IReadOnlyList<HotelImageResponse>>> List(
        Guid hotelId,
        CancellationToken cancellationToken)
    {
        return Ok(await _imageService.ListHotelImagesAsync(
            hotelId,
            cancellationToken));
    }



    [HttpDelete("{imageId:guid}")]
    [HasPermission(PermissionCatalog.HotelsUpdate)]
    [ProducesResponseType(StatusCodes.Status204NoContent)]
    public async Task<IActionResult> Delete(
        Guid hotelId,
        Guid imageId,
        CancellationToken cancellationToken)
    {
        await _imageService.DeleteFromHotelAsync(
            hotelId,
            imageId,
            cancellationToken);
        return NoContent();
    }
}
