using Microsoft.EntityFrameworkCore;
using Shipment.Database;
using Shipment.Entities;
using Shipment.Features.Shipments.UploadShipments;

namespace Shipment.Background;

public sealed class ShipmentsUploadWorker(
    ILogger<ShipmentsUploadWorker> logger,
    IServiceScopeFactory serviceScope,
    UploadShipmentQueue queue) : BackgroundService
{
    private const int BatchSize = 1000;
    private static readonly TimeSpan FlushInterval = TimeSpan.FromSeconds(2);
    private const int MaxRetries = 3;

    protected override async Task ExecuteAsync(CancellationToken stoppingToken)
    {
        logger.LogInformation("Batch Uploading of Shipments is Starting");

        var buffer = new List<ShipmentDetails>(BatchSize);
        var lastFlush = DateTime.UtcNow;

        try
        {
            await foreach (var shipment in queue.Reader.ReadAllAsync(stoppingToken))
            {
                buffer.Add(shipment);

                var shouldFlushBySize = buffer.Count >= BatchSize;
                var shouldFlushByTime = DateTime.UtcNow - lastFlush >= FlushInterval;

                if (shouldFlushBySize || shouldFlushByTime)
                {
                    await ProcessBatchWithRetries(buffer, stoppingToken);
                    buffer.Clear();
                    lastFlush = DateTime.UtcNow;
                }
            }

            // Final flush on shutdown or channel completion
            if (buffer.Count > 0)
            {
                await ProcessBatchWithRetries(buffer, stoppingToken);
            }
        }
        catch (OperationCanceledException)
        {
            logger.LogInformation("Shipment upload worker is stopping...");
        }
        catch (Exception ex)
        {
            logger.LogError(ex, "Fatal error in shipment upload worker");
        }
    }

    private async Task ProcessBatchWithRetries(List<ShipmentDetails> batch, CancellationToken ct)
    {
        for (int attempt = 1; attempt <= MaxRetries; attempt++)
        {
            try
            {
                await SaveBatch(batch, ct);
                return;
            }
            catch (Exception ex)
            {
                logger.LogWarning(ex,
                    "Batch insert failed on attempt {Attempt}/{MaxRetries}",
                    attempt, MaxRetries);

                if (attempt == MaxRetries)
                {
                    logger.LogError(ex, "Batch permanently failed. Data loss risk.");
                    throw;
                }

                await Task.Delay(TimeSpan.FromMilliseconds(500 * attempt), ct);
            }
        }
    }

    private async Task SaveBatch(List<ShipmentDetails> batch, CancellationToken ct)
    {
        using var scope = serviceScope.CreateScope();
        var context = scope.ServiceProvider.GetRequiredService<AppDbContext>();

        try
        {
            await context.Shipments.AddRangeAsync(batch, ct);
            await context.SaveChangesAsync(ct);

            logger.LogInformation(
                "Inserted batch of {Count} shipments at {Time}",
                batch.Count,
                DateTime.UtcNow);
        }
        catch (DbUpdateException ex) when (IsUniqueConstraintViolation(ex))
        {
            logger.LogWarning(ex, "Duplicate detected. Resolving batch...");

            await HandleDuplicateBatch(context, batch, ct);
        }
    }

    private async Task HandleDuplicateBatch(
        AppDbContext context,
        List<ShipmentDetails> batch,
        CancellationToken ct)
    {
        // Extract idempotency keys
        var keys = batch
            .Select(x => x.PurchaseOrderNumber)
            .ToList();

        // Fetch existing keys from DB
        var existingKeys = await context.Shipments
            .Where(x => keys.Contains(x.PurchaseOrderNumber))
            .Select(x => x.PurchaseOrderNumber)
            .ToListAsync(ct);

        // Filter out duplicates
        var filtered = batch
            .Where(x => !existingKeys.Contains(x.PurchaseOrderNumber))
            .ToList();

        if (filtered.Count == 0)
        {
            logger.LogWarning("All records in batch are duplicates. Skipping batch.");
            return;
        }

        await context.Shipments.AddRangeAsync(filtered, ct);
        await context.SaveChangesAsync(ct);

        logger.LogInformation(
            "Inserted filtered batch of {Count} shipments (duplicates removed)",
            filtered.Count);
    }

    private static bool IsUniqueConstraintViolation(DbUpdateException ex)
    {
        var message = ex.InnerException?.Message ?? "";

        return message.Contains("UNIQUE", StringComparison.OrdinalIgnoreCase) ||
               message.Contains("duplicate", StringComparison.OrdinalIgnoreCase);
    }
}