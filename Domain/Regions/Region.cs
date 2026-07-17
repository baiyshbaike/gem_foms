using Domain.Common;
using Domain.Patients;
using Domain.Tenants;

namespace Domain.Regions;

public sealed class Region : ActiveSoftDeletableAuditableEntityBase
{
    public string Name { get; set; } = string.Empty;

    public ICollection<District> Districts { get; set; } = new List<District>();
    public ICollection<Patient> Patients { get; set; } = new List<Patient>();
    public ICollection<Tenant> Tenants { get; set; } = new List<Tenant>();
    public ICollection<ManagerRegionAssignment> ManagerRegionAssignments { get; set; } = new List<ManagerRegionAssignment>();
}
