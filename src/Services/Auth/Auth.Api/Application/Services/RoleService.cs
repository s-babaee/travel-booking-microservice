using Auth.Api.Application.Abstractions;
using Auth.Api.Application.Contracts;
using Auth.Api.Application.Exceptions;
using Auth.Api.Domain.Entities;

namespace Auth.Api.Application.Services;

public sealed class RoleService : IRoleService
{
    private readonly IRoleRepository _roles;
    private readonly IUserRepository _users;
    private readonly IUserRoleRepository _userRoles;
    private readonly IIdentityProvider _identityProvider;
    private readonly IUnitOfWork _unitOfWork;
    private readonly TimeProvider _timeProvider;

    public RoleService(
        IRoleRepository roles,
        IUserRepository users,
        IUserRoleRepository userRoles,
        IIdentityProvider identityProvider,
        IUnitOfWork unitOfWork,
        TimeProvider timeProvider)
    {
        _roles = roles;
        _users = users;
        _userRoles = userRoles;
        _identityProvider = identityProvider;
        _unitOfWork = unitOfWork;
        _timeProvider = timeProvider;
    }

    public async Task<RoleResponse> CreateAsync(
        CreateRoleCommand command,
        CancellationToken cancellationToken)
    {
        var role = Role.Create(Guid.NewGuid(), command.Name, command.Description, UtcNow());
        await _identityProvider.CreateRoleAsync(role, cancellationToken);
        await _roles.AddAsync(role, cancellationToken);
        await _unitOfWork.SaveChangesAsync(cancellationToken);
        return role.ToResponse();
    }

    public async Task<IReadOnlyList<RoleResponse>> ListAsync(CancellationToken cancellationToken)
    {
        var roles = await _roles.ListAsync(cancellationToken);
        return roles.Select(role => role.ToResponse()).ToList();
    }

    public async Task<RoleResponse> UpdateAsync(
        Guid roleId,
        UpdateRoleCommand command,
        CancellationToken cancellationToken)
    {
        var role = await GetRequiredRoleAsync(roleId, cancellationToken);
        var previousName = role.Name;
        role.Update(command.Name, command.Description, UtcNow());
        await _identityProvider.UpdateRoleAsync(previousName, role, cancellationToken);
        await _unitOfWork.SaveChangesAsync(cancellationToken);
        return role.ToResponse();
    }

    public async Task DeleteAsync(Guid roleId, CancellationToken cancellationToken)
    {
        var role = await GetRequiredRoleAsync(roleId, cancellationToken);
        await _identityProvider.DeleteRoleAsync(role.Name, cancellationToken);
        role.SoftDelete(UtcNow());
        await _unitOfWork.SaveChangesAsync(cancellationToken);
    }

    public async Task AssignToUserAsync(
        Guid userId,
        Guid roleId,
        CancellationToken cancellationToken)
    {
        _ = await _users.GetByIdAsync(userId, cancellationToken)
            ?? throw new NotFoundException($"User '{userId}' was not found.");
        var role = await GetRequiredRoleAsync(roleId, cancellationToken);

        if (await _userRoles.ExistsAsync(userId, roleId, cancellationToken))
        {
            return;
        }

        await _identityProvider.AssignRoleAsync(userId, role.Name, cancellationToken);
        await _userRoles.AddAsync(new UserRole(userId, roleId), cancellationToken);
        await _unitOfWork.SaveChangesAsync(cancellationToken);
    }

    public async Task RemoveFromUserAsync(
        Guid userId,
        Guid roleId,
        CancellationToken cancellationToken)
    {
        var role = await GetRequiredRoleAsync(roleId, cancellationToken);
        await _identityProvider.RemoveRoleAsync(userId, role.Name, cancellationToken);
        await _userRoles.RemoveAsync(userId, roleId, cancellationToken);
        await _unitOfWork.SaveChangesAsync(cancellationToken);
    }

    public async Task<IReadOnlyList<RoleResponse>> GetUserRolesAsync(
        Guid userId,
        CancellationToken cancellationToken)
    {
        _ = await _users.GetByIdAsync(userId, cancellationToken)
            ?? throw new NotFoundException($"User '{userId}' was not found.");
        var roles = await _userRoles.GetRolesAsync(userId, cancellationToken);
        return roles.Select(role => role.ToResponse()).ToList();
    }

    private async Task<Role> GetRequiredRoleAsync(Guid roleId, CancellationToken cancellationToken)
    {
        return await _roles.GetByIdAsync(roleId, cancellationToken)
            ?? throw new NotFoundException($"Role '{roleId}' was not found.");
    }

    private DateTime UtcNow() => _timeProvider.GetUtcNow().UtcDateTime;
}
