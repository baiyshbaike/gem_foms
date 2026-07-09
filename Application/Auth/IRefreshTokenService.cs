namespace Application.Auth;

public interface IRefreshTokenService
{
    string CreatePlainToken();
    string HashToken(string plainToken);
}