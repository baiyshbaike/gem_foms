using Contracts.Admin;

namespace Application.Admin;

public interface IAdminUserService
{
    Task<IReadOnlyList<AdminUserDto>> GetUsersAsync(CancellationToken cancellationToken);
    Task<IReadOnlyList<RoleDto>> GetRolesAsync(CancellationToken cancellationToken);
    Task<IReadOnlyList<PermissionDto>> GetPermissionsAsync(CancellationToken cancellationToken);

    Task<AdminUserCommandResult<AdminUserDto>> CreateAsync(
        long actorUserId,
        CreateAdminUserRequest request,
        CancellationToken cancellationToken);

    Task<AdminUserCommandResult<AdminUserDto>> UpdateAsync(
        long actorUserId,
        long userId,
        UpdateAdminUserRequest request,
        CancellationToken cancellationToken);

    Task<AdminUserCommandResult<bool>> DeactivateAsync(
        long actorUserId,
        long userId,
        CancellationToken cancellationToken);
}
