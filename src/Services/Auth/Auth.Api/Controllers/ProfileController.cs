using Auth.Api.Application.Abstractions;
using Auth.Api.Application.Contracts;
using Auth.Api.Infrastructure.Web;
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
    private readonly IPasswordService _passwordService;

    public ProfileController(
        IUserService userService,
        IRoleService roleService,
        IPasswordService passwordService)
    {
        _userService = userService;
        _roleService = roleService;
        _passwordService = passwordService;
    }

    [HttpGet("me")]
    public async Task<ActionResult<UserResponse>> Me(CancellationToken cancellationToken)
    {
        var user = await _userService.GetAsync(
            User.GetRequiredUserId(),
            cancellationToken);
        return user is null ? NotFound() : Ok(user);
    }

    [HttpPut("me")]
    public Task<UserResponse> Update(
        UpdateUserCommand command,
        CancellationToken cancellationToken)
    {
        return _userService.UpdateAsync(
            User.GetRequiredUserId(),
            command,
            cancellationToken);
    }

    [HttpGet("me/roles")]
    public Task<IReadOnlyList<RoleResponse>> Roles(CancellationToken cancellationToken)
    {
        return _roleService.GetUserRolesAsync(
            User.GetRequiredUserId(),
            cancellationToken);
    }

    [HttpPatch("me/password")]
    public async Task<IActionResult> ChangePassword(
        ChangePasswordCommand command,
        CancellationToken cancellationToken)
    {
        await _passwordService.ChangeAsync(
            User.GetRequiredUserId(),
            command,
            cancellationToken);
        return NoContent();
    }
}
