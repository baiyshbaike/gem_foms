using Dialysis.Server.Domain;
using Dialysis.Server.Domain.Services;
using Dialysis.Shared.Dto;
using Dialysis.Shared.Models;
using Dialysis.Shared.Params;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;

namespace Dialysis.Server.Controllers
{
    [Route("api/audit/httplog")]
    [ApiController]
    [Authorize(Roles = "admin")]
    public class HttpLogController : Controller
    {
        private readonly IHttpLogService _httpLogService;
        private readonly AppDbContext _db;
        private readonly ILogSettingsProvider _settingsProvider;

        public HttpLogController(IHttpLogService httpLogService, AppDbContext db, ILogSettingsProvider settingsProvider)
        {
            _httpLogService = httpLogService;
            _db = db;
            _settingsProvider = settingsProvider;
        }

        [HttpGet("paged")]
        public async Task<ActionResult<PagedHttpLogResponse>> GetPaged([FromQuery] HttpLogParams parameters)
        {
            var response = await _httpLogService.GetPagedAsync(parameters);
            return Ok(response);
        }

        [HttpGet("{id}")]
        public async Task<ActionResult<HttpLogDto>> GetById(long id)
        {
            var log = await _httpLogService.GetByIdAsync(id);
            if (log == null)
                return NotFound();
            return Ok(log);
        }

        [HttpGet("methods")]
        public ActionResult<List<string>> GetMethods()
        {
            return Ok(new[] { "GET", "POST", "PUT", "PATCH", "DELETE" });
        }

        [HttpGet("settings")]
        public async Task<ActionResult<LogSettings>> GetSettings()
        {
            var settings = await _db.Set<LogSettings>().FirstOrDefaultAsync();
            if (settings == null)
            {
                settings = new LogSettings();
                _db.Set<LogSettings>().Add(settings);
                await _db.SaveChangesAsync();
            }
            return Ok(settings);
        }

        [HttpPost("settings")]
        public async Task<ActionResult> SaveSettings([FromBody] LogSettings settings)
        {
            var existing = await _db.Set<LogSettings>().FirstOrDefaultAsync();
            if (existing == null)
            {
                settings.Id = 0;
                _db.Set<LogSettings>().Add(settings);
            }
            else
            {
                existing.LogHttpEnabled = settings.LogHttpEnabled;
                existing.LoggedMethods = settings.LoggedMethods;
                existing.LogActionEnabled = settings.LogActionEnabled;
                existing.MaxBodyLength = settings.MaxBodyLength;
                existing.RetentionDays = settings.RetentionDays;
            }
            await _db.SaveChangesAsync();
            _settingsProvider.Refresh();
            return Ok();
        }
    }
}
