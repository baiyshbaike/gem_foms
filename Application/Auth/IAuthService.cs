using Contracts.Auth;

namespace Application.Auth;

public interface IAuthService
{
    Task<LoginResponse?> LoginAsync(LoginRequest request, string? ipAddress, string? userAgent, string correlationId, CancellationToken cancellationToken);
}