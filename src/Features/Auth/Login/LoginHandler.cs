using Microsoft.AspNetCore.Identity;

using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using Shipment.Abstract;

using Shipment.Database;
using Shipment.Entities;

namespace Shipment.Features.Auth.Login;

public record class LoginRequest(string Username, string Password);

public record class LoginResponse(string accessToken, string refreshToken);
internal sealed class LoginHandler(AppDbContext dbContext, PasswordHasher<Users> passwordHasher, ITokenProvider tokenProvider)
{

    public async Task<LoginResponse> LoginAsync(LoginRequest request)
    {
        var users = await dbContext.Users.SingleOrDefaultAsync(x => x.Username == request.Username);

        if (users is null)
        {
            throw new NullReferenceException("There is no existing user");
        }

        bool verified = passwordHasher.VerifyHashedPassword(users, users.Password, request.Password) != PasswordVerificationResult.Failed;

        string accessToken = tokenProvider.GenerateToken(users);

        var refreshToken = new RefreshTokens
        {
            RefreshTokenId = Guid.NewGuid(),
            UserId = users.UserId,
            Token = tokenProvider.GenerateRefreshToken(),
            CreatedAt = DateTime.UtcNow,
            ExpiresAt = DateTime.UtcNow.AddDays(7)
        };

        var storedToken = await dbContext.RefreshToken.AddAsync(refreshToken);
        await dbContext.SaveChangesAsync();

        return new LoginResponse(accessToken, refreshToken.Token);
    }
}

public sealed class LoginEdpoint : IEndpoint
{
    public void Endpoint(IEndpointRouteBuilder app)
    {
        app.MapPost("/api/v1/auth/login", async ([FromBody] LoginRequest request, LoginHandler handler) =>
        {
            var login = await handler.LoginAsync(request);

            return Results.Ok(login);
        });
    }
}