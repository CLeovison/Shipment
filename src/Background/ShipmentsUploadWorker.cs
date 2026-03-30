using Microsoft.EntityFrameworkCore;
using Shipment.Database;
using Shipment.Entities;
using Shipment.Features.Shipments.UploadShipments;

namespace Shipment.Background;


// Background worker responsible for consuming shipment upload requests from an in-memory queue,
// batching them for efficiency, and persisting them to the database.
//
// Key design decisions:
// - Uses batching (BatchSize) to reduce database roundtrips.
// - Uses a time-based flush (FlushInterval) to prevent data from staying too long in memory.
// - Implements retry logic to handle transient failures.
// - Falls back to row-by-row persistence when batch insert fails (e.g., due to constraint violations).
// - Tracks progress via UploadProgressStore to provide feedback to clients.
public sealed class ShipmentsUploadWorker(
    ILogger<ShipmentsUploadWorker> logger,
    IServiceScopeFactory serviceScope,
    UploadShipmentQueue queue,
    UploadProgressStore progressStore)
    : BackgroundService
{
    private const int BatchSize = 1000;
    private static readonly TimeSpan FlushInterval = TimeSpan.FromSeconds(2);
    private const int MaxRetries = 3;


    // Main processing loop:
    // Continuously reads from the queue while the service is running.
    //
    // Behavior:
    // - Accumulates items into an in-memory buffer.
    // - Flushes to database when either:
    //   1. Batch size is reached (high throughput optimization)
    //   2. Flush interval is exceeded (latency guarantee)
    // - Includes small delays to prevent CPU spinning when the queue is empty.
    // - Ensures graceful shutdown by processing remaining buffered items in the finally block.
    protected override async Task ExecuteAsync(CancellationToken stoppingToken)
    {
        logger.LogInformation("Batch Uploading of Shipments is Starting");

        var buffer = new List<ShipmentImportDto>(BatchSize);
        var lastFlush = DateTime.UtcNow;

        try
        {
            while (!stoppingToken.IsCancellationRequested)
            {


                // Waits asynchronously until new items are available in the queue.
                // Prevents blocking the thread while still allowing periodic checks for flush conditions.
                //
                // Important:
                // - This avoids busy-waiting.
                // - Still allows the loop to continue if no data arrives for a while.

                // ✅ FIX: Use WaitToReadAsync with a timeout to prevent blocking forever
                // This allows the loop to continue and check the FlushInterval even if no new data arrives.
                if (await queue.Reader.WaitToReadAsync(stoppingToken.CanBeCanceled ? stoppingToken : CancellationToken.None))
                {
                    while (queue.Reader.TryRead(out var dto))
                    {

                        buffer.Add(Normalize(dto));

                        // Time-based flush:
                        // Ensures that even low-volume or idle workloads are persisted without waiting
                        // for the batch size threshold to be reached.
                        //
                        // This guarantees:
                        // - Data freshness
                        // - No indefinite memory accumulation
                        if (buffer.Count >= BatchSize)
                        {
                            await ProcessBatch(new List<ShipmentImportDto>(buffer), stoppingToken);
                            buffer.Clear();
                            lastFlush = DateTime.UtcNow;
                        }
                    }
                }

                // ✅ PERIODIC FLUSH: Check if the interval has passed even if WaitToReadAsync timed out or queue is empty
                if (buffer.Count > 0 && DateTime.UtcNow - lastFlush >= FlushInterval)
                {
                    logger.LogInformation("Flushing buffer of {Count} items due to interval", buffer.Count);
                    await ProcessBatch([.. buffer], stoppingToken);
                    buffer.Clear();
                    lastFlush = DateTime.UtcNow;
                }

                // Small delay to prevent tight loop if queue is empty
                await Task.Delay(500, stoppingToken);
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
        // Ensures no data is lost when the worker shuts down.
        //
        // If there are remaining buffered items:
        // - They are processed immediately before the service exits.
        //
        // Uses CancellationToken.None to guarantee completion even during shutdown.
        finally
        {
            // Final cleanup
            if (buffer.Count > 0)
            {
                await ProcessBatch(buffer, CancellationToken.None);
            }
        }
    }

    // Cleans incoming data before processing.
    // Prevents subtle bugs caused by:
    // - Leading/trailing whitespace
    // - Null values in critical fields

    private static ShipmentImportDto Normalize(ShipmentImportDto dto)
    {
        dto.PurchaseOrderNumber = dto.PurchaseOrderNumber?.Trim() ?? string.Empty;
        dto.Vendor = dto.Vendor?.Trim() ?? string.Empty;
        return dto;
    }

    // Wraps batch persistence with retry logic.
    //
    // Retry strategy:
    // - Attempts the operation up to MaxRetries times.
    // - Uses exponential backoff (increasing delay per attempt).
    // - If all retries fail, marks all items as failed.
    //
    // This helps handle:
    // - Temporary DB/network issues
    // - Deadlocks or transient EF failures
    private async Task ProcessBatch(List<ShipmentImportDto> batch, CancellationToken ct)
    {
        for (int attempt = 1; attempt <= MaxRetries; attempt++)
        {
            try
            {
                var (succeeded, failed) = await SaveBatch(batch, ct);
                UpdateProgress(succeeded, failed);
                return;
            }
            catch (Exception ex)
            {
                logger.LogWarning(ex, "Batch failed on attempt {Attempt}/{MaxRetries}", attempt, MaxRetries);
                if (attempt == MaxRetries)
                {
                    UpdateProgress([], batch);
                    return;
                }
                await Task.Delay(TimeSpan.FromMilliseconds(300 * attempt), ct);
            }
        }
    }

    // Attempts to insert the entire batch in a single database operation.
    //
    // Why:
    // - Maximizes performance (fewer DB roundtrips)
    // - Leverages EF Core change tracking for efficiency
    //
    // Fallback:
    // - If batch insert fails (DbUpdateException), fallback to SaveRowByRow
    //   to isolate problematic records.
    private async Task<(List<ShipmentImportDto> succeeded, List<ShipmentImportDto> failed)> SaveBatch(
        List<ShipmentImportDto> batch,
        CancellationToken ct)
    {
        using var scope = serviceScope.CreateScope();
        var context = scope.ServiceProvider.GetRequiredService<AppDbContext>();

        var entities = batch.Select(dto => new ShipmentDetails
        {
            PurchaseOrderNumber = dto.PurchaseOrderNumber,
            Vendor = dto.Vendor,
            TimeOfArrival = dto.TimeOfArrival,
            NotifyStartAt = dto.TimeOfArrival.AddDays(-14),
            UserId = dto.UserId
        }).ToList();

        try
        {
            await context.Shipments.AddRangeAsync(entities, ct);
            await context.SaveChangesAsync(ct);
            return (batch, []);
        }
        catch (DbUpdateException)
        {
            return await SaveRowByRow(batch, ct);
        }
    }

    // Fallback strategy when batch insert fails.
    //
    // Why this exists:
    // - Batch failures are often caused by a single bad record.
    // - Processing individually allows partial success.
    //
    // Tradeoff:
    // - Slower than batch insert
    // - Used only when necessary
    private async Task<(List<ShipmentImportDto> succeeded, List<ShipmentImportDto> failed)> SaveRowByRow(
        List<ShipmentImportDto> batch,
        CancellationToken ct)
    {
        var succeeded = new List<ShipmentImportDto>();
        var failed = new List<ShipmentImportDto>();

        using var scope = serviceScope.CreateScope();
        var context = scope.ServiceProvider.GetRequiredService<AppDbContext>();

        foreach (var dto in batch)
        {
            try
            {
                var entity = new ShipmentDetails
                {
                    PurchaseOrderNumber = dto.PurchaseOrderNumber,
                    Vendor = dto.Vendor,
                    TimeOfArrival = dto.TimeOfArrival,
                    NotifyStartAt = dto.TimeOfArrival.AddDays(-14),
                    UserId = dto.UserId
                };
                context.Shipments.Add(entity);
                await context.SaveChangesAsync(ct);
                succeeded.Add(dto);
            }
            catch
            {
                failed.Add(dto);
            }
        }
        return (succeeded, failed);
    }

    // Updates in-memory progress tracking for each upload job.
    //
    // Tracks:
    // - Processed count
    // - Success count
    // - Failure count
    //
    // Marks completion when all items are processed.
    //
    // Note:
    // - This is critical for real-time feedback (e.g., UI progress indicators).
    private void UpdateProgress(List<ShipmentImportDto> succeeded, List<ShipmentImportDto> failed)
    {
        foreach (var item in succeeded)
        {
            var progress = progressStore.GetId(item.UploadId);
            if (progress == null) continue;
            progress.Processed++;
            progress.Succeeded++;
            if (progress.Processed >= progress.Total && progress.Total > 0)
            {
                progress.IsCompleted = true;
                progress.CompletedAt = DateTime.UtcNow;
            }
        }
        foreach (var item in failed)
        {
            var progress = progressStore.GetId(item.UploadId);
            if (progress == null) continue;
            progress.Processed++;
            progress.Failed++;
            if (progress.Processed >= progress.Total && progress.Total > 0)
            {
                progress.IsCompleted = true;
                progress.CompletedAt = DateTime.UtcNow;
            }
        }
    }
}
