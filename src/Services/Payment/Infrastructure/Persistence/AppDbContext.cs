using Payment.Api.Application.Abstractions;
using Payment.Api.Domain.Entities;
using Microsoft.EntityFrameworkCore;

namespace Payment.Api.Infrastructure.Persistence;

public sealed class AppDbContext(DbContextOptions<AppDbContext> options)
    : DbContext(options), IUnitOfWork, IPaymentRepository, IRefundRepository
{
    public DbSet<PaymentTransaction> PaymentTransactions =>
        Set<PaymentTransaction>();

    public DbSet<Refund> Refunds => Set<Refund>();

    protected override void OnModelCreating(ModelBuilder modelBuilder)
    {
        modelBuilder.Entity<PaymentTransaction>(entity =>
        {
            entity.ToTable("payment_transactions");
            entity.HasKey(payment => payment.Id);
            entity.Property(payment => payment.Currency)
                .HasMaxLength(3)
                .IsRequired();
            entity.Property(payment => payment.IdempotencyKey)
                .HasMaxLength(200)
                .IsRequired();
            entity.Property(payment => payment.Status)
                .HasConversion<string>()
                .HasMaxLength(20)
                .IsRequired();
            entity.Property(payment => payment.FailureReason)
                .HasMaxLength(1000);
            entity.Property(payment => payment.Amount)
                .HasPrecision(18, 2);
            entity.HasIndex(payment => payment.IdempotencyKey)
                .IsUnique();
            entity.HasIndex(payment => payment.BookingId);
            entity.HasIndex(payment => payment.UserId);
        });

        modelBuilder.Entity<Refund>(entity =>
        {
            entity.ToTable("refunds");
            entity.HasKey(refund => refund.Id);
            entity.Property(refund => refund.Currency)
                .HasMaxLength(3)
                .IsRequired();
            entity.Property(refund => refund.Status)
                .HasConversion<string>()
                .HasMaxLength(20)
                .IsRequired();
            entity.Property(refund => refund.Reason)
                .HasMaxLength(1000);
            entity.Property(refund => refund.Amount)
                .HasPrecision(18, 2);
            entity.HasIndex(refund => refund.PaymentId)
                .IsUnique();
            entity.HasIndex(refund => refund.BookingId);
            entity.HasIndex(refund => refund.UserId);
        });
    }

    async Task<PaymentTransaction?> IPaymentRepository.GetByIdAsync(
        Guid id,
        CancellationToken cancellationToken) =>
        await PaymentTransactions.SingleOrDefaultAsync(
            payment => payment.Id == id,
            cancellationToken);

    async Task<PaymentTransaction?>
        IPaymentRepository.GetByIdempotencyKeyAsync(
            string idempotencyKey,
            CancellationToken cancellationToken) =>
        await PaymentTransactions.SingleOrDefaultAsync(
            payment => payment.IdempotencyKey == idempotencyKey,
            cancellationToken);

    async Task<IReadOnlyList<PaymentTransaction>>
        IPaymentRepository.ListByBookingAsync(
            Guid bookingId,
            CancellationToken cancellationToken) =>
        await PaymentTransactions
            .AsNoTracking()
            .Where(payment => payment.BookingId == bookingId)
            .OrderByDescending(payment => payment.CreatedAtUtc)
            .ToArrayAsync(cancellationToken);

    async Task<Refund?> IRefundRepository.GetByIdAsync(
        Guid id,
        CancellationToken cancellationToken) =>
        await Refunds.SingleOrDefaultAsync(
            refund => refund.Id == id,
            cancellationToken);

    async Task<Refund?> IRefundRepository.GetByPaymentIdAsync(
        Guid paymentId,
        CancellationToken cancellationToken) =>
        await Refunds.SingleOrDefaultAsync(
            refund => refund.PaymentId == paymentId,
            cancellationToken);

    Task<int> IUnitOfWork.SaveChangesAsync(
        CancellationToken cancellationToken) =>
        base.SaveChangesAsync(cancellationToken);

    Task IPaymentRepository.AddAsync(
        PaymentTransaction payment,
        CancellationToken cancellationToken) =>
        PaymentTransactions.AddAsync(payment, cancellationToken).AsTask();

    Task IRefundRepository.AddAsync(
        Refund refund,
        CancellationToken cancellationToken) =>
        Refunds.AddAsync(refund, cancellationToken).AsTask();
}
