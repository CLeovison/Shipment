using System.Security.Claims;
using Microsoft.EntityFrameworkCore;
using Shipment.Abstract;
using Shipment.Database;

namespace Shipment.Features.Auth.RevokeRefreshToken;

internal sealed class RevokeRefreshToken(AppDbContext dbContext)
{
    public async Task RevokeRefreshTokenAsync(int userId, CancellationToken ct)
    {
        await dbContext.RefreshToken
            .Where(x => x.UserId == userId)
            .ExecuteDeleteAsync(ct);
    }
}

public sealed class RevokeRefreshTokenEndpoint : IEndpoint
{
    public void Endpoint(IEndpointRouteBuilder app)
    {
        app.MapDelete("/api/v1/auth/revoke",
        async (RevokeRefreshToken handler, ClaimsPrincipal user, CancellationToken ct) =>
        {
            var userClaim = user.FindFirst(ClaimTypes.NameIdentifier)?.Value;

            if (!int.TryParse(userClaim, out var userId))
                return Results.Unauthorized();

            await handler.RevokeRefreshTokenAsync(userId, ct);

            return Results.NoContent();
        }).RequireAuthorization();
    }
}