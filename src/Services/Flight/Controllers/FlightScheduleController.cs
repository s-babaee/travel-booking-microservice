using Flight.Api.Application.Abstractions;
using Flight.Api.Application.Contracts;
using BuildingBlocks.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace Flight.Api.Controllers;

[ApiController]
[Route("api")]
public sealed class FlightScheduleController : ControllerBase
{
    private readonly IFlightScheduleService _service;

    public FlightScheduleController(IFlightScheduleService service)
    {
        _service = service;
    }

    [HttpPost("flights/{flightId:guid}/schedules")]
    [HasPermission(PermissionCatalog.FlightsCreate)]
    [ProducesResponseType(typeof(FlightScheduleResponse), StatusCodes.Status201Created)]
    public async Task<ActionResult<FlightScheduleResponse>> Create(
        Guid flightId,
        [FromBody] CreateFlightScheduleRequest request,
        CancellationToken cancellationToken)
    {
        var response = await _service.CreateAsync(flightId, request, cancellationToken);
        return CreatedAtAction(nameof(GetById), new { scheduleId = response.Id }, response);
    }



    [HttpGet("flights/{flightId:guid}/schedules")]
    [HasPermission(PermissionCatalog.FlightsView)]
    [ProducesResponseType(typeof(IReadOnlyList<FlightScheduleResponse>), StatusCodes.Status200OK)]
    public async Task<ActionResult<IReadOnlyList<FlightScheduleResponse>>> ListByFlight(
        Guid flightId,
        CancellationToken cancellationToken) =>
        Ok(await _service.ListByFlightAsync(flightId, cancellationToken));



    [HttpGet("schedules/{scheduleId:guid}")]
    [HasPermission(PermissionCatalog.FlightsView)]
    [ProducesResponseType(typeof(FlightScheduleResponse), StatusCodes.Status200OK)]
    public async Task<ActionResult<FlightScheduleResponse>> GetById(
        Guid scheduleId,
        CancellationToken cancellationToken) =>
        Ok(await _service.GetAsync(scheduleId, cancellationToken));



    [HttpPut("schedules/{scheduleId:guid}")]
    [HasPermission(PermissionCatalog.FlightsUpdate)]
    [ProducesResponseType(typeof(FlightScheduleResponse), StatusCodes.Status200OK)]
    public async Task<ActionResult<FlightScheduleResponse>> Update(
        Guid scheduleId,
        [FromBody] UpdateFlightScheduleRequest request,
        CancellationToken cancellationToken) =>
        Ok(await _service.UpdateAsync(scheduleId, request, cancellationToken));



    [HttpDelete("schedules/{scheduleId:guid}")]
    [HasPermission(PermissionCatalog.FlightsDelete)]
    [ProducesResponseType(StatusCodes.Status204NoContent)]
    public async Task<IActionResult> Delete(
        Guid scheduleId,
        CancellationToken cancellationToken)
    {
        await _service.DeleteAsync(scheduleId, cancellationToken);
        return NoContent();
    }
}
