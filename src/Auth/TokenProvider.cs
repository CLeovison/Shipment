using System.IdentityModel.Tokens.Jwt;
using System.Security.Claims;
using System.Security.Cryptography;
using System.Text;
using Microsoft.IdentityModel.Tokens;
using Shipment.Abstract;
using Shipment.Entities;

namespace Shipment.Auth;

public sealed class TokenProvider(IConfiguration configuration) : ITokenProvider
{
    public string GenerateToken(Users user)
    {
        string? secretKey = configuration["Jwt:SecretKey"]!;

        var securityKey = new SymmetricSecurityKey(Encoding.UTF8.GetBytes(secretKey));

        var credentials = new SigningCredentials(securityKey, SecurityAlgorithms.HmacSha512Signature);

        var tokenDescriptor = new SecurityTokenDescriptor
        {
            Subject = new ClaimsIdentity(
            [
                new Claim(JwtRegisteredClaimNames.Sub, user.UserId.ToString()),
                new Claim(JwtRegisteredClaimNames.Name, user.FirstName),
                new Claim(JwtRegisteredClaimNames.Jti, Guid.NewGuid().ToString()),
            ]
            ),
            Expires = DateTime.UtcNow.AddMinutes(configuration.GetValue<int>("Jwt:Expirations")),
            SigningCredentials = credentials,
            Audience = configuration["Jwt:Audience"],
            Issuer = configuration["Jwt:Issuer"]
        };

        var handler = new JwtSecurityTokenHandler();

        return handler.CreateEncodedJwt(tokenDescriptor);
    }

    public string GenerateRefreshToken()
    {
        return Convert.ToBase64String(RandomNumberGenerator.GetBytes(32));
    }

    public ClaimsPrincipal GetClaimsPrincipal(string accessToken)
    {
        var tokenValidParameters = new TokenValidationParameters
        {
            IssuerSigningKey = new SymmetricSecurityKey(Encoding.UTF8.GetBytes(configuration["Jwt:SecretKey"]!)),
            ValidateAudience = true,
            ValidateIssuer = true,
            ValidateIssuerSigningKey = true,
            ValidIssuer = configuration["Jwt:Issuer"],
            ValidAudience = configuration["Jwt:Audience"],
            ClockSkew = TimeSpan.Zero
        };

        var tokenHandler = new JwtSecurityTokenHandler();

        var principal = tokenHandler.ValidateToken(accessToken, tokenValidParameters, out SecurityToken validatedToken);

        var jwtSecurityToken = validatedToken as JwtSecurityToken;

        if (jwtSecurityToken == null || !jwtSecurityToken.Header.Alg.Equals
        (SecurityAlgorithms.HmacSha512, StringComparison.InvariantCultureIgnoreCase))
        {
            throw new SecurityTokenException("Invalid Token");
        }
        return principal;
    }
}