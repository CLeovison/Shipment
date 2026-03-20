using Microsoft.EntityFrameworkCore;
using Shipment.Database;

namespace Shipment.Background;

public sealed class RefreshTokenWorker(ILogger<RefreshTokenWorker> logger, IServiceScopeFactory serviceScope) : BackgroundService
{

    protected override async Task ExecuteAsync(CancellationToken stoppingToken)
    {
        logger.LogInformation("Checking Refresh Token Expires are starting to work");
        while (!stoppingToken.IsCancellationRequested)
        {

            try
            {
                await RemoveExpireRefreshToken(stoppingToken);
                logger.LogInformation("RefreshTokenWorker sleeping for 24 hours");
                await Task.Delay(TimeSpan.FromHours(24), stoppingToken);
            }
            catch (OperationCanceledException) when (stoppingToken.IsCancellationRequested)
            {
                logger.LogInformation("Refresh Token is stopping ...");
                break;
            }
            catch (Exception ex)
            {
                logger.LogError(ex, "Error occured while cleaning expired refresh token");
                await Task.Delay(TimeSpan.FromMinutes(5), stoppingToken);
            }

            logger.LogInformation("Checking Expired Refresh Token stops");
        }


    }
    private async Task RemoveExpireRefreshToken(CancellationToken ct)
    {
        using var scope = serviceScope.CreateScope();

        var db = scope.ServiceProvider.GetRequiredService<AppDbContext>();

        var now = DateTime.UtcNow;

        var deletedToken = await db.RefreshToken.Where(x => x.ExpiresAt <= now).ExecuteDeleteAsync(ct);

        if (deletedToken > 0)
        {
            logger.LogInformation("Deleted {Count} expired refresh token", deletedToken);
        }
        else
        {
            logger.LogDebug("No expired refresh tokens found");
        }
    }
}