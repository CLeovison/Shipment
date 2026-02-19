using Shipment.Entities.Shared;

namespace Shipment.Entities;

public class ShipmentDetails : AuditableEntity
{
    public int ShipmentId { get; set; }
    public string PurchaseOrderNumber { get; set; } = string.Empty;
    public string Vendor { get; set; } = string.Empty;
    public DateTime TimeOfArrival { get; set; }
    public int UserId { get; set; }
}
