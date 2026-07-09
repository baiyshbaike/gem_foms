using Domain.Users;

namespace Application.Auth;

public interface IJwtTokenService
{
    (string Token, DateTimeOffset ExpiresAt) CreateAccessToken(User user, IReadOnlyCollection<string> permissions);
}