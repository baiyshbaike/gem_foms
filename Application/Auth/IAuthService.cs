using Contracts.Auth;

namespace Application.Auth;

public interface IAuthService
{
    Task<LoginResponse?> LoginAsync(LoginRequest request, CancellationToken cancellationToken);
    Task<LoginResponse?> RefreshAsync(RefreshTokenRequest request, CancellationToken cancellationToken);
    Task<AuthUserDto?> GetMeAsync(long userId, CancellationToken cancellationToken);
    Task LogoutAsync(LogoutRequest request, CancellationToken cancellationToken);
}