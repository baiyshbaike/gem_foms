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

// For more information on enabling MVC for empty projects, visit https://go.microsoft.com/fwlink/?LinkID=397860

namespace Dialysis.Server.Controllers
{
    [Route("api/user")]
    [ApiController]
    [Authorize]
    public class UserController : Controller
    {
        private readonly IUserService _userService;

        public UserController(IUserService userService)
        {
            _userService = userService;
        }

       

        [HttpPost("adduser")]
        public async Task<ActionResult> AddUser([FromBody] User model)
        {            
            var response = await _userService.AddUser(model);
            return Ok(response);
        }

        [HttpGet("allusers")]
        public async Task<ActionResult> AllUsers()
        {
            var response = await _userService.AllUsers();
            return Ok(response);
        }
        [HttpGet("allroles")]
        public async Task<ActionResult> AllRoles()
        {
            var response = await _userService.AllRoles();
            return Ok(response);
        }

        [HttpGet("userroles")]
        public async Task<ActionResult> UserRoles([FromQuery] long Id)
        {
            var response = await _userService.UserRoles(Id);
            return Ok(response);
        }

        [HttpGet("profileroles")]
        public async Task<ActionResult> ProfileRoles([FromQuery] long Id)
        {
            var response = await _userService.ProfileRoles(Id);
            return Ok(response);
        }

        [HttpGet("addremrole")]
        public async Task<ActionResult> AddRemRole([FromQuery] long UserId, [FromQuery] long RoleId, [FromQuery] int status)
        {
            var response = await _userService.AddRemoveRole(RoleId, UserId, 0, status);
            return Ok(response);
        }

        [HttpGet("addremprorole")]
        public async Task<ActionResult> AddRemProRole([FromQuery] long ProfileId, [FromQuery] long RoleId, [FromQuery] int status)
        {
            var response = await _userService.AddRemoveRole(RoleId, 0, ProfileId, status);
            return Ok(response);
        }
        [HttpGet("getuserprofile")]
        public async Task<ActionResult> GetUserProfile([FromQuery]long Id)
        {
            var response = await _userService.GetUserProfile(Id);
            return Ok(response);
        }
    }
}


