using System.Globalization;
using CsvHelper;
using Shipment.Abstract;
using Shipment.Database;

namespace Shipment.Features.Shipments.UploadShipments;

internal sealed class UploadShipmentsHandler(UploadShipmentQueue queue)
{
    public async Task UploadShipmentsAsync(IFormFile file, CancellationToken ct)
    {
        using var reader = new StreamReader(file.OpenReadStream());
        using var csv = new CsvReader(reader, CultureInfo.InvariantCulture);

        var records = csv.GetRecord<ShipmentCsvRecord>();

        
    }
}

public sealed class UploadShipmentEndpoint : IEndpoint
{
    public void Endpoint(IEndpointRouteBuilder app)
    {
        app.MapPost("/api/v1/shipments/upload", async () =>
        {

        });
    }
}