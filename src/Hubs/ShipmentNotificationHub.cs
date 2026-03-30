using System.Security.Claims;
using Microsoft.AspNetCore.SignalR;


namespace Shipment.Hubs;

public sealed class ShipmentNotificationHub : Hub<IShipmentClient>
{
    public override async Task OnConnectedAsync()
    {
        var userId = Context.User?.FindFirst(ClaimTypes.NameIdentifier)?.Value;

        if (string.IsNullOrWhiteSpace(userId))
        {
            Context.Abort();
            return;
        }

        await base.OnConnectedAsync();
    }
    public override async Task OnDisconnectedAsync(Exception? exception)
    {

        await base.OnDisconnectedAsync(exception);
    }

}