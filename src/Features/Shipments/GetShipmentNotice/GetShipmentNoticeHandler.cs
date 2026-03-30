using System.Security.Claims;
using Microsoft.AspNetCore.SignalR;
using Microsoft.EntityFrameworkCore;
using Shipment.Abstract;
using Shipment.Database;
using Shipment.Entities;
using Shipment.Entities.Shared;
using Shipment.Hubs;

namespace Shipment.Features.Shipments.GetShipmentNotice;

public sealed record GetShipmentNoticeResponse(
    string PurchaseOrderNumber,
    string Vendor,
    DateTime TimeOfArrival,
    int DaysRemaining);

public sealed record ShipmentProjection(
    string PurchaseOrderNumber,
    string Vendor,
    DateTime TimeOfArrival);

internal sealed class GetShipmentNoticeHandler(
    AppDbContext dbContext,
    IHubContext<ShipmentNotificationHub, IShipmentClient> hub,
    IHttpContextAccessor httpContext,
    ILogger<GetShipmentNoticeHandler> logger)
{
    private const int NoticeDays = 14;

    public async Task<IReadOnlyList<GetShipmentNoticeResponse>> GetNoticeShipmentAsync(CancellationToken ct)
    {
        var userId = GetUserId();

        if (userId is null)
        {
            logger.LogWarning("No user ID found in claims");
            return Array.Empty<GetShipmentNoticeResponse>();
        }

        var today = DateTime.UtcNow.Date;

        var shipments = await dbContext.Shipments
            .Where(x => x.UserId == userId)
            .ForNotifications(today, NoticeDays)
            .OrderBy(x => x.TimeOfArrival)
            .ToListAsync(ct);

        logger.LogInformation("Handler found {Count} shipments", shipments.Count);

        if (shipments.Count == 0)
            return Array.Empty<GetShipmentNoticeResponse>();

        var response = shipments
            .Select(x => new GetShipmentNoticeResponse(
                x.PurchaseOrderNumber,
                x.Vendor,
                x.TimeOfArrival,
                (x.TimeOfArrival.Date - today).Days))
            .ToList();

        await hub.Clients.User(userId.Value.ToString())
            .ShipmentArrivalNotice(response);

        foreach (var shipment in shipments)
        {
            shipment.LastNotifiedAt = DateTime.UtcNow;

            if (shipment.TimeOfArrival.Date < today)
                shipment.IsCompleted = true;
        }

        await dbContext.SaveChangesAsync(ct);

        return response;
    }

    private int? GetUserId()
    {
        var value = httpContext.HttpContext?.User?
            .FindFirst(ClaimTypes.NameIdentifier)?.Value;

        return int.TryParse(value, out var id) ? id : null;
    }
}

public sealed class GetNoticeShipmentEndpoint : IEndpoint
{
    public void Endpoint(IEndpointRouteBuilder app)
    {
        app.MapGet("/api/v1/shipments/notice", async (GetShipmentNoticeHandler handler, CancellationToken ct) =>
        {
            var result = await handler.GetNoticeShipmentAsync(ct);
            return Results.Ok(result);
        }).RequireAuthorization();
    }
}