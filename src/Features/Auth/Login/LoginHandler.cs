using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Identity.Data;
using Microsoft.EntityFrameworkCore;
using Shipment.Abstract;
using Shipment.Abstract.Results;
using Shipment.Abstract.Results.Errors;
using Shipment.Auth;
using Shipment.Database;
using Shipment.Entities;

namespace Shipment.Features.Auth.Login;

internal sealed class LoginHandler(AppDbContext dbContext,PasswordHasher<Users> passwordHasher, TokenProvider tokenProvider)
{
    public record class LoginRequest(string Username, string Password);

    public async Task<Result<string>> LoginAsync(LoginRequest request)
    {
        var users = await dbContext.Users.SingleOrDefaultAsync(x => x.Username == request.Username);

        if(users is null)
        {
            return Result.Failure<string>(Error.NotFound);
        }

        bool verified = passwordHasher.VerifyHashedPassword(users, request.Password, users.Password) != PasswordVerificationResult.Failed;

        string token = tokenProvider.GenerateToken(users);

        return token;
    }
}

public sealed class LoginEdpoint : IEndpoint
{
    public void Endpoint(IEndpointRouteBuilder app)
    {
        app.MapPost("/api/v1/login", async (LoginRequest request, LoginHandler handler) =>
        {
            
        });
    }
}