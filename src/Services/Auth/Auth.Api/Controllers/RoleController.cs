using Auth.Api.Application.Abstractions;
using Auth.Api.Application.Contracts;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace Auth.Api.Controllers;

[ApiController]
[Authorize(Policy = "admin")]
[Route("api")]
public sealed class RoleController : ControllerBase
{
    private readonly IRoleService _roleService;

    public RoleController(IRoleService roleService)
    {
        _roleService = roleService;
    }

    [HttpPost("roles")]
    public async Task<ActionResult<RoleResponse>> Create(
        CreateRoleCommand command,
        CancellationToken cancellationToken)
    {
        var role = await _roleService.CreateAsync(command, cancellationToken);
        return Created($"/api/roles/{role.RoleId}", role);
    }

    [HttpGet("roles")]
    public Task<IReadOnlyList<RoleResponse>> List(CancellationToken cancellationToken)
    {
        return _roleService.ListAsync(cancellationToken);
    }

    [HttpPut("roles/{roleId:guid}")]
    public Task<RoleResponse> Update(
        Guid roleId,
        UpdateRoleCommand command,
        CancellationToken cancellationToken)
    {
        return _roleService.UpdateAsync(roleId, command, cancellationToken);
    }

    [HttpDelete("roles/{roleId:guid}")]
    public async Task<IActionResult> Delete(Guid roleId, CancellationToken cancellationToken)
    {
        await _roleService.DeleteAsync(roleId, cancellationToken);
        return NoContent();
    }

    [HttpPost("users/{userId:guid}/roles/{roleId:guid}")]
    public async Task<IActionResult> AssignToUser(
        Guid userId,
        Guid roleId,
        CancellationToken cancellationToken)
    {
        await _roleService.AssignToUserAsync(userId, roleId, cancellationToken);
        return NoContent();
    }

    [HttpDelete("users/{userId:guid}/roles/{roleId:guid}")]
    public async Task<IActionResult> RemoveFromUser(
        Guid userId,
        Guid roleId,
        CancellationToken cancellationToken)
    {
        await _roleService.RemoveFromUserAsync(userId, roleId, cancellationToken);
        return NoContent();
    }
}
