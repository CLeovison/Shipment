using Microsoft.AspNetCore.Identity;
using Microsoft.EntityFrameworkCore;
using Shipment.Abstract;
using Shipment.Abstract.Results;
using Shipment.Abstract.Results.Errors;
using Shipment.Database;
using Shipment.Entities;

namespace Shipment.Features.Auth.Register;


internal sealed class RegisterHandler(AppDbContext dbContext, PasswordHasher<Users> passwordHasher)
{
    public async Task<Result<RegisterRequest>> RegisterAsync(RegisterRequest request, CancellationToken ct)
    {
        var exist = await dbContext.Users.SingleAsync(x => x.Username == request.Username, ct);

        if (exist is not null)
        {
            return Result.Failure<RegisterRequest>(Error.AlreadyExists("user"));
        }

        var create = request.ToReqisterRequest();
        create.Password = passwordHasher.HashPassword(create, request.Password);

        dbContext.Users.Add(create);
        await dbContext.SaveChangesAsync(ct);

        return Result.Success(request);
    }
}

public sealed class RegisterEndpoint : IEndpoint
{
    public void Endpoint(IEndpointRouteBuilder app)
    {
        app.MapPost("/api/v1/auth/register", async (RegisterHandler handler) =>
        {

        });
    }
}