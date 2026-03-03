using Shipment.Entities;
using Shipment.Features.Shipments.Shared;

namespace Shipment.Features.Shipments.UpdateShipments;

public record class UpdateShipmentRequest();
public record class UpdateShipmentResponse();

public static class UpdateShipmentsMapper
{

    public static void ToEntity(this ShipmentRequest request, ShipmentDetails details, string UpdatedBy)
    {
        details.ShipmentId = request.ShipmentId;
        details.PurchaseOrderNumber = request.PurchaseOrderNumber;
        details.Vendor = request.Vendor;
        details.TimeOfArrival = request.TimeOfArrival;
        details.ModifiedAt = DateTime.UtcNow;
        details.UserId = request.UserId;

    }

    public static ShipmentResponse ToResponse(this ShipmentDetails shipments, string UpdatedBy)
    {
        return new ShipmentResponse
        {
            PurchaseOrderNumber = shipments.PurchaseOrderNumber,
            Vendor = shipments.Vendor,
            TimeOfArrival = shipments.TimeOfArrival,
            UpdatedBy = UpdatedBy,
            ModifiedAt = shipments.ModifiedAt ?? DateTime.UtcNow,
        };
    }
}