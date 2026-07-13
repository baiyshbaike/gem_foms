using Application.Authorization;
using Application.Sessions;
using Application.Tenants;
using Contracts.Sessions;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace Api.Controllers;

[ApiController]
[Route("api/v1/settings/session-workflow")]
[Authorize]
public sealed class SessionWorkflowSettingsController : ControllerBase
{
    private readonly ISessionWorkflowSettingsService _settingsService;
    private readonly ITenantContext _tenantContext;

    public SessionWorkflowSettingsController(
        ISessionWorkflowSettingsService settingsService,
        ITenantContext tenantContext)
    {
        _settingsService = settingsService;
        _tenantContext = tenantContext;
    }

    [HttpGet]
    [Authorize(Policy = "Permission:" + Permissions.SessionSettingsManage)]
    public async Task<ActionResult<SessionWorkflowSettingsDto>> Get(CancellationToken cancellationToken)
    {
        return Ok(await _settingsService.GetAsync(_tenantContext.RequiredTenantId, cancellationToken));
    }

    [HttpPut]
    [Authorize(Policy = "Permission:" + Permissions.SessionSettingsManage)]
    public async Task<ActionResult<SessionWorkflowSettingsDto>> Update(
        UpdateSessionWorkflowSettingsRequest request,
        CancellationToken cancellationToken)
    {
        var userId = CurrentUserId();
        if (userId is null)
        {
            return Unauthorized();
        }

        var settings = await _settingsService.UpdateAsync(
            _tenantContext.RequiredTenantId,
            userId.Value,
            request,
            cancellationToken);

        return Ok(settings);
    }

    private long? CurrentUserId()
    {
        var sub = User.FindFirst("sub")?.Value;
        return long.TryParse(sub, out var userId) ? userId : null;
    }
}
