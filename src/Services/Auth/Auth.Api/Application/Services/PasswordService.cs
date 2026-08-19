using System.Security.Cryptography;
using System.Text;
using Microsoft.AspNetCore.WebUtilities;
using Microsoft.Extensions.Options;
using Auth.Api.Application.Abstractions;
using Auth.Api.Application.Contracts;
using Auth.Api.Application.Exceptions;

namespace Auth.Api.Application.Services;

public sealed class AuthOptions
{
    public bool ExposeResetToken { get; set; } = false;
    public int ResetTokenLifetimeMinutes { get; set; } = 30;
}

public sealed class PasswordService : IPasswordService
{
    private readonly IUserRepository _users;
    private readonly IPasswordResetTokenRepository _tokens;
    private readonly IIdentityProvider _identityProvider;
    private readonly IUnitOfWork _unitOfWork;
    private readonly TimeProvider _timeProvider;
    private readonly AuthOptions _options;

    public PasswordService(
        IUserRepository users,
        IPasswordResetTokenRepository tokens,
        IIdentityProvider identityProvider,
        IUnitOfWork unitOfWork,
        TimeProvider timeProvider,
        IOptions<AuthOptions> options)
    {
        _users = users;
        _tokens = tokens;
        _identityProvider = identityProvider;
        _unitOfWork = unitOfWork;
        _timeProvider = timeProvider;
        _options = options.Value;
    }

    public async Task<PasswordResetResponse> ForgotAsync(
        ForgotPasswordCommand command,
        CancellationToken cancellationToken)
    {
        var user = await _users.GetByEmailAsync(command.Email, cancellationToken);
        if (user is null || user.IsDeleted)
        {
            return new PasswordResetResponse(
                "If the account exists, a password reset request has been created.",
                null);
        }

        var token = WebEncoders.Base64UrlEncode(RandomNumberGenerator.GetBytes(32));
        var now = _timeProvider.GetUtcNow().UtcDateTime;
        var resetToken = Domain.Entities.PasswordResetToken.Create(
            Guid.NewGuid(),
            user.Id,
            Hash(token),
            now.AddMinutes(Math.Max(5, _options.ResetTokenLifetimeMinutes)),
            now);

        await _tokens.AddAsync(resetToken, cancellationToken);
        await _unitOfWork.SaveChangesAsync(cancellationToken);

        return new PasswordResetResponse(
            "If the account exists, a password reset request has been created.",
            _options.ExposeResetToken ? token : null);
    }

    public async Task ResetAsync(
        ResetPasswordCommand command,
        CancellationToken cancellationToken)
    {
        ValidatePassword(command.NewPassword);
        var token = await _tokens.GetByHashAsync(Hash(command.Token), cancellationToken);
        var now = _timeProvider.GetUtcNow().UtcDateTime;
        if (token is null || !token.IsValid(now))
        {
            throw new ValidationException("The password reset token is invalid or expired.");
        }

        var user = await _users.GetByIdAsync(token.UserId, cancellationToken)
            ?? throw new NotFoundException("The user associated with the reset token was not found.");

        await _identityProvider.ChangePasswordAsync(user.Id, command.NewPassword, cancellationToken);
        token.MarkUsed(now);
        await _unitOfWork.SaveChangesAsync(cancellationToken);
    }

    public async Task ChangeAsync(
        Guid userId,
        ChangePasswordCommand command,
        CancellationToken cancellationToken)
    {
        ValidatePassword(command.NewPassword);
        var user = await _users.GetByIdAsync(userId, cancellationToken)
            ?? throw new NotFoundException($"User '{userId}' was not found.");

        if (!await _identityProvider.ValidateCredentialsAsync(
                user.Username,
                command.CurrentPassword,
                cancellationToken))
        {
            throw new UnauthorizedException("The current password is incorrect.");
        }

        await _identityProvider.ChangePasswordAsync(user.Id, command.NewPassword, cancellationToken);
    }

    private static void ValidatePassword(string password)
    {
        if (password.Length < 8)
        {
            throw new ValidationException("Password must contain at least 8 characters.");
        }
    }

    private static string Hash(string value)
    {
        return Convert.ToHexString(SHA256.HashData(Encoding.UTF8.GetBytes(value)));
    }
}
