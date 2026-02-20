using Shipment.Entities;
using Shipment.Features.Shipments.Shared;

namespace Shipment.Features.Shipments.CreateShipments;


public static class CreateShipmentMapper
{
    public static ShipmentDetails ToRequest(this ShipmentRequest request)
    {
        return new ShipmentDetails
        {
            PurchaseOrderNumber = request.PurchaseOrderNumber,
            Vendor = request.Vendor,
            TimeOfArrival = request.TimeOfArrival,
            UserId = request.UserId
        };
    }

    public static ShipmentResponse ToResponse(this ShipmentDetails shipment, string CreatedBy)
    {
        return new ShipmentResponse
        {
            PurchaseOrderNumber = shipment.PurchaseOrderNumber,
            Vendor = shipment.Vendor,
            TimeOfArrival = shipment.TimeOfArrival,
            CreatedBy = CreatedBy
        };
    }
}