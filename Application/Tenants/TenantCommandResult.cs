namespace Application.Tenants;

public enum TenantCommandStatus
{
    Succeeded,
    NotFound,
    Conflict,
    ValidationFailed
}

public sealed record TenantCommandResult<T>(
    TenantCommandStatus Status,
    T? Value = default,
    string? Error = null);
