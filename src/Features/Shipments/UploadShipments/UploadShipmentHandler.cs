using System.Globalization;
using CsvHelper;
using Shipment.Abstract;
using System.Security.Claims;
using Shipment.Database;
using Shipment.Entities;
using Shipment.Features.Shipments.Shared;

namespace Shipment.Features.Shipments.UploadShipments;

internal sealed class UploadShipmentHandler(
    UploadShipmentQueue queue,
    IHttpContextAccessor httpContext,
    UploadProgressStore progressStore,
    IServiceScopeFactory serviceScopeFactory,
    ILogger<UploadShipmentHandler> logger)
{
    public async Task<Guid> UploadShipmentAsync(IFormFile file, CancellationToken ct)
    {
        var userIdClaim = httpContext.HttpContext?.User?.FindFirst(ClaimTypes.NameIdentifier)?.Value;
        if (string.IsNullOrWhiteSpace(userIdClaim) || !int.TryParse(userIdClaim, out var userId))
            throw new InvalidOperationException("Cannot determine authenticated user.");

        var uploadId = Guid.NewGuid();
        progressStore.Create(uploadId);

        // Create the database log entry
        using (var scope = serviceScopeFactory.CreateScope())
        {
            var context = scope.ServiceProvider.GetRequiredService<AppDbContext>();
            context.UploadLogs.Add(new UploadLog
            {
                Id = uploadId,
                UserId = userId,
                FileName = file.FileName,
                StartedAt = DateTime.UtcNow
            });
            await context.SaveChangesAsync(ct);
        }

        // Copy file to memory so we can return immediately
        var memoryStream = new MemoryStream();
        await file.CopyToAsync(memoryStream, ct);
        memoryStream.Position = 0;

        // Fire-and-forget: parse CSV and enqueue in the background
        _ = Task.Run(() => ParseAndEnqueueAsync(memoryStream, uploadId, userId), CancellationToken.None);

        return uploadId;
    }

    private async Task ParseAndEnqueueAsync(MemoryStream stream, Guid uploadId, int userId)
    {
        try
        {
            var config = new CsvHelper.Configuration.CsvConfiguration(CultureInfo.InvariantCulture)
            {
                HeaderValidated = null,
                MissingFieldFound = null,
                PrepareHeaderForMatch = args => args.Header.Trim(),
            };

            using (stream)
            using (var reader = new StreamReader(stream))
            using (var csv = new CsvReader(reader, config))
            {
                csv.Context.RegisterClassMap<ShipmentCsvRecordMap>();

                var records = new List<ShipmentImportDto>();

                await foreach (var record in csv.GetRecordsAsync<ShipmentCsvRecord>())
                {
                    if (string.IsNullOrWhiteSpace(record.PurchaseOrderNumber))
                        continue;

                    records.Add(new ShipmentImportDto
                    {
                        UserId = userId,
                        PurchaseOrderNumber = record.PurchaseOrderNumber.Trim(),
                        Vendor = record.Vendor?.Trim() ?? string.Empty,
                        TimeOfArrival = DateHelper.ToUtcDate(record.TimeOfArrival),
                        UploadId = uploadId
                    });
                }

                var progress = progressStore.GetId(uploadId);
                if (progress != null)
                    progress.Total = records.Count;

                logger.LogInformation("Found {Count} valid records in CSV for UploadId {UploadId}", records.Count, uploadId);

                foreach (var dto in records)
                {
                    await queue.Writer.WriteAsync(dto);
                }
            }
        }
        catch (Exception ex)
        {
            logger.LogError(ex, "Error parsing CSV for UploadId {UploadId}", uploadId);

            var progress = progressStore.GetId(uploadId);
            if (progress != null)
            {
                progress.IsCompleted = true;
                progress.CompletedAt = DateTime.UtcNow;
                progress.Errors.Add(ex.Message);
            }
        }
    }
}

public sealed class UploadShipmentEndpoint : IEndpoint
{
    public void Endpoint(IEndpointRouteBuilder app)
    {
        app.MapPost("/api/v1/shipments/upload", async (UploadShipmentHandler handler, IFormFile file, CancellationToken ct) =>
        {
            if (file == null || file.Length == 0)
                return Results.BadRequest("No file uploaded");

            try
            {
                var uploadId = await handler.UploadShipmentAsync(file, ct);
                return Results.Ok(new { Message = "File uploaded successfully", UploadId = uploadId });
            }
            catch (OperationCanceledException)
            {
                return Results.StatusCode(StatusCodes.Status499ClientClosedRequest);
            }
            catch (InvalidOperationException ex) when (ex.Message.Contains("authenticated user"))
            {
                return Results.Unauthorized();
            }
            catch (Exception ex)
            {
                return Results.Problem(ex.Message);
            }
        })
        .Accepts<IFormFile>("multipart/form-data") // ensures proper form-data binding
        .DisableAntiforgery()
        .RequireAuthorization();
    }
}