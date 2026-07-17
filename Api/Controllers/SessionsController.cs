using Application.Authorization;
using Application.Sessions;
using Api.Auth;
using Application.Tenants;
using Contracts.Sessions;
using Domain.Sessions;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace Api.Controllers;

[ApiController]
[Route("api/v1/sessions")]
[Authorize]
public sealed class SessionsController : ControllerBase
{
    private readonly IHdSessionService _sessionService;
    private readonly ITenantContext _tenant;

    public SessionsController(IHdSessionService sessionService, ITenantContext tenant)
    {
        _sessionService = sessionService;
        _tenant = tenant;
    }

    [HttpGet]
    [Authorize(Policy = "Permission:" + Permissions.SessionRead)]
    public async Task<ActionResult<IReadOnlyList<SessionDto>>> Get(
        [FromQuery] List<string>? tenantIds,
        CancellationToken cancellationToken)
    {
        var userId = CurrentUserId();
        if (userId is null)
        {
            return Unauthorized();
        }

        var sessions = await _sessionService.GetAsync(userId.Value, tenantIds, cancellationToken);
        return sessions is null ? Forbid() : Ok(sessions);
    }

    [HttpGet("{id:long}")]
    [Authorize(Policy = "Permission:" + Permissions.SessionRead)]
    [Authorize(Policy = ActiveTenantPolicy.Name)]
    public async Task<ActionResult<SessionDto>> GetById(long id, CancellationToken cancellationToken)
    {
        var userId = CurrentUserId();
        if (userId is null)
        {
            return Unauthorized();
        }

        var session = await _sessionService.GetByIdAsync(
            userId.Value,
            _tenant.RequiredTenantId,
            id,
            cancellationToken);

        return session is null ? NotFound() : Ok(session);
    }

    [HttpPost]
    [Authorize(Policy = "Permission:" + Permissions.SessionCreate)]
    [Authorize(Policy = ActiveTenantPolicy.Name)]
    public async Task<ActionResult<SessionDto>> Create(
        CreateSessionRequest request,
        CancellationToken cancellationToken)
    {
        var userId = CurrentUserId();
        if (userId is null)
        {
            return Unauthorized();
        }

        var result = await _sessionService.CreateIdentifiedAsync(
            _tenant.RequiredTenantId,
            userId.Value,
            request,
            cancellationToken);

        return ToActionResult(result, created: true);
    }

    [HttpPut("{id:long}/start")]
    [Authorize(Policy = "Permission:" + Permissions.SessionStart)]
    [Authorize(Policy = ActiveTenantPolicy.Name)]
    public async Task<ActionResult<SessionDto>> Start(
        long id,
        StartSessionRequest request,
        CancellationToken cancellationToken)
    {
        var userId = CurrentUserId();
        if (userId is null)
        {
            return Unauthorized();
        }

        var result = await _sessionService.StartAsync(
            _tenant.RequiredTenantId,
            userId.Value,
            id,
            request,
            HasOverride(),
            cancellationToken);

        return ToActionResult(result);
    }

    [HttpPut("{id:long}/pause")]
    [Authorize(Policy = "Permission:" + Permissions.SessionPause)]
    [Authorize(Policy = ActiveTenantPolicy.Name)]
    public async Task<ActionResult<SessionDto>> Pause(
        long id,
        PauseSessionRequest request,
        CancellationToken cancellationToken)
    {
        var userId = CurrentUserId();
        if (userId is null)
        {
            return Unauthorized();
        }

        var result = await _sessionService.PauseAsync(
            _tenant.RequiredTenantId,
            userId.Value,
            id,
            request,
            cancellationToken);

        return ToActionResult(result);
    }

    [HttpPut("{id:long}/resume")]
    [Authorize(Policy = "Permission:" + Permissions.SessionResume)]
    [Authorize(Policy = ActiveTenantPolicy.Name)]
    public async Task<ActionResult<SessionDto>> Resume(long id, CancellationToken cancellationToken)
    {
        var userId = CurrentUserId();
        if (userId is null)
        {
            return Unauthorized();
        }

        var result = await _sessionService.ResumeAsync(
            _tenant.RequiredTenantId,
            userId.Value,
            id,
            cancellationToken);

        return ToActionResult(result);
    }

