using Microsoft.AspNetCore.SignalR;
using Microsoft.EntityFrameworkCore;
using Shipment.Database;
using Shipment.Hubs;

namespace Shipment.Background;

public class ShipmentNoticeWorker(
    ILogger<ShipmentNoticeWorker> logger,
    IServiceScopeFactory serviceScope,
    IHubContext<ShipmentNotificationHub> hub) : BackgroundService
{
    protected override async Task ExecuteAsync(CancellationToken stoppingToken)
    {
        logger.LogInformation("Shipment notice worker started");

        while (!stoppingToken.IsCancellationRequested)
        {
            try
            {
                await ProcessShipmentNotifications(stoppingToken);
            }
            catch (OperationCanceledException) when (stoppingToken.IsCancellationRequested)
            {
                logger.LogInformation("Shipment notice worker is stopping.");
                break;
            }
            catch (Exception ex)
            {
                logger.LogError(ex, "Unexpected error while processing shipment notifications.");
            }

            await Task.Delay(TimeSpan.FromHours(1), stoppingToken);
        }
    }

    private async Task ProcessShipmentNotifications(CancellationToken ct)
    {
        using var scope = serviceScope.CreateScope();

        var db = scope.ServiceProvider.GetRequiredService<AppDbContext>();

        var threshold = DateTime.UtcNow.AddDays(14);

        var shipments = await db.Shipments
            .Where(x => !x.IsNotified && x.TimeOfArrival <= threshold)
            .ToListAsync(ct);

        if (shipments.Count == 0)
        {
            logger.LogInformation("No shipment notifications to process.");
            return;
        }

        foreach (var shipment in shipments)
        {
            logger.LogInformation(
                "Shipment {PurchaseOrderNumber} from {Vendor} arriving at {ArrivalDate}",
                shipment.PurchaseOrderNumber,
                shipment.Vendor,
                shipment.TimeOfArrival);

            await hub.Clients.User(shipment.UserId.ToString())
                .SendAsync(
                    "ShipmentArrivingSoon",
                    shipment.PurchaseOrderNumber,
                    shipment.Vendor,
                    shipment.TimeOfArrival,
                    ct);

            shipment.IsNotified = true;
        }

        await db.SaveChangesAsync(ct);

        logger.LogInformation("Processed {Count} shipment notifications.", shipments.Count);
    }
}