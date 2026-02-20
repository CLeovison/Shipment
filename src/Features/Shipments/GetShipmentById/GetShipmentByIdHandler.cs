using Shipment.Abstract;
using Shipment.Abstract.Results;
using Shipment.Abstract.Results.Errors;
using Shipment.Database;
using Microsoft.EntityFrameworkCore;

namespace Shipment.Features.Shipments.GetShipmentsById;

public record class GetShipmentByIdResponse(
    int ShipmentId,
    string PurchaseOrderNumber,
    string Vendor,
    DateTime TimeOfArrival,
    string CreatedBy,
    DateTime CreatedAt,
    DateTime ModifiedAt);

internal sealed class GetShipmentByIdHandler(AppDbContext dbContext)
{

    public async Task<Result<GetShipmentByIdResponse>> GetShipmentByIdHandlerAsync(int id, CancellationToken ct)
    {
        var shipment = await dbContext.Shipments.FindAsync(id, ct);

        if (shipment is null)
        {
            return Result.Failure<GetShipmentByIdResponse>(Error.NotFound);
        }

        var createBy = await dbContext.Users
            .Where(u => u.UserId == shipment.UserId)
            .Select(u => u.FirstName)
            .FirstOrDefaultAsync(ct);

        var response = new GetShipmentByIdResponse(
            shipment.ShipmentId,
            shipment.PurchaseOrderNumber,
            shipment.Vendor,
            shipment.TimeOfArrival,
            createBy ?? string.Empty,
            shipment.CreatedAt,
            shipment.ModifiedAt);

        return Result.Success(response);
    }
}

public sealed class GetShipmentByIdEndpoint : IEndpoint
{
    public void Endpoint(IEndpointRouteBuilder app)
    {
        app.MapGet("/api/v1/shipments/{id}", async (int id, GetShipmentByIdHandler handler, CancellationToken ct) =>
        {
            var result = await handler.GetShipmentByIdHandlerAsync(id, ct);

            return result.IsSuccess
                ? Results.Ok(result.Value)
                : Results.NotFound(result.Error);
        });
    }
}