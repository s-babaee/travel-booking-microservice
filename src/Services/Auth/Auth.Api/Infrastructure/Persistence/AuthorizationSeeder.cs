using Auth.Api.Domain.Entities;
using Auth.Api.Application.Abstractions;
using BuildingBlocks.Authorization;
using Microsoft.EntityFrameworkCore;

namespace Auth.Api.Infrastructure.Persistence;

public static class AuthorizationSeeder
{
    private static readonly IReadOnlyDictionary<string, string[]> RolePermissions =
        new Dictionary<string, string[]>(StringComparer.OrdinalIgnoreCase)
        {
            [RoleCatalog.Customer] =
            [
                PermissionCatalog.BookingsCreate,
                PermissionCatalog.BookingsReadOwn,
                PermissionCatalog.BookingsCancelOwn,
                PermissionCatalog.HotelsView,
                PermissionCatalog.FlightsView,
                PermissionCatalog.PaymentsInitiate,
                PermissionCatalog.PaymentsViewOwn,
                PermissionCatalog.ReviewsRead,
                PermissionCatalog.ReviewsCreate,
                PermissionCatalog.ProfileReadOwn,
                PermissionCatalog.ProfileUpdateOwn,
                PermissionCatalog.NotificationsReadOwn,
                PermissionCatalog.SearchRead
            ],
            [RoleCatalog.Support] =
            [
                PermissionCatalog.BookingsCreate,
                PermissionCatalog.BookingsReadOwn,
                PermissionCatalog.BookingsCancelOwn,
                PermissionCatalog.BookingsReadAll,
                PermissionCatalog.BookingsCancelAny,
                PermissionCatalog.HotelsView,
                PermissionCatalog.PaymentsInitiate,
                PermissionCatalog.PaymentsViewOwn,
                PermissionCatalog.PaymentsViewAll,
                PermissionCatalog.ReviewsRead,
                PermissionCatalog.ReviewsModerate,
                PermissionCatalog.FlightsView,
                PermissionCatalog.UsersReadAll,
                PermissionCatalog.ProfileReadOwn,
                PermissionCatalog.ProfileUpdateOwn,
                PermissionCatalog.NotificationsReadOwn,
                PermissionCatalog.SearchRead
            ],
            [RoleCatalog.HotelOwner] =
            [
                PermissionCatalog.HotelsView,
                PermissionCatalog.HotelsCreate,
                PermissionCatalog.HotelsUpdate,
                PermissionCatalog.HotelsDelete,
                PermissionCatalog.HotelsInventoryManage,
                PermissionCatalog.BookingsReadAll,
                PermissionCatalog.ReviewsRead,
                PermissionCatalog.ProfileReadOwn,
                PermissionCatalog.ProfileUpdateOwn,
                PermissionCatalog.NotificationsReadOwn,
                PermissionCatalog.SearchRead
            ],
            [RoleCatalog.Admin] = PermissionCatalog.All.ToArray()
        };

    public static async Task SeedAsync(
        AppDbContext db,
        IIdentityProvider identityProvider,
        CancellationToken cancellationToken)
    {
        var now = DateTime.UtcNow;
        var permissions = new Dictionary<string, Permission>(
            StringComparer.OrdinalIgnoreCase);

        foreach (var code in PermissionCatalog.All)
        {
            var permission = await db.Permissions.SingleOrDefaultAsync(
                item => item.Code == code,
                cancellationToken);
            if (permission is null)
            {
                permission = Permission.Create(
                    StableGuid($"permission:{code}"),
                    code,
                    $"Permission: {code}",
                    now);
                await db.Permissions.AddAsync(permission, cancellationToken);
            }

            permissions[code] = permission;
        }

        var roles = new Dictionary<string, Role>(
            StringComparer.OrdinalIgnoreCase);
        foreach (var roleName in RolePermissions.Keys)
        {
            var role = await db.Roles.SingleOrDefaultAsync(
                item => item.Name == roleName,
                cancellationToken);
            if (role is null)
            {
                role = Role.Create(
                    StableGuid($"role:{roleName}"),
                    roleName,
                    $"{roleName} role",
                    now);
                await db.Roles.AddAsync(role, cancellationToken);
            }

            roles[roleName] = role;
        }

        await db.SaveChangesAsync(cancellationToken);

        foreach (var permission in permissions.Values)
        {
            await identityProvider.CreatePermissionAsync(
                permission,
                cancellationToken);
        }

        foreach (var role in roles.Values)
        {
            await identityProvider.CreateRoleAsync(
                role,
                cancellationToken);
        }

        foreach (var (roleName, permissionCodes) in RolePermissions)
        {
            foreach (var permissionCode in permissionCodes)
            {
                var exists = await db.RolePermissions.AnyAsync(
                    item => item.RoleId == roles[roleName].Id
                        && item.PermissionId == permissions[permissionCode].Id,
                    cancellationToken);
                if (!exists)
                {
                    await db.RolePermissions.AddAsync(
                        new RolePermission(
                            roles[roleName].Id,
                            permissions[permissionCode].Id),
                        cancellationToken);
                }

                await identityProvider.AssignPermissionToRoleAsync(
                    roles[roleName].Name,
                    permissions[permissionCode].Code,
                    cancellationToken);
            }
        }

        await db.SaveChangesAsync(cancellationToken);
    }

    private static Guid StableGuid(string value)
    {
        var bytes = System.Security.Cryptography.MD5.HashData(
            System.Text.Encoding.UTF8.GetBytes(value));
        return new Guid(bytes);
    }
}
