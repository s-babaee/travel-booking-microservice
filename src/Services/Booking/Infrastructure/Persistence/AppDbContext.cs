using Microsoft.EntityFrameworkCore;

namespace Booking.Api.Infrastructure.Persistence;

public sealed class AppDbContext(DbContextOptions<AppDbContext> options)
    : DbContext(options);
