using Auth.Api.Application.Abstractions;
using Auth.Api.Application.Contracts;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace Auth.Api.Controllers;

[ApiController]
[Route("api/auth")]
public sealed class AuthController : ControllerBase
{
    private readonly IAuthService _authService;

    public AuthController(IAuthService authService)
    {
        _authService = authService;
    }

    [AllowAnonymous]
    [HttpPost("register")]
    public async Task<ActionResult<UserResponse>> Register(
        RegisterCommand command,
        CancellationToken cancellationToken)
    {
        var user = await _authService.RegisterAsync(command, cancellationToken);
        return Created($"/api/users/{user.UserId}", user);
    }


    [AllowAnonymous]
    [HttpPost("login")]
    public Task<AuthTokenResponse> Login(
        LoginCommand command,
        CancellationToken cancellationToken)
    {
        return _authService.LoginAsync(command, cancellationToken);
    }

    [AllowAnonymous]
    [HttpPost("refresh-token")]
    public Task<AuthTokenResponse> RefreshToken(
        RefreshTokenCommand command,
        CancellationToken cancellationToken)
    {
        return _authService.RefreshTokenAsync(command, cancellationToken);
    }

    [AllowAnonymous]
    [HttpPost("logout")]
    public async Task<IActionResult> Logout(
        LogoutCommand command,
        CancellationToken cancellationToken)
    {
        await _authService.LogoutAsync(command, cancellationToken);
        return NoContent();
    }

    [AllowAnonymous]
    [HttpPost("validate-token")]
    public Task<TokenValidationResponse> ValidateToken(
        ValidateTokenCommand command,
        CancellationToken cancellationToken)
    {
        return _authService.ValidateTokenAsync(command, cancellationToken);
    }
}
