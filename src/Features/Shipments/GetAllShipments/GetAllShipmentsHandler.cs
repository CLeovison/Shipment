using Microsoft.EntityFrameworkCore;
using Shipment.Abstract;
using Shipment.Database;
using Shipment.Entities.Shared;

namespace Shipment.Features.Shipments.GetAllShipments;

public record class GetAllShipmentResponse(string PurchaseOrderNumber, string Vendor, DateTime TimeOfArrival, string CreatedBy);

internal sealed class GetAllShipmentHandler(AppDbContext dbContext)
{
    public async Task<PaginationResponse<GetAllShipmentResponse>> GetAllShipmentAsync(
            int pageSize,
         int pageNumber,
         string? searchTerm,
         ShipmentFilter filter,
         CancellationToken ct)
    {
        var query = dbContext.Shipments.AsQueryable();

        var lowerCase = searchTerm?.Trim().ToLower();

        if (!string.IsNullOrWhiteSpace(lowerCase))
        {
            if (DateTime.TryParse(lowerCase, out DateTime parsedSearchDate))
            {
                query = query.Where(x => x.PurchaseOrderNumber.Contains(lowerCase) || x.TimeOfArrival.Date == parsedSearchDate);
            }
        }

        if (!string.IsNullOrWhiteSpace(filter.PurchaseOrderNumber))
        {
            query = query.Where(x => x.PurchaseOrderNumber.Contains(filter.PurchaseOrderNumber));
        }

        if (filter.TimeOfArrival != default)
        {
            query = query.Where(x => x.TimeOfArrival.Date == filter.TimeOfArrival);
        }

        var totalCount = await query.CountAsync(ct);

        var shipment = await query
        .OrderBy(x => x.PurchaseOrderNumber)
        .ThenBy(x => x.TimeOfArrival)
        .Skip((pageSize - 1) * pageNumber)
        .Take(pageNumber)
        .AsNoTracking()
        .Select(s => new GetAllShipmentResponse(
            s.PurchaseOrderNumber,
            s.Vendor,
            s.TimeOfArrival,
            s.User.FirstName
        ))
        .ToListAsync();

        return new PaginationResponse<GetAllShipmentResponse>(shipment, pageSize, pageNumber, totalCount);
    }
}

public sealed class GetAllShipmentEndpoint : IEndpoint
{
    public void Endpoint(IEndpointRouteBuilder app)
    {
        app.MapGet("/api/v1/shipments/", async (GetAllShipmentHandler handler,
        [AsParameters] ShipmentFilter filter,
         CancellationToken ct,
        int pageSize = 1,
        int pageNumber = 10,
        string? searchTerm = null) =>
        {
            var query = await handler.GetAllShipmentAsync(pageSize,
            pageNumber,
            searchTerm,
            filter,
            ct);

            return Results.Ok(query);
        });
    }
}