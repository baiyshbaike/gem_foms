namespace Contracts.Admin;

public sealed record AdminUserDto(
    long Id,
    string Username,
    string FirstName,
    string LastName,
    bool IsActive,
    int FailedLoginCount,
    DateTimeOffset? LockoutEndAt,
    DateTimeOffset? LastLoginAt,
    DateTimeOffset CreatedAt,
    DateTimeOffset? UpdatedAt,
    IReadOnlyList<RoleDto> Roles,
    ManagerRegionDto? ManagerRegion);
