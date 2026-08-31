using Auth.Api.Domain.Entities;

namespace Auth.Api.Application.Abstractions;

public interface IUserRepository
{
    Task<User?> GetByIdAsync(Guid id, CancellationToken cancellationToken);
    Task<User?> GetByEmailAsync(string email, CancellationToken cancellationToken);
    Task AddAsync(User user, CancellationToken cancellationToken);
}

public interface IRoleRepository
{
    Task<Role?> GetByIdAsync(Guid id, CancellationToken cancellationToken);
    Task<Role?> GetByNameAsync(string name, CancellationToken cancellationToken);
    Task<IReadOnlyList<Role>> ListAsync(CancellationToken cancellationToken);
    Task AddAsync(Role role, CancellationToken cancellationToken);
}

public interface IPermissionRepository
{
    Task<Permission?> GetByIdAsync(Guid id, CancellationToken cancellationToken);
    Task<IReadOnlyList<Permission>> ListAsync(CancellationToken cancellationToken);
    Task AddAsync(Permission permission, CancellationToken cancellationToken);
}

public interface IUserRoleRepository
{
    Task<bool> ExistsAsync(Guid userId, Guid roleId, CancellationToken cancellationToken);
    Task AddAsync(UserRole userRole, CancellationToken cancellationToken);
    Task RemoveAsync(Guid userId, Guid roleId, CancellationToken cancellationToken);
    Task<IReadOnlyList<Role>> GetRolesAsync(Guid userId, CancellationToken cancellationToken);
}

public interface IRolePermissionRepository
{
    Task<bool> ExistsAsync(Guid roleId, Guid permissionId, CancellationToken cancellationToken);
    Task AddAsync(RolePermission rolePermission, CancellationToken cancellationToken);
    Task RemoveAsync(Guid roleId, Guid permissionId, CancellationToken cancellationToken);
    Task<IReadOnlyList<Permission>> GetPermissionsAsync(Guid roleId, CancellationToken cancellationToken);
}

public interface IPasswordResetTokenRepository
{
    Task AddAsync(PasswordResetToken token, CancellationToken cancellationToken);
    Task<PasswordResetToken?> GetByHashAsync(string tokenHash, CancellationToken cancellationToken);
}

public interface IUnitOfWork
{
    Task<int> SaveChangesAsync(CancellationToken cancellationToken);
}
