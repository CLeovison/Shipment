using Microsoft.AspNetCore.SignalR;


namespace Shipment.Hubs;

public sealed class ShipmentNotificationHub : Hub<IShipmentClient>
{
    public async Task ShipmentStatusUpdate(string userId)
    {
        await Clients.User(userId).ShipmentStatusUpdate(userId);
    }
}