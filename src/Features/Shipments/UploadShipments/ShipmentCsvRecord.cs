namespace Shipment.Features.Shipments.UploadShipments;

public sealed class ShipmentCsvRecord
{
    public string PurchaseOrderNumber { get; set; } = string.Empty;
    public string Vendor { get; set; } = string.Empty;
    public DateTime TimeOfArrival { get; set; }
}