using Shipment.Entities;

namespace Shipment.Features.Shipments.UpdateShipments;

public record class UpdateShipmentRequest(
    int ShipmentId,
    string PurchaseOrderNumber,
    string Vendor,
    DateTime TimeOfArrival,
    DateTime ModfiedAt, int UserId);

public record class UpdateShipmentResponse(
    string PurchaseOrderNumber,
    string Vendor,
    DateTime TimeOfArrival,
    DateTime ModfiedAt,
    string UpdatedBy);

public static class UpdateShipmentsMapper
{

    public static void ToEntity(this UpdateShipmentRequest request, ShipmentDetails details, string UpdatedBy)
    {
        details.ShipmentId = request.ShipmentId;
        details.PurchaseOrderNumber = request.PurchaseOrderNumber;
        details.Vendor = request.Vendor;
        details.TimeOfArrival = request.TimeOfArrival;
        details.ModifiedAt = DateTime.UtcNow;
        details.UserId = request.UserId;
    }

    public static UpdateShipmentResponse ToResponse(this ShipmentDetails shipments, string UpdatedBy)
    {
        return new UpdateShipmentResponse
       (
        shipments.PurchaseOrderNumber,
        shipments.Vendor,
        shipments.TimeOfArrival,
        shipments.ModifiedAt ?? DateTime.UtcNow,
        UpdatedBy
      );
    }
}