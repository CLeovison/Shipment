using Shipment.Entities.Shared;

namespace Shipment.Entities;

public class ShipmentDetails : AuditableEntity
{
    public int ShipmentId { get; set; }
    public string PurchaseOrderNumber { get; set; } = string.Empty;
    public string Vendor { get; set; } = string.Empty;
    public DateTime TimeOfArrival { get; set; }

    public bool IsNotified { get; set; } = false;
    public int UserId { get; set; }

    //Injecting the Users into ShipmentDetails Entity is Called Navigation Property
    public Users User { get; set; } = null!;

}
