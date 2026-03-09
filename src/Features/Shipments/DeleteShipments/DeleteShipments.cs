using Shipment.Abstract;
using Shipment.Abstract.Results;
using Shipment.Abstract.Results.Errors;
using Shipment.Database;
using Shipment.Entities;

namespace Shipment.Features.Shipments.DeleteShipments;

internal sealed class DeleteShipmentHandler(AppDbContext dbContext)
{
    public async Task<Result<ShipmentDetails>> DeleteShipmentAsync(int id, CancellationToken ct)
    {
        try
        {
            var query = await dbContext.Shipments.FindAsync(id, ct);

            if (query is null)
            {
                return Result.Failure<ShipmentDetails>(Error.DidntExists(""));
            }
            dbContext.Shipments.Remove(query);
            await dbContext.SaveChangesAsync();

            return Result.Success(query);
        }
        catch (Exception)
        {
            return Result.Failure<ShipmentDetails>(new Error("Error", "An error occurred while deleting the shipment."));
        }
    }

}

public sealed class DeleteShipmentEndpoint : IEndpoint
{
    public void Endpoint(IEndpointRouteBuilder app)
    {
        app.MapDelete("/api/v1/shipments/{id}", async (int id, DeleteShipmentHandler handler, CancellationToken ct) =>
        {
            try
            {
                var removed = await handler.DeleteShipmentAsync(id, ct);

                if (removed.IsFailure)
                {
                    return Results.NotFound();
                }


                return Results.NoContent();
            }
            catch (Exception ex)
            {
                return Results.Problem(detail: ex.Message, statusCode: 500, title: "An error occurred while deleting the shipment");
            }
        })
        .RequireAuthorization();
    }
}