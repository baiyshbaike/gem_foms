using Domain.Common;
using Domain.Patients;
using Domain.Tenants;

namespace Domain.Regions;

public sealed class District : ActiveSoftDeletableAuditableEntityBase
{
    public string Name { get; set; } = string.Empty;
    public long RegionId { get; set; }

    public Region Region { get; set; } = default!;
    public ICollection<Patient> Patients { get; set; } = new List<Patient>();
    public ICollection<Tenant> Tenants { get; set; } = new List<Tenant>();
}
