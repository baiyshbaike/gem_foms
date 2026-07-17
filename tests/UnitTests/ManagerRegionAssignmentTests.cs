using Application.Admin;
using Application.Audit;
using Application.Authorization;
using Contracts.Admin;
using Domain.Tenants;
using Domain.Users;
using Infrastructure.Admin;
using Infrastructure.Data;
using Infrastructure.Tenants;
using Microsoft.EntityFrameworkCore;

namespace UnitTests;

public sealed class ManagerRegionAssignmentTests
{
    [Fact]
    public async Task Manager_can_access_all_tenants_in_assigned_region_only()
    {
        await using var context = CreateContext();
        var manager = new User
        {
            Id = 10,
            Username = "manager",
            IsActive = true,
            CreatedAt = DateTimeOffset.UtcNow
        };
        var role = new Role
        {
            Id = 20,
            Code = "Manager",
            Name = "Manager",
            CreatedAt = DateTimeOffset.UtcNow
        };
        var permission = new Permission
        {
            Id = 30,
            Code = Permissions.TenantAccessAssigned,
            Module = "tenant",
            Name = Permissions.TenantAccessAssigned
        };
        var north = Region(101, "North");
        var south = Region(102, "South");

        context.AddRange(manager, role, permission, north, south);
        context.UserRoles.Add(new UserRole { UserId = manager.Id, RoleId = role.Id });
        context.RolePermissions.Add(new RolePermission { RoleId = role.Id, PermissionId = permission.Id });
        context.ManagerRegionAssignments.Add(new ManagerRegionAssignment
        {
            UserId = manager.Id,
            RegionId = north.Id,
            AssignedAt = DateTimeOffset.UtcNow,
            AssignedBy = 1
        });
        context.Tenants.AddRange(
            Tenant("north-a", north.Id, 1001),
            Tenant("north-b", north.Id, 1002),
            Tenant("south-a", south.Id, 2001));
        await context.SaveChangesAsync();

        var service = new TenantAccessService(context);
        var tenantIds = await service.GetAccessibleTenantIdsAsync(manager.Id, CancellationToken.None);

        Assert.Equal(["north-a", "north-b"], tenantIds.OrderBy(id => id).ToArray());
    }

    [Fact]
    public async Task Admin_user_service_assigns_reassigns_and_revokes_manager_region()
    {
        await using var context = CreateContext();
        var managerRole = new Role
        {
            Id = 20,
            Code = "Manager",
            Name = "Manager",
            CreatedAt = DateTimeOffset.UtcNow
        };
        var doctorRole = new Role
        {
            Id = 21,
            Code = "Doctor",
            Name = "Doctor",
            CreatedAt = DateTimeOffset.UtcNow
        };
        context.AddRange(managerRole, doctorRole, Region(101, "North"), Region(102, "South"));
        await context.SaveChangesAsync();
        var audit = new RecordingActionLogService();
        var service = new AdminUserService(context, audit);

        var missingRegion = await service.CreateAsync(1, new CreateAdminUserRequest
        {
            Username = "invalid-manager",
            Password = "password",
            FirstName = "Invalid",
            LastName = "Manager",
            RoleIds = [managerRole.Id]
        }, CancellationToken.None);
        Assert.Equal(AdminUserCommandStatus.ValidationFailed, missingRegion.Status);

        var created = await service.CreateAsync(1, new CreateAdminUserRequest
        {
            Username = "manager",
            Password = "password",
            FirstName = "Region",
            LastName = "Manager",
            RoleIds = [managerRole.Id],
            ManagerRegionId = 101
        }, CancellationToken.None);
        Assert.Equal(AdminUserCommandStatus.Succeeded, created.Status);
        Assert.Equal(101, created.Value!.ManagerRegion!.Id);

        var reassigned = await service.UpdateAsync(1, created.Value.Id, new UpdateAdminUserRequest
        {
            Username = created.Value.Username,
            FirstName = created.Value.FirstName,
            LastName = created.Value.LastName,
            IsActive = true,
            RoleIds = [managerRole.Id],
            ManagerRegionId = 102
        }, CancellationToken.None);
        Assert.Equal(AdminUserCommandStatus.Succeeded, reassigned.Status);
        Assert.Equal(102, reassigned.Value!.ManagerRegion!.Id);
        Assert.Equal(2, context.ManagerRegionAssignments.Count());
        Assert.Single(context.ManagerRegionAssignments, assignment => assignment.RevokedAt == null);
        Assert.Contains(
            context.ManagerRegionAssignments,
            assignment => assignment.RegionId == 101
                && assignment.RevokedAt != null
                && assignment.RevokedBy == 1);

        var roleRemoved = await service.UpdateAsync(1, created.Value.Id, new UpdateAdminUserRequest
        {
            Username = created.Value.Username,
            FirstName = created.Value.FirstName,
            LastName = created.Value.LastName,
            IsActive = true,
            RoleIds = [doctorRole.Id],
            ManagerRegionId = null
        }, CancellationToken.None);
        Assert.Equal(AdminUserCommandStatus.Succeeded, roleRemoved.Status);
        Assert.Null(roleRemoved.Value!.ManagerRegion);
        Assert.DoesNotContain(context.ManagerRegionAssignments, assignment => assignment.RevokedAt == null);
        Assert.Contains(audit.Entries, entry => entry.Action == "ManagerRegionAssigned");
        Assert.Contains(audit.Entries, entry => entry.Action == "ManagerRegionReassigned");
        Assert.Contains(audit.Entries, entry => entry.Action == "ManagerRegionRevoked");
    }

    [Fact]
    public void Manager_region_model_has_one_active_assignment_index()
    {
        using var context = CreateContext();
        var entity = context.Model.FindEntityType(typeof(ManagerRegionAssignment));
        var index = Assert.Single(entity!.GetIndexes(), index => index.IsUnique);

        Assert.Equal([nameof(ManagerRegionAssignment.UserId)], index.Properties.Select(x => x.Name));
        Assert.Equal("\"RevokedAt\" IS NULL", index.GetFilter());
    }

    private static AppDbContext CreateContext()
    {
        var options = new DbContextOptionsBuilder<AppDbContext>()
            .UseInMemoryDatabase(Guid.NewGuid().ToString("N"))
            .Options;
        return new AppDbContext(options);
    }

    private static Domain.Regions.Region Region(long id, string name) => new()
    {
        Id = id,
        Name = name,
        IsActive = true,
        CreatedAt = DateTimeOffset.UtcNow,
        CreatedBy = 1
    };

    private static Tenant Tenant(string id, long regionId, long districtId) => new()
    {
        Id = id,
        Code = id.ToUpperInvariant(),
        Name = id,
        RegionId = regionId,
        DistrictId = districtId,
        IsActive = true,
        CreatedAt = DateTimeOffset.UtcNow
    };

    private sealed class RecordingActionLogService : IActionLogService
    {
        public List<ActionLogRequest> Entries { get; } = [];

        public Task AddAsync(ActionLogRequest request, CancellationToken cancellationToken)
        {
            Entries.Add(request);
            return Task.CompletedTask;
        }
    }
}
