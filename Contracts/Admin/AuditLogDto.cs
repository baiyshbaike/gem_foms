namespace Contracts.Admin;

public sealed record AuditLogDto(
    long Id,
    long? UserId,
    string? UsernameSnapshot,
    string Action,
    string Module,
    bool Succeeded,
    string? FailureReason,
    DateTimeOffset CreatedAt);