namespace Shipment.Features.Shipments.GetAllShipments;


public record class ShipmentFilter(string? PurchaseOrderNumber, DateTime? TimeOfArrival);