using Microsoft.EntityFrameworkCore;
using Shipment.Abstract;
using Shipment.Database;
using Shipment.Entities.Shared;

namespace Shipment.Features.Shipments.GetAllShipments;

public record class GetAllShipmentResponse(
    string PurchaseOrderNumber,
    string Vendor,
    DateTime TimeOfArrival,
    string CreatedBy
);

internal sealed class GetAllShipmentHandler(AppDbContext dbContext)
{
    public async Task<PaginationResponse<GetAllShipmentResponse>> GetAllShipmentAsync(
        int page,
        int pageSize,
        string? searchTerm,
        ShipmentFilter filter,
        CancellationToken ct)
    {
        var query = dbContext.Shipments.AsQueryable();


        if (!string.IsNullOrWhiteSpace(searchTerm))
        {
            var lowerCase = searchTerm.Trim().ToLower();
            
            if (DateTime.TryParse(lowerCase, out DateTime parsedDate))
            {
                query = query.Where(x =>
                    x.PurchaseOrderNumber.ToLower().Contains(lowerCase) ||
                    x.TimeOfArrival.Date == parsedDate
                );
            }
            else
            {
                query = query.Where(x =>
                    x.PurchaseOrderNumber.ToLower().Contains(lowerCase)
                );
            }
        }

        if (!string.IsNullOrWhiteSpace(filter.PurchaseOrderNumber))
        {
            query = query.Where(x =>
                x.PurchaseOrderNumber.Contains(filter.PurchaseOrderNumber)
            );
        }

        if (filter.TimeOfArrival != default)
        {
            query = query.Where(x => x.TimeOfArrival.Date == filter.TimeOfArrival);
        }

        var totalCount = await query.CountAsync(ct);

        var shipments = await query
            .OrderBy(x => x.PurchaseOrderNumber)
            .ThenBy(x => x.TimeOfArrival)
            .Skip((page - 1) * pageSize)
            .Take(pageSize)
            .AsNoTracking()
            .Select(s => new GetAllShipmentResponse(
                s.PurchaseOrderNumber,
                s.Vendor,
                s.TimeOfArrival,
                s.User.FirstName
            ))
            .ToListAsync(ct);

        return new PaginationResponse<GetAllShipmentResponse>(
            shipments,
            page,
            pageSize,
            totalCount
        );
    }
}

public sealed class GetAllShipmentEndpoint : IEndpoint
{
    public void Endpoint(IEndpointRouteBuilder app)
    {
        app.MapGet("/api/v1/shipments", async (GetAllShipmentHandler handler, [AsParameters] ShipmentFilter filter,
            int page = 1,
            int pageSize = 10,
            string? searchTerm = null,
            CancellationToken ct = default) =>
        {
            var result = await handler.GetAllShipmentAsync(page, pageSize, searchTerm, filter, ct);

            return Results.Ok(result);
        });
    }
}