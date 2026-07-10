namespace Application.Audit;

public sealed record ActionLogRequest
{
    public long? UserId { get; init; }
    public string? UsernameSnapshot { get; init; }
    public string Action { get; init; } = string.Empty;
    public string Module { get; init; } = string.Empty;
    public string? EntityName { get; init; }
    public string? EntityId { get; init; }
    public int? StatusCode { get; init; }
    public bool Succeeded { get; init; }
    public string? FailureReason { get; init; }
    public string? MetadataJson { get; init; }
}