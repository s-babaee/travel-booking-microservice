using Flight.Api.Application.Abstractions;
using Flight.Api.Domain.Entities;
using Microsoft.EntityFrameworkCore;
using FlightEntity = Flight.Api.Domain.Entities.Flight;
using RouteEntity = Flight.Api.Domain.Entities.Route;

namespace Flight.Api.Infrastructure.Persistence;

public sealed class AppDbContext : DbContext,
    IUnitOfWork,
    IAirlineRepository,
    IRouteRepository,
    IFlightRepository,
    IFlightScheduleRepository,
    IFlightClassRepository,
    IFlightPolicyRepository
{
    public AppDbContext(DbContextOptions<AppDbContext> options)
        : base(options)
    {
    }

    public DbSet<Airline> Airlines => Set<Airline>();
    public DbSet<RouteEntity> Routes => Set<RouteEntity>();
    public DbSet<FlightEntity> Flights => Set<FlightEntity>();
    public DbSet<FlightSchedule> FlightSchedules => Set<FlightSchedule>();
    public DbSet<FlightClass> FlightClasses => Set<FlightClass>();
    public DbSet<FlightPolicy> FlightPolicies => Set<FlightPolicy>();

    protected override void OnModelCreating(ModelBuilder modelBuilder)
    {
        modelBuilder.Entity<Airline>(entity =>
        {
            entity.ToTable("airlines");
            entity.HasKey(airline => airline.Id);
            entity.Property(airline => airline.Name).HasMaxLength(200).IsRequired();
            entity.Property(airline => airline.IataCode).HasMaxLength(2).IsRequired();
            entity.Property(airline => airline.IcaoCode).HasMaxLength(3).IsRequired();
            entity.Property(airline => airline.Country).HasMaxLength(120).IsRequired();
            entity.Property(airline => airline.WebsiteUrl).HasMaxLength(500);
            entity.Property(airline => airline.Status)
                .HasConversion<string>()
                .HasMaxLength(20)
                .IsRequired();
            entity.HasIndex(airline => airline.Name)
                .IsUnique()
                .HasFilter("\"IsDeleted\" = false");
            entity.HasIndex(airline => airline.IataCode)
                .IsUnique()
                .HasFilter("\"IsDeleted\" = false");
            entity.HasIndex(airline => airline.IcaoCode)
                .IsUnique()
                .HasFilter("\"IsDeleted\" = false");
        });

        modelBuilder.Entity<RouteEntity>(entity =>
        {
            entity.ToTable("flight_routes");
            entity.HasKey(route => route.Id);
            entity.Property(route => route.OriginAirportCode).HasMaxLength(3).IsRequired();
            entity.Property(route => route.DestinationAirportCode).HasMaxLength(3).IsRequired();
            entity.Property(route => route.OriginCity).HasMaxLength(120).IsRequired();
            entity.Property(route => route.DestinationCity).HasMaxLength(120).IsRequired();
            entity.HasIndex(route => new
                {
                    route.OriginAirportCode,
                    route.DestinationAirportCode
                })
                .IsUnique()
                .HasFilter("\"IsDeleted\" = false");
        });

        modelBuilder.Entity<FlightEntity>(entity =>
        {
            entity.ToTable("flights");
            entity.HasKey(flight => flight.Id);
            entity.Property(flight => flight.FlightNumber).HasMaxLength(12).IsRequired();
            entity.Property(flight => flight.AircraftType).HasMaxLength(120);
            entity.Property(flight => flight.Description).HasMaxLength(1000);
            entity.Property(flight => flight.Status)
                .HasConversion<string>()
                .HasMaxLength(20)
                .IsRequired();
            entity.HasIndex(flight => new
                {
                    flight.AirlineId,
                    flight.FlightNumber
                })
                .IsUnique()
                .HasFilter("\"IsDeleted\" = false");
            entity.HasOne<Airline>()
                .WithMany()
                .HasForeignKey(flight => flight.AirlineId)
                .OnDelete(DeleteBehavior.Restrict);
            entity.HasOne<RouteEntity>()
                .WithMany()
                .HasForeignKey(flight => flight.RouteId)
                .OnDelete(DeleteBehavior.Restrict);
        });

        modelBuilder.Entity<FlightSchedule>(entity =>
        {
            entity.ToTable("flight_schedules");
            entity.HasKey(schedule => schedule.Id);
            entity.Property(schedule => schedule.OperatingDays).HasMaxLength(50).IsRequired();
            entity.Property(schedule => schedule.TimeZoneId).HasMaxLength(100).IsRequired();
            entity.HasIndex(schedule => new
                {
                    schedule.FlightId,
                    schedule.EffectiveFrom
                })
                .HasFilter("\"IsDeleted\" = false");
            entity.HasOne<FlightEntity>()
                .WithMany()
                .HasForeignKey(schedule => schedule.FlightId)
                .OnDelete(DeleteBehavior.Cascade);
        });

        modelBuilder.Entity<FlightClass>(entity =>
        {
            entity.ToTable("flight_classes");
            entity.HasKey(flightClass => flightClass.Id);
            entity.Property(flightClass => flightClass.Code).HasMaxLength(10).IsRequired();
            entity.Property(flightClass => flightClass.Name).HasMaxLength(120).IsRequired();
            entity.Property(flightClass => flightClass.Type)
                .HasConversion<string>()
                .HasMaxLength(30)
                .IsRequired();
            entity.Property(flightClass => flightClass.BasePrice).HasPrecision(18, 2);
            entity.Property(flightClass => flightClass.Currency).HasMaxLength(3).IsRequired();
            entity.Property(flightClass => flightClass.Status)
                .HasConversion<string>()
                .HasMaxLength(20)
                .IsRequired();
            entity.HasIndex(flightClass => new
                {
                    flightClass.FlightId,
                    flightClass.Code
                })
                .IsUnique()
                .HasFilter("\"IsDeleted\" = false");
            entity.HasOne<FlightEntity>()
                .WithMany()
                .HasForeignKey(flightClass => flightClass.FlightId)
                .OnDelete(DeleteBehavior.Cascade);
        });

        modelBuilder.Entity<FlightPolicy>(entity =>
        {
            entity.ToTable("flight_policies");
            entity.HasKey(policy => policy.Id);
            entity.Property(policy => policy.PolicyType).HasMaxLength(100).IsRequired();
            entity.Property(policy => policy.Title).HasMaxLength(200).IsRequired();
            entity.Property(policy => policy.Content).HasMaxLength(10000).IsRequired();
            entity.Property(policy => policy.BaggageAllowanceKg).HasPrecision(10, 2);
            entity.Property(policy => policy.ChangeFee).HasPrecision(18, 2);
            entity.Property(policy => policy.Conditions).HasMaxLength(5000);
            entity.HasIndex(policy => new
                {
                    policy.FlightId,
                    policy.PolicyType
                });
            entity.HasOne<FlightEntity>()
                .WithMany()
                .HasForeignKey(policy => policy.FlightId)
                .OnDelete(DeleteBehavior.Cascade);
        });
    }

    async Task<Airline?> IAirlineRepository.GetByIdAsync(
        Guid id,
        CancellationToken cancellationToken) =>
        await Airlines.SingleOrDefaultAsync(
            airline => airline.Id == id && !airline.IsDeleted,
            cancellationToken);

    async Task<IReadOnlyList<Airline>> IAirlineRepository.ListAsync(
        CancellationToken cancellationToken) =>
        await Airlines
            .Where(airline => !airline.IsDeleted)
            .OrderBy(airline => airline.Name)
            .ToListAsync(cancellationToken);

    Task<bool> IAirlineRepository.ExistsByNameAsync(
        string name,
        Guid? excludingId,
        CancellationToken cancellationToken)
    {
        var normalizedName = name.Trim().ToLower();
        return Airlines.AnyAsync(
            airline => !airline.IsDeleted
                && airline.Name.ToLower() == normalizedName
                && (!excludingId.HasValue || airline.Id != excludingId.Value),
            cancellationToken);
    }

    Task<bool> IAirlineRepository.ExistsByCodeAsync(
        string iataCode,
        string icaoCode,
        Guid? excludingId,
        CancellationToken cancellationToken)
    {
        var iata = iataCode.Trim().ToUpper();
        var icao = icaoCode.Trim().ToUpper();
        return Airlines.AnyAsync(
            airline => !airline.IsDeleted
                && (airline.IataCode == iata || airline.IcaoCode == icao)
                && (!excludingId.HasValue || airline.Id != excludingId.Value),
            cancellationToken);
    }

    public Task AddAsync(Airline airline, CancellationToken cancellationToken) =>
        Airlines.AddAsync(airline, cancellationToken).AsTask();

    async Task<RouteEntity?> IRouteRepository.GetByIdAsync(
        Guid id,
        CancellationToken cancellationToken) =>
        await Routes.SingleOrDefaultAsync(
            route => route.Id == id && !route.IsDeleted,
            cancellationToken);

    async Task<IReadOnlyList<RouteEntity>> IRouteRepository.ListAsync(
        CancellationToken cancellationToken) =>
        await Routes
            .Where(route => !route.IsDeleted)
            .OrderBy(route => route.OriginAirportCode)
            .ThenBy(route => route.DestinationAirportCode)
            .ToListAsync(cancellationToken);

    Task<bool> IRouteRepository.ExistsAsync(
        string originAirportCode,
        string destinationAirportCode,
        Guid? excludingId,
        CancellationToken cancellationToken)
    {
        var origin = originAirportCode.Trim().ToUpper();
        var destination = destinationAirportCode.Trim().ToUpper();
        return Routes.AnyAsync(
            route => !route.IsDeleted
                && route.OriginAirportCode == origin
                && route.DestinationAirportCode == destination
                && (!excludingId.HasValue || route.Id != excludingId.Value),
            cancellationToken);
    }

    public Task AddAsync(RouteEntity route, CancellationToken cancellationToken) =>
        Routes.AddAsync(route, cancellationToken).AsTask();

    async Task<FlightEntity?> IFlightRepository.GetByIdAsync(
        Guid id,
        CancellationToken cancellationToken) =>
        await Flights.SingleOrDefaultAsync(
            flight => flight.Id == id && !flight.IsDeleted,
            cancellationToken);

    Task<bool> IFlightRepository.ExistsByNumberAsync(
        Guid airlineId,
        string flightNumber,
        Guid? excludingId,
        CancellationToken cancellationToken)
    {
        var number = flightNumber.Trim().ToUpper();
        return Flights.AnyAsync(
            flight => !flight.IsDeleted
                && flight.AirlineId == airlineId
                && flight.FlightNumber == number
                && (!excludingId.HasValue || flight.Id != excludingId.Value),
            cancellationToken);
    }

    public Task AddAsync(FlightEntity flight, CancellationToken cancellationToken) =>
        Flights.AddAsync(flight, cancellationToken).AsTask();

    async Task<FlightSchedule?> IFlightScheduleRepository.GetByIdAsync(
        Guid id,
        CancellationToken cancellationToken) =>
        await FlightSchedules.SingleOrDefaultAsync(
            schedule => schedule.Id == id && !schedule.IsDeleted,
            cancellationToken);

    async Task<IReadOnlyList<FlightSchedule>> IFlightScheduleRepository.ListByFlightAsync(
        Guid flightId,
        CancellationToken cancellationToken) =>
        await FlightSchedules
            .Where(schedule => schedule.FlightId == flightId && !schedule.IsDeleted)
            .OrderBy(schedule => schedule.EffectiveFrom)
            .ThenBy(schedule => schedule.DepartureTime)
            .ToListAsync(cancellationToken);

    public Task AddAsync(FlightSchedule schedule, CancellationToken cancellationToken) =>
        FlightSchedules.AddAsync(schedule, cancellationToken).AsTask();

    public void Remove(FlightSchedule schedule) => FlightSchedules.Remove(schedule);

    async Task<FlightClass?> IFlightClassRepository.GetByIdAsync(
        Guid id,
        CancellationToken cancellationToken) =>
        await FlightClasses.SingleOrDefaultAsync(
            flightClass => flightClass.Id == id && !flightClass.IsDeleted,
            cancellationToken);

    async Task<IReadOnlyList<FlightClass>> IFlightClassRepository.ListByFlightAsync(
        Guid flightId,
        CancellationToken cancellationToken) =>
        await FlightClasses
            .Where(flightClass => flightClass.FlightId == flightId && !flightClass.IsDeleted)
            .OrderBy(flightClass => flightClass.Type)
            .ThenBy(flightClass => flightClass.Code)
            .ToListAsync(cancellationToken);

    Task<bool> IFlightClassRepository.ExistsByCodeAsync(
        Guid flightId,
        string code,
        Guid? excludingId,
        CancellationToken cancellationToken)
    {
        var normalizedCode = code.Trim().ToUpper();
        return FlightClasses.AnyAsync(
            flightClass => !flightClass.IsDeleted
                && flightClass.FlightId == flightId
                && flightClass.Code == normalizedCode
                && (!excludingId.HasValue || flightClass.Id != excludingId.Value),
            cancellationToken);
    }

    public Task AddAsync(FlightClass flightClass, CancellationToken cancellationToken) =>
        FlightClasses.AddAsync(flightClass, cancellationToken).AsTask();

    async Task<FlightPolicy?> IFlightPolicyRepository.GetByIdAsync(
        Guid id,
        CancellationToken cancellationToken) =>
        await FlightPolicies.SingleOrDefaultAsync(
            policy => policy.Id == id,
            cancellationToken);

    async Task<IReadOnlyList<FlightPolicy>> IFlightPolicyRepository.ListByFlightAsync(
        Guid flightId,
        CancellationToken cancellationToken) =>
        await FlightPolicies
            .Where(policy => policy.FlightId == flightId)
            .OrderBy(policy => policy.PolicyType)
            .ThenBy(policy => policy.Title)
            .ToListAsync(cancellationToken);

    public Task AddAsync(FlightPolicy policy, CancellationToken cancellationToken) =>
        FlightPolicies.AddAsync(policy, cancellationToken).AsTask();

    public void Remove(FlightPolicy policy) => FlightPolicies.Remove(policy);
}
