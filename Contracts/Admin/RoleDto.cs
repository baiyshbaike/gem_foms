namespace Contracts.Admin;

public sealed record RoleDto(
    long Id,
    string Code,
    string Name,
    bool IsSystem);
