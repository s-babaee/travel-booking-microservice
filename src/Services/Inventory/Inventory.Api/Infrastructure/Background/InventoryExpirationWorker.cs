using Inventory.Api.Application.Abstractions;

namespace Inventory.Api.Infrastructure.Background;

public sealed class InventoryExpirationWorker : BackgroundService
{
    private readonly IServiceScopeFactory _scopeFactory;
    private readonly TimeProvider _timeProvider;
    private readonly ILogger<InventoryExpirationWorker> _logger;

    public InventoryExpirationWorker(
        IServiceScopeFactory scopeFactory,
        TimeProvider timeProvider,
        ILogger<InventoryExpirationWorker> logger)
    {
        _scopeFactory = scopeFactory;
        _timeProvider = timeProvider;
        _logger = logger;
    }

    protected override async Task ExecuteAsync(
        CancellationToken stoppingToken)
    {
        using var timer = new PeriodicTimer(TimeSpan.FromSeconds(30));
        while (await timer.WaitForNextTickAsync(stoppingToken))
        {
            try
            {
                await using var scope = _scopeFactory.CreateAsyncScope();
                var nowUtc = _timeProvider.GetUtcNow().UtcDateTime;
                await scope.ServiceProvider
                    .GetRequiredService<IHotelInventoryService>()
                    .ExpireAsync(nowUtc, stoppingToken);
                await scope.ServiceProvider
                    .GetRequiredService<IFlightInventoryService>()
                    .ExpireAsync(nowUtc, stoppingToken);
            }
            catch (OperationCanceledException) when (stoppingToken.IsCancellationRequested)
            {
                return;
            }
            catch (Exception exception)
            {
                _logger.LogError(
                    exception,
                    "Failed to expire inventory holds.");
            }
        }
    }
}
