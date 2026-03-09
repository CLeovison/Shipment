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
    public async Task<Result<CreateShipmentResponse>> CreateShipmentAsync(
        ShipmentDetails shipment,
        CancellationToken ct)
    {
        var duplicateExists = await dbContext.Shipments
            .AnyAsync(x => x.PurchaseOrderNumber == shipment.PurchaseOrderNumber, ct);

        if (duplicateExists)
        {
            return Result.Failure<CreateShipmentResponse>(
                Error.AlreadyExists(nameof(Shipment)));
        }

        var userExists = await dbContext.Users
            .AnyAsync(u => u.UserId == shipment.UserId, ct);

        if (!userExists)
        {
            return Result.Failure<CreateShipmentResponse>(
                Error.NotFound);
        }

        dbContext.Shipments.Add(shipment);
        await dbContext.SaveChangesAsync(ct);


        var userFirstName = await dbContext.Users
            .Where(u => u.UserId == shipment.UserId)
            .Select(u => u.FirstName)
            .FirstAsync(ct);

        var response = shipment.ToResponse(userFirstName);
        return Result.Success(response);
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
            var shipmentEntity = request.ToRequest();
            var result = await handler.CreateShipmentAsync(shipmentEntity, ct);

            if (!result.IsSuccess)
            {
                return result.Error.Code switch
                {
                    "Error.AlreadyExists" => Results.Conflict(result.Error.Description),
                    "Error.NullValue" => Results.BadRequest(result.Error.Description),
                    _ => Results.BadRequest(result.Error.Description)
                };
            }

            return Results.Created(
                $"/api/shipments/{result.Value.PurchaseOrderNumber}",
                result.Value);
        })
        .WithValidation<CreateShipmentRequest>()
        .RequireAuthorization();
    }
}
