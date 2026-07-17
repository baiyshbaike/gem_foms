using Application.Tenants;
using Microsoft.AspNetCore.Authorization;

namespace Api.Auth;

public static class ActiveTenantPolicy
{
    public const string Name = "ActiveTenant";
}

public sealed class ActiveTenantRequirement : IAuthorizationRequirement;

public sealed class ActiveTenantAuthorizationHandler
    : AuthorizationHandler<ActiveTenantRequirement>
{
    private readonly ITenantAccessService _tenantAccessService;

    public ActiveTenantAuthorizationHandler(ITenantAccessService tenantAccessService)
    {
        _tenantAccessService = tenantAccessService;
    }

    protected override async Task HandleRequirementAsync(
        AuthorizationHandlerContext context,
        ActiveTenantRequirement requirement)
    {
        var userIdValue = context.User.FindFirst("sub")?.Value;
        var tenantId = context.User.FindFirst("active_tenant")?.Value;
        if (!long.TryParse(userIdValue, out var userId) || string.IsNullOrWhiteSpace(tenantId))
        {
            return;
        }

        if (await _tenantAccessService.CanAccessTenantAsync(
                userId,
                tenantId,
                CancellationToken.None))
        {
            context.Succeed(requirement);
        }
    }
}
