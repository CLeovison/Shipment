using Shipment.Abstract;
using Shipment.Database;

namespace Shipment.Features.Auth.RevokeRefreshToken;

internal sealed class RevokeRefreshtToken(AppDbContext dbContext, IHttpContextAccessor accessor)
{

    public async Task RevokeRefreshTokenAsync()
    {

    }

}

public sealed class RevokeRefreshTokenEndpoint : IEndpoint
{
    public void Endpoint(IEndpointRouteBuilder app)
    {

    }
}