using Application.Authorization;
using Application.Regions;
using Contracts.Regions;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace Api.Controllers;

[ApiController]
[Route("api/v1/regions")]
[Authorize]
public sealed class RegionsController : ControllerBase
{
    private readonly IRegionService _regionService;

    public RegionsController(IRegionService regionService)
    {
        _regionService = regionService;
    }

    [HttpGet]
    [Authorize(Policy = "Permission:" + Permissions.RegionRead)]
    public async Task<ActionResult<IReadOnlyList<RegionDto>>> Get(
        [FromQuery] bool includeInactive,
        CancellationToken cancellationToken)
    {
        return Ok(await _regionService.GetAsync(includeInactive, cancellationToken));
    }

    [HttpGet("{id:long}")]
    [Authorize(Policy = "Permission:" + Permissions.RegionRead)]
    public async Task<ActionResult<RegionDto>> GetById(
        long id,
        [FromQuery] bool includeInactive,
        CancellationToken cancellationToken)
    {
        var region = await _regionService.GetByIdAsync(id, includeInactive, cancellationToken);
        return region is null ? NotFound() : Ok(region);
    }

    [HttpGet("{id:long}/districts")]
    [Authorize(Policy = "Permission:" + Permissions.DistrictRead)]
    public async Task<ActionResult<IReadOnlyList<DistrictDto>>> GetDistricts(
        long id,
        [FromQuery] bool includeInactive,
        CancellationToken cancellationToken)
    {
        var districts = await _regionService.GetDistrictsAsync(id, includeInactive, cancellationToken);
        return districts is null ? NotFound() : Ok(districts);
    }
}
