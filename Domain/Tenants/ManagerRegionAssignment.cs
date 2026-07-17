using Domain.Regions;
using Domain.Users;

namespace Domain.Tenants;

public sealed class ManagerRegionAssignment
{
    public long Id { get; set; }
    public long UserId { get; set; }
    public long RegionId { get; set; }
    public DateTimeOffset AssignedAt { get; set; }
    public long? AssignedBy { get; set; }
    public DateTimeOffset? RevokedAt { get; set; }
    public long? RevokedBy { get; set; }

    public User User { get; set; } = default!;
    public Region Region { get; set; } = default!;
}
