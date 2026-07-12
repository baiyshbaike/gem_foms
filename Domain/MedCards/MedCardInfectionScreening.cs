using Domain.Common;

namespace Domain.MedCards;

public sealed class MedCardInfectionScreening : TenantAuditableEntityBase
{
    public long MedCardId { get; set; }

    public ScreeningResult Pediculosis { get; set; } = ScreeningResult.NotChecked;
    public ScreeningResult Scabies { get; set; } = ScreeningResult.NotChecked;
    public ScreeningResult Wasserman { get; set; } = ScreeningResult.NotChecked;
    public ScreeningResult Fluorography { get; set; } = ScreeningResult.NotChecked;
    public ScreeningResult AlcoholUse { get; set; } = ScreeningResult.NotChecked;

    public ScreeningResult HepatitisB { get; set; } = ScreeningResult.NotChecked;
    public DateTimeOffset? HepatitisBCheckedAt { get; set; }

    public ScreeningResult HepatitisC { get; set; } = ScreeningResult.NotChecked;
    public DateTimeOffset? HepatitisCCheckedAt { get; set; }

    public ScreeningResult Hiv { get; set; } = ScreeningResult.NotChecked;
    public DateTimeOffset? HivCheckedAt { get; set; }

    public MedCard MedCard { get; set; } = default!;
}