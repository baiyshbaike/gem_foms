using Application.Tenants;
using Contracts.Tenants;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace Api.Controllers;

[ApiController]
[Route("api/v1/tenants")]
[Authorize]
public sealed class TenantsController : ControllerBase
{
    private readonly ITenantService _tenantService;

    public TenantsController(ITenantService tenantService)
    {
        _tenantService = tenantService;
    }

    [HttpGet("my")]
    public async Task<ActionResult<IReadOnlyList<TenantDto>>> My(CancellationToken cancellationToken)
    {
        var userId = CurrentUserId();
        if (userId is null)
        {
            return Unauthorized();
        }

        var tenants = await _tenantService.GetMyTenantsAsync(userId.Value, cancellationToken);
        return Ok(tenants);
    }

    [HttpPost("{tenantId}/switch")]
    public async Task<ActionResult<SwitchTenantResponse>> Switch(string tenantId, CancellationToken cancellationToken)
    {
        var userId = CurrentUserId();
        if (userId is null)
        {
            return Unauthorized();
        }

        var response = await _tenantService.SwitchAsync(userId.Value, tenantId, cancellationToken);
        return response is null ? Forbid() : Ok(response);
    }

    private long? CurrentUserId()
    {
        var sub = User.FindFirst("sub")?.Value;
        return long.TryParse(sub, out var userId) ? userId : null;
    }
}
