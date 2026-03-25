using System.Globalization;
using CsvHelper;
using Shipment.Entities;

namespace Shipment.Features.Shipments.UploadShipments;


internal sealed class UploadShipmentHandler(UploadShipmentQueue queue)
{
    public async Task UploadShipmentAsync(IFormFile file, CancellationToken ct)
    {
        using var stream = file.OpenReadStream();
        using var reader = new StreamReader(stream);
        using var csv = new CsvReader(reader, CultureInfo.InvariantCulture);

        await foreach (var shipments in csv.GetRecordsAsync<ShipmentDetails>())
        {
            await queue.Writer.WaitToWriteAsync();
        }
    }
}