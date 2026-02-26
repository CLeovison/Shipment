using Microsoft.AspNetCore.Mvc;
using Shipment.Abstract;
using Shipment.Abstract.Results;
using Shipment.Abstract.Results.Errors;
using Shipment.Database;

namespace Shipment.Features.User.UpdateUsers;

public record class UpdateUserRequest(
    int UserId,
    string FirstName,
    string LastName,
    string Username,
    string Password,
    DateTime Birthday,
    DateTime ModifiedAt);

internal sealed class UpdateUserHandler(AppDbContext dbContext)
{
    public async Task<Result<UpdateUserRequest>> UpdateUserAsync(int id, UpdateUserRequest request, CancellationToken ct)
    {
        try
        {
            var existing = await dbContext.Users.FindAsync(id, ct);

            if (existing is null)
            {
                return Result.Failure<UpdateUserRequest>(Error.DidntExists("users"));
            }

            request.ToEntity(existing, id);

            dbContext.Users.Update(existing);
            await dbContext.SaveChangesAsync(ct);

            var requestWithId = request with { UserId = id };
            return Result.Success(requestWithId);
        }
        catch (Exception ex)
        {
            return Result.Failure<UpdateUserRequest>(
                new Error("UnhandledException", $"Unexpected error: {ex.Message}")
            );
        }
    }
}

public sealed class UpdateUserEndpoint : IEndpoint
{
    public void Endpoint(IEndpointRouteBuilder app)
    {
        app.MapPut("/api/v1/users/{id}", async (
            int id,
            [FromBody] UpdateUserRequest request,
            [FromServices] UpdateUserHandler handler,
            CancellationToken ct) =>
        {
            try
            {
                var result = await handler.UpdateUserAsync(id, request, ct);

                if (result.IsSuccess)
                {
                    return Results.Ok(result.Value);
                }

                return result.Error.Code switch
                {
                    "DidntExists"        => Results.NotFound(result.Error),
                    "DatabaseUpdateError"=> Results.Json(result.Error, statusCode: 500),
                    "OperationCanceled"  => Results.Json(result.Error, statusCode: 499),
                    _                    => Results.BadRequest(result.Error)
                };
            }
            catch (Exception ex)
            {
                var error = new Error("UnhandledException", $"Unexpected error: {ex.Message}");
                return Results.Json(error, statusCode: 500);
            }
        });
    }
}