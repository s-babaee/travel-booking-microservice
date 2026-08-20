using Flight.Api.Application.Abstractions;
using Flight.Api.Application.Contracts;
using Microsoft.AspNetCore.Mvc;

namespace Flight.Api.Controllers;

[ApiController]
[Route("api/flights")]
public sealed class FlightController : ControllerBase
{
    private readonly IFlightService _service;

    public FlightController(IFlightService service)
    {
        _service = service;
    }

    [HttpPost]
    [ProducesResponseType(typeof(FlightResponse), StatusCodes.Status201Created)]
    public async Task<ActionResult<FlightResponse>> Create(
        [FromBody] CreateFlightRequest request,
        CancellationToken cancellationToken)
    {
        var response = await _service.CreateAsync(request, cancellationToken);
        return CreatedAtAction(nameof(GetById), new { flightId = response.Id }, response);
    }

    [HttpGet("{flightId:guid}")]
    [ProducesResponseType(typeof(FlightResponse), StatusCodes.Status200OK)]
    public async Task<ActionResult<FlightResponse>> GetById(
        Guid flightId,
        CancellationToken cancellationToken) =>
        Ok(await _service.GetAsync(flightId, cancellationToken));

    [HttpPut("{flightId:guid}")]
    [ProducesResponseType(typeof(FlightResponse), StatusCodes.Status200OK)]
    public async Task<ActionResult<FlightResponse>> Update(
        Guid flightId,
        [FromBody] UpdateFlightRequest request,
        CancellationToken cancellationToken) =>
        Ok(await _service.UpdateAsync(flightId, request, cancellationToken));

    [HttpPatch("{flightId:guid}/status")]
    [ProducesResponseType(typeof(FlightResponse), StatusCodes.Status200OK)]
    public async Task<ActionResult<FlightResponse>> ChangeStatus(
        Guid flightId,
        [FromBody] ChangeFlightStatusRequest request,
        CancellationToken cancellationToken) =>
        Ok(await _service.ChangeStatusAsync(flightId, request, cancellationToken));

    [HttpDelete("{flightId:guid}")]
    [ProducesResponseType(StatusCodes.Status204NoContent)]
    public async Task<IActionResult> Delete(
        Guid flightId,
        CancellationToken cancellationToken)
    {
        await _service.DeleteAsync(flightId, cancellationToken);
        return NoContent();
    }
}
