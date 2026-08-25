using Auth.Api.Application.Abstractions;
using Auth.Api.Domain.Entities;
using Microsoft.EntityFrameworkCore;

namespace Auth.Api.Infrastructure.Persistence.Repositories
{
    public sealed class UserRepository : IUserRepository
    {
        private readonly AppDbContext _context;

        public UserRepository(AppDbContext context)
        {
            _context = context;
        }

        public async Task<User?> GetByIdAsync(Guid id, CancellationToken cancellationToken)
        {
            return await _context.Users.SingleOrDefaultAsync(user => user.Id == id, cancellationToken);
        }

        public Task<User?> GetByEmailAsync(string email, CancellationToken cancellationToken)
        {
            var normalizedEmail = email.Trim().ToLower();
            return _context.Users.SingleOrDefaultAsync(user => user.Email == normalizedEmail, cancellationToken);
        }

        public Task AddAsync(User user, CancellationToken cancellationToken)
        {
            return _context.Users.AddAsync(user, cancellationToken).AsTask();
        }
    }
}
