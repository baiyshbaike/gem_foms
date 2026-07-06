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
    [Route("api/profile")]
    [ApiController]
    [Authorize]
    public class ProfileController : Controller
    {
        private readonly IUserProfileService _profileService;
        private readonly IActiveUserService _currentUser;

        public ProfileController(IUserProfileService profileService, IActiveUserService currentUser)
        {
            _profileService = profileService;
            _currentUser = currentUser;
        }

        [HttpPost("addprofile")]
        public async Task<ActionResult> AddProfile([FromBody] UserProfile model)
        {
            var userId = _currentUser.UserId;
            //if (model != null) model.CreatedBy = _currentUser.UserId;

            var response = await _profileService.AddProfile(model);
            return Ok(response);
        }

        [HttpGet("allprofiles")]
        public async Task<ActionResult> AllProfiles()
        {

            var response = await _profileService.AllProfiles();
            return Ok(response);
        }
        [HttpGet("getuserbyprofileid")]
        public async Task<ActionResult> GetUserByProfileId()
        {
            var response = await _profileService.GetUserByProfileId();
            return Ok(response);
        }
    }
}


