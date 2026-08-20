using Flight.Api.Application.Abstractions;
using Flight.Api.Application.Contracts;
using Microsoft.AspNetCore.Mvc;

namespace Flight.Api.Controllers;

[ApiController]
[Route("api/routes")]
public sealed class RouteController : ControllerBase
{
    private readonly IRouteService _service;

    public RouteController(IRouteService service)
    {
        _service = service;
    }

    [HttpPost]
    [ProducesResponseType(typeof(RouteResponse), StatusCodes.Status201Created)]
    public async Task<ActionResult<RouteResponse>> Create(
        [FromBody] CreateRouteRequest request,
        CancellationToken cancellationToken)
    {
        var response = await _service.CreateAsync(request, cancellationToken);
        return CreatedAtAction(nameof(GetById), new { routeId = response.Id }, response);
    }

    [HttpGet("{routeId:guid}")]
    [ProducesResponseType(typeof(RouteResponse), StatusCodes.Status200OK)]
    public async Task<ActionResult<RouteResponse>> GetById(
        Guid routeId,
        CancellationToken cancellationToken) =>
        Ok(await _service.GetAsync(routeId, cancellationToken));

    [HttpGet]
    [ProducesResponseType(typeof(IReadOnlyList<RouteResponse>), StatusCodes.Status200OK)]
    public async Task<ActionResult<IReadOnlyList<RouteResponse>>> List(
        CancellationToken cancellationToken) =>
        Ok(await _service.ListAsync(cancellationToken));

    [HttpPut("{routeId:guid}")]
    [ProducesResponseType(typeof(RouteResponse), StatusCodes.Status200OK)]
    public async Task<ActionResult<RouteResponse>> Update(
        Guid routeId,
        [FromBody] UpdateRouteRequest request,
        CancellationToken cancellationToken) =>
        Ok(await _service.UpdateAsync(routeId, request, cancellationToken));

    [HttpDelete("{routeId:guid}")]
    [ProducesResponseType(StatusCodes.Status204NoContent)]
    public async Task<IActionResult> Delete(
        Guid routeId,
        CancellationToken cancellationToken)
    {
        await _service.DeleteAsync(routeId, cancellationToken);
        return NoContent();
    }
}
