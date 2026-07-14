using Application.Authorization;
using Application.Patients;
using Contracts.Patients;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace Api.Controllers;

[ApiController]
[Route("api/v1/patients")]
[Authorize]
public sealed class PatientsController : ControllerBase
{
    private readonly IPatientService _patientService;

    public PatientsController(IPatientService patientService)
    {
        _patientService = patientService;
    }

    [HttpPost("grid/query")]
    [Authorize(Policy = "Permission:" + Permissions.PatientRead)]
    public async Task<ActionResult<PatientGridLoadResult>> LoadGrid(
        PatientGridLoadRequest request,
        CancellationToken cancellationToken)
    {
        return Ok(await _patientService.LoadGridAsync(request, cancellationToken));
    }

    [HttpPost("grid/export")]
    [Authorize(Policy = "Permission:" + Permissions.PatientExport)]
    public async Task<ActionResult<PatientGridLoadResult>> ExportGrid(
        PatientGridExportRequest request,
        CancellationToken cancellationToken)
    {
        return Ok(await _patientService.ExportGridAsync(request, cancellationToken));
    }

    [HttpGet("groups")]
    [Authorize(Policy = "Permission:" + Permissions.PatientRead)]
    public async Task<ActionResult<IReadOnlyList<PatientGroupDto>>> GetGroups(
        CancellationToken cancellationToken)
    {
        return Ok(await _patientService.GetGroupsAsync(cancellationToken));
    }

    [HttpGet]
    [Authorize(Policy = "Permission:" + Permissions.PatientRead)]
    public async Task<ActionResult<IReadOnlyList<PatientDto>>> Get(
        [FromQuery] string? search,
        [FromQuery] long? groupId,
        CancellationToken cancellationToken)
    {
        var patients = await _patientService.GetAsync(search, groupId, cancellationToken);
        return Ok(patients);
    }

    [HttpGet("{id:long}")]
    [Authorize(Policy = "Permission:" + Permissions.PatientRead)]
    public async Task<ActionResult<PatientDto>> GetById(long id, CancellationToken cancellationToken)
    {
        var patient = await _patientService.GetByIdAsync(id, cancellationToken);
        return patient is null ? NotFound() : Ok(patient);
    }

    [HttpGet("by-inn/{inn}")]
    [Authorize(Policy = "Permission:" + Permissions.PatientLookup)]
    public async Task<ActionResult<PatientDto>> GetByInn(string inn, CancellationToken cancellationToken)
    {
        var patient = await _patientService.GetByInnAsync(inn, cancellationToken);
        return patient is null ? NotFound() : Ok(patient);
    }

    [HttpPost]
    [Authorize(Policy = "Permission:" + Permissions.PatientCreate)]
    public async Task<ActionResult<PatientDto>> Create(CreatePatientRequest request, CancellationToken cancellationToken)
    {
        var userId = CurrentUserId();
        if (userId is null)
        {
            return Unauthorized();
        }

        var result = await _patientService.CreateAsync(userId.Value, request, cancellationToken);

        return result.Status switch
        {
            PatientCommandStatus.Succeeded => CreatedAtAction(
                nameof(GetById),
                new { id = result.Value!.Id },
                result.Value),
            PatientCommandStatus.Conflict => Conflict(),
            PatientCommandStatus.ValidationFailed => BadRequest(),
            _ => BadRequest()
        };
    }

    [HttpPut("{id:long}")]
    [Authorize(Policy = "Permission:" + Permissions.PatientUpdate)]
    public async Task<ActionResult<PatientDto>> Update(long id, UpdatePatientRequest request, CancellationToken cancellationToken)
    {
        var userId = CurrentUserId();
        if (userId is null)
        {
            return Unauthorized();
        }

        var result = await _patientService.UpdateAsync(id, userId.Value, request, cancellationToken);
        return result.Status switch
        {
            PatientCommandStatus.Succeeded => Ok(result.Value),
            PatientCommandStatus.NotFound => NotFound(),
            PatientCommandStatus.Conflict => Conflict(),
            PatientCommandStatus.ValidationFailed => BadRequest(),
            _ => BadRequest()
        };
    }

    [HttpDelete("{id:long}")]
    [Authorize(Policy = "Permission:" + Permissions.PatientDelete)]
    public async Task<IActionResult> Delete(long id, CancellationToken cancellationToken)
    {
        var userId = CurrentUserId();
        if (userId is null)
        {
            return Unauthorized();
        }

        var deleted = await _patientService.DeleteAsync(id, userId.Value, cancellationToken);
        return deleted ? NoContent() : NotFound();
    }

    private long? CurrentUserId()
    {
        var sub = User.FindFirst("sub")?.Value;
        return long.TryParse(sub, out var userId) ? userId : null;
    }
}
