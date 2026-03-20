using System.IdentityModel.Tokens.Jwt;
using System.Security.Claims;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.SignalR;
using Microsoft.EntityFrameworkCore;
using Shipment.Abstract;
using Shipment.Abstract.Results;
using Shipment.Abstract.Results.Errors;
using Shipment.Database;
using Shipment.Entities;
using Shipment.Extensions;
using Shipment.Hubs;

namespace Shipment.Features.Shipments.CreateShipments;

internal sealed class CreateShipmentHandler(
    AppDbContext dbContext,
    IHubContext<ShipmentNotificationHub, IShipmentClient> hub,
    IHttpContextAccessor httpContext)
{
    public async Task<Result<CreateShipmentResponse>> CreateShipmentAsync(ShipmentDetails shipment, CancellationToken ct)
    {
        var userIdClaim = httpContext.HttpContext?.User?
            .FindFirst(ClaimTypes.NameIdentifier)?.Value;

        if (string.IsNullOrWhiteSpace(userIdClaim) || !int.TryParse(userIdClaim, out var userId))
        {
            return Result.Failure<CreateShipmentResponse>(Error.Unauthorized);
        }

        shipment.UserId = userId;

        var duplicateExists = await dbContext.Shipments
            .AnyAsync(x => x.PurchaseOrderNumber == shipment.PurchaseOrderNumber, ct);

        if (duplicateExists)
            return Result.Failure<CreateShipmentResponse>(Error.AlreadyExists(nameof(Shipment)));

        dbContext.Shipments.Add(shipment);
        await dbContext.SaveChangesAsync(ct);

        var userName = httpContext.HttpContext?.User?
            .FindFirst(ClaimTypes.Name)?.Value ?? "Unknown";

        var response = shipment.ToResponse(userName);

        await hub.Clients.User(userId.ToString()).ShipmentCreated(response);

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
            try
            {
                var shipmentEntity = request.ToRequest();
                var result = await handler.CreateShipmentAsync(shipmentEntity, ct);

                if (!result.IsSuccess)
                {
                    return result.Error.Code switch
                    {
                        "Error.AlreadyExists" => Results.Conflict(result.Error.Description),
                        "Error.NullValue" => Results.BadRequest(result.Error.Description),
                        "Error.NotFound" => Results.NotFound(result.Error.Description),
                        _ => Results.BadRequest(result.Error.Description)
                    };
                }

                return Results.Created(
                    $"/api/v1/shipments/{result.Value.PurchaseOrderNumber}",
                    result.Value);
            }
            catch (Exception ex)
            {

                return Results.Problem(
                    detail: ex.Message,
                    statusCode: 500,
                    title: "An unexpected error occurred while creating the shipment");
            }
        })
        .WithValidation<CreateShipmentRequest>().RequireAuthorization();
    }
}