
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using Shipment.Abstract;
using Shipment.Abstract.Results;
using Shipment.Abstract.Results.Errors;
using Shipment.Database;
using Shipment.Extensions;
using Shipment.Features.Shipments.Shared;

namespace Shipment.Features.Shipments.UpdateShipments;

internal sealed class UpdateShipmentHandler(AppDbContext dbContext)
{
    public async Task<Result<ShipmentResponse>> UpdateShipmentsAsync(ShipmentRequest request, CancellationToken ct)
    {
        var existing = await dbContext.Shipments.FindAsync(request.ShipmentId, ct);

        if (existing is null)
        {
            return Result.Failure<ShipmentResponse>(Error.NullValue);
        }

        var updateUser = await dbContext.Users.Where(x => x.UserId == request.UserId).Select(x => x.FirstName).FirstAsync(ct);

        request.ToEntity(existing, updateUser);

        var response = existing.ToResponse(updateUser);
        await dbContext.SaveChangesAsync(ct);
        return Result.Success(response);
    }
}

public sealed class UpdateShipmentEndpoint : IEndpoint
{
    public void Endpoint(IEndpointRouteBuilder app)
    {
     app.MapPut("/api/v1/shipments/{id}", async (
    int id,
    [FromBody] ShipmentRequest request,
    [FromServices] UpdateShipmentHandler handler,
    CancellationToken ct) =>
{

    request.ShipmentId = id;

    var result = await handler.UpdateShipmentsAsync(request, ct);

    if (result.IsFailure)
    {
        return Results.BadRequest(result.Error);
    }

    return Results.Ok(result.Value);
});

    }
}