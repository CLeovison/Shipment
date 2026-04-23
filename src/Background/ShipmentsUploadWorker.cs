using Microsoft.EntityFrameworkCore;
using Shipment.Database;
using Shipment.Entities;
using Shipment.Features.Shipments.Shared;
using Shipment.Features.Shipments.UploadShipments;

namespace Shipment.Background;

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

    protected override async Task ExecuteAsync(CancellationToken stoppingToken)
    {
        logger.LogInformation("Batch Uploading of Shipments is Starting");

        var buffer = new List<ShipmentImportDto>(BatchSize);
        var lastFlush = DateTime.UtcNow;

        try
        {
            while (!stoppingToken.IsCancellationRequested)
            {
                if (await queue.Reader.WaitToReadAsync(stoppingToken.CanBeCanceled ? stoppingToken : CancellationToken.None))
                {
                    while (queue.Reader.TryRead(out var dto))
                    {
                        buffer.Add(Normalize(dto));

                        if (buffer.Count >= BatchSize)
                        {
                            await ProcessBatch([.. buffer], stoppingToken);
                            buffer.Clear();
                            lastFlush = DateTime.UtcNow;
                        }
                    }
                }

                if (buffer.Count > 0 && DateTime.UtcNow - lastFlush >= FlushInterval)
                {
                    await ProcessBatch([.. buffer], stoppingToken);
                    buffer.Clear();
                    lastFlush = DateTime.UtcNow;
                }

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
        finally
        {
            if (buffer.Count > 0)
            {
                await ProcessBatch(buffer, CancellationToken.None);
            }
        }
    }

    private static ShipmentImportDto Normalize(ShipmentImportDto dto)
    {
        dto.PurchaseOrderNumber = dto.PurchaseOrderNumber?.Trim() ?? string.Empty;
        dto.Vendor = dto.Vendor?.Trim() ?? string.Empty;
        return dto;
    }

    private static DateTime EnsureTime(DateTime date)
    {
        if (date.TimeOfDay == TimeSpan.Zero)
            return date.Date.AddHours(8);

        return date;
    }

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

    private async Task<(List<ShipmentImportDto> succeeded, List<ShipmentImportDto> failed)> SaveBatch(
        List<ShipmentImportDto> batch,
        CancellationToken ct)
    {
        using var scope = serviceScope.CreateScope();
        var context = scope.ServiceProvider.GetRequiredService<AppDbContext>();

        var entities = batch.Select(dto =>
        {
            var arrival = EnsureTime(dto.TimeOfArrival);

            return new ShipmentDetails
            {
                PurchaseOrderNumber = dto.PurchaseOrderNumber,
                Vendor = dto.Vendor,
                TimeOfArrival = arrival,
                NotifyStartAt = arrival.AddDays(-14),
                Status = ShipmentStatus.Received,
                UserId = dto.UserId
            };
        }).ToList();

        foreach (var entity in entities)
        {
            if (entity.PurchaseOrderNumber == "" || entity.Vendor == "" || entity.TimeOfArrival == default)
            {
                entity.Status = ShipmentStatus.Pending;
            }
        }

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

    private async Task<(List<ShipmentImportDto> succeeded, List<ShipmentImportDto> failed)> SaveRowByRow(
        List<ShipmentImportDto> batch,
        CancellationToken ct)
    {
        var succeeded = new List<ShipmentImportDto>();
        var failed = new List<ShipmentImportDto>();

        using var scope = serviceScope.CreateScope();
        var context = scope.ServiceProvider.GetRequiredService<AppDbContext>();

        int counter = 0;
        foreach (var dto in batch)
        {
            try
            {
                var arrival = EnsureTime(dto.TimeOfArrival);

                var entity = new ShipmentDetails
                {
                    PurchaseOrderNumber = dto.PurchaseOrderNumber,
                    Vendor = dto.Vendor,
                    TimeOfArrival = arrival,
                    NotifyStartAt = arrival.AddDays(-14),
                    UserId = dto.UserId
                };

                if (entity.PurchaseOrderNumber == "" || entity.Vendor == "" || entity.TimeOfArrival == default)
                {
                    entity.Status = ShipmentStatus.Pending;
                }

                context.Shipments.Add(entity);
                succeeded.Add(dto);
                counter++;

                if (++counter % 50 == 0)
                {
                    await context.SaveChangesAsync(ct);
                    context.ChangeTracker.Clear();
                }
            }
            catch
            {
                failed.Add(dto);
            }
        }

        if (counter % 50 == 0)
        {
            await context.SaveChangesAsync(ct);
            context.ChangeTracker.Clear();
        }
        return (succeeded, failed);
    }

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