using System.Globalization;
using CsvHelper;
using Shipment.Abstract;
using Shipment.Entities;
using System.Security.Claims;

namespace Shipment.Features.Shipments.UploadShipments;

internal sealed class UploadShipmentHandler(UploadShipmentQueue queue, IHttpContextAccessor httpContext)
{
    public async Task UploadShipmentAsync(IFormFile file, CancellationToken ct)
    {
        // Capture authenticated user ID
        var userIdClaim = httpContext.HttpContext?.User?
                   .FindFirst(ClaimTypes.NameIdentifier)?.Value;


        if (string.IsNullOrWhiteSpace(userIdClaim) || !int.TryParse(userIdClaim, out var userId))
            throw new InvalidOperationException("Cannot determine authenticated user.");

        var config = new CsvHelper.Configuration.CsvConfiguration(CultureInfo.InvariantCulture)
        {
            HeaderValidated = null,
            MissingFieldFound = null
        };

        using var stream = file.OpenReadStream();
        using var reader = new StreamReader(stream);
        using var csv = new CsvReader(reader, config);

        csv.Context.RegisterClassMap<ShipmentCsvRecordMap>();

        await foreach (var record in csv.GetRecordsAsync<ShipmentDetails>(ct))
        {
            if (string.IsNullOrWhiteSpace(record.PurchaseOrderNumber))
                continue;
                
            await queue.Writer.WriteAsync(record, ct);
        }
    }
}

// Minimal API endpoint
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