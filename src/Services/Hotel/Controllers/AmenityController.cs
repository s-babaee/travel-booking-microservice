using Hotel.Api.Application.Abstractions;
using Hotel.Api.Application.Contracts;
using BuildingBlocks.Authorization;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace Hotel.Api.Controllers;

[ApiController]
[Authorize]
[Route("api")]
public sealed class AmenityController : ControllerBase
{
    private readonly IAmenityService _amenityService;

    public AmenityController(IAmenityService amenityService)
    {
        _amenityService = amenityService;
    }

    [HttpPost("amenities")]
    [HasPermission(PermissionCatalog.HotelsCreate)]
    [ProducesResponseType(typeof(AmenityResponse), StatusCodes.Status201Created)]
    public async Task<ActionResult<AmenityResponse>> Create(
        [FromBody] CreateAmenityRequest request,
        CancellationToken cancellationToken)
    {
        var response = await _amenityService.CreateAsync(
            request,
            cancellationToken);
        return Created($"api/amenities/{response.Id}", response);
    }



    [HttpGet("amenities")]
    [HasPermission(PermissionCatalog.HotelsView)]
    [ProducesResponseType(typeof(IReadOnlyList<AmenityResponse>), StatusCodes.Status200OK)]
    public async Task<ActionResult<IReadOnlyList<AmenityResponse>>> List(
        CancellationToken cancellationToken)
    {
        return Ok(await _amenityService.ListAsync(cancellationToken));
    }



    [HttpPut("amenities/{amenityId:guid}")]
    [HasPermission(PermissionCatalog.HotelsUpdate)]
    [ProducesResponseType(typeof(AmenityResponse), StatusCodes.Status200OK)]
    public async Task<ActionResult<AmenityResponse>> Update(
        Guid amenityId,
        [FromBody] UpdateAmenityRequest request,
        CancellationToken cancellationToken)
    {
        return Ok(await _amenityService.UpdateAsync(
            amenityId,
            request,
            cancellationToken));
    }



    [HttpDelete("amenities/{amenityId:guid}")]
    [HasPermission(PermissionCatalog.HotelsDelete)]
    [ProducesResponseType(StatusCodes.Status204NoContent)]
    public async Task<IActionResult> Delete(
        Guid amenityId,
        CancellationToken cancellationToken)
    {
        await _amenityService.DeleteAsync(amenityId, cancellationToken);
        return NoContent();
    }



    [HttpPost("hotels/{hotelId:guid}/amenities/{amenityId:guid}")]
    [HasPermission(PermissionCatalog.HotelsUpdate)]
    [ProducesResponseType(StatusCodes.Status204NoContent)]
    public async Task<IActionResult> AssignToHotel(
        Guid hotelId,
        Guid amenityId,
        CancellationToken cancellationToken)
    {
        await _amenityService.AssignToHotelAsync(
            hotelId,
            amenityId,
            cancellationToken);
        return NoContent();
    }



    [HttpDelete("hotels/{hotelId:guid}/amenities/{amenityId:guid}")]
    [HasPermission(PermissionCatalog.HotelsUpdate)]
    [ProducesResponseType(StatusCodes.Status204NoContent)]
    public async Task<IActionResult> RemoveFromHotel(
        Guid hotelId,
        Guid amenityId,
        CancellationToken cancellationToken)
    {
        await _amenityService.RemoveFromHotelAsync(
            hotelId,
            amenityId,
            cancellationToken);
        return NoContent();
    }



    [HttpPost("room-types/{roomTypeId:guid}/amenities/{amenityId:guid}")]
    [HasPermission(PermissionCatalog.HotelsUpdate)]
    [ProducesResponseType(StatusCodes.Status204NoContent)]
    public async Task<IActionResult> AssignToRoomType(
        Guid roomTypeId,
        Guid amenityId,
        CancellationToken cancellationToken)
    {
        await _amenityService.AssignToRoomTypeAsync(
            roomTypeId,
            amenityId,
            cancellationToken);
        return NoContent();
    }



    [HttpDelete("room-types/{roomTypeId:guid}/amenities/{amenityId:guid}")]
    [HasPermission(PermissionCatalog.HotelsUpdate)]
    [ProducesResponseType(StatusCodes.Status204NoContent)]
    public async Task<IActionResult> RemoveFromRoomType(
        Guid roomTypeId,
        Guid amenityId,
        CancellationToken cancellationToken)
    {
        await _amenityService.RemoveFromRoomTypeAsync(
            roomTypeId,
            amenityId,
            cancellationToken);
        return NoContent();
    }
}
