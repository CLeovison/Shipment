using Microsoft.AspNetCore.SignalR;


namespace Shipment.Hubs;

public sealed class ShipmentNotificationHub : Hub<IShipmentClient>
{

}