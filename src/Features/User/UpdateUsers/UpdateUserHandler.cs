using Shipment.Abstract;
using Shipment.Abstract.Results;
using Shipment.Abstract.Results.Errors;
using Shipment.Database;

namespace Shipment.Features.User.UpdateUsers;

internal sealed class UpdateUserHandler(AppDbContext dbContext)
{
    public async Task<Result<UpdateUserRequest>> UpdateUserAsync(UpdateUserRequest request, CancellationToken ct)
    {
        var existing = await dbContext.Users.FindAsync(request.UserId, ct);
        if (existing is null)
        {
            return Result.Failure<UpdateUserRequest>(Error.DidntExists("users"));
        }

        request.ToEntity(existing);

        dbContext.Users.Update(existing);
        await dbContext.SaveChangesAsync(ct);

        return Result.Success(request);
    }
}


public sealed class UpdateUserEndpoint : IEndpoint
{
    public void Endpoint(IEndpointRouteBuilder app)
    {
        app.MapPut("/api/v1/users/{id}", async (int id, UpdateUserRequest request, UpdateUserHandler handler, CancellationToken ct) =>
        {
         
            request = request with { UserId = id };

            var result = await handler.UpdateUserAsync(request, ct);

            if (result.IsSuccess)
            {
                return Results.Ok(result.Value); 
            }

            // You can customize error handling here
            return Results.NotFound(result.Error); // 404 Not Found if user doesn't exist
        });
    }
}
