using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using Shipment.Abstract;
using Shipment.Abstract.Results;
using Shipment.Abstract.Results.Errors;
using Shipment.Database;
using Shipment.Entities;
using Shipment.Extensions;

namespace Shipment.Features.User.CreateUsers;

internal sealed class CreateUserHandler(AppDbContext dbContext, PasswordHasher<Users> passwordHasher)
{
    public async Task<Result<Users>> CreateUserAsync(CreateUserRequest request, CancellationToken ct)
    {
        try
        {
            if (string.IsNullOrWhiteSpace(request.Username))
            {
                return Result.Failure<Users>(Error.NullValue);
            }

            if (await dbContext.Users.AnyAsync(u => u.Username.ToLower() == request.Username, ct))
            {
                return Result.Failure<Users>(Error.AlreadyExists("user"));
            }

            var user = request.ToUserCreateRequest();

            user.Password = passwordHasher.HashPassword(user, request.Password);

            dbContext.Users.Add(user);
            await dbContext.SaveChangesAsync(ct);

            return Result.Success(user);
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
        app.MapPost("/api/v1/users/create", async ([FromBody] CreateUserRequest request, CreateUserHandler handler, CancellationToken ct) =>
        {
            try
            {
                var result = await handler.CreateUserAsync(request, ct);

                if (!result.IsSuccess)
                {
                    return result.Error.Code switch
                    {
                        "Error.AlreadyExists" => Results.Conflict(result.Error.Description),
                        "Error.NullValue" => Results.BadRequest(result.Error.Description),
                        _ => Results.BadRequest(result.Error.Description)
                    };
                }
                var response = result.Value.ToCreateUserResponse();

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