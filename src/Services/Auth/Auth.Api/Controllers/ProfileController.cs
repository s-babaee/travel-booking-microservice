using Auth.Api.Application.Abstractions;
using Auth.Api.Application.Contracts;
using Auth.Api.Infrastructure.Web;
using BuildingBlocks.Authorization;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace Auth.Api.Controllers;

[ApiController]
[Authorize]
[Route("api/profile")]
public sealed class ProfileController : ControllerBase
{
    private readonly IUserService _userService;
    private readonly IRoleService _roleService;

    public ProfileController(
        IUserService userService,
        IRoleService roleService,
        IPasswordService passwordService)
    {
        _userService = userService;
        _roleService = roleService;
    }

    [HttpGet("me")]
    [HasPermission(PermissionCatalog.ProfileReadOwn)]
    public async Task<ActionResult<UserResponse>> Me(CancellationToken cancellationToken)
    {
        var user = await _userService.GetAsync(
            User.GetRequiredUserId(),
            cancellationToken);
        return user is null ? NotFound() : Ok(user);
    }


    [HttpPut("me")]
    [HasPermission(PermissionCatalog.ProfileUpdateOwn)]
    public Task<UserResponse> Update(UpdateUserCommand command, CancellationToken cancellationToken)
    {
        return _userService.UpdateAsync(
            User.GetRequiredUserId(),
            command,
            cancellationToken);
    }


    [HttpGet("me/roles")]
    [HasPermission(PermissionCatalog.ProfileReadOwn)]
    public Task<IReadOnlyList<RoleResponse>> Roles(CancellationToken cancellationToken)
    {
        return _roleService.GetUserRolesAsync(
            User.GetRequiredUserId(),
            cancellationToken);
    }

}
