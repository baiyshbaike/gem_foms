namespace Domain.Tenants;

public sealed class Tenant
{
    public string Id { get; set; } = string.Empty;
    public string Code { get; set; } = string.Empty;
    public string Name { get; set; } = string.Empty;
    public string Locale { get; set; } = "ru-RU";
    public bool IsActive { get; set; } = true;
    public DateTimeOffset CreatedAt { get; set; }
    public DateTimeOffset? DisabledAt { get; set; }

    public ICollection<TenantUser> TenantUsers { get; set; } = new List<TenantUser>();
}