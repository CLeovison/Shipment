using Shipment.Abstract;
using Shipment.Abstract.Results;
using Shipment.Abstract.Results.Errors;
using Shipment.Database;
using Shipment.Entities;

namespace Shipment.Features.User.DeleteUsers;

internal sealed class DeleteUserHandler(AppDbContext dbContext)
{
    public async Task<Result<Users>> DeleteUserAsync(int id, CancellationToken ct)
    {
        try
        {
            var existing = await dbContext.Users.FindAsync(id, ct);

            if (existing is null)
            {
                return Result.Failure<Users>(Error.DidntExists("user"));
            }

            dbContext.Users.Remove(existing);
            await dbContext.SaveChangesAsync(ct);

            return Result.Success(existing);
        }
        catch (Exception)
        {
            return Result.Failure<Users>(
                new Error("Error.Unexpected", "An unexpected error occurred while deleting the user.")
            );
        }
    }
}

public sealed class DeleteUserEndpoint : IEndpoint
{
    public void Endpoint(IEndpointRouteBuilder app)
    {
        app.MapDelete("/api/v1/users/{id}", async (int id, DeleteUserHandler handler, CancellationToken ct) =>
        {
            try
            {
                var removed = await handler.DeleteUserAsync(id, ct);

                if (removed.IsFailure)
                {
                    return Results.NotFound(new { message = "User not found or could be deleted." });
                }

                return Results.Ok(new { message = "The user is successfully deleted." });
            }
            catch (Exception ex)
            {
                return Results.Problem(
                  detail: ex.Message,
                  statusCode: 500,
                  title: "An error occurred while deleting the user"
              );

            }
        });
    }
}