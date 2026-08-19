using Auth.Api.Application.Abstractions;
using Auth.Api.Application.Contracts;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace Auth.Api.Controllers;

[ApiController]
[Authorize(Policy = "admin")]
[Route("api")]
public sealed class PermissionController : ControllerBase
{
    private readonly IPermissionService _permissionService;

    public PermissionController(IPermissionService permissionService)
    {
        _permissionService = permissionService;
    }

    [HttpPost("permissions")]
    public async Task<ActionResult<PermissionResponse>> Create(
        CreatePermissionCommand command,
        CancellationToken cancellationToken)
    {
        var permission = await _permissionService.CreateAsync(command, cancellationToken);
        return Created($"/api/permissions/{permission.PermissionId}", permission);
    }

    [HttpGet("permissions")]
    public Task<IReadOnlyList<PermissionResponse>> List(CancellationToken cancellationToken)
    {
        return _permissionService.ListAsync(cancellationToken);
    }

    [HttpPut("permissions/{permissionId:guid}")]
    public Task<PermissionResponse> Update(
        Guid permissionId,
        UpdatePermissionCommand command,
        CancellationToken cancellationToken)
    {
        return _permissionService.UpdateAsync(permissionId, command, cancellationToken);
    }

    [HttpDelete("permissions/{permissionId:guid}")]
    public async Task<IActionResult> Delete(Guid permissionId, CancellationToken cancellationToken)
    {
        await _permissionService.DeleteAsync(permissionId, cancellationToken);
        return NoContent();
    }

    [HttpPost("roles/{roleId:guid}/permissions/{permissionId:guid}")]
    public async Task<IActionResult> AssignToRole(
        Guid roleId,
        Guid permissionId,
        CancellationToken cancellationToken)
    {
        await _permissionService.AssignToRoleAsync(roleId, permissionId, cancellationToken);
        return NoContent();
    }

    [HttpDelete("roles/{roleId:guid}/permissions/{permissionId:guid}")]
    public async Task<IActionResult> RemoveFromRole(
        Guid roleId,
        Guid permissionId,
        CancellationToken cancellationToken)
    {
        await _permissionService.RemoveFromRoleAsync(roleId, permissionId, cancellationToken);
        return NoContent();
    }
}
