using Domain.Common;

namespace Domain.Sessions;

public sealed class HdSessionMeasurement : TenantAuditableEntityBase
{
    public long HdSessionId { get; set; }
    public SessionMeasurementPoint Point { get; set; }

    public string? Sys { get; set; }
    public string? Dia { get; set; }
    public string? Temp { get; set; }
    public string? Ritm { get; set; }

    public DateTimeOffset? MeasuredAt { get; set; }
    public string? Note { get; set; }

    public HdSession HdSession { get; set; } = default!;
}