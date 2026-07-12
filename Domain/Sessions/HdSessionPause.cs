using Domain.Common;

namespace Domain.Sessions;

public sealed class HdSessionPause : TenantAuditableEntityBase
{
    public long HdSessionId { get; set; }

    public DateTimeOffset StartedAt { get; set; }
    public DateTimeOffset? EndedAt { get; set; }

    public string? Reason { get; set; }
    public long? PausedBy { get; set; }
    public long? ResumedBy { get; set; }

    public HdSession HdSession { get; set; } = default!;
}