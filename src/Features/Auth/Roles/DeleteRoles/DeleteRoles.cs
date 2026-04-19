using Shipmennt.Entities;
using Shipment.Abstract;
using Shipment.Abstract.Results;
using Shipment.Abstract.Results.Errors;
using Shipment.Database;

namespace Shipment.Features.Auth.Roles.DeleteRoles;


internal sealed class DeleteRolesHandler(AppDbContext dbContext)
{
    public async Task<Result<Role>> DeleteRolesAsync(int id, CancellationToken ct)
    {

        var existingRole = await dbContext.Roles.FindAsync(id, ct);

        if (existingRole is null)
        {
            return Result.Failure<Role>(Error.NotFound);
        }

        dbContext.Roles.Remove(existingRole);
        await dbContext.SaveChangesAsync();

        return Result.Success(existingRole);
    }
}


public sealed class DeleteRolesEndpoint : IEndpoint
{
    public void Endpoint(IEndpointRouteBuilder app)
    {
        app.MapDelete("/api/v1/auth/role/{id}", async (int id, DeleteRolesHandler handler, CancellationToken ct) =>
        {
            try
            {
                var result = await handler.DeleteRolesAsync(id, ct);

                if (result.IsFailure)
                {
                    return Results.NotFound();
                }
                return Results.NoContent();
            }
            catch (Exception ex)
            {
                return Results.Problem(detail: ex.Message, statusCode: 500, title: "An error occurred while deleting the role");
            }
        }).RequireAuthorization();
    }
}