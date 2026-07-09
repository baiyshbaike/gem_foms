using System.Security.Cryptography;
using System.Text;
using Application.Auth;

namespace Infrastructure.Auth;

public sealed class RefreshTokenService : IRefreshTokenService
{
    public string CreatePlainToken()
    {
        return Convert.ToBase64String(RandomNumberGenerator.GetBytes(64));
    }

    public string HashToken(string plainToken)
    {
        var bytes = SHA256.HashData(Encoding.UTF8.GetBytes(plainToken));
        return Convert.ToHexString(bytes);
    }
}