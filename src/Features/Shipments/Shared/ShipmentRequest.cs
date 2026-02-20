namespace Shipment.Features.Shipments.Shared;

public sealed class ShipmentRequest
{
    public int ShipmentId { get; set; }
    public int UserId { get; set; }
    public string PurchaseOrderNumber { get; set; } = string.Empty;
    public string Vendor { get; set; } = string.Empty;
    public string UpdatedBy { get; set; } = string.Empty;
    public DateTime TimeOfArrival { get; set; }

    public DateTime CreatedAt { get; set; }
    public DateTime ModifiedAt { get; set; }
}