using System.Security.Cryptography;
using Microsoft.AspNetCore.Identity;
using Shipment.Entities;

namespace Shipment.Auth;

public sealed class PasswordHasher : PasswordHasher<Users>
{
    private const int SaltSize = 16;
    private const int HashSize = 32;
    private const int Iterations = 100000;
    private static readonly HashAlgorithmName Algorithm = HashAlgorithmName.SHA512;

    public override string HashPassword(Users user, string password)
    {
        byte[] salt = RandomNumberGenerator.GetBytes(SaltSize);
        byte[] hash = Rfc2898DeriveBytes.Pbkdf2(password, salt, Iterations, Algorithm, HashSize);

        return $"{Convert.ToHexString(hash)}-{Convert.ToHexString(salt)}";
    }

    public override PasswordVerificationResult VerifyHashedPassword(Users user, string password, string hashedPassword)
    {
        string[] parts = hashedPassword.Split('-');
        byte[] hash = Convert.FromHexString(parts[0]);
        byte[] salt = Convert.FromHexString(parts[1]);

        byte[] inputHash = Rfc2898DeriveBytes.Pbkdf2(password, salt, Iterations, Algorithm, HashSize);

        bool verifiedPassword = CryptographicOperations.FixedTimeEquals(hash, inputHash);

        return verifiedPassword ? PasswordVerificationResult.Success : PasswordVerificationResult.Failed;
    }
}