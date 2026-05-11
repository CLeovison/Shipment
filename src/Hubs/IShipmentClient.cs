
using Shipment.Features.Shipments.CreateShipments;
using Shipment.Features.Shipments.GetShipmentNotice;
using Shipment.Features.Shipments.UpdateShipments;
using Shipment.Features.Shipments.UploadShipments;

namespace Shipment.Hubs;

public interface IShipmentClient
{
    Task ShipmentCreated(CreateShipmentResponse response);
    Task ShipmentUpdated(UpdateShipmentResponse response);
    Task ShipmentArrivalNotice(IReadOnlyList<GetShipmentNoticeResponse> response);
    Task UploadProgressUpdated(UploadProgressResponse response);
}