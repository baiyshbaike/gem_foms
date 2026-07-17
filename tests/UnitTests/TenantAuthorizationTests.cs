using System.Security.Claims;
using Api.Auth;
using Application.Authorization;
using Application.Tenants;
using Contracts.Tenants;
using Microsoft.AspNetCore.Authorization;

namespace UnitTests;

public sealed class TenantAuthorizationTests
{
    [Fact]
    public async Task AdminTenants_requirement_needs_matching_permission_claim()
    {
        var requirement = new PermissionRequirement(Permissions.AdminTenants);
        var deniedContext = CreateAuthorizationContext(requirement, []);
        var allowedContext = CreateAuthorizationContext(requirement,
        [
            new Claim("permission", Permissions.AdminTenants)
        ]);
        var handler = new PermissionAuthorizationHandler();

        await handler.HandleAsync(deniedContext);
        await handler.HandleAsync(allowedContext);

        Assert.False(deniedContext.HasSucceeded);
        Assert.True(allowedContext.HasSucceeded);
        Assert.Contains(Permissions.AdminTenants, Permissions.All);
    }

    [Fact]
    public async Task ActiveTenant_requirement_rejects_stale_tenant_claim()
    {
        var requirement = new ActiveTenantRequirement();
        var claims = new[]
        {
            new Claim("sub", "42"),
            new Claim("active_tenant", "disabled-tenant")
        };
        var context = CreateAuthorizationContext(requirement, claims);
        var handler = new ActiveTenantAuthorizationHandler(new DeniedTenantAccessService());

        await handler.HandleAsync(context);

        Assert.False(context.HasSucceeded);
    }

    private static AuthorizationHandlerContext CreateAuthorizationContext(
        IAuthorizationRequirement requirement,
        IEnumerable<Claim> claims)
    {
        var principal = new ClaimsPrincipal(new ClaimsIdentity(claims, "test"));
        return new AuthorizationHandlerContext([requirement], principal, resource: null);
    }

    private sealed class DeniedTenantAccessService : ITenantAccessService
    {
        public Task<IReadOnlyList<TenantDto>> GetAccessibleTenantsAsync(long userId, CancellationToken cancellationToken) =>
            Task.FromResult<IReadOnlyList<TenantDto>>([]);

        public Task<IReadOnlyList<string>> GetAccessibleTenantIdsAsync(long userId, CancellationToken cancellationToken) =>
            Task.FromResult<IReadOnlyList<string>>([]);

        public Task<bool> CanAccessTenantAsync(long userId, string tenantId, CancellationToken cancellationToken) =>
            Task.FromResult(false);

        public Task<TenantFilterResult> ResolveTenantFilterAsync(
            long userId,
            IReadOnlyList<string>? requestedTenantIds,
            CancellationToken cancellationToken) =>
            Task.FromResult(new TenantFilterResult(false, []));
    }
}
