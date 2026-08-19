using Auth.Api.Application.Abstractions;
using Auth.Api.Domain.Entities;
using Microsoft.EntityFrameworkCore;

namespace Auth.Api.Infrastructure.Persistence;

public sealed class AppDbContext : DbContext,
    IUnitOfWork,
    IUserRepository,
    IRoleRepository,
    IPermissionRepository,
    IUserRoleRepository,
    IRolePermissionRepository,
    IPasswordResetTokenRepository
{
    public AppDbContext(DbContextOptions<AppDbContext> options)
        : base(options)
    {
    }

    public DbSet<User> Users => Set<User>();
    public DbSet<Role> Roles => Set<Role>();
    public DbSet<Permission> Permissions => Set<Permission>();
    public DbSet<UserRole> UserRoles => Set<UserRole>();
    public DbSet<RolePermission> RolePermissions => Set<RolePermission>();
    public DbSet<PasswordResetToken> PasswordResetTokens => Set<PasswordResetToken>();

    protected override void OnModelCreating(ModelBuilder modelBuilder)
    {
        modelBuilder.Entity<User>(entity =>
        {
            entity.ToTable("users");
            entity.HasKey(user => user.Id);
            entity.Property(user => user.Username).HasMaxLength(100).IsRequired();
            entity.Property(user => user.Email).HasMaxLength(320).IsRequired();
            entity.Property(user => user.FirstName).HasMaxLength(100);
            entity.Property(user => user.LastName).HasMaxLength(100);
            entity.Property(user => user.Status).HasConversion<string>().HasMaxLength(20);
            entity.HasIndex(user => user.Email).IsUnique();
            entity.HasIndex(user => user.Username).IsUnique();
        });

        modelBuilder.Entity<Role>(entity =>
        {
            entity.ToTable("roles");
            entity.HasKey(role => role.Id);
            entity.Property(role => role.Name).HasMaxLength(100).IsRequired();
            entity.Property(role => role.Description).HasMaxLength(500);
            entity.HasIndex(role => role.Name).IsUnique();
        });

        modelBuilder.Entity<Permission>(entity =>
        {
            entity.ToTable("permissions");
            entity.HasKey(permission => permission.Id);
            entity.Property(permission => permission.Code).HasMaxLength(150).IsRequired();
            entity.Property(permission => permission.Description).HasMaxLength(500);
            entity.HasIndex(permission => permission.Code).IsUnique();
        });

        modelBuilder.Entity<UserRole>(entity =>
        {
            entity.ToTable("user_roles");
            entity.HasKey(userRole => new { userRole.UserId, userRole.RoleId });
            entity.HasOne<User>().WithMany().HasForeignKey(userRole => userRole.UserId);
            entity.HasOne<Role>().WithMany().HasForeignKey(userRole => userRole.RoleId);
        });

        modelBuilder.Entity<RolePermission>(entity =>
        {
            entity.ToTable("role_permissions");
            entity.HasKey(rolePermission => new { rolePermission.RoleId, rolePermission.PermissionId });
            entity.HasOne<Role>().WithMany().HasForeignKey(rolePermission => rolePermission.RoleId);
            entity.HasOne<Permission>().WithMany().HasForeignKey(rolePermission => rolePermission.PermissionId);
        });

        modelBuilder.Entity<PasswordResetToken>(entity =>
        {
            entity.ToTable("password_reset_tokens");
            entity.HasKey(token => token.Id);
            entity.Property(token => token.TokenHash).HasMaxLength(128).IsRequired();
            entity.HasIndex(token => token.TokenHash).IsUnique();
            entity.HasOne<User>().WithMany().HasForeignKey(token => token.UserId);
        });
    }

    async Task<User?> IUserRepository.GetByIdAsync(
        Guid id,
        CancellationToken cancellationToken)
    {
        return await Users.SingleOrDefaultAsync(user => user.Id == id, cancellationToken);
    }

    public Task<User?> GetByEmailAsync(string email, CancellationToken cancellationToken)
    {
        var normalizedEmail = email.Trim().ToLower();
        return Users.SingleOrDefaultAsync(user => user.Email == normalizedEmail, cancellationToken);
    }

    public Task AddAsync(User user, CancellationToken cancellationToken)
    {
        return Users.AddAsync(user, cancellationToken).AsTask();
    }

    async Task<Role?> IRoleRepository.GetByIdAsync(
        Guid id,
        CancellationToken cancellationToken)
    {
        return await Roles.SingleOrDefaultAsync(role => role.Id == id && !role.IsDeleted, cancellationToken);
    }

    async Task<IReadOnlyList<Role>> IRoleRepository.ListAsync(
        CancellationToken cancellationToken)
    {
        return await Roles
            .Where(role => !role.IsDeleted)
            .OrderBy(role => role.Name)
            .ToListAsync(cancellationToken);
    }

    public Task AddAsync(Role role, CancellationToken cancellationToken)
    {
        return Roles.AddAsync(role, cancellationToken).AsTask();
    }

    async Task<Permission?> IPermissionRepository.GetByIdAsync(
        Guid id,
        CancellationToken cancellationToken)
    {
        return await Permissions.SingleOrDefaultAsync(
            permission => permission.Id == id && !permission.IsDeleted,
            cancellationToken);
    }

    async Task<IReadOnlyList<Permission>> IPermissionRepository.ListAsync(
        CancellationToken cancellationToken)
    {
        return await Permissions
            .Where(permission => !permission.IsDeleted)
            .OrderBy(permission => permission.Code)
            .ToListAsync(cancellationToken);
    }

    public Task AddAsync(Permission permission, CancellationToken cancellationToken)
    {
        return Permissions.AddAsync(permission, cancellationToken).AsTask();
    }

    Task<bool> IUserRoleRepository.ExistsAsync(
        Guid userId,
        Guid roleId,
        CancellationToken cancellationToken)
    {
        return UserRoles.AnyAsync(
            userRole => userRole.UserId == userId && userRole.RoleId == roleId,
            cancellationToken);
    }

    public Task AddAsync(UserRole userRole, CancellationToken cancellationToken)
    {
        return UserRoles.AddAsync(userRole, cancellationToken).AsTask();
    }

    async Task IUserRoleRepository.RemoveAsync(
        Guid userId,
        Guid roleId,
        CancellationToken cancellationToken)
    {
        var userRole = await UserRoles.FindAsync([userId, roleId], cancellationToken);
        if (userRole is not null)
        {
            UserRoles.Remove(userRole);
        }
    }

    public async Task<IReadOnlyList<Role>> GetRolesAsync(
        Guid userId,
        CancellationToken cancellationToken)
    {
        return await UserRoles
            .Where(userRole => userRole.UserId == userId)
            .Join(
                Roles.Where(role => !role.IsDeleted),
                userRole => userRole.RoleId,
                role => role.Id,
                (_, role) => role)
            .OrderBy(role => role.Name)
            .ToListAsync(cancellationToken);
    }

    Task<bool> IRolePermissionRepository.ExistsAsync(
        Guid roleId,
        Guid permissionId,
        CancellationToken cancellationToken)
    {
        return RolePermissions.AnyAsync(
            rolePermission => rolePermission.RoleId == roleId
                && rolePermission.PermissionId == permissionId,
            cancellationToken);
    }

    public Task AddAsync(RolePermission rolePermission, CancellationToken cancellationToken)
    {
        return RolePermissions.AddAsync(rolePermission, cancellationToken).AsTask();
    }

    async Task IRolePermissionRepository.RemoveAsync(
        Guid roleId,
        Guid permissionId,
        CancellationToken cancellationToken)
    {
        var rolePermission = await RolePermissions.FindAsync([roleId, permissionId], cancellationToken);
        if (rolePermission is not null)
        {
            RolePermissions.Remove(rolePermission);
        }
    }

    public Task AddAsync(PasswordResetToken token, CancellationToken cancellationToken)
    {
        return PasswordResetTokens.AddAsync(token, cancellationToken).AsTask();
    }

    public Task<PasswordResetToken?> GetByHashAsync(
        string tokenHash,
        CancellationToken cancellationToken)
    {
        return PasswordResetTokens
            .Where(token => token.TokenHash == tokenHash)
            .OrderByDescending(token => token.CreatedAtUtc)
            .FirstOrDefaultAsync(cancellationToken);
    }
}
