using Domain.Users;

namespace Domain.Tenants;

public sealed class TenantUser
{
    public long Id { get; set; }
    public string TenantId { get; set; } = string.Empty;
    public long UserId { get; set; }
    public bool IsTenantAdmin { get; set; }
    public DateTimeOffset JoinedAt { get; set; }
    public DateTimeOffset? LeftAt { get; set; }

    public Tenant Tenant { get; set; } = default!;
    public User User { get; set; } = default!;
}