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

internal sealed class GetShipmentNoticeHandler(
    AppDbContext db,
    IHttpContextAccessor http,
    ILogger<GetShipmentNoticeHandler> logger)
{
    private const int NoticeDays = 14;

    public async Task<IReadOnlyList<GetShipmentNoticeResponse>> Handle(CancellationToken ct)
    {
        var userId = GetUserId();
        if (userId is null) return [];

        var today = DateTime.UtcNow.Date;

        var shipments = await db.Shipments
            .Where(x => x.UserId == userId &&
                        !x.IsCompleted &&
                        x.TimeOfArrival >= today &&
                        x.TimeOfArrival <= today.AddDays(NoticeDays))
            .OrderBy(x => x.TimeOfArrival)
            .ToListAsync(ct);

        return shipments.Select(x => new GetShipmentNoticeResponse(
            x.PurchaseOrderNumber,
            x.Vendor,
            x.TimeOfArrival,
            (x.TimeOfArrival - today).Days)).ToList();
    }

    private int? GetUserId()
    {
        var value = http.HttpContext?.User?
            .FindFirst(ClaimTypes.NameIdentifier)?.Value;

        return int.TryParse(value, out var id) ? id : null;
    }
}

public sealed class GetNoticeShipmentEndpoint : IEndpoint
{
    public void Endpoint(IEndpointRouteBuilder app)
    {
        app.MapGet("/api/v1/shipments/notice",
            async (GetShipmentNoticeHandler handler, CancellationToken ct) =>
            {
                var result = await handler.Handle(ct);
                return Results.Ok(result);
            })
        .RequireAuthorization();
    }
}