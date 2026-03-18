using Microsoft.AspNetCore.SignalR;
using Microsoft.EntityFrameworkCore;
using Shipment.Database;
using Shipment.Hubs;

namespace Shipment.Background;

public class ShipmentNoticeWorker(ILogger<ShipmentNoticeWorker> logger,
IServiceScopeFactory serviceScope,
IHubContext<ShipmentNotificationHub> hub) : BackgroundService
{
    protected override async Task ExecuteAsync(CancellationToken stoppingToken)
    {
        logger.LogInformation("Shipment notice worker is started");

        while (!stoppingToken.IsCancellationRequested)
        {
            await ShipmentNotifiation(stoppingToken);

            await Task.Delay(TimeSpan.FromHours(1), stoppingToken);
        }
    }

    private async Task ShipmentNotifiation(CancellationToken ct)
    {
        using var scope = serviceScope.CreateScope();
        var db = scope.ServiceProvider.GetRequiredService<AppDbContext>();

        var threshold = DateTime.UtcNow.AddDays(14);

        var shipments = await db.Shipments.Where(x => !x.IsNotified && x.TimeOfArrival <= threshold).ToListAsync();

        foreach (var shipment in shipments)
        {
            logger.LogInformation($"{shipment.PurchaseOrderNumber} will be arriving with {shipments}", shipment.Vendor);

            await hub.Clients.User(shipment.UserId.ToString()).SendAsync(
            "ShipmentArrivingSoon",
            shipment.PurchaseOrderNumber,
            shipment.Vendor,
            shipment.TimeOfArrival);

            shipment.IsNotified = true;
        }
        await db.SaveChangesAsync(ct);
    }
}