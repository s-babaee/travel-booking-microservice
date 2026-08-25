using Auth.Api.Domain.Common;
using Auth.Api.Domain.Entities;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace Auth.Api.Infrastructure.Persistence.Configurations
{
    public sealed class UserRoleConfiguration : IEntityTypeConfiguration<UserRole>
    {
        public void Configure(EntityTypeBuilder<UserRole> entity)
        {
            entity.ToTable("user_roles");
            entity.HasKey(userRole => new { userRole.UserId, userRole.RoleId });
            entity.HasOne<User>().WithMany().HasForeignKey(userRole => userRole.UserId);
            entity.HasOne<Role>().WithMany().HasForeignKey(userRole => userRole.RoleId);
        }
    }
}
