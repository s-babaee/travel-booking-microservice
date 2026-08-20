using Flight.Api.Application.Abstractions;
using Flight.Api.Application.Contracts;
using Microsoft.AspNetCore.Mvc;

namespace Flight.Api.Controllers;

[ApiController]
[Route("api/airlines")]
public sealed class AirlineController : ControllerBase
{
    private readonly IAirlineService _service;

    public AirlineController(IAirlineService service)
    {
        _service = service;
    }

    [HttpPost]
    [ProducesResponseType(typeof(AirlineResponse), StatusCodes.Status201Created)]
    public async Task<ActionResult<AirlineResponse>> Create(
        [FromBody] CreateAirlineRequest request,
        CancellationToken cancellationToken)
    {
        var response = await _service.CreateAsync(request, cancellationToken);
        return CreatedAtAction(nameof(GetById), new { airlineId = response.Id }, response);
    }

    [HttpGet("{airlineId:guid}")]
    [ProducesResponseType(typeof(AirlineResponse), StatusCodes.Status200OK)]
    public async Task<ActionResult<AirlineResponse>> GetById(
        Guid airlineId,
        CancellationToken cancellationToken) =>
        Ok(await _service.GetAsync(airlineId, cancellationToken));

    [HttpGet]
    [ProducesResponseType(typeof(IReadOnlyList<AirlineResponse>), StatusCodes.Status200OK)]
    public async Task<ActionResult<IReadOnlyList<AirlineResponse>>> List(
        CancellationToken cancellationToken) =>
        Ok(await _service.ListAsync(cancellationToken));

    [HttpPut("{airlineId:guid}")]
    [ProducesResponseType(typeof(AirlineResponse), StatusCodes.Status200OK)]
    public async Task<ActionResult<AirlineResponse>> Update(
        Guid airlineId,
        [FromBody] UpdateAirlineRequest request,
        CancellationToken cancellationToken) =>
        Ok(await _service.UpdateAsync(airlineId, request, cancellationToken));

    [HttpDelete("{airlineId:guid}")]
    [ProducesResponseType(StatusCodes.Status204NoContent)]
    public async Task<IActionResult> Delete(
        Guid airlineId,
        CancellationToken cancellationToken)
    {
        await _service.DeleteAsync(airlineId, cancellationToken);
        return NoContent();
    }
}
