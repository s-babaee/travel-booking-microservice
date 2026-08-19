using Auth.Api.Application.Abstractions;
using Auth.Api.Application.Contracts;
using Auth.Api.Application.Exceptions;
using Auth.Api.Domain.Entities;
using Auth.Api.Domain.Enums;

namespace Auth.Api.Application.Services;

public sealed class AuthService : IAuthService
{
    private readonly IIdentityProvider _identityProvider;
    private readonly IUserRepository _users;
    private readonly IUnitOfWork _unitOfWork;
    private readonly TimeProvider _timeProvider;

    public AuthService(
        IIdentityProvider identityProvider,
        IUserRepository users,
        IUnitOfWork unitOfWork,
        TimeProvider timeProvider)
    {
        _identityProvider = identityProvider;
        _users = users;
        _unitOfWork = unitOfWork;
        _timeProvider = timeProvider;
    }

    public async Task<UserResponse> RegisterAsync(
    RegisterCommand command,
    CancellationToken cancellationToken)
    {
        if (command.Password.Length < 8)
        {
            throw new ValidationException(
                "Password must contain at least 8 characters.");
        }

        var normalizedEmail = command.Email.Trim().ToLowerInvariant();

        var existingUser = await _users.GetByEmailAsync(
            normalizedEmail,
            cancellationToken);

        if (existingUser is not null)
        {
            throw new ConflictException(
                "A user with this email already exists.");
        }

        var externalUser = await _identityProvider.CreateUserAsync(
            command,
            cancellationToken);

        var now = _timeProvider.GetUtcNow().UtcDateTime;

        var user = User.Create(
            externalUser.UserId,
            externalUser.Username,
            externalUser.Email,
            externalUser.FirstName,
            externalUser.LastName,
            now);

        if (!externalUser.Enabled)
        {
            user.SetStatus(UserStatus.Inactive, now);
        }

        await _users.AddAsync(user, cancellationToken);
        await _unitOfWork.SaveChangesAsync(cancellationToken);

        return user.ToResponse();
    }


    public async Task<AuthTokenResponse> LoginAsync(
        LoginCommand command,
        CancellationToken cancellationToken)
    {
        var tokens = await _identityProvider.LoginAsync(command, cancellationToken);
        var localUser = await _users.GetByIdAsync(tokens.UserId, cancellationToken);
        if (localUser is null)
        {
            var externalUser = await _identityProvider.GetUserAsync(
                tokens.UserId,
                cancellationToken);
            if (externalUser is not null)
            {
                var now = _timeProvider.GetUtcNow().UtcDateTime;
                var user = User.Create(
                    externalUser.UserId,
                    externalUser.Username,
                    externalUser.Email,
                    externalUser.FirstName,
                    externalUser.LastName,
                    now);
                if (!externalUser.Enabled)
                {
                    user.SetStatus(UserStatus.Inactive, now);
                }

                await _users.AddAsync(user, cancellationToken);
                await _unitOfWork.SaveChangesAsync(cancellationToken);
            }
        }

        return tokens;
    }

    public Task<AuthTokenResponse> RefreshTokenAsync(
        RefreshTokenCommand command,
        CancellationToken cancellationToken)
    {
        return _identityProvider.RefreshTokenAsync(command, cancellationToken);
    }

    public Task LogoutAsync(LogoutCommand command, CancellationToken cancellationToken)
    {
        return _identityProvider.LogoutAsync(command, cancellationToken);
    }

    public Task<TokenValidationResponse> ValidateTokenAsync(
        ValidateTokenCommand command,
        CancellationToken cancellationToken)
    {
        return _identityProvider.ValidateTokenAsync(command, cancellationToken);
    }
}
