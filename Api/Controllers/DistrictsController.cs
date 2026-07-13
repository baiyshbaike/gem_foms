using Application.Authorization;
using Application.Regions;
using Contracts.Regions;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace Api.Controllers;

[ApiController]
[Route("api/v1/districts")]
[Authorize]
public sealed class DistrictsController : ControllerBase
{
    private readonly IDistrictService _districtService;

    public DistrictsController(IDistrictService districtService)
    {
        _districtService = districtService;
    }

    [HttpGet]
    [Authorize(Policy = "Permission:" + Permissions.DistrictRead)]
    public async Task<ActionResult<IReadOnlyList<DistrictDto>>> Get(
        [FromQuery] long? regionId,
        [FromQuery] bool includeInactive,
        CancellationToken cancellationToken)
    {
        return Ok(await _districtService.GetAsync(regionId, includeInactive, cancellationToken));
    }

    [HttpGet("{id:long}")]
    [Authorize(Policy = "Permission:" + Permissions.DistrictRead)]
    public async Task<ActionResult<DistrictDto>> GetById(
        long id,
        [FromQuery] bool includeInactive,
        CancellationToken cancellationToken)
    {
        var district = await _districtService.GetByIdAsync(id, includeInactive, cancellationToken);
        return district is null ? NotFound() : Ok(district);
    }
}
