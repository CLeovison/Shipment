using System.Security.Claims;
using Shipment.Entities;

namespace Shipment.Abstract;


public interface ITokenProvider
{
    string GenerateToken(Users users);
    string GenerateRefreshToken();
    
    ClaimsPrincipal GetClaimsPrincipal(string accessToken);
}