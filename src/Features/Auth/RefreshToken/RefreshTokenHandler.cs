using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using Shipment.Abstract;
using Shipment.Database;
using Shipment.Extensions;

namespace Shipment.Features.Auth.RefreshToken;

public record class Request(string RefreshToken);
public record class Response(string AccessToken, string RefreshToken);

internal sealed class RefreshTokenHandler(AppDbContext dbContext, ITokenProvider tokenProvider)
{
    public async Task<Response> RefreshTokenAsync(Request request)
    {
        var tokenEntity = await dbContext.RefreshToken
            .Include(x => x.User)
            .SingleOrDefaultAsync(x => x.Token == request.RefreshToken);

        if (tokenEntity is null || tokenEntity.ExpiresAt < DateTime.UtcNow)
            throw new ApplicationException("The refresh token is expired");

        if (tokenEntity.User is null)
            throw new NullReferenceException("The user does not exist");

        var accessToken = tokenProvider.GenerateToken(tokenEntity.User);

        tokenEntity.Token = tokenProvider.GenerateRefreshToken();
        tokenEntity.ExpiresAt = DateTime.UtcNow.AddDays(7);

        await dbContext.SaveChangesAsync();

        return new Response(accessToken, tokenEntity.Token);
    }
}

public sealed class RefreshTokenEndpoint : IEndpoint
{
    public void Endpoint(IEndpointRouteBuilder app)
    {
        app.MapPost("/api/v1/auth/refreshToken", async ([FromBody] Request request, RefreshTokenHandler handler, HttpContext httpContext
        ) =>
        {
            try
            {
                var response = await handler.RefreshTokenAsync(request);

                httpContext.StoredTokenInCookie("accessToken", response.AccessToken, DateTime.UtcNow.AddMinutes(15));
                httpContext.StoredTokenInCookie("refreshToken", response.RefreshToken, DateTime.UtcNow.AddDays(7));

                return Results.Ok(response);
            }
            catch
            {
                return Results.Unauthorized();
            }
        });
    }
}