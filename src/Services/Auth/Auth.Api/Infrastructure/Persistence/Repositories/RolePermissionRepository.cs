using Auth.Api.Application.Abstractions;
using Auth.Api.Domain.Entities;
using Microsoft.EntityFrameworkCore;

namespace Auth.Api.Infrastructure.Persistence.Repositories
{
    public sealed class RolePermissionRepository : IRolePermissionRepository
    {
        private readonly AppDbContext _context;

        public RolePermissionRepository(AppDbContext context)
        {
            _context = context;
        }

        public Task<bool> ExistsAsync(Guid roleId, Guid permissionId, CancellationToken cancellationToken)
        {
            return _context.RolePermissions.AnyAsync(
                rolePermission => rolePermission.RoleId == roleId
                    && rolePermission.PermissionId == permissionId,
                cancellationToken);
        }

        public Task AddAsync(RolePermission rolePermission, CancellationToken cancellationToken)
        {
            return _context.RolePermissions.AddAsync(rolePermission, cancellationToken).AsTask();
        }

        public async Task RemoveAsync(Guid roleId, Guid permissionId, CancellationToken cancellationToken)
        {
            var rolePermission = await _context.RolePermissions.FindAsync([roleId, permissionId], cancellationToken);
            if (rolePermission is not null)
            {
                _context.RolePermissions.Remove(rolePermission);
            }
        }
    }
}
