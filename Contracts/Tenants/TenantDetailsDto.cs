namespace Contracts.Tenants;

public sealed record TenantDetailsDto(
    string Id,
    string Code,
    string Name,
    string? Address,
    string Phone,
    long RegionId,
    string RegionName,
    long DistrictId,
    string DistrictName,
    bool IsActive,
    DateTimeOffset CreatedAt,
    DateTimeOffset? DisabledAt);
