using Microsoft.AspNetCore.SignalR;


namespace Shipment.Hubs;

public sealed class ShipmentNotificationHub : Hub<IShipmentClient>
{
    public override Task OnConnectedAsync()
    {
        return base.OnConnectedAsync();
    }
    public override Task OnDisconnectedAsync(Exception? exception)
    {
        return base.OnDisconnectedAsync(exception);
    }

}