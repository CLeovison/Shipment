using Shipment.Entities;

namespace Shipment.Hubs;

public interface IShipmentClient
{
    Task ShipmentStatusUpdate(string userId);
}