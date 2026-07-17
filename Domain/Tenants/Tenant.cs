namespace Domain.Tenants;

public sealed class Tenant
{
    public string Id { get; set; } = string.Empty;
    public string Code { get; set; } = string.Empty;
    public string Name { get; set; } = string.Empty;
    public string? Address { get; set; }
    public string Phone { get; set; } = string.Empty;
    public bool IsActive { get; set; } = true;
    public DateTimeOffset CreatedAt { get; set; }
    public DateTimeOffset? DisabledAt { get; set; }
    public ICollection<TenantUser> TenantUsers { get; set; } = new List<TenantUser>();
    public long RegionId { get; set; }
    public long DistrictId { get; set; }
    public Domain.Regions.Region Region { get; set; } = default!;
    public Domain.Regions.District District { get; set; } = default!;
}
