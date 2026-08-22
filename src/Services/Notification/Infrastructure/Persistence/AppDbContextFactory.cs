using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Design;

namespace Notification.Infrastructure.Persistence;

public sealed class AppDbContextFactory
    : IDesignTimeDbContextFactory<AppDbContext>
{
    public AppDbContext CreateDbContext(string[] args)
    {
        var connectionString =
            Environment.GetEnvironmentVariable(
                "ConnectionStrings__NotificationDb")
            ?? "Host=localhost;Port=5432;Database=notification_db;Username=notification_user";
        var optionsBuilder = new DbContextOptionsBuilder<AppDbContext>();
        optionsBuilder.UseNpgsql(connectionString);
        return new AppDbContext(optionsBuilder.Options);
    }
}
