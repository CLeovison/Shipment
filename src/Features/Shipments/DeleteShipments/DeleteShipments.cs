using Shipment.Abstract.Results;
using Shipment.Database;

namespace Shipment.Features.Shipments.DeleteShipments;


internal sealed class DeleteShipmentHandler(AppDbContext dbContext)
{
    public async Task<Result<bool>> DeleteShipmentAsync(int id, CancellationToken ct)
    {
        var query = await dbContext.Users.FindAsync(id, ct);

        if (query is null)
        {

        }

        return query is not null;
    }

}