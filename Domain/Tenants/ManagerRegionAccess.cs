using Domain.Users;

namespace Domain.Tenants;

public sealed class ManagerRegionAccess
{
    public long Id { get; set; }
    public long UserId { get; set; }
    public string RegionId { get; set; } = string.Empty;
    public long? GeoRegionId { get; set; }
    public long? DistrictId { get; set; }
    public DateTimeOffset GrantedAt { get; set; }
    public long? GrantedBy { get; set; }
    public DateTimeOffset? RevokedAt { get; set; }

    public User User { get; set; } = default!;
    public Region Region { get; set; } = default!;
    public Domain.Regions.Region? GeoRegion { get; set; }
    public Domain.Regions.District? District { get; set; }
}
