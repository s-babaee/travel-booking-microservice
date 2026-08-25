using Auth.Api.Application.Abstractions;
using Auth.Api.Domain.Entities;
using Microsoft.EntityFrameworkCore;

namespace Auth.Api.Infrastructure.Persistence.Repositories
{
    public sealed class RoleRepository : IRoleRepository
    {
        private readonly AppDbContext _context;

        public RoleRepository(AppDbContext context)
        {
            _context = context;
        }

        public async Task<Role?> GetByIdAsync(Guid id, CancellationToken cancellationToken)
        {
            return await _context.Roles.SingleOrDefaultAsync(role => role.Id == id && !role.IsDeleted, cancellationToken);
        }

        public async Task<IReadOnlyList<Role>> ListAsync(CancellationToken cancellationToken)
        {
            return await _context.Roles
                .Where(role => !role.IsDeleted)
                .OrderBy(role => role.Name)
                .ToListAsync(cancellationToken);
        }

        public Task AddAsync(Role role, CancellationToken cancellationToken)
        {
            return _context.Roles.AddAsync(role, cancellationToken).AsTask();
        }
    }
}
