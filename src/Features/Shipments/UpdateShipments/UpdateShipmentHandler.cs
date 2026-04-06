

using System.Security.Claims;
using Microsoft.AspNetCore.Mvc;
using Shipment.Abstract;
using Shipment.Abstract.Results;
using Shipment.Abstract.Results.Errors;
using Shipment.Database;
using Shipment.Extensions;

namespace Shipment.Features.Shipments.UpdateShipments;

internal sealed class UpdateShipmentHandler(AppDbContext dbContext, IHttpContextAccessor httpContext)
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

            var updateUser = httpContext.HttpContext?.User?.FindFirst(ClaimTypes.NameIdentifier)?.Value;

            if (string.IsNullOrWhiteSpace(updateUser) || !int.TryParse(updateUser, out var userId))
            {
                return Result.Failure<UpdateShipmentResponse>(Error.NullValue);
            }


            request.ToEntity(existing, updateUser);

            existing.UserId = userId;

            var arrival = EnsureUtc(existing.TimeOfArrival);
            existing.NotifyStartAt = arrival.AddDays(-14);


            var userName = httpContext.HttpContext?.User?
                     .FindFirst(ClaimTypes.Name)?.Value ?? "Unknown";


            await dbContext.SaveChangesAsync(ct);

            var response = existing.ToResponse(userName);
            return Result.Success(response);
        }
        catch (Exception ex)
        {
            return Result.Failure<UpdateShipmentResponse>(
             new Error("UnhandledException", $"An unexpected error occurred: {ex.Message}")
         );

        }
    }
    private static DateTime EnsureUtc(DateTime date)
    {
        var withTime = date.TimeOfDay == TimeSpan.Zero
            ? date.Date.AddHours(8)
            : date;

        return DateTime.SpecifyKind(TimeZoneInfo.ConvertTimeToUtc(withTime), DateTimeKind.Utc);
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
        })
        .RequireAuthorization()
        .WithValidation<UpdateShipmentRequest>();
    }
}