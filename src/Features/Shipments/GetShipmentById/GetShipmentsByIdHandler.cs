using Shipment.Abstract;
using Shipment.Abstract.Results;
using Shipment.Abstract.Results.Errors;
using Shipment.Database;
using Microsoft.EntityFrameworkCore;

namespace Shipment.Features.Shipments.GetShipmentsById;

internal sealed class GetShipmentByIdHandler(AppDbContext dbContext)
{

    public async Task<Result<GetShipmentByIdResponse>> GetShipmentByIdHandlerAsync(int id, CancellationToken ct)
    {
        var shipment = await dbContext.Shipments.FindAsync(id, ct);

        if (shipment is null)
        {
            return Result.Failure<GetShipmentByIdResponse>(Error.NotFound);
        }

        var createdBy = await dbContext.Users
            .Where(u => u.UserId == shipment.UserId)
            .Select(u => u.FirstName)
            .FirstOrDefaultAsync(ct);

        if (createdBy is null)
        {
            return Result.Failure<GetShipmentByIdResponse>(Error.NotFound);
        }
        
        var response = shipment.ToResponse(createdBy);
        
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