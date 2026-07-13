using Application.Admin;
using Application.Audit;
using Contracts.Admin;
using Domain.Users;
using Infrastructure.Data;
using Microsoft.AspNetCore.Identity;
using Microsoft.EntityFrameworkCore;

namespace Infrastructure.Admin;

public sealed class AdminUserService : IAdminUserService
{
    private readonly AppDbContext _db;
    private readonly IActionLogService _actionLogService;

    public AdminUserService(AppDbContext db, IActionLogService actionLogService)
    {
        _db = db;
        _actionLogService = actionLogService;
    }

    public async Task<IReadOnlyList<AdminUserDto>> GetUsersAsync(CancellationToken cancellationToken)
    {
        var users = await _db.Users
            .AsNoTracking()
            .OrderBy(x => x.Username)
            .Select(x => new AdminUserDto(
                x.Id,
                x.Username,
                x.FirstName,
                x.LastName,
                x.IsActive,
                x.FailedLoginCount,
                x.LockoutEndAt,
                x.LastLoginAt,
                x.CreatedAt,
                x.UpdatedAt,
                x.UserRoles
                    .OrderBy(ur => ur.Role.Name)
                    .Select(ur => new RoleDto(ur.Role.Id, ur.Role.Code, ur.Role.Name, ur.Role.IsSystem))
                    .ToList()))
            .ToListAsync(cancellationToken);

        await _actionLogService.AddAsync(new ActionLogRequest
        {
            Action = "AdminUsersViewed",
            Module = "admin",
            EntityName = "User",
            StatusCode = 200,
            Succeeded = true,
            MetadataJson = $$"""{"resultCount":{{users.Count}}}"""
        }, cancellationToken);

        await _db.SaveChangesAsync(cancellationToken);
        return users;
    }

    public async Task<IReadOnlyList<RoleDto>> GetRolesAsync(CancellationToken cancellationToken)
    {
        return await _db.Roles
            .AsNoTracking()
            .OrderBy(x => x.Name)
            .Select(x => new RoleDto(x.Id, x.Code, x.Name, x.IsSystem))
            .ToListAsync(cancellationToken);
    }

    public async Task<IReadOnlyList<PermissionDto>> GetPermissionsAsync(CancellationToken cancellationToken)
    {
        return await _db.Permissions
            .AsNoTracking()
            .OrderBy(x => x.Module)
            .ThenBy(x => x.Code)
            .Select(x => new PermissionDto(x.Id, x.Code, x.Module, x.Name, x.Description))
            .ToListAsync(cancellationToken);
    }

    public async Task<AdminUserCommandResult<AdminUserDto>> CreateAsync(
        long actorUserId,
        CreateAdminUserRequest request,
        CancellationToken cancellationToken)
    {
        var username = request.Username.Trim();
        if (await _db.Users.AnyAsync(x => x.Username == username, cancellationToken))
        {
            await AddUserLogAsync(actorUserId, "AdminUserCreateFailed", null, 409, false, "Username already exists", cancellationToken);
            await _db.SaveChangesAsync(cancellationToken);
            return new AdminUserCommandResult<AdminUserDto>(AdminUserCommandStatus.Conflict);
        }

        var roleIds = request.RoleIds.Distinct().ToList();
        if (roleIds.Count == 0 || await _db.Roles.CountAsync(x => roleIds.Contains(x.Id), cancellationToken) != roleIds.Count)
        {
            await AddUserLogAsync(actorUserId, "AdminUserCreateFailed", null, 400, false, "Invalid role selection", cancellationToken);
            await _db.SaveChangesAsync(cancellationToken);
            return new AdminUserCommandResult<AdminUserDto>(AdminUserCommandStatus.ValidationFailed);
        }

        var user = new User
        {
            Username = username,
            FirstName = request.FirstName.Trim(),
            LastName = request.LastName.Trim(),
            IsActive = request.IsActive,
            CreatedAt = DateTimeOffset.UtcNow
        };

        user.PasswordHash = new PasswordHasher<User>().HashPassword(user, request.Password);
        _db.Users.Add(user);
        await _db.SaveChangesAsync(cancellationToken);

        foreach (var roleId in roleIds)
        {
            _db.UserRoles.Add(new UserRole
            {
                UserId = user.Id,
                RoleId = roleId
            });
        }

        await AddUserLogAsync(actorUserId, "AdminUserCreated", user.Id, 201, true, null, cancellationToken);
        await _db.SaveChangesAsync(cancellationToken);

        return new AdminUserCommandResult<AdminUserDto>(
            AdminUserCommandStatus.Succeeded,
            (await GetUserByIdAsync(user.Id, cancellationToken))!);
    }

