using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using Shipment.Abstract;
using Shipment.Abstract.Results;
using Shipment.Abstract.Results.Errors;
using Shipment.Database;
using Shipment.Entities;
using Shipment.Extensions;

namespace Shipment.Features.Shipments.CreateShipments;

internal sealed class CreateShipmentHandler(AppDbContext dbContext)
{
    public async Task<Result<ShipmentDetails>> CreateShipmentAsync(
        ShipmentDetails shipment,
        CancellationToken ct)
    {

        var duplicateExists = await dbContext.Shipments
            .AnyAsync(x => x.PurchaseOrderNumber == shipment.PurchaseOrderNumber, ct);

        if (duplicateExists)
        {
            return Result.Failure<ShipmentDetails>(
                Error.AlreadyExists(nameof(Shipment)));
        }

        dbContext.Shipments.Add(shipment);
        await dbContext.SaveChangesAsync(ct);

        return Result.Success(shipment);
    }

}

public sealed class CreateShipmentEndpoint : IEndpoint
{
    public void Endpoint(IEndpointRouteBuilder app)
    {
        app.MapPost("/api/v1/shipments/create", async (
            [FromBody] CreateShipmentRequest request,
            CreateShipmentHandler handler,
            CancellationToken ct) =>
        {
            var shipmentRequest = request.ToRequest();
            var result = await handler.CreateShipmentAsync(shipmentRequest, ct);

            if (!result.IsSuccess)
            {
                return result.Error.Code switch
                {
                    "Error.AlreadyExists" => Results.Conflict(result.Error.Description),
                    "Error.NullValue" => Results.BadRequest(result.Error.Description),
                    _ => Results.BadRequest(result.Error.Description)
                };
            }

            var response = result.Value.ToResponse();
            return Results.Created(
                $"/api/shipments/{response.PurchaseOrderNumber}",
                response);
        })
        .WithValidation<CreateShipmentRequest>();
    }
}