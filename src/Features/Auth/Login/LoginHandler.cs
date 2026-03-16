using Microsoft.AspNetCore.Identity;

using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using Shipment.Abstract;

using Shipment.Database;
using Shipment.Entities;
using Shipment.Extensions;

namespace Shipment.Features.Auth.Login;

public record class LoginRequest(string Username, string Password);

public record class LoginResponse(string accessToken, string refreshToken);
internal sealed class LoginHandler(
    AppDbContext dbContext, PasswordHasher<Users> passwordHasher,
    ITokenProvider tokenProvider)
{

    public async Task<LoginResponse> LoginAsync(LoginRequest request, CancellationToken ct)
    {
        var users = await dbContext.Users.SingleOrDefaultAsync(x => x.Username == request.Username, ct);

        if (users is null)
        {
            throw new UnauthorizedAccessException("Invalid credentials");
        }

        var verified = passwordHasher.VerifyHashedPassword(users, users.Password, request.Password);

        if (verified == PasswordVerificationResult.Failed)
        {
            throw new UnauthorizedAccessException("Invalid credentials");
        }

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
        app.MapPost("/api/v1/auth/login", async ([FromBody] LoginRequest request, LoginHandler handler, CancellationToken ct,
         HttpContext httpContext) =>
        {
            var login = await handler.LoginAsync(request, ct);

            httpContext.StoredTokenInCookie("accessToken", login.accessToken, DateTime.UtcNow.AddMinutes(2));
            httpContext.StoredTokenInCookie("refreshToken", login.refreshToken, DateTime.UtcNow.AddDays(7));

            return Results.Ok(login);
        });
    }
}