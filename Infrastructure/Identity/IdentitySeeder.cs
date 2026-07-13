using Application.Authorization;
using Domain.Tenants;
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
    private const string ManagerRoleCode = "Manager";
    private const string DoctorRoleCode = "Doctor";

    public static async Task SeedAsync(IServiceProvider services, IConfiguration configuration)
    {
        using var scope = services.CreateScope();
        var db = scope.ServiceProvider.GetRequiredService<AppDbContext>();

        await SeedPermissionsAsync(db);

        var adminRole = await SeedRoleAsync(db, AdminRoleCode, "Administrator");
        var managerRole = await SeedRoleAsync(db, ManagerRoleCode, "Manager");
        var doctorRole = await SeedRoleAsync(db, DoctorRoleCode, "Doctor");

        await SeedRolePermissionsAsync(db, adminRole, Permissions.All);

        await SeedRolePermissionsAsync(db, managerRole, new[]
        {
            Permissions.TenantRead,
            Permissions.TenantSwitch,
            Permissions.TenantAccessAssigned,
            Permissions.RegionRead,
            Permissions.DistrictRead,
            Permissions.MedCenterMachineRead,
            Permissions.MedCenterMachineCreate,
            Permissions.MedCenterMachineUpdate,
            Permissions.MedCenterMachineDelete,
        });

        await SeedRolePermissionsAsync(db, doctorRole, new[]
        {
            Permissions.TenantRead,
            Permissions.TenantSwitch,
            Permissions.TenantAccessOwn,
            Permissions.RegionRead,
            Permissions.DistrictRead,
            Permissions.MedCenterMachineRead,
        });

        var adminUser = await SeedAdminUserAsync(db, configuration);
        await SeedUserRoleAsync(db, adminUser, adminRole);

        var region = await SeedDefaultRegionAsync(db, configuration);
        await SeedDefaultTenantAsync(db, configuration, region);
    }

    private static async Task SeedPermissionsAsync(AppDbContext db)
    {
        var existingCodes = await db.Permissions
            .Select(x => x.Code)
            .ToListAsync();

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

    private static async Task<Role> SeedRoleAsync(AppDbContext db, string code, string name)
    {
        var role = await db.Roles.FirstOrDefaultAsync(x => x.Code == code);
        if (role is not null)
        {
            return role;
        }

        role = new Role
        {
            Code = code,
            Name = name,
            IsSystem = true,
            CreatedAt = DateTimeOffset.UtcNow
        };

        db.Roles.Add(role);
        await db.SaveChangesAsync();

        return role;
    }

    private static async Task<User> SeedAdminUserAsync(AppDbContext db, IConfiguration configuration)
    {
        var username = configuration["SEED_ADMIN_USERNAME"] ?? "admin";

        var user = await db.Users.FirstOrDefaultAsync(x => x.Username == username);
        if (user is not null)
        {
            return user;
        }

        var password = configuration["SEED_ADMIN_PASSWORD"];
        if (string.IsNullOrWhiteSpace(password))
        {
            throw new InvalidOperationException("SEED_ADMIN_PASSWORD is required.");
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

    private static async Task SeedRolePermissionsAsync(
        AppDbContext db,
        Role role,
        IReadOnlyCollection<string> permissionCodes)
    {
        var codes = permissionCodes.ToArray();

        var permissionIds = await db.Permissions
            .Where(x => codes.Contains(x.Code))
            .Select(x => x.Id)
            .ToListAsync();

        var existingPermissionIds = await db.RolePermissions
            .Where(x => x.RoleId == role.Id)
            .Select(x => x.PermissionId)
            .ToListAsync();

        foreach (var permissionId in permissionIds.Except(existingPermissionIds))
        {
            db.RolePermissions.Add(new RolePermission
            {
                RoleId = role.Id,
                PermissionId = permissionId
            });
        }

        await db.SaveChangesAsync();
    }

    private static async Task SeedUserRoleAsync(AppDbContext db, User user, Role role)
    {
        var exists = await db.UserRoles.AnyAsync(x => x.UserId == user.Id && x.RoleId == role.Id);
        if (exists)
        {
            return;
        }

        db.UserRoles.Add(new UserRole
        {
            UserId = user.Id,
            RoleId = role.Id
        });

        await db.SaveChangesAsync();
    }

    private static async Task<Region> SeedDefaultRegionAsync(
        AppDbContext db,
        IConfiguration configuration)
    {
        var regionId = configuration["SEED_REGION_ID"] ?? "dev-region";

        var region = await db.Regions.FirstOrDefaultAsync(x => x.Id == regionId);
        if (region is not null)
        {
            return region;
        }

        region = new Region
        {
            Id = regionId,
            Code = configuration["SEED_REGION_CODE"] ?? "DEV-REGION",
            Name = configuration["SEED_REGION_NAME"] ?? "Development Region",
            IsActive = true,
            CreatedAt = DateTimeOffset.UtcNow
        };

        db.Regions.Add(region);
        await db.SaveChangesAsync();

        return region;
    }

    private static async Task<Tenant> SeedDefaultTenantAsync(
        AppDbContext db,
        IConfiguration configuration,
        Region region)
    {
        var tenantId = configuration["SEED_TENANT_ID"] ?? "dev-center";
        var defaultDistrict = await db.Districts
            .Include(x => x.Region)
            .Where(x => x.IsActive && !x.IsDeleted && x.Region.IsActive && !x.Region.IsDeleted)
            .OrderBy(x => x.Region.Name)
            .ThenBy(x => x.Name)
            .FirstOrDefaultAsync();

        if (defaultDistrict is null)
        {
            throw new InvalidOperationException("At least one active district is required before tenant seed.");
        }
        
        var tenant = await db.Tenants.FirstOrDefaultAsync(x => x.Id == tenantId);
        if (tenant is not null)
        {
            var hasChanges = false;

            if (tenant.RegionId is null)
            {
                tenant.RegionId = region.Id;
                hasChanges = true;
            }

            if (string.IsNullOrWhiteSpace(tenant.TimeZoneId))
            {
                tenant.TimeZoneId = configuration["SEED_TENANT_TIME_ZONE"] ?? "Asia/Bishkek";
                hasChanges = true;
            }

            if (tenant.GeoRegionId <= 0)
            {
                tenant.GeoRegionId = defaultDistrict.RegionId;
                hasChanges = true;
            }

            if (tenant.DistrictId <= 0)
            {
                tenant.DistrictId = defaultDistrict.Id;
                hasChanges = true;
            }

            if (hasChanges)
            {
                await db.SaveChangesAsync();
            }

            return tenant;
        }

        tenant = new Tenant
        {
            Id = tenantId,
            Code = configuration["SEED_TENANT_CODE"] ?? "DEV",
            Name = configuration["SEED_TENANT_NAME"] ?? "Development Dialysis Center",
            TimeZoneId = configuration["SEED_TENANT_TIME_ZONE"] ?? "Asia/Bishkek",
            Locale = "ru-RU",
            RegionId = region.Id,
            GeoRegionId = defaultDistrict.RegionId,
            DistrictId = defaultDistrict.Id,
            IsActive = true,
            CreatedAt = DateTimeOffset.UtcNow
        };

        db.Tenants.Add(tenant);
        await db.SaveChangesAsync();

        return tenant;
    }
}
