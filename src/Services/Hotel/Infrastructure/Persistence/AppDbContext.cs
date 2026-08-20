using Hotel.Api.Application.Abstractions;
using Hotel.Api.Domain.Entities;
using Microsoft.EntityFrameworkCore;
using HotelEntity = Hotel.Api.Domain.Entities.Hotel;

namespace Hotel.Api.Infrastructure.Persistence;

public sealed class AppDbContext : DbContext,
    IUnitOfWork,
    IHotelRepository,
    IRoomTypeRepository,
    IAmenityRepository,
    IHotelAmenityRepository,
    IRoomTypeAmenityRepository,
    IHotelPolicyRepository,
    IHotelImageRepository,
    IRoomTypeImageRepository
{
    public AppDbContext(DbContextOptions<AppDbContext> options)
        : base(options)
    {
    }

    public DbSet<HotelEntity> Hotels => Set<HotelEntity>();
    public DbSet<RoomType> RoomTypes => Set<RoomType>();
    public DbSet<Amenity> Amenities => Set<Amenity>();
    public DbSet<HotelAmenity> HotelAmenities => Set<HotelAmenity>();
    public DbSet<RoomTypeAmenity> RoomTypeAmenities => Set<RoomTypeAmenity>();
    public DbSet<HotelPolicy> HotelPolicies => Set<HotelPolicy>();
    public DbSet<HotelImage> HotelImages => Set<HotelImage>();
    public DbSet<RoomTypeImage> RoomTypeImages => Set<RoomTypeImage>();

    protected override void OnModelCreating(ModelBuilder modelBuilder)
    {
        modelBuilder.Entity<HotelEntity>(entity =>
        {
            entity.ToTable("hotels");
            entity.HasKey(hotel => hotel.Id);
            entity.Property(hotel => hotel.Name)
                .HasMaxLength(200)
                .IsRequired();
            entity.Property(hotel => hotel.Description)
                .HasMaxLength(2000);
            entity.Property(hotel => hotel.StarRating)
                .IsRequired();
            entity.Property(hotel => hotel.AddressLine1)
                .HasMaxLength(300)
                .IsRequired();
            entity.Property(hotel => hotel.AddressLine2)
                .HasMaxLength(300);
            entity.Property(hotel => hotel.City)
                .HasMaxLength(120)
                .IsRequired();
            entity.Property(hotel => hotel.StateOrProvince)
                .HasMaxLength(120);
            entity.Property(hotel => hotel.Country)
                .HasMaxLength(120)
                .IsRequired();
            entity.Property(hotel => hotel.PostalCode)
                .HasMaxLength(30);
            entity.Property(hotel => hotel.PhoneNumber)
                .HasMaxLength(50);
            entity.Property(hotel => hotel.Email)
                .HasMaxLength(320);
            entity.Property(hotel => hotel.WebsiteUrl)
                .HasMaxLength(500);
            entity.Property(hotel => hotel.Latitude)
                .HasPrecision(9, 6);
            entity.Property(hotel => hotel.Longitude)
                .HasPrecision(9, 6);
            entity.Property(hotel => hotel.Status)
                .HasConversion<string>()
                .HasMaxLength(20)
                .IsRequired();
            entity.HasIndex(hotel => hotel.Name)
                .IsUnique()
                .HasFilter("\"IsDeleted\" = false");
            entity.HasIndex(hotel => hotel.City);
            entity.HasIndex(hotel => hotel.Country);
        });

        modelBuilder.Entity<RoomType>(entity =>
        {
            entity.ToTable("room_types");
            entity.HasKey(roomType => roomType.Id);
            entity.Property(roomType => roomType.Name)
                .HasMaxLength(200)
                .IsRequired();
            entity.Property(roomType => roomType.Description)
                .HasMaxLength(2000);
            entity.Property(roomType => roomType.BedType)
                .HasMaxLength(120);
            entity.Property(roomType => roomType.SizeInSquareMeters)
                .HasPrecision(10, 2);
            entity.Property(roomType => roomType.View)
                .HasMaxLength(120);
            entity.Property(roomType => roomType.Status)
                .HasConversion<string>()
                .HasMaxLength(20)
                .IsRequired();
            entity.HasIndex(roomType => new
                {
                    roomType.HotelId,
                    roomType.Name
                })
                .IsUnique()
                .HasFilter("\"IsDeleted\" = false");
            entity.HasOne<HotelEntity>()
                .WithMany()
                .HasForeignKey(roomType => roomType.HotelId)
                .OnDelete(DeleteBehavior.Restrict);
        });

        modelBuilder.Entity<Amenity>(entity =>
        {
            entity.ToTable("amenities");
            entity.HasKey(amenity => amenity.Id);
            entity.Property(amenity => amenity.Name)
                .HasMaxLength(150)
                .IsRequired();
            entity.Property(amenity => amenity.Type)
                .HasConversion<string>()
                .HasMaxLength(50)
                .IsRequired();
            entity.Property(amenity => amenity.Description)
                .HasMaxLength(500);
            entity.HasIndex(amenity => amenity.Name)
                .IsUnique()
                .HasFilter("\"IsDeleted\" = false");
        });

        modelBuilder.Entity<HotelAmenity>(entity =>
        {
            entity.ToTable("hotel_amenities");
            entity.HasKey(assignment => new
            {
                assignment.HotelId,
                assignment.AmenityId
            });
            entity.HasOne<HotelEntity>()
                .WithMany()
                .HasForeignKey(assignment => assignment.HotelId)
                .OnDelete(DeleteBehavior.Cascade);
            entity.HasOne<Amenity>()
                .WithMany()
                .HasForeignKey(assignment => assignment.AmenityId)
                .OnDelete(DeleteBehavior.Cascade);
        });

        modelBuilder.Entity<RoomTypeAmenity>(entity =>
        {
            entity.ToTable("room_type_amenities");
            entity.HasKey(assignment => new
            {
                assignment.RoomTypeId,
                assignment.AmenityId
            });
            entity.HasOne<RoomType>()
                .WithMany()
                .HasForeignKey(assignment => assignment.RoomTypeId)
                .OnDelete(DeleteBehavior.Cascade);
            entity.HasOne<Amenity>()
                .WithMany()
                .HasForeignKey(assignment => assignment.AmenityId)
                .OnDelete(DeleteBehavior.Cascade);
        });

        modelBuilder.Entity<HotelPolicy>(entity =>
        {
            entity.ToTable("hotel_policies");
            entity.HasKey(policy => policy.Id);
            entity.Property(policy => policy.PolicyType)
                .HasMaxLength(100)
                .IsRequired();
            entity.Property(policy => policy.Title)
                .HasMaxLength(200)
                .IsRequired();
            entity.Property(policy => policy.Content)
                .HasMaxLength(5000)
                .IsRequired();
            entity.Property(policy => policy.Conditions)
                .HasMaxLength(5000);
            entity.HasIndex(policy => new
            {
                policy.HotelId,
                policy.PolicyType
            });
            entity.HasOne<HotelEntity>()
                .WithMany()
                .HasForeignKey(policy => policy.HotelId)
                .OnDelete(DeleteBehavior.Cascade);
        });

        modelBuilder.Entity<HotelImage>(entity =>
        {
            entity.ToTable("hotel_images");
            entity.HasKey(image => image.Id);
            entity.Property(image => image.Url)
                .HasMaxLength(1000)
                .IsRequired();
            entity.Property(image => image.AltText)
                .HasMaxLength(300);
            entity.HasIndex(image => new
            {
                image.HotelId,
                image.DisplayOrder
            });
            entity.HasOne<HotelEntity>()
                .WithMany()
                .HasForeignKey(image => image.HotelId)
                .OnDelete(DeleteBehavior.Cascade);
        });

        modelBuilder.Entity<RoomTypeImage>(entity =>
        {
            entity.ToTable("room_type_images");
            entity.HasKey(image => image.Id);
            entity.Property(image => image.Url)
                .HasMaxLength(1000)
                .IsRequired();
            entity.Property(image => image.AltText)
                .HasMaxLength(300);
            entity.HasIndex(image => new
            {
                image.RoomTypeId,
                image.DisplayOrder
            });
            entity.HasOne<RoomType>()
                .WithMany()
                .HasForeignKey(image => image.RoomTypeId)
                .OnDelete(DeleteBehavior.Cascade);
        });
    }

    async Task<HotelEntity?> IHotelRepository.GetByIdAsync(
        Guid id,
        CancellationToken cancellationToken)
    {
        return await Hotels.SingleOrDefaultAsync(
            hotel => hotel.Id == id && !hotel.IsDeleted,
            cancellationToken);
    }

    Task<bool> IHotelRepository.ExistsByNameAsync(
        string name,
        Guid? excludingId,
        CancellationToken cancellationToken)
    {
        var normalizedName = name.Trim().ToLower();
        return Hotels.AnyAsync(
            hotel => !hotel.IsDeleted
                && hotel.Name.ToLower() == normalizedName
                && (!excludingId.HasValue || hotel.Id != excludingId.Value),
            cancellationToken);
    }

    public Task AddAsync(
        HotelEntity hotel,
        CancellationToken cancellationToken)
    {
        return Hotels.AddAsync(hotel, cancellationToken).AsTask();
    }

    async Task<RoomType?> IRoomTypeRepository.GetByIdAsync(
        Guid id,
        CancellationToken cancellationToken)
    {
        return await RoomTypes.SingleOrDefaultAsync(
            roomType => roomType.Id == id
                && !roomType.IsDeleted
                && Hotels.Any(hotel =>
                    hotel.Id == roomType.HotelId
                    && !hotel.IsDeleted),
            cancellationToken);
    }

    async Task<IReadOnlyList<RoomType>> IRoomTypeRepository.ListByHotelAsync(
        Guid hotelId,
        CancellationToken cancellationToken)
    {
        return await RoomTypes
            .Where(roomType =>
                roomType.HotelId == hotelId
                && !roomType.IsDeleted
                && Hotels.Any(hotel =>
                    hotel.Id == roomType.HotelId
                    && !hotel.IsDeleted))
            .OrderBy(roomType => roomType.Name)
            .ToListAsync(cancellationToken);
    }

    Task<bool> IRoomTypeRepository.ExistsByNameAsync(
        Guid hotelId,
        string name,
        Guid? excludingId,
        CancellationToken cancellationToken)
    {
        var normalizedName = name.Trim().ToLower();
        return RoomTypes.AnyAsync(
            roomType => roomType.HotelId == hotelId
                && !roomType.IsDeleted
                && roomType.Name.ToLower() == normalizedName
                && (!excludingId.HasValue
                    || roomType.Id != excludingId.Value),
            cancellationToken);
    }

    public Task AddAsync(
        RoomType roomType,
        CancellationToken cancellationToken)
    {
        return RoomTypes.AddAsync(roomType, cancellationToken).AsTask();
    }

    async Task<Amenity?> IAmenityRepository.GetByIdAsync(
        Guid id,
        CancellationToken cancellationToken)
    {
        return await Amenities.SingleOrDefaultAsync(
            amenity => amenity.Id == id && !amenity.IsDeleted,
            cancellationToken);
    }

    async Task<IReadOnlyList<Amenity>> IAmenityRepository.ListAsync(
        CancellationToken cancellationToken)
    {
        return await Amenities
            .Where(amenity => !amenity.IsDeleted)
            .OrderBy(amenity => amenity.Name)
            .ToListAsync(cancellationToken);
    }

    Task<bool> IAmenityRepository.ExistsByNameAsync(
        string name,
        Guid? excludingId,
        CancellationToken cancellationToken)
    {
        var normalizedName = name.Trim().ToLower();
        return Amenities.AnyAsync(
            amenity => !amenity.IsDeleted
                && amenity.Name.ToLower() == normalizedName
                && (!excludingId.HasValue
                    || amenity.Id != excludingId.Value),
            cancellationToken);
    }

    public Task AddAsync(
        Amenity amenity,
        CancellationToken cancellationToken)
    {
        return Amenities.AddAsync(amenity, cancellationToken).AsTask();
    }

    async Task<bool> IHotelAmenityRepository.ExistsAsync(
        Guid hotelId,
        Guid amenityId,
        CancellationToken cancellationToken)
    {
        return await HotelAmenities.AnyAsync(
            assignment => assignment.HotelId == hotelId
                && assignment.AmenityId == amenityId,
            cancellationToken);
    }

    async Task<IReadOnlyList<Amenity>> IHotelAmenityRepository
        .ListAmenitiesAsync(
            Guid hotelId,
            CancellationToken cancellationToken)
    {
        return await HotelAmenities
            .Where(assignment => assignment.HotelId == hotelId)
            .Join(
                Amenities.Where(amenity => !amenity.IsDeleted),
                assignment => assignment.AmenityId,
                amenity => amenity.Id,
                (_, amenity) => amenity)
            .OrderBy(amenity => amenity.Name)
            .ToListAsync(cancellationToken);
    }

    public Task AddAsync(
        HotelAmenity hotelAmenity,
        CancellationToken cancellationToken)
    {
        return HotelAmenities.AddAsync(
            hotelAmenity,
            cancellationToken).AsTask();
    }

    async Task IHotelAmenityRepository.RemoveAsync(
        Guid hotelId,
        Guid amenityId,
        CancellationToken cancellationToken)
    {
        var assignment = await HotelAmenities.FindAsync(
            new object?[] { hotelId, amenityId },
            cancellationToken);
        if (assignment is not null)
        {
            HotelAmenities.Remove(assignment);
        }
    }

    async Task<bool> IRoomTypeAmenityRepository.ExistsAsync(
        Guid roomTypeId,
        Guid amenityId,
        CancellationToken cancellationToken)
    {
        return await RoomTypeAmenities.AnyAsync(
            assignment => assignment.RoomTypeId == roomTypeId
                && assignment.AmenityId == amenityId,
            cancellationToken);
    }

    async Task<IReadOnlyList<Amenity>> IRoomTypeAmenityRepository
        .ListAmenitiesAsync(
            Guid roomTypeId,
            CancellationToken cancellationToken)
    {
        return await RoomTypeAmenities
            .Where(assignment => assignment.RoomTypeId == roomTypeId)
            .Join(
                Amenities.Where(amenity => !amenity.IsDeleted),
                assignment => assignment.AmenityId,
                amenity => amenity.Id,
                (_, amenity) => amenity)
            .OrderBy(amenity => amenity.Name)
            .ToListAsync(cancellationToken);
    }

    public Task AddAsync(
        RoomTypeAmenity roomTypeAmenity,
        CancellationToken cancellationToken)
    {
        return RoomTypeAmenities.AddAsync(
            roomTypeAmenity,
            cancellationToken).AsTask();
    }

    async Task IRoomTypeAmenityRepository.RemoveAsync(
        Guid roomTypeId,
        Guid amenityId,
        CancellationToken cancellationToken)
    {
        var assignment = await RoomTypeAmenities.FindAsync(
            new object?[] { roomTypeId, amenityId },
            cancellationToken);
        if (assignment is not null)
        {
            RoomTypeAmenities.Remove(assignment);
        }
    }

    async Task<HotelPolicy?> IHotelPolicyRepository.GetByIdAsync(
        Guid hotelId,
        Guid policyId,
        CancellationToken cancellationToken)
    {
        return await HotelPolicies.SingleOrDefaultAsync(
            policy => policy.HotelId == hotelId
                && policy.Id == policyId,
            cancellationToken);
    }

    async Task<IReadOnlyList<HotelPolicy>> IHotelPolicyRepository
        .ListByHotelAsync(
            Guid hotelId,
            CancellationToken cancellationToken)
    {
        return await HotelPolicies
            .Where(policy => policy.HotelId == hotelId)
            .OrderBy(policy => policy.PolicyType)
            .ThenBy(policy => policy.Title)
            .ToListAsync(cancellationToken);
    }

    public Task AddAsync(
        HotelPolicy policy,
        CancellationToken cancellationToken)
    {
        return HotelPolicies.AddAsync(policy, cancellationToken).AsTask();
    }

    public void Remove(HotelPolicy policy)
    {
        HotelPolicies.Remove(policy);
    }

    async Task<HotelImage?> IHotelImageRepository.GetByIdAsync(
        Guid hotelId,
        Guid imageId,
        CancellationToken cancellationToken)
    {
        return await HotelImages.SingleOrDefaultAsync(
            image => image.HotelId == hotelId && image.Id == imageId,
            cancellationToken);
    }

    async Task<IReadOnlyList<HotelImage>> IHotelImageRepository
        .ListByHotelAsync(
            Guid hotelId,
            CancellationToken cancellationToken)
    {
        return await HotelImages
            .Where(image => image.HotelId == hotelId)
            .OrderBy(image => image.DisplayOrder)
            .ThenBy(image => image.CreatedAtUtc)
            .ToListAsync(cancellationToken);
    }

    async Task<IReadOnlyList<HotelImage>> IHotelImageRepository
        .ListPrimaryCandidatesAsync(
            Guid hotelId,
            CancellationToken cancellationToken)
    {
        return await HotelImages
            .Where(image =>
                image.HotelId == hotelId
                && image.IsPrimary)
            .ToListAsync(cancellationToken);
    }

    public Task AddAsync(
        HotelImage image,
        CancellationToken cancellationToken)
    {
        return HotelImages.AddAsync(image, cancellationToken).AsTask();
    }

    public void Remove(HotelImage image)
    {
        HotelImages.Remove(image);
    }

    async Task<RoomTypeImage?> IRoomTypeImageRepository.GetByIdAsync(
        Guid roomTypeId,
        Guid imageId,
        CancellationToken cancellationToken)
    {
        return await RoomTypeImages.SingleOrDefaultAsync(
            image => image.RoomTypeId == roomTypeId
                && image.Id == imageId,
            cancellationToken);
    }

    async Task<IReadOnlyList<RoomTypeImage>> IRoomTypeImageRepository
        .ListByRoomTypeAsync(
            Guid roomTypeId,
            CancellationToken cancellationToken)
    {
        return await RoomTypeImages
            .Where(image => image.RoomTypeId == roomTypeId)
            .OrderBy(image => image.DisplayOrder)
            .ThenBy(image => image.CreatedAtUtc)
            .ToListAsync(cancellationToken);
    }

    async Task<IReadOnlyList<RoomTypeImage>> IRoomTypeImageRepository
        .ListPrimaryCandidatesAsync(
            Guid roomTypeId,
            CancellationToken cancellationToken)
    {
        return await RoomTypeImages
            .Where(image =>
                image.RoomTypeId == roomTypeId
                && image.IsPrimary)
            .ToListAsync(cancellationToken);
    }

    public Task AddAsync(
        RoomTypeImage image,
        CancellationToken cancellationToken)
    {
        return RoomTypeImages.AddAsync(image, cancellationToken).AsTask();
    }

    public void Remove(RoomTypeImage image)
    {
        RoomTypeImages.Remove(image);
    }
}
