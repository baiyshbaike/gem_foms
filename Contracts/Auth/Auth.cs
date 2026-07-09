namespace Contracts.Auth;
public sealed record LoginRequest(string Username, string Password);
public sealed record LoginResponse(string AccessToken, string RefreshToken, DateTimeOffset ExpiresAt, AuthUserDto User);
public sealed record RefreshTokenRequest(string RefreshToken);
public sealed record AuthUserDto(long Id, string Username, string FirstName, string LastName, IReadOnlyList<string> Permissions);