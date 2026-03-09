using Microsoft.EntityFrameworkCore;
using Microsoft.IdentityModel.Tokens;
using Shipment.Database;

namespace Shipment.Features.Auth;

internal sealed class RefreshTokenHandler(AppDbContext dbContext)
{
    public async Task RefreshTokenAsync(string expiredToken, string refreshToken)
    {
        if (string.IsNullOrWhiteSpace(expiredToken))
        {
            throw new SecurityTokenExpiredException("Invalid Token");
        }

        if (string.IsNullOrWhiteSpace(refreshToken))
        {
            throw new SecurityTokenException("Invalid Token");
        }

        var tokenRotation = await dbContext.RefreshToken.Include(x => x.User).FirstOrDefaultAsync(x => x.Token == refreshToken);
    }
}