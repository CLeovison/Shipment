using Microsoft.EntityFrameworkCore;
using Shipmennt.Entities;
using Shipment.Abstract;
using Shipment.Abstract.Results;
using Shipment.Abstract.Results.Errors;
using Shipment.Database;

namespace Shipment.Features.Auth.Roles;

internal sealed class CreateRoleHandler(AppDbContext dbContext)
{
    public async Task<Result<RoleResponse>> RoleAsync(RoleRequest request, CancellationToken ct)
    {
        var roleExist = await dbContext.Roles.SingleOrDefaultAsync(x => x.RoleName == request.RoleName, ct);

        if (roleExist is not null)
        {
            return Result.Failure<RoleResponse>(Error.AlreadyExists(nameof(Role)));
        }

        var roleCreation = request.ToRoleRequest();


        await dbContext.Roles.AddAsync(roleCreation);
        await dbContext.SaveChangesAsync(ct);

        var response = roleCreation.ToRoleResponse();

        return Result.Success(response);

    }
}


public sealed class RoleEndpoint : IEndpoint
{
    public void Endpoint(IEndpointRouteBuilder app)
    {
        app.MapPost("/api/v1/auth/role/create", async (CreateRoleHandler handler, RoleRequest request, CancellationToken ct) =>
        {
            var result = await handler.RoleAsync(request, ct);

            if (!result.IsSuccess)
            {
                return result.Error.Code switch
                {
                    "Error.AlreadyExists" => Results.Conflict(result.Error.Description),
                    "Error.NullValue" => Results.BadRequest(result.Error.Description),
                    _ => Results.BadRequest(result.Error.Description)
                };
            }

            return Results.Created($"/api/v1/auth/role/{result.Value.RoleName}", result.Value);
        }).RequireAuthorization();
    }
}