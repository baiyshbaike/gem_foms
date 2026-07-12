using Domain.Common;

namespace Domain.MedCards;

public sealed class MedCardClinicalInfo : TenantAuditableEntityBase
{
    public long MedCardId { get; set; }

    public string? BloodGroup { get; set; }
    public string? RhFactor { get; set; }
    public string? AllergyHistory { get; set; }
    public string? OutTreatment { get; set; }
    public string? Justification { get; set; }
    public string? Plan { get; set; }
    public string? IndividualPlan { get; set; }
    public string? Recommendation { get; set; }
    public string? Note { get; set; }

    public MedCard MedCard { get; set; } = default!;
}