using Microsoft.EntityFrameworkCore;
using Shipment.Abstract;
using Shipment.Database;

namespace Shipment.Features.Shipments.GetShipmentNotice;

public sealed record GetNoticeShipmentResponse(string PurchaseOrderNumber, string Vendor, DateTime TimeOfArrival, int DaysRemaining);

public sealed record ShipmentProjection(string PurchaseOrderNumber, string Vendor, DateTime TimeOfArrival);


internal sealed class GetShipmentNoticeHandler(AppDbContext dbContext)
{
    private const int NoticeWindowDays = 14;

    public async Task<IReadOnlyList<GetNoticeShipmentResponse>> GetNoticeShipmentAsync(CancellationToken ct)
    {
        var today = DateTime.UtcNow.Date;
        var endDate = today.AddDays(NoticeWindowDays);

        var shipments = await QueryShipments(today, endDate)
            .ToListAsync(ct);

        return MapToResponse(shipments, today);
    }

    private IQueryable<ShipmentProjection> QueryShipments(DateTime start, DateTime end)
    {
        return dbContext.Shipments
            .Where(x => x.TimeOfArrival >= start && x.TimeOfArrival <= end)
            .OrderBy(x => x.TimeOfArrival)
            .AsNoTracking()
            .Select(x => new ShipmentProjection(
                x.PurchaseOrderNumber,
                x.Vendor,
                x.TimeOfArrival
            ));
    }

    private static IReadOnlyList<GetNoticeShipmentResponse> MapToResponse(IEnumerable<ShipmentProjection> shipments, DateTime today)
    {
        return shipments.Select(x =>
            new GetNoticeShipmentResponse(
                x.PurchaseOrderNumber,
                x.Vendor,
                x.TimeOfArrival,
                (x.TimeOfArrival.Date - today).Days
            )).ToList();
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
        });
        
    }
}