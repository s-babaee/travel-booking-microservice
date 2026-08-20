using Flight.Api.Application.Abstractions;
using Flight.Api.Application.Contracts;
using Microsoft.AspNetCore.Mvc;

namespace Flight.Api.Controllers;

[ApiController]
[Route("api/flights/{flightId:guid}/policies")]
public sealed class FlightPolicyController : ControllerBase
{
    private readonly IFlightPolicyService _service;

    public FlightPolicyController(IFlightPolicyService service)
    {
        _service = service;
    }

    [HttpPost]
    [ProducesResponseType(typeof(FlightPolicyResponse), StatusCodes.Status201Created)]
    public async Task<ActionResult<FlightPolicyResponse>> Create(
        Guid flightId,
        [FromBody] CreateFlightPolicyRequest request,
        CancellationToken cancellationToken)
    {
        var response = await _service.CreateAsync(flightId, request, cancellationToken);
        return CreatedAtAction(nameof(List), new { flightId }, response);
    }

    [HttpGet]
    [ProducesResponseType(typeof(IReadOnlyList<FlightPolicyResponse>), StatusCodes.Status200OK)]
    public async Task<ActionResult<IReadOnlyList<FlightPolicyResponse>>> List(
        Guid flightId,
        CancellationToken cancellationToken) =>
        Ok(await _service.ListByFlightAsync(flightId, cancellationToken));

    [HttpPut("{policyId:guid}")]
    [ProducesResponseType(typeof(FlightPolicyResponse), StatusCodes.Status200OK)]
    public async Task<ActionResult<FlightPolicyResponse>> Update(
        Guid flightId,
        Guid policyId,
        [FromBody] UpdateFlightPolicyRequest request,
        CancellationToken cancellationToken) =>
        Ok(await _service.UpdateAsync(flightId, policyId, request, cancellationToken));

    [HttpDelete("{policyId:guid}")]
    [ProducesResponseType(StatusCodes.Status204NoContent)]
    public async Task<IActionResult> Delete(
        Guid flightId,
        Guid policyId,
        CancellationToken cancellationToken)
    {
        await _service.DeleteAsync(flightId, policyId, cancellationToken);
        return NoContent();
    }
}