    public async Task<AdminUserCommandResult<AdminUserDto>> UpdateAsync(
        long actorUserId,
        long userId,
        UpdateAdminUserRequest request,
        CancellationToken cancellationToken)
    {
        var user = await _db.Users
            .Include(x => x.UserRoles)
            .FirstOrDefaultAsync(x => x.Id == userId, cancellationToken);

        if (user is null)
        {
            await AddUserLogAsync(actorUserId, "AdminUserUpdateFailed", userId, 404, false, "User not found", cancellationToken);
            await _db.SaveChangesAsync(cancellationToken);
            return new AdminUserCommandResult<AdminUserDto>(AdminUserCommandStatus.NotFound);
        }

        var username = request.Username.Trim();
        if (await _db.Users.AnyAsync(x => x.Id != userId && x.Username == username, cancellationToken))
        {
            await AddUserLogAsync(actorUserId, "AdminUserUpdateFailed", userId, 409, false, "Username already exists", cancellationToken);
            await _db.SaveChangesAsync(cancellationToken);
            return new AdminUserCommandResult<AdminUserDto>(AdminUserCommandStatus.Conflict);
        }

        if (actorUserId == userId && !request.IsActive)
        {
            await AddUserLogAsync(actorUserId, "AdminUserUpdateFailed", userId, 400, false, "Current user cannot deactivate self", cancellationToken);
            await _db.SaveChangesAsync(cancellationToken);
            return new AdminUserCommandResult<AdminUserDto>(AdminUserCommandStatus.ValidationFailed);
        }

        var roleIds = request.RoleIds.Distinct().ToList();
        if (roleIds.Count == 0 || await _db.Roles.CountAsync(x => roleIds.Contains(x.Id), cancellationToken) != roleIds.Count)
        {
            await AddUserLogAsync(actorUserId, "AdminUserUpdateFailed", userId, 400, false, "Invalid role selection", cancellationToken);
            await _db.SaveChangesAsync(cancellationToken);
            return new AdminUserCommandResult<AdminUserDto>(AdminUserCommandStatus.ValidationFailed);
        }

        user.Username = username;
        user.FirstName = request.FirstName.Trim();
        user.LastName = request.LastName.Trim();
        user.IsActive = request.IsActive;
        user.UpdatedAt = DateTimeOffset.UtcNow;

        if (!string.IsNullOrWhiteSpace(request.Password))
        {
            user.PasswordHash = new PasswordHasher<User>().HashPassword(user, request.Password);
        }

        _db.UserRoles.RemoveRange(user.UserRoles);
        foreach (var roleId in roleIds)
        {
            _db.UserRoles.Add(new UserRole
            {
                UserId = user.Id,
                RoleId = roleId
            });
        }

        await AddUserLogAsync(actorUserId, "AdminUserUpdated", user.Id, 200, true, null, cancellationToken);
        await _db.SaveChangesAsync(cancellationToken);

        return new AdminUserCommandResult<AdminUserDto>(
            AdminUserCommandStatus.Succeeded,
            (await GetUserByIdAsync(user.Id, cancellationToken))!);
    }

    public async Task<AdminUserCommandResult<bool>> DeactivateAsync(
        long actorUserId,
        long userId,
        CancellationToken cancellationToken)
    {
        if (actorUserId == userId)
        {
            await AddUserLogAsync(actorUserId, "AdminUserDeactivateFailed", userId, 400, false, "Current user cannot deactivate self", cancellationToken);
            await _db.SaveChangesAsync(cancellationToken);
            return new AdminUserCommandResult<bool>(AdminUserCommandStatus.ValidationFailed, false);
        }

        var user = await _db.Users.FirstOrDefaultAsync(x => x.Id == userId, cancellationToken);
        if (user is null)
        {
            await AddUserLogAsync(actorUserId, "AdminUserDeactivateFailed", userId, 404, false, "User not found", cancellationToken);
            await _db.SaveChangesAsync(cancellationToken);
            return new AdminUserCommandResult<bool>(AdminUserCommandStatus.NotFound, false);
        }

        user.IsActive = false;
        user.UpdatedAt = DateTimeOffset.UtcNow;

        var activeRefreshTokens = await _db.RefreshTokens
            .Where(x => x.UserId == userId && x.RevokedAt == null)
            .ToListAsync(cancellationToken);

        foreach (var refreshToken in activeRefreshTokens)
        {
            refreshToken.RevokedAt = DateTimeOffset.UtcNow;
        }

        await AddUserLogAsync(actorUserId, "AdminUserDeactivated", user.Id, 200, true, null, cancellationToken);
        await _db.SaveChangesAsync(cancellationToken);

        return new AdminUserCommandResult<bool>(AdminUserCommandStatus.Succeeded, true);
    }

    private async Task<AdminUserDto?> GetUserByIdAsync(long id, CancellationToken cancellationToken)
    {
        return await _db.Users
            .AsNoTracking()
            .Where(x => x.Id == id)
            .Select(x => new AdminUserDto(
                x.Id,
                x.Username,
                x.FirstName,
                x.LastName,
                x.IsActive,
                x.FailedLoginCount,
                x.LockoutEndAt,
                x.LastLoginAt,
                x.CreatedAt,
                x.UpdatedAt,
                x.UserRoles
                    .OrderBy(ur => ur.Role.Name)
                    .Select(ur => new RoleDto(ur.Role.Id, ur.Role.Code, ur.Role.Name, ur.Role.IsSystem))
                    .ToList()))
            .FirstOrDefaultAsync(cancellationToken);
    }

    private Task AddUserLogAsync(
        long actorUserId,
        string action,
        long? userId,
        int statusCode,
        bool succeeded,
        string? failureReason,
        CancellationToken cancellationToken)
    {
        return _actionLogService.AddAsync(new ActionLogRequest
        {
            UserId = actorUserId,
            Action = action,
            Module = "admin",
            EntityName = "User",
            EntityId = userId?.ToString(),
            StatusCode = statusCode,
            Succeeded = succeeded,
            FailureReason = failureReason
        }, cancellationToken);
    }
}
