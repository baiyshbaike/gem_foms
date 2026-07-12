using Application.Audit;
using Application.Auth;
using Application.Tenants;
using Contracts.Tenants;
using Infrastructure.Data;
using Microsoft.EntityFrameworkCore;

namespace Infrastructure.Tenants;

public sealed class TenantService : ITenantService
{
    private readonly AppDbContext _db;
    private readonly IJwtTokenService _jwtTokenService;
    private readonly IActionLogService _actionLogService;
    private readonly ITenantAccessService _tenantAccessService;

    public TenantService(
        AppDbContext db,
        IJwtTokenService jwtTokenService,
        IActionLogService actionLogService,
        ITenantAccessService tenantAccessService)
    {
        _db = db;
        _jwtTokenService = jwtTokenService;
        _actionLogService = actionLogService;
        _tenantAccessService = tenantAccessService;
    }

    public async Task<IReadOnlyList<TenantDto>> GetMyTenantsAsync(long userId, CancellationToken cancellationToken)
    {
        return await _tenantAccessService.GetAccessibleTenantsAsync(userId, cancellationToken);
    }

    public async Task<SwitchTenantResponse?> SwitchAsync(
        long userId,
        string tenantId,
        CancellationToken cancellationToken)
    {
        var user = await _db.Users.FirstOrDefaultAsync(x => x.Id == userId, cancellationToken);
        if (user is null || !user.IsActive)
        {
            return null;
        }

        var canAccessTenant = await _tenantAccessService.CanAccessTenantAsync(
            userId,
            tenantId,
            cancellationToken);

        if (!canAccessTenant)
        {
            await _actionLogService.AddAsync(new ActionLogRequest
            {
                UserId = userId,
                UsernameSnapshot = user.Username,
                Action = "TenantSwitchFailed",
                Module = "tenant",
                EntityName = "Tenant",
                EntityId = tenantId,
                StatusCode = 403,
                Succeeded = false,
                FailureReason = "Tenant access denied"
            }, cancellationToken);

            await _db.SaveChangesAsync(cancellationToken);
            return null;
        }

        var tenant = await _db.Tenants
            .AsNoTracking()
            .FirstOrDefaultAsync(x => x.Id == tenantId && x.IsActive, cancellationToken);

        if (tenant is null)
        {
            return null;
        }

        var permissions = await _db.UserRoles
            .AsNoTracking()
            .Where(x => x.UserId == user.Id)
            .SelectMany(x => x.Role.RolePermissions)
            .Select(x => x.Permission.Code)
            .Distinct()
            .ToListAsync(cancellationToken);

        var token = _jwtTokenService.CreateAccessToken(user, permissions, tenant.Id);

        await _actionLogService.AddAsync(new ActionLogRequest
        {
            UserId = userId,
            UsernameSnapshot = user.Username,
            Action = "TenantSwitchSucceeded",
            Module = "tenant",
            EntityName = "Tenant",
            EntityId = tenant.Id,
            StatusCode = 200,
            Succeeded = true
        }, cancellationToken);

        await _db.SaveChangesAsync(cancellationToken);

        return new SwitchTenantResponse(
            token.Token,
            token.ExpiresAt,
            new TenantDto(tenant.Id, tenant.Code, tenant.Name));
    }
}