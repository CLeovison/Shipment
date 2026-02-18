using Microsoft.EntityFrameworkCore;
using Shipment.Abstract;
using Shipment.Database;
using Shipment.Entities.Shared;

namespace Shipment.Features.Shipments.GetAllShipments;

public record class GetAllShipmentResponse(string PurchaseOrderNumber, string Vendor, DateTime TimeOfArrival);
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
            query = query.Where(x => x.PurchaseOrderNumber.Contains(lowerCase) || x.TimeOfArrival.ToString().Contains(lowerCase));
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
        .Skip((pageNumber - 1) * pageSize)
        .Take(pageSize)
        .AsNoTracking()
        .Select(s => new GetAllShipmentResponse(
            s.PurchaseOrderNumber,
            s.Vendor,
            s.TimeOfArrival
        ))
        .ToListAsync();


        return new PaginationResponse<GetAllShipmentResponse>(shipment, pageSize, pageNumber, totalCount);
    }
}

public sealed class GetAllShipmentEndpoint : IEndpoint
{
    public void Endpoint(IEndpointRouteBuilder app)
    {

    }
}