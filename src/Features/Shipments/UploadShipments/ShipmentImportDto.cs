using Shipment.Features.Shipments.Shared;

namespace Shipment.Features.Shipments.UploadShipments;

public sealed class ShipmentImportDto
{
    public int UserId { get; set; }
    public Guid UploadId { get; set; }
    public string PurchaseOrderNumber { get; set; } = string.Empty;
    public string Vendor { get; set; } = string.Empty;
    public DateTime TimeOfArrival { get; set; }

    public ShipmentStatus Status { get; set; }

}