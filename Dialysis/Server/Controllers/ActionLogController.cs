using Dialysis.Server.Domain.Services;
using Dialysis.Shared.Dto;
using Dialysis.Shared.Params;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace Dialysis.Server.Controllers
{
    [Route("api/audit/actionlog")]
    [ApiController]
    [Authorize(Roles = "admin")]
    public class ActionLogController : Controller
    {
        private readonly IActionLogService _actionLogService;

        public ActionLogController(IActionLogService actionLogService)
        {
            _actionLogService = actionLogService;
        }

        [HttpGet("paged")]
        public async Task<ActionResult<PagedActionLogResponse>> GetPaged([FromQuery] ActionLogParams parameters)
        {
            var response = await _actionLogService.GetPagedAsync(parameters);
            return Ok(response);
        }

        [HttpGet("{id}")]
        public async Task<ActionResult<ActionLogDto>> GetById(long id)
        {
            var log = await _actionLogService.GetByIdAsync(id);
            if (log == null)
                return NotFound();
            return Ok(log);
        }

        [HttpGet("action-types")]
        public ActionResult<List<string>> GetActionTypes()
        {
            return Ok(new[] { "Create", "Update", "Delete", "Login", "Logout", "Export", "View" });
        }
    }
}
