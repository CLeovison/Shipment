using Microsoft.AspNetCore.SignalR;
using Microsoft.EntityFrameworkCore;
using Shipment.Database;
using Shipment.Entities.Shared;
using Shipment.Hubs;

namespace Shipment.Background;

public class ShipmentNoticeWorker(
    ILogger<ShipmentNoticeWorker> logger,
    IServiceScopeFactory serviceScope,
    IHubContext<ShipmentNotificationHub> hub)
    : BackgroundService
{
    private const int NoticeDays = 14;

    protected override async Task ExecuteAsync(CancellationToken stoppingToken)
    {
        logger.LogInformation("Shipment notice worker started");

        while (!stoppingToken.IsCancellationRequested)
        {
            try
            {
                await ProcessShipmentNotifications(stoppingToken);
            }
            catch (Exception ex)
            {
                logger.LogError(ex, "Error processing shipment notifications");
            }

            await Task.Delay(TimeSpan.FromDays(1), stoppingToken);
        }
    }

    private async Task ProcessShipmentNotifications(CancellationToken ct)
    {
        using var scope = serviceScope.CreateScope();
        var db = scope.ServiceProvider.GetRequiredService<AppDbContext>();

        var today = DateTime.UtcNow.Date;

        var shipments = await db.Shipments
            .ForNotifications(today, NoticeDays)
            .ToListAsync(ct);

        if (shipments.Count == 0)
        {
            logger.LogInformation("No shipment notifications for {Today}", today);
            return;
        }

        logger.LogInformation("Worker processing {Count} shipments", shipments.Count);

        foreach (var shipment in shipments)
        {
            var daysRemaining = (shipment.TimeOfArrival.Date - today).Days;

            await hub.Clients.User(shipment.UserId.ToString())
                .SendAsync(
                    "ShipmentArrivingSoon",
                    shipment.PurchaseOrderNumber,
                    shipment.Vendor,
                    shipment.TimeOfArrival,
                    daysRemaining,
                    ct);

            shipment.LastNotifiedAt = DateTime.UtcNow;

            if (shipment.TimeOfArrival.Date < today)
                shipment.IsCompleted = true;
        }

        await db.SaveChangesAsync(ct);
    }
}