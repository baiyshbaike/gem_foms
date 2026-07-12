using Domain.Common;
using Domain.MedCards;
using Domain.MedCenters;
using Domain.Patients;

namespace Domain.Sessions;

public sealed class HdSession : TenantAuditableEntityBase
{
    public long PatientId { get; set; }
    public long MedCardId { get; set; }
    public long? MachineId { get; set; }

    public SessionStatus Status { get; set; } = SessionStatus.Identified;

    public DateTimeOffset IdentifiedAt { get; set; }
    public DateTimeOffset? StartedAt { get; set; }
    public DateTimeOffset? FinishedAt { get; set; }
    public DateTimeOffset? EndIdentifiedAt { get; set; }
    public DateTimeOffset? SentToPayAt { get; set; }
    public DateTimeOffset? PaidAt { get; set; }
    public DateTimeOffset? ArchivedAt { get; set; }
    public DateTimeOffset? CancelledAt { get; set; }

    public DateTimeOffset StatusChangedAt { get; set; }
    public string? StatusReason { get; set; }

    public int? ActiveMinutes { get; set; }
    public int? PauseMinutes { get; set; }

    public Patient Patient { get; set; } = default!;
    public MedCard MedCard { get; set; } = default!;
    public ICollection<HdSessionMeasurement> Measurements { get; set; } = new List<HdSessionMeasurement>();
    public ICollection<HdSessionPause> Pauses { get; set; } = new List<HdSessionPause>();
    public MedCenterMachine? Machine { get; set; }
}