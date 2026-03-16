using System.Security.Claims;
using Microsoft.AspNetCore.SignalR;
using Microsoft.EntityFrameworkCore;
using Shipment.Abstract;
using Shipment.Database;
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
    IHttpContextAccessor httpContext)
{
    private const int NoticeDays = 14;

    public async Task<IReadOnlyList<GetShipmentNoticeResponse>> GetNoticeShipmentAsync(CancellationToken ct)
    {
        var userId = GetUserId();
        if (userId is null)
            return [];

        var today = DateTime.UtcNow.Date;

        var shipments = await QueryShipments(userId.Value, today)
            .ToListAsync(ct);

        var response = MapToResponse(shipments, today);

        if (response.Count > 0)
        {
            await hub.Clients.User(userId.Value.ToString()).ShipmentArrivalNotice(response);
        }

        return response;
    }

    private int? GetUserId()
    {
        var value = httpContext.HttpContext?.User?.FindFirstValue(ClaimTypes.NameIdentifier);

        return int.TryParse(value, out var id) ? id : null;
    }

    private IQueryable<ShipmentProjection> QueryShipments(int userId, DateTime today)
    {
        var targetArrival = today.AddDays(NoticeDays);

        return dbContext.Shipments
            .Where(x =>
                x.UserId == userId &&
                x.TimeOfArrival >= targetArrival &&
                x.TimeOfArrival < targetArrival.AddDays(1))
            .OrderBy(x => x.TimeOfArrival)
            .AsNoTracking()
            .Select(x => new ShipmentProjection(
                x.PurchaseOrderNumber,
                x.Vendor,
                x.TimeOfArrival));
    }

    private static IReadOnlyList<GetShipmentNoticeResponse> MapToResponse(IEnumerable<ShipmentProjection> shipments, DateTime today)
    {
        return shipments
            .Select(x => new GetShipmentNoticeResponse(
                x.PurchaseOrderNumber,
                x.Vendor,
                x.TimeOfArrival,
                (x.TimeOfArrival.Date - today).Days))
            .ToList();
    }
}

public sealed class GetNoticeShipmentEndpoint : IEndpoint
{
    public void Endpoint(IEndpointRouteBuilder app)
    {
        app.MapGet("/api/v1/shipments/notice",
        async (GetShipmentNoticeHandler handler, CancellationToken ct) =>
        {
            var result = await handler.GetNoticeShipmentAsync(ct);
            return Results.Ok(result);
        });
    }
}