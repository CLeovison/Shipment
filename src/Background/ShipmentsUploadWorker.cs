using Microsoft.EntityFrameworkCore;
using Shipment.Database;
using Shipment.Entities;
using Shipment.Features.Shipments.UploadShipments;

namespace Shipment.Background;

// BackgroundService = runs continuously in ASP.NET Core as a hosted worker
public sealed class ShipmentsUploadWorker(
    ILogger<ShipmentsUploadWorker> logger,        // Logging for observability (VERY important in async systems)
    IServiceScopeFactory serviceScope,            // Used to create scoped services (DbContext per batch)
    UploadShipmentQueue queue)                    // The queue where producers push shipment data
    : BackgroundService
{
    // Max number of records per batch before flushing to DB
    private const int BatchSize = 1000;

    // Max time to wait before flushing even if batch is not full
    private static readonly TimeSpan FlushInterval = TimeSpan.FromSeconds(2);

    // Retry attempts for transient failures (e.g. DB hiccups)
    private const int MaxRetries = 3;

    protected override async Task ExecuteAsync(CancellationToken stoppingToken)
    {
        logger.LogInformation("Batch Uploading of Shipments is Starting");

        // Buffer = temporary in-memory storage before committing to DB
        // This improves performance vs inserting one-by-one
        var buffer = new List<ShipmentDetails>(BatchSize);

        // Tracks last time we flushed to DB
        var lastFlush = DateTime.UtcNow;

        try
        {
            // Continuously read from queue until shutdown
            await foreach (var shipment in queue.Reader.ReadAllAsync(stoppingToken))
            {
                // Add incoming item to buffer
                buffer.Add(shipment);

                // Flush conditions:
                // 1. Buffer is full
                var shouldFlushBySize = buffer.Count >= BatchSize;

                // 2. Enough time has passed (prevents low traffic from stalling)
                var shouldFlushByTime = DateTime.UtcNow - lastFlush >= FlushInterval;

                // If either condition is met → persist batch
                if (shouldFlushBySize || shouldFlushByTime)
                {
                    await ProcessBatchWithRetries(buffer, stoppingToken);

                    // Clear buffer after successful processing
                    buffer.Clear();

                    // Reset flush timer
                    lastFlush = DateTime.UtcNow;
                }
            }

            // Final flush when:
            // - App is shutting down
            // - Channel is completed
            if (buffer.Count > 0)
            {
                await ProcessBatchWithRetries(buffer, stoppingToken);
            }
        }
        catch (OperationCanceledException)
        {
            // Normal shutdown path (not an error)
            logger.LogInformation("Shipment upload worker is stopping...");
        }
        catch (Exception ex)
        {
            // Fatal errors should ALWAYS be logged
            logger.LogError(ex, "Fatal error in shipment upload worker");
        }
    }

    private async Task ProcessBatchWithRetries(List<ShipmentDetails> batch, CancellationToken ct)
    {
        // Retry loop for resiliency
        for (int attempt = 1; attempt <= MaxRetries; attempt++)
        {
            try
            {
                await SaveBatch(batch, ct);
                return; // Success → exit retry loop
            }
            catch (Exception ex)
            {
                logger.LogWarning(ex,
                    "Batch insert failed on attempt {Attempt}/{MaxRetries}",
                    attempt, MaxRetries);

                // If final attempt → escalate failure
                if (attempt == MaxRetries)
                {
                    logger.LogError(ex, "Batch permanently failed. Data loss risk.");
                    throw;
                }

                // Exponential-ish backoff (simple)
                await Task.Delay(TimeSpan.FromMilliseconds(500 * attempt), ct);
            }
        }
    }

    private async Task SaveBatch(List<ShipmentDetails> batch, CancellationToken ct)
    {
        // IMPORTANT:
        // DbContext is scoped → must NOT reuse across threads or long-lived services
        using var scope = serviceScope.CreateScope();

        var context = scope.ServiceProvider.GetRequiredService<AppDbContext>();

        try
        {
            // Bulk insert (much faster than per-row insert)
            await context.Shipments.AddRangeAsync(batch, ct);

            // Commit to DB
            await context.SaveChangesAsync(ct);

            logger.LogInformation(
                "Inserted batch of {Count} shipments at {Time}",
                batch.Count,
                DateTime.UtcNow);
        }
        catch (DbUpdateException ex) when (IsUniqueConstraintViolation(ex))
        {
            // If duplicate constraint violation occurs:
            // → we assume idempotency conflict
            logger.LogWarning(ex, "Duplicate detected. Resolving batch...");

            await HandleDuplicateBatch(context, batch, ct);
        }
    }

    private async Task HandleDuplicateBatch(
        AppDbContext context,
        List<ShipmentDetails> batch,
        CancellationToken ct)
    {
        // Extract idempotency keys (business-defined uniqueness)
        var keys = batch
            .Select(x => x.PurchaseOrderNumber)
            .ToList();

        // Query DB for already existing records
        var existingKeys = await context.Shipments
            .Where(x => keys.Contains(x.PurchaseOrderNumber))
            .Select(x => x.PurchaseOrderNumber)
            .ToListAsync(ct);

        // Remove duplicates from incoming batch
        var filtered = batch
            .Where(x => !existingKeys.Contains(x.PurchaseOrderNumber))
            .ToList();

        // If everything is duplicate → skip safely
        if (filtered.Count == 0)
        {
            logger.LogWarning("All records in batch are duplicates. Skipping batch.");
            return;
        }

        // Insert only new records
        await context.Shipments.AddRangeAsync(filtered, ct);
        await context.SaveChangesAsync(ct);

        logger.LogInformation(
            "Inserted filtered batch of {Count} shipments (duplicates removed)",
            filtered.Count);
    }

    private static bool IsUniqueConstraintViolation(DbUpdateException ex)
    {
        // DB-agnostic (but fragile) way to detect uniqueness violations
        var message = ex.InnerException?.Message ?? "";

        return message.Contains("UNIQUE", StringComparison.OrdinalIgnoreCase) ||
               message.Contains("duplicate", StringComparison.OrdinalIgnoreCase);
    }
}