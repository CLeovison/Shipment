using System.Globalization;
using CsvHelper;
using Shipment.Abstract;
using System.Security.Claims;

namespace Shipment.Features.Shipments.UploadShipments;

internal sealed class UploadShipmentHandler(
    UploadShipmentQueue queue, 
    IHttpContextAccessor httpContext, 
    UploadProgressStore progressStore,
    ILogger<UploadShipmentHandler> logger) // Added logger
{
    public async Task UploadShipmentAsync(IFormFile file, CancellationToken ct)
    {
        var userIdClaim = httpContext.HttpContext?.User?.FindFirst(ClaimTypes.NameIdentifier)?.Value;
        if (string.IsNullOrWhiteSpace(userIdClaim) || !int.TryParse(userIdClaim, out var userId))
            throw new InvalidOperationException("Cannot determine authenticated user.");


        var config = new CsvHelper.Configuration.CsvConfiguration(CultureInfo.InvariantCulture)
        {
            HeaderValidated = null,
            MissingFieldFound = null,
            PrepareHeaderForMatch = args => args.Header.Trim(), // Trim headers
        };

        var uploadId = Guid.NewGuid();
        var progress = progressStore.Create(uploadId);

        using var stream = file.OpenReadStream();
        using var reader = new StreamReader(stream);
        using var csv = new CsvReader(reader, config);

        csv.Context.RegisterClassMap<ShipmentCsvRecordMap>();

        var records = new List<ShipmentImportDto>();
        
        try 
        {
            await foreach (var record in csv.GetRecordsAsync<ShipmentCsvRecord>(ct))
            {
                if (string.IsNullOrWhiteSpace(record.PurchaseOrderNumber))
                {
                    logger.LogWarning("Skipping row with empty PurchaseOrderNumber at row {Row}", csv.Context.Parser?.Row ?? 0);
                    continue;
                }

                records.Add(new ShipmentImportDto
                {
                    UserId = userId,
                    PurchaseOrderNumber = record.PurchaseOrderNumber.Trim(),
                    Vendor = record.Vendor?.Trim() ?? string.Empty,
                    TimeOfArrival = record.TimeOfArrival ?? DateTime.UtcNow,
                    UploadId = uploadId
                });
            }
        }
        catch (Exception ex)
        {
            logger.LogError(ex, "Error reading CSV at row {Row}", csv.Context.Parser?.Row ?? 0);
            throw; // Re-throw to let the endpoint handle it
        }

        progress.Total = records.Count;
        logger.LogInformation("Found {Count} valid records in CSV for UploadId {UploadId}", records.Count, uploadId);

        foreach (var dto in records)
        {
            await queue.Writer.WriteAsync(dto, ct);
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
                await handler.UploadShipmentAsync(file, ct);
                return Results.Ok("File uploaded successfully");
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