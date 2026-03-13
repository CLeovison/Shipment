
using Shipment.Features.Shipments.CreateShipments;
using Shipment.Features.Shipments.GetShipmentNotice;
using Shipment.Features.Shipments.UpdateShipments;

namespace Shipment.Hubs;

public interface IShipmentClient
{
    Task ShipmentCreated(CreateShipmentResponse response);
    Task ShipmentUpdated(UpdateShipmentResponse response);
    Task ShipmentArrivalNotice(IReadOnlyList<GetShipmentNoticeResponse> response);
}