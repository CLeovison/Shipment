
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using Shipment.Abstract;
using Shipment.Abstract.Results;
using Shipment.Abstract.Results.Errors;
using Shipment.Database;
using Shipment.Extensions;

namespace Shipment.Features.Shipments.UpdateShipments;

internal sealed class UpdateShipmentHandler(AppDbContext dbContext)
{
    public async Task<Result<UpdateShipmentResponse>> UpdateShipmentsAsync(int id, UpdateShipmentRequest request, CancellationToken ct)
    {
        try
        {
            var existing = await dbContext.Shipments.FindAsync(id, ct);

            if (existing is null)
            {
                return Result.Failure<UpdateShipmentResponse>(Error.NullValue);
            }


            var updateUser = await dbContext.Users
                .Where(x => x.UserId == request.UserId)
                .Select(x => x.FirstName)
                .FirstOrDefaultAsync(ct);

            if (updateUser is null)
            {
                return Result.Failure<UpdateShipmentResponse>(Error.NullValue);
            }

            request.ToEntity(existing, updateUser);

            await dbContext.SaveChangesAsync(ct);

            var response = existing.ToResponse(updateUser);
            return Result.Success(response);
        }
        catch (Exception ex)
        {
            return Result.Failure<UpdateShipmentResponse>(
             new Error("UnhandledException", $"An unexpected error occurred: {ex.Message}")
         );

        }
    }
}


public sealed class UpdateShipmentEndpoint : IEndpoint
{
    public void Endpoint(IEndpointRouteBuilder app)
    {
        app.MapPut("/api/v1/shipments/{id}", async (
            int id,
            [FromBody] UpdateShipmentRequest request,
            [FromServices] UpdateShipmentHandler handler,
            CancellationToken ct) =>
        {
            try
            {
                var result = await handler.UpdateShipmentsAsync(id, request, ct);

                if (result.IsFailure)
                {

                    return result.Error.Code switch
                    {
                        "NullValue" => Results.NotFound(result.Error),
                        "DatabaseUpdateError" => Results.Json(result.Error, statusCode: 500),
                        "OperationCanceled" => Results.Json(result.Error, statusCode: 499),
                        _ => Results.BadRequest(result.Error)
                    };
                }

                return Results.Ok(result.Value);
            }

            catch (Exception ex)
            {

                var error = new Error("UnhandledException", $"Unexpected error: {ex.Message}");
                return Results.Json(error, statusCode: 500);
            }
        }).WithValidation<UpdateShipmentRequest>();
    }
}