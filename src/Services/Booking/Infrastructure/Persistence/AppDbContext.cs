using Booking.Api.Application.Abstractions;
using Booking.Api.Application.Contracts;
using Booking.Api.Domain.Entities;
using Microsoft.EntityFrameworkCore;
using BookingEntity = Booking.Api.Domain.Entities.Booking;

namespace Booking.Api.Infrastructure.Persistence;

public sealed class AppDbContext(DbContextOptions<AppDbContext> options)
    : DbContext(options), IUnitOfWork, IBookingRepository, IOrderRepository
{
    public DbSet<BookingEntity> Bookings => Set<BookingEntity>();
    public DbSet<BookingItem> BookingItems => Set<BookingItem>();
    public DbSet<Order> Orders => Set<Order>();

    protected override void OnModelCreating(ModelBuilder modelBuilder)
    {
        modelBuilder.Entity<BookingEntity>(entity =>
        {
            entity.ToTable("bookings");
            entity.HasKey(booking => booking.Id);
            entity.Property(booking => booking.Type)
                .HasConversion<string>()
                .HasMaxLength(20)
                .IsRequired();
            entity.Property(booking => booking.Status)
                .HasConversion<string>()
                .HasMaxLength(30)
                .IsRequired();
            entity.Property(booking => booking.Currency)
                .HasMaxLength(3)
                .IsRequired();
            entity.Property(booking => booking.TotalAmount)
                .HasPrecision(18, 2);
            entity.Property(booking => booking.IdempotencyKey)
                .HasMaxLength(200);
            entity.Property(booking => booking.PassengerName)
                .HasMaxLength(200);
            entity.Property(booking => booking.FailureReason)
                .HasMaxLength(1000);
            entity.HasIndex(booking => new
            {
                booking.UserId,
                booking.IdempotencyKey
            })
                .IsUnique()
                .HasFilter("\"IdempotencyKey\" IS NOT NULL");
            entity.HasIndex(booking => booking.UserId);
            entity.HasIndex(booking => booking.Status);
            entity.HasIndex(booking => booking.CreatedAtUtc);
            entity.HasMany(booking => booking.Items)
                .WithOne()
                .HasForeignKey(item => item.BookingId)
                .OnDelete(DeleteBehavior.Cascade);
        });

        modelBuilder.Entity<BookingItem>(entity =>
        {
            entity.ToTable("booking_items");
            entity.HasKey(item => new { item.BookingId, item.ResourceTypeId });
            entity.Property(item => item.UnitAmount)
                .HasPrecision(18, 2);
        });

        modelBuilder.Entity<Order>(entity =>
        {
            entity.ToTable("orders");
            entity.HasKey(order => order.Id);
            entity.Property(order => order.Currency)
                .HasMaxLength(3)
                .IsRequired();
            entity.Property(order => order.TotalAmount)
                .HasPrecision(18, 2);
            entity.HasIndex(order => order.BookingId)
                .IsUnique();
            entity.HasIndex(order => order.UserId);
        });
    }

    async Task<BookingEntity?> IBookingRepository.GetByIdAsync(
        Guid id,
        CancellationToken cancellationToken) =>
        await Bookings
            .Include(booking => booking.Items)
            .SingleOrDefaultAsync(booking => booking.Id == id, cancellationToken);

    async Task<BookingEntity?> IBookingRepository.GetByUserAndIdempotencyKeyAsync(
        Guid userId,
        string idempotencyKey,
        CancellationToken cancellationToken) =>
        await Bookings
            .Include(booking => booking.Items)
            .SingleOrDefaultAsync(
                booking => booking.UserId == userId
                    && booking.IdempotencyKey == idempotencyKey,
                cancellationToken);

    async Task<IReadOnlyList<BookingEntity>> IBookingRepository.ListByUserAsync(
        Guid userId,
        int page,
        int pageSize,
        CancellationToken cancellationToken) =>
        await Bookings
            .AsNoTracking()
            .Include(booking => booking.Items)
            .Where(booking => booking.UserId == userId)
            .OrderByDescending(booking => booking.CreatedAtUtc)
            .Skip((page - 1) * pageSize)
            .Take(pageSize)
            .ToArrayAsync(cancellationToken);

    async Task<(IReadOnlyList<BookingEntity> Items, int TotalCount)>
        IBookingRepository.SearchAsync(
            BookingSearchQuery query,
            CancellationToken cancellationToken)
    {
        var bookings = Bookings
            .AsNoTracking()
            .Include(booking => booking.Items)
            .AsQueryable();

        if (query.UserId.HasValue)
        {
            bookings = bookings.Where(booking =>
                booking.UserId == query.UserId.Value);
        }

        if (query.Status.HasValue)
        {
            bookings = bookings.Where(booking =>
                booking.Status == query.Status.Value);
        }

        if (query.Type.HasValue)
        {
            bookings = bookings.Where(booking =>
                booking.Type == query.Type.Value);
        }

        if (query.FromUtc.HasValue)
        {
            bookings = bookings.Where(booking =>
                booking.CreatedAtUtc >= query.FromUtc.Value);
        }

        if (query.ToUtc.HasValue)
        {
            bookings = bookings.Where(booking =>
                booking.CreatedAtUtc <= query.ToUtc.Value);
        }

        var totalCount = await bookings.CountAsync(cancellationToken);
        var items = await bookings
            .OrderByDescending(booking => booking.CreatedAtUtc)
            .Skip((query.Page - 1) * query.PageSize)
            .Take(query.PageSize)
            .ToArrayAsync(cancellationToken);
        return (items, totalCount);
    }

    async Task<BookingStatsResponse> IBookingRepository.GetStatsAsync(
        DateTime? fromUtc,
        DateTime? toUtc,
        CancellationToken cancellationToken)
    {
        var bookings = Bookings.AsNoTracking().AsQueryable();
        if (fromUtc.HasValue)
        {
            bookings = bookings.Where(booking =>
                booking.CreatedAtUtc >= fromUtc.Value);
        }

        if (toUtc.HasValue)
        {
            bookings = bookings.Where(booking =>
                booking.CreatedAtUtc <= toUtc.Value);
        }

        var total = await bookings.CountAsync(cancellationToken);
        var pending = await bookings.CountAsync(
            booking => booking.Status == Domain.Enums.BookingStatus.PendingInventory
                || booking.Status == Domain.Enums.BookingStatus.PendingPayment
                || booking.Status == Domain.Enums.BookingStatus.ConfirmingInventory,
            cancellationToken);
        var confirmed = await bookings.CountAsync(
            booking => booking.Status == Domain.Enums.BookingStatus.Confirmed,
            cancellationToken);
        var cancelled = await bookings.CountAsync(
            booking => booking.Status == Domain.Enums.BookingStatus.Cancelled,
            cancellationToken);
        var failed = await bookings.CountAsync(
            booking => booking.Status == Domain.Enums.BookingStatus.Failed,
            cancellationToken);
        var confirmedAmount = await bookings
            .Where(booking => booking.Status == Domain.Enums.BookingStatus.Confirmed)
            .SumAsync(booking => (decimal?)booking.TotalAmount, cancellationToken)
            ?? 0m;

        return new BookingStatsResponse(
            total,
            pending,
            confirmed,
            cancelled,
            failed,
            confirmedAmount);
    }

    async Task<IReadOnlyList<Order>> IOrderRepository.ListAsync(
        int page,
        int pageSize,
        CancellationToken cancellationToken) =>
        await Orders
            .AsNoTracking()
            .OrderByDescending(order => order.CreatedAtUtc)
            .Skip((page - 1) * pageSize)
            .Take(pageSize)
            .ToArrayAsync(cancellationToken);

    async Task<Order?> IOrderRepository.GetByIdAsync(
        Guid id,
        CancellationToken cancellationToken) =>
        await Orders.SingleOrDefaultAsync(order => order.Id == id, cancellationToken);

    async Task IBookingRepository.AddAsync(
        BookingEntity booking,
        CancellationToken cancellationToken) =>
        await Bookings.AddAsync(booking, cancellationToken);

    async Task IOrderRepository.AddAsync(
        Order order,
        CancellationToken cancellationToken) =>
        await Orders.AddAsync(order, cancellationToken);

    Task<int> IUnitOfWork.SaveChangesAsync(
        CancellationToken cancellationToken) =>
        base.SaveChangesAsync(cancellationToken);
}
