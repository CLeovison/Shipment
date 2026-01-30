using Microsoft.EntityFrameworkCore;
using Shipment.Abstract;
using Shipment.Abstract.Results;
using Shipment.Abstract.Results.Errors;
using Shipment.Database;
using Shipment.Entities;

namespace Shipment.Features.User.GetUserById;

internal sealed class GetUserByIdHandler(AppDbContext dbContext)
{
    public async Task<Result<Users>> GetUserByIdAsync(int id, CancellationToken ct)
    {
        try
        {
            var user = await dbContext.Users
                .SingleOrDefaultAsync(u => u.UserId == id, ct);

            if (user is null)
            {
                return Result.Failure<Users>(Error.NotFound);
            }

            return Result.Success(user);
        }
        catch (OperationCanceledException)
        {
            return Result.Failure<Users>(Error.Cancelled);
        }

    }
}

public sealed class GetUserByIdEndpoint : IEndpoint
{
    public void Endpoint(IEndpointRouteBuilder app)
    {
        app.MapGet("/api/v1/users/{id}", async (int id, GetUserByIdHandler handler, CancellationToken ct) =>
        {
            try
            {
                var result = await handler.GetUserByIdAsync(id, ct);

                if (result.IsFailure)
                {
                    return Results.Problem(result.Error.Description, statusCode: 400);
                }

                return Results.Ok(result.Value);
            }
            catch (Exception ex)
            {
                return Results.Problem($"Unexpected error: {ex.Message}", statusCode: 500);
            }
        });
    }
}