using Application.Authorization;
using Application.MedCenterMachines;
using Application.Tenants;
using Contracts.MedCenterMachines;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace Api.Controllers;

[ApiController]
[Route("api/v1/med-center-machines")]
[Authorize]
public sealed class MedCenterMachinesController : ControllerBase
{
    private readonly IMedCenterMachineService _machineService;
    private readonly ITenantContext _tenant;

    public MedCenterMachinesController(
        IMedCenterMachineService machineService,
        ITenantContext tenant)
    {
        _machineService = machineService;
        _tenant = tenant;
    }

    [HttpGet]
    [Authorize(Policy = "Permission:" + Permissions.MedCenterMachineRead)]
    public async Task<ActionResult<IReadOnlyList<MedCenterMachineDto>>> Get(
        [FromQuery] List<string>? tenantIds,
        CancellationToken cancellationToken)
    {
        var userId = CurrentUserId();
        if (userId is null)
        {
            return Unauthorized();
        }

        var machines = await _machineService.GetAsync(userId.Value, tenantIds, cancellationToken);
        return machines is null ? Forbid() : Ok(machines);
    }

    [HttpGet("{id:long}")]
    [Authorize(Policy = "Permission:" + Permissions.MedCenterMachineRead)]
    public async Task<ActionResult<MedCenterMachineDto>> GetById(
        long id,
        CancellationToken cancellationToken)
    {
        var userId = CurrentUserId();
        if (userId is null)
        {
            return Unauthorized();
        }

        var machine = await _machineService.GetByIdAsync(
            userId.Value,
            _tenant.RequiredTenantId,
            id,
            cancellationToken);

        return machine is null ? NotFound() : Ok(machine);
    }

    [HttpPost]
    [Authorize(Policy = "Permission:" + Permissions.MedCenterMachineCreate)]
    public async Task<ActionResult<MedCenterMachineDto>> Create(
        CreateMedCenterMachineRequest request,
        CancellationToken cancellationToken)
    {
        var userId = CurrentUserId();
        if (userId is null)
        {
            return Unauthorized();
        }

        var result = await _machineService.CreateAsync(
            _tenant.RequiredTenantId,
            userId.Value,
            request,
            cancellationToken);

        return result.Status switch
        {
            MedCenterMachineCommandStatus.Succeeded => CreatedAtAction(nameof(GetById), new { id = result.Value!.Id }, result.Value),
            MedCenterMachineCommandStatus.Forbidden => Forbid(),
            MedCenterMachineCommandStatus.Conflict => Conflict(),
            _ => Conflict()
        };
    }

    [HttpPut("{id:long}")]
    [Authorize(Policy = "Permission:" + Permissions.MedCenterMachineUpdate)]
    public async Task<ActionResult<MedCenterMachineDto>> Update(
        long id,
        UpdateMedCenterMachineRequest request,
        CancellationToken cancellationToken)
    {
        var userId = CurrentUserId();
        if (userId is null)
        {
            return Unauthorized();
        }

        var result = await _machineService.UpdateAsync(
            _tenant.RequiredTenantId,
            id,
            userId.Value,
            request,
            cancellationToken);

        return result.Status switch
        {
            MedCenterMachineCommandStatus.Succeeded => Ok(result.Value),
            MedCenterMachineCommandStatus.Forbidden => Forbid(),
            MedCenterMachineCommandStatus.NotFound => NotFound(),
            MedCenterMachineCommandStatus.Conflict => Conflict(),
            _ => Conflict()
        };
    }

    [HttpDelete("{id:long}")]
    [Authorize(Policy = "Permission:" + Permissions.MedCenterMachineDelete)]
    public async Task<IActionResult> Delete(
        long id,
        CancellationToken cancellationToken)
    {
        var userId = CurrentUserId();
        if (userId is null)
        {
            return Unauthorized();
        }

        var result = await _machineService.DeleteAsync(
            _tenant.RequiredTenantId,
            id,
            userId.Value,
            cancellationToken);

        return result.Status switch
        {
            MedCenterMachineCommandStatus.Succeeded => NoContent(),
            MedCenterMachineCommandStatus.Forbidden => Forbid(),
            MedCenterMachineCommandStatus.NotFound => NotFound(),
            MedCenterMachineCommandStatus.Conflict => Conflict(),
            _ => Conflict()
        };
    }

    private long? CurrentUserId()
    {
        var sub = User.FindFirst("sub")?.Value;
        return long.TryParse(sub, out var userId) ? userId : null;
    }
}