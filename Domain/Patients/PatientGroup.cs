using Domain.Common;

namespace Domain.Patients;

public sealed class PatientGroup : ActiveAuditableEntityBase
{
    public string Code { get; set; } = string.Empty;
    public string Name { get; set; } = string.Empty;
    public bool IsSystem { get; set; } = true;

    public ICollection<Patient> Patients { get; set; } = new List<Patient>();
}