using Microsoft.AspNetCore.SignalR;
using Microsoft.EntityFrameworkCore;
using Shipment.Database;
using Shipment.Features.Shipments.Shared;
using Shipment.Hubs;

namespace Shipment.Background;

public class ShipmentNoticeWorker(
    ILogger<ShipmentNoticeWorker> logger,
    IServiceScopeFactory scopeFactory,
    IHubContext<ShipmentNotificationHub> hub)
    : BackgroundService
{
    private const int NoticeDays = 14;
    private static readonly TimeSpan Interval = TimeSpan.FromMinutes(2);

    protected override async Task ExecuteAsync(CancellationToken stoppingToken)
    {
        using var timer = new PeriodicTimer(Interval);

        while (await timer.WaitForNextTickAsync(stoppingToken))
        {
            try
            {
                await Process(stoppingToken);
            }
            catch (Exception ex)
            {
                logger.LogError(ex, "Worker error");
            }
        }
    }

    private async Task Process(CancellationToken ct)
    {
        using var scope = scopeFactory.CreateScope();
        var db = scope.ServiceProvider.GetRequiredService<AppDbContext>();

        var now = DateTime.UtcNow;
        var throttle = TimeSpan.FromMinutes(2);

        var shipment = await db.Shipments
            .ForNotifications(now, NoticeDays, throttle)
            .OrderBy(x => x.LastNotifiedAt ?? DateTime.MinValue)
            .ThenBy(x => x.TimeOfArrival)
            .FirstOrDefaultAsync(ct);

        if (shipment is null)
            return;

        // Persist FIRST (avoid duplicates)
        shipment.LastNotifiedAt = now;

        if (shipment.TimeOfArrival < now.Date)
            shipment.IsCompleted = true;

        await db.SaveChangesAsync(ct);

        var daysRemaining = (shipment.TimeOfArrival - now.Date).Days;

        await hub.Clients.User(shipment.UserId.ToString())
            .SendAsync(
                "ShipmentArrivingSoon",
                shipment.PurchaseOrderNumber,
                shipment.Vendor,
                shipment.TimeOfArrival,
                daysRemaining,
                ct);
    }
}