using System;
using System.Collections.Generic;
using System.Linq;
using System.Security.Claims;
using System.Threading.Tasks;
using Dialysis.Server.Domain;
using Dialysis.Server.Domain.Services;
using Dialysis.Shared.Models;
using Dialysis.Shared.Params;
using Dialysis.Shared.Responses;

using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.RateLimiting;
using Microsoft.EntityFrameworkCore;



namespace Dialysis.Server.Controllers
{
    [Route("api/account")]
    [ApiController]
    public class AccountController : Controller
    {
          private readonly ILoginService _loginService;
          private readonly ITokenBlacklistService _tokenBlacklistService;
          private readonly IActiveUserService _activeUser;
          private readonly AppDbContext _dbContext;
          private readonly IActionLogService _actionLogService;

          public AccountController(
              ILoginService loginService,
              ITokenBlacklistService tokenBlacklistService,
              IActiveUserService activeUser,
              AppDbContext dbContext,
              IActionLogService actionLogService)
          {
              _loginService = loginService;
              _tokenBlacklistService = tokenBlacklistService;
              _activeUser = activeUser;
              _dbContext = dbContext;
              _actionLogService = actionLogService;
          }

        [HttpGet("getmd5")]
        public async Task<ActionResult> GetMD5([FromQuery] string tt)
        {
            var response = await _loginService.Md5Async(tt);
            return Ok(response);
        }

        [HttpPost]
        [EnableRateLimiting("login")]
        public async Task<ActionResult> Get([FromBody] LoginParams model)
        {
            var ipAddress = HttpContext.Connection.RemoteIpAddress?.ToString() ?? "unknown";
            var response = await _loginService.LoginAsync(model, ipAddress);
            return Ok(response);
        }

        [HttpPost("login")]
        [EnableRateLimiting("login")]
        public async Task<ActionResult> Login([FromBody] LoginParams model)
        {
            var ipAddress = HttpContext.Connection.RemoteIpAddress?.ToString() ?? "unknown";
            var response = await _loginService.LoginAsync(model, ipAddress);
            return Ok(response);
        }

        [HttpPost("logout")]
        [Authorize]
        public async Task<ActionResult> Logout()
        {
            var jti = User.FindFirstValue("jti");
            if (!string.IsNullOrEmpty(jti))
            {
                await _tokenBlacklistService.BlacklistTokenAsync(jti);
            }

            await _tokenBlacklistService.RemoveActiveSessionAsync(_activeUser.UserId);

            // Increment TokenVersion in DB to invalidate token after restart
            var user = await _dbContext.User.FirstOrDefaultAsync(u => u.Id == _activeUser.UserId);
            if (user != null)
            {
                user.TokenVersion++;
                user.LastIp = HttpContext.Connection.RemoteIpAddress?.ToString() ?? "unknown";
                user.LastLogoutDate = DateTime.Now;
                  await _dbContext.SaveChangesAsync();

                  await _actionLogService.LogAsync(
                      _activeUser.UserId,
                      _activeUser.User.FindFirstValue(ClaimTypes.Name) ?? _activeUser.User.FindFirstValue(ClaimTypes.GivenName),
                      HttpContext.Connection.RemoteIpAddress?.ToString(), "Logout", "User", _activeUser.UserId.ToString(), null, null, null);

              }

              return Ok(await Result.SuccessAsync("Вы успешно вышли из системы."));
        }
    }
}


