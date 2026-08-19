using Auth.Api.Application.Abstractions;
using Auth.Api.Application.Contracts;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace Auth.Api.Controllers;

[ApiController]
[Authorize(Policy = "admin")]
[Route("api/users")]
public sealed class UserController : ControllerBase
{
    private readonly IUserService _userService;

    public UserController(IUserService userService)
    {
        _userService = userService;
    }

    [HttpGet("{userId:guid}")]
    public async Task<ActionResult<UserResponse>> Get(
        Guid userId,
        CancellationToken cancellationToken)
    {
        var user = await _userService.GetAsync(userId, cancellationToken);
        return user is null ? NotFound() : Ok(user);
    }

    [HttpPut("{userId:guid}")]
    public Task<UserResponse> Update(
        Guid userId,
        UpdateUserCommand command,
        CancellationToken cancellationToken)
    {
        return _userService.UpdateAsync(userId, command, cancellationToken);
    }

    [HttpPatch("{userId:guid}/status")]
    public Task<UserResponse> ChangeStatus(
        Guid userId,
        ChangeUserStatusCommand command,
        CancellationToken cancellationToken)
    {
        return _userService.ChangeStatusAsync(userId, command, cancellationToken);
    }

    [HttpDelete("{userId:guid}")]
    public async Task<IActionResult> Delete(Guid userId, CancellationToken cancellationToken)
    {
        await _userService.DeleteAsync(userId, cancellationToken);
        return NoContent();
    }
}
