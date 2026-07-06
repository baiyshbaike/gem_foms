using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Dialysis.Server.Domain.Services;
using Dialysis.Shared.Models;
using Dialysis.Shared.Params;
using IdentityModel.Client;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;


namespace Dialysis.Server.Controllers
{
    [Route("api/region")]
    [ApiController]
    [Authorize]
    public class RegionController : Controller
    {
        private readonly IRegionService _regionService;
        private readonly IActiveUserService _currentUser;

        public RegionController(IRegionService regionService, IActiveUserService currentUser)
        {
            _regionService = regionService;
            _currentUser = currentUser;
        }

        [HttpPost("adddistrict")]
        public async Task<ActionResult> AddDistrict([FromBody] District model)
        {
            var userId = _currentUser.UserId;
            //if (model != null) model.CreatedBy = _currentUser.UserId;

            var response = await _regionService.AddDistrict(model);
            return Ok(response);
        }

        [HttpGet("alldistricts")]
        public async Task<ActionResult> AllDistricts()
        {

            var response = await _regionService.AllDistricts();
            return Ok(response);
        }
     
        [HttpPost("addregion")]
        public async Task<ActionResult> AddRegion([FromBody] Region model)
        {
            
            var response = await _regionService.AddRegion(model);
            return Ok(response);
        }

        [HttpGet("alldistregions")]
        public async Task<ActionResult> DistRegions([FromQuery] long Id)
        {
            var response = await _regionService.DistRegions(Id);
            return Ok(response);
        }

        [HttpGet("allregions")]
        public async Task<ActionResult> AllRegions()
        {
            var response = await _regionService.AllRegions();
            return Ok(response);
        }
    }
}


