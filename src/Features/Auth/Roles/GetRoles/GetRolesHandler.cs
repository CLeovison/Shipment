using Microsoft.EntityFrameworkCore;
using Shipmennt.Entities;
using Shipment.Abstract;
using Shipment.Database;

namespace Shipment.Features.Auth.Roles.GetRoles;

internal sealed class GetRolesHandler(AppDbContext dbContext)
{
    public async Task<List<Role>> GetRolesAsync(CancellationToken ct)
    {
        return await dbContext.Roles.ToListAsync(ct);
    }
}

public sealed class GetRolesEndpoint : IEndpoint
{
    public void Endpoint(IEndpointRouteBuilder app)
    {
        app.MapGet("/api/v1/auth/roles", async (GetRolesHandler handler, CancellationToken ct) =>
        {
            return await handler.GetRolesAsync(ct);
        });
    }
}