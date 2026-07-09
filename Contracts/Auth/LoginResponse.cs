namespace Contracts.Auth;

public sealed record LoginResponse(string AccessToken, string RefreshToken, DateTimeOffset ExpiresAt, AuthUserDto User);