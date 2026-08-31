using Auth.Api.Application.Abstractions;
using Auth.Api.Application.Contracts;
using BuildingBlocks.Authorization;
using Auth.Api.Infrastructure.Web;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace Auth.Api.Controllers;

[ApiController]
[Route("api/password")]
public sealed class PasswordController : ControllerBase
{
    private readonly IPasswordService _passwordService;

    public PasswordController(IPasswordService passwordService)
    {
        _passwordService = passwordService;
    }

    /*
    [AllowAnonymous]
    [HttpPost("forgot")]
    public Task<PasswordResetResponse> Forgot(
        ForgotPasswordCommand command,
        CancellationToken cancellationToken)
    {
        return _passwordService.ForgotAsync(command, cancellationToken);
    }
    */

    /*
    [AllowAnonymous]
    [HttpPost("reset")]
    public async Task<IActionResult> Reset(
        ResetPasswordCommand command,
        CancellationToken cancellationToken)
    {
        await _passwordService.ResetAsync(command, cancellationToken);
        return NoContent();
    }
    */

    [HasPermission(PermissionCatalog.ProfileUpdateOwn)]
    [HttpPost("change")]
    public async Task<IActionResult> Change(
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
