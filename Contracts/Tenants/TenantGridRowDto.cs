namespace Contracts.Tenants;

public sealed class TenantGridRowDto
{
    public string Id { get; init; } = string.Empty;
    public string Code { get; init; } = string.Empty;
    public string Name { get; init; } = string.Empty;
    public string? Address { get; init; }
    public string Phone { get; init; } = string.Empty;
    public long RegionId { get; init; }
    public string RegionName { get; init; } = string.Empty;
    public long DistrictId { get; init; }
    public string DistrictName { get; init; } = string.Empty;
    public bool IsActive { get; init; }
    public DateTimeOffset CreatedAt { get; init; }
    public DateTimeOffset? DisabledAt { get; init; }
}
