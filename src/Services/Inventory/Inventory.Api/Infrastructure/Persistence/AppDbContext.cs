using Inventory.Api.Application.Abstractions;
using Inventory.Api.Domain.Entities;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Storage;

namespace Inventory.Api.Infrastructure.Persistence;

public sealed class AppDbContext : DbContext,
    IUnitOfWork,
    IHotelInventoryRepository,
    IFlightInventoryRepository,
    IHotelInventoryHoldRepository,
    IFlightInventoryHoldRepository
{
    public AppDbContext(DbContextOptions<AppDbContext> options)
        : base(options)
    {
    }

    public DbSet<HotelInventoryDay> HotelInventoryDays =>
        Set<HotelInventoryDay>();

    public DbSet<FlightInventoryDay> FlightInventoryDays =>
        Set<FlightInventoryDay>();

    public DbSet<HotelInventoryHold> HotelInventoryHolds =>
        Set<HotelInventoryHold>();

    public DbSet<HotelInventoryHoldLine> HotelInventoryHoldLines =>
        Set<HotelInventoryHoldLine>();

    public DbSet<FlightInventoryHold> FlightInventoryHolds =>
        Set<FlightInventoryHold>();

    public DbSet<FlightInventoryHoldLine> FlightInventoryHoldLines =>
        Set<FlightInventoryHoldLine>();

    protected override void OnModelCreating(ModelBuilder modelBuilder)
    {
        modelBuilder.Entity<HotelInventoryDay>(entity =>
        {
            entity.ToTable("hotel_inventory_days");
            entity.HasKey(day => new
            {
                day.HotelId,
                day.RoomTypeId,
                day.Date
            });
            entity.Property(day => day.HotelId)
                .HasColumnName("hotel_id");
            entity.Property(day => day.RoomTypeId)
                .HasColumnName("room_type_id");
            entity.Property(day => day.Date)
                .HasColumnName("inventory_date");
            entity.Property(day => day.TotalUnits)
                .HasColumnName("total_units");
            entity.Property(day => day.AvailableUnits)
                .HasColumnName("available_units");
            entity.Property(day => day.HeldUnits)
                .HasColumnName("held_units");
            entity.Property(day => day.ConfirmedUnits)
                .HasColumnName("confirmed_units");
            entity.Property(day => day.UpdatedAtUtc)
                .HasColumnName("updated_at_utc");
            entity.HasIndex(day => new
            {
                day.HotelId,
                day.Date
            });
        });

        modelBuilder.Entity<FlightInventoryDay>(entity =>
        {
            entity.ToTable("flight_inventory_days");
            entity.HasKey(day => new
            {
                day.FlightId,
                day.FlightClassId,
                day.Date
            });
            entity.Property(day => day.FlightId)
                .HasColumnName("flight_id");
            entity.Property(day => day.FlightClassId)
                .HasColumnName("flight_class_id");
            entity.Property(day => day.Date)
                .HasColumnName("inventory_date");
            entity.Property(day => day.TotalSeats)
                .HasColumnName("total_seats");
            entity.Property(day => day.AvailableSeats)
                .HasColumnName("available_seats");
            entity.Property(day => day.HeldSeats)
                .HasColumnName("held_seats");
            entity.Property(day => day.ConfirmedSeats)
                .HasColumnName("confirmed_seats");
            entity.Property(day => day.UpdatedAtUtc)
                .HasColumnName("updated_at_utc");
            entity.HasIndex(day => new
            {
                day.FlightId,
                day.Date
            });
        });

        modelBuilder.Entity<HotelInventoryHold>(entity =>
        {
            entity.ToTable("hotel_inventory_holds");
            entity.HasKey(hold => hold.Id);
            entity.Property(hold => hold.Id)
                .HasColumnName("hold_id");
            entity.Property(hold => hold.HotelId)
                .HasColumnName("hotel_id");
            entity.Property(hold => hold.Status)
                .HasConversion<string>()
                .HasColumnName("status")
                .HasMaxLength(20)
                .IsRequired();
            entity.Property(hold => hold.ExpiresAtUtc)
                .HasColumnName("expires_at_utc");
            entity.Property(hold => hold.CreatedAtUtc)
                .HasColumnName("created_at_utc");
            entity.Property(hold => hold.CompletedAtUtc)
                .HasColumnName("completed_at_utc");
            entity.HasIndex(hold => new
            {
                hold.Status,
                hold.ExpiresAtUtc
            });
            entity.HasMany(hold => hold.Lines)
                .WithOne()
                .HasForeignKey("HoldId")
                .OnDelete(DeleteBehavior.Cascade);
        });

        modelBuilder.Entity<HotelInventoryHoldLine>(entity =>
        {
            entity.ToTable("hotel_inventory_hold_lines");
            entity.Property<Guid>("HoldId")
                .HasColumnName("hold_id");
            entity.HasKey(
                "HoldId",
                nameof(HotelInventoryHoldLine.RoomTypeId),
                nameof(HotelInventoryHoldLine.Date));
            entity.Property(line => line.RoomTypeId)
                .HasColumnName("room_type_id");
            entity.Property(line => line.Date)
                .HasColumnName("inventory_date");
            entity.Property(line => line.Quantity)
                .HasColumnName("quantity");
        });

        modelBuilder.Entity<FlightInventoryHold>(entity =>
        {
            entity.ToTable("flight_inventory_holds");
            entity.HasKey(hold => hold.Id);
            entity.Property(hold => hold.Id)
                .HasColumnName("hold_id");
            entity.Property(hold => hold.FlightId)
                .HasColumnName("flight_id");
            entity.Property(hold => hold.Status)
                .HasConversion<string>()
                .HasColumnName("status")
                .HasMaxLength(20)
                .IsRequired();
            entity.Property(hold => hold.ExpiresAtUtc)
                .HasColumnName("expires_at_utc");
            entity.Property(hold => hold.CreatedAtUtc)
                .HasColumnName("created_at_utc");
            entity.Property(hold => hold.CompletedAtUtc)
                .HasColumnName("completed_at_utc");
            entity.HasIndex(hold => new
            {
                hold.Status,
                hold.ExpiresAtUtc
            });
            entity.HasMany(hold => hold.Lines)
                .WithOne()
                .HasForeignKey("HoldId")
                .OnDelete(DeleteBehavior.Cascade);
        });

        modelBuilder.Entity<FlightInventoryHoldLine>(entity =>
        {
            entity.ToTable("flight_inventory_hold_lines");
            entity.Property<Guid>("HoldId")
                .HasColumnName("hold_id");
            entity.HasKey(
                "HoldId",
                nameof(FlightInventoryHoldLine.FlightClassId),
                nameof(FlightInventoryHoldLine.Date));
            entity.Property(line => line.FlightClassId)
                .HasColumnName("flight_class_id");
            entity.Property(line => line.Date)
                .HasColumnName("inventory_date");
            entity.Property(line => line.Quantity)
                .HasColumnName("quantity");
        });
    }

    public async Task<IUnitOfWorkTransaction> BeginTransactionAsync(
        CancellationToken cancellationToken)
    {
        var transaction = await Database.BeginTransactionAsync(
            cancellationToken);
        return new EfUnitOfWorkTransaction(transaction);
    }

    async Task<IReadOnlyList<HotelInventoryDay>>
        IHotelInventoryRepository.GetForUpdateAsync(
            Guid hotelId,
            IReadOnlyCollection<Guid> roomTypeIds,
            DateOnly from,
            DateOnly to,
            CancellationToken cancellationToken)
    {
        var ids = roomTypeIds.ToArray();
        return await HotelInventoryDays
            .FromSqlInterpolated($"""
                SELECT hotel_id, room_type_id, inventory_date,
                       total_units, available_units, held_units,
                       confirmed_units, updated_at_utc
                FROM hotel_inventory_days
                WHERE hotel_id = {hotelId}
                  AND room_type_id = ANY({ids})
                  AND inventory_date >= {from}
                  AND inventory_date < {to}
                ORDER BY room_type_id, inventory_date
                FOR UPDATE
                """)
            .ToListAsync(cancellationToken);
    }

    async Task<HotelInventoryDay?>
        IHotelInventoryRepository.GetForUpdateAsync(
            Guid hotelId,
            Guid roomTypeId,
            DateOnly date,
            CancellationToken cancellationToken)
    {
        return await HotelInventoryDays
            .FromSqlInterpolated($"""
                SELECT hotel_id, room_type_id, inventory_date,
                       total_units, available_units, held_units,
                       confirmed_units, updated_at_utc
                FROM hotel_inventory_days
                WHERE hotel_id = {hotelId}
                  AND room_type_id = {roomTypeId}
                  AND inventory_date = {date}
                FOR UPDATE
                """)
            .SingleOrDefaultAsync(cancellationToken);
    }

    async Task<IReadOnlyList<HotelInventoryDay>>
        IHotelInventoryRepository.ListAsync(
            Guid hotelId,
            DateOnly from,
            DateOnly to,
            Guid? roomTypeId,
            CancellationToken cancellationToken)
    {
        return await HotelInventoryDays
            .AsNoTracking()
            .Where(day =>
                day.HotelId == hotelId
                && day.Date >= from
                && day.Date < to
                && (!roomTypeId.HasValue
                    || day.RoomTypeId == roomTypeId.Value))
            .OrderBy(day => day.Date)
            .ThenBy(day => day.RoomTypeId)
            .ToListAsync(cancellationToken);
    }

    public Task AddAsync(
        HotelInventoryDay inventory,
        CancellationToken cancellationToken)
    {
        return HotelInventoryDays.AddAsync(inventory, cancellationToken).AsTask();
    }

    async Task IHotelInventoryRepository.EnsureExistsAsync(
        Guid hotelId,
        Guid roomTypeId,
        DateOnly date,
        DateTime nowUtc,
        CancellationToken cancellationToken)
    {
        await Database.ExecuteSqlInterpolatedAsync($"""
            INSERT INTO hotel_inventory_days
                (hotel_id, room_type_id, inventory_date, total_units,
                 available_units, held_units, confirmed_units, updated_at_utc)
            VALUES ({hotelId}, {roomTypeId}, {date}, 0, 0, 0, 0, {nowUtc})
            ON CONFLICT (hotel_id, room_type_id, inventory_date) DO NOTHING
            """, cancellationToken);
    }

    async Task<IReadOnlyList<FlightInventoryDay>>
        IFlightInventoryRepository.GetForUpdateAsync(
            Guid flightId,
            IReadOnlyCollection<Guid> flightClassIds,
            DateOnly date,
            CancellationToken cancellationToken)
    {
        var ids = flightClassIds.ToArray();
        return await FlightInventoryDays
            .FromSqlInterpolated($"""
                SELECT flight_id, flight_class_id, inventory_date,
                       total_seats, available_seats, held_seats,
                       confirmed_seats, updated_at_utc
                FROM flight_inventory_days
                WHERE flight_id = {flightId}
                  AND flight_class_id = ANY({ids})
                  AND inventory_date = {date}
                ORDER BY flight_class_id
                FOR UPDATE
                """)
            .ToListAsync(cancellationToken);
    }

    async Task<FlightInventoryDay?>
        IFlightInventoryRepository.GetForUpdateAsync(
            Guid flightId,
            Guid flightClassId,
            DateOnly date,
            CancellationToken cancellationToken)
    {
        return await FlightInventoryDays
            .FromSqlInterpolated($"""
                SELECT flight_id, flight_class_id, inventory_date,
                       total_seats, available_seats, held_seats,
                       confirmed_seats, updated_at_utc
                FROM flight_inventory_days
                WHERE flight_id = {flightId}
                  AND flight_class_id = {flightClassId}
                  AND inventory_date = {date}
                FOR UPDATE
                """)
            .SingleOrDefaultAsync(cancellationToken);
    }

    async Task<IReadOnlyList<FlightInventoryDay>>
        IFlightInventoryRepository.ListAsync(
            Guid flightId,
            DateOnly date,
            Guid? flightClassId,
            CancellationToken cancellationToken)
    {
        return await FlightInventoryDays
            .AsNoTracking()
            .Where(day =>
                day.FlightId == flightId
                && day.Date == date
                && (!flightClassId.HasValue
                    || day.FlightClassId == flightClassId.Value))
            .OrderBy(day => day.FlightClassId)
            .ToListAsync(cancellationToken);
    }

    public Task AddAsync(
        FlightInventoryDay inventory,
        CancellationToken cancellationToken)
    {
        return FlightInventoryDays.AddAsync(inventory, cancellationToken).AsTask();
    }

    async Task IFlightInventoryRepository.EnsureExistsAsync(
        Guid flightId,
        Guid flightClassId,
        DateOnly date,
        DateTime nowUtc,
        CancellationToken cancellationToken)
    {
        await Database.ExecuteSqlInterpolatedAsync($"""
            INSERT INTO flight_inventory_days
                (flight_id, flight_class_id, inventory_date, total_seats,
                 available_seats, held_seats, confirmed_seats, updated_at_utc)
            VALUES ({flightId}, {flightClassId}, {date}, 0, 0, 0, 0, {nowUtc})
            ON CONFLICT (flight_id, flight_class_id, inventory_date) DO NOTHING
            """, cancellationToken);
    }

    async Task<HotelInventoryHold?>
        IHotelInventoryHoldRepository.GetByIdAsync(
            Guid id,
            CancellationToken cancellationToken)
    {
        return await HotelInventoryHolds
            .AsNoTracking()
            .Include(hold => hold.Lines)
            .SingleOrDefaultAsync(hold => hold.Id == id, cancellationToken);
    }

    public Task AddAsync(
        HotelInventoryHold hold,
        CancellationToken cancellationToken)
    {
        return HotelInventoryHolds.AddAsync(hold, cancellationToken).AsTask();
    }

    async Task<IReadOnlyList<HotelInventoryHold>>
        IHotelInventoryHoldRepository.ListExpiredAsync(
            DateTime nowUtc,
            CancellationToken cancellationToken)
    {
        return await HotelInventoryHolds
            .Include(hold => hold.Lines)
            .Where(hold =>
                hold.Status == Domain.Enums.HoldStatus.Active
                && hold.ExpiresAtUtc <= nowUtc)
            .OrderBy(hold => hold.ExpiresAtUtc)
            .ToListAsync(cancellationToken);
    }

    async Task<FlightInventoryHold?>
        IFlightInventoryHoldRepository.GetByIdAsync(
            Guid id,
            CancellationToken cancellationToken)
    {
        return await FlightInventoryHolds
            .AsNoTracking()
            .Include(hold => hold.Lines)
            .SingleOrDefaultAsync(hold => hold.Id == id, cancellationToken);
    }

    public Task AddAsync(
        FlightInventoryHold hold,
        CancellationToken cancellationToken)
    {
        return FlightInventoryHolds.AddAsync(hold, cancellationToken).AsTask();
    }

    async Task<IReadOnlyList<FlightInventoryHold>>
        IFlightInventoryHoldRepository.ListExpiredAsync(
            DateTime nowUtc,
            CancellationToken cancellationToken)
    {
        return await FlightInventoryHolds
            .Include(hold => hold.Lines)
            .Where(hold =>
                hold.Status == Domain.Enums.HoldStatus.Active
                && hold.ExpiresAtUtc <= nowUtc)
            .OrderBy(hold => hold.ExpiresAtUtc)
            .ToListAsync(cancellationToken);
    }
}

internal sealed class EfUnitOfWorkTransaction : IUnitOfWorkTransaction
{
    private readonly IDbContextTransaction _transaction;
    private bool _committed;

    public EfUnitOfWorkTransaction(IDbContextTransaction transaction)
    {
        _transaction = transaction;
    }

    public async Task CommitAsync(CancellationToken cancellationToken)
    {
        await _transaction.CommitAsync(cancellationToken);
        _committed = true;
    }

    public async ValueTask DisposeAsync()
    {
        if (!_committed)
        {
            await _transaction.RollbackAsync();
        }

        await _transaction.DisposeAsync();
    }
}
