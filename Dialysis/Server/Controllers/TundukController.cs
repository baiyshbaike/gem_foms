namespace Dialysis.Server.Controllers;
using Domain.Services;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

[ApiController]
[Route("api/tunduk")]
[Authorize]
public class TundukController : Controller
{
    private readonly IVerifyService _verifyService;
    private readonly string _hashSecretKey;
    
    public TundukController(IVerifyService verifyService, IConfiguration configuration)
    {
        _verifyService = verifyService;
        _hashSecretKey = configuration.GetValue<string>("HashSecretKey") ?? "6+FmZx4Gwxata3fV";
    }
    
    [HttpGet("get_patient_sessions")]
    public async Task<ActionResult> GetPatientSessions()
    {
        var result = await _verifyService.GetPatientSessions(_hashSecretKey);
        if (!result.Succeeded)
        {
            return BadRequest(result.Messages);
        }
        return Ok(result.Data);
    }
}