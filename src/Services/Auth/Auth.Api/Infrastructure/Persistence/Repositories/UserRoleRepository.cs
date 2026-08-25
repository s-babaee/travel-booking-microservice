using Auth.Api.Application.Abstractions;
using Auth.Api.Domain.Entities;
using Microsoft.EntityFrameworkCore;

namespace Auth.Api.Infrastructure.Persistence.Repositories
{
    public sealed class UserRoleRepository : IUserRoleRepository
    {
        private readonly AppDbContext _context;

        public UserRoleRepository(AppDbContext context)
        {
            _context = context;
        }

        public Task<bool> ExistsAsync(Guid userId, Guid roleId, CancellationToken cancellationToken)
        {
            return _context.UserRoles.AnyAsync(
                userRole => userRole.UserId == userId && userRole.RoleId == roleId,
                cancellationToken);
        }

        public Task AddAsync(UserRole userRole, CancellationToken cancellationToken)
        {
            return _context.UserRoles.AddAsync(userRole, cancellationToken).AsTask();
        }

        public async Task RemoveAsync(Guid userId, Guid roleId, CancellationToken cancellationToken)
        {
            var userRole = await _context.UserRoles.FindAsync([userId, roleId], cancellationToken);
            if (userRole is not null)
            {
                _context.UserRoles.Remove(userRole);
            }
        }

        public async Task<IReadOnlyList<Role>> GetRolesAsync(Guid userId, CancellationToken cancellationToken)
        {
            return await _context.UserRoles
                .Where(userRole => userRole.UserId == userId)
                .Join(
                    _context.Roles.Where(role => !role.IsDeleted),
                    userRole => userRole.RoleId,
                    role => role.Id,
                    (_, role) => role)
                .OrderBy(role => role.Name)
                .ToListAsync(cancellationToken);
        }
    }
}
