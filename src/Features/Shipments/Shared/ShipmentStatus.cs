namespace Shipment.Features.Shipments.Shared;


public enum ShipmentStatus
{
    // Shipment is incomplete in documentation, such as missing PO number, ETA, or vendor name. Action is required before processing can proceed.
    Pending,
    // Shipment has been received by the DC; however, there are discrepancies between the commercial invoice and the actual items received. 
    // Further validation or correction is required.
    Partial,

    // Shipment is incomplete in documentation, such as missing PO number, ETA, or vendor name. Action is required before processing can proceed.
    Received,
}

