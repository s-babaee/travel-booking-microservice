using Auth.Api.Domain.Common;
using Auth.Api.Domain.Entities;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace Auth.Api.Infrastructure.Persistence.Configurations
{
    public sealed class RolePermissionConfiguration : IEntityTypeConfiguration<RolePermission>
    {
        public void Configure(EntityTypeBuilder<RolePermission> entity)
        {
            entity.ToTable("role_permissions");
            entity.HasKey(rolePermission => new { rolePermission.RoleId, rolePermission.PermissionId });
            entity.HasOne<Role>().WithMany().HasForeignKey(rolePermission => rolePermission.RoleId);
            entity.HasOne<Permission>().WithMany().HasForeignKey(rolePermission => rolePermission.PermissionId);
        }
    }
}
