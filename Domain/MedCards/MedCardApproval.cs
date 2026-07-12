using Domain.Common;

namespace Domain.MedCards;

public sealed class MedCardApproval : TenantAuditableEntityBase
{
    public long MedCardId { get; set; }

    public long? DoctorId { get; set; }
    public long? DirectorId { get; set; }

    public string? DoctorNameSnapshot { get; set; }
    public string? DirectorNameSnapshot { get; set; }

    public DateTimeOffset? DoctorSignedAt { get; set; }
    public DateTimeOffset? DirectorSignedAt { get; set; }
    public DateTimeOffset? PatientFamiliarizedAt { get; set; }

    public MedCard MedCard { get; set; } = default!;
}