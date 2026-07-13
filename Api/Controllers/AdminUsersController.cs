using Application.Admin;
using Application.Authorization;
using Contracts.Admin;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace Api.Controllers;

[ApiController]
[Route("api/v1/admin/users")]
[Authorize]
public sealed class AdminUsersController : ControllerBase
{
    private readonly IAdminUserService _adminUserService;

    public AdminUsersController(IAdminUserService adminUserService)
    {
        _adminUserService = adminUserService;
    }

    [HttpGet]
    [Authorize(Policy = "Permission:" + Permissions.AdminUsers)]
    public async Task<ActionResult<IReadOnlyList<AdminUserDto>>> GetUsers(CancellationToken cancellationToken)
    {
        return Ok(await _adminUserService.GetUsersAsync(cancellationToken));
    }

    [HttpGet("roles")]
    [Authorize(Policy = "Permission:" + Permissions.AdminRoles)]
    public async Task<ActionResult<IReadOnlyList<RoleDto>>> GetRoles(CancellationToken cancellationToken)
    {
        return Ok(await _adminUserService.GetRolesAsync(cancellationToken));
    }

    [HttpGet("permissions")]
    [Authorize(Policy = "Permission:" + Permissions.AdminPermissions)]
    public async Task<ActionResult<IReadOnlyList<PermissionDto>>> GetPermissions(CancellationToken cancellationToken)
    {
        return Ok(await _adminUserService.GetPermissionsAsync(cancellationToken));
    }

    [HttpPost]
    [Authorize(Policy = "Permission:" + Permissions.AdminUsers)]
    public async Task<ActionResult<AdminUserDto>> Create(CreateAdminUserRequest request, CancellationToken cancellationToken)
    {
        var userId = CurrentUserId();
        if (userId is null)
        {
            return Unauthorized();
        }

        var result = await _adminUserService.CreateAsync(userId.Value, request, cancellationToken);
        return ToActionResult(result, created: true);
    }

    [HttpPut("{id:long}")]
    [Authorize(Policy = "Permission:" + Permissions.AdminUsers)]
    public async Task<ActionResult<AdminUserDto>> Update(long id, UpdateAdminUserRequest request, CancellationToken cancellationToken)
    {
        var userId = CurrentUserId();
        if (userId is null)
        {
            return Unauthorized();
        }

        var result = await _adminUserService.UpdateAsync(userId.Value, id, request, cancellationToken);
        return ToActionResult(result);
    }

    [HttpDelete("{id:long}")]
    [Authorize(Policy = "Permission:" + Permissions.AdminUsers)]
    public async Task<IActionResult> Delete(long id, CancellationToken cancellationToken)
    {
        var userId = CurrentUserId();
        if (userId is null)
        {
            return Unauthorized();
        }

        var result = await _adminUserService.DeactivateAsync(userId.Value, id, cancellationToken);
        return result.Status switch
        {
            AdminUserCommandStatus.Succeeded => NoContent(),
            AdminUserCommandStatus.NotFound => NotFound(),
            AdminUserCommandStatus.Conflict => Conflict(),
            AdminUserCommandStatus.ValidationFailed => BadRequest(),
            _ => Conflict()
        };
    }

    private ActionResult<T> ToActionResult<T>(AdminUserCommandResult<T> result, bool created = false)
    {
        return result.Status switch
        {
            AdminUserCommandStatus.Succeeded when created => Created(string.Empty, result.Value),
            AdminUserCommandStatus.Succeeded => Ok(result.Value),
            AdminUserCommandStatus.NotFound => NotFound(),
            AdminUserCommandStatus.Conflict => Conflict(),
            AdminUserCommandStatus.ValidationFailed => BadRequest(),
            _ => Conflict()
        };
    }

    private long? CurrentUserId()
    {
        var sub = User.FindFirst("sub")?.Value;
        return long.TryParse(sub, out var userId) ? userId : null;
    }
}
