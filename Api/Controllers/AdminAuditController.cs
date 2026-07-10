using Application.Admin;
using Application.Authorization;
using Contracts.Admin;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace Api.Controllers;

[ApiController]
[Route("api/v1/admin/audit-logs")]
[Authorize(Policy = "Permission:" + Permissions.AdminAudit)]
public sealed class AdminAuditController : ControllerBase
{
    private readonly IAuditLogQueryService _auditLogQueryService;

    public AdminAuditController(IAuditLogQueryService auditLogQueryService)
    {
        _auditLogQueryService = auditLogQueryService;
    }

    [HttpGet]
    public async Task<ActionResult<IReadOnlyList<AuditLogDto>>> Get(CancellationToken cancellationToken)
    {
        return Ok(await _auditLogQueryService.GetLatestAsync(100, cancellationToken));
    }
}