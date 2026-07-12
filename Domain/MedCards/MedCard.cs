using Domain.Common;
using Domain.Patients;
using Domain.Sessions;

namespace Domain.MedCards;

public sealed class MedCard : TenantSoftDeletableAuditableEntityBase
{
    public long PatientId { get; set; }
    public string CardNumber { get; set; } = string.Empty;
    public DateTimeOffset OpenedAt { get; set; }
    public DateTimeOffset? ClosedAt { get; set; }
    public MedCardStatus Status { get; set; } = MedCardStatus.Open;
    public string? Notes { get; set; }

    public Patient Patient { get; set; } = default!;
    public MedCardClinicalInfo? ClinicalInfo { get; set; }
    public MedCardInfectionScreening? InfectionScreening { get; set; }
    public MedCardApproval? Approval { get; set; }
    public ICollection<MedCardDiagnosis> Diagnoses { get; set; } = new List<MedCardDiagnosis>();
    public ICollection<HdSession> Sessions { get; set; } = new List<HdSession>();
}