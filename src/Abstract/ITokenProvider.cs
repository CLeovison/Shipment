using Shipment.Entities;

namespace Shipment.Abstract;


public interface ITokenProvider
{
    string GenerateToken(Users users);
    string GenerateRefreshToken();
}