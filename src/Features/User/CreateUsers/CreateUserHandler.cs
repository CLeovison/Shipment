using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using Shipment.Abstract;
using Shipment.Abstract.Results;
using Shipment.Abstract.Results.Errors;
using Shipment.Database;
using Shipment.Entities;
using Shipment.Extension;

namespace Shipment.Features.User.CreateUsers;

internal sealed class CreateUserHandler(AppDbContext dbContext)
{
    public async Task<Result<Users>> CreateUserAsync(Users users, CancellationToken ct)
    {
        try
        {
            if (string.IsNullOrWhiteSpace(users.Username))
            {
                return Result.Failure<Users>(Error.NullValue);
            }

            var exists = await dbContext.Users.AnyAsync(u => u.Username == users.Username, ct);
            if (exists)
            {
                return Result.Failure<Users>(Error.AlreadyExists("user"));
            }

            dbContext.Users.Add(users);
            await dbContext.SaveChangesAsync(ct);

            return Result.Success(users);
        }
        catch (Exception)
        {
            return Result.Failure<Users>(
                new Error("Error.Unexpected", "An unexpected error occurred while creating the user.")
            );
        }
    }
}

public class CreateUserEndpoint : IEndpoint
{
    public void Endpoint(IEndpointRouteBuilder app)
    {
        app.MapPost("/api/v1/users/create", async (
            [FromBody] CreateUserRequest request,
            CreateUserHandler handler,
            CancellationToken ct) =>
        {
            try
            {
                var userEntity = request.ToUserCreateRequest();
                var create = await handler.CreateUserAsync(userEntity, ct);

                if (!create.IsSuccess)
                {
                    return create.Error.Code switch
                    {
                        "Error.AlreadyExists" => Results.Conflict(create.Error.Description),
                        "Error.NullValue" => Results.BadRequest(create.Error.Description),
                        _ => Results.BadRequest(create.Error.Description)
                    };
                }

                var response = create.Value.ToCreateUserResponse();
                return Results.Created($"/api/v1/users/{response.Username}", response);
            }
            catch (Exception)
            {
                return Results.Problem(
                    detail: "An unexpected error occurred while creating user",
                    statusCode: StatusCodes.Status500InternalServerError
                );
            }
        })
        .WithValidation<CreateUserRequest>();
    }
}