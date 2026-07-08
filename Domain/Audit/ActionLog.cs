namespace Domain.Audit;

public sealed class ActionLog
{
    public long Id { get; set; }
    public long? UserId { get; set; }
    public string? UsernameSnapshot { get; set; }
    public string Action { get; set; } = string.Empty;
    public string Module { get; set; } = string.Empty;
    public string? EntityName { get; set; }
    public string? EntityId { get; set; }
    public string? HttpMethod { get; set; }
    public string? Path { get; set; }
    public string? IpAddress { get; set; }
    public string? UserAgent { get; set; }
    public int? StatusCode { get; set; }
    public bool Succeeded { get; set; }
    public string? FailureReason { get; set; }
    public string CorrelationId { get; set; } = string.Empty;
    public string? MetadataJson { get; set; }
    public DateTimeOffset CreatedAt { get; set; }
}