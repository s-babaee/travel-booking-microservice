using Flight.Api.Application.Abstractions;
using Flight.Api.Application.Contracts;
using BuildingBlocks.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace Flight.Api.Controllers;

[ApiController]
[Route("api")]
public sealed class FlightClassController : ControllerBase
{
    private readonly IFlightClassService _service;

    public FlightClassController(IFlightClassService service)
    {
        _service = service;
    }

    [HttpPost("flights/{flightId:guid}/classes")]
    [HasPermission(PermissionCatalog.FlightsCreate)]
    [ProducesResponseType(typeof(FlightClassResponse), StatusCodes.Status201Created)]
    public async Task<ActionResult<FlightClassResponse>> Create(
        Guid flightId,
        [FromBody] CreateFlightClassRequest request,
        CancellationToken cancellationToken)
    {
        var response = await _service.CreateAsync(flightId, request, cancellationToken);
        return CreatedAtAction(nameof(GetById), new { classId = response.Id }, response);
    }



    [HttpGet("flights/{flightId:guid}/classes")]
    [HasPermission(PermissionCatalog.FlightsView)]
    [ProducesResponseType(typeof(IReadOnlyList<FlightClassResponse>), StatusCodes.Status200OK)]
    public async Task<ActionResult<IReadOnlyList<FlightClassResponse>>> ListByFlight(
        Guid flightId,
        CancellationToken cancellationToken) =>
        Ok(await _service.ListByFlightAsync(flightId, cancellationToken));



    [HttpGet("classes/{classId:guid}")]
    [HasPermission(PermissionCatalog.FlightsView)]
    [ProducesResponseType(typeof(FlightClassResponse), StatusCodes.Status200OK)]
    public async Task<ActionResult<FlightClassResponse>> GetById(
        Guid classId,
        CancellationToken cancellationToken) =>
        Ok(await _service.GetAsync(classId, cancellationToken));



    [HttpPut("classes/{classId:guid}")]
    [HasPermission(PermissionCatalog.FlightsUpdate)]
    [ProducesResponseType(typeof(FlightClassResponse), StatusCodes.Status200OK)]
    public async Task<ActionResult<FlightClassResponse>> Update(
        Guid classId,
        [FromBody] UpdateFlightClassRequest request,
        CancellationToken cancellationToken) =>
        Ok(await _service.UpdateAsync(classId, request, cancellationToken));



    [HttpPatch("classes/{classId:guid}/status")]
    [HasPermission(PermissionCatalog.FlightsUpdate)]
    [ProducesResponseType(typeof(FlightClassResponse), StatusCodes.Status200OK)]
    public async Task<ActionResult<FlightClassResponse>> ChangeStatus(
        Guid classId,
        [FromBody] ChangeFlightClassStatusRequest request,
        CancellationToken cancellationToken) =>
        Ok(await _service.ChangeStatusAsync(classId, request, cancellationToken));



    [HttpDelete("classes/{classId:guid}")]
    [HasPermission(PermissionCatalog.FlightsDelete)]
    [ProducesResponseType(StatusCodes.Status204NoContent)]
    public async Task<IActionResult> Delete(
        Guid classId,
        CancellationToken cancellationToken)
    {
        await _service.DeleteAsync(classId, cancellationToken);
        return NoContent();
    }
}
