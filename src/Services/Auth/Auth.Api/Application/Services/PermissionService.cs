using Auth.Api.Application.Abstractions;
using Auth.Api.Application.Contracts;
using Auth.Api.Application.Exceptions;
using Auth.Api.Domain.Entities;

namespace Auth.Api.Application.Services;

public sealed class PermissionService : IPermissionService
{
    private readonly IPermissionRepository _permissions;
    private readonly IRoleRepository _roles;
    private readonly IRolePermissionRepository _rolePermissions;
    private readonly IIdentityProvider _identityProvider;
    private readonly IUnitOfWork _unitOfWork;
    private readonly TimeProvider _timeProvider;

    public PermissionService(
        IPermissionRepository permissions,
        IRoleRepository roles,
        IRolePermissionRepository rolePermissions,
        IIdentityProvider identityProvider,
        IUnitOfWork unitOfWork,
        TimeProvider timeProvider)
    {
        _permissions = permissions;
        _roles = roles;
        _rolePermissions = rolePermissions;
        _identityProvider = identityProvider;
        _unitOfWork = unitOfWork;
        _timeProvider = timeProvider;
    }

    public async Task<PermissionResponse> CreateAsync(
        CreatePermissionCommand command,
        CancellationToken cancellationToken)
    {
        var permission = Permission.Create(
            Guid.NewGuid(),
            command.Code,
            command.Description,
            UtcNow());
        await _identityProvider.CreatePermissionAsync(permission, cancellationToken);
        await _permissions.AddAsync(permission, cancellationToken);
        await _unitOfWork.SaveChangesAsync(cancellationToken);
        return permission.ToResponse();
    }

    public async Task<IReadOnlyList<PermissionResponse>> ListAsync(
        CancellationToken cancellationToken)
    {
        var permissions = await _permissions.ListAsync(cancellationToken);
        return permissions.Select(permission => permission.ToResponse()).ToList();
    }

    public async Task<PermissionResponse> UpdateAsync(
        Guid permissionId,
        UpdatePermissionCommand command,
        CancellationToken cancellationToken)
    {
        var permission = await GetRequiredPermissionAsync(permissionId, cancellationToken);
        var previousCode = permission.Code;
        permission.Update(command.Code, command.Description, UtcNow());
        await _identityProvider.UpdatePermissionAsync(
            previousCode,
            permission,
            cancellationToken);
        await _unitOfWork.SaveChangesAsync(cancellationToken);
        return permission.ToResponse();
    }

    public async Task DeleteAsync(Guid permissionId, CancellationToken cancellationToken)
    {
        var permission = await GetRequiredPermissionAsync(permissionId, cancellationToken);
        await _identityProvider.DeletePermissionAsync(permission.Code, cancellationToken);
        permission.SoftDelete(UtcNow());
        await _unitOfWork.SaveChangesAsync(cancellationToken);
    }

    public async Task AssignToRoleAsync(
        Guid roleId,
        Guid permissionId,
        CancellationToken cancellationToken)
    {
        var role = await GetRequiredRoleAsync(roleId, cancellationToken);
        var permission = await GetRequiredPermissionAsync(permissionId, cancellationToken);

        if (await _rolePermissions.ExistsAsync(roleId, permissionId, cancellationToken))
        {
            return;
        }

        await _identityProvider.AssignPermissionToRoleAsync(
            role.Name,
            permission.Code,
            cancellationToken);
        await _rolePermissions.AddAsync(
            new RolePermission(roleId, permissionId),
            cancellationToken);
        await _unitOfWork.SaveChangesAsync(cancellationToken);
    }

    public async Task RemoveFromRoleAsync(
        Guid roleId,
        Guid permissionId,
        CancellationToken cancellationToken)
    {
        var role = await GetRequiredRoleAsync(roleId, cancellationToken);
        var permission = await GetRequiredPermissionAsync(permissionId, cancellationToken);
        await _identityProvider.RemovePermissionFromRoleAsync(
            role.Name,
            permission.Code,
            cancellationToken);
        await _rolePermissions.RemoveAsync(roleId, permissionId, cancellationToken);
        await _unitOfWork.SaveChangesAsync(cancellationToken);
    }

    private async Task<Role> GetRequiredRoleAsync(Guid roleId, CancellationToken cancellationToken)
    {
        return await _roles.GetByIdAsync(roleId, cancellationToken)
            ?? throw new NotFoundException($"Role '{roleId}' was not found.");
    }

    private async Task<Permission> GetRequiredPermissionAsync(
        Guid permissionId,
        CancellationToken cancellationToken)
    {
        return await _permissions.GetByIdAsync(permissionId, cancellationToken)
            ?? throw new NotFoundException($"Permission '{permissionId}' was not found.");
    }

    private DateTime UtcNow() => _timeProvider.GetUtcNow().UtcDateTime;
}
