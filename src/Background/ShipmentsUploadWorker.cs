using Shipment.Database;
using Shipment.Features.Shipments.UploadShipments;

namespace Shipment.Background;

public sealed class ShipmentsUploadWorker(
    ILogger<ShipmentsUploadWorker> logger,
    IServiceScopeFactory serviceScope,
    UploadShipmentQueue queue) : BackgroundService
{
    protected override async Task ExecuteAsync(CancellationToken stoppingToken)
    {
        logger.LogInformation("Batch Uploading of Shipments is Starting");

        try
        {
            await foreach (var shipments in queue.Reader.ReadAllAsync())
            {

            }
        }
        catch (Exception)
        {

        }
    }

    private async Task SaveBatch()
    {
        using var service = serviceScope.CreateScope();
        var context = service.ServiceProvider.GetRequiredService<AppDbContext>();

        await context.Shipments.AddRangeAsync();
        await context.SaveChangesAsync();

        logger.LogInformation("Processing the Shipments");
    }
}