using Domain.Users;

namespace Application.Auth;

public interface IJwtTokenService
{
    public (string Token, DateTimeOffset ExpiresAt) CreateAccessToken(
        User user,
        IReadOnlyCollection<string> permissions,
        string? activeTenantId = null);
}