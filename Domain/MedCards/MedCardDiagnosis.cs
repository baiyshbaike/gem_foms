using Domain.Common;

namespace Domain.MedCards;

public sealed class MedCardDiagnosis : TenantAuditableEntityBase
{
    public long MedCardId { get; set; }
    public MedCardDiagnosisType Type { get; set; }
    public int SortOrder { get; set; }
    public string? Code { get; set; }
    public string Text { get; set; } = string.Empty;
    public DateTimeOffset? DiagnosedAt { get; set; }
    public string? Note { get; set; }

    public MedCard MedCard { get; set; } = default!;
}