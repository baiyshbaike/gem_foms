namespace Contracts.Admin;

public sealed record PermissionDto(
    long Id,
    string Code,
    string Module,
    string Name,
    string? Description);
