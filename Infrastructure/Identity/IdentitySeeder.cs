using Application.Authorization;
using Domain.Users;
using Infrastructure.Data;
using Microsoft.AspNetCore.Identity;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;

namespace Infrastructure.Identity;

public static class IdentitySeeder
{
    private const string AdminRoleCode = "Admin";

    public static async Task SeedAsync(IServiceProvider services, IConfiguration configuration)
    {
        using var scope = services.CreateScope();
        var db = scope.ServiceProvider.GetRequiredService<AppDbContext>();

        await SeedPermissionsAsync(db);
        var adminROle = await SeedAdminRoleAsync(db);
        var adminUser = await SeedAdminUserAsync(db, configuration);
        await SeedAdminLinkAsync(db, adminUser, adminROle);
    }

    private static async Task SeedPermissionsAsync(AppDbContext db)
    {
        var existingCodes = await db.Permissions.Select(x => x.Code).ToListAsync();
        foreach (var code in Permissions.All.Except(existingCodes))
        {
            db.Permissions.Add(new Permission
            {
                Code = code,
                Module = code.Split('.')[0],
                Name = code
            }); 
        }
        await db.SaveChangesAsync();
    }

    private static async Task<Role> SeedAdminRoleAsync(AppDbContext db)
    {
        var role = await db.Roles.FirstOrDefaultAsync(x => x.Code == AdminRoleCode);
        if (role is not null)
        {
            return role;
        }

        role = new Role
        {
            Code = AdminRoleCode,
            Name = "Administrator",
            IsSystem = true,
            CreatedAt = DateTimeOffset.UtcNow,
        };
        db.Roles.Add(role);
        await db.SaveChangesAsync();
        return role;
    }

    private static async Task<User> SeedAdminUserAsync(AppDbContext db, IConfiguration configuration)
    {
        var username = configuration["SEED_ADMIN_USERNAME"] ?? "admin";
        var password = configuration["SEED_ADMIN_PASSWORD"];

        if (string.IsNullOrWhiteSpace(password))
        {
            throw new InvalidOperationException("SEED_ADMIN_PASSWORD is required.");
        }

        var user = await db.Users.FirstOrDefaultAsync(x => x.Username == username);
        if (user is not null)
        {
            return user;
        }

        user = new User
        {
            Username = username,
            FirstName = configuration["SEED_ADMIN_FIRST_NAME"] ?? "System",
            LastName = configuration["SEED_ADMIN_LAST_NAME"] ?? "Admin",
            IsActive = true,
            CreatedAt = DateTimeOffset.UtcNow
        };
        var hasher = new PasswordHasher<User>();
        user.PasswordHash = hasher.HashPassword(user, password);
        
        db.Users.Add(user);
        await db.SaveChangesAsync();
        return user;
    }

    private static async Task SeedAdminLinkAsync(AppDbContext db, User user, Role role)
    {
        var permissionIds = await db.Permissions.Select(x => x.Id).ToListAsync();
        var existingRolePermissionIds = await db.RolePermissions.Where(x=>x.RoleId == role.Id).Select(x=>x.PermissionId).ToListAsync();
        foreach (var permissionId in permissionIds.Except(existingRolePermissionIds))
        {
            db.RolePermissions.Add(new RolePermission
            {
                RoleId = role.Id,
                PermissionId = permissionId
            });
        }
        var hasUserRole = await db.UserRoles.AnyAsync(x => x.UserId == user.Id && x.RoleId == role.Id);
        if (!hasUserRole)
        {
            db.UserRoles.Add(new UserRole
            {
                UserId = user.Id,
                RoleId = role.Id
            });
        }
        await db.SaveChangesAsync();
    }
}