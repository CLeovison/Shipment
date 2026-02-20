using Shipment.Entities;

namespace Shipment.Features.Shipments.GetShipmentsById;

public record class GetShipmentByIdResponse(
    int ShipmentId,
    string PurchaseOrderNumber,
    string Vendor,
    DateTime TimeOfArrival,
    string CreatedBy,
    DateTime CreatedAt,
    DateTime ModifiedAt);

public static class GetShipmentByIdMapper
{
    public static GetShipmentByIdResponse ToResponse(this ShipmentDetails shipment, string CreatedBy)
    {
        return new GetShipmentByIdResponse(
            shipment.ShipmentId,
            shipment.PurchaseOrderNumber,
            shipment.Vendor,
            shipment.TimeOfArrival,
            CreatedBy,
            shipment.CreatedAt,
            shipment.ModifiedAt ?? DateTime.UtcNow);

    }
}