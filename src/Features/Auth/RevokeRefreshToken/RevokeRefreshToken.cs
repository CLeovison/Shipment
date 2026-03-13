using System.Security.Claims;
using Microsoft.EntityFrameworkCore;
using Shipment.Abstract;
using Shipment.Database;

namespace Shipment.Features.Auth.RevokeRefreshToken;

internal sealed class RevokeRefreshToken(AppDbContext dbContext, IHttpContextAccessor accessor)
{
    public async Task<bool> RevokeRefreshTokenAsync()
    {
        var claim = accessor.HttpContext?.User.FindFirst(ClaimTypes.NameIdentifier)?.Value;

        if (claim is null)
        {
            throw new UnauthorizedAccessException("User not authenticated");
        }

        var userId = int.Parse(claim);

        await dbContext.RefreshToken.Where(x => x.UserId == userId).ExecuteDeleteAsync();

        return true;
    }


}

public sealed class RevokeRefreshTokenEndpoint : IEndpoint
{
    public void Endpoint(IEndpointRouteBuilder app)
    {
        app.MapDelete("/api/v1/auth/revoke", async (RevokeRefreshToken handler) =>
        {
            await handler.RevokeRefreshTokenAsync();
            return Results.Ok();
        });
      
    }
}