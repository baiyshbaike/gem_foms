using Application.Authorization;
using Application.Tenants;
using Contracts.Tenants;
using Infrastructure.Data;
using Microsoft.EntityFrameworkCore;

namespace Infrastructure.Tenants;

public sealed class TenantAccessService : ITenantAccessService
{
    private readonly AppDbContext _db;

    public TenantAccessService(AppDbContext db)
    {
        _db = db;
    }

    public async Task<IReadOnlyList<TenantDto>> GetAccessibleTenantsAsync(
        long userId,
        CancellationToken cancellationToken)
    {
        var tenantIds = await GetAccessibleTenantIdsAsync(userId, cancellationToken);

        return await _db.Tenants
            .AsNoTracking()
            .Where(x => tenantIds.Contains(x.Id) && x.IsActive)
            .OrderBy(x => x.Name)
            .Select(x => new TenantDto(x.Id, x.Code, x.Name))
            .ToListAsync(cancellationToken);
    }

    public async Task<IReadOnlyList<string>> GetAccessibleTenantIdsAsync(
        long userId,
        CancellationToken cancellationToken)
    {
        var permissions = await LoadPermissionsAsync(userId, cancellationToken);

        if (permissions.Contains(Permissions.TenantAccessAll))
        {
            return await _db.Tenants
                .AsNoTracking()
                .Where(x => x.IsActive)
                .Select(x => x.Id)
                .ToListAsync(cancellationToken);
        }

        var tenantIds = new HashSet<string>();

        if (permissions.Contains(Permissions.TenantAccessAssigned))
        {
            var managerTenantIds = await _db.ManagerRegionAccesses
                .AsNoTracking()
                .Where(x => x.UserId == userId && x.RevokedAt == null && x.Region.IsActive)
                .SelectMany(x => x.Region.Tenants)
                .Where(x => x.IsActive)
                .Select(x => x.Id)
                .ToListAsync(cancellationToken);

            foreach (var tenantId in managerTenantIds)
            {
                tenantIds.Add(tenantId);
            }

            var managerGeoRegionTenantIds = await (
                from access in _db.ManagerRegionAccesses.AsNoTracking()
                from tenant in _db.Tenants.AsNoTracking()
                where access.UserId == userId &&
                      access.RevokedAt == null &&
                      access.GeoRegionId != null &&
                      tenant.GeoRegionId == access.GeoRegionId &&
                      tenant.IsActive
                select tenant.Id)
                .ToListAsync(cancellationToken);

            foreach (var tenantId in managerGeoRegionTenantIds)
            {
                tenantIds.Add(tenantId);
            }

            var managerDistrictTenantIds = await (
                from access in _db.ManagerRegionAccesses.AsNoTracking()
                from tenant in _db.Tenants.AsNoTracking()
                where access.UserId == userId &&
                      access.RevokedAt == null &&
                      access.DistrictId != null &&
                      tenant.DistrictId == access.DistrictId &&
                      tenant.IsActive
                select tenant.Id)
                .ToListAsync(cancellationToken);

            foreach (var tenantId in managerDistrictTenantIds)
            {
                tenantIds.Add(tenantId);
            }
        }

        if (permissions.Contains(Permissions.TenantAccessOwn))
        {
            var ownTenantIds = await _db.TenantUsers
                .AsNoTracking()
                .Where(x => x.UserId == userId && x.LeftAt == null && x.Tenant.IsActive)
                .Select(x => x.TenantId)
                .ToListAsync(cancellationToken);

            foreach (var tenantId in ownTenantIds)
            {
                tenantIds.Add(tenantId);
            }
        }

        return tenantIds.ToList();
    }

    public async Task<bool> CanAccessTenantAsync(
        long userId,
        string tenantId,
        CancellationToken cancellationToken)
    {
        var tenantIds = await GetAccessibleTenantIdsAsync(userId, cancellationToken);
        return tenantIds.Contains(tenantId);
    }

    public async Task<TenantFilterResult> ResolveTenantFilterAsync(
        long userId,
        IReadOnlyList<string>? requestedTenantIds,
        CancellationToken cancellationToken)
    {
        var accessibleTenantIds = await GetAccessibleTenantIdsAsync(userId, cancellationToken);

        if (requestedTenantIds is null || requestedTenantIds.Count == 0)
        {
            return new TenantFilterResult(true, accessibleTenantIds);
        }

        var accessibleSet = accessibleTenantIds.ToHashSet();
        var requestedDistinct = requestedTenantIds.Distinct().ToList();
        var hasForbiddenTenant = requestedDistinct.Any(x => !accessibleSet.Contains(x));

        return hasForbiddenTenant
            ? new TenantFilterResult(false, [])
            : new TenantFilterResult(true, requestedDistinct);
    }

    private async Task<HashSet<string>> LoadPermissionsAsync(long userId, CancellationToken cancellationToken)
    {
        var permissions = await _db.UserRoles
            .AsNoTracking()
            .Where(x => x.UserId == userId)
            .SelectMany(x => x.Role.RolePermissions)
            .Select(x => x.Permission.Code)
            .Distinct()
            .ToListAsync(cancellationToken);

        return permissions.ToHashSet();
    }
}
