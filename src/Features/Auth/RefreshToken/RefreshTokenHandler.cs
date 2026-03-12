using Microsoft.EntityFrameworkCore;
using Microsoft.IdentityModel.Tokens;
using Shipment.Abstract;
using Shipment.Database;

namespace Shipment.Features.Auth;

internal sealed class RefreshTokenHandler(AppDbContext dbContext, ITokenProvider tokenProvider)
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

        if (tokenRotation is null)
        {
            throw new SecurityTokenArgumentException("Unable to retrieve user for refresh token");
        }

        if (tokenRotation.ExpiresAt < DateTime.UtcNow)
        {
            throw new SecurityTokenExpiredException("The refresh token is expired.");
        }

        if (tokenRotation.User is null)
        {
            throw new KeyNotFoundException("The user is not existing");
        }
        var newAccessToken = tokenProvider.GenerateToken(tokenRotation.User);
        var newRefreshToken = tokenProvider.GenerateRefreshToken();

        tokenRotation.Token = newRefreshToken;

        await dbContext.SaveChangesAsync();
    }
}