using Microsoft.EntityFrameworkCore;

namespace Payment.Api.Infrastructure.Persistence;

public sealed class AppDbContext(DbContextOptions<AppDbContext> options)
    : DbContext(options);
