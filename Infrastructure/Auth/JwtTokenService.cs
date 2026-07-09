using System.IdentityModel.Tokens.Jwt;
using System.Security.Claims;
using System.Text;
using Application.Auth;
using Domain.Users;
using Microsoft.Extensions.Configuration;
using Microsoft.IdentityModel.Tokens;

namespace Infrastructure.Auth;

public sealed class JwtTokenService : IJwtTokenService
{
    private readonly JwtOptions _options;

    public JwtTokenService(IConfiguration configuration)
    {
        _options = new JwtOptions
        {
            Issuer = configuration["JWT_ISSUER"] ?? "Dialysis",
            Audience = configuration["JWT_AUDIENCE"] ?? "Api",
            Secret = configuration["JWT_SECRET"] ?? throw new InvalidOperationException("JWT_SECRET is required."),
            AccessTokenMinutes = int.TryParse(configuration["JWT_ACCESS_TOKEN_MINUTES"], out var minutes) ? minutes : 30
        };
    }
    public (string Token, DateTimeOffset ExpiresAt) CreateAccessToken(User user, IReadOnlyCollection<string> permissions)
    {
        var expiresAt = DateTimeOffset.UtcNow.AddMinutes(_options.AccessTokenMinutes);
        var key = new SymmetricSecurityKey(Encoding.UTF8.GetBytes(_options.Secret));
        var credentials = new SigningCredentials(key, SecurityAlgorithms.HmacSha256);
        var claims = new List<Claim>
        {
            new(JwtRegisteredClaimNames.Sub, user.Id.ToString()),
            new(ClaimTypes.Name, user.Username)
        };

        claims.AddRange(permissions.Select(x => new Claim("permission", x)));

        var token = new JwtSecurityToken(
            issuer: _options.Issuer,
            audience: _options.Audience,
            claims: claims,
            expires: expiresAt.UtcDateTime,
            signingCredentials: credentials);

        return (new JwtSecurityTokenHandler().WriteToken(token), expiresAt);
    }
}