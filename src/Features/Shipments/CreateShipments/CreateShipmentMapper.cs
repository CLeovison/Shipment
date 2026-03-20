using Shipment.Entities;

namespace Shipment.Features.Shipments.CreateShipments;

public record class CreateShipmentRequest(string PurchaseOrderNumber, string Vendor, DateTime TimeOfArrival);
public record class CreateShipmentResponse(string PurchaseOrderNumber, string Vendor, DateTime TimeOfArrival, string CreatedBy);

public static class CreateShipmentMapper
{
    public static ShipmentDetails ToRequest(this CreateShipmentRequest request)
    {
        return new ShipmentDetails
        {
            PurchaseOrderNumber = request.PurchaseOrderNumber,
            Vendor = request.Vendor,
            TimeOfArrival = request.TimeOfArrival,
          
        };
    }

    public static CreateShipmentResponse ToResponse(this ShipmentDetails shipment, string CreatedBy)
    {
        return new CreateShipmentResponse
        (
            shipment.PurchaseOrderNumber,
            shipment.Vendor,
            shipment.TimeOfArrival,
            CreatedBy
        );
    }
}