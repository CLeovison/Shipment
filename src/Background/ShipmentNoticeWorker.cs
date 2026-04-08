using Microsoft.AspNetCore.SignalR;
using Microsoft.EntityFrameworkCore;
using Shipment.Database;
using Shipment.Entities;
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

        // ✅ FIX: Use PH time
        var now = DateTime.UtcNow.AddHours(8);
        var today = now.Date;

        var shipments = await db.Shipments
      .ForNotifications(now, NoticeDays, TimeSpan.FromMinutes(2))
      .ToListAsync(ct);

        foreach (var shipment in shipments)
        {
            shipment.LastNotifiedAt = now;

            if (shipment.TimeOfArrival.Date <= today)
            {
                shipment.IsCompleted = true;
            }

            var daysRemaining = (shipment.TimeOfArrival.Date - today).Days;

            await hub.Clients.User(shipment.UserId.ToString())
                .SendAsync(
                    "ShipmentArrivingSoon",
                    shipment.PurchaseOrderNumber,
                    shipment.Vendor,
                    shipment.TimeOfArrival,
                    daysRemaining,
                    ct);
        }

        await db.SaveChangesAsync(ct);
    }
}