using Domain.Common;
using Domain.MedCards;
using Domain.Sessions;

namespace Domain.Patients;

public sealed class Patient : ActiveSoftDeletableAuditableEntityBase
{
    public string Inn { get; set; } = string.Empty;
    public string FirstName { get; set; } = string.Empty;
    public string LastName { get; set; } = string.Empty;
    public string MiddleName { get; set; } = string.Empty;
    public DateOnly BirthDate { get; set; }
    public PatientGender Gender { get; set; }

    public string Address { get; set; } = string.Empty;
    public string Address2 { get; set; } = string.Empty;
    public string Phone { get; set; } = string.Empty;
    public long DistrictId { get; set; }
    public long RegionId { get; set; }

    public long GroupId { get; set; } = PatientGroupIds.New;
    public bool SpecialStatus { get; set; }

    public PatientGroup Group { get; set; } = default!;
    public ICollection<MedCard> MedCards { get; set; } = new List<MedCard>();
    public ICollection<HdSession> Sessions { get; set; } = new List<HdSession>();
}