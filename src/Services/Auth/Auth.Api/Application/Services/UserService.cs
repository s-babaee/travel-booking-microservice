using Auth.Api.Application.Abstractions;
using Auth.Api.Application.Contracts;
using Auth.Api.Application.Exceptions;
using Auth.Api.Domain.Enums;

namespace Auth.Api.Application.Services;

public sealed class UserService : IUserService
{
    private readonly IUserRepository _users;
    private readonly IIdentityProvider _identityProvider;
    private readonly IUnitOfWork _unitOfWork;
    private readonly TimeProvider _timeProvider;

    public UserService(
        IUserRepository users,
        IIdentityProvider identityProvider,
        IUnitOfWork unitOfWork,
        TimeProvider timeProvider)
    {
        _users = users;
        _identityProvider = identityProvider;
        _unitOfWork = unitOfWork;
        _timeProvider = timeProvider;
    }

    public async Task<UserResponse?> GetAsync(Guid userId, CancellationToken cancellationToken)
    {
        var user = await _users.GetByIdAsync(userId, cancellationToken);
        return user?.ToResponse();
    }

    public async Task<UserResponse> UpdateAsync(
        Guid userId,
        UpdateUserCommand command,
        CancellationToken cancellationToken)
    {
        var user = await GetRequiredUserAsync(userId, cancellationToken);
        var existingByEmail = await _users.GetByEmailAsync(command.Email, cancellationToken);
        if (existingByEmail is not null && existingByEmail.Id != userId)
        {
            throw new ConflictException("A user with this email already exists.");
        }

        await _identityProvider.UpdateUserAsync(userId, command, cancellationToken);
        user.UpdateProfile(
            command.Email,
            command.FirstName,
            command.LastName,
            _timeProvider.GetUtcNow().UtcDateTime);
        await _unitOfWork.SaveChangesAsync(cancellationToken);

        return user.ToResponse();
    }

    public async Task<UserResponse> ChangeStatusAsync(
        Guid userId,
        ChangeUserStatusCommand command,
        CancellationToken cancellationToken)
    {
        var user = await GetRequiredUserAsync(userId, cancellationToken);
        await _identityProvider.SetUserEnabledAsync(
            userId,
            command.Status == UserStatus.Active,
            cancellationToken);

        user.SetStatus(command.Status, _timeProvider.GetUtcNow().UtcDateTime);
        await _unitOfWork.SaveChangesAsync(cancellationToken);
        return user.ToResponse();
    }

    public async Task DeleteAsync(Guid userId, CancellationToken cancellationToken)
    {
        var user = await GetRequiredUserAsync(userId, cancellationToken);
        await _identityProvider.SetUserEnabledAsync(userId, false, cancellationToken);
        user.SoftDelete(_timeProvider.GetUtcNow().UtcDateTime);
        await _unitOfWork.SaveChangesAsync(cancellationToken);
    }

    private async Task<Domain.Entities.User> GetRequiredUserAsync(
        Guid userId,
        CancellationToken cancellationToken)
    {
        return await _users.GetByIdAsync(userId, cancellationToken)
            ?? throw new NotFoundException($"User '{userId}' was not found.");
    }
}
