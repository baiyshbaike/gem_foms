using Application.Authorization;
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

    [HttpPost("grid/query")]
    [Authorize(Policy = "Permission:" + Permissions.AdminTenants)]
    public async Task<ActionResult<TenantGridQueryResult>> QueryGrid(
        TenantGridQueryRequest request,
        CancellationToken cancellationToken)
    {
        var userId = CurrentUserId();
        if (userId is null)
        {
            return Unauthorized();
        }

        return Ok(await _tenantService.QueryGridAsync(userId.Value, request, cancellationToken));
    }

    [HttpPost("grid/export")]
    [Authorize(Policy = "Permission:" + Permissions.AdminTenants)]
    public async Task<ActionResult<TenantGridQueryResult>> ExportGrid(
        TenantGridExportRequest request,
        CancellationToken cancellationToken)
    {
        var userId = CurrentUserId();
        if (userId is null)
        {
            return Unauthorized();
        }

        return Ok(await _tenantService.ExportGridAsync(userId.Value, request, cancellationToken));
    }

    [HttpGet("my")]
    [Authorize(Policy = "Permission:" + Permissions.TenantRead)]
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

    [HttpGet("{tenantId}")]
    [Authorize(Policy = "Permission:" + Permissions.AdminTenants)]
    public async Task<ActionResult<TenantDetailsDto>> GetById(
        string tenantId,
        CancellationToken cancellationToken)
    {
        var userId = CurrentUserId();
        if (userId is null)
        {
            return Unauthorized();
        }

        var tenant = await _tenantService.GetByIdAsync(userId.Value, tenantId, cancellationToken);
        return tenant is null ? NotFound() : Ok(tenant);
    }

    [HttpPost]
    [Authorize(Policy = "Permission:" + Permissions.AdminTenants)]
    public async Task<ActionResult<TenantDetailsDto>> Create(
        CreateTenantRequest request,
        CancellationToken cancellationToken)
    {
        var userId = CurrentUserId();
        if (userId is null)
        {
            return Unauthorized();
        }

        var result = await _tenantService.CreateAsync(userId.Value, request, cancellationToken);
        return result.Status switch
        {
            TenantCommandStatus.Succeeded => CreatedAtAction(
                nameof(GetById),
                new { tenantId = result.Value!.Id },
                result.Value),
            TenantCommandStatus.Conflict => ConflictProblem(result.Error),
            TenantCommandStatus.ValidationFailed => BadRequestProblem(result.Error),
            _ => BadRequest()
        };
    }

    [HttpPut("{tenantId}")]
    [Authorize(Policy = "Permission:" + Permissions.AdminTenants)]
    public async Task<ActionResult<TenantDetailsDto>> Update(
        string tenantId,
        UpdateTenantRequest request,
        CancellationToken cancellationToken)
    {
        var userId = CurrentUserId();
        if (userId is null)
        {
            return Unauthorized();
        }

        var result = await _tenantService.UpdateAsync(userId.Value, tenantId, request, cancellationToken);
        return result.Status switch
        {
            TenantCommandStatus.Succeeded => Ok(result.Value),
            TenantCommandStatus.NotFound => NotFoundProblem(result.Error),
            TenantCommandStatus.Conflict => ConflictProblem(result.Error),
            TenantCommandStatus.ValidationFailed => BadRequestProblem(result.Error),
            _ => BadRequest()
        };
    }

    [HttpDelete("{tenantId}")]
    [Authorize(Policy = "Permission:" + Permissions.AdminTenants)]
    public async Task<IActionResult> Deactivate(
        string tenantId,
        CancellationToken cancellationToken)
    {
        var userId = CurrentUserId();
        if (userId is null)
        {
            return Unauthorized();
        }

        var result = await _tenantService.DeactivateAsync(userId.Value, tenantId, cancellationToken);
        return result.Status switch
        {
            TenantCommandStatus.Succeeded => NoContent(),
            TenantCommandStatus.NotFound => NotFoundProblem(result.Error),
            TenantCommandStatus.Conflict => ConflictProblem(result.Error),
            _ => BadRequest()
        };
    }

    [HttpPost("{tenantId}/switch")]
    [Authorize(Policy = "Permission:" + Permissions.TenantSwitch)]
    public async Task<ActionResult<SwitchTenantResponse>> Switch(
        string tenantId,
        CancellationToken cancellationToken)
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

    private ObjectResult ConflictProblem(string? detail) =>
        Problem(statusCode: StatusCodes.Status409Conflict, detail: detail);

    private ObjectResult BadRequestProblem(string? detail) =>
        Problem(statusCode: StatusCodes.Status400BadRequest, detail: detail);

    private ObjectResult NotFoundProblem(string? detail) =>
        Problem(statusCode: StatusCodes.Status404NotFound, detail: detail);
}
