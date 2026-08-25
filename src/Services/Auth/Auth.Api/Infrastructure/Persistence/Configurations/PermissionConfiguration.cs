using Auth.Api.Domain.Common;
using Auth.Api.Domain.Entities;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace Auth.Api.Infrastructure.Persistence.Configurations
{
    public sealed class PermissionConfiguration : IEntityTypeConfiguration<Permission>
    {
        public void Configure(EntityTypeBuilder<Permission> entity)
        {
            entity.ToTable("permissions");
            entity.HasKey(permission => permission.Id);
            entity.Property(permission => permission.Code).HasMaxLength(150).IsRequired();
            entity.Property(permission => permission.Description).HasMaxLength(500);
            entity.HasIndex(permission => permission.Code).IsUnique();
        }
    }
}
