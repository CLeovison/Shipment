using Microsoft.EntityFrameworkCore;
using Shipment.Abstract;
using Shipment.Abstract.Results;
using Shipment.Abstract.Results.Errors;
using Shipment.Database;

namespace Shipment.Features.Auth.Logout;

internal sealed class LogoutHandler(AppDbContext _dbContext)
{
    public async Task<Result<string>> LogoutAsync(string? refreshToken)
    {

        if (string.IsNullOrWhiteSpace(refreshToken))
        {
            return Result.Success("You are already logged out.");
        }

        try
        {
            var tokenEntity = await _dbContext.RefreshToken
                .Where(x => x.Token == refreshToken)
                .FirstOrDefaultAsync();

            if (tokenEntity is null)
            {
                // Token already removed → still a success
                return Result.Success("You are already logged out.");
            }

            _dbContext.RefreshToken.Remove(tokenEntity);
            await _dbContext.SaveChangesAsync();

            return Result.Success("You've successfully logged out");
        }
        catch
        {
            return Result.Failure<string>(Error.Logout);
        }
    }
}

public sealed class LogoutEndpoint : IEndpoint
{
    public void Endpoint(IEndpointRouteBuilder app)
    {
        app.MapPost("/api/v1/auth/logout", async (HttpContext httpContext, LogoutHandler handler) =>
        {
            var refreshToken = httpContext.Request.Cookies["refreshToken"];
            var result = await handler.LogoutAsync(refreshToken);

            // Always delete cookies
            httpContext.Response.Cookies.Delete("accessToken");
            httpContext.Response.Cookies.Delete("refreshToken");

            // Customize JSON output to remove empty error object on success
            if (result.IsSuccess)
            {
                return Results.Ok(new { message = result.Value });
            }
            else
            {
                return Results.BadRequest(new { error = result.Error });
            }
        });
    }
}