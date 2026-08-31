using Microsoft.EntityFrameworkCore;

namespace Auth.Api.Infrastructure.Persistence;

public static class DatabaseInitializer
{
    public static async Task InitializeAsync(
        IServiceProvider services,
        CancellationToken cancellationToken = default)
    {
        await using var scope = services.CreateAsyncScope();
        var dbContext = scope.ServiceProvider.GetRequiredService<AppDbContext>();
        await dbContext.Database.MigrateAsync(cancellationToken);
        await AuthorizationSeeder.SeedAsync(
            dbContext,
            scope.ServiceProvider.GetRequiredService<
                Application.Abstractions.IIdentityProvider>(),
            cancellationToken);
    }
}
