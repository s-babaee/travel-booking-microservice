using Auth.Api.Application.Abstractions;
using Auth.Api.Domain.Entities;
using Microsoft.EntityFrameworkCore;

namespace Auth.Api.Infrastructure.Persistence.Repositories
{
    public sealed class PermissionRepository : IPermissionRepository
    {
        private readonly AppDbContext _context;

        public PermissionRepository(AppDbContext context)
        {
            _context = context;
        }

        public async Task<Permission?> GetByIdAsync(Guid id, CancellationToken cancellationToken)
        {
            return await _context.Permissions.SingleOrDefaultAsync(
                permission => permission.Id == id && !permission.IsDeleted,
                cancellationToken);
        }

        public async Task<IReadOnlyList<Permission>> ListAsync(CancellationToken cancellationToken)
        {
            return await _context.Permissions
                .Where(permission => !permission.IsDeleted)
                .OrderBy(permission => permission.Code)
                .ToListAsync(cancellationToken);
        }

        public Task AddAsync(Permission permission, CancellationToken cancellationToken)
        {
            return _context.Permissions.AddAsync(permission, cancellationToken).AsTask();
        }
    }
}
