using Auth.Api.Application.Abstractions;
using Auth.Api.Domain.Entities;
using Microsoft.EntityFrameworkCore;

namespace Auth.Api.Infrastructure.Persistence.Repositories
{
    public sealed class PasswordResetTokenRepository : IPasswordResetTokenRepository
    {
        private readonly AppDbContext _context;

        public PasswordResetTokenRepository(AppDbContext context)
        {
            _context = context;
        }

        public Task AddAsync(PasswordResetToken token, CancellationToken cancellationToken)
        {
            return _context.PasswordResetTokens.AddAsync(token, cancellationToken).AsTask();
        }

        public Task<PasswordResetToken?> GetByHashAsync(string tokenHash, CancellationToken cancellationToken)
        {
            return _context.PasswordResetTokens
                .Where(token => token.TokenHash == tokenHash)
                .OrderByDescending(token => token.CreatedAtUtc)
                .FirstOrDefaultAsync(cancellationToken);
        }
    }
}
