using Microsoft.AspNetCore.SignalR;
using Shipment.Entities;

namespace Shipment.Hubs;

public sealed class ShipmentNotificationHub : Hub
{
    public async Task ShipmentStatusUpdate(ShipmentDetails details)
    {

    }
}