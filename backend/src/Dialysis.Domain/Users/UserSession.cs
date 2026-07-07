namespace Dialysis.Domain.Users;

public sealed class UserSession
{
    public long Id { get; set; }
    public long UserId { get; set; }
    public string Jti { get; set; } = string.Empty;
    public string RefreshTokenHash { get; set; } = string.Empty;
    public string? IpAddress { get; set; }
    public string? UserAgent { get; set; }
    public DateTimeOffset ExpiresAt { get; set; }
    public DateTimeOffset? RevokedAt { get; set; }
    public DateTimeOffset CreatedAt { get; set; }
}