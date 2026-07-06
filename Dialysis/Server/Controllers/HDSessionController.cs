using Dialysis.Shared.Params;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using System.Globalization;
using Dialysis.Server.Domain.Services;
using Dialysis.Shared.Dto;
using Dialysis.Shared.Models;

namespace Dialysis.Server.Controllers;
[Route("api/hdsession")]
[ApiController]
[Authorize]
public class HDSessionController : Controller
{
    private readonly IHDSessionService _hdSessionService;
    public HDSessionController(IHDSessionService hdSessionService)
    {
        _hdSessionService = hdSessionService;
    }
    [HttpGet("hdsessions-paged")]
    public async Task<ActionResult<SessionResponseDto<HDSessionDto>>> GetPagedPatients(
        [FromQuery] int pageNumber = 1,
        [FromQuery] int pageSize = 10,
        [FromQuery] int statusId = 0,
        [FromQuery] long medCenterId = 0,
        [FromQuery] string sortBy = "",
        [FromQuery] bool sortAsc = false,
        [FromQuery] string searchString = "",
        [FromQuery] DateTime? sessionStartDateFrom = null,
        [FromQuery] DateTime? sessionStartDateTo = null
       )
    {
        var response = await _hdSessionService.GetSessionsPagedAsync(
            sessionStartDateFrom,
            sessionStartDateTo,
            pageNumber,
            pageSize,
            statusId,
            medCenterId,
            sortBy,
            sortAsc,
            searchString);
    
        return Ok(response);
    }
    [HttpGet("searchpatient")]
    public async Task<ActionResult> SearchPatient([FromQuery] string inn)
    {
        var response = await _hdSessionService.SearchByInn(inn);
        return Ok(response);
    }

    [HttpPost("delete-hdsession")]
    public async Task<ActionResult> DeleteModel([FromBody] HDSession hdSession)
    {
        return Ok(await _hdSessionService.DeleteModel(hdSession));
    }
    [HttpPost("addidentify")]
    public async Task<ActionResult> AddIdentify([FromBody] HDSession hdSession)
    {
        return Ok(await _hdSessionService.AddIdentify(hdSession));
    }

    [HttpPost("update-hdsession")]
    public async Task<ActionResult> StartHdSession([FromBody] HDSession hdSession)
    {
        return Ok(await _hdSessionService.StartHdSession(hdSession));
    }
    [HttpPost("add-edit-hour-hdsession")]
    public async Task<ActionResult> HourHdSession([FromBody] HDSessionHour hdSessionHour)
    {
        return Ok(await _hdSessionService.AddEditHourHdSession(hdSessionHour));
    }
}