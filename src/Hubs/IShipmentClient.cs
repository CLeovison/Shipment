
using Shipment.Features.Shipments.CreateShipments;

namespace Shipment.Hubs;

public interface IShipmentClient
{
    Task ShipmentCreated(CreateShipmentResponse response);
}