    [HttpPut("{id:long}/finish")]
    [Authorize(Policy = "Permission:" + Permissions.SessionFinish)]
    [Authorize(Policy = ActiveTenantPolicy.Name)]
    public async Task<ActionResult<SessionDto>> Finish(
        long id,
        FinishSessionRequest request,
        CancellationToken cancellationToken)
    {
        var userId = CurrentUserId();
        if (userId is null)
        {
            return Unauthorized();
        }

        var result = await _sessionService.FinishAsync(
            _tenant.RequiredTenantId,
            userId.Value,
            id,
            request,
            cancellationToken);

        return ToActionResult(result);
    }

    [HttpPut("{id:long}/end-identify")]
    [Authorize(Policy = "Permission:" + Permissions.SessionEndIdentify)]
    [Authorize(Policy = ActiveTenantPolicy.Name)]
    public async Task<ActionResult<SessionDto>> EndIdentify(long id, CancellationToken cancellationToken)
    {
        var userId = CurrentUserId();
        if (userId is null)
        {
            return Unauthorized();
        }

        var result = await _sessionService.EndIdentifyAsync(
            _tenant.RequiredTenantId,
            userId.Value,
            id,
            HasOverride(),
            cancellationToken);

        return ToActionResult(result);
    }

    [HttpPut("{id:long}/send-to-pay")]
    [Authorize(Policy = "Permission:" + Permissions.SessionSendToPay)]
    [Authorize(Policy = ActiveTenantPolicy.Name)]
    public async Task<ActionResult<SessionDto>> SendToPay(long id, CancellationToken cancellationToken)
    {
        var userId = CurrentUserId();
        if (userId is null)
        {
            return Unauthorized();
        }

        var result = await _sessionService.SendToPayAsync(
            _tenant.RequiredTenantId,
            userId.Value,
            id,
            HasOverride(),
            cancellationToken);

        return ToActionResult(result);
    }

    [HttpPut("{id:long}/mark-paid")]
    [Authorize(Policy = "Permission:" + Permissions.SessionMarkPaid)]
    [Authorize(Policy = ActiveTenantPolicy.Name)]
    public async Task<ActionResult<SessionDto>> MarkPaid(long id, CancellationToken cancellationToken)
    {
        var userId = CurrentUserId();
        if (userId is null)
        {
            return Unauthorized();
        }

        var result = await _sessionService.MarkPaidAsync(
            _tenant.RequiredTenantId,
            userId.Value,
            id,
            cancellationToken);

        return ToActionResult(result);
    }

    [HttpPut("{id:long}/archive")]
    [Authorize(Policy = "Permission:" + Permissions.SessionArchive)]
    [Authorize(Policy = ActiveTenantPolicy.Name)]
    public async Task<ActionResult<SessionDto>> Archive(long id, CancellationToken cancellationToken)
    {
        var userId = CurrentUserId();
        if (userId is null)
        {
            return Unauthorized();
        }

        var result = await _sessionService.ArchiveAsync(
            _tenant.RequiredTenantId,
            userId.Value,
            id,
            HasOverride(),
            cancellationToken);

        return ToActionResult(result);
    }

    [HttpPut("{id:long}/measurements/{point}")]
    [Authorize(Policy = "Permission:" + Permissions.SessionMeasurementUpdate)]
    [Authorize(Policy = ActiveTenantPolicy.Name)]
    public async Task<ActionResult<SessionMeasurementDto>> AddOrUpdateMeasurement(
        long id,
        SessionMeasurementPoint point,
        SessionMeasurementRequest request,
        CancellationToken cancellationToken)
    {
        var userId = CurrentUserId();
        if (userId is null)
        {
            return Unauthorized();
        }

        var result = await _sessionService.AddOrUpdateMeasurementAsync(
            _tenant.RequiredTenantId,
            userId.Value,
            id,
            point,
            request,
            cancellationToken);

        return ToActionResult(result);
    }

    private ActionResult<T> ToActionResult<T>(SessionCommandResult<T> result, bool created = false)
    {
        return result.Status switch
        {
            SessionCommandStatus.Succeeded when created => Created(string.Empty, result.Value),
            SessionCommandStatus.Succeeded => Ok(result.Value),
            SessionCommandStatus.NotFound => NotFound(),
            SessionCommandStatus.Forbidden => Forbid(),
            SessionCommandStatus.Conflict => Conflict(),
            _ => Conflict()
        };
    }

    private bool HasOverride()
    {
        return User.HasClaim("permission", Permissions.SessionOverrideTimeLimits);
    }

    private long? CurrentUserId()
    {
        var sub = User.FindFirst("sub")?.Value;
        return long.TryParse(sub, out var userId) ? userId : null;
    }
}
