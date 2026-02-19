using Microsoft.EntityFrameworkCore;
using Shipment.Abstract.Results;
using Shipment.Abstract.Results.Errors;
using Shipment.Database;
using Shipment.Entities;

namespace Shipment.Features.Shipments.GetShipmentsById;


internal sealed class GetShipmentByIdHandler(AppDbContext dbContext)
{
    public async Task<Result<ShipmentDetails>> GetShipmentByIdHandlerAsync(int id, CancellationToken ct)
    {
        var query = await dbContext.Shipments.FindAsync(id, ct);

        if (query is null)
        {
            return Result.Failure<ShipmentDetails>(Error.NotFound);
        }

        return Result.Success(query);
    }
}