using Domain.Common;

namespace Domain.Sessions;

public sealed class SessionWorkflowSettings : TenantAuditableEntityBase
{
    public int IdentificationStartLimitMinutes { get; set; } = 240;
    public int AutoFinishActiveMinutes { get; set; } = 270;
    public int EndIdentificationLimitMinutes { get; set; } = 120;
    public int SendToPayLimitMinutes { get; set; } = 360;
}